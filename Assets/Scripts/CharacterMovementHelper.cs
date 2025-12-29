using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CharacterMovementHelper : MonoBehaviour {
  private XRRig XRRig;
  private CharacterController CharacterController;
  private CharacterControllerDriver driver;

  // Start is called before the first frame update
  void Start() {
    XRRig = GetComponent<XRRig>();
    CharacterController = GetComponent<CharacterController>();
    driver = GetComponent<CharacterControllerDriver>();
  }

  // Update is called once per frame
  void Update() { UpdateCharacterController(); }

  /// <summary>
  /// Update the <see cref="CharacterController.height"/> and <see
  /// cref="CharacterController.center"/> based on the camera's position.
  /// </summary>
  protected virtual void UpdateCharacterController() {
    if (XRRig == null || CharacterController == null)
      return;

    var height = Mathf.Clamp(XRRig.cameraInRigSpaceHeight, driver.minHeight,
                             driver.maxHeight);

    Vector3 center = XRRig.cameraInRigSpacePos;
    center.y = height / 2f + CharacterController.skinWidth;

    CharacterController.height = height;
    CharacterController.center = center;
  }
}
