/////////////////BarGraphExample.cs////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq.Expressions;
using BarGraph.VittorCloud;

public class BarGraphExample : MonoBehaviour
{
    public List<BarGraphDataSet> exampleDataSet; // public data set for inserting data into the bar graph
    BarGraphGenerator barGraphGenerator;

    [HideInInspector]
    public dbconn db;
    //[HideInInspector]
    //public sqlitetest1 up;
    [HideInInspector]
    public dataRetrivaldbconn dR;

    void Start()
    {
        db = new dbconn();

        //up = new sqlitetest1();
        //dR = new dataRetrivaldbconn();

        dR = db.getData();

        //dR = up.getData();

        barGraphGenerator = GetComponent<BarGraphGenerator>();


        //if the exampleDataSet list is empty then return.
        if (exampleDataSet.Count == 0)
        {

            Debug.LogError("ExampleDataSet is Empty!");
            return;
        }
        barGraphGenerator.GeneratBarGraph(exampleDataSet);

    }


    //call when the graph starting animation completed,  for updating the data on run time
    public void StartUpdatingGraph()
    {


        StartCoroutine(CreateDataSet());
    }



    IEnumerator CreateDataSet()
    {
        //  yield return new WaitForSeconds(3.0f);
        while (true)
        {

            GenerateRandomData();

            yield return new WaitForSeconds(2.0f);

        }

    }



    //Generates the random data for the created bars
    void GenerateRandomData()
    {

        //int dataSetIndex = UnityEngine.Random.Range(0, exampleDataSet.Count);
        //int xyValueIndex = UnityEngine.Random.Range(0, exampleDataSet[dataSetIndex].ListOfBars.Count);
        //exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue = UnityEngine.Random.Range(barGraphGenerator.yMinValue, barGraphGenerator.yMaxValue);
        //barGraphGenerator.AddNewDataSet(dataSetIndex, xyValueIndex, exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue);

        int userid = int.Parse(dR.idGet);
        int bicepcurlsExerciseId = 1;
        int frontraiseExerciseId = 2;
        int squatsExerciseId = 3;
        int jumpingJacksExerciseId = 4;

        List<double> setsForBicepCurls = db.setsGraphData(userid, bicepcurlsExerciseId); // Date-wise data arranged in list to be shown when bar is clicked (7 elements)
        List<double> setsForFrontRaise = db.setsGraphData(userid, frontraiseExerciseId);   // Date-wise data arranged in list to be shown when bar is clicked (7 elements)
        List<double> setsForSquats = db.setsGraphData(userid, squatsExerciseId);   // Date-wise data arranged in list to be shown when bar is clicked (7 elements)
        List<double> setsForJumpingJacks = db.setsGraphData(userid, jumpingJacksExerciseId);

        int dataSetIndex;
        int xyValueIndex;
        dataSetIndex = 0;
        xyValueIndex = 0;

        double bicepcurlsbarchartvalue = setsForBicepCurls[6];
        double frontraisebarchartvalue = setsForFrontRaise[6];
        double squatsbarchartvalue = setsForSquats[6];
        double jumpingjacksbarchartvalue = setsForJumpingJacks[6];


        for (int i = 0; i < 7; i++)
        {
            Debug.Log("Biecep sets for day " + i + " is:" + setsForBicepCurls[i]);
        }


        Debug.Log("bicepcurlsbarchartvalue:" + bicepcurlsbarchartvalue);
        Debug.Log("frontraisebarchartvalue:" + frontraisebarchartvalue);
        Debug.Log("squatsbarchartvalue:" + squatsbarchartvalue);
        Debug.Log("jumpingjacksbarchartvalue:" + jumpingjacksbarchartvalue);

        exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue = float.Parse(bicepcurlsbarchartvalue.ToString());
        barGraphGenerator.AddNewDataSet(dataSetIndex, xyValueIndex, exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue);
        xyValueIndex = 1;
        exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue = float.Parse(frontraisebarchartvalue.ToString());
        barGraphGenerator.AddNewDataSet(dataSetIndex, xyValueIndex, exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue);
        xyValueIndex = 2;
        exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue = float.Parse(squatsbarchartvalue.ToString());
        barGraphGenerator.AddNewDataSet(dataSetIndex, xyValueIndex, exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue);
        xyValueIndex = 3;
        exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue = float.Parse(jumpingjacksbarchartvalue.ToString());
        barGraphGenerator.AddNewDataSet(dataSetIndex, xyValueIndex, exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue);



    }
}



