using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon;

public class Saber : MonoBehaviour {
  // public GameObject FloatingText;
  public LayerMask layer;
  public AudioSource soundEffect;
  public XRBaseController xr;
  public int score = 0;

  // Start is called before the first frame update
  void Start() {}
  // destroy the game objects if the player hits the cube
  void DestroyObject(GameObject obj) {
    PhotonView pView = obj.GetComponent<PhotonView>();

    if (pView == null) {
      print(
          "Error tried to remove object without a photonview attached to it: " +
          obj.name);
      return;
    }

    /* if(pView.IsMine == false)
     {
         pView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
     }*/

    PhotonNetwork.Destroy(obj);
  }

  // Update to check if the player htis the cube or not
  void Update() {
    RaycastHit hit;

    bool rayHit = Physics.Raycast(transform.position, transform.forward,
                                  out hit, 0.5f, layer);

    if (rayHit == true) {
      GameObject hitObject = hit.collider.gameObject;

      if (hitObject.tag == "Cubes1") {
        // DestroyObject(hitObject);
        PhotonNetwork.Destroy(hitObject);

        soundEffect.Play();
        Debug.Log("HIT1");
        ScoreManager.instance.AddPoint();
        ScoreManager.instance.CountStreak();
      } else if (hitObject.tag == "Cubes") {
        DestroyObject(hitObject);

        soundEffect.Play();
        Debug.Log("HIT2");
        ScoreManager.instance.AddPoint();
        ScoreManager.instance.CountStreak();
      } else if (hitObject.tag == "Dodge") {
        DestroyObject(hitObject);
        ScoreManager.instance.RemovePointDodge();
        ScoreManager.instance.StreakReset();
        healthManager.healthRemove();
      } else if (hitObject.tag == "Target") {
        DestroyObject(hitObject.transform.parent.gameObject);
        soundEffect.Play();
        Debug.Log("TARGET HIT");
        ScoreManager.instance.AddPoint();
        ScoreManager.instance.CountStreak();
      } else if (hitObject.tag == "NotTarget") {
        DestroyObject(hitObject.transform.parent.gameObject);
        // soundEffect.Play();
        // ScoreManager.instance.RemovePoint();
        ScoreManager.instance.StreakReset();

      } else if (hitObject.tag == "Target2") {
        DestroyObject(hitObject.transform.parent.gameObject);
        soundEffect.Play();
        Debug.Log("TARGET2 HIT");
        ScoreManager.instance.AddPoint();
        ScoreManager.instance.CountStreak();
      } else if (hitObject.tag == "Health") {
        DestroyObject(hitObject);
        Debug.Log("Health hit");
        soundEffect.Play();
        healthManager.healthAdd();
      }
    }
  }
}
