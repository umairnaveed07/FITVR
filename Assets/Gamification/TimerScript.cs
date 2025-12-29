using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.Collections.Generic;
using System.Collections;
using System;
using System.Data;
using System.Linq;
using Mono.Data.Sqlite;
using System.IO;

using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon;

public class TimerScript : MonoBehaviourPunCallbacks {
  private static string connection;

  public static bool TimerOn = true;
  public static bool isPlayingGame;
  public static float max, avg;
  static int maxHRPossible;
  public static float zone1, zone2, zone3, zone4, zone5;
  public static float zone0Count, zone1Count, zone2Count, zone3Count,
      zone4Count, zone5Count;
  private upmDatabase dataPull = new upmDatabase();
  private dataRetrivalUPM data = new dataRetrivalUPM();
  int age;
  public float RoundTime;
  private float TimeLeft;
  public TMP_Text TimerTxt;
  int playingMode;
  [HideInInspector]
  public static List<float> HR_Data = new List<float>();
  public int flag;

  void Start() {

    TimeLeft = RoundTime;
    TimerOn = true;
    flag = 0;
    changePlayingStatus(true);
    exerGamesDbConnection();

    dataPull.dbconnection();
    dataPull.getLatestEnteredID();

    playingMode = 1;
    zone0Count = 0;
    zone1Count = 0;
    zone2Count = 0;
    zone3Count = 0;
    zone4Count = 0;
    zone5Count = 0;
    data = dataPull.getData();
    int age = 26;
    // int age = int.Parse(data.ageGet);
    Debug.Log("age is " + age);
    maxHRPossible = 220 - age;

    zone1 = (float)0.5 * maxHRPossible;
    zone2 = (float)0.6 * maxHRPossible;
    zone3 = (float)0.7 * maxHRPossible;
    zone4 = (float)0.8 * maxHRPossible;
    zone5 = (float)0.9 * maxHRPossible;
  }

  // update script to check the time left and do functions if the time is 0 or
  // not
  void Update() {
    if (PauseMenu.GameIsPaused == false) {
      if (TimerOn) {
        changePlayingStatus(true);

        if (TimeLeft > 0) {
          TimeLeft -= Time.deltaTime;
          updateTimer(TimeLeft);
        }

        else {
          Debug.Log("Time is UP!");
          TimeLeft = 0;

          theGreatReset();
          changePlayingStatus(false);
        }
      }
    }
  }

  // function to update the timer per sec

  void updateTimer(float currentTime) {
    currentTime += 1;

    float minutes = Mathf.FloorToInt(currentTime / 60);
    float seconds = Mathf.FloorToInt(currentTime % 60);

    TimerTxt.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    photonView.RPC("SynchronizeTime", Photon.Pun.RpcTarget.AllBuffered,
                   this.TimeLeft);
  }

  // function to synchroniye the timer for collaboration mode
  [PunRPC]
  void SynchronizeTime(float syncTime) {

    if (PhotonNetwork.IsMasterClient == false) {
      this.TimeLeft = syncTime;
    }

    float minutes = Mathf.FloorToInt(TimeLeft / 60);
    float seconds = Mathf.FloorToInt(TimeLeft % 60);

    TimerTxt.text = string.Format("{0:00}:{1:00}", minutes, seconds);

    // this.TimerTxt.text = TimeLeft.ToString("F2");
  }

  // function to reset everything once the game ends and timer stops

  public void theGreatReset() {
    Debug.Log("Great Reset Activated");
    GameObject.Find("Spawner").GetComponent<Spawnerr>().enabled = false;
    PauseMenu.instance.ShowGameOver(ScoreManager.instance.GetScoreValue());
  }

  public void Restart() { this.TimeLeft = RoundTime; }
  // function to enter the data into the database once the timer hits 0
  public void exerDataEnter() {
    if (PhotonNetwork.IsMasterClient) {
      if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
        int dbScore = ScoreManager.instance.GetScoreValue();
        string connection =
            "URI=file:" + Application.persistentDataPath + "/" + "UPMdatabase";
        Debug.Log("Test 1");
        using (IDbConnection dbcon = new SqliteConnection(connection)) {
          Debug.Log("Test 2");
          dbcon.Open();
          Debug.Log("Test 3");
          using (IDbCommand dbcmd = dbcon.CreateCommand()) {
            Debug.Log("Test 4");
            data = dataPull.getData();
            Debug.Log("Test 5");

            int exerID = int.Parse(data.idGet);
            if (flag == 0) {
              string dataQuery = String.Format(
                  "INSERT INTO exerGamesUpdate1 ( id, heartAvg, heartMax, zone0, zone1, zone2, zone3, zone4, zone5, playingMode, score) VALUES ( '" +
                  exerID + "', '" + avg + "', '" + max + "', '" + zone0Count +
                  "', '" + zone1Count + "','" + zone2Count + "','" +
                  zone3Count + "','" + zone4Count + "','" + zone5Count + "','" +
                  playingMode + "','" + dbScore + "')");
              dbcmd.CommandText = dataQuery;
              Debug.Log("Inserted in table");
              dbcmd.ExecuteScalar();
              dbcon.Close();
            }
          }
        }
      }
      if (PhotonNetwork.CurrentRoom.PlayerCount == 2) {
        playingMode = 2;
        int dbScore = ScoreManager.instance.GetScoreValue();
        string connection =
            "URI=file:" + Application.persistentDataPath + "/" + "UPMdatabase";
        Debug.Log("Test 1");
        using (IDbConnection dbcon = new SqliteConnection(connection)) {
          Debug.Log("Test 2");
          dbcon.Open();
          Debug.Log("Test 3");
          using (IDbCommand dbcmd = dbcon.CreateCommand()) {
            Debug.Log("Test 4");
            data = dataPull.getData();
            Debug.Log("Test 5");

            int exerID = int.Parse(data.idGet);
            if (flag == 0) {
              string dataQuery = String.Format(
                  "INSERT INTO exerGamesUpdate1 ( id, heartAvg, heartMax, zone0, zone1, zone2, zone3, zone4, zone5, playingMode, score) VALUES ( '" +
                  exerID + "', '" + avg + "', '" + max + "', '" + zone0Count +
                  "', '" + zone1Count + "','" + zone2Count + "','" +
                  zone3Count + "','" + zone4Count + "','" + zone5Count + "','" +
                  playingMode + "','" + dbScore + "')");

              dbcmd.CommandText = dataQuery;
              Debug.Log("Inserted in table");

              dbcmd.ExecuteScalar();
              dbcon.Close();
            }
          }
        }
      }
    }
  }

  // function to get the DB connection for UPM
  public void exerGamesDbConnection() {

    // Create database
    string connection =
        "URI=file:" + Application.persistentDataPath + "/" + "UPMdatabase";
    Debug.Log(connection);
    // Open connection
    IDbConnection dbcon = new SqliteConnection(connection);
    dbcon.Open();

    // Create table
    IDbCommand dbcmd;
    dbcmd = dbcon.CreateCommand();
    string q_createTable =
        "CREATE TABLE IF NOT EXISTS exerGamesUpdate1 (id INT, heartAvg FLOAT, heartMax FLOAT, zone0 FLOAT,  zone1 FLOAT, zone2 FLOAT, zone3 FLOAT, zone4 FLOAT, zone5 FLOAT, playingMode INT, score, FOREIGN KEY (id) REFERENCES userInformation(id))";

    dbcmd.CommandText = q_createTable;
    dbcmd.ExecuteReader();

    // Close connection
    dbcon.Close();
  }

  public void updateFlag() {
    flag = 1;
    Debug.Log("update flag" + flag);
    TimerOn = false;
  }
  public void resetFlag() {
    flag = 0;
    TimerOn = true;
  }

  public void zoneReset() {
    zone0Count = 0;
    zone1Count = 0;
    zone2Count = 0;
    zone3Count = 0;
    zone4Count = 0;
    zone5Count = 0;
  }
  // function for heart rate max from the DMS team
  public double Compute_MAX_HR(List<float> data1) {
    max = data1.Max();
    print("the max value is" + max);
    return max;
  }
  // function for heart rate values from the DMS team
  public void Get_HR_Value(float new_value) {
    print("new value inside gethrvalue is" + new_value);

    if (isPlayingGame) // false
    {
      Debug.Log("Adding to List : " + new_value);
      HR_Data.Add(new_value);
      Debug.Log("Zone 1 is: " + zone1Count + " and zone 0 is " + zone0Count);
      //////////////////////////////////////////////////////////
      if (new_value < zone1) //<Zone1
      {
        zone0Count++;
        Debug.Log("Zone 0 is: " + zone0Count);
      } else if (new_value > zone1 && new_value <= zone2) // Zone1
      {
        zone1Count++;
        Debug.Log("Zone 1 is: " + zone1Count);
      } else if (new_value > zone2 && new_value <= zone3) // Zone2
      {
        zone2Count++;
      } else if (new_value > zone3 && new_value <= zone4) // Zone3
      {
        zone3Count++;
      } else if (new_value > zone4 && new_value <= zone5) // Zone4
      {
        zone4Count++;
      } else if (new_value > zone5 && new_value <= maxHRPossible) // Zone5
      {
        zone5Count++;
      }
    }
  }
  // function to change the playing status and get the hear rate values from the
  // sensor
  public void changePlayingStatus(bool status) {
    float total;
    TimerOn = status;
    isPlayingGame = status;
    Debug.Log("isPlayingGame : " + isPlayingGame);

    if (!status) {

      try {

        avg = HR_Data.Average();
        max = HR_Data.Max();
        Debug.Log("Average is : " + avg);
        Debug.Log("Max is : " + max);
        total = zone0Count + zone1Count + zone2Count + zone3Count + zone4Count +
                zone5Count;
        zone0Count = (zone0Count / (total)) * 100;
        zone1Count = (zone1Count / (total)) * 100;
        zone2Count = (zone2Count / (total)) * 100;
        zone3Count = (zone3Count / (total)) * 100;
        zone4Count = (zone4Count / (total)) * 100;
        zone5Count = (zone5Count / (total)) * 100;

        exerDataEnter();
      } catch (Exception e) {
        Debug.Log("here");
        print(e);
      }
    }
  }
}