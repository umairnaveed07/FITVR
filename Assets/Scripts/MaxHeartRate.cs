using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * This Class deals with Maximum HR bar in Daily Stats Scene
 */
public class MaxHeartRate : MonoBehaviour {
  public double Max_HR_value;
  public Text value;
  public Image HR;
  private Slider hr_slider;
  [HideInInspector]
  public dbconn db;
  [HideInInspector]
  public userdata usd_maxhr;
  [HideInInspector]
  public dataRetrivaldbconn dR;

  public int userid1;

  // Start is called before the first frame update
  void Start() {
    db = new dbconn();
    dR = db.getData();
    userid1 = int.Parse(dR.idGet);
  }
  void Awake() { hr_slider = GetComponent<Slider>(); }

  // Update is called once per frame
  void Update() { GetHR__Max_Value(); }

  void GetHR__Max_Value() {
    Max_HR_value = db.maxHRoneday(userid1); // usd_maxhr.userid;
    hr_slider.value = (float)Max_HR_value;
    value.text = Max_HR_value.ToString();
  }
}
