using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Class that manages the connection to the photon server
/// </summary>
public class ConnectionManager : MonoBehaviourPunCallbacks {
  public bool hideAvatar = false;
  public bool offlineMode = false;
  public string roomName = "vr-fitness";

  public List<NetworkStart> callbacks;
  public List<Transform> spawnPositions;

  private GameObject localPlayer;
  public string Avatar = "ExerGamesAvatar";

  /// <summary>
  /// Initialize the class
  /// </summary>
  void Start() {
    if (this.offlineMode == false) {
      ConnectToServer();
    } else {
      PhotonNetwork.OfflineMode = true;
      Debug.Log("Using offline mode (emulate connections only)");
    }
  }

  /// <summary>
  /// Connects to the photon server
  /// </summary>
  void ConnectToServer() {
    PhotonNetwork.ConnectUsingSettings();
    Debug.Log("Connecting to server...");
  }

  /// <summary>
  /// Connects to the photon server with overritten settings
  /// </summary>
  public override void OnConnectedToMaster() {
    Debug.Log("Connected to server: " + PhotonNetwork.OfflineMode);
    base.OnConnectedToMaster();

    RoomOptions roomOptions = new RoomOptions();
    roomOptions.MaxPlayers = 20;
    roomOptions.IsVisible = true;
    roomOptions.IsOpen = true;
    PhotonNetwork.JoinOrCreateRoom(this.roomName, roomOptions,
                                   TypedLobby.Default);
  }

  /// <summary>
  /// Spawn the local player after we joined a room. Note that onjoinedroom is
  /// only for the local player
  /// </summary>
  public override void OnJoinedRoom() {
    Debug.Log("Locally joined room");
    int playerCount = PhotonNetwork.CurrentRoom.PlayerCount - 1;

    if (playerCount >= 0 && playerCount < this.spawnPositions.Count) {
      OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
      if (rig != null) {
        rig.transform.parent.position =
            this.spawnPositions[playerCount].position;
        rig.transform.parent.rotation =
            this.spawnPositions[playerCount].rotation;
      }
    }

    base.OnJoinedRoom();
    this.localPlayer =
        PhotonNetwork.Instantiate(this.Avatar, new Vector3(0.0f, 0.0f, 0.0f),
                                  new Quaternion(0.0f, 0.0f, 0.0f, 0.0f));

    foreach (NetworkStart obj in callbacks) {
      obj.OnNetworkConnectionInitilaized(this.localPlayer);
    }

    this.localPlayer.SetActive(!this.hideAvatar);
  }

  /// <summary>
  /// Removed the local player from the server after we left the room
  /// </summary>
  public override void OnLeftRoom() {
    Debug.Log("Localy left room");

    base.OnLeftRoom();
    PhotonNetwork.Destroy(this.localPlayer);
  }
}
