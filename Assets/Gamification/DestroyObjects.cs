using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class DestroyObjects : MonoBehaviour {
  public GameObject FloatingText;
  public Transform points;
  public GameObject FloatingTextSpawnPoint;

  // Update is called once per frame

  void Start() {}

  void Update() {}

  // function for the  showing the text in game when object is missed.
  void ShowFloatingText() {
    GameObject cube = Instantiate(FloatingText);
    cube.transform.position = FloatingTextSpawnPoint.transform.position;
    Debug.Log("Float txt miss running");
    // var go1 = Instantiate(FloatingText, transform.position,
    // Quaternion.identity, transform);
    cube.GetComponent<TextMeshPro>().text = "MISS";
    Debug.Log("Float txt end is running");
  }

  // Function for collision detection
  private void OnCollisionEnter(Collision col) {
    // var dodge = transform.FindWithTag("Dodge");
    if (col.gameObject.tag == "Wall") {

      PhotonNetwork.Destroy(gameObject);
      Debug.Log("Cube was missed this time");
      if (col.gameObject.tag == "Healthcube") {

        Debug.Log("HealthCube so no score deduction required");
      } else {
        Debug.Log("Going inside floating prefab cube loop");
        // ScoreManager.instance.RemovePoint();
        Debug.Log("Score deducted -10");
        ScoreManager.instance.StreakReset();
      }
    }
  }
}
