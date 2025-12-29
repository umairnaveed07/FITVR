using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that will play an audio if an object with the desired tag hits the trigger
/// </summary>
public class AudioTrigger : MonoBehaviour
{
    public AudioSource toPlay;
    public string searchTag = "LocalRig";
    private bool wasTriggered = false;

    /// <summary>
    /// Checks the passed collider for a tag match and will play the audio if the tag is matching 
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (this.wasTriggered == true)
        {
            return ;
        }

        if (other.gameObject.tag == this.searchTag)
        {
            this.toPlay.Play();
            this.wasTriggered = true;
        }

    }
}
