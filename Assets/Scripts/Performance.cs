using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Performance : MonoBehaviour {
  public Text reps;
  public Text sets;

  public float timeValue = 60;
  public Text timerText;
  static bool isRest;
  static bool timerSt;
  public static int no_of_sets;

  public GameObject rest;
  public AudioSource timerAudio;

  private void Update() {
    if (!isRest && !timerAudio.isPlaying) {      // If User is not Resting
      if (OVRInput.GetDown(OVRInput.Button.One)) // Incrementing Reps on
                                                 // Controller Button Press
        Perform();
      timerSt = false;
      rest.SetActive(false);
    } else { //  Runs Pause Timer
      rest.SetActive(true);
      if (!timerAudio.isPlaying) {
        timerAudio.Play();
        timerSt = true;
      }
      if (timeValue > -1) {
        timeValue -= Time.deltaTime;
        DisplayTime(timeValue);
      } else {
        timerAudio.Stop();
        timeValue = 60;
        isRest = false;
        rest.SetActive(false);
      }
    }
  }

  // Displays Rest Time
  void DisplayTime(float timeToDisplay) {
    if (timeToDisplay < 0) {
      timeToDisplay = 0;
    }

    float minutes = Mathf.FloorToInt(timeToDisplay / 60);
    float seconds = Mathf.FloorToInt(timeToDisplay % 60);

    timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
  }

  // Increses Reps and Sets on screen
  public void Perform() {
    if (int.Parse(reps.text) == 9) {
      reps.text = "0";
      sets.text = (int.Parse(sets.text) + 1).ToString();
    } else {
      reps.text = (int.Parse(reps.text) + 1).ToString();
    }
    no_of_sets = int.Parse(sets.text);
    print("the value of sets is ===" + no_of_sets);
  }

  // Resets Sets and Reps on Screen
  public void ResetTrainingData() {
    reps.text = "0";
    sets.text = "0";
    if (rest.activeSelf) {
      timerAudio.Stop();
      timeValue = 120;
      isRest = false;
      rest.SetActive(false);
    }
  }

  // Returns Number of Sets
  public int getSets() { return no_of_sets; }

  // Returns if Resting
  public bool isResting() { return isRest; }

  // Returns if Timer is Running
  public bool timerStatus() { return timerSt; }

  // Initiates Pause
  public void initiatePause(bool status) {
    Debug.Log("Initiate pause entered Rest");
    isRest = status;
    Debug.Log("is rest :" + isRest);
  }
}
