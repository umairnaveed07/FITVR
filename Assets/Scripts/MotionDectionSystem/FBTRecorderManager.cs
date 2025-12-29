

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// Class that manages the Recording and the Avatar Recording but on a top level so that all the UI elements are handled here  
/// </summary>
public class FBTRecorderManager : NetworkStart
{
    private const float COUNTDOWN_TIME = 3.0f;
    private const float TRACKING_MARGIN_TIME = 2.0f;

    private float countdown = COUNTDOWN_TIME;
    private string loadedRecording = "";

    enum State : ushort
    {
        None = 0,
        CountdownRecording = 1,
        Recording = 2,
        Playing = 3
    }

    public AvatarBoneHighlighter boneHighlighter;
    public AvatarVisualizerManager avatarVisualizerManager;
    public MotionMatchingSystem mSystem;

    private FBTRecorder recorder;

    [Header("UI buttons")]
    public List<Button> groupButtons;
    public Dropdown workoutList;
    public Text percentageText;
    public TMP_InputField recordName;
    public Image fbtBodyVisible;

    private float outOfTrackingTime = TRACKING_MARGIN_TIME;
    private State cState = State.None;

    /// <summary>
    /// Resets the bone highlighter to its selection (which is nothing in this case)
    /// </summary>
    void Start()
    {
        if(this.boneHighlighter.isActiveAndEnabled == true)
        {
            this.boneHighlighter.SetBoneSelection(new List<int>());
        }
        
    }

    /// <summary>
    /// Called by the network manager to tell us that we have a valid connection to the server and therefore can access the local player
    /// </summary>
    /// <param name="localPlayer"> Reference of the local player game object</param>
    public override void OnNetworkConnectionInitilaized(GameObject localPlayer)
    {
        this.recorder = this.GetComponentInChildren<FBTRecorder>();
        this.recorder.Initialize(localPlayer);
        this.avatarVisualizerManager.Initialize(this.recorder);
        this.mSystem.Initialize(this);

        this.PopulateUIDropdown();
    }

    /// <summary>
    /// Updates the state we are currently in and also manages the indicator if the whole body isnt visible (the small icon in the top corner)
    /// </summary>
    void Update()
    {
        if(this.recorder == null)
        {
            return;
        }

        if (this.cState == State.Recording)
        {
            if (OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.Three))
            {
                this.StopUIRecording();
            }
            else
            {
                this.recorder.RecordFrame();
            }
        }
        else if (this.cState == State.Playing)
        {
            this.recorder.PlayRecordingFrame();

        }
        else if(this.cState == State.CountdownRecording)
        {
            this.StatePerformCountdown();
        }

        if(this.recorder.GetFBTManager().IsWholeBodyVisible() == false)
        {
            this.outOfTrackingTime -= Time.deltaTime;

            if(this.outOfTrackingTime <= 0.0f)
            {
                this.fbtBodyVisible.gameObject.SetActive(true);
            }
        }
        else
        {
            this.fbtBodyVisible.gameObject.SetActive(false);
            this.outOfTrackingTime = TRACKING_MARGIN_TIME;
        }
    }

    /// <summary>
    /// Get the recorder game object
    /// </summary>
    /// <returns>Recorder as GameObject</returns>
    public FBTRecorder GetRecorder()
    {
        return this.recorder;
    }

    /// <summary>
    /// Called by the UI and will start the recording of the player 
    /// </summary>
    public void StartUIRecording()
    {
        if (this.recorder.GetFBTManager().IsFullbodyTrackingActive() == false)
        {
            print("FBT is not enabled and therefore recording is not possible");
            return;
        }
        else if (this.boneHighlighter.GetSelectedBones().Count <= 0)
        {
            print("No bones for recording were selected");
            return;
        }

        this.cState = State.CountdownRecording;
        this.countdown = COUNTDOWN_TIME;
        this.percentageText.text = "";

        this.EnableDisableAllGroupButtons(false);
        this.groupButtons[this.groupButtons.Count - 1].interactable = true;//the last element will always be the stop button
    }

    /// <summary>
    /// Called by the ui and will stop the recording of the player
    /// </summary>
    public void StopUIRecording()
    {
        this.EnableDisableAllGroupButtons(true);
        this.groupButtons[this.groupButtons.Count - 1].interactable = false;//the last element will always be the stop button

        this.cState = State.None;
        this.recorder.StopRecording();
    }

    /// <summary>
    /// Will play the loaded recording (if one is loaded) by switching the state 
    /// </summary>
    public void PlayLoadedRecording()
    {
        if (this.recorder.HasRecordingLoaded() == false)
        {
            return;
        }

        this.cState = State.Playing;
    }

    /// <summary>
    /// Stops the playing of the current recording
    /// </summary>
    public void StopPlaying()
    {
        this.cState = State.None;
    }

    /// <summary>
    /// Is called by the ui and will save the recording by the given name from the input field (not_set will be the name otherwise)
    /// </summary>
    public void SaveUIRecording()
    {
        string recordingName;

        if (this.recordName.text == "")
        {
            recordingName = "not_set";
        }
        else
        {
            recordingName = this.recordName.text;
        }

        this.recordName.text = "";
        this.recorder.SaveCurrentRecording(recordingName);

        this.PopulateUIDropdown();
    }

    /// <summary>
    /// Populates the dropdown menu with all the trainer-recording from the database
    /// </summary>
    public void PopulateUIDropdown()
    {
        List<string> options = new List<string>();
        options = MotionMatchingDBManager.GetRecordingNames();

        this.workoutList.ClearOptions();
        this.workoutList.AddOptions(options);
        this.workoutList.captionText.text = "Select Recording";
        this.workoutList.RefreshShownValue();
    }


    /// <summary>
    /// Enable or disable all buttons the recorder manager is managing (all of the recorder ui elements in the top corner)
    /// </summary>
    /// <param name="en">Boolean for enabling/disabling the buttons</param>
    private void EnableDisableAllGroupButtons(bool en)
    {
        for (int i = 0; i < this.groupButtons.Count; i++)
        {
            this.groupButtons[i].interactable = en;
        }
    }

    /// <summary>
    /// Manages the current state for performing the countdown meaning that it will perform the countdown and after its reaching zero will move to the next state which is the recording state
    /// </summary>
    private void StatePerformCountdown()
    {
        this.countdown -= Time.deltaTime;
        this.percentageText.text = "Countdown: " + (System.Math.Round((this.countdown * 10.0f)) / 10.0f) + "s";

        if (this.countdown <= 0.0f)
        {
            this.percentageText.text = "Recording";
            this.cState = State.Recording;

            this.recorder.StartNewRecording(this.boneHighlighter.GetSelectedBones());
        }
    }

    /// <summary>
    /// Gets the loadedrecording name
    /// </summary>
    /// <returns>Loaded recording name as string</returns>
    public string GetLoadedRecording()
    {
        return this.loadedRecording;
    }

    /// <summary>
    /// Loads the currently selected recording from the ui 
    /// </summary>
    public void LoadSelectedUIRecording()
    {
        this.loadedRecording = this.workoutList.options[this.workoutList.value].text;
        this.recorder.LoadRecording(loadedRecording);

        FBTRecorder.Recording rec = this.recorder.GetLoadedRecording();
        this.boneHighlighter.SetBoneSelection(rec.recordedBones);
    }
}
