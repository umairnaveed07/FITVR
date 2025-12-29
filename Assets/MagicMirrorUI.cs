using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Class that updates the time and data in the UI
/// </summary>
public class MagicMirrorUI : MonoBehaviour {
  public Text timeData;
  public Text dateData;

  /// <summary>
  /// Updates the time and day for the UI
  /// </summary>
  void Update() {
    this.timeData.text = DateTime.Now.ToString("hh:mm tt");
    this.dateData.text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
  }
}
