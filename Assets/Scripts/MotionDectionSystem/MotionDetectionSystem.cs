using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionDetectionSystem : MonoBehaviour
{
    public bool usedForUserStuy = true;
    public MirrorManager activationButton;


    /// <summary>
    /// Checks if the Virtual Trainer/MotionDetectionSystem will be used for the user study or not. If not it will disabled the activation button and therefore the application
    /// </summary>
    void Start()
    {
        if(this.usedForUserStuy == false)
        {
            this.activationButton.Initialize();
            this.activationButton.enabled = false;
        }

    }

}
