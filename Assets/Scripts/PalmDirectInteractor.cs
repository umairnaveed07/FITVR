using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

public class PalmDirectInteractor : MonoBehaviour {
  private PhotonView photonView;

  public Transform leftPalm;
  public Transform rightPalm;

  private XRRig rig;

  // Start is called before the first frame update
  void Start() {
    photonView = GetComponent<PhotonView>();

    rig = FindObjectOfType<XRRig>();
  }

  // Update is called once per frame
  void Update() {
    if (photonView.IsMine) {
      rig.GetComponentsInChildren<XRRayInteractor>()[0].attachTransform =
          leftPalm;
      rig.GetComponentsInChildren<XRRayInteractor>()[1].attachTransform =
          rightPalm;
    }
  }
}
