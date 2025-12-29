////////////////////////dbconn/////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using System.Data;
using Mono.Data.Sqlite;
using System.Globalization;
using System.Linq;

public class dbconn : MonoBehaviour {
  private string dbName = "URI=file:" + Application.persistentDataPath + "/" +
                          "Statistics"; // Path for DB
  public double weight = 50;
  public double calories = 0;
  public double MET = 0;
  public double duration;
  public double maxHR;

  public static string conn;
  public dataRetrivaldbconn getUser = new dataRetrivaldbconn();
  public dataRetrivaldbconn dR;

  // Start is called before the first frame update
  void Start() {
    CreateDB(); // Creates the local database
    dR = getData();
    weight = double.Parse(dR.weightGet);
  }

  // Calculates Calories Burned at the end of exercise
  public double calculateCaloriesBurnt(int userid, int exercise_id,
                                       DateTime exercise_start,
                                       DateTime exercise_end) {
    duration = (exercise_end - exercise_start).TotalMinutes;

    using (var connection = new SqliteConnection(dbName)) {
      connection.Open();
      using (var command = connection.CreateCommand()) {
        command.CommandText =
            "SELECT MET_Value FROM exercise_info where exercise_id = '" +
            exercise_id + "';";
        using (IDataReader reader = command.ExecuteReader()) {
          while (reader.Read()) {
            MET = float.Parse(reader["MET_Value"].ToString());
            print("the met value is " + MET);
          }
          reader.Close();
        }
      }
      connection.Close();
    }

    calories = duration * (MET * 3.5 * weight) / 200; // Calories Burned Formula
    return calories;
  }

  // Creates a local database if not present
  public void CreateDB() {
    int count = 0;
    using (var connection = new SqliteConnection(dbName)) {
      Debug.Log("entered createdb part 1");
      connection.Open();
      using (var command = connection.CreateCommand()) {
        Debug.Log("entered createdb part 2");
        command.CommandText =
            "CREATE TABLE IF NOT EXISTS session_info (userid INT, exercise_id INT, exercise_start VARCHAR(35), exercise_end VARCHAR(35), No_of_sets INT, avg_HR FLOAT, max_HR FLOAT, calories_burnt FLOAT);";
        command.ExecuteNonQuery();
        command.CommandText =
            "CREATE TABLE IF NOT EXISTS exercise_info (exercise_id INT, exercise_name VARCHAR(35), MET_Value FLOAT,UNIQUE (exercise_id) ON CONFLICT IGNORE);";
        command.ExecuteNonQuery();
        command.CommandText = "SELECT COUNT(*) FROM exercise_info;";
        command.ExecuteNonQuery();
        using (IDataReader reader = command.ExecuteReader()) {
          while (reader.Read()) {
            count = int.Parse(reader[0].ToString());
          }

          reader.Close();
        }
        if (count == 0) {
          command.CommandText =
              "INSERT INTO exercise_info ( exercise_id,  exercise_name,  MET_Value) VALUES (1, 'Bicep Curls', 5.0);";
          command.ExecuteNonQuery();
          command.CommandText =
              "INSERT INTO exercise_info ( exercise_id,  exercise_name,  MET_Value) VALUES (2, 'Front Raises', 4.0);";
          command.ExecuteNonQuery();
          command.CommandText =
              "INSERT INTO exercise_info ( exercise_id,  exercise_name,  MET_Value) VALUES (3, 'Squats', 5.5);";
          command.ExecuteNonQuery();
          command.CommandText =
              "INSERT INTO exercise_info ( exercise_id,  exercise_name,  MET_Value) VALUES (4, 'Jumping Jacks', 7.7);";
          command.ExecuteNonQuery();
        }
      }

      connection.Close();
    }
  }

  // Adds a record into the database
  public void AddRecord(userdata o) {

    using (var connection = new SqliteConnection(dbName)) {
      connection.Open();
      using (var command = connection.CreateCommand()) {
        command.CommandText =
            "INSERT INTO session_info (userid, exercise_id, exercise_start, exercise_end, No_of_sets, avg_HR, max_HR, calories_burnt) VALUES ('" +
            o.userid + "', '" + o.exercise_id + "', '" + o.exercise_start +
            "', '" + o.exercise_end + "', '" + o.noOfSets + "', '" + o.avg_HR +
            "', '" + o.max_HR + "', '" + o.calories_burnt + "');";
        command.ExecuteNonQuery();
      }

      connection.Close();
    }
    DisplayRecord(o.userid);
  }

  // Displays in console for verifying
  public void DisplayRecord(int userid) {
    using (var connection = new SqliteConnection(dbName)) {
      connection.Open();
      using (var command = connection.CreateCommand()) {
        command.CommandText =
            "SELECT * FROM session_info s JOIN exercise_info e ON s.exercise_id = e.exercise_id WHERE s.userid = '" +
            userid + "'; ";
        using (IDataReader reader = command.ExecuteReader()) {
          while (reader.Read())
            Debug.Log("User ID: " + reader["userid"] +
                      "\nexercise_name: " + reader["exercise_name"] +
                      "\nexercise_start: " + reader["exercise_start"] +
                      "\nexercise_end: " + reader["exercise_end"] +
                      "\nNo_of_sets: " + reader["No_of_sets"] + "\navg_HR: " +
                      reader["avg_HR"] + "\nmax_HR: " + reader["max_HR"] +
                      "\nCalories Burnt: " + reader["calories_burnt"]);
          reader.Close();
        }
      }
      connection.Close();
    }
  }

  // Retrieves and adjusts data for Number of Sets Graph
  public List<double> setsGraphData(int userid, int exercise_id) {
    List<double> sets = new List<double>();
    string dateString;
    string check;

    using (var connection = new SqliteConnection(dbName)) {
      connection.Open();
      using (var command = connection.CreateCommand()) {

        for (int i = 0; i < 7; i++) {
          check = null;

          dateString = DateTime.Now.AddDays(-6 + i).ToString("yyyyMMdd");

          command.CommandText =
              "SELECT count(*) FROM session_info WHERE substr(REPLACE(exercise_start,' - ',''),0,9)  like '" +
              dateString + "' AND userid = '" + userid +
              "' and exercise_id = '" + exercise_id + "';";
          check = command.ExecuteScalar().ToString();
          if (check == "0") {
            sets.Add(0);
          } else {
            command.CommandText =
                "SELECT SUM(no_of_sets) FROM session_info WHERE substr(REPLACE(exercise_start,' - ',''),0,9)  like '" +
                dateString + "' AND userid = '" + userid +
                "' and exercise_id = '" + exercise_id + "';";
            double value = double.Parse(command.ExecuteScalar().ToString());
            sets.Add(value);
          }
        }
      }
      connection.Close();
    }
    Debug.Log("List size:" + sets.Count);
    for (int j = 0; j < 7; j++) {
      Debug.Log("List item [" + j + "] = " + sets[j].ToString());
    }
    return sets;
  }

  // Retrieves and adjusts data for Calories Burned Graph
  public List<double> caloriesGraphData(int userid, DateTime date) {
    List<double> cal = new List<double>();
    // public float[] cal;
    string dateString;
    dateString = date.ToString("yyyyMMdd");
    string check;
    int limit;

    Debug.Log(dateString);

    using (var connection = new SqliteConnection(dbName)) {
      connection.Open();
      using (var command = connection.CreateCommand()) {
        command.CommandText =
            "select count(DISTINCT(exercise_id)) FROM exercise_info";
        limit = int.Parse(command.ExecuteScalar().ToString());
        Debug.Log("Limit:" + limit);
        for (int id = 1; id <= limit; id++) {
          command.CommandText =
              "SELECT count(*) FROM session_info WHERE userid = '" + userid +
              "' AND substr(REPLACE(exercise_start,' - ',''),0,9) like '" +
              dateString + "' AND exercise_id = '" + id + "'; ";
          check = command.ExecuteScalar().ToString();
          if (check == "0") {
            cal.Add(0);
          } else {
            command.CommandText =
                "SELECT SUM(calories_burnt) FROM session_info WHERE userid = '" +
                userid +
                "' AND substr(REPLACE(exercise_start,' - ',''),0,9) like '" +
                dateString + "' AND exercise_id = '" + id + "'; ";
            double value = double.Parse(command.ExecuteScalar().ToString());
            cal.Add(value);
          }
        }
      }
      connection.Close();
    }
    Debug.Log("List size:" + cal.Count);
    for (int j = 0; j < limit; j++) {
      Debug.Log("List item [" + j + "] = " + cal[j].ToString());
    }
    return cal;
  }

  // Retrieves and adjusts data for Average HR Graph
  public List<double> avgHRGraphData(int userid, int exercise_id) {
    List<double> avgHR = new List<double>();
    string dateString;
    string check;

    using (var connection = new SqliteConnection(dbName)) {
      connection.Open();
      using (var command = connection.CreateCommand()) {
        for (int i = 0; i < 7; i++) {
          check = null;

          dateString = DateTime.Now.AddDays(-6 + i).ToString("yyyyMMdd");

          command.CommandText =
              "SELECT count(*) FROM session_info WHERE substr(REPLACE(exercise_start,' - ',''),0,9)  like '" +
              dateString + "' AND userid = '" + userid +
              "' and exercise_id = '" + exercise_id + "';";
          check = command.ExecuteScalar().ToString();
          if (check == "0") {
            avgHR.Add(0);
          } else {
            command.CommandText =
                "SELECT AVG(avg_HR) FROM session_info WHERE substr(REPLACE(exercise_start,' - ',''),0,9)  like '" +
                dateString + "' AND userid = '" + userid +
                "' and exercise_id = '" + exercise_id + "';";
            double value = double.Parse(command.ExecuteScalar().ToString());
            avgHR.Add(value);
          }
        }
      }
      connection.Close();
    }
    Debug.Log("List size:" + avgHR.Count);
    for (int j = 0; j < 7; j++) {
      Debug.Log("List item [" + j + "] = " + avgHR[j].ToString());
    }
    return avgHR;
  }

  // Retrieves and adjusts data for Maximum HR Graph
  public List<double> maxHRGraphData(int userid, int exercise_id) {
    List<double> maxHR = new List<double>();
    string dateString;
    string check;

    using (var connection = new SqliteConnection(dbName)) {
      connection.Open();
      using (var command = connection.CreateCommand()) {
        for (int i = 0; i < 7; i++) {
          check = null;

          dateString = DateTime.Now.AddDays(-6 + i).ToString("yyyyMMdd");

          command.CommandText =
              "SELECT count(*) FROM session_info WHERE substr(REPLACE(exercise_start,' - ',''),0,9)  like '" +
              dateString + "' AND userid = '" + userid +
              "' and exercise_id = '" + exercise_id + "';";
          check = command.ExecuteScalar().ToString();
          if (check == "0") {
            maxHR.Add(0);
          } else {
            command.CommandText =
                "SELECT MAX(max_HR) FROM session_info WHERE substr(REPLACE(exercise_start,' - ',''),0,9)  like '" +
                dateString + "' AND userid = '" + userid +
                "' and exercise_id = '" + exercise_id + "';";
            double value = double.Parse(command.ExecuteScalar().ToString());
            maxHR.Add(value);
          }
        }
      }
      connection.Close();
    }
    Debug.Log("List size:" + maxHR.Count);
    for (int j = 0; j < 7; j++) {
      Debug.Log("List item [" + j + "] = " + maxHR[j].ToString());
    }
    return maxHR;
  }

  // Retrieves and adjusts data for Average HR for one day Graph
  public double avgHRoneday(int userid) {
    string dateString;
    string check;
    double avgHR;

    using (var connection = new SqliteConnection(dbName)) {
      connection.Open();
      using (var command = connection.CreateCommand()) {

        check = null;

        dateString = DateTime.Now.ToString("yyyyMMdd");
        Debug.Log("User id is: " + userid);

        command.CommandText =
            "SELECT count(*) FROM session_info WHERE substr(REPLACE(exercise_start,' - ',''),0,9)  like '" +
            dateString + "' AND userid = '" + userid + "';";
        check = command.ExecuteScalar().ToString();
        if (check == "0") {
          avgHR = 0;
          Debug.Log("Nothing found in db");
        } else {
          Debug.Log("entered else");
          command.CommandText =
              "SELECT AVG(avg_HR) FROM session_info WHERE substr(REPLACE(exercise_start,' - ',''),0,9)  like '" +
              dateString + "' AND userid = '" + userid + "';";
          avgHR = double.Parse(command.ExecuteScalar().ToString());
        }
      }
      connection.Close();
    }
    return avgHR;
  }

  // Retrieves and adjusts data for Maximum HR for one day Graph
  public double maxHRoneday(int userid) {

    string dateString;
    string check;

    using (var connection = new SqliteConnection(dbName)) {
      connection.Open();
      using (var command = connection.CreateCommand()) {

        check = null;

        dateString = DateTime.Now.ToString("yyyyMMdd");

        command.CommandText =
            "SELECT count(*) FROM session_info WHERE substr(REPLACE(exercise_start,' - ',''),0,9)  like '" +
            dateString + "' AND userid = '" + userid + "';";
        check = command.ExecuteScalar().ToString();
        if (check == "0") {
          maxHR = 0;
        } else {
          command.CommandText =
              "SELECT MAX(max_HR) FROM session_info WHERE substr(REPLACE(exercise_start,' - ',''),0,9)  like '" +
              dateString + "' AND userid = '" + userid + "';";
          maxHR = double.Parse(command.ExecuteScalar().ToString());
        }
      }
      connection.Close();
    }
    Debug.Log("Max hr in dbconn" + maxHR);
    return maxHR;
  }

  // Creates Connection with UPM Table to retrieve data of current user
  public void dbconnectionUPM() {
    // Create database
    conn = "URI=file:" + Application.persistentDataPath + "/" + "UPMdatabase";
    Debug.Log(conn);
    // Open connection
    IDbConnection dbconUPM = new SqliteConnection(conn);
    dbconUPM.Open();

    // Create table
    IDbCommand dbcmd;
    dbcmd = dbconUPM.CreateCommand();
    string q_createTable =
        "CREATE TABLE IF NOT EXISTS userInformation (id INTEGER PRIMARY KEY AUTOINCREMENT, height FLOAT NOT NULL, weight FLOAT NOT NULL, age TEXT NOT NULL, gender TEXT )";
    dbcmd.CommandText = q_createTable;
    dbcmd.ExecuteReader();
  }

  // Retrieves Data for the Current User from UPM Table
  public void getLatestEnteredID() {
    using (IDbConnection dbconUPM = new SqliteConnection(conn)) {
      dbconUPM.Open();

      using (IDbCommand dbcmd = dbconUPM.CreateCommand()) {

        string dataQuery = String.Format(
            "SELECT * from userInformation order by ROWID DESC limit 1");

        dbcmd.CommandText = dataQuery;

        using (IDataReader reader = dbcmd.ExecuteReader()) {

          while (reader.Read()) {
            Debug.Log("user id in UPM: " + reader["id"].ToString());
            getUser.heightGet = reader["height"].ToString();
            getUser.weightGet = reader["weight"].ToString();
            getUser.ageGet = reader["age"].ToString();
            getUser.genderGet = reader["gender"].ToString();
            getUser.idGet = reader["id"].ToString();
            Debug.Log(reader["id"].ToString());
          }
        }
        dbcmd.ExecuteScalar();
        dbconUPM.Close();
      }
    }
  }

  // Returns dataRetrivaldbconn object with all the information of the user
  public dataRetrivaldbconn getData() {
    dbconnectionUPM();
    getLatestEnteredID();
    Debug.Log("UserID in dbconn from upm:" + getUser.idGet);
    return getUser;
  }
}

// Class with User Data Attributes
public class dataRetrivaldbconn {
  // relevant for Stats and history

  public string heightGet;
  public string weightGet;
  public string ageGet;
  public string genderGet;
  public string idGet;
}

// Class with Data to be Saved Attributes
public class userdata {
  public int userid;
  public int exercise_id;
  public string exercise_start;
  public string exercise_end;
  public int noOfSets;
  public double avg_HR;
  public double max_HR;
  public double calories_burnt;
}
