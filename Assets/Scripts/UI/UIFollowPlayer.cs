using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

/// <summary>
/// Class that manages that an UI follow the camera / player
/// </summary>
public class UIFollowPlayer : MonoBehaviour
{
    private Transform mainCamera;
    public Vector3 offset = new Vector3(0.0f, -0.5f, 0.0f);

    public float uiDistance = 4.0f;
    public float followspeed = 3.0f;


    /// <summary>
    /// Initialize the class by finding the ovr rig in the scene
    /// </summary>
    void Start()
    {
        OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
        this.mainCamera = rig.transform.Find("TrackingSpace/CenterEyeAnchor");
    }


    /// <summary>
    /// Updates the ui position by setting its position on the current looking direction of the camera
    /// </summary>
    void Update()
    {
        Quaternion cameraRot = Quaternion.Euler(0.0f, this.mainCamera.eulerAngles.y , 0.0f);
        Vector3 targetPosition = this.mainCamera.position + this.mainCamera.forward * this.uiDistance + this.offset;

        this.transform.rotation = cameraRot;
        this.transform.position = targetPosition;//Vector3.Lerp(this.transform.position, targetPosition, this.followspeed * Time.deltaTime);
    }
}
