using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wackamole : MonoBehaviour {

  private bool activated = true;

  public GameObject bonus1;

  private void OnTriggerEnter(Collider other) {
    if (other.tag != "foots") {
      return;
    }
    Debug.Log(" Hit the health stomp");
    // Debug.Log("Avatar is "+ other.tag);
    ScoreManager.instance.scoreAddMole50();
    bonus1.SetActive(false);
  }

  private void OnTriggerExit(Collider other) {
    Debug.Log(" exit the health stomp");
  }
  // Start is called before the first frame update
  void Start() {}

  // Update is called once per frame
  void Update() {}
}
