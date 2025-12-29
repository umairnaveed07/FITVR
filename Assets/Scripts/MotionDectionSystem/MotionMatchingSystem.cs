using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class that handles all of the motion matching specific codes and also the modes (demo mode, intruction mode etc.)
/// </summary>
public class MotionMatchingSystem : MonoBehaviour
{

    public const float MAX_ANGLE = 90.0f; // choosen based on this paper https://www.ncbi.nlm.nih.gov/pmc/articles/PMC6651850/

    //they are controllable variables instead of constants (for now) because sometimes the user might get stuck in the instructions and due to inprecisions in the kinect and therefore we can adjust this while the user is playing it
    public float REQUIRED_INSTRUCTION_MATCHING = 0.92f;
    public float INSTRUCTIONS_TARGRT_PERCENTAGE = 92.0f;

    private const float ARROW_DISTANCE = 0.065f;

    //based on the idea of the slope & window restriction from this paper: https://www.researchgate.net/publication/324177668_Gesture_Recognition_Using_Dynamic_Time_Warping_and_Kinect_A_Practical_Approach/link/5c12ac6e299bf139c756bd2a/
    private const int WINDOW_SIZE = 20;
    private const int MAX_SLOPE_SIZE = 4;

    private const float GREAT_SOUND = 0.8f;
    private const float GOOD_SOUND = 0.7f;
    private const float PERCENTAGE_RANGE = 0.4f;

    private const float COUNTDOWN_GRAPH_TIME = 3.0f;
    private const float COUNTDOWN_TIME = 5.0f;

    private const float MIN_MOTION_DISTANCE = 0.02f;
    private const float TIME_TO_CHECK_FOR_MOTION = 0.175f;

    private const float WAIT_FOR_START = 0.45f;
    private const float WAIT_FOR_FINISH = 0.2f;
    private const float MATCHING_START_ANGLE = 10.0f;
    private const float TIME_SKIPS = 10.0f;


    public struct LastDTWEntry
    {
        public float[,] dtwMatrix;
        public List<FBTRecorder.Frame> playerArray;
        public List<FBTRecorder.Frame> recordingArray;
        public List<DTWCalculator.DTWPath> bestPath;
        public List<int> recordedBones;
        public int involvedBoneCount;
        public float matchingPercentage;
    }

    enum State : ushort
    {
        None = 0,
        Countdown = 1,
        PerformWorkout = 2,
        PerformPreview = 3,
        PerformInstruction = 4,
        ShowGraph = 5,
    }

    [Header("UI buttons")]
    public List<Button> groupButtons;
    public Text percentageText;
    public Text additionalInformations;

    [Header("Sounds")]
    public AudioSource greatSound;
    public AudioSource okSound;
    public AudioSource badSound;

    [Header("Others")]
    public GraphSystem graphSystem;
    public AvatarVisualizerManager stickFigureManager;
    public GameObject AvatarRecordingVisualizer;
    public GameObject AvatarTrainingRegions;

    public Color matchinColor;
    public Color worstMarchingColor;
    public GameObject arrowPrefab;


    [Header("User Study")]
    public bool inTestEnvironment = false;
    private List<GameObject> spawnedArrows = new List<GameObject>();

    private FBTRecorder fbtRecorder;
    private FBTRecorderManager fbtRecorderManager;
    private List<FBTRecorder.Frame> playerRecording;
    private LastDTWEntry lastDTWEntry = new LastDTWEntry();

    private State cState = State.None;
    private State nState = State.None;

    private bool usedInstructionsOnce = false;
    private bool redidWorkout = false;

    private float countdown = COUNTDOWN_TIME;
    private AngleCostFunction costFunction;

    private int currentInstructionFrame = 0;
    private int repititionCount = 0;
    private int previewCount = 0;
    private int instructionCount = 0;

    private long previewTime = 0;
    private long instructionTime = 0;
    private long workoutTime = 0;

    private long totalPreviewTime = 0;
    private long totalInstructionTime = 0;
    private long totalWorkoutTime = 0;


    /// <summary>
    /// Initialize the motion matching system by getting a reference to the FBTRecorder (to get the trainer reference data)
    /// </summary>
    /// <param name="manager">FBTRecorderManager reference</param>
    public void Initialize(FBTRecorderManager manager)
    {
        MotionMatchingDBManager.CreateNewUser();
        MotionMatchingDBManager.CreateUserRecordingTablesIfNotExisiting();

        this.fbtRecorderManager = manager;
        this.fbtRecorder = this.fbtRecorderManager.GetRecorder();
        this.playerRecording = new List<FBTRecorder.Frame>();
        this.costFunction = new AngleCostFunction(this.fbtRecorder);

        for (int i = 0; i < this.fbtRecorder.GetPlayerRotation().Length; i++)
        {
            GameObject arrow = Instantiate(this.arrowPrefab, Vector3.zero, Quaternion.identity);
            arrow.transform.parent = this.transform;

            this.spawnedArrows.Add(arrow);
            arrow.SetActive(false);
        }
    }

    /// <summary>
    /// Enable or disable all of the buttons the motionmatchingsystem handles
    /// </summary>
    /// <param name="en">bool if the buttons should be enabled or disabled</param>
    public void EnableDisableAllGroupButtons(bool en)
    {
        for (int i = 0; i < this.groupButtons.Count; i++)
        {
            this.groupButtons[i].interactable = en;
        }
    }

    /// <summary>
    /// Enabled or disables all of the given buttons which are passed in as an array
    /// </summary>
    /// <param name="arr">int-array with all the buttons that should be enabled/disabled</param>
    /// <param name="en">bool if the buttons should be enabled or disabled</param>
    public void EnableDisableSpecificGroupButtons(int[] arr, bool en = true)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            this.groupButtons[arr[i]].interactable = en;
        }
    }

    /// <summary>
    /// Rearrange the given percentage values to indicate errors more
    /// </summary>
    /// <param name="percentage">float of the percentage</param>
    /// <returns>float of the converted percentage</returns>
    private float ConvertPercentage(float percentage)
    {
        float shifted = Mathf.Max(percentage - PERCENTAGE_RANGE, 0.0f);
        return Mathf.Min(shifted / (1.0f - PERCENTAGE_RANGE), 1.0f);
    }



    /// <summary>
    /// Get the peformance based on the last DTW results. Based on the formular from this paper https://www.ncbi.nlm.nih.gov/pmc/articles/PMC6651850/
    /// </summary>
    /// <param name="lastEntry">LastDTWEntry</param>
    /// <returns>float of the converted percentage</returns>
    private float GetPerformance(LastDTWEntry lastEntry)
    {
        if (lastEntry.bestPath.Count <= 2)
        {
            return 0.0f;
        }

        float totalSum = lastEntry.bestPath[0].sumValue;
        float percentage = 1.0f - (totalSum / (MAX_ANGLE * (float)lastEntry.involvedBoneCount * (float)lastEntry.bestPath.Count));
        return this.ConvertPercentage(percentage);
    }

    /// <summary>
    /// Called by the instruction class to indicated that the instructions are now done
    /// </summary>
    public void InstructionsComplete()
    {
        this.groupButtons[0].interactable = true; //allow the loading of the workout after the instructions are done 
    }

    /// <summary>
    /// Called by the recording manager to indicate that a recording was loaded so that the motiondetection manager are aware of this
    /// </summary>
    public void RecordingLoaded()
    {
        this.usedInstructionsOnce = false;
        this.redidWorkout = false;

        //reset all the database dependend data 
        this.repititionCount = 0;
        this.previewCount = 0;
        this.instructionCount = 0;

        this.previewTime = 0;
        this.instructionTime = 0;
        this.workoutTime = 0;

        this.totalPreviewTime = 0;
        this.totalInstructionTime = 0;
        this.totalWorkoutTime = 0;

        //disable every button after loading a new workout except for the preview to enforce the user to start with it 
        if (this.inTestEnvironment == false)
        {
            this.EnableDisableAllGroupButtons(false);
            this.groupButtons[this.groupButtons.Count - 2].interactable = true;
        }
        else
        {
            this.EnableDisableAllGroupButtons(true);
            this.groupButtons[this.groupButtons.Count - 1].interactable = false;
        }
    }

    /// <summary>
    /// Called by the ui to start the workout mode
    /// </summary>
    public void StartWorkout()
    {
        if (this.fbtRecorder.HasRecordingLoaded() == false)
        {
            return;
        }

        this.EnableDisableAllGroupButtons(false);
        this.groupButtons[this.groupButtons.Count - 1].interactable = true; //the last element will always be the stop element

        this.additionalInformations.text = "Informations:\n\n'A' or 'X' for matching percentage\n'B' or 'Y' for graph\n'B' or 'Y' to leave graph";

        this.cState = State.Countdown;
        this.nState = State.PerformWorkout;
        this.countdown = COUNTDOWN_TIME;
        this.repititionCount = 0;
        this.PrepareAvatarPreview();
    }

    /// <summary>
    /// Called by the ui to start the instructions mode
    /// </summary>
    public void StartInstructions()
    {
        if (this.fbtRecorder.HasRecordingLoaded() == false)
        {
            return;
        }

        FBTRecorder.Recording rec = this.fbtRecorder.GetLoadedRecording();
        
        if(rec.instructionPointsOfInterest.Count <= 0)
        {
            return;
        }

        this.EnableDisableAllGroupButtons(false);
        this.groupButtons[this.groupButtons.Count - 1].interactable = true; //the last element will always be the stop element

        this.additionalInformations.text = "Informations:\n\nReach 92% to get to the next step";
        this.currentInstructionFrame = 0;

        this.cState = State.Countdown;
        this.nState = State.PerformInstruction;

        this.countdown = COUNTDOWN_TIME;
        this.PrepareAvatarPreview();

        this.fbtRecorder.PlaySpecificFrame(rec.instructionPointsOfInterest[0]);
    }

    /// <summary>
    /// Called by the ui to start the preview/demo mode
    /// </summary>
    public void StartPreview()
    {
        if (this.fbtRecorder.HasRecordingLoaded() == false)
        {
            return;
        }

        this.EnableDisableAllGroupButtons(false);
        this.groupButtons[this.groupButtons.Count - 1].interactable = true; //the last element will always be the stop element

        this.cState = State.Countdown;
        this.nState = State.PerformPreview;

        this.countdown = COUNTDOWN_GRAPH_TIME;
        this.PrepareAvatarPreview();
    }

    /// <summary>
    /// Called by the ui to stop the current workout (also instruction mode etc. will be stoped with this)
    /// </summary>
    public void StopWorkout()
    {
        this.EnableDisableAllGroupButtons(true);
        this.groupButtons[this.groupButtons.Count - 1].interactable = false; //the last element will always be the stop element

        State oldState = this.nState;

        this.additionalInformations.text = "";
        this.percentageText.text = "What would you like \nto do next?";

        this.cState = State.None;
        this.fbtRecorderManager.StopPlaying();
        this.playerRecording = new List<FBTRecorder.Frame>();

        AvatarStickVisualizer trainer = this.stickFigureManager.GetTrainerVisualizer();
        trainer.gameObject.SetActive(false);
        this.graphSystem.gameObject.SetActive(false);

        this.AvatarRecordingVisualizer.SetActive(false);
        this.AvatarTrainingRegions.SetActive(true);

        this.stickFigureManager.GetPlayerVisualizer().ResetColor();
        this.repititionCount = 0;

        this.stickFigureManager.SetCustomStickfigureUpdate(false);

        for (int i = 0; i < this.spawnedArrows.Count; i++)
        {
            this.spawnedArrows[i].SetActive(false);
        }

        //calculations for the time
        if (oldState == State.PerformInstruction)
        {
            this.instructionTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - this.instructionTime;
            this.totalInstructionTime += this.instructionTime;
        }
        else if (oldState == State.PerformPreview)
        {
            this.previewTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - this.previewTime;
            this.totalPreviewTime += previewTime;
        }
        else if (oldState == State.PerformWorkout)
        {
            this.workoutTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - this.workoutTime;
            //note that we dont add the workout time here since stopping the workout shouldnt be part of it 
        }

        if (this.inTestEnvironment == false && this.usedInstructionsOnce == false)
        {
            this.groupButtons[0].interactable = false;//dont allow to load a new workout before the user hasnt performed the instruction

            //make sure, that we can only do the normal workout after that so we can distinct between workout before and after instructions
            if (oldState == State.PerformPreview)
            {
                this.groupButtons[2].interactable = false;
                this.groupButtons[3].interactable = false;
            }
            //make sure that we enforce the instruction mode after a normal workout 
            else if (oldState == State.PerformWorkout)
            {
                this.groupButtons[1].interactable = false;
                this.groupButtons[3].interactable = false;
            }
        }
        else if(this.usedInstructionsOnce == true && this.redidWorkout == false)
        {
            //makes sure that we are enforcing the normal workout once more 
            this.redidWorkout = true;

            this.groupButtons[0].interactable = false;
            this.groupButtons[2].interactable = false;
            this.groupButtons[3].interactable = false;
        }

    }

    /// <summary>
    /// Prepares the avatar preview (by reseting the played frame etc.)
    /// </summary>
    public void PrepareAvatarPreview()
    {
        this.AvatarRecordingVisualizer.SetActive(true);
        this.AvatarTrainingRegions.SetActive(false);
        this.fbtRecorder.ResetFrame();

        this.stickFigureManager.SetCustomStickfigureUpdate(false);
        AvatarStickVisualizer trainer = this.stickFigureManager.GetTrainerVisualizer();

        trainer.gameObject.SetActive(true);

        FBTRecorder.Recording rec = this.fbtRecorder.GetLoadedRecording();
        trainer.ShowOnlyFocusBones(rec.recordedBones);
    }

    /// <summary>
    /// Updates the current state we are in
    /// </summary>
    void Update()
    {

        if (this.cState == State.Countdown)
        {
            this.PerformCountdown();
        }
        else if (this.cState == State.PerformWorkout)
        {
            this.PerformWorkout();
        }
        else if (this.cState == State.PerformPreview)
        {
            this.PerformPreview();
        }
        else if (this.cState == State.PerformInstruction)
        {
            this.PerformInstruction();
        }
        else if (this.cState == State.ShowGraph)
        {
            this.PerformShowGraph();
        }
    }

    /// <summary>
    /// Performs the countdown state and after we reached zero move to the next state (which is internally stored as a vatiable nState)
    /// </summary>
    void PerformCountdown()
    {
        this.countdown -= Time.deltaTime;
        this.percentageText.text = "Countdown: " + (Mathf.Round((this.countdown * 10.0f)) / 10.0f) + "s";

        if (this.countdown <= 0.0f)
        {
            this.cState = this.nState;
            this.playerRecording.Clear();

            //set the timers accordingly to measure the whole time between start and stop of an activity
            if(this.cState == State.PerformInstruction)
            {
                this.instructionTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
            else if(this.cState == State.PerformPreview)
            {
                this.previewTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
            else if (this.cState == State.PerformWorkout)
            {
                this.workoutTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                this.fbtRecorderManager.PlayLoadedRecording();
            }
        }
    }

    /// <summary>
    /// Displays the graph based on the lasDTWEntry performance
    /// </summary>
    /// <param name="lastDTWEntry">LastDTWEntry-Struct</param>
    private void DisplayGraph(LastDTWEntry lastDTWEntry)
    {
        this.graphSystem.gameObject.SetActive(true);
        this.stickFigureManager.gameObject.SetActive(false);

        float[] yData = GetPerformanceDiagramDataY(lastDTWEntry);
        float[] xData = GetPerformanceDiagramDataX(lastDTWEntry);

        this.graphSystem.PlotGraph(xData, yData);
    }

    /// <summary>
    /// Gets all the data for the Y-Axis of our graph
    /// </summary>
    /// <param name="lastEntry">LastDTWEntry-Struct</param>
    /// <returns>float array of the y entries for the y-axis</returns>
    private float[] GetPerformanceDiagramDataY(LastDTWEntry lastEntry)
    {
        float[] result = new float[lastEntry.playerArray.Count];
        int xIndex = 0;

        for (int i = 0; i < lastEntry.bestPath.Count; i++)
        {
            if (lastEntry.bestPath[i].dX == 0 && i != 0)
            {
                continue;//only show values when we moved on the player recording site 
            }

            float maxAngle = lastEntry.bestPath[i].singleValue;
            float normAngle = maxAngle / ((float)lastEntry.involvedBoneCount * MAX_ANGLE);

            result[(lastEntry.playerArray.Count - 1) - xIndex] = this.ConvertPercentage(1.0f - normAngle); //store the values in reverse to make sense 
            xIndex++;
        }

        return result;
    }

    /// <summary>
    /// Get all of the plot points for the x-axis of the graph
    /// </summary>
    /// <param name="lastEntry">LastDTWEntry-Struct</param>
    /// <returns>float array of the x entries for the x-axis</returns>
    private float[] GetPerformanceDiagramDataX(LastDTWEntry lastEntry)
    {
        float[] result = new float[lastEntry.playerArray.Count];

        float cTime = 0;

        for (int i = 0; i < lastEntry.playerArray.Count; i++)
        {
            result[i] = cTime;
            cTime += 1.0f / lastEntry.playerArray[i].fps; //one entry so we move 1 fps/ timeslot further
        }

        return result;
    }

    /// <summary>
    /// Returns the current frame of the player
    /// </summary>
    /// <returns>List of FBTRecorder.Frame</returns>
    public List<FBTRecorder.Frame> GetCurrentPlayerArray()
    {
        return this.playerRecording;
    }

    /// <summary>
    /// Returns the current frame of the recording
    /// </summary>
    /// <returns>List of FBTRecorder.Frame</returns>
    public List<FBTRecorder.Frame> GetCurrentRecordingArray()
    {
        FBTRecorder.Recording rec = this.fbtRecorder.GetLoadedRecording();
        return rec.frames;
    }

    /// <summary>
    /// Returns the last dtw performances as a struct
    /// </summary>
    /// <returns>LastDTWEntry-struct</returns>
    public LastDTWEntry GetLastPerformanceData()
    {
        return this.lastDTWEntry;
    }

    /// <summary>
    /// Displays the current selected frame from the graph with the supposed posture and the target posture
    /// </summary>
    /// <param name="xIndex">int of the selected entry</param>
    public void ShowTimeFrame(int xIndex)
    {
        LastDTWEntry lastEntry = this.lastDTWEntry;

        if (xIndex < 0 || xIndex >= lastEntry.playerArray.Count)
        {
            return;
        }

        int yIndex = 0;

        for (int i = 0; i < lastEntry.bestPath.Count; i++)
        {
            if (lastEntry.bestPath[i].posX == xIndex)
            {
                yIndex = lastEntry.bestPath[i].posY;
                break;
            }
        }

        this.stickFigureManager.GetTrainerVisualizer().gameObject.SetActive(true);
        this.stickFigureManager.GetPlayerVisualizer().gameObject.SetActive(true);

        this.stickFigureManager.gameObject.SetActive(true);
        this.stickFigureManager.SetCustomStickfigureUpdate(true);

        FBTRecorder.Frame playerArray = lastEntry.playerArray[xIndex];
        FBTRecorder.Frame recordingArray = lastEntry.recordingArray[yIndex];

        List<Quaternion> playerQuaternions = new List<Quaternion>();
        List<Quaternion> recorderQuaternions = new List<Quaternion>();

        for (int i = 0; i < playerArray.keypoints.Count; i++)
        {
            playerQuaternions.Add(playerArray.keypoints[i].localRotation);
        }

        for (int i = 0; i < recordingArray.keypoints.Count; i++)
        {
            recorderQuaternions.Add(recordingArray.keypoints[i].localRotation);
        }

        this.stickFigureManager.GetPlayerVisualizer().UpdateRotations(playerQuaternions);
        this.stickFigureManager.GetTrainerVisualizer().UpdateRotations(recorderQuaternions);
    }

    /// <summary>
    /// Manages the show graph state to get back to the workout state
    /// </summary>
    void PerformShowGraph()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two) || OVRInput.GetDown(OVRInput.Button.Four))
        {
            this.fbtRecorder.ResetFrame();

            this.countdown = COUNTDOWN_GRAPH_TIME;
            this.cState = State.Countdown;
            this.nState = State.PerformWorkout;

            this.stickFigureManager.SetCustomStickfigureUpdate(false);

            this.graphSystem.gameObject.SetActive(false);
            this.stickFigureManager.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Manages the preview/demo mode state
    /// </summary>
    void PerformPreview()
    {
        this.percentageText.text = "Demo mode";
        bool isRepeating = this.fbtRecorder.PlayRecordingFrame();

        if (isRepeating == true)
        {
            this.previewCount++;
        }

    }

    /// <summary>
    /// Manages the instruction mode state
    /// </summary>
    void PerformInstruction()
    {
        this.usedInstructionsOnce = true;//so we can differentiate between preview and instructions

        FBTRecorder.Recording rec = this.fbtRecorder.GetLoadedRecording();

        FBTRecorder.Frame playerFrame = new FBTRecorder.Frame();
        FBTRecorder.Frame recorderFrame = this.fbtRecorder.GetCurrentFrame();


        playerFrame.keypoints = this.fbtRecorder.GetPlayerCurrentKeyPoints();
        playerFrame.fps = this.fbtRecorder.GetLastFrameFps();


        if (playerFrame.keypoints == null || recorderFrame == null || rec.instructionPointsOfInterest.Count <= 0)
        {
            return;
        }

        AvatarStickVisualizer trainerFigure = this.stickFigureManager.GetTrainerVisualizer();
        AvatarStickVisualizer playerFigure = this.stickFigureManager.GetPlayerVisualizer();

        float[] matchings = this.costFunction.GetDTWCostsArray(playerFrame, recorderFrame);
        float matching = 0.0f;

        for (int i = 0; i < matchings.Length; i++)
        {
            if (matchings[i] <= 0.0f)
            {
                continue;
            }

            float boneMatchingAsPercentage = matchings[i] / MAX_ANGLE;
            Color tColor = Color.Lerp(this.matchinColor, this.worstMarchingColor, boneMatchingAsPercentage);
            playerFigure.SetConnectedBoneColor(i, tColor);

            matching += matchings[i];


            if ((1.0f - boneMatchingAsPercentage) < REQUIRED_INSTRUCTION_MATCHING)
            {

                //arrow calculations
                var playerBone = playerFigure.GetBoneCenterPosition(i);
                var trainerBone = trainerFigure.GetBoneCenterPosition(i);

                Vector3 direction = (playerBone.Item2 - trainerBone.Item2).normalized;

                this.spawnedArrows[i].transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90.0f, 0.0f, 0.0f);
                this.spawnedArrows[i].transform.position = playerBone.Item1 + playerBone.Item3 * ARROW_DISTANCE;


                this.spawnedArrows[i].SetActive(true);

            }
            else
            {
                this.spawnedArrows[i].SetActive(false);
            }

        }


        float asPercentage = 1.0f - (matching / (MAX_ANGLE * rec.involvedBoneCount));

        asPercentage = Mathf.Round(asPercentage * 10000.0f) / 100.0f;
        this.percentageText.text = "Your turn!\nMatching performance: " + asPercentage.ToString() + "%\n Repitition: " + this.instructionCount.ToString();


        if (asPercentage >= INSTRUCTIONS_TARGRT_PERCENTAGE)
        {
            this.greatSound.Play();

            bool isRepeating = false;
            int targetFrame = 0;

            if(this.currentInstructionFrame >= rec.instructionPointsOfInterest.Count-1)
            {
                this.currentInstructionFrame = 0;
                isRepeating = true;
            }
            else
            {
                this.currentInstructionFrame += 1;
            }

            targetFrame = rec.instructionPointsOfInterest[this.currentInstructionFrame];
            this.fbtRecorder.PlaySpecificFrame(targetFrame);


            if (isRepeating == true)
            {
                this.instructionCount++;
            }


        }

    }

    /// <summary>
    /// Saves the current performances etc. in the database
    /// </summary>
    /// <param name="lastEntry">LastDTWEntry-Struct with holds all of the processed data</param>
    void SaveUserData(LastDTWEntry lastEntry)
    {
        if (this.inTestEnvironment == true)
        {
            return;
        }

        string workoutName = this.fbtRecorderManager.GetLoadedRecording();

        if (this.usedInstructionsOnce == true)
        {
            workoutName += "_after_instructions";
        }

        MotionMatchingDBManager.SavePlayerRecordings(MotionMatchingDBManager.GetUserStudyID(), workoutName, 
            this.previewCount, this.instructionCount, this.repititionCount, 
            this.totalPreviewTime, this.totalInstructionTime, this.totalWorkoutTime,
            lastDTWEntry.matchingPercentage, lastDTWEntry.playerArray);
    }

    /// <summary>
    /// Manages the workout mode state
    /// </summary>
    void PerformWorkout()
    {
        FBTRecorder.Recording rec = this.fbtRecorder.GetLoadedRecording();

        FBTRecorder.Frame newFrame = new FBTRecorder.Frame();
        newFrame.keypoints = this.fbtRecorder.GetPlayerCurrentKeyPoints();
        newFrame.fps = this.fbtRecorder.GetLastFrameFps();


        if (newFrame.keypoints != null)
        {
            this.playerRecording.Add(newFrame);
        }

        if (this.playerRecording.Count <= 1)
        {
            return;
        }

        bool showPerformance = false;
        bool showDiagram = false;

        if (OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.Three))
        {
            showPerformance = true;
        }
        else if (OVRInput.GetDown(OVRInput.Button.Two) || OVRInput.GetDown(OVRInput.Button.Four))
        {
            showPerformance = true;
            showDiagram = true;
        }

        if (repititionCount <= 0)
        {
            this.percentageText.text = "Your turn!";
        }

        if (showPerformance == true)
        {
            //do the time calculation before so it doesnt get influenced by the performance calculations etc. 
            this.workoutTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - this.workoutTime;
            this.totalWorkoutTime += this.workoutTime;

            float[,] dtwMatrix = DTWCalculator.PerformWindowDTW(this.playerRecording, rec.frames, this.costFunction, WINDOW_SIZE);

            this.lastDTWEntry.dtwMatrix = dtwMatrix;
            this.lastDTWEntry.playerArray = this.playerRecording;
            this.lastDTWEntry.recordingArray = rec.frames;
            this.lastDTWEntry.recordedBones = rec.recordedBones;

            this.lastDTWEntry.bestPath = DTWCalculator.GetSlopeRestrictedOptimalPath(dtwMatrix, MAX_SLOPE_SIZE);
            this.lastDTWEntry.bestPath = DTWCalculator.RecalculatePathValues(this.lastDTWEntry.bestPath, dtwMatrix, this.costFunction, this.playerRecording, rec.frames);

            this.lastDTWEntry.involvedBoneCount = rec.involvedBoneCount;
            this.lastDTWEntry.matchingPercentage = this.GetPerformance(lastDTWEntry);

            float dtwPerformance = this.lastDTWEntry.matchingPercentage;
            float dtwAsPercentage = Mathf.Round(dtwPerformance * 10000.0f) / 100.0f;


            this.repititionCount++;
            this.percentageText.text = "Last performance: " + dtwAsPercentage.ToString() + "% \nRepition: " + this.repititionCount.ToString() + "\n";

            if (dtwPerformance >= GREAT_SOUND)
            {
                this.greatSound.Play();
                this.percentageText.text += "Great !";
            }
            else if (dtwPerformance >= GOOD_SOUND)
            {
                this.okSound.Play();
                this.percentageText.text += "Good !";
            }
            else
            {
                this.badSound.Play();
                this.percentageText.text += "Bad !";
            }


            if (showDiagram == true)
            {
                this.DisplayGraph(this.lastDTWEntry);
                this.fbtRecorderManager.StopPlaying();
                this.cState = State.ShowGraph;
            }

            

            this.SaveUserData(this.lastDTWEntry);
            this.playerRecording = new List<FBTRecorder.Frame>();//so that the pointer to the lastdtwentry doesnt get removed

            this.workoutTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }

}
