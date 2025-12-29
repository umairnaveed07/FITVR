using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Globalization;
using System.Data;
using System.Text;
using Mono.Data.Sqlite;
using System.IO;
using TMPro;

public class Trial : MonoBehaviour
{

    public TMP_InputField height_TMP;
    public TMP_InputField weight_TMP;
    public TMP_InputField username_TMP;
    public TMP_InputField loginIdHolderField;
    public TMP_InputField dob_TMP_day;
    public TMP_InputField dob_TMP_month;
    public TMP_InputField dob_TMP_year;
    public TMP_InputField age_TMP;
    public TMP_InputField bmi_TMP;
    public Text showError;
    public Dropdown Gender;
    public Dropdown hobbies;
    public Text datacheck;
    public float bmassi;
    public float getheight;
    public float getweight;
    public float heightinmt;
    public GameObject Editprofile1;
    public GameObject Profilepanel1;
    public int counter=1;
    public int flag=0;
    public int checkheight = 0;
    public int checkweight = 0;
    public int checkdob = 0;
    public string dob1;

    //sqlitetest1 mainObj= new sqlitetest1();

    void Start()
    {
        
        // Create database
        string connection = "URI=file:" + Application.persistentDataPath + "/" + "MainFitvr5";
        Debug.Log(connection);
        // Open connection
        IDbConnection dbcon = new SqliteConnection(connection);
        dbcon.Open();

        // Create table
        IDbCommand dbcmd;
        dbcmd = dbcon.CreateCommand();
        string q_createTable = "CREATE TABLE IF NOT EXISTS newuserData (username VARCHAR[255] NOT NULL PRIMARY KEY UNIQUE, height FLOAT NOT NULL, weight FLOAT NOT NULL, bmi FLOAT, dob TEXT, age TEXT, gender TEXT, hobbies TEXT, id INT, FOREIGN KEY (id) REFERENCES usersLogin(id))";
        
        dbcmd.CommandText = q_createTable;
        dbcmd.ExecuteReader();

        // Close connection
        dbcon.Close();
        GetDataFromDB();

    }


    public void logoutClear(){
        Debug.Log("running fine");
        height_TMP.text="";
        weight_TMP.text="";
        bmi_TMP.text="";
        dob_TMP_day.text="";   
        dob_TMP_month.text = "";
        dob_TMP_year.text = "";
        age_TMP.text = "";
        Gender.value=0;
        hobbies.value=0;
        
        }

public void GetDataFromDB(){
         string connection = "URI=file:" + Application.persistentDataPath + "/" + "MainFitvr5";
		using(IDbConnection dbcon = new SqliteConnection(connection)){
			dbcon.Open();

			using(IDbCommand dbcmd = dbcon.CreateCommand()){

				string dataQuery ="SELECT * FROM newuserData WHERE id= '"+ loginIdHolderField.text +"'";
				dbcmd.CommandText = dataQuery;
				
				using(IDataReader reader = dbcmd.ExecuteReader()){
                    //var test;

					while(reader.Read())
					{
						Debug.Log("from get data from DB TRIAL" + loginIdHolderField.text);
					  
						Debug.Log(reader[1]);
                        height_TMP.text = reader[1].ToString();
                        weight_TMP.text = reader[2].ToString();
                        bmi_TMP.text = reader[3].ToString();
                        dob1 = reader["dob"].ToString();
                        int index = dob1.IndexOf('-');
                        age_TMP.text = reader["age"].ToString();
                        dob_TMP_day.text = dob1.Substring(0, index);
                        dob_TMP_month.text = dob1.Substring(index + 1, 2);
                        //var monlen = dob_TMP_month.text;
                        /*if (monlen.Length == 1)
                        {
                            dob_TMP_month.text = dob1.Substring(index + 1, 1);
                        }
                        if (monlen.Length == 2)
                        {
                            dob_TMP_month.text = dob1.Substring(index + 1, 2);
                        }
                        */
                        dob_TMP_year.text = dob1.Substring(dob1.Length - 4, 4);
                        Gender.value = Convert.ToInt32(reader[7]);
                        hobbies.value = Convert.ToInt32(reader[8]);
                        


                    }
					dbcon.Close();
					reader.Close();
				}
			}
		}
	}
   
   
    public void InsertProf() {
        // Open connection
        string connection = "URI=file:" + Application.persistentDataPath + "/" + "MainFitvr5";
        if((height_TMP.text != string.Empty) && (weight_TMP.text != string.Empty) && (dob_TMP_day.text != string.Empty) && (dob_TMP_month.text != string.Empty) && (dob_TMP_year.text != string.Empty))
        {

                    // getheight= Convert.ToDouble(height.text);
            float getheight = float.Parse(height_TMP.text, CultureInfo.InvariantCulture.NumberFormat);
            float getweight = float.Parse(weight_TMP.text, CultureInfo.InvariantCulture.NumberFormat);
            // getweight= weight.text;
            heightinmt = (getheight/100);
            bmassi = (getweight/heightinmt);
            
                
            //Debug.Log("The age is " + age);
            //var age = 22;
            IDbConnection dbcon = new SqliteConnection(connection);
        dbcon.Open();

            // Check if data is fine
            // Check height

            int intheight = Convert.ToInt32(getheight);
            int intweight = Convert.ToInt32(getweight);
            if (80 <= intheight && intheight <= 220)
            {
                checkheight = 1;
            }
            else
            {
                checkheight = 2;
            }
            if (10 <= intweight && intweight <= 300)
            {
                checkweight = 1;
            }
            else
            {
                checkweight = 2;
            }
            var Date = DateTime.Now;
            var dob = dob_TMP_day.text + "-" + dob_TMP_month.text + "-" + dob_TMP_year.text;
            DateTime dobdt = Convert.ToDateTime(dob);
            Debug.Log("The dob ombo is" + dobdt);
            //DateTime dobdt = DateTime.ParseExact(dob, "dd-MM-yyyy", System.Globalization.DateTimeStyles.None);
            var agediff = Date - dobdt;
            int ageint = agediff.Days;
            var age = ageint / 365;
            //.ToString("dd-MM-yyyy");
            if ((1 <= Convert.ToInt32(dob_TMP_day.text)) && (Convert.ToInt32(dob_TMP_day.text) <= 31) && (1 <= Convert.ToInt32(dob_TMP_month.text)) && (Convert.ToInt32(dob_TMP_month.text) <= 12) && (1920 <= Convert.ToInt32(dob_TMP_year.text)) && (Convert.ToInt32(dob_TMP_year.text) <= 2016))
            {
                checkdob = 1;
            }
            else
            {
                checkdob = 2;
            }
            if ( checkheight == 1 && checkweight == 1 && checkdob == 1)
            {
                flag = 1;
            }
            
            // Insert values in table
            IDbCommand cmnd = dbcon.CreateCommand();
            
            if ( flag == 1)
            {
                //Calulate age
                cmnd.CommandText = "INSERT INTO newuserData (username, height, weight, bmi, dob, age, gender, hobbies, id) SELECT '" + username_TMP.text + "' , '" + height_TMP.text + "', '" + weight_TMP.text + "', '" + bmassi + "', '" + dob + "', '" + age + "', '" + Gender.value + "', '" + hobbies.value + "', '" + loginIdHolderField.text + "' WHERE NOT EXISTS (SELECT id from newuserData WHERE id='" + loginIdHolderField.text + "')";
            }
            else
            {
                if (checkheight == 2)
                {
                    Debug.Log("The entered height is incorrect");
                    showError.text = "The entered height is incorrect";
                }
                if (checkweight == 2)
                {
                    Debug.Log("The entered weight is incorrect");
                    showError.text = "The entered weight is incorrect";
                }
                if (checkdob == 2)
                {
                    Debug.Log("The entered date of birth is incorrect");
                    showError.text = "The entered date of birth is incorrect. Please enter the correct date";
                }
            }
        cmnd.ExecuteNonQuery();

        //Update values in table
        IDbCommand cmnd_upd = dbcon.CreateCommand();
            if (flag == 1)
            {
                cmnd_upd.CommandText = "UPDATE newuserData SET height = '" + height_TMP.text + "', weight = '" + weight_TMP.text + "', bmi = '" + bmassi + "', dob = '" + dob + "', age = '" + age + "', gender = '" + Gender.value + "', hobbies = '" + hobbies.value + "', id = '" + loginIdHolderField.text + "' WHERE id = '" + loginIdHolderField.text + "'";
            }
             else
            {
                if (checkheight == 2)
                {
                    Debug.Log("The entered height is incorrect");
                    showError.text = "The entered height is incorrect";
                }
                if (checkweight == 2)
                {
                    Debug.Log("The entered weight is incorrect");
                    showError.text = "The entered weight is incorrect";
                }
                if (checkdob == 2)
                {
                    Debug.Log("The entered date of birth is incorrect");
                    showError.text = "The entered date of birth is incorrect. Please enter the correct date";
                }
            }
        cmnd_upd.ExecuteNonQuery();
        showError.text = "";

        //show bmi
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string query ="SELECT * FROM newuserData WHERE id = '" + loginIdHolderField.text + "'";
        cmnd_read.CommandText = query;
        reader = cmnd_read.ExecuteReader();
        while (reader.Read()) { 
           bmi_TMP.text = reader[3].ToString();
        }
        Editprofile1.SetActive(false);
        Profilepanel1.SetActive(true);


        dbcon.Close();
        //displayLog();
        }
        else {
            Debug.Log("Please enter details");
            showError.text = "Please enter details";
        }

    

    }

    public void flagReset(){

    flag=0;
}
    




    

}



