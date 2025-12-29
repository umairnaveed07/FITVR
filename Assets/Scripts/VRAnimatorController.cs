using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the animator by passing the animation like the walking speed to the
/// animator
/// </summary>
public class VRAnimatorController : MonoBehaviour {
  [Range(0, 1)]
  public float smoothing = 1.0f;

  public float speedThreshold = 0.1f;
  public AudioSource walkingAudio;
  private Animator animator;
  private Vector3 previousPos;
  private VRRig vrRig;
  private NetworkAvatarManager avatarManager;

  /// <summary>
  /// Initialize the class
  /// </summary>
  void Start() {
    animator = GetComponent<Animator>();
    vrRig = GetComponent<VRRig>();

    this.avatarManager = GetComponentInParent<NetworkAvatarManager>();
    this.previousPos = new Vector3(0.0f, 0.0f, 0.0f);
  }

  /// <summary>
  /// Updates all of the informations for the animation to define in which
  /// direction the legs should face etc. while walking
  /// </summary>
  void Update() {
    bool is_moving = false;

    // Speed of the headset
    Vector3 headsetSpeed =
        (this.avatarManager.GetMainCamera().position - previousPos) /
        Time.deltaTime;
    headsetSpeed.y = 0;

    // Local speed
    Vector3 headsetLocalSpeed =
        transform.InverseTransformDirection(headsetSpeed);
    previousPos = this.avatarManager.GetMainCamera().position;

    if (headsetLocalSpeed.magnitude > speedThreshold) {
      is_moving = true;
    }

    if (this.avatarManager.UsesFullBodyTracking() == false) {
      // Animator values
      float previousDirectionX = animator.GetFloat("DirectionX"); // to smooth
      float previousDirectionY = animator.GetFloat("DirectionY");

      animator.SetBool(
          "isMoving", is_moving); // if the speed exceeds this value, then walk.
      animator.SetFloat("DirectionX",
                        Mathf.Lerp(previousDirectionX,
                                   Mathf.Clamp(headsetLocalSpeed.x, -1, 1),
                                   smoothing));
      animator.SetFloat("DirectionY",
                        Mathf.Lerp(previousDirectionY,
                                   Mathf.Clamp(headsetLocalSpeed.z, -1, 1),
                                   smoothing));
    } else {
      Vector2 moveDirection2d = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
      Vector3 camForward =
          this.avatarManager.GetMainCamera().rotation * Vector3.forward;
      Vector3 camSideward =
          this.avatarManager.GetMainCamera().rotation * Vector3.right;
      Vector3 desiredDir =
          camForward * moveDirection2d.y + camSideward * moveDirection2d.x;
      desiredDir.y = 0.0f;
      desiredDir = desiredDir.normalized;

      animator.SetBool("isMoving", moveDirection2d.magnitude > speedThreshold);
      animator.SetFloat("DirectionX", desiredDir.x);
      animator.SetFloat("DirectionY", desiredDir.z);
    }

    if (is_moving == true) {
      if (!walkingAudio.isPlaying) {
        walkingAudio.Play();
      }
    } else {
      walkingAudio.Pause();
    }
  }
}
