using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour {

  public GameObject HexDumbell1, HexDumbell2, HexDumbell3, HexDumbell4,
      HexDumbell5, HexDumbell6, HexDumbell7, HexDumbell8, HexDumbell9,
      HexDumbell10, HexDumbell11;
  public GameObject Dumbell1, Dumbell2, Dumbell3, Dumbell4, Dumbell5, Dumbell6,
      Dumbell7, Dumbell8, Dumbell9, Dumbell10, Dumbell11;
  public GameObject HexDumbell12, HexDumbell13, HexDumbell14, HexDumbell15,
      HexDumbell16, HexDumbell17, HexDumbell18, HexDumbell19, HexDumbell20,
      HexDumbell21, HexDumbell22;
  public GameObject Dumbell12, Dumbell13, Dumbell14, Dumbell15, Dumbell16,
      Dumbell17, Dumbell18, Dumbell19, Dumbell20, Dumbell21, Dumbell22;

  public void RespawnDumbbells() {
    HexDumbell1.transform.localPosition =
        new Vector3(-1.372526f, 0, -41.53893f);
    HexDumbell2.transform.localPosition =
        new Vector3(-0.8674024f, 0f, -34.11783f);
    HexDumbell3.transform.localPosition =
        new Vector3(-0.8674024f, 0f, -26.3112f);
    HexDumbell4.transform.localPosition = new Vector3(0f, 0f, -18.21546f);
    HexDumbell5.transform.localPosition = new Vector3(0f, 0f, -9.58963f);
    HexDumbell6.transform.localPosition = new Vector3(0f, 0f, 0f);
    HexDumbell7.transform.localPosition = new Vector3(0f, 0f, 9.637793f);
    HexDumbell8.transform.localPosition = new Vector3(0f, 0.337323f, 19.66112f);
    HexDumbell9.transform.localPosition = new Vector3(0f, 0.337323f, 29.44349f);
    HexDumbell10.transform.localPosition =
        new Vector3(-0.16f, 0.34f, 40.09325f);
    // HexDumbell11.transform.localPosition = new Vector3(-0.1599992f,
    // 0.340002f, 51.46585f);

    HexDumbell1.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell2.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell3.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell4.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell5.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell6.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell7.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell8.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell9.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell10.transform.localRotation = new Quaternion(0, 0, 0, 0);
    // HexDumbell11.transform.localRotation = new Quaternion(0, 0, 0, 0);
    //
    Dumbell1.transform.localPosition =
        new Vector3(-23.74871f, -0.8767584f, -41.28389f);
    Dumbell2.transform.localPosition =
        new Vector3(-23.74871f, -0.3811995f, -33.43119f);
    Dumbell3.transform.localPosition =
        new Vector3(-23.74871f, -0.3811995f, -25.31163f);
    Dumbell4.transform.localPosition =
        new Vector3(-23.74871f, -0.3811995f, -17.23021f);
    Dumbell5.transform.localPosition =
        new Vector3(-23.74871f, -0.3811995f, -8.424511f);
    Dumbell6.transform.localPosition =
        new Vector3(-23.74871f, -0.03812001f, 0.1524687f);
    Dumbell7.transform.localPosition =
        new Vector3(-23.74871f, -0.03812001f, 9.377495f);
    Dumbell8.transform.localPosition =
        new Vector3(-23.74871f, -0.03812001f, 19.1362f);
    Dumbell9.transform.localPosition =
        new Vector3(-23.74871f, -0.03812001f, 29.54293f);
    Dumbell10.transform.localPosition =
        new Vector3(-23.74871f, -0.03812001f, 40.21651f);
    Dumbell11.transform.localPosition =
        new Vector3(-23.74871f, -0.03812001f, 51.19506f);

    Dumbell1.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell2.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell3.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell4.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell5.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell6.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell7.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell8.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell9.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell10.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell11.transform.localRotation = new Quaternion(0, 180, 0, 0);
    //
    HexDumbell12.transform.localPosition =
        new Vector3(-1.372526f, 0f, -41.53893f);
    HexDumbell13.transform.localPosition =
        new Vector3(-0.8674024f, 0f, -34.11783f);
    HexDumbell14.transform.localPosition =
        new Vector3(-0.8674024f, 0f, -26.3112f);
    HexDumbell15.transform.localPosition = new Vector3(0f, 0f, -18.21546f);
    HexDumbell16.transform.localPosition = new Vector3(0f, 0f, -9.58963f);
    HexDumbell17.transform.localPosition = new Vector3(0f, 0f, 0f);
    HexDumbell18.transform.localPosition = new Vector3(0f, 0f, 9.637793f);
    HexDumbell19.transform.localPosition =
        new Vector3(0f, 0.337323f, 19.66112f);
    HexDumbell20.transform.localPosition =
        new Vector3(0f, 0.337323f, 29.44349f);
    HexDumbell21.transform.localPosition =
        new Vector3(-0.16f, 0.34f, 40.09325f);
    // HexDumbell22.transform.localPosition = new Vector3(-0.1599992f,
    // 0.340002f, 51.46585f);

    HexDumbell12.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell13.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell14.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell15.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell16.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell17.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell18.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell19.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell20.transform.localRotation = new Quaternion(0, 0, 0, 0);
    HexDumbell21.transform.localRotation = new Quaternion(0, 0, 0, 0);
    // HexDumbell22.transform.localRotation = new Quaternion(0, 0, 0, 0);
    //
    Dumbell12.transform.localPosition =
        new Vector3(-23.74871f, -0.8767584f, -41.28389f);
    Dumbell13.transform.localPosition =
        new Vector3(-23.74871f, -0.3811995f, -33.43119f);
    Dumbell14.transform.localPosition =
        new Vector3(-23.74871f, -0.3811995f, -25.31163f);
    Dumbell15.transform.localPosition =
        new Vector3(-23.74871f, -0.3811995f, -17.23021f);
    Dumbell16.transform.localPosition =
        new Vector3(-23.74871f, -0.3811995f, -8.424511f);
    Dumbell17.transform.localPosition =
        new Vector3(-23.74871f, -0.03812001f, 0.1524687f);
    Dumbell18.transform.localPosition =
        new Vector3(-23.74871f, -0.03812001f, 9.377495f);
    Dumbell19.transform.localPosition =
        new Vector3(-23.74871f, -0.03812001f, 19.1362f);
    Dumbell20.transform.localPosition =
        new Vector3(-23.74871f, -0.03812001f, 29.54293f);
    Dumbell21.transform.localPosition =
        new Vector3(-23.74871f, -0.03812001f, 40.21651f);
    Dumbell22.transform.localPosition =
        new Vector3(-23.74871f, -0.03812001f, 51.19506f);

    Dumbell12.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell13.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell14.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell15.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell16.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell17.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell18.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell19.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell20.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell21.transform.localRotation = new Quaternion(0, 180, 0, 0);
    Dumbell22.transform.localRotation = new Quaternion(0, 180, 0, 0);
  }
}
