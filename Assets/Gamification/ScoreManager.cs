using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon;

public class ScoreManager : MonoBehaviourPunCallbacks {

  public static ScoreManager instance;

  public Text scoreText;
  public Text highscoreText;
  public int streakcount = 0;
  public GameObject FloatingText;
  public GameObject FloatingTextSpawnPoint;

  int score = 0;
  int highscore = 0;
  int flag = 0;
  // public int flagdodge = 0;

  public void Awake() { instance = this; }

  // Start is called before the first frame update
  void Start() {

    scoreText.text = score.ToString();

    this.highscore = PlayerPrefs.GetInt("highscore", 0);
    this.highscoreText.text = "HIGHSCORE: " + highscore.ToString();
  }

  // function to add points to players score
  public void AddPoint() {
    score += 10;
    HighScore();

    photonView.RPC("SynchronizeScoreboard", Photon.Pun.RpcTarget.AllBuffered,
                   this.score);
  }
  // function to remove points from players score
  public void RemovePoint() {
    score -= 5;
    if (score < 0) {
      score = 0;
    } else {
      HighScore();
    }

    photonView.RPC("SynchronizeScoreboard", Photon.Pun.RpcTarget.AllBuffered,
                   this.score);
  }

  // function to reset the players points to 0
  public void ResetPoint() {
    score = 0;

    photonView.RPC("SynchronizeScoreboard", Photon.Pun.RpcTarget.AllBuffered,
                   this.score);
  }

  // function to add players points once the dodge has been successfully dodged
  public void AddPointDodge() {
    score += 5;
    HighScore();

    photonView.RPC("SynchronizeScoreboard", Photon.Pun.RpcTarget.AllBuffered,
                   this.score);
  }

  public void RemovePointDodge() {
    score -= 5;
    if (score < 0) {
      score = 0;
    } else {
      HighScore();
    }

    photonView.RPC("SynchronizeScoreboard", Photon.Pun.RpcTarget.AllBuffered,
                   this.score);
  }

  // function to displaye score multiplayer
  public string DisplayScoreMultiplayer() {
    scoreText.text = score.ToString();
    return scoreText.text;
  }

  // photon RPC to synchroniye the scoreboard
  [PunRPC]
  void SynchronizeScoreboard(int currentScore) {

    if (PhotonNetwork.IsMasterClient == false) {
      this.score = currentScore;
    }

    this.scoreText.text = score.ToString();
  }

  public int GetScoreValue() { return this.score; }
  // photon RPC to synchronize the highscore
  [PunRPC]
  void SynchronizeHighscore(int currentHighscore) {

    if (PhotonNetwork.IsMasterClient == false) {
      this.highscore = currentHighscore;
    }
  }

  public void HighScore() {
    if (highscore < score)
      PlayerPrefs.SetInt("highscore", score);

    photonView.RPC("SynchronizeScoreboard", Photon.Pun.RpcTarget.AllBuffered,
                   this.score);
  }

  public void scoreAddMole50() {
    score += 50;
    HighScore();

    photonView.RPC("SynchronizeScoreboard", Photon.Pun.RpcTarget.AllBuffered,
                   this.score);
  }

  public void scoreAddMole100() {
    score += 100;
    HighScore();

    photonView.RPC("SynchronizeScoreboard", Photon.Pun.RpcTarget.AllBuffered,
                   this.score);
  }

  // photon RPC to synchronize the score streak
  [PunRPC]
  void SynchronizeStreak(int countstreak) {

    if (PhotonNetwork.IsMasterClient == false) {
      this.streakcount = countstreak;
    }
  }

  // function to trigger the streakcount
  public void CountStreak() {
    streakcount += 1;
    photonView.RPC("SynchronizeStreak", Photon.Pun.RpcTarget.AllBuffered,
                   this.streakcount);
    Debug.Log("Countstreak function called!!!! Count is now " + streakcount);
    if (streakcount <= 2) {
      flag = 0;
    } else if (streakcount == 3) {
      flag = 1;
      score += 3;
      scoreText.text = score.ToString();
      HighScore();
      if (FloatingText) {
        ShowFloatingText3();
      }
      Debug.Log("Value of streakcount is  here in if loop " + streakcount);
    } else if (streakcount == 4) {
      flag = 2;
      score += 4;

      scoreText.text = score.ToString();
      HighScore();
      if (FloatingText) {
        ShowFloatingText4();
      }
      Debug.Log("Value of streakcount is  here in if loop " + streakcount);
    } else if (streakcount == 5) {
      flag = 3;
      score += 5;

      scoreText.text = score.ToString();
      HighScore();
      if (FloatingText) {
        ShowFloatingText5();
      }
      Debug.Log("Value of streakcount is  here in if loop " + streakcount);
    } else if (streakcount == 6) {
      flag = 4;
      score += 6;

      scoreText.text = score.ToString();
      HighScore();
      if (FloatingText) {
        ShowFloatingText6();
      }
      Debug.Log("Value of streakcount is  here in if loop " + streakcount);
    } else if (streakcount == 7) {
      flag = 5;
      score += 7;

      scoreText.text = score.ToString();
      HighScore();
      if (FloatingText) {
        ShowFloatingText7();
      }
      Debug.Log("Value of streakcount is  here in if loop " + streakcount);
    } else if (streakcount == 8) {
      flag = 6;
      score += 8;

      scoreText.text = score.ToString();
      HighScore();
      if (FloatingText) {
        ShowFloatingText8();
      }
      Debug.Log("Value of streakcount is  here in if loop " + streakcount);
    } else if (streakcount == 9) {
      flag = 7;
      score += 9;

      scoreText.text = score.ToString();
      HighScore();
      if (FloatingText) {
        ShowFloatingText9();
      }
      Debug.Log("Value of streakcount is  here in if loop " + streakcount);
    } else if (streakcount == 10) {
      flag = 8;
      score += 10;

      scoreText.text = score.ToString();
      HighScore();
      if (FloatingText) {
        ShowFloatingText10();
      }
      Debug.Log("Value of streakcount is  here in if loop " + streakcount);
    } else if (streakcount > 10) {
      flag = 9;
      score += 20;

      scoreText.text = score.ToString();
      HighScore();
      if (FloatingText) {
        ShowFloatingText11();
      }
      Debug.Log("Value of streakcount is  here in if loop " + streakcount);
    }
  }

  public void StreakReset() {
    streakcount = 0;
    photonView.RPC("SynchronizeStreak", Photon.Pun.RpcTarget.AllBuffered,
                   this.streakcount);
    Debug.Log("Countstreak has been reset because you missed ");
  }

  public void ShowFloatingText3() {
    GameObject cube = Instantiate(FloatingText);
    cube.transform.position = FloatingTextSpawnPoint.transform.position;
    Debug.Log("Float txt for 3 successful score inside new script running");
    cube.GetComponent<TextMeshPro>().text = "+ 3";
  }
  public void ShowFloatingText4() {
    GameObject cube = Instantiate(FloatingText);
    cube.transform.position = FloatingTextSpawnPoint.transform.position;
    Debug.Log("Float txt for 4 successful score inside new script running");
    cube.GetComponent<TextMeshPro>().text = "+ 4";
  }
  public void ShowFloatingText5() {
    GameObject cube = Instantiate(FloatingText);
    cube.transform.position = FloatingTextSpawnPoint.transform.position;
    Debug.Log("Float txt for 5 successful score inside new script running");
    cube.GetComponent<TextMeshPro>().text = "+ 5";
  }
  public void ShowFloatingText6() {
    GameObject cube = Instantiate(FloatingText);
    cube.transform.position = FloatingTextSpawnPoint.transform.position;
    Debug.Log("Float txt for 6 successful score inside new script running");
    cube.GetComponent<TextMeshPro>().text = "+ 6";
  }
  public void ShowFloatingText7() {
    GameObject cube = Instantiate(FloatingText);
    cube.transform.position = FloatingTextSpawnPoint.transform.position;
    Debug.Log("Float txt for 7 successful score inside new script running");
    cube.GetComponent<TextMeshPro>().text = "+ 7";
  }
  public void ShowFloatingText8() {
    GameObject cube = Instantiate(FloatingText);
    cube.transform.position = FloatingTextSpawnPoint.transform.position;
    Debug.Log("Float txt for 8 successful score inside new script running");
    cube.GetComponent<TextMeshPro>().text = "+ 8";
  }
  public void ShowFloatingText9() {
    GameObject cube = Instantiate(FloatingText);
    cube.transform.position = FloatingTextSpawnPoint.transform.position;
    Debug.Log("Float txt for 9 successful score inside new script running");
    cube.GetComponent<TextMeshPro>().text = "+ 9";
  }
  public void ShowFloatingText10() {
    GameObject cube = Instantiate(FloatingText);
    cube.transform.position = FloatingTextSpawnPoint.transform.position;
    Debug.Log("Float txt for 10 successful score inside new script running");
    cube.GetComponent<TextMeshPro>().text = "+ 10";
  }
  public void ShowFloatingText11() {
    GameObject cube = Instantiate(FloatingText);
    cube.transform.position = FloatingTextSpawnPoint.transform.position;
    Debug.Log(
        "Float txt for more than 10 successful score inside new script running");
    cube.GetComponent<TextMeshPro>().text = "Score X 2";
  }
}
