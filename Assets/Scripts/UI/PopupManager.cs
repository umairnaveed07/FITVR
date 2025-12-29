using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class that manages popup(FBT popups for now)
/// </summary>
public class PopupManager : MonoBehaviour
{
    private const float POPUP_TIME = 5.0f;

    private float timeLeft = 0.0f;
    private Text popupMessage;
    private Canvas mainCanvas;


    /// <summary>
    /// Initialize the class by get all the components from its children etc.
    /// </summary>
    void Start()
    {
        this.mainCanvas = this.GetComponent<Canvas>();
        this.popupMessage = this.GetComponentInChildren<Text>();

        this.mainCanvas.enabled = false;
    }

    /// <summary>
    /// Updates the popups by reducing the timer and hides the popup if it is reaching zero
    /// </summary>
    void Update()
    {
        this.timeLeft -= Time.deltaTime;

        if (this.timeLeft <= 0.0f)
        {
            this.mainCanvas.enabled = false;
        }
        else
        {
            this.mainCanvas.enabled = true;
        }
    }

    /// <summary>
    /// Shows the given popup message to the user 
    /// </summary>
    /// <param name="text">string with the text that should be displayed</param>
    /// <param name="display_time">float of the duration time how long the popup should be visible</param>
    public void ShowPopup(string text, float displayTime = POPUP_TIME)
    {
        this.timeLeft = displayTime;
        this.popupMessage.text = text;
    }
}
