using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**
 * This Class handles the Dynamic Music System.
 * Gets HR Values, applies zones and changes the music levels and files.
 */

public class Visualizer : MonoBehaviour {
  [HideInInspector]
  public dbconn db;

  // UI Related
  public float minHeight = 15.0f;
  public float maxHeight = 425.0f;
  public float updateSenstivity = 0.5f;
  public Color visualizerColor = Color.gray;
  public static bool status;
  public Button Music1;
  public Button Music2;
  public Slider volumeSlider;
  static Scene activeScene;
  static string sceneName;

  // Music Related
  [Space(15), Range(64, 8192)]
  public int visualizerSimples = 64;
  static VisualizerObjectScript[] visualizerObjects;
  public static AudioSource audioSource;
  static AudioSource lev1audioSource;
  static AudioSource lev2audioSource;
  static AudioSource lev3audioSource;
  static AudioSource lev4audioSource;
  static AudioSource lev5audioSource;
  static AudioSource M2lev1audioSource;
  static AudioSource M2lev2audioSource;
  static AudioSource M2lev3audioSource;
  static AudioSource M2lev4audioSource;
  static AudioSource M2lev5audioSource;
  static AudioSource cool_off_music;

  public dataRetrivaldbconn dR;
  public BotAnimationStateController bsc;

  // Heart Rate Zones Related
  static int age;
  static int maxHRPossible;
  static float zone1Start;
  static float zone2Start;
  static float zone3Start;
  static float zone4Start;
  static float zone5Start;

  // Music Buttons
  static bool M1toggle;
  static bool M2toggle;

  // Conditional Variables
  public static bool StopWorkout;
  public static bool isSpeaking;
  public static string playingClip;

  // Start is called before the first frame update
  void Start() {
    db = new dbconn();
    activeScene = SceneManager.GetActiveScene();
    sceneName = activeScene.name;
    StopWorkout = false;

    bsc = new BotAnimationStateController();
    dR = db.getData();

    // Audios
    AudioSource[] audios = GetComponents<AudioSource>();
    audioSource = audios[0];
    lev1audioSource = audios[1];
    lev2audioSource = audios[2];
    lev3audioSource = audios[3];
    lev4audioSource = audios[4];
    lev5audioSource = audios[5];
    M2lev1audioSource = audios[6];
    M2lev2audioSource = audios[7];
    M2lev3audioSource = audios[8];
    M2lev4audioSource = audios[9];
    M2lev5audioSource = audios[10];
    cool_off_music = audios[11];

    volumeSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });

    // Defining HR Zones
    age = int.Parse(dR.ageGet);
    Debug.Log("entering the start function in visualizer, age : " + age);
    maxHRPossible = 220 - age;
    zone1Start = (float)0.5 * maxHRPossible;
    zone2Start = (float)0.6 * maxHRPossible;
    zone3Start = (float)0.7 * maxHRPossible;
    zone4Start = (float)0.8 * maxHRPossible;
    zone5Start = (float)0.9 * maxHRPossible;

    Debug.Log("Zone 2 Starts from " + zone2Start);

    if (audioSource.isPlaying) {
      audioSource.Stop();
      Debug.Log("audio start is" + audioSource.clip.ToString());
    }
    if (lev1audioSource.isPlaying) {
      Debug.Log("audio start is" + lev1audioSource.clip.ToString());
      lev1audioSource.Stop();
    }
    if (lev2audioSource.isPlaying) {
      Debug.Log("audio start is" + lev2audioSource.clip.ToString());
      lev2audioSource.Stop();
    }
    if (lev3audioSource.isPlaying) {
      Debug.Log("audio start is" + lev3audioSource.clip.ToString());
      lev3audioSource.Stop();
    }
    if (lev4audioSource.isPlaying) {
      Debug.Log("audio start is" + lev4audioSource.clip.ToString());
      lev4audioSource.Stop();
    }
    if (lev5audioSource.isPlaying) {
      Debug.Log("audio start is" + lev5audioSource.clip.ToString());
      lev5audioSource.Stop();
    }

    visualizerObjects = GetComponentsInChildren<VisualizerObjectScript>();
  }

  // Update is called once per frame
  void Update() {
    audioSource.loop = true;
    Debug.Log("Active Scene is " + sceneName);
    if (sceneName == "Exergames") { // Turning Workout button to True initially
                                    // for Exergames Scene
      StopWorkout = false;
      M2toggle = true;
    }

    spectrumCreate(); // DMS Visualizer UI

    if (!M1toggle && !M2toggle) { // Playing Audios
      if (audioSource.isPlaying) {
        audioSource.Stop();
        Debug.Log("audio update is" + audioSource.clip.ToString());
      }
      if (lev1audioSource.isPlaying) {
        Debug.Log("audio update is" + lev1audioSource.clip.ToString());
        lev1audioSource.Stop();
      }
      if (lev2audioSource.isPlaying) {
        Debug.Log("audio update is" + lev2audioSource.clip.ToString());
        lev2audioSource.Stop();
      }
      if (lev3audioSource.isPlaying) {
        Debug.Log("audio update is" + lev3audioSource.clip.ToString());
        lev3audioSource.Stop();
      }
      if (lev4audioSource.isPlaying) {
        Debug.Log("audio update is" + lev4audioSource.clip.ToString());
        lev4audioSource.Stop();
      }
      if (lev5audioSource.isPlaying) {
        Debug.Log("audio update is" + lev5audioSource.clip.ToString());
        lev5audioSource.Stop();
      }
    }
  }

  // Creates Spectrum on Visualizer UI
  public void spectrumCreate() {
    float[] spectrumData = audioSource.GetSpectrumData(visualizerSimples, 0,
                                                       FFTWindow.Rectangular);
    for (int i = 0; i < visualizerObjects.Length; i++) {
      Vector2 newSize =
          visualizerObjects[i].GetComponent<RectTransform>().rect.size;
      newSize.y =
          Mathf.Clamp(Mathf.Lerp(newSize.y,
                                 minHeight + (spectrumData[i] *
                                              (maxHeight - minHeight) * 5.0f),
                                 updateSenstivity),
                      minHeight, maxHeight);
      visualizerObjects[i].GetComponent<RectTransform>().sizeDelta = newSize;
      visualizerObjects[i].GetComponent<Image>().color =
          Color.Lerp(Color.blue, Color.red, Mathf.PingPong(Time.time, 0.9f));
    }
  }

  // Function for Music 1 Button
  public void toggleFunctionButton() {
    M1toggle = !M1toggle;
    M2toggle = false;
    Debug.Log("Play Button Music 1: " + M1toggle.ToString());
    if (M1toggle && !isSpeaking) {
      Music1.GetComponent<Image>().color = Color.green;
      Music2.GetComponent<Image>().color = Color.white;
      Debug.Log("entering the if loop of toggle function");
      audioSource.loop = true;
      audioSource.Play();
      // spectrumCreate();
    } else {
      Music1.GetComponent<Image>().color = Color.white;
      audioSource.Stop();
    }
  }

  // Function for Music 2 Button
  public void M2toggleFunctionButton() {
    M1toggle = false;
    M2toggle = !M2toggle;
    Debug.Log("Play Button Music 2: " + M2toggle.ToString());
    if (M2toggle && !isSpeaking) {
      Music1.GetComponent<Image>().color = Color.white;
      Music2.GetComponent<Image>().color = Color.green;
      Debug.Log("entering the if loop of toggle function");
      audioSource.loop = true;
      audioSource.Play();
      // spectrumCreate();
    } else {
      Music2.GetComponent<Image>().color = Color.white;
      audioSource.Stop();
    }
  }

  // Gets HR values from LSL_Inlet_HR and changes the music accordingly
  public void updateMusic(float HRvalue) {
    Debug.Log("Update Music Entered!!!");

    if (!isSpeaking) {
      if (!StopWorkout) {
        if (M1toggle) {
          Debug.Log("playing music is: " + audioSource.clip.ToString());
          Debug.Log("M1Toggle is True!!!");
          Debug.Log("Received HR Value is : " + HRvalue.ToString());
          if (HRvalue < zone1Start) //<Zone1
          {
            // Default
            //  Check Music Level Playing
            if (audioSource != lev1audioSource) {
              audioSource.Stop();
              audioSource = lev1audioSource;
              audioSource.Play();
            }
          } else if (HRvalue > zone1Start && HRvalue <= zone2Start) // Zone1
          {
            // Level 3
            //  Check Music Level Playing
            if (audioSource != lev3audioSource) {
              audioSource.Stop();
              audioSource = lev3audioSource;
              audioSource.Play();
            }
          } else if (HRvalue > zone2Start && HRvalue <= zone3Start) // Zone2
          {
            // Level 4
            //  Check Music Level Playing
            if (audioSource != lev4audioSource) {
              audioSource.Stop();
              audioSource = lev4audioSource;
              audioSource.Play();
            }
          } else if (HRvalue > zone3Start && HRvalue <= zone4Start) // Zone3
          {
            // Level 5
            //  Check Music Level Playing
            if (audioSource != lev5audioSource) {
              audioSource.Stop();
              audioSource = lev5audioSource;
              audioSource.Play();
            }
          } else if (HRvalue > zone4Start && HRvalue <= zone5Start) // Zone4
          {
            // Level 3
            //  Check Music Level Playing

            if (audioSource != lev2audioSource) {
              audioSource.Stop();
              audioSource = lev2audioSource;
              audioSource.Play();
            }
          } else if (HRvalue > zone5Start && HRvalue <= maxHRPossible) // Zone5
          {
            // Level 2 and Ask to Stop
            //  Check Music Level Playing

            if (audioSource != lev1audioSource) {
              audioSource.Stop();
              audioSource = lev1audioSource;
              audioSource.Play();
            }
          }
        } else if (M2toggle) {
          Debug.Log("playing music is: " + audioSource.clip.ToString());
          Debug.Log("M2Toggle is True!!!");
          Debug.Log("Received HR Value is : " + HRvalue.ToString());
          if (HRvalue < zone1Start) //<Zone1
          {
            // Default
            //  Check Music Level Playing
            if (audioSource != M2lev1audioSource) {
              audioSource.Stop();
              audioSource = M2lev1audioSource;
              audioSource.Play();
            }
          } else if (HRvalue > zone1Start && HRvalue <= zone2Start) // Zone1
          {
            // Level 3
            //  Check Music Level Playing
            if (audioSource != M2lev3audioSource) {
              audioSource.Stop();
              audioSource = M2lev3audioSource;
              audioSource.Play();
            }
          } else if (HRvalue > zone2Start && HRvalue <= zone3Start) // Zone2
          {
            // Level 4
            //  Check Music Level Playing
            if (audioSource != M2lev4audioSource) {
              audioSource.Stop();
              audioSource = M2lev4audioSource;
              audioSource.Play();
            }
          } else if (HRvalue > zone3Start && HRvalue <= zone4Start) // Zone3
          {
            // Level 5
            //  Check Music Level Playing
            if (audioSource != M2lev5audioSource) {
              audioSource.Stop();
              audioSource = M2lev5audioSource;
              audioSource.Play();
            }
          } else if (HRvalue > zone4Start && HRvalue <= zone5Start) // Zone4
          {
            // Level 3
            //  Check Music Level Playing

            if (audioSource != M2lev2audioSource) {
              audioSource.Stop();
              audioSource = M2lev2audioSource;
              audioSource.Play();
            }
          } else if (HRvalue > zone5Start && HRvalue <= maxHRPossible) // Zone5
          {
            // Level 2 and Ask to Stop
            //  Check Music Level Playing

            if (audioSource != M2lev1audioSource) {
              audioSource.Stop();
              audioSource = M2lev1audioSource;
              audioSource.Play();
            }
          }
        }
      } else {
        // play cool_off music
        if (audioSource != cool_off_music) {
          Debug.Log("else playing music is: " + audioSource.clip.ToString());
          audioSource.Stop();
          audioSource = cool_off_music;
          audioSource.Play();
        }
      }
    }
  }

  public void Workoutstop() {
    Debug.Log("workoutStop is called");
    StopWorkout = true;
  }
  public void ValueChangeCheck() {
    audioSource.volume = (volumeSlider.value) / 4;
  }
}