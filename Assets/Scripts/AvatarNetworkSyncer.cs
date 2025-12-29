using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Photon.Pun;

/// <summary>
/// Class that sync the network synchronization with the individual bones over
/// the network
/// </summary>
public class AvatarNetworkSyncer : MonoBehaviour, IPunObservable {

  const float SERVER_TICK_RATE =
      1.0f / 60.0f; // note this is just a assumption, probably it is wrong

  public Transform main_avatar;
  public List<Transform> to_sync;

  public BoneInterpolationManager interpolator;
  private List<Quaternion> pose_to_send = new List<Quaternion>();

  /// <summary>
  /// Serialize the bones data so that we can send them through the photon
  /// server
  /// </summary>
  /// <param name="stream">PhotonStream </param>
  /// <param name="info">PhotonMessageInfo with additional information about the
  /// current state etc.</param>
  public void OnPhotonSerializeView(PhotonStream stream,
                                    PhotonMessageInfo info) {
    if (stream.IsWriting == true) {
      if (this.pose_to_send.Count > 0) {
        stream.SendNext(this.main_avatar.position);

        foreach (Quaternion rot in this.pose_to_send) {
          stream.SendNext(rot);
        }
      }
    } else if (stream.IsReading == true) {
      this.main_avatar.position = (Vector3)stream.ReceiveNext();

      foreach (Transform t in to_sync) {
        Quaternion rotation = (Quaternion)stream.ReceiveNext();
        t.rotation = rotation;
      }
    }
  }

  /// <summary>
  /// Updates the bones by storing them in a list to transmit them in the next
  /// photon call
  /// </summary>
  void LateUpdate() {
    this.pose_to_send.Clear();

    foreach (Transform t in to_sync) {
      this.pose_to_send.Add(t.rotation);
    }
  }
}
