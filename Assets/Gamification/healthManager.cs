using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class healthManager : MonoBehaviour {
  public static healthManager instance;
  public Text healthText;
  public static int health = 100;

  // Start is called before the first frame update
  void Start() {}

  // Update is called once per frame
  void Update() { healthText.text = "Health: " + health.ToString() + "%"; }

  IEnumerator ExampleCoroutine() {
    // Print the time of when the function is first called.
    Debug.Log("Started Coroutine at timestamp : " + Time.time);

    // yield on a new YieldInstruction that waits for 5 seconds.
    yield return new WaitForSeconds(10);
    // Pause();
    // After we have waited 5 seconds print the time again.
    Debug.Log("Finished Coroutine at timestamp : " + Time.time);
  }

  // This function removes the health from the player
  public static void healthRemove() {

    if (health <= 0) {
      health = 0;

    } else {
      health = health - 10;
    }
  }

  // This function adds health to players health
  public static void healthAdd() {
    Debug.Log("Showing health add text");

    if (health >= 100) {
      health = 100;
    }

    else {
      health = health + 10;
    }
  }

  // This function adds health if player stomps on the health circle in whack a
  // mole game
  public static void healthAddMole() {
    Debug.Log("Showing health add text");

    if (health >= 100) {
      health = 100;
    }

    else {
      health = health + 20;
    }
  }
}
