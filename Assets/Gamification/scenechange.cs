using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class scenechange : MonoBehaviour {
  public void btn_change_screen() {
    SceneManager.LoadScene("main scene", LoadSceneMode.Additive);
  }
}
