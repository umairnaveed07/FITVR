using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that manages the inverse kinematics for the foots
/// </summary>
public class VRFootIK : MonoBehaviour {
  private Animator animator;
  public Vector3 footOffset; // To not drop below ground.
  [Range(0, 1)]
  public float rightFootPosWeight = 1;
  [Range(0, 1)]
  public float rightFootRotWeight = 1;
  [Range(0, 1)]
  public float leftFootPosWeight = 1;
  [Range(0, 1)]
  public float leftFootRotWeight = 1;

  /// <summary>
  /// Initialize the class by getting the animation
  /// </summary>
  void Start() { this.animator = GetComponent<Animator>(); }

  /// <summary>
  /// Overrites the IK based on the layer to define if ik should be applied if
  /// we are on the ground or not (weight will be set to zero if not grounded)
  /// </summary>
  /// <param name="layerIndex"></param>
  private void OnAnimatorIK(int layerIndex) {
    int layer_mask = LayerMask.GetMask("Ground");

    // Right Foot.
    Vector3 rightFootPos = animator.GetIKPosition(AvatarIKGoal.RightFoot);
    RaycastHit hit;

    bool hasHit = Physics.Raycast(rightFootPos + Vector3.up, Vector3.down,
                                  out hit, Mathf.Infinity, layer_mask);
    if (hasHit) // Ground.
    {
      animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootPosWeight);
      animator.SetIKPosition(AvatarIKGoal.RightFoot, hit.point + footOffset);

      Quaternion footRotation = Quaternion.LookRotation(
          Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
      animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootRotWeight);
      animator.SetIKRotation(AvatarIKGoal.RightFoot, footRotation);
    } else // No ground.
    {
      animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
    }

    // Left Foot.
    Vector3 leftFootPos = animator.GetIKPosition(AvatarIKGoal.LeftFoot);

    hasHit = Physics.Raycast(leftFootPos + Vector3.up, Vector3.down, out hit,
                             Mathf.Infinity, layer_mask);
    if (hasHit) // Ground.
    {
      animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootPosWeight);
      animator.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point + footOffset);

      Quaternion leftFootRotation = Quaternion.LookRotation(
          Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
      animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootRotWeight);
      animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRotation);
    } else // No ground.
    {
      animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
    }
  }
}
