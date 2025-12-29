using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon;

public class Cube : MonoBehaviour {
  private float movementSpeed = 0.0f;

  // will be called by the spawner script
  public void Initialize(float speed) { this.movementSpeed = speed; }

  // Update is called once per frame
  void Update() {
    // Do not run the script if the game is paused
    if (PauseMenu.GameIsPaused == true) {
      return;
    }

    // Making the spawned cube to move in the forward direction
    transform.position +=
        movementSpeed * Time.deltaTime * (transform.forward * -1);
    Debug.Log("Speed is " + movementSpeed);
  }
}
