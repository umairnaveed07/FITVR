using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


/// <summary>
/// Class that manages the fullbody tracking for the first person avatar
/// </summary>
public class FullBodyTrackingManager : MonoBehaviour
{

    public class CachedRotation
    {
        public Quaternion rotation;
        public bool updatedThisFrame = false;

        public void UpdateRotation(Quaternion r)
        {
            this.rotation = r;
            this.updatedThisFrame = true;
        }

        public void ForceUpdate()
        {
            this.updatedThisFrame = true;
        }

        public bool WasUpdated()
        {
            return this.updatedThisFrame;
        }
    }

    /// <summary>
    /// Helper class for the posedata
    /// </summary>
    public class PoseData
    {
        public int idx;
        public Vector3 pos;

        public PoseData(int _idx, Vector3 _pos)
        {
            this.idx = _idx;
            this.pos = _pos;
        }
    };

    /// <summary>
    /// Helper class for the networkposebuffer
    /// </summary>
    public class NetworkPoseBuffer
    {
        public int camId;

        public double fps;
        public double timestamp;

        public PoseData[] poses;


        public NetworkPoseBuffer(float camId, double fps, double timestamp, PoseData[] poses)
        {
            this.camId = (int)camId;
            this.fps = fps;
            this.timestamp = timestamp;
            this.poses = poses;
        }
    }


    public const int MAX_KEYPOINTS = 33;
    public const int USED_AVATAR_TRANSFORM_COUNT = 15;

    private const float MAX_WAIT_TIME = 5.0f;
    private const int ENTRY_SIZE = 25;
    private const int END_OF_GROUP = 50;
    private const float FABRIK_ACCURACY = 0.01f;

    //Given by the library constants of MediaPipe
    private static readonly int[] CONNECTIONS =
    {
        15, 21,16, 20,18, 20,3, 7,14, 16,23, 25,28, 30,11, 23,27, 31,6, 8,15, 17,24, 26,16, 22,4, 5,5,
        6,29, 31,12, 24,23, 24,0, 1,9, 10,1, 2,0, 4,11, 13,30, 32,28, 32,15, 19,16, 18,25, 27,26, 28,12,
        14,17, 19,2, 3,11, 12,27, 29,13, 15
    };

    //connected body groups. Minus means that it shouldnt be remove from the group
    private static readonly int[] BODY_GROUPS =
    {
        -12,14,16,          END_OF_GROUP,//left arm
        -11,13,15,          END_OF_GROUP,//right arm
        -11,-12,-23,-24,    END_OF_GROUP,//hip
        -24,26,28,          END_OF_GROUP,//left leg
        -23,25,27,          END_OF_GROUP,//right leg
        5,2,8,7,            END_OF_GROUP //head
    };


    [Header("Avatar Bone-Mapping")]
    public Transform avatarLeftShoulder;
    public Transform avatarLeftElbow;
    public Transform avatarLeftHand;

    public Transform avatarRightShoulder;
    public Transform avatarRightElbow;
    public Transform avatarRightHand;

    public Transform avatarLeftUpLeg;
    public Transform avatarLeftLeg;
    public Transform avatarLeftFoot;

    public Transform avatarRightUpLeg;
    public Transform avatarRightLeg;
    public Transform avatarRightFoot;

    public Transform avatarHip;
    public Transform avatarShoulders;
    public Transform avatarHead;

    [Header("Other Settings")]

    public Transform avatar;
    public bool showDebugCharacter = false;

    public GameObject debugDrawObject;
    public Transform cameraRefPoint;

    //Not exposed variables
    private Animator animationController;
    private NetworkAvatarManager mainAvatar;
    private int maxBuffer = 0;
    private float packageTime = MAX_WAIT_TIME;
    private bool fbtIsActive = false;

    private Transform xrDevice;

    private Transform leftVrController;
    private Transform rightVrController;

    private List<GameObject> lineRenderers;


    private Transform[] usedAvatarTransforms = new Transform[USED_AVATAR_TRANSFORM_COUNT];
    private UdpClient receivingUdp;
    private List<NetworkPoseBuffer> networkBuffer;
    private PoseData[] lastPose = null;

    private float interpolationTime = 1.0f / 60.0f;//note that this is just a starting value and will be calculated during runtime 

    private Quaternion initalFootRot;
    private CachedRotation[] prevRotation = new CachedRotation[USED_AVATAR_TRANSFORM_COUNT];
    
    private bool isNewFrame = false;
    private bool isWholeBodyVisible = true;

    /// <summary>
    /// Initialize the usedAvatarTransform array (so that it can be used by other classes also)
    /// </summary>
    void Awake()
    {
        //to make all the bones iteratable
        this.usedAvatarTransforms[0] = this.avatarHip;
        this.usedAvatarTransforms[1] = this.avatarShoulders;
        this.usedAvatarTransforms[2] = this.avatarHead;

        this.usedAvatarTransforms[3] = this.avatarLeftShoulder;
        this.usedAvatarTransforms[4] = this.avatarLeftElbow;
        this.usedAvatarTransforms[5] = this.avatarLeftHand;

        this.usedAvatarTransforms[6] = this.avatarRightShoulder;
        this.usedAvatarTransforms[7] = this.avatarRightElbow;
        this.usedAvatarTransforms[8] = this.avatarRightHand;

        this.usedAvatarTransforms[9] = this.avatarLeftUpLeg;
        this.usedAvatarTransforms[10] = this.avatarLeftLeg;
        this.usedAvatarTransforms[11] = this.avatarLeftFoot;

        this.usedAvatarTransforms[12] = this.avatarRightUpLeg;
        this.usedAvatarTransforms[13] = this.avatarRightLeg;
        this.usedAvatarTransforms[14] = this.avatarRightFoot;
    }

    /// <summary>
    /// Starts FBT by initializing everything
    /// </summary>
    void Start()
    {
        this.mainAvatar = this.GetComponentInParent(typeof(NetworkAvatarManager)) as NetworkAvatarManager;

        if (this.showDebugCharacter == true)
        {
            this.lineRenderers = new List<GameObject>();
            int loops = CONNECTIONS.Length / 2;

            for (int i = 0; i < loops; i++)
            {
                GameObject linerObj = new GameObject("LineRenderer");
                linerObj.AddComponent<LineRenderer>();

                LineRenderer l = linerObj.GetComponent<LineRenderer>();
                l.widthMultiplier = 0.015f;
                l.positionCount = 2;
                l.material = new Material(Shader.Find("Sprites/Default"));

                linerObj.transform.parent = debugDrawObject.transform;
                this.lineRenderers.Add(linerObj);
            }
        }

        for (int i = 0; i < USED_AVATAR_TRANSFORM_COUNT; i++)
        {
            this.prevRotation[i] = new CachedRotation();
        }

        this.initalFootRot = this.avatarLeftFoot.localRotation;
        OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();

        this.xrDevice = rig.transform.Find("TrackingSpace/CenterEyeAnchor");
        this.leftVrController= rig.transform.Find("TrackingSpace/LeftHandAnchor/LeftControllerAnchor");
        this.rightVrController = rig.transform.Find("TrackingSpace/RightHandAnchor/RightControllerAnchor");

        this.animationController = this.avatar.gameObject.GetComponent<Animator>();

        this.receivingUdp = new UdpClient(8081);
        this.networkBuffer = new List<NetworkPoseBuffer>();

        if(this.showDebugCharacter == false)
        {
            this.debugDrawObject.SetActive(false);
        }
    }

    /// <summary>
    /// Performs the update based on the last data we received. Note that this is an late update since we want to apply this after all of the animations
    /// </summary>
    void LateUpdate()
    {
        if (this.receivingUdp!= null)
        {
            this.UpdateUdpConnection();
        }
    }

    /// <summary>
    /// Makes sure, that the udp connection gets deleted
    /// </summary>
    public void OnDestroy()
    {
        if (this.receivingUdp != null)
        {
            this.receivingUdp.Close();
        }
    }

    /// <summary>
    /// Returns if we got a new frame 
    /// </summary>
    /// <returns>bool if we got a new frame</returns>
    public bool GotNewFrame()
    {
        return this.isNewFrame;
    }

    /// <summary>
    /// Returns if all of the body parts where visible by the capturing device or not
    /// </summary>
    /// <returns>bool if the body is visible or not</returns>
    public bool IsWholeBodyVisible()
    {
        if(this.IsFullbodyTrackingActive() == false)
        {
            return false;
        }

        return this.isWholeBodyVisible;
    }

    /// <summary>
    /// Returns if FBT is enabled or not
    /// </summary>
    /// <returns>booll is FBT is active</returns>
    public bool IsFullbodyTrackingActive()
    {
        return this.fbtIsActive;
    }

    /// <summary>
    /// Returns the fps of the newest recroded frame
    /// </summary>
    /// <returns>float of the last fps</returns>
    public float GetLastFrameFps()
    {
        return 1.0f / this.interpolationTime;
    }

    /// <summary>
    /// Returns the last valid pose
    /// </summary>
    /// <returns>PoseData array of the last valid pose</returns>
    public PoseData[] GetLastPose()
    {
        return this.lastPose;
    }

    /// <summary>
    /// Returns the previous avatar rotation which also tells us, if the rotation was updated that frame or not
    /// </summary>
    /// <returns>CachedRotation array of the previous avatar rotation</returns>
    public CachedRotation[] GetPreviousAvatarRotation()
    {
        return this.prevRotation;
    }

    /// <summary>
    /// Return the bones as an iteratable array
    /// </summary>
    /// <returns>Transform array of the bones</returns>
    public Transform[] GetUsedAvatarTransforms()
    {
        return this.usedAvatarTransforms;
    }

    /// <summary>
    /// Pulls the newest data from the transmitted udp data points
    /// </summary>
    void UpdateUdpConnection()
    {
        if (this.receivingUdp != null && this.receivingUdp.Available > 0)
        {

            if (this.fbtIsActive == false)
            {
                this.CacheCurrentRotation(); //to avoid weird flipping at startup
                this.mainAvatar.StartFullBodyTracking();

                this.fbtIsActive = true;
            }

            this.packageTime = MAX_WAIT_TIME;

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            // Blocks until a message returns on this socket from a remote host.
            byte[] receiveBytes = this.receivingUdp.Receive(ref RemoteIpEndPoint);
            int packetCount = (receiveBytes.Length - 32) / ENTRY_SIZE;

            PoseData[] poses = new PoseData[MAX_KEYPOINTS];

            float camId = (float)System.BitConverter.ToDouble(receiveBytes, 0);
            float tmpMaxBuffer = (float)System.BitConverter.ToDouble(receiveBytes, 8);
            double timestamp = System.BitConverter.ToDouble(receiveBytes, 16);
            double fps = System.BitConverter.ToDouble(receiveBytes, 24);

            this.maxBuffer = (int)Math.Max((float)this.maxBuffer, tmpMaxBuffer);
            this.interpolationTime = 1.0f / (float)fps;

            for (int i = 0; i < packetCount; i++)
            {
                PoseData pose = this.ExtractVector(receiveBytes, 32 + i * ENTRY_SIZE);
                poses[pose.idx] = pose;
            }

            NetworkPoseBuffer currentBuffer = new NetworkPoseBuffer(camId, fps, timestamp, poses);
            this.networkBuffer.Add(currentBuffer);
        }


        //Logic to check if FBT should still be enabled or not
        this.packageTime -= Time.deltaTime;

        if (this.packageTime <= 0.0f)
        {
            if (this.fbtIsActive == true)
            {
                this.mainAvatar.StopFullBodyTracking();
            }

            this.fbtIsActive = false;
        }

        //Apply FBT if it is enabled
        if (this.fbtIsActive == true)
        {
            this.isNewFrame = false;

            if (this.networkBuffer.Count >= this.maxBuffer)
            {
                this.isNewFrame = true;

                this.ProcessNetworkBuffer();
                this.networkBuffer.Clear();
            }

            if (this.lastPose != null)
            {
                this.ApplyPose(this.lastPose);

                //now determine if the whole body was visible or not
                this.isWholeBodyVisible = true;

                for (int i = 0; i < USED_AVATAR_TRANSFORM_COUNT; i++)
                {
                    if (this.prevRotation[i].WasUpdated() == false)
                    {
                        this.isWholeBodyVisible = false;
                        break;
                    }
                }
            }

            this.AlignAvatarWithDevice();
        }
    }

    /// <summary>
    /// Aligns the body ref point to the match the camera position
    /// </summary>
    void AlignAvatarWithDevice()
    {
        this.avatar.position -= (this.cameraRefPoint.position - this.xrDevice.position);
        //now apply the inverse kinematics to it
        Quaternion shiftRotX = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
        var finalRotation = this.SolveArmIk(avatarLeftShoulder, avatarLeftElbow, avatarLeftHand, leftVrController.position);

        avatarLeftShoulder.rotation = finalRotation.Item1 * shiftRotX;
        avatarLeftElbow.rotation = finalRotation.Item2 * shiftRotX;

        finalRotation = this.SolveArmIk(avatarRightShoulder, avatarRightElbow, avatarRightHand, rightVrController.position);

        avatarRightShoulder.rotation = finalRotation.Item1 * shiftRotX;
        avatarRightElbow.rotation = finalRotation.Item2 * shiftRotX;
    }


    /// <summary>
    /// Process our network buffer to merge all the data to one single array containing the final positions
    /// </summary>
    void ProcessNetworkBuffer()
    {
        if (this.networkBuffer.Count <= 0)
        {
            return;
        }

        this.RemoveUnnecessaryNetworkData();
        NetworkPoseBuffer focusCamBuffer = this.GetFocusCamFromNetworkBuffer();

        if (focusCamBuffer == null)
        {
            print("Something unexpected happened");
            return; //its also an error but for now leave it this way
        }

        this.lastPose = this.MergeNetworkPoses(focusCamBuffer);
    }

    /// <summary>
    /// Merges all network buffers to one array by adding missing ref points from the given focus cam to it. Also apply rotation correction to match the focus camera
    /// </summary>
    /// <param name="focusCam">NetworkPoseBuffer which is the main package we take as an reference</param>
    /// <returns>PoseData array with the merged pose</returns>
    PoseData[] MergeNetworkPoses(NetworkPoseBuffer focusCam)
    {
        PoseData[] baseData = focusCam.poses;
        Quaternion baseHeadRot = this.GetYAxis(this.ExtractHeadRotation(baseData));


        if (baseHeadRot == Quaternion.identity)
        {
            return baseData;
        }

        foreach (NetworkPoseBuffer buffer in this.networkBuffer)
        {
            if (buffer == focusCam || buffer.camId == focusCam.camId)
            {
                continue;
            }

            Quaternion headRot = this.GetYAxis(this.ExtractHeadRotation(buffer.poses));

            if (headRot == Quaternion.identity)
            {
                continue;
            }

            Quaternion difference = Quaternion.Inverse(headRot) * baseHeadRot;

            foreach (PoseData cPose in buffer.poses)
            {
                if (cPose == null)
                {
                    continue;
                }

                if (this.GetPoseIndex(baseData, cPose.idx) == null)
                {
                    Vector3 finalPos = difference * cPose.pos;

                    baseData[cPose.idx] = cPose;
                    baseData[cPose.idx].pos = finalPos;
                }
            }
        }

        return baseData;
    }

    /// <summary>
    /// Removes outdated packages from our list
    /// Note: runs in N^2 but even with the best pc you wont have more data here than 8 so it should be fine
    /// </summary>
    void RemoveUnnecessaryNetworkData()
    {
        List<NetworkPoseBuffer> newestBuffer = new List<NetworkPoseBuffer>();

        foreach (NetworkPoseBuffer buffer in this.networkBuffer)
        {
            bool isNewest = true;

            foreach (NetworkPoseBuffer checkBuffer in this.networkBuffer)
            {
                if (buffer.camId == checkBuffer.camId)
                {
                    if (buffer.timestamp < checkBuffer.timestamp)
                    {
                        isNewest = false;
                        break;
                    }
                }
            }

            if (isNewest == true)
            {
                //get rid of unreliable connections (will improve multi cam detection) and then add it to our final buffer
                buffer.poses = this.RemoveNonConnectedGoups(buffer.poses);
                newestBuffer.Add(buffer);
            }

        }

        this.networkBuffer = newestBuffer;
    }

    /// <summary>
    /// Removes connections where only part of the limb etc. is detected (reduces jitter with multicam).
    /// Since this way we wont add single points which lacks in precision and therefore messes up the rotation calculation
    /// </summary>
    /// <param name="poses">PoseData array with current poses</param>
    /// <returns></returns>
    PoseData[] RemoveNonConnectedGoups(PoseData[] poses)
    {
        int groupStartIndex = 0;
        bool contained = true;

        for (int i = 0; i < BODY_GROUPS.Length; i++)
        {
            int bodyIdx = BODY_GROUPS[i];
            int realBodyIdx = Math.Abs(bodyIdx);

            if (bodyIdx == END_OF_GROUP)
            {
                contained = true;
                groupStartIndex = i + 1;
                continue;
            }
            else if (poses[realBodyIdx] == null)
            {
                if (contained == true)
                {
                    int iterator = groupStartIndex;

                    while (true)
                    {
                        int cIdx = BODY_GROUPS[iterator];//

                        if (cIdx == END_OF_GROUP)
                        {
                            break;
                        }
                        else if (cIdx > 0)
                        {
                            poses[cIdx] = null;
                        }

                        iterator++;
                    }
                }

                contained = false;
            }
        }

        return poses;
    }

    /// <summary>
    /// Get the focus camera / packages from our given buffer by searching for a match with the cam-id and if it is the newest package. If it fails it will recursively search for a better replacement (higher cam id as current one)
    /// </summary>
    /// <param name="camId">int of the camId we should check now</param>
    /// <returns></returns>
    NetworkPoseBuffer GetFocusCamFromNetworkBuffer(int camId = 0)
    {
        //initial variables and can track of the highest id in our list (usefull to avoid infinity loop)
        NetworkPoseBuffer focusCamBuffer = null;
        int highestId = -1;

        foreach (NetworkPoseBuffer buffer in this.networkBuffer)
        {

            highestId = Math.Max(highestId, buffer.camId);

            if (buffer.camId == camId)
            {
                if (focusCamBuffer != null)
                {
                    if (focusCamBuffer.timestamp < buffer.timestamp)
                    {
                        focusCamBuffer = buffer;
                    }
                }
                else
                {
                    focusCamBuffer = buffer;
                }
            }
        }

        if (focusCamBuffer == null || this.IsUsablePoseData(focusCamBuffer.poses) == false)
        {
            //its not longer possible for us to find a suitable main camera so therefore we pick the first we can find(avoiding infinite loop)
            if (camId + 1 > highestId || highestId == -1)
            {
                return this.networkBuffer[0];
            }

            return GetFocusCamFromNetworkBuffer(camId + 1);
        }

        return focusCamBuffer;
    }

    /// <summary>
    /// Checks if the given pose is actually usable (contains the head so we can extract the head rotation from it)
    /// </summary>
    /// <param name="poses">PoseData array of the current pose</param>
    /// <returns>bool if the pose is usable</returns>
    bool IsUsablePoseData(PoseData[] poses)
    {
        PoseData rightEye = this.GetPoseIndex(poses, 5);
        PoseData leftEye = this.GetPoseIndex(poses, 2);

        PoseData rightEar = this.GetPoseIndex(poses, 8);
        PoseData leftEar = this.GetPoseIndex(poses, 7);

        if (rightEye == null || leftEye == null || rightEar == null || leftEye == null)
        {
            return false;
        }

        return true;
    }

   
    /// <summary>
    /// Extracts the head rotation from the given pose data
    /// </summary>
    /// <param name="poses">PoseData array of the current pose</param>
    /// <returns>Quaternion of the head rotation</returns>
    Quaternion ExtractHeadRotation(PoseData[] poses)
    {
        PoseData rightEye = this.GetPoseIndex(poses, 5);
        PoseData leftEye = this.GetPoseIndex(poses, 2);

        PoseData rightEar = this.GetPoseIndex(poses, 8);
        PoseData leftEar = this.GetPoseIndex(poses, 7);

        if (rightEye == null || leftEye == null || rightEar == null || leftEye == null)
        {
            return Quaternion.identity;
        }

        Vector3 midEye = (rightEye.pos + leftEye.pos) * 0.5f;
        Vector3 midEar = (rightEar.pos + leftEar.pos) * 0.5f;


        Vector3 headDirX = (leftEye.pos - rightEye.pos).normalized;
        Vector3 headDirZ = (midEye - midEar).normalized;
        Vector3 headDirY = Vector3.Cross(headDirX, headDirZ);

        return Quaternion.LookRotation(headDirZ, headDirY);
    }

    /// <summary>
    /// Extract the positions data from a network package
    /// </summary>
    /// <param name="arr">Byte-array transmitted by udp</param>
    /// <param name="pos">int index of the position in the Byte-array </param>
    /// <returns>PoseData array of the current pose</returns>

    PoseData ExtractVector(byte[] arr, int pos)
    {
        int indx = arr[pos];
        double return1 = System.BitConverter.ToDouble(arr, pos + 1);
        double return2 = System.BitConverter.ToDouble(arr, pos + 9);
        double return3 = System.BitConverter.ToDouble(arr, pos + 17);

        return new PoseData(indx, new Vector3(-(float)return1, -(float)return2, -(float)return3));
    }

    /// <summary>
    /// small utility function to extract the y rotation of a quaternion
    /// </summary>
    /// <param name="q">Quaternion where the Y-Axis should be extracted</param>
    /// <returns>Quaternion with only the Y-Axis</returns>
    Quaternion GetYAxis(Quaternion q)
    {
        float theta = Mathf.Atan2(q.y, q.w);
        return new Quaternion(0.0f, Mathf.Sin(theta), 0.0f, Mathf.Cos(theta));
    }

    /// <summary>
    /// Solve inverse kinematics with a 3 connection chain by using FABRIK and returns the corresponding rotation data
    /// </summary>
    /// <param name="shoulder">Transform of the shoulder</param>
    /// <param name="elbow">Transform of the elbow</param>
    /// <param name="handWrist">Transform of the hand wrist</param>
    /// <param name="targetPos">Vector3 of the target position</param>
    /// <returns>Quaternions of the required rotation to match the target position</returns>
    
    (Quaternion, Quaternion) SolveArmIk(Transform shoulder, Transform elbow, Transform handWrist, Vector3 targetPos)
    {
        Vector3 rootPos = shoulder.position;

        Vector3 shoulderPos = shoulder.position;
        Vector3 elbowPos = elbow.position;
        Vector3 handWristPos = handWrist.position;

        float sELength = (elbow.position - shoulder.position).magnitude;
        float eHLength = (elbow.position - handWrist.position).magnitude;
        float accuracy = 1.0f;
        int maxIterations = 10;

        //perform fabirk (Forward and backward pass)
        while (accuracy > FABRIK_ACCURACY)
        {
            handWristPos = targetPos;
            elbowPos = handWristPos - (handWristPos - elbowPos).normalized * eHLength;
            shoulderPos = elbowPos - (elbowPos - shoulderPos).normalized * sELength;

            shoulderPos = rootPos;
            elbowPos = shoulderPos + (elbowPos - shoulderPos).normalized * sELength;
            handWristPos = elbowPos + (handWristPos - elbowPos).normalized * eHLength;

            accuracy = (handWristPos - targetPos).magnitude;
            maxIterations--;

            if (maxIterations <= 0)
            {
                break;
            }
        }

        //convert final position to rotation data
        Vector3 elbowDirZ = (elbowPos - handWristPos).normalized;
        Vector3 elbowDirY = Vector3.Cross(elbowDirZ, handWrist.right);

        Vector3 shoulderDirZ = (shoulderPos - elbowPos).normalized;
        Vector3 shoulderDirY = Vector3.Cross(shoulderDirZ, handWrist.right);


        return (Quaternion.LookRotation(shoulderDirZ, shoulderDirY),
                Quaternion.LookRotation(elbowDirZ, elbowDirY));

    }

    /// <summary>
    /// Applies the given pose to our avatar
    /// </summary>
    /// <param name="poses">PoseData array of the current pose</param>
    void ApplyPose(PoseData[] poses)
    {
        this.ApplyCachedPose();//to overwrite wrong values from the classical player

        PoseData[] tPoses = this.TransformPoseData(poses);

        if (this.showDebugCharacter == true)
        {
            this.ApplyDebugTransforms(poses);
        }


        this.CalculateShoulderAndHipsRotation(tPoses);
        this.CalculateHeadRotation(tPoses);
        this.CalculateArmsRotation(tPoses);


        //blending check to blend between the classic animation(when using the locomotion-system) and our real foot positions
        if (this.animationController.GetBool("isMoving") == false)
        {
            this.CalculateLegsRotation(tPoses, false);
        }
        else
        {
            this.CalculateLegsRotation(tPoses, true);

            Quaternion shiftY = Quaternion.Euler(0.0f, this.avatarShoulders.eulerAngles.y, 0.0f);
            Quaternion invHip = Quaternion.Inverse(this.avatarHip.rotation);
            
            this.avatarLeftUpLeg.rotation = shiftY * invHip * this.avatarLeftUpLeg.rotation;
            this.avatarRightUpLeg.rotation = shiftY * invHip * this.avatarRightUpLeg.rotation;
        }
    }



    /// <summary>
    /// Transform our local pose_data (local to webcam space) to the hmd space 
    /// </summary>
    /// <param name="poses"></param>
    /// <returns> PoseData array of the current pose</returns>
    PoseData[] TransformPoseData(PoseData[] poses)
    {
        Quaternion headRot = this.ExtractHeadRotation(poses);

        //note that we dont use euler here, since they would suffer from gimbal looks so we have to extract the axis directly from the quaternion 
        //otherweise we would have flipping issues when moving close to an "edge" of an axis
        Quaternion qHeadRotY = this.GetYAxis(headRot);
        Quaternion qRealHeadRotY = this.GetYAxis(this.xrDevice.rotation);

        Quaternion difference = Quaternion.Inverse(qHeadRotY) * qRealHeadRotY;
        PoseData[] tPoses = new PoseData[MAX_KEYPOINTS];


        foreach (PoseData pose in poses)
        {
            if (pose != null)
            {
                Vector3 poseFixed = difference * pose.pos;

                PoseData tPose = new PoseData(pose.idx, poseFixed);
                tPoses[pose.idx] = tPose;
            }
        }

        return tPoses;
    }

    /// <summary>
    /// Only for debugging to show the 3d pos estimation
    /// </summary>
    /// <param name="poses">PoseData array of the current pose</param>
    void ApplyDebugTransforms(PoseData[] poses)
    {
        for (int i = 0; i < MAX_KEYPOINTS; i++)
        {
            Transform obj = this.debugDrawObject.transform.GetChild(i);
            PoseData pose = this.GetPoseIndex(poses, i);

            if (pose != null)
            {
                obj.localPosition = pose.pos;
                obj.gameObject.SetActive(true);
            }
            else
            {
                obj.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < CONNECTIONS.Length; i += 2)
        {
            GameObject obj = this.lineRenderers[i / 2];
            LineRenderer l = obj.GetComponent<LineRenderer>();

            GameObject con1 = this.debugDrawObject.transform.GetChild(CONNECTIONS[i]).gameObject;
            GameObject con2 = this.debugDrawObject.transform.GetChild(CONNECTIONS[i + 1]).gameObject;

            Vector3 pos1 = con1.transform.position;
            Vector3 pos2 = con2.transform.position;

            if (con1.activeSelf == false || con2.activeSelf == false)
            {
                l.enabled = false;
            }
            else
            {
                l.enabled = true;
            }

            l.SetPosition(0, pos1);
            l.SetPosition(1, pos2);
        }
    }


    /// <summary>
    /// Saves the current rotation as the cache rotation
    /// </summary>
    void CacheCurrentRotation()
    {
        for (int i = 0; i < this.prevRotation.Length; i++)
        {
            this.prevRotation[i].rotation = this.usedAvatarTransforms[i].rotation;
        }
    }

    /// <summary>
    /// Applies the last cached transform data, so we wont flip between the fbt model and the "classic" model (reduce jittering if a certain pose wasnt detected)
    /// </summary>
    void ApplyCachedPose()
    {
        for (int i = 0; i < USED_AVATAR_TRANSFORM_COUNT; i++)
        {
            if (this.animationController.GetBool("isMoving") == false || i < 9)//to allow blending between the locomotion animation and our keypoints
            {
                this.usedAvatarTransforms[i].rotation = this.prevRotation[i].rotation;
            }

            this.prevRotation[i].updatedThisFrame = false;
        }
    }


    /// <summary>
    /// Calculate and applies the hip position
    /// </summary>
    /// <param name="poses">PoseData array of the current pose</param>
    void CalculateShoulderAndHipsRotation(PoseData[] poses)
    {
        PoseData leftHip = this.GetPoseIndex(poses, 23);
        PoseData rightHip = this.GetPoseIndex(poses, 24);

        PoseData leftShoulder = this.GetPoseIndex(poses, 11);
        PoseData rightShoulder = this.GetPoseIndex(poses, 12);

        if (leftHip == null || rightHip == null || leftShoulder == null || rightShoulder == null)
        {
            return;
        }

        Quaternion shiftRotX = Quaternion.Euler(180.0f, 0.0f, 0.0f);

        Vector3 hipMiddlePos = (leftHip.pos + rightHip.pos) * 0.5f;
        Vector3 shoulderMiddlePos = (leftShoulder.pos + rightShoulder.pos) * 0.5f;

        Vector3 hipDirX = (leftHip.pos - rightHip.pos).normalized;
        Vector3 hipDirY = (hipMiddlePos - shoulderMiddlePos).normalized;
        Vector3 hipDirZ = Vector3.Cross(hipDirY, hipDirX);

        Vector3 shouldersDirX = (leftShoulder.pos - rightShoulder.pos).normalized;
        Vector3 shouldersDirZ = Vector3.Cross(hipDirY, shouldersDirX);

        Quaternion rotAvatarHip = Quaternion.LookRotation(hipDirZ, hipDirY) * shiftRotX;
        Quaternion rotAvatarShoulder = Quaternion.LookRotation(shouldersDirZ, hipDirY) * shiftRotX;

        Quaternion rotAvatarXZ = Quaternion.Euler(rotAvatarHip.eulerAngles.x, 0.0f, rotAvatarHip.eulerAngles.z);
        Quaternion rotAvatarY = Quaternion.Euler(0.0f, rotAvatarHip.eulerAngles.y, 0.0f);

        this.avatar.rotation = rotAvatarY;
        this.avatarHip.localRotation = rotAvatarXZ;

        //this.avatarHip.rotation = rotAvatarHip;

        this.avatarShoulders.rotation = rotAvatarShoulder;

        this.prevRotation[0].UpdateRotation(rotAvatarHip);
        this.prevRotation[1].UpdateRotation(rotAvatarShoulder);
    }

    /// <summary>
    /// Calculate and applies the head rotation
    /// </summary>
    /// <param name="poses">PoseData array of the current pose</param>
    void CalculateHeadRotation(PoseData[] poses)
    {
        Quaternion rotAvatarHead = this.ExtractHeadRotation(poses);
        this.avatarHead.rotation = rotAvatarHead;

        this.prevRotation[2].UpdateRotation(rotAvatarHead);
    }

    /// <summary>
    /// calculates and applies the arm(left & right) rotation 
    /// </summary>
    /// <param name="poses">PoseData array of the current pose</param>
    void CalculateArmsRotation(PoseData[] poses)
    {
        PoseData leftShoulder = this.GetPoseIndex(poses, 11);
        PoseData leftElbow = this.GetPoseIndex(poses, 13);
        PoseData leftHand = this.GetPoseIndex(poses, 15);

        PoseData leftPinky = this.GetPoseIndex(poses, 17);
        PoseData leftIndex = this.GetPoseIndex(poses, 19);

        Quaternion shiftRotX = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
        //slightly adjusted to match hand coordinates and not controller coordinate
        Quaternion xrLshiftRot = Quaternion.Euler(0.0f, 0.0f, 90.0f) * Quaternion.Euler(90.0f, 0.0f, 0.0f);
        Quaternion xrRshiftRot = Quaternion.Euler(0.0f, 0.0f, -90.0f) * Quaternion.Euler(90.0f, 0.0f, 0.0f);

        if (leftShoulder != null && leftElbow != null && leftHand != null && leftPinky != null && leftIndex != null)
        {
            Quaternion controllerRot = this.leftVrController.rotation * xrLshiftRot;

            Vector3 lHandDirZ = (leftHand.pos - ((leftIndex.pos + leftPinky.pos) * 0.5f)).normalized;
            Vector3 lHandDirX = (leftIndex.pos - leftPinky.pos).normalized;
            Vector3 lHandDirY = Vector3.Cross(lHandDirZ, lHandDirX);

            Vector3 lElbowDirZ = (avatarLeftElbow.position - leftVrController.position).normalized;
            Vector3 lElbowDirY = Vector3.Cross(lElbowDirZ, (controllerRot * new Vector3(1.0f, 0.0f, 0.0f)));

            Vector3 lShoulderDirZ = (leftShoulder.pos - leftElbow.pos).normalized;
            Vector3 lShoulderDirY = Vector3.Cross(lShoulderDirZ, lHandDirX);

            this.avatarLeftShoulder.rotation = Quaternion.LookRotation(lShoulderDirZ, lShoulderDirY) * shiftRotX;
            this.avatarLeftElbow.rotation = Quaternion.LookRotation(lElbowDirZ, lElbowDirY) * shiftRotX;
            this.avatarLeftHand.rotation = controllerRot;

            this.prevRotation[3].UpdateRotation(avatarLeftShoulder.rotation);
            this.prevRotation[4].UpdateRotation(avatarLeftElbow.rotation);
            this.prevRotation[5].UpdateRotation(avatarLeftHand.rotation);
        }

        PoseData rightShoulder = this.GetPoseIndex(poses, 12);
        PoseData rightElbow = this.GetPoseIndex(poses, 14);
        PoseData rightHand = this.GetPoseIndex(poses, 16);

        PoseData rightPinky = this.GetPoseIndex(poses, 18);
        PoseData rightIndex = this.GetPoseIndex(poses, 20);

        if (rightShoulder != null && rightElbow != null && rightHand != null && rightPinky != null && rightIndex != null)
        {
            Quaternion controllerRot = this.rightVrController.rotation * xrRshiftRot;

            Vector3 rHandDirZ = (rightHand.pos - ((rightIndex.pos + rightPinky.pos) * 0.5f)).normalized;
            Vector3 rHandDirX = (rightPinky.pos - rightIndex.pos).normalized;//its swapped here because the order is different 
            Vector3 rHandDirY = Vector3.Cross(rHandDirZ, rHandDirX);

            Vector3 rElbowDirZ = (avatarRightElbow.position - rightVrController.position).normalized;
            Vector3 rElbowDirY = Vector3.Cross(rElbowDirZ, (controllerRot * new Vector3(1.0f, 0.0f, 0.0f)));

            Vector3 rShoulderDirZ = (rightShoulder.pos - rightElbow.pos).normalized;
            Vector3 rShoulderDirY = Vector3.Cross(rShoulderDirZ, rHandDirX);

            this.avatarRightShoulder.rotation = Quaternion.LookRotation(rShoulderDirZ, rShoulderDirY) * shiftRotX;
            this.avatarRightElbow.rotation = Quaternion.LookRotation(rElbowDirZ, rElbowDirY) * shiftRotX;
            this.avatarRightHand.rotation = controllerRot;

            this.prevRotation[6].UpdateRotation(avatarRightShoulder.rotation);
            this.prevRotation[7].UpdateRotation(avatarRightElbow.rotation);
            this.prevRotation[8].UpdateRotation(avatarRightHand.rotation);
        }

    }


    /// <summary>
    /// Calculate and applies the rotation data for the legs (left & right leg)
    /// </summary>
    /// <param name="poses">PoseData array of the current pose</param>
    /// <param name="onlyUpdateState">bool to define if only the internal data should be updated and should not be visible directly on the avatar</param>
    void CalculateLegsRotation(PoseData[] poses, bool onlyUpdateState)
    {
        PoseData leftHip = this.GetPoseIndex(poses, 23);
        PoseData leftKnee = this.GetPoseIndex(poses, 25);
        PoseData leftAnkle = this.GetPoseIndex(poses, 27);

        PoseData leftFootHeel = this.GetPoseIndex(poses, 29);
        PoseData leftFootTip = this.GetPoseIndex(poses, 31);

        Quaternion shiftRotX = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
        Quaternion footInitialRot = Quaternion.Euler(-this.initalFootRot.eulerAngles.x * 0.5f, 0.0f, 0.0f);

        if (leftHip != null && leftKnee != null && leftAnkle != null && leftFootHeel != null && leftFootTip != null)
        {
            //for now we align the foot always to be straight 
            //Vector3 lFootAngle = (leftAnkle.pos - leftFootTip.pos).normalized;
            if (onlyUpdateState == false)
            {

                Vector3 lFootDirZ = (leftFootHeel.pos - leftFootTip.pos);
                lFootDirZ.y = 0.0f;
                lFootDirZ = lFootDirZ.normalized;

                Vector3 lFootDirX = Vector3.Cross(Vector3.up, lFootDirZ).normalized;
                Vector3 lFootDirY = Vector3.Cross(lFootDirZ, lFootDirX);

                Vector3 lKneeDirZ = (leftKnee.pos - leftAnkle.pos).normalized;
                Vector3 lKneeDirY = Vector3.Cross(lKneeDirZ, lFootDirX);


                Vector3 lUpLegDirZ = (leftHip.pos - leftKnee.pos).normalized;
                Vector3 lUpLegDirY = Vector3.Cross(lUpLegDirZ, lFootDirX);

                Quaternion rotAvatarLeftUpLeg = Quaternion.LookRotation(lUpLegDirZ, lUpLegDirY) * shiftRotX;
                Quaternion rotAvatarLeftLeg = Quaternion.LookRotation(lKneeDirZ, lKneeDirY) * shiftRotX;
                Quaternion rotAvatarLeftFoot = Quaternion.LookRotation(lFootDirZ, lFootDirY) * shiftRotX * footInitialRot;

                this.avatarLeftUpLeg.rotation = rotAvatarLeftUpLeg;
                this.avatarLeftLeg.rotation = rotAvatarLeftLeg;
                this.avatarLeftFoot.rotation = rotAvatarLeftFoot;

                this.prevRotation[9].UpdateRotation(rotAvatarLeftUpLeg);
                this.prevRotation[10].UpdateRotation(rotAvatarLeftLeg);
                this.prevRotation[11].UpdateRotation(rotAvatarLeftFoot);
            }
            else
            {
                this.prevRotation[9].ForceUpdate();
                this.prevRotation[10].ForceUpdate();
                this.prevRotation[11].ForceUpdate();
            }


        }


        PoseData rightHip = this.GetPoseIndex(poses, 24);
        PoseData rightKnee = this.GetPoseIndex(poses, 26);
        PoseData rightAnkle = this.GetPoseIndex(poses, 28);

        PoseData rightFootHeel = this.GetPoseIndex(poses, 30);
        PoseData rightFootTip = this.GetPoseIndex(poses, 32);

        if (rightHip != null && rightKnee != null && rightAnkle != null && rightFootHeel != null && rightFootTip != null)
        {

            if (onlyUpdateState == false)
            {
                Vector3 rFootDirZ = (rightFootHeel.pos - rightFootTip.pos);
                rFootDirZ.y = 0.0f;
                rFootDirZ = rFootDirZ.normalized;

                Vector3 rFootDirX = Vector3.Cross(Vector3.up, rFootDirZ).normalized; //swapped 
                Vector3 rFootDirY = Vector3.Cross(rFootDirZ, rFootDirX);

                Vector3 rKneeDirZ = (rightKnee.pos - rightAnkle.pos).normalized;
                Vector3 rKneeDirY = Vector3.Cross(rKneeDirZ, rFootDirX);


                Vector3 rUpLegDirZ = (rightHip.pos - rightKnee.pos).normalized;
                Vector3 rUpLegDirY = Vector3.Cross(rUpLegDirZ, rFootDirX);

                Quaternion rotAvatarRightUpLeg = Quaternion.LookRotation(rUpLegDirZ, rUpLegDirY) * shiftRotX;
                Quaternion rotAvatarRightLeg = Quaternion.LookRotation(rKneeDirZ, rKneeDirY) * shiftRotX;
                Quaternion rotAvatarRightFoot = Quaternion.LookRotation(rFootDirZ, rFootDirY) * shiftRotX * footInitialRot;

                this.avatarRightUpLeg.rotation = rotAvatarRightUpLeg;
                this.avatarRightLeg.rotation = rotAvatarRightLeg;
                this.avatarRightFoot.rotation = rotAvatarRightFoot;

                this.prevRotation[12].UpdateRotation(rotAvatarRightUpLeg);
                this.prevRotation[13].UpdateRotation(rotAvatarRightLeg);
                this.prevRotation[14].UpdateRotation(rotAvatarRightFoot);
            }
            else
            {
                this.prevRotation[12].ForceUpdate();
                this.prevRotation[13].ForceUpdate();
                this.prevRotation[14].ForceUpdate();
            }

        }
    }


    /// <summary>
    /// Helper function to get a single entries from the pose array and to make sure that we wont go out of bounds
    /// </summary>
    /// <param name="poses">PoseData array of the current pose</param>
    /// <param name="index"></param>
    /// <returns>PoseData for the given index</returns>
    PoseData GetPoseIndex(PoseData[] poses, int index)
    {
        if (index >= MAX_KEYPOINTS || index < 0)
        {
            return null;
        }

        return poses[index];
    }
}
