using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour {
  private Animator animator;
  public AudioSource walkingAudio;

  // Start is called before the first frame update
  void Start() { animator = GetComponent<Animator>(); }

  // Update is called once per frame
  void Update() {
    if (animator.GetBool("isMoving") == true) {
      if (!walkingAudio.isPlaying) {
        walkingAudio.Play();
      }
    } else {
      walkingAudio.Pause();
    }
  }
}
