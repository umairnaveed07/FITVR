using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;
using System.Linq.Expressions;

public class Piechart_History : MonoBehaviour {
  public static int buttoncheck;
  public Image[] Pie_Charts;
  [HideInInspector]
  public DateTime date1;
  public int userid;
  public int exercise_id;
  public double[] piechart_values;
  [HideInInspector]
  public dbconn db;
  [HideInInspector]
  public userdata usd;
  [HideInInspector]
  public dataRetrivaldbconn dR;
  public Text bicep_curls_value;
  public Text squats_value;
  public Text FrontRaises_value;
  public Text jumpingjacks_value;
  public int bicepcurlsExerciseId;
  public int frontraiseExerciseId;
  public int squatsExerciseId;
  public int jumpingJacksExerciseId;

  /**
   * Controls 4-layer pie-charts for the History Scene
   */

  //  Start is called before the first frame update
  void Start() {
    bicepcurlsExerciseId = 1;
    frontraiseExerciseId = 2;
    squatsExerciseId = 3;
    jumpingJacksExerciseId = 4;
    db = new dbconn();
    usd = new userdata();
    dR = db.getData();
  }

  // Update is called once per frame
  void Update() { SetPieChartValues(piechart_values); }

  public void NoofSetsGraph() { buttoncheck = 1; }

  public void MaxHRGraph() { buttoncheck = 4; }

  public void AvgHRGraph() { buttoncheck = 3; }

  public void SetPieChartValues(double[] values) {
    if (buttoncheck == 1) {
      userid = int.Parse(dR.idGet);
      date1 = System.DateTime.Now;
      List<double> temp_values_sets = new List<double>();
      List<double> setsForBicepCurls =
          db.setsGraphData(userid, bicepcurlsExerciseId);
      List<double> setsForFrontRaise =
          db.setsGraphData(userid, frontraiseExerciseId);
      List<double> setsForSquats = db.setsGraphData(userid, squatsExerciseId);
      List<double> setsForJumpingJacks =
          db.setsGraphData(userid, jumpingJacksExerciseId);

      temp_values_sets.Add(setsForBicepCurls.Sum());
      temp_values_sets.Add(setsForFrontRaise.Sum());
      temp_values_sets.Add(setsForSquats.Sum());
      temp_values_sets.Add(setsForJumpingJacks.Sum());

      piechart_values = temp_values_sets.ToArray();
      values = temp_values_sets.ToArray();
    } else if (buttoncheck == 2) {

    } else if (buttoncheck == 3) {
      userid = int.Parse(dR.idGet);
      date1 = System.DateTime.Now;
      List<double> temp_values_avg = new List<double>();

      List<double> avgHRForBicepCurls =
          db.avgHRGraphData(userid, bicepcurlsExerciseId);
      List<double> avgHRForFrontRaise =
          db.avgHRGraphData(userid, frontraiseExerciseId);
      List<double> avgHRForSquats = db.avgHRGraphData(userid, squatsExerciseId);
      List<double> avgHRForJumpingJacks =
          db.avgHRGraphData(userid, jumpingJacksExerciseId);

      double nonzero = 0;
      if (avgHRForBicepCurls.Sum() != 0) {
        for (int i = 0; i < avgHRForBicepCurls.Count; i++) {
          if (avgHRForBicepCurls[i] != 0)
            nonzero++;
        }
        temp_values_avg.Add(avgHRForBicepCurls.Sum() / nonzero);
      } else
        temp_values_avg.Add(0);

      if (avgHRForFrontRaise.Sum() != 0) {
        nonzero = 0;
        for (int i = 0; i < avgHRForFrontRaise.Count; i++) {
          if (avgHRForFrontRaise[i] != 0)
            nonzero++;
        }
        temp_values_avg.Add(avgHRForFrontRaise.Sum() / nonzero);
      } else
        temp_values_avg.Add(0);

      if (avgHRForSquats.Sum() != 0) {
        nonzero = 0;
        for (int i = 0; i < avgHRForSquats.Count; i++) {
          if (avgHRForSquats[i] != 0)
            nonzero++;
        }
        temp_values_avg.Add(avgHRForSquats.Sum() / nonzero);
      } else
        temp_values_avg.Add(0);

      if (avgHRForJumpingJacks.Sum() != 0) {
        nonzero = 0;
        for (int i = 0; i < avgHRForJumpingJacks.Count; i++) {
          if (avgHRForJumpingJacks[i] != 0)
            nonzero++;
        }
        temp_values_avg.Add(avgHRForJumpingJacks.Sum() / nonzero);
      } else
        temp_values_avg.Add(0);

      piechart_values = temp_values_avg.ToArray();
      values = temp_values_avg.ToArray();
    } else if (buttoncheck == 4) {
      userid = int.Parse(dR.idGet);
      date1 = System.DateTime.Now;
      List<double> temp_values_maxhr = new List<double>();

      List<double> maxHRForBicepCurls =
          db.maxHRGraphData(userid, bicepcurlsExerciseId);
      List<double> maxHRForFrontRaise =
          db.maxHRGraphData(userid, frontraiseExerciseId);
      List<double> maxHRForSquats = db.maxHRGraphData(userid, squatsExerciseId);
      List<double> maxHRForJumpingJacks =
          db.maxHRGraphData(userid, jumpingJacksExerciseId);

      temp_values_maxhr.Add(maxHRForBicepCurls.Max());
      temp_values_maxhr.Add(maxHRForFrontRaise.Max());
      temp_values_maxhr.Add(maxHRForSquats.Max());
      temp_values_maxhr.Add(maxHRForJumpingJacks.Max());

      piechart_values = temp_values_maxhr.ToArray();
      values = temp_values_maxhr.ToArray();
    }

    bicep_curls_value.text = values[0].ToString();
    FrontRaises_value.text = values[1].ToString();
    squats_value.text = values[2].ToString();
    jumpingjacks_value.text = values[3].ToString();
    double value_in_image = 0;
    for (int i = 0; i < Pie_Charts.Length; i++) {
      Debug.Log("In Piechart function" + values[i]);
      value_in_image += Compute_Percentage(values, i);

      Pie_Charts[i].fillAmount = (float)value_in_image;
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
