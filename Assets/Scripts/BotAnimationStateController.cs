using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using System.Data;
using Mono.Data.Sqlite;
using System.Globalization;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using ExciteOMeter;

/**
 *  This class controls all the dashboards in the scene, including the PoW
 * buttons.
 */
public class BotAnimationStateController : MonoBehaviour {
  // Main Dashboard Buttons
  public Button startButtonMain;
  public Button repeatButtonMain;
  public Button skipExplanationMain;

  // Training Avatar and the User Avatar
  public GameObject botAvatar;
  public GameObject avatar;

  // Teleportation Portals
  public GameObject zumbaPortal;
  public GameObject workoutPortal;
  public GameObject customizedPortal;

  public static Animator animator;

  static AudioSource greetingAudio;
  public static int voicecheck;

  public Button chooseExerciseButton;
  public Button viewAssessmentButton;

  // Multiple UIs for each feature
  public GameObject botZumbaUI;
  public GameObject botWorkoutUI;
  public GameObject botExercisesUI;
  public GameObject botAssessmentUI;
  public GameObject botDynamicMusicUI;

  // Instruction Texts
  public GameObject bicepCurlText;
  public GameObject generalText;
  public GameObject frontRaisesText;
  public GameObject squadsText;
  public GameObject jumpingJacksText;
  public GameObject popUpHRText;

  // Buttons
  public Button stopZumbaButton;
  public Button repeatZumbaButton;
  public Button disableZumbaButton;
  public Button startWorkoutExplanationButton;
  public Button startZumbaButton;

  public Button workoutMusicButton;
  public Button zumbaMusicButton;
  public Button dynamicMusicButton;
  public Button disableWorkoutButton;
  public Button stopWorkoutButton;
  public Button repeatWorkoutButton;

  public Button bicepCurlsButton;
  public Button frontRaisesButton;
  public Button squatButton;
  public Button jumpingJacksButton;

  public Button skipWorkoutExplanationButton;
  public Button skipZumbaExplanationButton;

  // Local Variables
  public int count = 0;
  public bool music_status;
  public double a, b;
  public double average, max;
  public Button statistics;
  public Button history;

  [HideInInspector]
  public DateTime dateStart, dateEnd;

  // Objects of other classes
  [HideInInspector]
  public static Performance p;
  [HideInInspector]
  public dbconn db;
  [HideInInspector]
  public userdata us;
  [HideInInspector]
  public Average_HeartRate avghr;
  [HideInInspector]
  public MaxHeartRate maxhr;
  [HideInInspector]
  public Piechart p1;
  [HideInInspector]
  public static dataRetrivaldbconn dR;
  [HideInInspector]
  public Visualizer vis;
  [HideInInspector]
  public static List<float> HR_Data = new List<float>();

  // Exercise Audios
  static AudioSource bicepCurlsAudio;
  static AudioSource frontRaisesAudio;
  static AudioSource squatAudio;
  static AudioSource jumpingJacksAudio;

  // Music Audios
  static AudioSource zumbaAudio;
  static AudioSource workoutMusic;

  public static AudioSource startworkoutexp;

  // Conditional Variables
  bool isStarted;
  bool isZumbaHandled;
  bool isDancing;
  bool isWorkoutHandled;
  bool isWorkingOut;
  public static bool isWorkOutStop;

  // User Specific Data
  static int age;
  public int user_id;
  public double weight;
  static int maxHRPossible;
  static float zone1Start;
  static float zone2Start;
  static float zone3Start;
  static float zone4Start;
  static float zone5Start;

  void Start() {
    db = new dbconn();
    us = new userdata();
    p = new Performance();
    vis = new Visualizer();

    dR = new dataRetrivaldbconn();
    dR = db.getData();
    Debug.Log("User ID in bot animation" + dR.idGet);

    // Heart Rate Zone Definition
    age = int.Parse(dR.ageGet);
    maxHRPossible = 220 - age;
    zone1Start = (float)0.5 * maxHRPossible;
    zone2Start = (float)0.6 * maxHRPossible;
    zone3Start = (float)0.7 * maxHRPossible;
    zone4Start = (float)0.8 * maxHRPossible;
    zone5Start = (float)0.9 * maxHRPossible;

    p1 = new Piechart();
    maxhr = new MaxHeartRate();
    avghr = new Average_HeartRate();

    botWorkoutUI.SetActive(true);
    botExercisesUI.SetActive(false);
    botAssessmentUI.SetActive(false);
    botDynamicMusicUI.SetActive(false);

    chooseExerciseButton.interactable = false;
    stopWorkoutButton.interactable = false;
    repeatWorkoutButton.interactable = false;
    skipWorkoutExplanationButton.interactable = false;

    // Audios
    AudioSource[] audios = GetComponents<AudioSource>();
    greetingAudio = audios[0];
    startworkoutexp = audios[5];
    bicepCurlsAudio = audios[4];
    frontRaisesAudio = audios[3];
    jumpingJacksAudio = audios[2];
    squatAudio = audios[1];
    workoutMusic = audios[6];

    if (voicecheck == 1)
      repeatWorkoutButton.interactable = true;

    // Conditional Variables
    isStarted = false;
    isStarted = false;
    isZumbaHandled = false;
    isDancing = false;
    isWorkoutHandled = false;
    isWorkingOut = false;
  }

  void Awake() {
    AudioSource[] audios = GetComponents<AudioSource>();
    greetingAudio = audios[0];
    startworkoutexp = audios[5];
    bicepCurlsAudio = audios[4];
    frontRaisesAudio = audios[3];
    jumpingJacksAudio = audios[2];
    squatAudio = audios[1];
    workoutMusic = audios[6];
    animator = GetComponentInChildren<Animator>();
    botExercisesUI.SetActive(false);
    botAssessmentUI.SetActive(false);
    repeatWorkoutButton.interactable = false;
    chooseExerciseButton.interactable = false;
    viewAssessmentButton.interactable = false;
    generalText.SetActive(true);
    popUpHRText.SetActive(false);
    isWorkOutStop = true;
  }
  void Update() {
    animator = GetComponentInChildren<Animator>();
    Debug.Log("in update function");
    Debug.Log("IsWorkoutStop : " + isWorkOutStop);
    if (!isWorkOutStop) { // Instruction Texts Visibility Handling
      Debug.Log("Is Working Out");
      if (bicepCurlsButton.interactable) {
        Debug.Log("Is Working Out Bicep Curls");
        if (p.timerStatus()) {
          Debug.Log("Is Working Out Bicep Curls PAUSE");
          // Pause Text
          generalText.SetActive(false);
          bicepCurlText.SetActive(false);
          frontRaisesText.SetActive(false);
          squadsText.SetActive(false);
          jumpingJacksText.SetActive(false);
          popUpHRText.SetActive(true);
        } else {
          Debug.Log("Is Working Out Bicep Curls NORMAL");
          // Bicep Curls Text
          generalText.SetActive(false);
          bicepCurlText.SetActive(true);
          frontRaisesText.SetActive(false);
          squadsText.SetActive(false);
          jumpingJacksText.SetActive(false);
          popUpHRText.SetActive(false);
        }

      } else if (frontRaisesButton.interactable) {
        if (p.timerStatus()) {
          // Pause Text
          generalText.SetActive(false);
          bicepCurlText.SetActive(false);
          frontRaisesText.SetActive(false);
          squadsText.SetActive(false);
          jumpingJacksText.SetActive(false);
          popUpHRText.SetActive(true);
        } else {
          // Front Raises Text
          generalText.SetActive(false);
          bicepCurlText.SetActive(false);
          frontRaisesText.SetActive(true);
          squadsText.SetActive(false);
          jumpingJacksText.SetActive(false);
          popUpHRText.SetActive(false);
        }
      }

      else if (squatButton.interactable) {
        if (p.timerStatus()) {
          // Pause Text
          generalText.SetActive(false);
          bicepCurlText.SetActive(false);
          frontRaisesText.SetActive(false);
          squadsText.SetActive(false);
          jumpingJacksText.SetActive(false);
          popUpHRText.SetActive(true);
        } else {
          // Squats Text
          generalText.SetActive(false);
          bicepCurlText.SetActive(false);
          frontRaisesText.SetActive(false);
          squadsText.SetActive(true);
          jumpingJacksText.SetActive(false);
          popUpHRText.SetActive(false);
        }
      } else if (jumpingJacksButton.interactable) {
        if (p.timerStatus()) {
          // Pause Text
          generalText.SetActive(false);
          bicepCurlText.SetActive(false);
          frontRaisesText.SetActive(false);
          squadsText.SetActive(false);
          jumpingJacksText.SetActive(false);
          popUpHRText.SetActive(true);
        } else {
          // Jumpimg Jacks Text
          generalText.SetActive(false);
          bicepCurlText.SetActive(false);
          frontRaisesText.SetActive(false);
          squadsText.SetActive(false);
          jumpingJacksText.SetActive(true);
          popUpHRText.SetActive(false);
        }
      }
    }

    if (HR_Data == null)
      HR_Data = new List<float>();

    if (!startworkoutexp.isPlaying && isStarted) { // Bot Avatar Animations
      chooseExerciseButton.interactable = true;
      viewAssessmentButton.interactable = true;
      animator.SetBool("isIdle", true);
      animator.SetBool("isTalking", false);
    }
  }

  /**
   * Below are the Functions Associated with Different Buttons
   */
  public void TaskOnStartClickMain() {
    Debug.Log("main button clicked");
    if (animator == null)
      Debug.Log("Audio in main is null:");
    isStarted = true;
    startButtonMain.interactable = false;
    repeatButtonMain.interactable = true;
    skipExplanationMain.interactable = true;
    greetingAudio.Play();

    animator.SetBool("isIdle", false);
    animator.SetBool("isTalking", true);
    // if (!greetingAudio.isPlaying)
    //{
    //     animator.SetBool("isTalking", false);
    //     animator.SetBool("isIdle", true);
    // }
  }

  public void TaskOnWorkoutExplanationClick() {
    startWorkoutExplanationButton.interactable = false;
    repeatWorkoutButton.interactable = true;
    Debug.Log("Animator in start explanation is:" + animator);
    Debug.Log("Audio is : " + startworkoutexp.clip.ToString());

    startworkoutexp.Play();
    animator.SetBool("isTalking", true);
    animator.SetBool("isIdle", false);

    Debug.Log("Talking is running");

    isStarted = true;
    skipWorkoutExplanationButton.interactable = true;
    startWorkoutExplanationButton.interactable = false;
    chooseExerciseButton.interactable = true;
  }
  public void TaskOnRepeatClickMain() {
    greetingAudio.Play();
    startButtonMain.interactable = false;
    // Invoke("SwitchAudio", greetingAudio.clip.length);
    animator.SetBool("isIdle", false);
    animator.SetBool("isTalking", true);
    if (!greetingAudio.isPlaying) {
      animator.SetBool("isTalking", false);
      animator.SetBool("isIdle", true);
    }
  }
  public void TaskOnSkipClickMain() {
    if (greetingAudio.isPlaying)
      greetingAudio.Stop();
    startButtonMain.interactable = true;
    animator.SetBool("isTalking", false);
    animator.SetBool("isIdle", true);
  }
  public void TaskOnPortalZumbaClick() {
    avatar.gameObject.SetActive(false);
    botAvatar.transform.position = new Vector3(-14.74f, 0.0f, -30.73f);
    botAvatar.transform.eulerAngles = new Vector3(0, 0, 0);
    avatar.transform.position =
        zumbaPortal.transform.position + Vector3.up * 2.0f;
    // avatar.transform.eulerAngles= new Vector3(0, 90, 0);
    avatar.gameObject.SetActive(true);
  }
  public void TaskOnPortalWorkoutClick() {
    avatar.gameObject.SetActive(false);
    botAvatar.transform.position = new Vector3(-21.22f, 0.0f, -6.16f);
    botAvatar.transform.eulerAngles = new Vector3(0, 90, 0);
    avatar.transform.position =
        workoutPortal.transform.position + Vector3.up * 2.0f;
    // avatar.transform.eulerAngles= new Vector3(0, 180, 0);
    avatar.gameObject.SetActive(true);
    chooseExerciseButton.interactable = false;
    botAssessmentUI.SetActive(false);
    botWorkoutUI.SetActive(true);
  }
  public void TaskOnPortalCustomisedWorkoutClick() {
    avatar.gameObject.SetActive(false);
    botAvatar.transform.position = new Vector3(1.87f, 0.0f, -26.68f);
    botAvatar.transform.eulerAngles = new Vector3(0, 270, 0);
    avatar.transform.position =
        customizedPortal.transform.position + Vector3.up * 2.0f;
    // avatar.transform.eulerAngles= new Vector3(0, 180, 0);
    avatar.gameObject.SetActive(true);
    botWorkoutUI.SetActive(true);
  }
  public void TaskOnDynamicMusicButtonClick() {
    if (EoM_SignalEmulator.setDeactiveDMS == true) {
      Debug.Log("excitometer is connected");
      botDynamicMusicUI.SetActive(true);
    } else {
      Debug.Log("excitometer not connected");
      botDynamicMusicUI.SetActive(false);
    }

    if (!botDynamicMusicUI.activeSelf &&
        EoM_SignalEmulator.setDeactiveDMS == true) {
      botDynamicMusicUI.SetActive(true);
    } else if (botDynamicMusicUI.activeSelf &&
               EoM_SignalEmulator.setDeactiveDMS == false) {
      botDynamicMusicUI.SetActive(false);
    }
  }
  void TaskOnStartZumbaClick() {
    if (!zumbaMusicButton.GetComponent<AudioSource>().isPlaying) {
      zumbaMusicButton.onClick.Invoke();
      animator.SetBool("isIdle", false);
      animator.SetBool("isDancing", true);
      isDancing = true;
    }
  }
  void TaskOnZumbaMusicClick() {
    if (isZumbaHandled && !isDancing) {
      animator.SetBool("isDancing", true);
      animator.SetBool("isIdle", false);
      animator.SetBool("isTalking", false);
      isDancing = true;
    }
  }
  void TaskOnZumbaStopClick() {

    animator.SetBool("isDancing", false);
    animator.SetBool("isTalking", false);
    animator.SetBool("isIdle", true);
    zumbaAudio.Stop();
    if (zumbaMusicButton.GetComponent<AudioSource>().isPlaying) {
      zumbaMusicButton.onClick.Invoke();
    }
    isDancing = false;
    botAvatar.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
    startZumbaButton.interactable = true;
  }
  void TaskOnZumbaRepeatClick() {

    zumbaAudio.Play();
    // Invoke("SwitchAudio", zumbaAudio.clip.length);
    if (!isDancing) {
      animator.SetBool("isIdle", false);
      animator.SetBool("isTalking", true);
    }
  }
  void TaskOnZumbaDisableClick() {
    botAvatar.SetActive(false);
    botZumbaUI.SetActive(false);
  }
  public void TaskOnExerciseClick() {
    botAssessmentUI.SetActive(false);

    if (!botExercisesUI.activeSelf) {
      botExercisesUI.SetActive(true);
    } else {
      botExercisesUI.SetActive(false);
    }
  }
  public void TaskOnAssessmentClick() {
    botExercisesUI.SetActive(false);

    if (!botAssessmentUI.activeSelf) {
      botAssessmentUI.SetActive(true);
    } else {
      botAssessmentUI.SetActive(false);
    }
  }
  public void TaskOnWorkoutDisableClick() {
    botAvatar.SetActive(false);
    botWorkoutUI.SetActive(false);
  }
  public void TaskOnWorkoutStopClick() {
    db.CreateDB();
    // botAvatar.transform.localPosition = new Vector3(1.606f, -0.15f,
    // -11.826f); botAvatar.transform.rotation = new Quaternion(90.0f, 0.0f,
    // 0.0f, 0);
    startWorkoutExplanationButton.interactable = true;
    repeatWorkoutButton.interactable = true;
    skipWorkoutExplanationButton.interactable = true;
    chooseExerciseButton.interactable = true;
    viewAssessmentButton.interactable = true;
    bicepCurlsButton.interactable = true;
    frontRaisesButton.interactable = true;
    squatButton.interactable = true;
    jumpingJacksButton.interactable = true;

    generalText.SetActive(true);
    bicepCurlText.SetActive(false);
    frontRaisesText.SetActive(false);
    squadsText.SetActive(false);
    jumpingJacksText.SetActive(false);
    viewAssessmentButton.interactable = true;
    isWorkingOut = false;
    animator.SetBool("isBicepCurls", false);
    animator.SetBool("isFrontRaises", false);
    animator.SetBool("isSquat", false);
    animator.SetBool("isJumpingJacks", false);
    animator.SetBool("isTalking", false);
    animator.SetBool("isIdle", true);
    startworkoutexp.Stop();
    if (bicepCurlsButton.gameObject.activeSelf) {
      bicepCurlsAudio.Stop();
      a = Compute_MAX_HR(HR_Data);
      b = Compute_AVG_HR(HR_Data);

      isWorkOutStop = true;

      if (us.userid == int.Parse(dR.idGet) && us.exercise_id == 1 &&
          us.exercise_end == " ") {
        us.exercise_end =
            System.DateTime.Now.ToString("yyyy - MM - dd\\T HH: mm:ss\\Z");
        us.noOfSets = p.getSets();
        us.avg_HR = b;
        us.max_HR = a;
        Debug.Log("stored average value is  " + b);
        Debug.Log("stored max value is  " + a);
        dateStart = DateTime.Parse(us.exercise_start);
        dateEnd = DateTime.Parse(us.exercise_end);
        us.calories_burnt = db.calculateCaloriesBurnt(us.userid, us.exercise_id,
                                                      dateStart, dateEnd);
        db.AddRecord(us);
      }
    }
    if (frontRaisesButton.gameObject.activeSelf) {
      frontRaisesAudio.Stop();
      a = Compute_MAX_HR(HR_Data);
      b = Compute_AVG_HR(HR_Data);
      Debug.Log("front raise stored average value is  " + b);
      Debug.Log("stored max value is  " + a);

      isWorkOutStop = true;

      if (us.userid == int.Parse(dR.idGet) && us.exercise_id == 2 &&
          us.exercise_end == " ") {
        us.exercise_end =
            System.DateTime.Now.ToString("yyyy - MM - dd\\T HH: mm:ss\\Z");
        us.noOfSets = p.getSets();
        us.avg_HR = b;
        us.max_HR = a;
        Debug.Log("stored average value is  " + b);
        Debug.Log("stored max value is  " + a);
        dateStart = DateTime.Parse(us.exercise_start);
        dateEnd = DateTime.Parse(us.exercise_end);
        us.calories_burnt = db.calculateCaloriesBurnt(us.userid, us.exercise_id,
                                                      dateStart, dateEnd);
        db.AddRecord(us);
      }
    }
    if (squatButton.gameObject.activeSelf) {
      squatAudio.Stop();
      a = Compute_MAX_HR(HR_Data);
      b = Compute_AVG_HR(HR_Data);

      isWorkOutStop = true;

      if (us.userid == int.Parse(dR.idGet) && us.exercise_id == 3 &&
          us.exercise_end == " ") {
        us.exercise_end =
            System.DateTime.Now.ToString("yyyy - MM - dd\\T HH: mm:ss\\Z");
        us.noOfSets = p.getSets();
        us.avg_HR = b;
        us.max_HR = a;
        Debug.Log("stored average value is  " + b);
        Debug.Log("stored max value is  " + a);
        dateStart = DateTime.Parse(us.exercise_start);
        dateEnd = DateTime.Parse(us.exercise_end);
        us.calories_burnt = db.calculateCaloriesBurnt(us.userid, us.exercise_id,
                                                      dateStart, dateEnd);
        db.AddRecord(us);
      }
    }
    if (jumpingJacksButton.gameObject.activeSelf) {
      jumpingJacksAudio.Stop();
      a = Compute_MAX_HR(HR_Data);
      b = Compute_AVG_HR(HR_Data);

      isWorkOutStop = true;

      if (us.userid == int.Parse(dR.idGet) && us.exercise_id == 4 &&
          us.exercise_end == " ") {
        us.exercise_end =
            System.DateTime.Now.ToString("yyyy - MM - dd\\T HH: mm:ss\\Z");
        us.noOfSets = p.getSets();
        us.avg_HR = b;
        us.max_HR = a;
        Debug.Log("stored average value is  " + b);
        Debug.Log("stored max value is  " + a);
        dateStart = DateTime.Parse(us.exercise_start);
        dateEnd = DateTime.Parse(us.exercise_end);
        us.calories_burnt = db.calculateCaloriesBurnt(us.userid, us.exercise_id,
                                                      dateStart, dateEnd);
        db.AddRecord(us);
      }
    }
    // botAvatar.transform.localPosition = new Vector3(2.8f, -0.6f, 2.4f);
    // botAvatar.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
  }

  public void TaskOnWorkoutRepeatClick() {
    startworkoutexp.Play();
    if (animator.GetBool("isIdle")) {
      animator.SetBool("isIdle", false);
      animator.SetBool("isTalking", true);
    }
  }
  public void TaskOnBicepCurlsClick() {
    Debug.Log("BicepCurls");
    isStarted = false;
    startWorkoutExplanationButton.interactable = false;
    repeatWorkoutButton.interactable = false;
    skipWorkoutExplanationButton.interactable = false;
    chooseExerciseButton.interactable = false;
    viewAssessmentButton.interactable = false;
    frontRaisesButton.interactable = false;
    squatButton.interactable = false;
    jumpingJacksButton.interactable = false;
    stopWorkoutButton.interactable = true;
    dR = db.getData();
    isWorkingOut = true;
    isWorkOutStop = false;
    bicepCurlsAudio.Play();
    us.userid = int.Parse(dR.idGet);
    Debug.Log("id is" + us.userid);
    us.exercise_id = 1;
    us.exercise_start =
        System.DateTime.Now.ToString("yyyy - MM - dd\\T HH: mm:ss\\Z");
    us.exercise_end = " ";
    us.noOfSets = 0;
    us.avg_HR = 0;
    us.max_HR = 0;
    us.calories_burnt = 0;
    Debug.Log("Bicep Audio is:" + bicepCurlsAudio.clip.ToString());
    frontRaisesAudio.Stop();
    squatAudio.Stop();
    jumpingJacksAudio.Stop();
    viewAssessmentButton.interactable = false;

    animator.SetBool("isFrontRaises", false);
    animator.SetBool("isSquat", false);
    animator.SetBool("isJumpingJacks", false);
    animator.SetBool("isTalking", false);
    animator.SetBool("isIdle", false);
    animator.SetBool("isBicepCurls", true);
  }
  public void TaskOnFrontRaisesClick() {
    isStarted = false;
    startWorkoutExplanationButton.interactable = false;
    repeatWorkoutButton.interactable = false;
    skipWorkoutExplanationButton.interactable = false;
    chooseExerciseButton.interactable = false;
    viewAssessmentButton.interactable = false;
    bicepCurlsButton.interactable = false;
    squatButton.interactable = false;
    jumpingJacksButton.interactable = false;
    stopWorkoutButton.interactable = true;
    Debug.Log("FrontRaises");
    isWorkingOut = true;
    isWorkOutStop = false;
    us.userid = int.Parse(dR.idGet);
    us.exercise_id = 2;
    us.exercise_start =
        System.DateTime.Now.ToString("yyyy - MM - dd\\T HH: mm: ss\\Z");
    us.exercise_end = " ";
    us.noOfSets = 0;
    us.avg_HR = 0;
    us.max_HR = 0;
    us.calories_burnt = 0;
    // botAvatar.transform.localPosition = new Vector3(2.8f, -0.6f, 2.4f);
    // botAvatar.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

    bicepCurlsAudio.Stop();
    frontRaisesAudio.Play();
    squatAudio.Stop();
    jumpingJacksAudio.Stop();
    animator.SetBool("isFrontRaises", true);
    animator.SetBool("isBicepCurls", false);
    animator.SetBool("isSquat", false);
    animator.SetBool("isJumpingJacks", false);
    animator.SetBool("isTalking", false);
    animator.SetBool("isIdle", false);
    viewAssessmentButton.interactable = false;
  }
  public void TaskOnSquatClick() {
    isStarted = false;
    isWorkingOut = true;
    isWorkOutStop = false;
    startWorkoutExplanationButton.interactable = false;
    repeatWorkoutButton.interactable = false;
    skipWorkoutExplanationButton.interactable = false;
    chooseExerciseButton.interactable = false;
    viewAssessmentButton.interactable = false;
    bicepCurlsButton.interactable = false;
    frontRaisesButton.interactable = false;
    jumpingJacksButton.interactable = false;
    stopWorkoutButton.interactable = true;
    dR = db.getData();
    Debug.Log("Squats");
    us.userid = int.Parse(dR.idGet);
    us.exercise_id = 3;
    us.exercise_start =
        System.DateTime.Now.ToString("yyyy - MM - dd\\T HH: mm: ss\\Z");
    us.exercise_end = " ";
    us.noOfSets = 0;
    us.avg_HR = 0;
    us.max_HR = 0;
    us.calories_burnt = 0;
    // botAvatar.transform.localPosition = new Vector3(2.8f, -0.6f, 2.4f);
    // botAvatar.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
    animator.SetBool("isBicepCurls", false);
    animator.SetBool("isFrontRaises", false);
    animator.SetBool("isJumpingJacks", false);
    animator.SetBool("isTalking", false);
    animator.SetBool("isIdle", false);
    animator.SetBool("isSquat", true);
    bicepCurlsAudio.Stop();
    frontRaisesAudio.Stop();
    squatAudio.Play();
    jumpingJacksAudio.Stop();
    viewAssessmentButton.interactable = false;
  }
  public void TaskOnJumpingJacksClick() {
    isStarted = false;
    isWorkingOut = true;
    isWorkOutStop = false;
    startWorkoutExplanationButton.interactable = false;
    repeatWorkoutButton.interactable = false;
    skipWorkoutExplanationButton.interactable = false;
    chooseExerciseButton.interactable = false;
    viewAssessmentButton.interactable = false;
    bicepCurlsButton.interactable = false;
    frontRaisesButton.interactable = false;
    squatButton.interactable = false;
    stopWorkoutButton.interactable = true;
    dR = db.getData();
    Debug.Log("JumpingJacks");
    us.userid = int.Parse(dR.idGet);
    us.exercise_id = 4;
    us.exercise_start =
        System.DateTime.Now.ToString("yyyy - MM - dd\\T HH: mm: ss\\Z");
    us.exercise_end = " ";
    us.noOfSets = 0;
    us.avg_HR = 0;
    us.max_HR = 0;
    us.calories_burnt = 0;
    animator.SetBool("isBicepCurls", false);
    animator.SetBool("isFrontRaises", false);
    animator.SetBool("isSquat", false);
    animator.SetBool("isTalking", false);
    animator.SetBool("isIdle", false);
    animator.SetBool("isJumpingJacks", true);
    bicepCurlsAudio.Stop();
    frontRaisesAudio.Stop();
    squatAudio.Stop();
    jumpingJacksAudio.Play();
    viewAssessmentButton.interactable = false;
  }
  public void TaskOnskipWorkoutExplanationClick() {
    animator.SetBool("isTalking", false);
    animator.SetBool("isIdle", true);
    if (startworkoutexp.isPlaying)
      startworkoutexp.Stop();
    repeatWorkoutButton.interactable = true;
  }
  void TaskOnSkipZumbaExplanantionClick() {
    if (zumbaAudio.isPlaying)
      zumbaAudio.Stop();
    startWorkoutExplanationButton.interactable = true;
  }
  public void TaskonWorkoutMusicClick() {
    if (workoutMusic.isPlaying) {
      workoutMusic.Stop();
    } else {
      workoutMusic.Play();
    }
  }

  // Gets HR values sent by LSL_Inlet_HR Script
  public void Get_HR_Value(float new_value) {

    Debug.Log("new value inside gethrvalue is " + new_value +
              " and workout stop is " + isWorkOutStop);

    if (!isWorkOutStop) // false
    {
      Debug.Log("before adding");
      HR_Data.Add(new_value);
      if (HR_Data == null)
        Debug.Log("Not adding");
      Debug.Log("value of isWorkOutStop is " + isWorkOutStop);
      checkForRest(new_value);
    }
  }

  // Computes Maximum HR for each exercise
  public double Compute_MAX_HR(List<float> data1) {
    max = data1.Max();
    Debug.Log("the max value is" + max);
    return max;
  }

  // Computes Average HR for each exercise
  public double Compute_AVG_HR(List<float> data2) {
    Debug.Log("values stored in list are");
    average = data2.Average();
    Debug.Log("the average value is" + average);
    return average;
  }

  // Checks for Pause Condition
  public void checkForRest(float HRvalue) {
    Debug.Log("Zone 4 starts from " + zone4Start + " and current value is " +
              HRvalue);

    if (HRvalue > zone4Start && HRvalue <= zone5Start) // Zone4
    {
      Debug.Log("before calling pause");
      p.initiatePause(true);
    }

    else if (HRvalue > zone5Start && HRvalue <= maxHRPossible) // Zone5
    {
      Debug.Log("before calling pause");
      p.initiatePause(true);
    }

    else {
      Debug.Log("before calling pause");
      p.initiatePause(false);
    }
  }
}