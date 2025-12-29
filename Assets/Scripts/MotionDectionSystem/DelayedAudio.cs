using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Small helper class to play the initial instruction audio a bit delayed(can also be used for other audios)
/// </summary>
public class DelayedAudio : MonoBehaviour
{

    public float audioDelay = 1.0f;
    public AudioSource clip;
    public MirrorManager manager;

    private bool triggered = false;


    /// <summary>
    /// Updates the timer and plays the audio if it reaches zero
    /// </summary>
    void Update()
    {

        if(this.manager.IsDebugTriggered() == true)
        {
            this.enabled = false;
        }

        this.audioDelay -= Time.deltaTime;

        if(this.audioDelay <= 0 && this.triggered == false)
        {
            clip.Play();
            this.triggered = true;
        }
    }
}
