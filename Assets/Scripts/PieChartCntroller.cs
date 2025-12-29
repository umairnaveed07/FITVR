using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
// create an object for dbconn and call the function defined in it get the
// calories burnt values
public class PieChartCntroller : MonoBehaviour {
  [HideInInspector]
  public dbconn db;
  [HideInInspector]
  public Piechart piechart;
  public DateTime date1;
  public double[] values;
  public int userid;
  void Start() { Calories_display(); }
  public void Calories_display() {
    userid = 3;
    values = new double[4];
    db = new dbconn();
    piechart = new Piechart();
    date1 = System.DateTime.Now;
    Debug.Log("Entered pie chart controller");
    values[0] = 10.5;
    values[1] = 20.8;
    values[2] = 30.9;
    values[3] = 40.9;
    piechart.caloriesburnt_values = values;
  }
}
