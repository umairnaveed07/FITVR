using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * This Class deals with Average HR bar in Daily Stats Scene
 */
public class Average_HeartRate : MonoBehaviour {
  [HideInInspector]
  public dbconn db;
  [HideInInspector]
  public userdata usd;
  [HideInInspector]
  public dataRetrivaldbconn dR;

  public double Avg_HR_value;
  public Text value;
  public Image AvgHR;
  private Slider avg_hr_slider;
  public int userid2;
  void Start() {
    db = new dbconn();
    dR = db.getData();
    userid2 = int.Parse(dR.idGet);
  }

  // Start is called before the first frame update
  void Awake() { avg_hr_slider = GetComponent<Slider>(); }

  // Update is called once per frame
  void Update() { GetHR__Avg_Value(); }

  public void GetHR__Avg_Value() {
    Avg_HR_value = db.avgHRoneday(userid2);
    Debug.Log("Average heart rate is" + Avg_HR_value);
    avg_hr_slider.value = (float)Avg_HR_value;
    value.text = Avg_HR_value.ToString();
  }
}
