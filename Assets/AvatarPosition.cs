using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarPosition : MonoBehaviour {

  public GameObject ExerGamesAvatar;
  public GameObject Spawner2;

  private void OnTriggerEnter(Collider other) {

    Debug.Log(" Entering the circle");
    Debug.Log("Avatar is " + other.tag);
    GameObject.Find("Spawner2").GetComponent<GridManager>().enabled = true;
  }

  private void OnTriggerExit(Collider other) {
    Debug.Log(" I am exiting");
    GameObject.Find("Spawner2").GetComponent<GridManager>().enabled = false;
  }
  // Start is called before the first frame update
  void Start() {}

  // Update is called once per frame
  void Update() {}
}
