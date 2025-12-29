using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Deals with the Tabs at the Top of History Dashboard
 */
public class MyTab : MonoBehaviour {
  public GameObject tabbutton1;
  public GameObject tabbutton2;
  public GameObject tabbutton3;
  public GameObject tabbutton4;

  public GameObject tabcontent1;
  public GameObject tabcontent2;
  public GameObject tabcontent3;
  public GameObject tabcontent4;

  public GameObject noofsetsgraph;
  public GameObject caloriesburntgraph;
  public GameObject maxHRgraph;
  public GameObject avgHRgraph;

  public Piechart_History ph;
  public Piechart_CaloriesHistory calories_history;
  // Start is called before the first frame update
  void Start() {
    HideAllTabs();
    tabcontent1.SetActive(true);
    ph = new Piechart_History();
    calories_history = new Piechart_CaloriesHistory();
  }

  // Update is called once per frame
  void Update() {}
  public void HideAllTabs() {

    noofsetsgraph.SetActive(false);
    caloriesburntgraph.SetActive(false);
    maxHRgraph.SetActive(false);
    avgHRgraph.SetActive(false);
    tabcontent1.SetActive(false);
    tabcontent2.SetActive(false);
    tabcontent3.SetActive(false);
    tabcontent4.SetActive(false);
  }
  public void ShowTab1() {
    HideAllTabs();
    tabcontent1.SetActive(true);
    noofsetsgraph.SetActive(true);
    ph.NoofSetsGraph();
  }
  public void ShowTab2() {
    HideAllTabs();
    // HideAllTabs();
    tabcontent2.SetActive(true);
    calories_history.CaloriesGraph();
    caloriesburntgraph.SetActive(true);
  }

  public void ShowTab3() {
    HideAllTabs();
    // HideAllTabs();
    tabcontent3.SetActive(true);
    ph.AvgHRGraph();
    avgHRgraph.SetActive(true);
  }
  public void ShowTab4() {
    HideAllTabs();
    // HideAllTabs();
    tabcontent4.SetActive(true);
    ph.MaxHRGraph();
    maxHRgraph.SetActive(true);
  }
}
