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


public static class Extension
	{
    public static void clear(this TMP_InputField inputfield)
    {
        inputfield.Select();
        inputfield.text = "";
    }
	}

public  class sqlitetest1 : MonoBehaviour {


private string connection;
public TMP_InputField usernameLogin_TMP;
public TMP_InputField passwordLogin_TMP;
public TMP_InputField usernameRegister_TMP;
public TMP_InputField passwordRegister_TMP;
public TMP_InputField confirmPasswordRegister_TMP;
public TMP_InputField emailRegister_TMP;
public TMP_InputField age;
//public InputField fullNameRegiter;
public TMP_InputField username;//
public GameObject Register;
public GameObject Login;
public GameObject buttonPrefab;
public GameObject ParentPanel;
public GameObject EditProfile;
public GameObject profilePanel;
public GameObject Panel2;
public GameObject Panel3;
//public TMP_InputField loginIdHolderField_TMP;
public TMP_InputField loginIdHolderField;
public Text errorText;
public Text errorTextRegister;
public int loginIdHolder;//
public Text usernameProfileTag;
public string idholder;
public int loginStatus;
public int playerPrefId;
public string playerUsernamePref;
public string playerDisplayNamePref;



	public string username1;//
	public string height1;
	public string weight1;
	public string bmi1;
    public string dob1;
    public string dobday1;
    public string dobmonth1;
    public string dobyear1;
    public string age1;
	public string id1;//

public static dataRetrival newobj = new dataRetrival();
//Trial trialObj= new Trial();

	// Use this for initialization
	void Start () {

			username.interactable = false;
			loginIdHolderField.interactable = false;
        age.interactable = false;
        dbconnection();
		LoginStatusChecker();
		loginStatus = PlayerPrefs.GetInt("LoginStatusValue",0);
		playerPrefId = PlayerPrefs.GetInt("PlayerId", 0);
		playerUsernamePref = PlayerPrefs.GetString("PlayerName", "Fitvr");
		playerDisplayNamePref = PlayerPrefs.GetString("PlayerDisplayName", "FitVR" );
		if(loginStatus==1){
			ParentPanel.SetActive(false);
			profilePanel.SetActive(true);
			loginIdHolder = playerPrefId;
			loginIdHolderField.text = loginIdHolder.ToString();
			usernameProfileTag.text = playerUsernamePref;
			username.text = playerDisplayNamePref;
			//trialObj.GetDataFromDB();
			pullDataFromDb();
			newobj.usernameGet = username1;
			newobj.heightGet = height1;
			newobj.weightGet = weight1;
			newobj.bmiGet = bmi1;
			newobj.dobdayGet = dobday1;
            newobj.dobmonthGet = dobmonth1;
            newobj.dobyearGet = dobyear1;
            newobj.ageGet = age1;
			newobj.idGet = id1;



		}
	
	}

	
	public void dbconnection(){
		// Create database
		connection = "URI=file:" + Application.persistentDataPath + "/" + "MainFitvr5";
		Debug.Log(connection);
		// Open connection
		IDbConnection dbcon = new SqliteConnection(connection);
		dbcon.Open();

		// Create table
		 IDbCommand dbcmd;
		dbcmd = dbcon.CreateCommand();
		string q_createTable = "CREATE TABLE IF NOT EXISTS usersLogin (id INTEGER PRIMARY KEY AUTOINCREMENT, username TEXT NOT NULL, password TEXT NOT NULL, email TEXT NOT NULL )";
		dbcmd.CommandText = q_createTable;
		dbcmd.ExecuteReader();
	}

	public void logOutTrigger(){
			loginStatus = 0;
			loginIdHolder = 0;
			playerUsernamePref = "";
			PlayerPrefs.SetInt("LoginStatusValue", loginStatus);
			PlayerPrefs.SetInt("PlayerId", loginIdHolder);
			PlayerPrefs.SetString("PlayerName", playerUsernamePref);
			profilePanel.SetActive(false);
			ParentPanel.SetActive(true);
			
			username1="";
			height1="";
			weight1="";
			bmi1="";
			dobday1="";
            dobmonth1 = "";
            dobyear1 = "";
            age1 = "";
			id1="";

		}

IEnumerator LateCall()
     {
        Debug.Log("Started Coroutine at timestamp : " + Time.time);
         
         yield return new WaitForSeconds(5);
  
         errorText.text = "";
		  
         //Do Function here...
     }

		public void LoginUserTrigger(){
		
			using(IDbConnection dbcon = new SqliteConnection(connection)){
			

			try{
					dbcon.Open();
					using(IDbCommand dbcmd = dbcon.CreateCommand()){
					string dataQuery =  String.Format("SELECT * FROM usersLogin WHERE username ='"+ this.usernameLogin_TMP.text +"' AND password = '"+ this.passwordLogin_TMP.text +"' ");
					dbcmd.CommandText = dataQuery;
					// SQLiteCommand CreateCommand = new SQLiteCommand(dataQuery, dbcon);
					// CreateCommand.ExecuteNonQuery();
					// SQLiteDataReader dr = CreateCommand.ExecuteReader();
					using(IDataReader reader = dbcmd.ExecuteReader()){
					int count =0;
					while(reader.Read())
					{

						count++;
						Debug.Log("username: " + reader[1].ToString());//USERNAME PRINTED
						username.text= reader["username"].ToString();
                        //age.text = reader["age"].ToString();
						usernameProfileTag.text = reader[1].ToString();
						PlayerPrefs.SetString("PlayerName", usernameProfileTag.text );
						PlayerPrefs.SetString("PlayerDisplayName", username.text );
					}
					
					

					if(count==1){
						Debug.Log("user and pass correct");
						
						//Register.SetActive(true);
						//Login.SetActive(false);
						reader.Close();
						
						string dataQuery2 =  String.Format("SELECT id FROM usersLogin WHERE username ='"+ this.usernameLogin_TMP.text +"' AND password = '"+ this.passwordLogin_TMP.text +"' ");
					    
						dbcmd.CommandText = dataQuery2;
						using(IDataReader reader2 = dbcmd.ExecuteReader()){
							while(reader2.Read())
								{
									
									loginIdHolder = Convert.ToInt32(reader2[0]);
									Debug.Log("LoginIDHolder" + loginIdHolder);
									loginIdHolderField.text = loginIdHolder.ToString();
									Debug.Log("loginIdHolderField" + loginIdHolderField.text);
									//PlayerPrefs.SetString("username_pref", this.usernameLogin_TMP.text);
									//PlayerPrefs.SetString("password_pref", this.passwordLogin_TMP.text);
									loginStatus = 1;
									PlayerPrefs.SetInt("LoginStatusValue", loginStatus);
									PlayerPrefs.SetInt("PlayerId", loginIdHolder);
									 
									
								}
								reader.Close();
						}
						
						//relevant for Stats and history
						pullDataFromDb();

							newobj.usernameGet = username1;
							newobj.heightGet = height1;
							newobj.weightGet = weight1;
							newobj.bmiGet = bmi1;
							newobj.dobdayGet = dobday1;
                            newobj.dobmonthGet = dobmonth1;
                            newobj.dobyearGet = dobyear1;
                            newobj.ageGet = age1;
							newobj.idGet = id1;

							Debug.Log("the height from logintrigger is " + newobj.heightGet.ToString());//height and user id
							Debug.Log("the userid from logintrigger " + newobj.idGet.ToString());

							//Debug.Log(newobj.heightGet.ToString());

							getData();
						//newobj.getDataRetrieval(heightGet, weightGet, dobGet, bmiGet, idGet);
						//Debug.Log(newobj.heightGet);
						
						//
						usernameLogin_TMP.clear();
						passwordLogin_TMP.clear();
						ParentPanel.SetActive(false);
						profilePanel.SetActive(true);
					


					}
					if(count<1){
						Debug.Log("user and pass incorrect");
						errorText.text = "wrong username or password";
							
						StartCoroutine(LateCall());					



					}
					dbcon.Close();
					reader.Close();
					}
					}

				}

			catch(Exception ex){
				Debug.Log(ex);}
		}
		}

		public void LoginStatusChecker(){

			if (loginStatus==1){
				Debug.Log("Its true");
			}
			else{

				Debug.Log("Currentyl False");
			}
		}

		
		public void editProfileTrigger(){
			
			//Debug.Log(trialObj2.height.text);
			profilePanel.SetActive(false);
			EditProfile.SetActive(true);
		}
    
    private void RegisterUser(string username, string password, string email){

			using(IDbConnection dbcon = new SqliteConnection(connection)){
			dbcon.Open();

			using(IDbCommand dbcmd = dbcon.CreateCommand()){
                int intpassword = Convert.ToInt32(password);
                if ( 999 <= intpassword && intpassword <= 10000)
                {
                    string dataQuery = String.Format("INSERT INTO usersLogin ( Username, Password, email) VALUES ( \"{0}\", \"{1}\", \"{2}\")", username, password, email);
                    dbcmd.CommandText = dataQuery;
                }
                else
                {
                    Debug.Log("The password should be 4 digits");
                    errorTextRegister.text = "The password should be 4 digits";
                }
				dbcmd.ExecuteScalar();
				dbcon.Close();
				
				
			}
		}
	}

	public void passwordVerifier(){

		if(passwordRegister_TMP.text==confirmPasswordRegister_TMP.text){
			Debug.Log("password and confirm pass matched");
			RegisterUserTrigger();
		}
		else{
			Debug.Log("Password doesnt match confirm password");
			errorTextRegister.text= "Password doesnt match confirm password";
		}
	}

	
	
	public void RegistrationVerifier(){
		if((usernameRegister_TMP.text != string.Empty) && (emailRegister_TMP.text != string.Empty) && (passwordRegister_TMP.text != string.Empty) && (confirmPasswordRegister_TMP.text != string.Empty) ){
		using(IDbConnection dbcon = new SqliteConnection(connection)){
			

			try{
					dbcon.Open();
					using(IDbCommand dbcmd = dbcon.CreateCommand()){
					string dataQuery =  String.Format("SELECT * FROM usersLogin WHERE username ='"+ this.usernameRegister_TMP.text +"' OR email = '"+ this.emailRegister_TMP.text +"'");
					dbcmd.CommandText = dataQuery;
					using(IDataReader reader = dbcmd.ExecuteReader()){
					int count =0;
					while(reader.Read())
					{
						count++;						
					}
					
					if(count>=1){
						Debug.Log("(>=1)Username and email already created. Use another username or email");
						errorTextRegister.text= "username/email exists. use another";
					}
					if(count<1){
						Debug.Log("No user found.");
					
						passwordVerifier();
					}
					dbcon.Close();
					reader.Close();
					}
					}

				}

			catch(Exception ex){
				Debug.Log(ex);}
		}
	}
	else{

		Debug.Log("One or more fields empty");
		errorTextRegister.text= "One or more fields empty";
	}

	}

	private void RegisterUserTrigger(){

		if (usernameRegister_TMP.text != string.Empty){
			Debug.Log("In Register User Triger");
			RegisterUser(usernameRegister_TMP.text, passwordRegister_TMP.text, emailRegister_TMP.text);
						Register.SetActive(false);
						Login.SetActive(true);
						Debug.Log("login id holder from register user " + loginIdHolderField.text);
						usernameRegister_TMP.clear();
						emailRegister_TMP.clear();
						passwordRegister_TMP.clear();
						confirmPasswordRegister_TMP.clear();

						
						
		}
		
	}

//clear fields function

public void ClearFields(){

	usernameLogin_TMP.text = "";
	passwordLogin_TMP.text = "";
	usernameRegister_TMP.text = "";
	emailRegister_TMP.text = "";
	passwordRegister_TMP.text ="";
	confirmPasswordRegister_TMP.text = "";

	}
	
//For stats and history
	public void pullDataFromDb(){
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
					  Debug.Log("test data"+loginIdHolderField.text);
					 

						//Debug.Log(reader[1]);
					  username1= reader["username"].ToString();
                      height1 = reader[1].ToString();
                      weight1 = reader[2].ToString();
                      bmi1 = reader[3].ToString();
                        dob1 = reader["dob"].ToString();
                        int index = dob1.IndexOf('-');
                        Debug.Log("length of string is  " + dob1.Length);
                        dobday1 = dob1.Substring(0, index);
                        //var monlen = dobmonth1;
                        int index1 = dob1.IndexOf('-',3);
                        dobmonth1 = dob1.Substring(index + 1, 2);
                        /*if (monlen.Length == 1)
                        {
                            dobmonth1 = dob1.Substring(index + 1, 1);
                        }
                        else
                        {
                            dobmonth1 = dob1.Substring(index + 1, 2);
                        }
                        */
                        dobyear1 = dob1.Substring(dob1.Length - 4, 4);
                        age1 = reader["age"].ToString();
					  id1 = reader["id"].ToString();
                     // Gender.value = Convert.ToInt32(reader[5]);
                     //hobbies.value = Convert.ToInt32(reader[6]);
                     //Debug.Log(height1);

                        //sqlitetest1.editProfileData obj2 = new sqlitetest1.editProfileData();
                        //obj2.height1 = height.text;\
                    }
					
					dbcon.Close();
					reader.Close();
				}
			}
		}
	}

	public dataRetrival getData()	//Public function that we will use
	{
		Debug.Log("the height from sqlitetest1 is "+newobj.heightGet.ToString());//height and user id
		Debug.Log("the userid from sqlitetest1 "+newobj.idGet.ToString());
		return newobj;
		//Debug.Log(newobj.heightGet.ToString());
	}

			
}

//For stats and history
public  class dataRetrival{
//relevant for Stats and history
    public string usernameGet;
	public string heightGet;
	public string weightGet;
	public string bmiGet;
	public string dobdayGet;
    public string dobmonthGet;
    public string dobyearGet;
    public string ageGet;
	public string idGet;
	

	public void getDataRetrieval( string height, string weight, string dob, string age, string bmi, string id){
		



		Debug.Log(height);
		Debug.Log(weight);
		Debug.Log(dob);
		Debug.Log(bmi);
        Debug.Log(age);
        Debug.Log(id);
	}
}





