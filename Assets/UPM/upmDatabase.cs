using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Data;
using System.Linq;
using Mono.Data.Sqlite;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;


public class upmDatabase : MonoBehaviour
{

    public  dataRetrivalUPM getUser = new dataRetrivalUPM();

    //public static upmDatabase instance;

    private static string connection;
    public TMP_InputField height_TMP;
    public TMP_InputField weight_TMP;
    public TMP_InputField age_TMP;
    public Dropdown Gender;
    public static Text ID;

    public GameObject dataPanel;
    public GameObject userPanel;
    // Start is called before the first frame update
    void Start()
    {
        
        dbconnection();
             height_TMP.text = "150";
                weight_TMP.text = "150";
                age_TMP.text = "60";
                Gender.value = 1;
                enterInDatabase();
                getLatestEnteredID();
                getData();
       // RegisterUser();
    }


    public void dbconnection()
    {
        // Create database
        connection = "URI=file:" + Application.persistentDataPath + "/" + "UPMdatabase";
        Debug.Log(connection);
        // Open connection
        IDbConnection dbcon = new SqliteConnection(connection);
        dbcon.Open();

        // Create table
        IDbCommand dbcmd;
        dbcmd = dbcon.CreateCommand();
        string q_createTable = "CREATE TABLE IF NOT EXISTS userInformation (id INTEGER PRIMARY KEY AUTOINCREMENT, height FLOAT NOT NULL, weight FLOAT NOT NULL, age TEXT NOT NULL, gender TEXT )";
        dbcmd.CommandText = q_createTable;
        dbcmd.ExecuteReader();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



public   void getLatestEnteredID(){
using (IDbConnection dbcon = new SqliteConnection(connection))
        {
            dbcon.Open();

            using (IDbCommand dbcmd = dbcon.CreateCommand())
            {

            
                
                    string dataQuery = String.Format("SELECT * from userInformation order by ROWID DESC limit 1");

                //string dataQuery = String.Format("INSERT INTO userInformation ( height, weight, age, gender) VALUES ( 15,15, 15, 0)");
                dbcmd.CommandText = dataQuery;
                
             using(IDataReader reader = dbcmd.ExecuteReader()){
					
					while(reader.Read())
					{
						Debug.Log("user id in UPM: " +reader["id"].ToString());
                        getUser.heightGet =  reader["height"].ToString();
                        getUser.weightGet =  reader["weight"].ToString();
                        getUser.ageGet =  reader["age"].ToString();
                        getUser.genderGet =  reader["gender"].ToString();
                        getUser.idGet =  reader["id"].ToString();
                        Debug.Log(reader["id"].ToString());
                        
                        //ID.text = reader["id"].ToString();

                    }

             }

                dbcmd.ExecuteScalar();
                dbcon.Close();



            }
        }

}

public dataRetrivalUPM getData()	//Public function that we will use
	{
		//Debug.Log("the height from UPM is "+ getUser.heightGet.ToString());//height and user id
		Debug.Log("the userid from sqlitetest1 "+getUser.idGet);
		return getUser;
		//Debug.Log(newobj.heightGet.ToString());
	}


    public void enterInDatabase()
    {
        Debug.Log("In register User");

        using (IDbConnection dbcon = new SqliteConnection(connection))
        {
            dbcon.Open();

            using (IDbCommand dbcmd = dbcon.CreateCommand())
            {

                

               if((height_TMP.text != string.Empty) && (weight_TMP.text != string.Empty) && (age_TMP.text != string.Empty) ){
                
                    string dataQuery = String.Format("INSERT INTO userInformation ( height, weight, age, gender) VALUES ( \"{0}\", \"{1}\", \"{2}\", \"{3}\")", height_TMP.text, weight_TMP.text, age_TMP.text, Gender.value);

                //string dataQuery = String.Format("INSERT INTO userInformation ( height, weight, age, gender) VALUES ( 15,15, 15, 0)");
                dbcmd.CommandText = dataQuery;
                Debug.Log("Inserted in table");

                dataPanel.SetActive(false);
                userPanel.SetActive(true);

                        }
                        else {


                             Debug.Log("One or more fields is missing.");
                        }
                

                dbcmd.ExecuteScalar();
                dbcon.Close();


            }
        }
    }

 
}

public class dataRetrivalUPM
{
    //relevant for Stats and history
    
    public string heightGet;
    public string weightGet;
    public string ageGet;
    public string genderGet;
    public string idGet;


    public void getDataRetrieval(string height, string weight, string age, string genderGet, string id)
    {

        Debug.Log(height);
        Debug.Log(weight);
        Debug.Log(age);
        Debug.Log(id);
    }
}
