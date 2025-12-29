using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/**
 * Controls the Pie Charts with 4 layers
 */
public class Piechart : MonoBehaviour {

  public Image[] PieCharts;
  [HideInInspector]
  public DateTime date1;
  public int userid;
  public double[] caloriesburnt_values;
  [HideInInspector]
  public dbconn db;
  [HideInInspector]
  public userdata usd_maxhr;
  [HideInInspector]
  public dataRetrivaldbconn dR;
  public Text bicep_curls_value;
  public Text squats_value;
  public Text FrontRaises_value;
  public Text jumpingjacks_value;

  PieChartCntroller piecont;
  // Start is called before the first frame update
  void Start() {
    db = new dbconn();
    dR = db.getData();
    piecont = new PieChartCntroller();
  }

  // Update is called once per frame
  void Update() { SetPieChartValues(caloriesburnt_values); }

  public void SetPieChartValues(double[] values) {
    userid = int.Parse(dR.idGet);
    date1 = System.DateTime.Now;
    List<double> temp_values = new List<double>();
    temp_values = db.caloriesGraphData(userid, date1);
    values = temp_values.ToArray();

    bicep_curls_value.text = values[0].ToString();
    FrontRaises_value.text = values[1].ToString();
    squats_value.text = values[2].ToString();
    jumpingjacks_value.text = values[3].ToString();
    double value_in_image = 0;
    for (int i = 0; i < PieCharts.Length; i++) {
      Debug.Log("In Piechart function" + values[i]);
      value_in_image += Compute_Percentage(values, i);
      PieCharts[i].fillAmount = (float)value_in_image;
      Debug.Log("Piechart function values" + (float)value_in_image);
    }
  }

  public double Compute_Percentage(double[] values, int index) {
    double total_calories_burnt = 0;
    for (int i = 0; i < values.Length; i++) {
      total_calories_burnt += values[i];
    }
    return values[index] / total_calories_burnt;
  }
}