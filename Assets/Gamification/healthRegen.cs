using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class healthRegen : MonoBehaviour {
  // Start is called before the first frame update
  void Start() {}

  // Update is called once per frame
  void Update() {}

  private void OnCollisionEnter(Collision col) {

    if (col.gameObject.tag == "LocalPlayer") {
      Debug.Log("health removed");
      healthManager.healthRemove();
      PhotonNetwork.Destroy(gameObject);
    }
  }
}
