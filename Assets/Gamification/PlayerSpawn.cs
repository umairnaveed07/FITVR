using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour {
  // Start is called before the first frame update
  public static PlayerSpawn PS;
  public Transform[] spawnPoints;

  private void onEnable() {
    if (PlayerSpawn.PS == null) {
      PlayerSpawn.PS = this;
    }
  }
}