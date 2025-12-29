
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using System.Linq;
using System.Collections;
using System.IO;
using System.Data;

using System.Globalization;


/// <summary>
/// Class that manages all of the recordingsand avatar preview generation 
/// </summary>
public class FBTRecorder : MonoBehaviour
{
    private const float DEFAULT_RECORDING_SPEED = 0.8f; //to match 24 fps when recording with 30 

    public int rootIndex = 0;
    public Transform previewAvatar;
    public Transform floorRefPoint;

    public class Keypoint
    {
        public Vector3 position;
        public Vector3 localPosition;
        public Quaternion rotation;
        public Quaternion localRotation;
        public bool empty;
        public bool ignore;
    };

    public class Frame
    {
        public Vector3 relPosition;
        public float fps;

        public List<Keypoint> keypoints;
    }

    public class Recording
    {
        public float avgFPS = 0.0f;
        public string name = "";
        public List<int> recordedBones = new List<int>();
        public List<int> instructionPointsOfInterest = new List<int>();

        public List<Frame> frames = new List<Frame>();
        public int involvedBoneCount = 0;
    }

    enum State : ushort
    {
        None = 0,
        Recording = 1,
        Playing = 2
    }

    [Header("Avatar Bone-Mapping (check order in code to match them) ")]
    public List<Transform> avatarMapping;

    [Header("Other Settings")]
    public bool enableInterpolation = true;


    private FullBodyTrackingManager fbtManager;
    private BoneInterpolationManager interpolationManager;

    private Transform[] recordingTransforms;
    private Recording recordedAnimation = new Recording();

    private float cFrame = 0.0f;

    private Vector3 basePosition;
    private Quaternion baseRotation;
    private Vector3 playerStartPosition;
    private GameObject localPlayer;


    /// <summary>
    /// Checks if the WorkoutTable in the SQLite database exist or not (creates them if this isnt the case)
    /// </summary>
    void Start()
    {
        MotionMatchingDBManager.CreateWorkoutTablesIfNotExisiting();
    }

    /// <summary>
    /// Adjust the current position of the avatar to match with the floor through this update call 
    /// </summary>
    void Update()
    {
        this.AdjustAvatarPosition();
    }

    /// <summary>
    /// Called by the FBTRecorderManager and initalize the recorder
    /// </summary>
    /// <param name="localPlayer">Reference to the local player </param>
    public void Initialize(GameObject localPlayer)
    {
        this.localPlayer = localPlayer;
        this.fbtManager = localPlayer.GetComponentInChildren<FullBodyTrackingManager>();
        this.interpolationManager = this.GetComponentInChildren<BoneInterpolationManager>();
        
        this.basePosition = this.previewAvatar.position;
        this.baseRotation = this.previewAvatar.rotation;

        if (fbtManager == null)
        {
            Object.Destroy(this.gameObject);
        }
        else
        {
            this.recordingTransforms = this.fbtManager.GetUsedAvatarTransforms();
        }

    }

    /// <summary>
    /// Returns the FBTManager class 
    /// </summary>
    /// <returns>FBTManager class </returns>
    public FullBodyTrackingManager GetFBTManager()
    {
        return this.fbtManager;
    }

    /// <summary>
    /// Returns which index is the root index in our AvatarMapping list
    /// </summary>
    /// <returns>Integer of the root index</returns>
    public int GetRootIndex()
    {
        return this.rootIndex;
    }

    /// <summary>
    /// Returns if a recording is currently loaded or not
    /// </summary>
    /// <returns>bool if a recording is loaded</returns>
    public bool HasRecordingLoaded()
    {
        return this.recordedAnimation != null && this.recordedAnimation.frames.Count > 0;
    }


    /// <summary>
    /// Returns all of the watched transform objects of the local player avatar 
    /// </summary>
    /// <returns>Local player transforms as an array</returns>
    public Transform[] GetPlayerRotation()
    {
        return this.fbtManager.GetUsedAvatarTransforms();
    }

    /// <summary>
    /// Returns the list of the current transform of the preview avatar 
    /// </summary>
    /// <returns>List of transfrom from the preview avatar</returns>
    public List<Transform> GetCurrentFrameTransform()
    {
        return this.avatarMapping;
    }

    /// <summary>
    /// Returns the amount of captured keyframe from the avatar mapping
    /// </summary>
    /// <returns>int of the amount</returns>
    public int GetCapturedKeyPointCount()
    {
        return this.avatarMapping.Count;
    }

    /// <summary>
    /// Returns the current played frame of the recorder
    /// </summary>
    /// <returns>Frame class of the current frame</returns>
    public Frame GetCurrentFrame()
    {
        int cFrame = (int)this.cFrame;

        if (cFrame < 0 || cFrame >= this.recordedAnimation.frames.Count)
        {
            return null;
        }


        return this.recordedAnimation.frames[cFrame];
    }

    /// <summary>
    /// Resets the current recording to zero
    /// </summary>
    public void ResetFrame()
    {
        this.cFrame = 0.0f;
        this.PlayRecordingFrame(0.0f, true);
    }

    /// <summary>
    /// Returns the currently loaded recording
    /// </summary>
    /// <returns>Recording of the currently loaded one</returns>
    public Recording GetLoadedRecording()
    {
        return this.recordedAnimation;
    }

    /// <summary>
    /// Returns the last frames per second of the current recording
    /// </summary>
    /// <returns>float of the fps</returns>
    public float GetLastFrameFps()
    {
        return this.fbtManager.GetLastFrameFps();
    }

    /// <summary>
    /// Get all of the current local player transform data but as a keypoint data class 
    /// </summary>
    /// <param name="shouldIgnore">bool if the keypoints should be ignored (for the matching percentage calculation)</param>
    /// <returns></returns>
    public List<Keypoint> GetPlayerCurrentKeyPoints(bool shouldIgnore = true)
    {
        if (this.fbtManager.GotNewFrame() == false)
        {
            return null;
        }

        List<Keypoint> keypoints = new List<Keypoint>();
        FullBodyTrackingManager.CachedRotation[] prevRotation = this.fbtManager.GetPreviousAvatarRotation();


        for (int i = 0; i < prevRotation.Length; i++)
        {
            Keypoint cKeypoint = new Keypoint();
            cKeypoint.ignore = shouldIgnore;
            cKeypoint.empty = true;

            cKeypoint.position = this.recordingTransforms[i].position;
            cKeypoint.localPosition = this.recordingTransforms[i].localPosition;

            cKeypoint.rotation = this.recordingTransforms[i].rotation;
            cKeypoint.localRotation = this.recordingTransforms[i].localRotation;

            if (prevRotation[i].WasUpdated() == true)
            {
                cKeypoint.empty = false;
            }

            keypoints.Add(cKeypoint);
        }

        return keypoints;
    }

    /// <summary>
    /// Starts a new recording 
    /// </summary>
    /// <param name="bonesToRecord">List of ints where all of the ids should be stored of the bones that are intereseting for us in this workout </param>
    public void StartNewRecording(List<int> bonesToRecord)
    {
        this.recordedAnimation = new Recording();
        this.recordedAnimation.frames = new List<Frame>();
        this.recordedAnimation.recordedBones = bonesToRecord;
        this.recordedAnimation.involvedBoneCount = MotionMatchingDBManager.GetConnectedBones(bonesToRecord).Count;

        this.recordingTransforms = this.fbtManager.GetUsedAvatarTransforms();
        this.playerStartPosition = this.recordingTransforms[this.rootIndex].position;
        this.cFrame = 0.0f;
    }

    /// <summary>
    /// Stops the recording
    /// </summary>
    public void StopRecording()
    {
        this.recordedAnimation.avgFPS /= this.recordedAnimation.frames.Count;
    }

    /// <summary>
    /// Records the current frame and store it in an internal list (to continue the recording)
    /// </summary>
    public void RecordFrame()
    {

        if(this.fbtManager.GotNewFrame() == false)
        {
            return;
        }


        Frame frame = new Frame();
        frame.keypoints = new List<Keypoint>();
        frame.fps = this.fbtManager.GetLastFrameFps();
        
        this.recordedAnimation.avgFPS += frame.fps;


        frame.keypoints = this.GetPlayerCurrentKeyPoints();

        if(frame.keypoints.Count > 0)
        {
            frame.relPosition = this.playerStartPosition - frame.keypoints[this.rootIndex].position;
        }

        for(int i = 0; i< this.recordedAnimation.recordedBones.Count; i++)
        {
            int idx = this.recordedAnimation.recordedBones[i];
            frame.keypoints[idx].ignore = false;
        }

        this.recordedAnimation.frames.Add(frame);
    }


    /// <summary>
    /// Plays just a specific frame and also allows interpolation to thatz frame
    /// </summary>
    /// <param name="targetFrame">int of the frame that should be played</param>
    /// <param name="interpolationSpeed">float of the interpolation speed (0 = no interpolation)</param>
    public void PlaySpecificFrame(int targetFrame, float interpolationSpeed = 0.0f)
    {

        if(this.HasRecordingLoaded() == false)
        {
            return;
        }
        else if (targetFrame < 0 || targetFrame >= this.recordedAnimation.frames.Count)
        {
            return;
        }

        this.cFrame = targetFrame;

        this.interpolationManager.FinishFrame();

        Frame cFrame = this.recordedAnimation.frames[targetFrame];
 

        for (int j = 0; j < cFrame.keypoints.Count; j++)
        {
            this.InterpolateBone(this.avatarMapping[j], cFrame.keypoints[j].localRotation, interpolationSpeed);
        }
    }

    /// <summary>
    /// Adjust the preview avatar position so that it doesnt float over the floor)
    /// </summary>
    private void AdjustAvatarPosition()
    {
        float lowestYBonePosition = 10000.0f;

        for (int j = 0; j < this.avatarMapping.Count; j++)
        {
            lowestYBonePosition = System.Math.Min(lowestYBonePosition, this.avatarMapping[j].position.y);
        }

        float floorY = this.floorRefPoint.position.y - (lowestYBonePosition - this.previewAvatar.position.y);
        this.previewAvatar.position = new Vector3(this.previewAvatar.position.x, floorY, this.previewAvatar.position.z);
    }

    /// <summary>
    /// Plays the current loaded recording 
    /// </summary>
    /// <param name="speedAmplifier">float of the speed of which the recording should be played</param>
    /// <param name="forceUpdate">bool if an update should be forced in this frame otherwise we wait until we reached the next keyframe from our list</param>
    /// <returns>bool returning if the animation reached a whole loop or not</returns>
    public bool PlayRecordingFrame(float speedAmplifier = 1.0f, bool forceUpdate = false)
    {
        bool isRepeating = false;
        int oldFrame = (int)this.cFrame;

        float fps = this.recordedAnimation.frames[oldFrame].fps;
        float timeTick = fps * DEFAULT_RECORDING_SPEED * speedAmplifier * Time.deltaTime;

        this.cFrame += timeTick;//in general we want to play everything in 24 fps
        int frame = (int)cFrame;

        if (frame >= this.recordedAnimation.frames.Count - 1)
        {
            this.cFrame = 0.0f;
            oldFrame = this.recordedAnimation.frames.Count - 1;
            frame = 0;

            isRepeating = true;
        }
        else if (frame < 0)
        {
            frame = this.recordedAnimation.frames.Count - 1;
            oldFrame = 0;

            isRepeating = true;
        }

        if (oldFrame != frame || forceUpdate == true)
        {
            this.PlaySpecificFrame(frame, timeTick);
        }

        return isRepeating;
    }
    
    /// <summary>
    /// Interpolates the bones to the given target position and rotation 
    /// </summary>
    /// <param name="ref_">Transform reference of the transform object that should be interpolated</param>
    /// <param name="targetRot">Quaternion target rotation</param>
    /// <param name="interpolationTime">float of the interpolation time (how long it should take to interpolate to this)</param>
    void InterpolateBone(Transform ref_, Quaternion targetRot, float interpolationTime = 0.0f)
    {
        float tTime = interpolationTime;

        if (this.enableInterpolation == false)
        {
            tTime = 0.0f;
        }

        this.interpolationManager.AddRotationInterpolation(ref_, ref_.rotation, targetRot, tTime, true);
    }

    /// <summary>
    /// Loads the given recording by his name in the database
    /// </summary>
    /// <param name="name">string of the name</param>
    public void LoadRecording(string name)
    {
        this.recordedAnimation = MotionMatchingDBManager.LoadRecording(name);
        this.cFrame = 0.0f;
    }

    /// <summary>
    /// Saves the current recording 
    /// </summary>
    /// <param name="name">Name of the recording that it should have in the database</param>
    public void SaveCurrentRecording(string name)
    {
        if (this.HasRecordingLoaded() == false)
        {
            return;
        }

        MotionMatchingDBManager.SaveTrainerRecording(name, this.recordedAnimation);
    }
}

