using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisionDetect : MonoBehaviour {
  // Start is called before the first frame update
  void OnCollisionEnter() { Debug.Log("We hit something"); }
}
