using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {
  public GameObject portal;
  public GameObject NetworkSportsmanAvatar;
  public GameObject TaskCanvas;
  private bool activated = true;

  private void OnTriggerEnter(Collider other) {

    Debug.Log(" Entering the portal");
    Debug.Log("Avatar is " + other.tag);
    // if (other.tag=="NetworkSportsmanAvatar")
    //{
    portal.SetActive(true);
    // if(!activated){
    activated = true;
    // NetworkSportsmanAvatar.SetActive(true);
    TaskCanvas.SetActive(true);
    //}

    //}
  }

  private void OnTriggerExit(Collider other) {
    Debug.Log(" I am exiting");
    // if(other.tag=="NetworkSportsmanAvatar")
    //{
    portal.SetActive(false);
    TaskCanvas.SetActive(false);

    //}
  }
  // Start is called before the first frame update
  void Start() {}

  // Update is called once per frame
  void Update() {}
}
