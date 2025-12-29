using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wackamolefrenzy : MonoBehaviour {
  private bool activated = true;

  public GameObject bonus2;
  public GameObject Spawner;
  public GameObject Spawner2;

  private void OnTriggerEnter(Collider other) {
    Debug.Log(" Hit the health stomp");
    // Debug.Log("Avatar is "+ other.tag);

    bonus2.SetActive(false);
    GameObject.Find("Spawner").GetComponent<Spawnerr>().enabled = false;
    GameObject.Find("Spawner2").GetComponent<GridManager>().enabled = true;
  }

  private void OnTriggerExit(Collider other) {
    Debug.Log(" exit the health stomp");
  }

  // Start is called before the first frame update
  void Start() {}

  // Update is called once per frame
  void Update() {}
}
