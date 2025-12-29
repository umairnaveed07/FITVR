using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class ForgotPassword : MonoBehaviour
{

    public TMP_InputField email_TMP;
    public TMP_InputField password_TMP;
    public TMP_InputField repassword_TMP;
    public GameObject Panel1;
    public GameObject Panel2;
    public GameObject Panel3;
    public Text errorEmail;
    public Text passwordCheck;

    void Start()
    {
    
// Create database
        string connection = "URI=file:" + Application.persistentDataPath + "/" + "MainFitvr5";
        
        // Open connection
        IDbConnection dbcon = new SqliteConnection(connection);
        dbcon.Open();
        
        // Create table
		IDbCommand dbcmd;
		dbcmd = dbcon.CreateCommand();
		string q_createTable = "CREATE TABLE IF NOT EXISTS usersLogin (id INTEGER PRIMARY KEY AUTOINCREMENT, username TEXT NOT NULL, password TEXT NOT NULL, email TEXT NOT NULL )";
		dbcmd.CommandText = q_createTable;
		dbcmd.ExecuteReader();


		// Insert values in table
		// IDbCommand cmnd = dbcon.CreateCommand();
		// cmnd.CommandText = "INSERT INTO users (username, password, email) VALUES ('you','123456', 'you@gmail.com')";
		// cmnd.ExecuteNonQuery();

        dbcon.Close();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void verifyEmail() {
        // Open connection
        string connection = "URI=file:" + Application.persistentDataPath + "/" + "MainFitvr5";
                IDbConnection dbcon = new SqliteConnection(connection);
        dbcon.Open();

        //Check if email field is empty
        if((this.email_TMP.text != string.Empty)){
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string query ="SELECT * FROM usersLogin WHERE email= '" + this.email_TMP.text + "'";
        cmnd_read.CommandText = query;
        reader = cmnd_read.ExecuteReader();
        int count =0;
        while (reader.Read()) {
                count++;
        }
                    // Check if email exists in DB
        			if(count==1){
						Debug.Log("email exists");
                        Panel1.SetActive(false);
                        Panel2.SetActive(true);
					}
					if(count<1){
						Debug.Log("email doesn't exist");
                        errorEmail.text = "E-mail doesn't exist";
					}
        }
        else {        
            Debug.Log("Enter E-mail Address");
            errorEmail.text = "Enter E-mail Address";

        }
        dbcon.Close();
        
    }
    

    public void updatePass() {
        // Open connection
        string connection = "URI=file:" + Application.persistentDataPath + "/" + "MainFitvr5";
        IDbConnection dbcon = new SqliteConnection(connection);
        dbcon.Open();

        //Check if both password fields are filled
        if((this.password_TMP.text != string.Empty) &&(this.repassword_TMP.text != string.Empty) ){
        IDbCommand cmnd1 = dbcon.CreateCommand();
        // Check if both passwords match
        if (this.password_TMP.text == this.repassword_TMP.text) {
            int intpassword = Convert.ToInt32(this.password_TMP.text);
                if ( 999 <= intpassword && intpassword <= 10000)
                {
        cmnd1.CommandText = "UPDATE usersLogin SET password = '" + this.password_TMP.text + "' WHERE email= '" + this.email_TMP.text + "'";
        cmnd1.ExecuteNonQuery();
        Panel2.SetActive(false);
        Panel3.SetActive(true);
        displayLog();
                }
        else
                {
                    Debug.Log("The password should be of 4 digits");
                    passwordCheck.text = "The password should be of 4 digits";
                }
        }
        else {
           
            Debug.Log("Passwords don't match");
            passwordCheck.text = "Passwords don't match";

        }
        }
        else {
            Debug.Log("Enter New Password");
            passwordCheck.text = "Enter New Password";
        }
        dbcon.Close();
        
    }


        public void displayLog() {
        // Open connection
        string connection = "URI=file:" + Application.persistentDataPath + "/" + "MainFitvr5";
        IDbConnection dbcon = new SqliteConnection(connection);
        dbcon.Open();

        //Read values
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string query ="SELECT * FROM usersLogin";
        cmnd_read.CommandText = query;
        reader = cmnd_read.ExecuteReader();

        while (reader.Read())
        {
            Debug.Log("email: " + reader[3].ToString());
            Debug.Log("password: " + reader[2].ToString());
        }
        dbcon.Close();
    }

}



