using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Helper class to define the applyment data in the inspector
/// </summary>
[System.Serializable]
public class VRMap {
  private Transform vrTarget;
  public Transform rigTarget;
  public Vector3 trackingPositionOffset;
  public Vector3 trackingRotationOffset;

  /// <summary>
  /// Maps the target position & rotation data to the rig
  /// </summary>
  public void Map() {
    rigTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
    rigTarget.rotation =
        vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
  }

  /// <summary>
  /// Sets the target rig
  /// </summary>
  /// <param name="target"></param>
  public void setVRTarget(Transform target) { this.vrTarget = target; }
}

/// <summary>
/// Class that sets the postiion data etc. for the avatar (if FBT is not used)
/// and also for networking
/// </summary>
public class VRRig : MonoBehaviour {
  public float turnSmoothness;

  public VRMap head;
  public VRMap leftHand;
  public VRMap rightHand;

  public Transform headConstraint;
  public Vector3 headBodyOffset;

  /// <summary>
  /// Initialize the class at startup
  /// </summary>
  void Start() {

    OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
    Transform headRig = rig.transform.Find("TrackingSpace/CenterEyeAnchor");
    Transform leftHandRig =
        rig.transform.Find("TrackingSpace/LeftHandAnchor/LeftControllerAnchor");
    Transform rightHandRig = rig.transform.Find(
        "TrackingSpace/RightHandAnchor/RightControllerAnchor");

    this.head.setVRTarget(headRig);
    this.leftHand.setVRTarget(leftHandRig);
    this.rightHand.setVRTarget(rightHandRig);
  }

  /// <summary>
  /// updates the stored avatar maps
  /// </summary>
  void FixedUpdate() {
    transform.position = headConstraint.position + headBodyOffset;
    transform.forward = Vector3.Lerp(
        transform.forward,
        Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized,
        Time.deltaTime * turnSmoothness);
    head.Map();
    leftHand.Map();
    rightHand.Map();
  }
}
