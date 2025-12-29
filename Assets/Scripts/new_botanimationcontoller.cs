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
public class new_botanimationcontoller : MonoBehaviour {
  public Button startButton;
  public Button repeatButton;
  public Button skipExplanation;
  // public Button zumbaButton;
  // public Button workoutButton;
  // public Button customisedWorkoutButton;
  public GameObject botAvatar;
  public GameObject avatar;
  public GameObject zumbaPortal;
  public GameObject workoutPortal;
  public GameObject customizedPortal;

  Animator animator;
  public GameObject greetingAudioSource;
  AudioSource greetingAudio;
  public static int voicecheck;

  public Button chooseExerciseButton; //
  public Button viewAssessmentButton;
  public GameObject botZumbaUI;
  public GameObject botWorkoutUI;
  public GameObject botAssessmentUI;

  int c = 0;
  bool isStarted;
  // Start is called before the first frame update
  void Start() {
    botWorkoutUI.SetActive(false);
    // botZumbaUI.SetActive(false);
    botAssessmentUI.SetActive(false);
    animator = GetComponentInChildren<Animator>();
    // InvokeRepeating("LaunchProjectile", 2.0f, 5.0f);
    startButton.onClick.AddListener(TaskOnStartClick);
    repeatButton.onClick.AddListener(TaskOnRepeatClick);
    skipExplanation.onClick.AddListener(TaskOnSkipClick);
    // zumbaButton.onClick.AddListener(TaskOnZumbaClick);
    // workoutButton.onClick.AddListener(TaskOnWorkoutClick);
    // customisedWorkoutButton.onClick.AddListener(TaskOnCustomisedWorkoutClick);
    greetingAudio =
        greetingAudioSource.GetComponent<AudioSource>(); // yet to be created

    if (voicecheck == 1) {
      // animator.SetBool("isTalking", false);
      // animator.SetBool("isIdle", true);
      // zumbaButton.interactable = true;
      repeatButton.interactable = true;
      // workoutButton.interactable = true;
    }
    isStarted = false;
    skipExplanation.interactable = false;
  }
  // void LaunchProjectile()
  //{
  //     if(c==0){
  //     botAvatar.transform.position = new Vector3(-14.74f, 0.0f, -30.73f);
  //     botAvatar.transform.eulerAngles = new Vector3(0, 0, 0);

  //    avatar.transform.position = zumbaPortal.transform.position + new
  //    Vector3(0, 0, 8); avatar.transform.eulerAngles= new Vector3(0, 0, 0);
  //    c=1;

  //    animator.SetBool("isTalking", true);
  //    }
  //    else if( c==1){
  //         c=2;
  //    botAvatar.transform.position = new Vector3(-21.22f, 0.0f, -6.16f);
  //    botAvatar.transform.eulerAngles = new Vector3(0, 90, 0);

  //     avatar.transform.position = workoutPortal.transform.position+ new
  //     Vector3(-8, 0, 0); avatar.transform.eulerAngles= new Vector3(0, 180,
  //     0); animator.SetBool("isTalking", false);
  //     animator.SetBool("isIdle",true);
  //     }
  //     else{
  //        botAvatar.transform.position = new Vector3(1.87f, 0.0f, -26.68f);
  //    botAvatar.transform.eulerAngles = new Vector3(0, 270, 0);
  //        avatar.transform.position = customizedPortal.transform.position+new
  //        Vector3(-4, 0, 8); avatar.transform.eulerAngles= new Vector3(0, 180,
  //        0);

  //    }
  //    //animator.SetBool("isTalking",true);

  //}
  // Update is called once per frame
  // void Update()
  //{
  //    if (!greetingAudio.isPlaying && isStarted)
  //    {
  //        voicecheck = 1;
  //        //animator.SetBool("isTalking", false);
  //        //animator.SetBool("isIdle", true);
  //        //zumbaButton.interactable = true;
  //        //workoutButton.interactable = true;
  //        //dynamicMusicButton.interactable = true;
  //        //botDynamicMusicUI.SetActive(true);
  //    }
  //}
  void TaskOnStartClick() {
    isStarted = true;
    startButton.interactable = false;
    repeatButton.interactable = true;
    skipExplanation.interactable = true;
    greetingAudio.Play();
    animator.SetBool("isTalking", true);
    if (!greetingAudio.isPlaying) {
      animator.SetBool("isTalking", false);
      animator.SetBool("isIdle", true);
    }
  }
  void TaskOnRepeatClick() {
    // if (vis != null)
    //      vis.StopMusic();
    greetingAudio.Play();
    startButton.interactable = false;
    // Invoke("SwitchAudio", greetingAudio.clip.length);
    animator.SetBool("isIdle", false);
    animator.SetBool("isTalking", true);
    if (!greetingAudio.isPlaying) {
      animator.SetBool("isTalking", false);
      animator.SetBool("isIdle", true);
    }
  }
  void TaskOnSkipClick() {
    if (greetingAudio.isPlaying)
      greetingAudio.Stop();
    startButton.interactable = true;
    animator.SetBool("isTalking", false);
    animator.SetBool("isIdle", true);
  }
  public void TaskOnZumbaClick() {
    avatar.gameObject.SetActive(false);
    botAvatar.transform.position = new Vector3(-14.74f, 0.0f, -30.73f);
    botAvatar.transform.eulerAngles = new Vector3(0, 0, 0);
    avatar.transform.position =
        zumbaPortal.transform.position + Vector3.up * 2.0f;
    // avatar.transform.eulerAngles= new Vector3(0, 90, 0);

    avatar.gameObject.SetActive(true);
  }
  public void TaskOnWorkoutClick() {
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
  public void TaskOnCustomisedWorkoutClick() {
    avatar.gameObject.SetActive(false);
    botAvatar.transform.position = new Vector3(1.87f, 0.0f, -26.68f);
    botAvatar.transform.eulerAngles = new Vector3(0, 270, 0);
    avatar.transform.position =
        customizedPortal.transform.position + Vector3.up * 2.0f;
    // avatar.transform.eulerAngles= new Vector3(0, 180, 0);
    avatar.gameObject.SetActive(true);
    botWorkoutUI.SetActive(true);
  }
}