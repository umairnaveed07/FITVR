using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class GridManager : MonoBehaviour {
  public float x_Start, y_Start, x_Start1, y_Start1;
  public int collength, rowlength;
  public float x_Space, y_Space;
  public GameObject cubes;
  public LayerMask layer;
  public AudioSource soundEffect;
  public XRBaseController xr;

  bool isCreated = false;

  bool isCreated1 = false;
  public GameObject Spawner;
  public GameObject Spawner2;

  // Start is called before the first frame update
  void Start() { StartCoroutine(replaygame()); }

  IEnumerator replaygame() {
    yield return new WaitForSeconds(20);
    for (int i = 0; i < 10; i++) {
      PhotonNetwork.Destroy(GameObject.FindWithTag("Cubes1"));
    }
    isCreated = false;
    isCreated1 = false;
    GameObject.Find("Spawner").GetComponent<Spawnerr>().enabled = true;
    GameObject.Find("Spawner2").GetComponent<GridManager>().enabled = false;
  }

  // Update is called once per frame
  void Update() {
    if (PhotonNetwork.CurrentRoom.PlayerCount <= 1) {
      if (!isCreated) {
        for (int i = 0; i < collength * rowlength; i++) {
          PhotonNetwork.Instantiate(
              "Redtest",
              new Vector3(x_Start + (x_Space * (i % collength)),
                          y_Start + ((byte)(y_Space * (i / collength))), 1f),
              Quaternion.identity);
        }
        isCreated = true;
      }
    }
    if (PhotonNetwork.CurrentRoom.PlayerCount > 1) {
      if (!isCreated1) {
        for (int i = 0; i < collength * rowlength; i++) {
          PhotonNetwork.Instantiate(
              "Redtest",
              new Vector3(x_Start1 + (x_Space * (i % collength)),
                          y_Start1 + ((byte)(y_Space * (i / collength))), 1f),
              Quaternion.identity);
        }
        isCreated1 = true;
      }
    }
  }
}
