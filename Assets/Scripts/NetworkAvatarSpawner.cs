using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkAvatarSpawner : MonoBehaviourPunCallbacks {
  private GameObject spawnedPlayerPrefab;

  public override void OnJoinedRoom() {
    base.OnJoinedRoom();
    spawnedPlayerPrefab = PhotonNetwork.Instantiate(
        "ExerGamesAvatar", new Vector3(0.0f, 0.0f, 0.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 0.0f));
  }

  public override void OnLeftRoom() {
    base.OnLeftRoom();
    PhotonNetwork.Destroy(spawnedPlayerPrefab);
  }
}
