using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;


/// <summary>
/// Class that manages the magic-mirror but only from the interaction side
/// </summary>
public class MirrorManager : MonoBehaviour
{
    [Serializable]
    public struct InstructionSection
    {
        public string instructionText;
        public AudioSource instructionClip;
        public GameObject focusObject;
        public TextAnchor textAlignment;
        public bool isInteractive;
        public OVRInput.Button nextStepButton;
    }


    private const float TARGET_Y_SCALE = 0.002f;
    private const float ANIMATION_TIME = 5.0f;
    private int cInstruction = -1;

    private float animationProgress = 0.0f;
    private bool playAnimation = false;
    private bool triggered = false;
    private bool interactiveSessionStarted = false;
    private bool completed = false;

    public List<GameObject> toHide;
    public List<GameObject> toShow;
    public GameObject MainCanvas;
    public GameObject matchingPercentageText;
    public GameObject button;
    public Text instructionText;

    public bool debugTrigger = false;
    public AudioSource turnOnSound;

    public MotionMatchingSystem mSystem;

    public List<InstructionSection> instructions;

    /// <summary>
    /// Checks if the buttons should be already pressed (only for debugging purposes) and to initialize the manager
    /// </summary>
    void Start()
    {
        this.Initialize();

        if (this.debugTrigger == true)
        {
            this.ButtonPressed();
        }
        

        //this.mSystem.Initialize(this);
        this.mSystem.EnableDisableAllGroupButtons(false);
    }

    /// <summary>
    /// Returns if the debug trigger was set in the inspector 
    /// </summary>
    /// <returns>bool of the debug trigger</returns>
    public bool IsDebugTriggered()
    {
        return this.debugTrigger;
    }

    /// <summary>
    /// Initializes the manager by hiding all of the stored object and disablign them 
    /// </summary>
    public void Initialize()
    {
        for (int i = 0; i < this.toHide.Count; i++)
        {
            this.toHide[i].SetActive(false);
        }


        for (int i = 0; i < this.toShow.Count; i++)
        {
            this.toShow[i].SetActive(false);
        }
    }

    /// <summary>
    /// Small easing function for a smother animation (easeInQuart)
    /// Take a look here for more informations https://easings.net/
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private float EaseInCubic(float x)
    {
        return x*x*x;
    }

    /// <summary>
    /// Updates all of the elements etc. here
    /// </summary>
    void Update()
    {

        if(this.playAnimation == true)
        {
            if (this.animationProgress <= 1.0f)
            {
                float asProgess = this.EaseInCubic(this.animationProgress);

                this.animationProgress += Time.deltaTime * ANIMATION_TIME;
                this.MainCanvas.transform.localScale = new Vector3(this.MainCanvas.transform.localScale.x, TARGET_Y_SCALE * asProgess, this.MainCanvas.transform.localScale.z);
            }
            else
            {
                this.MainCanvas.transform.localScale = new Vector3(this.MainCanvas.transform.localScale.x, TARGET_Y_SCALE, this.MainCanvas.transform.localScale.z);
                this.playAnimation = false;
            }
        }
        else if(this.triggered == true)
        {
            this.PlayInstructions();
        }

        


    }

    /// <summary>
    /// Called by the ui when a button was pressed (to go further in the instructions)
    /// </summary>
    public void ButtonPressed()
    {
        if(this.triggered == true)
        {
            return;
        }


        this.button.SetActive(false);
        this.triggered = true;
        this.turnOnSound.Play();

        this.toShow[0].SetActive(true);

        this.MainCanvas.transform.localScale = new Vector3(this.MainCanvas.transform.localScale.x, 0.0f, this.MainCanvas.transform.localScale.z);
        this.playAnimation = true;

        this.matchingPercentageText.SetActive(false);
        this.PlayNextInstruction();
    }

    /// <summary>
    /// Called by the ui when a workout was loaded (to go further in the instructions)
    /// </summary>
    public void WorkoutLoaded()
    {

        if(this.completed == true)
        {
            return ;
        }

        this.matchingPercentageText.SetActive(false);
        this.PlayNextInstruction();
    }

    /// <summary>
    /// Called by the ui when the preivew will be played (to hide the instruction text etc.)
    /// </summary>
    public void PlayPreview()
    {
        if(this.completed == true)
        {
            return ;
        }

        this.instructionText.enabled = false;
        this.matchingPercentageText.SetActive(true);
    }

    /// <summary>
    /// Called by the ui when the workout mode is clicked on (to hide the instrucion text etc.)
    /// </summary>
    public void StartWorkout()
    {
        if(this.completed == true)
        {
            return ;
        }

        this.instructionText.enabled = false;
        this.matchingPercentageText.SetActive(true);
    }

    /// <summary>
    /// Called by the ui when the instruction mode is clicked on (to hide the instrucion text etc.)
    /// </summary>
    public void StartInstruction()
    {
        if(this.completed == true)
        {
            return ;
        }

        this.instructionText.enabled = false;
        this.matchingPercentageText.SetActive(true);
    }

    /// <summary>
    /// Called by the ui when the workout mode will be stoped (to showthe instrucion text etc.)
    /// </summary>
    public void StopWorkout()
    {
        if(this.completed == true)
        {
            return ;
        }

        this.matchingPercentageText.SetActive(false);
        this.instructionText.enabled = true;
        this.PlayNextInstruction();
    }

    /// <summary>
    /// Plays the next instruction from the internal array of this class
    /// </summary>
    void PlayNextInstruction()
    {
        if(this.completed == true)
        {
            return ;
        }

        if (this.cInstruction < this.instructions.Count - 1)
        {
            //Stop logic
            if (this.cInstruction >= 0)
            {
                this.instructions[this.cInstruction].instructionClip.Stop();

                if (this.instructions[this.cInstruction].focusObject != null)
                {
                    this.instructions[this.cInstruction].focusObject.SetActive(false);
                }
            }

            //Play logic
            this.cInstruction += 1;
            this.instructions[this.cInstruction].instructionClip.Play();
            this.instructionText.text = this.instructions[this.cInstruction].instructionText;
            this.instructionText.alignment = this.instructions[this.cInstruction].textAlignment;

            if (this.instructions[this.cInstruction].focusObject != null)
            {
                this.instructions[this.cInstruction].focusObject.SetActive(true);
            }

        }
    }

    /// <summary>
    /// Plays the current instruction (not increasing the instruction counter)
    /// </summary>
    void PlayInstructions()
    { 
        if(this.completed == true)
        {
            return ;
        }


        if (this.instructions.Count > 0 && this.debugTrigger == false)
        {

            if (this.debugTrigger == true || (this.cInstruction >= this.instructions.Count - 1 && this.instructions[this.instructions.Count - 1].instructionClip.isPlaying == false))
            {
                if (this.instructions[this.instructions.Count-1].focusObject != null)
                {
                    this.instructions[this.instructions.Count - 1].focusObject.SetActive(false);
                }

                for(int i = 0; i < this.toShow.Count; i++)
                {
                    this.toShow[i].SetActive(true);
                }

                this.matchingPercentageText.SetActive(true);
                this.instructionText.enabled = false;
                this.enabled = false;
                this.completed = true;

                return;
            }

            if (this.cInstruction >= 0 && this.cInstruction < this.instructions.Count)
            {
                if(this.instructions[this.cInstruction].isInteractive == true)
                {
                    if(this.interactiveSessionStarted == false)
                    {
                        this.instructionText.gameObject.SetActive(true);
                        this.mSystem.InstructionsComplete();

                        for (int i = 0; i < this.toShow.Count; i++)
                        {
                            this.toShow[i].SetActive(true);
                        }


                        this.matchingPercentageText.SetActive(false);
                        this.interactiveSessionStarted = true;
                    }


                    return;
                }

                if (OVRInput.GetDown(this.instructions[this.cInstruction].nextStepButton) )
                {
                    PlayNextInstruction();
                }

            }

            
        }
        else if(this.debugTrigger == true)
        {
            for(int i = 0; i < this.toShow.Count; i++)
            {
                this.toShow[i].SetActive(true);
            }

            this.matchingPercentageText.SetActive(true);
            this.instructionText.enabled = false;
            this.enabled = false;
            this.completed = true;

            this.mSystem.EnableDisableAllGroupButtons(true);
        }

       
    }
}
