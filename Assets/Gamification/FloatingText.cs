using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour {
  // public float DestroyTime = 0.001f;
  public static FloatingText instance;
  // Start is called before the first frame update
  void Start() { Destroy(gameObject, 0.5f); }

  private void Update() {
    float moveYSpeed = 10f;
    transform.position += new Vector3(0, 0, -moveYSpeed) * Time.deltaTime;
  }
}
