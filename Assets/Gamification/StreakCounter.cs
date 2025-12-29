using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StreakCounter : MonoBehaviour {
  int streakcount = 0;
  public static StreakCounter instance1;
  public GameObject FloatingText;
  public GameObject FloatingTextSpawnPoint;

  public void Awake() { instance1 = this; }

  public void CountStreak() {

    streakcount++;
    Debug.Log("Value of streakcount is  " + streakcount);
    if (streakcount == 6) {
      Debug.Log("Value of streakcount is here in if loop " + streakcount);
      if (FloatingText) {
        ShowFloatingText3();
      }
    } else {
      Debug.Log("Value of streakcount inside else loop is  " + streakcount);
    }
  }
  void ShowFloatingText3() {
    GameObject cube = Instantiate(FloatingText);
    cube.transform.position = FloatingTextSpawnPoint.transform.position;
    Debug.Log("Float txt for 3 successful score inside new script running");
    cube.GetComponent<TextMeshPro>().text = "+ 3";
  }
}
