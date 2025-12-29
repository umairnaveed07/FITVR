using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Class that manages the networkavatar manager
/// </summary>
public class NetworkAvatarManager : MonoBehaviour {
  public GameObject sportman;

  public Transform avatarHead;

  private Transform headRig;
  private PhotonView photonView;
  private bool usesFullbodyTracking = false;

  private FullBodyTrackingManager fullbodytrackingController;
  private PopupManager popupManager;

  /// <summary>
  /// Initialize the networkavatar manager
  /// </summary>
  void Start() {
    this.fullbodytrackingController =
        this.GetComponentInChildren<FullBodyTrackingManager>();
    this.popupManager = this.GetComponentInChildren<PopupManager>();

    photonView = GetComponent<PhotonView>();

    OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
    headRig = rig.transform.Find("TrackingSpace/CenterEyeAnchor");

    if (this.IsClientCharacter() == false) {
      Object.Destroy(this.fullbodytrackingController.gameObject);
      Object.Destroy(this.popupManager.gameObject);

      this.sportman.GetComponent<RigBuilder>().enabled = false;
      this.sportman.GetComponent<VRRig>().enabled = false;
      this.sportman.GetComponent<VRFootIK>().enabled = false;
      this.sportman.GetComponent<VRAnimatorController>().enabled = false;
      this.sportman.GetComponent<Animator>().enabled = false;
    } else {
      this.gameObject.tag = "LocalPlayer";
    }
  }

  /// <summary>
  /// Enables the Fullbody tracking
  /// </summary>
  public void StartFullBodyTracking() {
    if (this.IsClientCharacter() == false) {
      return;
    }

    print("using fbt");
    this.usesFullbodyTracking = true;

    this.sportman.GetComponent<RigBuilder>().enabled = false;
    this.sportman.GetComponent<VRRig>().enabled = false;

    this.avatarHead.localScale = new Vector3(0.0f, 0.0f, 0.0f);
    this.popupManager.ShowPopup("Fullbody tracking is enabled");
  }

  /// <summary>
  /// Stops the fullbody tracking
  /// </summary>
  public void StopFullBodyTracking() {
    if (this.IsClientCharacter() == false) {
      return;
    }

    print("stopping fbt");

    this.usesFullbodyTracking = false;

    this.sportman.GetComponent<RigBuilder>().enabled = true;
    this.sportman.GetComponent<VRRig>().enabled = true;

    this.avatarHead.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    this.popupManager.ShowPopup("Fullbody tracking is disabled");
  }

  /// <summary>
  /// Returns if FBT is ued
  /// </summary>
  /// <returns>bool if FBT is used</returns>
  public bool UsesFullBodyTracking() { return this.usesFullbodyTracking; }

  /// <summary>
  /// Returns the avatar model as an Unity GameObject
  /// </summary>
  /// <returns>GameObject</returns>
  public GameObject GetAvatarObject() { return this.sportman; }

  /// <summary>
  /// Returns the main camera
  /// </summary>
  /// <returns>Transform of the main camera</returns>
  public Transform GetMainCamera() { return this.headRig; }

  /// <summary>
  /// Returns if this avatar is the local player or not
  /// </summary>
  /// <returns>bool if this is the local player</returns>
  public bool IsClientCharacter() { return this.photonView.IsMine; }
}
