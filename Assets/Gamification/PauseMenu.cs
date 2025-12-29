using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviourPunCallbacks {
  // Start is called before the first frame update
  public static bool GameIsPaused = false;
  int flag = 0;

  public GameObject pauseMenuUI;
  public GameObject resumeButton;
  public GameObject playButton;
  public GameObject ControllerRay;
  public GameObject ControllerRay2;
  public GameObject displaytext;
  public GameObject goBack;
  public GameObject demoPlay;

  public GameObject gameOverMenu;
  public Text gameOverScore;

  public GameObject Instruction;
  public GameObject Instruction1;
  public GameObject Instruction2;

  public GameObject backButtonObject;
  public static bool GameStart = false;
  public TimerScript gametime;
  public bool testForPause = false;
  public static PauseMenu instance;

  public bool demoPause;
  public void Awake() { instance = this; }

  void Start() {
    Debug.Log("Before Pause");
    Pause();
    GameIsPaused = true;
    Debug.Log("Before Pause");
  }

  void Update() {
    Debug.Log("testforpause 1: " + testForPause);
    if (OVRInput.GetDown(OVRInput.Button.One)) {

      if (GameIsPaused == true) {
        Resume();
        testForPause = false;
        DisplayText();
      } else {

        Pause();
        testForPause = true;
        /* if (demoPause == true)
         {
             return;
         }
         else
         {
             Debug.Log("testforpause 2: " + testForPause);
             DisplayText();
         }*/
        Debug.Log("testforpause 2: " + testForPause);
        DisplayText();
      }
    }
  }

  // Photon RPC function to synchronize the display such as the pause menu
  [PunRPC]
  void SynchronizeDisplay(bool currentpause) {

    if (PhotonNetwork.IsMasterClient == false) {
      this.testForPause = currentpause;
    }
    // pauseMenuUI.SetActive(testForPause);
    displaytext.SetActive(testForPause);
    Debug.Log("testforpause RPC: " + testForPause);

    // this.displaytext.text = "Game is Paused";
  }

  public void DisplayText() {

    photonView.RPC("SynchronizeDisplay", Photon.Pun.RpcTarget.AllBuffered,
                   this.testForPause);
  }

  // function to pause the game and show the menu screen
  public void Pause() {
    if (PhotonNetwork.IsMasterClient) {

      Debug.Log("am i master?");
      pauseMenuUI.SetActive(true);
      // Time.timeScale = 0f;
      GameObject.Find("Spawner").GetComponent<Spawnerr>().enabled = false;
      Debug.Log("spawner disabled?");
      GameObject.Find("BonusManager").GetComponent<BonusManager>().enabled =
          false;

      GameIsPaused = true;
      ControllerRay.SetActive(true);
      ControllerRay2.SetActive(true);
      playButton.SetActive(false);
      if (demoPause == true) {
        resumeButton.SetActive(false);
      } else {
        resumeButton.SetActive(true);
      }
    }
  }
  // Instruction turn on and off
  public void Instructions() {
    Instruction.SetActive(true);
    pauseMenuUI.SetActive(false);
  }
  // next button turn on and off
  public void nextButton() {
    Instruction1.SetActive(true);
    Instruction.SetActive(false);
  }

  // nextbutton1 turn on and off
  public void nextButton1() {
    Instruction2.SetActive(true);
    Instruction1.SetActive(false);
  }

  // back button turn on and off
  public void backButton() {
    Instruction.SetActive(false);
    pauseMenuUI.SetActive(true);
  }

  // backbutton1 turn on and off
  public void backButton1() {
    Instruction1.SetActive(false);
    Instruction.SetActive(true);
  }

  // backbutton2  turn on and off
  public void backButton2() {
    Instruction2.SetActive(false);
    Instruction1.SetActive(true);
  }
  // backButton2menu turn on and off
  public void backButton2menu() {
    Instruction2.SetActive(false);
    pauseMenuUI.SetActive(true);
  }

  // play button function to start the game

  public void Play() {

    goBack.SetActive(false);
    pauseMenuUI.SetActive(false);
    // Time.timeScale = 1f;
    GameObject.Find("Spawner").GetComponent<Spawnerr>().enabled = true;
    GameObject.Find("BonusManager").GetComponent<BonusManager>().enabled = true;
    GameIsPaused = false;
    playButton.SetActive(false);
    resumeButton.SetActive(true);
    ControllerRay.SetActive(false);
    ControllerRay2.SetActive(false);
    gametime.changePlayingStatus(true);

    healthManager.health = 100;
  }

  // Resume button function to resume the game if it is paused
  public void Resume() {

    testForPause = false;
    DisplayText();
    displaytext.SetActive(false);
    GameStart = true;
    pauseMenuUI.SetActive(false);
    // Time.timeScale = 1f;
    GameObject.Find("Spawner").GetComponent<Spawnerr>().enabled = true;
    GameObject.Find("BonusManager").GetComponent<BonusManager>().enabled = true;

    GameIsPaused = false;
    ControllerRay.SetActive(false);
    ControllerRay2.SetActive(false);
    GameStart = true;
  }

  // Show the gameover result after the game ends
  public void ShowGameOver(int score) {
    this.gameOverMenu.SetActive(true);
    this.gameOverScore.text = "Score: " + score.ToString();

    ControllerRay.SetActive(true);
    ControllerRay2.SetActive(true);
  }

  // play again  button function to play the game again
  public void PlayAgain() {
    gametime.flag = 0;
    pauseMenuUI.SetActive(false);

    GameObject.Find("Spawner").GetComponent<Spawnerr>().enabled = true;
    GameObject.Find("Spawner").GetComponent<Spawnerr>().Reset();

    GameIsPaused = false;

    pauseMenuUI.SetActive(false);
    gameOverMenu.SetActive(false);

    playButton.SetActive(false);
    resumeButton.SetActive(true);
    ControllerRay.SetActive(false);
    ControllerRay2.SetActive(false);

    gametime.Restart();
    gametime.changePlayingStatus(true);
    ScoreManager.instance.ResetPoint();

    healthManager.health = 100;
  }

  // function to to reset everything

  public void goBackFunc() {
    gametime.flag = 0;
    playButton.SetActive(true);
    goBack.SetActive(false);
    demoPlay.SetActive(true);
    GameObject.Find("Spawner").GetComponent<Spawnerr>().Reset();
    demoPause = false;
    ScoreManager.instance.ResetPoint();
    healthManager.health = 100;
  }

  // function for demo button to set buttons accordingly

  public void demoButtonSettings() {
    resumeButton.SetActive(false);
    gametime.flag = 0;
    playButton.SetActive(false);
    demoPause = true;
    goBack.SetActive(true);
  }
}
