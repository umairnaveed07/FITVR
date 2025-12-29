using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class destroydodge : MonoBehaviour {
  public GameObject FloatingText;
  public GameObject FloatingText1;
  public GameObject FloatingTextSpawnPoint;
  int countdodge = 0;
  bool flag = false;

  private void OnCollisionEnter(Collision col) {

    print("test1:" + col.gameObject.tag);

    if (col.gameObject.tag == "Wall") {
      flag = true;
      PhotonNetwork.Destroy(gameObject);
      Debug.Log("Dodge hits the wall!!!! So add point ");
      Debug.Log("+10 ");
      ScoreManager.instance.AddPointDodge();
      // ScoreManager.instance.CountStreak();
      // countdodge++;

      if (FloatingText) {
        ShowFloatingText2();
        Debug.Log("Going inside floating prefab loop in Dodge loop");
      }
      // StreakCounter.instance1.CountStreak();
    }

    /*Debug.Log("Dodge hits the wall!!!! Count is now " + countdodge);
    if (countdodge == 2)
    {
        ShowFloatingText3();
        Debug.Log("3 objects dodged");
    }
    */
  }

  private void OnTriggerEnter(Collider other) {

    print("test2: " + other.tag);

    if (other.tag != "LocalPlayer") {
      return;
    }

    Debug.Log("After Hitting ");
    Object.Destroy(other);
    // DestroyObject(other);
    Debug.Log("Dodge hits the player!!!! So remove point ");
    Debug.Log("-10 ");
    ScoreManager.instance.RemovePointDodge();
    // ScoreManager.instance.CountStreak();
    // countdodge++;

    if (FloatingText) {
      ShowFloatingText2();
      Debug.Log("Going inside floating prefab loop in Dodge loop");
    }
    // StreakCounter.instance1.CountStreak();
  }

  void ShowFloatingText2() {
    GameObject cube = Instantiate(FloatingText);
    cube.transform.position = FloatingTextSpawnPoint.transform.position;
    Debug.Log("Float txt for dodge running");
    cube.GetComponent<TextMeshPro>().text = "NICE";
  }

  /*void ShowFloatingText3()
  {
      GameObject cube = Instantiate(FloatingText1);
      cube.transform.position = FloatingTextSpawnPoint.transform.position;
      Debug.Log("Float txt for 3 successful dodge running");
      cube.GetComponent<TextMeshPro>().text = "+3";
  }
  */
}
