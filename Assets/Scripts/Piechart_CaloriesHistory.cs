using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Linq.Expressions;

/**
 * Controls the Pie Charts for Calories Burned with 7 layers
 */

public class Piechart_CaloriesHistory : MonoBehaviour {

  public Image[] PieCharts_calories;
  [HideInInspector]
  public DateTime date1;
  public int userid;

  public double[] caloriesburnt_values;
  [HideInInspector]
  public dbconn db;
  [HideInInspector]
  public userdata usd;
  [HideInInspector]
  public dataRetrivaldbconn dR;
  public Text Day1;
  public Text Day2;
  public Text Day3;
  public Text Day4;
  public Text Day5;
  public Text Day6;
  public Text Day7;

  PieChartCntroller piecont;
  // Start is called before the first frame update
  void Start() {
    db = new dbconn();
    dR = db.getData();
    piecont = new PieChartCntroller();
  }

  // Update is called once per frame
  void Update() { SetPieChartValues(caloriesburnt_values); }

  public void CaloriesGraph() {

    // SetPieChartValues(caloriesburnt_values);
  }

  public void SetPieChartValues(double[] values) {
    userid = int.Parse(dR.idGet);
    date1 = System.DateTime.Now;
    List<double> temp_values = new List<double>();

    List<double> calsForDay0 = db.caloriesGraphData(
        userid,
        DateTime.Now.Date.AddDays(-6)); // Exercise-wise values (4 elements)
    List<double> calsForDay1 = db.caloriesGraphData(
        userid,
        DateTime.Now.Date.AddDays(-5)); // Exercise-wise values (4 elements)
    List<double> calsForDay2 = db.caloriesGraphData(
        userid,
        DateTime.Now.Date.AddDays(-4)); // Exercise-wise values (4 elements)
    List<double> calsForDay3 = db.caloriesGraphData(
        userid,
        DateTime.Now.Date.AddDays(-3)); // Exercise-wise values (4 elements)
    List<double> calsForDay4 = db.caloriesGraphData(
        userid,
        DateTime.Now.Date.AddDays(-2)); // Exercise-wise values (4 elements)
    List<double> calsForDay5 = db.caloriesGraphData(
        userid,
        DateTime.Now.Date.AddDays(-1)); // Exercise-wise values (4 elements)
    List<double> calsForDay6 = db.caloriesGraphData(userid, DateTime.Now.Date);

    temp_values.Add(calsForDay0.Sum());
    temp_values.Add(calsForDay1.Sum());
    temp_values.Add(calsForDay2.Sum());
    temp_values.Add(calsForDay3.Sum());
    temp_values.Add(calsForDay4.Sum());
    temp_values.Add(calsForDay5.Sum());
    temp_values.Add(calsForDay6.Sum());

    caloriesburnt_values = temp_values.ToArray();
    Day1.text = values[0].ToString();
    Day2.text = values[1].ToString();
    Day3.text = values[2].ToString();
    Day4.text = values[3].ToString();
    Day5.text = values[4].ToString();
    Day6.text = values[5].ToString();
    Day7.text = values[6].ToString();

    double value_in_image = 0;
    for (int i = 0; i < PieCharts_calories.Length; i++) {
      Debug.Log("In Piechart function" + values[i]);
      value_in_image += Compute_Percentage(values, i);

      PieCharts_calories[i].fillAmount = (float)value_in_image;
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