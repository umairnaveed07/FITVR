using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class wackamolehealth : MonoBehaviour {

  public enum TYPES { HEALTH = 0, FREMZY = 1, BONUS = 2 }
  ;

  private bool activated = true;
  public TYPES type = TYPES.HEALTH;

  public List<GameObject> bonuses;

  // function to check if the foot stomps on the circle

  private void OnTriggerEnter(Collider other) {

    if (other.tag != "foots") {
      return;
    }

    Debug.Log(" Hit the health stomp");

    if (type == TYPES.HEALTH) {
      bonuses[0].SetActive(false);

      healthManager.healthAddMole();
    } else if (type == TYPES.FREMZY) {
      bonuses[0].SetActive(false);
      ScoreManager.instance.scoreAddMole50();

      Debug.Log("stepped in frenzy");
    } else if (type == TYPES.BONUS) {
      bonuses[0].SetActive(false);

      ScoreManager.instance.scoreAddMole100();
    }
  }

  // function to check if the foot exited  the circle
  private void OnTriggerExit(Collider other) {
    Debug.Log(" exit the health stomp");
  }

  // Start is called before the first frame update
  void Start() {}

  // Update is called once per frame
  void Update() {}
}
