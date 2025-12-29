using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using System.Linq;
using System.Collections;
using System.IO;
using System.Data;

using System.Globalization;

/// <summary>
/// Helper class for storing and loading all of the recording data for the virtual trainer / motiondetectionsystem. Note that all the functions are static and therefore no instance should be created for this class
/// </summary>
public class MotionMatchingDBManager
{

    public static readonly int[] CONNECTIONS =
    {
        0,1, //Hip 
        1,0, //Spine 2(duplication to hip for simpler alignment)
        2,1, //Head 
        
        //left arm
        5,4,
        4,3,
        3,1,

        //right arm
        8,7,
        7,6,
        6,1,

        //left leg
        11,10,
        10,9,
        9,0,

        //right leg
        14,13,
        13,12,
        12,0
    };

    private const int SQL_WORKOUT_TABLE_PARAMETERS = 5;
    private const int SQL_WORKOUT_FRAME_TABLE_PARAMETERS = 5;
    private const int SQL_WORKOUT_KEYPOINT_TABLE_PARAMETERS = 8;


    /// <summary>
    /// Creates all of the workout tables in the sqlite database if they doesnt exist before
    /// </summary>
    public static void CreateWorkoutTablesIfNotExisiting()
    {
        IDbConnection dbcon = SQLDatabaseManager.GetDatabaseConnection();

        IDbCommand dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = @"CREATE TABLE IF NOT EXISTS 'customWorkouts' (
            'id'    INTEGER,
	        'name'  TEXT,
	        'avgFPS'    REAL,
	        'recordedBoneIDs'   BLOB,
            'instructionPointsOfInterest' BLOB,
	        PRIMARY KEY('id')
        ); ";
        dbcmd.ExecuteReader();

        dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = @"CREATE TABLE IF NOT EXISTS 'customWorkoutFrame' (
            'id'    INTEGER,
            'nameID'    INTEGER,
            'frameIndex'    INTEGER,
            'relPosition'   BLOB,
            'fps'   REAL,
            PRIMARY KEY('id')
        ); ";
        dbcmd.ExecuteReader();

        dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = @"CREATE TABLE IF NOT EXISTS 'customWorkoutKeypoints' (
            'id'    INTEGER,
            'frameID'    INTEGER,
            'empty' INTEGER,
            'boneID' INTEGER,
            'position'  BLOB,
            'localPosition' BLOB,
            'rotation'  BLOB,
            'localRotation' BLOB,
            PRIMARY KEY('id')
            );";
        dbcmd.ExecuteReader();

        dbcon.Close();
    }

    /// <summary>
    /// Creates all of the user recording tables in the sqlite database if they doesnt exist before
    /// </summary>
    public static void CreateUserRecordingTablesIfNotExisiting()
    {
        IDbConnection dbcon = SQLDatabaseManager.GetDatabaseConnection();

        IDbCommand dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = @"CREATE TABLE IF NOT EXISTS 'userRecordingWorkouts' (
            'id'    INTEGER,
	        'userID'  Integer,
            'tag' TEXT,
	        'avgFPS'    REAL,
            'previewReps' REAL,
            'instructionReps' REAL,
            'workoutReps' REAL,
            'previewTime' REAL,
            'instructionTime' REAL,
            'workoutTime' REAL,
            'percentage' REAL,
	        PRIMARY KEY('id')
        ); ";
        dbcmd.ExecuteReader();

        dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = @"CREATE TABLE IF NOT EXISTS 'userWorkoutFrame' (
            'id'    INTEGER,
            'nameID'    INTEGER,
            'frameIndex'    INTEGER,
            'relPosition'   BLOB,
            'fps'   REAL,
            PRIMARY KEY('id')
        ); ";
        dbcmd.ExecuteReader();

        dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = @"CREATE TABLE IF NOT EXISTS 'userWorkoutKeypoints' (
            'id'    INTEGER,
            'frameID'    INTEGER,
            'empty' INTEGER,
            'boneID' INTEGER,
            'position'  BLOB,
            'localPosition' BLOB,
            'rotation'  BLOB,
            'localRotation' BLOB,
            PRIMARY KEY('id')
            );";
        dbcmd.ExecuteReader();

        dbcon.Close();
    }


    /// <summary>
    /// Get the list of bones that are also connect to the desired bones without duplicates
    /// Note in theory its faster to sort the list in n* log(n) and then remove them in one iteration N which makes it effictively run in n* log(n) + n and not in n^2 like this one here. But since we are not dealing with many data is not worth the extra steps
    /// </summary>
    /// <param name="boneIds">List of int containing the bonesIDS we are interested in</param>
    /// <returns></returns>
    public static List<int> GetConnectedBones(List<int> boneIds)
    {
        List<int> focusBones = new List<int>();

        for (int i = 0; i < boneIds.Count; i++)
        {
            focusBones.Add(boneIds[i]);

            for (int j = 0; j < CONNECTIONS.Length; j += 2)
            {
                if (CONNECTIONS[j + 1] == boneIds[i])
                {
                    //containment check to look for duplicates. 
                    bool isContained = false;

                    for (int k = 0; k < focusBones.Count; k++)
                    {
                        if (focusBones[k] == CONNECTIONS[j])
                        {
                            isContained = true;
                            break;
                        }
                    }

                    if (isContained == false)
                    {
                        focusBones.Add(CONNECTIONS[j]);
                    }


                }
            }
        }

        return focusBones;
    }

    /// <summary>
    /// Creates a new user id in the sqlite table
    /// </summary>
    public static void CreateNewUser()
    {
        IDbConnection dbcon = SQLDatabaseManager.GetDatabaseConnection();

        List<System.Object> values = SQLDatabaseManager.ExecuteSQLReturnCommand("INSERT INTO 'main'.'userInformation' ('height','weight','age','gender') VALUES('0.0','0.0','0','-');", dbcon, 1);
        dbcon.Close();
    }

    /// <summary>
    /// Returns the user id from the table 
    /// </summary>
    /// <returns>int of the user id in the database</returns>
    public static int GetUserStudyID()
    {
        IDbConnection dbcon = SQLDatabaseManager.GetDatabaseConnection();

        List<System.Object> values = SQLDatabaseManager.ExecuteSQLReturnCommand("SELECT id from 'main'.'userInformation' order by ROWID DESC limit 1;", dbcon, 1);
        dbcon.Close();

        return int.Parse(values[0].ToString());
    }

    /// <summary>
    /// Returns a list of all the stored recording in the database
    /// </summary>
    /// <returns>List of string of the recording names in the database</returns>
    public static List<string> GetRecordingNames()
    {
        List<string> workoutList = new List<string>();
        IDbConnection dbcon = SQLDatabaseManager.GetDatabaseConnection();


        List<System.Object> values = SQLDatabaseManager.ExecuteSQLReturnCommand("SELECT name FROM 'main'.'customWorkouts' ;", dbcon, 1);

        for (int i = 0; i < values.Count; i++)
        {
            workoutList.Add(values[i].ToString());
        }


        dbcon.Close();
        return workoutList;
    }


    /// <summary>
    /// Saves the player recording in the database
    /// </summary>
    /// <param name="userId">int of the user id</param>
    /// <param name="tag">string of the used tag for the recording</param>
    /// <param name="previews">inf of how often the user watched the preview</param>
    /// <param name="instructions">inf of how often the user performed the instructions repition</param>
    /// <param name="reps">int of how often the user performed the workout repition</param>
    /// <param name="previewTime">float of how long it took for the user to watch the preview</param>
    /// <param name="instructionTime">float of how long it took for the user to perform the instruction mode</param>
    /// <param name="workoutTime">float of how long it took for the user to perform the workout mode</param>
    /// <param name="matchingPercentage">float of the matching percentage the system gave the user</param>
    /// <param name="playerRecording">List of FBTRecorder.Frame containg the actualy data of the user movements</param>
    public static void SavePlayerRecordings(int userId, string tag, int previews, int instructions, int reps, long previewTime, long instructionTime, long workoutTime, float matchingPercentage, List<FBTRecorder.Frame> playerRecording)
    {

        if (playerRecording.Count <= 0)
        {
            return;
        }

        IDbConnection dbcon = SQLDatabaseManager.GetDatabaseConnection();

        float avgFPS = 0.0f;

        for (int i = 0; i < playerRecording.Count; i++)
        {
            avgFPS += playerRecording[i].fps;
        }

        avgFPS /= (float)playerRecording.Count;


        string cmd = "INSERT INTO 'main'.'userRecordingWorkouts'('userID', 'tag', 'avgFPS','previewReps','instructionReps','workoutReps','previewTime','instructionTime','workoutTime', 'percentage' ) VALUES ('";
        cmd += userId.ToString() + "','" + tag + "', '" + avgFPS.ToString(CultureInfo.InvariantCulture) + "','" + previews.ToString(CultureInfo.InvariantCulture) + "','" + instructions.ToString(CultureInfo.InvariantCulture) + "','" + reps.ToString(CultureInfo.InvariantCulture);
        cmd += "','" + previewTime.ToString(CultureInfo.InvariantCulture) + "','" + instructionTime.ToString(CultureInfo.InvariantCulture) + "','" + workoutTime.ToString(CultureInfo.InvariantCulture) + "','" + matchingPercentage.ToString(CultureInfo.InvariantCulture) + "');";
       
        int recID = SQLDatabaseManager.ExecuteSQLCommand(cmd, dbcon);
        string recIDString = recID.ToString();


        FBTRecorder.Frame frame = playerRecording[0];
        cmd = "INSERT INTO 'main'.'userWorkoutFrame'('nameID', 'frameIndex', 'relPosition', 'fps') VALUES";
        cmd += " ('" + recIDString + "', '0', '" + SQLDatabaseManager.VectorToString(frame.relPosition) + "', '" + frame.fps.ToString(CultureInfo.InvariantCulture) + "') ";

        int frameID = SQLDatabaseManager.ExecuteSQLCommand(cmd, dbcon);

        string sqlBulk = "INSERT INTO 'main'.'userWorkoutFrame'('nameID', 'frameIndex', 'relPosition', 'fps') VALUES ($value0,$value1,$value2,$value3)";
        var bulkCommand = SQLDatabaseManager.BeginBulkInsert(sqlBulk, dbcon, 4);

        for (int i = 1; i < playerRecording.Count; i++)
        {
            frame = playerRecording[i];

            bulkCommand.Item3[0].Value = recIDString;
            bulkCommand.Item3[1].Value = i.ToString();
            bulkCommand.Item3[2].Value = SQLDatabaseManager.VectorToString(frame.relPosition);
            bulkCommand.Item3[3].Value = frame.fps.ToString(CultureInfo.InvariantCulture);

            bulkCommand.Item2.ExecuteNonQuery();
        }

        SQLDatabaseManager.EndBulkInsert(bulkCommand.Item1);

        sqlBulk = "INSERT INTO 'main'.'userWorkoutKeypoints'('frameID', 'empty', 'boneID', 'position', 'localPosition', 'rotation' , 'localRotation' ) VALUES ($value0,$value1,$value2,$value3,$value4,$value5,$value6)";
        bulkCommand = SQLDatabaseManager.BeginBulkInsert(sqlBulk, dbcon, 7);

        for (int i = 0; i < playerRecording.Count; i++)
        {
            frame = playerRecording[i];

            for (int j = 0; j < frame.keypoints.Count; j++)
            {
                FBTRecorder.Keypoint key = frame.keypoints[j];

                bulkCommand.Item3[0].Value = (frameID + i).ToString();
                bulkCommand.Item3[1].Value = "0";
                bulkCommand.Item3[2].Value = j.ToString();
                bulkCommand.Item3[3].Value = SQLDatabaseManager.VectorToString(key.position);
                bulkCommand.Item3[4].Value = SQLDatabaseManager.VectorToString(key.localPosition);
                bulkCommand.Item3[5].Value = SQLDatabaseManager.QuaternionToString(key.rotation);
                bulkCommand.Item3[6].Value = SQLDatabaseManager.QuaternionToString(key.localRotation);

                bulkCommand.Item2.ExecuteNonQuery();
            }

        }

        SQLDatabaseManager.EndBulkInsert(bulkCommand.Item1);

        dbcon.Close();

    }

    /// <summary>
    /// Saves the trainer recording in the sqlite database
    /// </summary>
    /// <param name="name">string of the name</param>
    /// <param name="recordedAnimation">List of FBTRecorder.Frame containg all of the recording of the trainers movement</param>
    public static void SaveTrainerRecording(string name, FBTRecorder.Recording recordedAnimation)
    {
        IDbConnection dbcon = SQLDatabaseManager.GetDatabaseConnection();

        //INSERT INTO "main"."customWorkouts" ("id", "name", "avgFPS", "recordedBoneIDs") VALUES ('1', 'test', '32.2', '0 1 2 3 4 5 6');
        string cmd = "INSERT INTO 'main'.'customWorkouts'('name', 'avgFPS', 'recordedBoneIDs', 'instructionPointsOfInterest') VALUES ('" + name + "', '" + recordedAnimation.avgFPS.ToString(CultureInfo.InvariantCulture) + "', '" + SQLDatabaseManager.IntegerListToString(recordedAnimation.recordedBones) + "', '');";
        int recID = SQLDatabaseManager.ExecuteSQLCommand(cmd, dbcon);

        for (int i = 0; i < recordedAnimation.frames.Count; i++)
        {
            FBTRecorder.Frame frame = recordedAnimation.frames[i];

            cmd = "INSERT INTO 'main'.'customWorkoutFrame'('nameID', 'frameIndex', 'relPosition', 'fps')";
            cmd += "VALUES('" + recID + "', '" + i.ToString() + "', '" + SQLDatabaseManager.VectorToString(frame.relPosition) + "', '" + frame.fps.ToString(CultureInfo.InvariantCulture) + "');";

            int frameID = SQLDatabaseManager.ExecuteSQLCommand(cmd, dbcon);
            List<string> commands = new List<string>();

            for (int j = 0; j < frame.keypoints.Count; j++)
            {
                FBTRecorder.Keypoint key = frame.keypoints[j];
                cmd = "INSERT INTO 'main'.'customWorkoutKeypoints'('frameID', 'empty', 'boneID', 'position', 'localPosition', 'rotation' , 'localRotation' ) VALUES ";

                if (key.empty == true)
                {
                    cmd += "('" + frameID.ToString() + "','1','" + j.ToString() + "','" + SQLDatabaseManager.VectorToString(key.position) + "', '" + SQLDatabaseManager.VectorToString(key.localPosition) + "','" + SQLDatabaseManager.QuaternionToString(key.rotation) + "','" + SQLDatabaseManager.QuaternionToString(key.localRotation) + "')";
                }
                else
                {
                    cmd += "('" + frameID.ToString() + "','0','" + j.ToString() + "','" + SQLDatabaseManager.VectorToString(key.position) + "', '" + SQLDatabaseManager.VectorToString(key.localPosition) + "','" + SQLDatabaseManager.QuaternionToString(key.rotation) + "','" + SQLDatabaseManager.QuaternionToString(key.localRotation) + "')";
                }

                commands.Add(cmd);
            }

            SQLDatabaseManager.ExecuteTransaction(commands, dbcon);
        }

        dbcon.Close();
    }

    /// <summary>
    /// Load a recording by the given name
    /// </summary>
    /// <param name="name">string of the recording name</param>
    /// <returns>FBTRecorder.Recording of the loaded recording</returns>
    public static FBTRecorder.Recording LoadRecording(string name)
    {
        IDbConnection dbcon = SQLDatabaseManager.GetDatabaseConnection();

        List<System.Object> values = SQLDatabaseManager.ExecuteSQLReturnCommand("SELECT * FROM 'main'.'customWorkouts' WHERE name='" + name.ToString() + "';", dbcon, SQL_WORKOUT_TABLE_PARAMETERS);

        FBTRecorder.Recording recordedAnimation = new FBTRecorder.Recording();
        recordedAnimation.frames = new List<FBTRecorder.Frame>();

        if (values.Count > 0)
        {
            int recID = System.Int32.Parse(values[0].ToString());

            recordedAnimation.name = values[1].ToString();
            recordedAnimation.avgFPS = (float)values[2];
            recordedAnimation.recordedBones = SQLDatabaseManager.StringToIntegerList(System.Text.Encoding.Default.GetString((byte[])values[3]));
            recordedAnimation.instructionPointsOfInterest = SQLDatabaseManager.StringToIntegerList(System.Text.Encoding.Default.GetString((byte[])values[4]));
            recordedAnimation.involvedBoneCount = MotionMatchingDBManager.GetConnectedBones(recordedAnimation.recordedBones).Count;

            int paramsCount = SQL_WORKOUT_FRAME_TABLE_PARAMETERS + SQL_WORKOUT_KEYPOINT_TABLE_PARAMETERS;
            string sql = "SELECT * FROM 'main'.'customWorkoutKeypoints' as cs INNER JOIN 'main'.'customWorkoutFrame' as cf ON cs.frameID = cf.id WHERE nameID = '" + recID.ToString() + "' ORDER by cf.id, cs.boneID;";

            List<System.Object> frameValues = SQLDatabaseManager.ExecuteSQLReturnCommand(sql, dbcon, paramsCount);
            int numberOfData = frameValues.Count / paramsCount;

            int cFrame = -1;
            FBTRecorder.Frame frame = null;


            for (int i = 0; i < numberOfData; i++)
            {
                int j = i * paramsCount;
                int frameID = System.Int32.Parse(frameValues[j + 8].ToString());


                if (frameID != cFrame)
                {
                    if (frame != null)
                    {
                        for (int k = 0; k < recordedAnimation.recordedBones.Count; k++)
                        {
                            int fIdx = recordedAnimation.recordedBones[k];
                            frame.keypoints[fIdx].ignore = false;
                        }

                        recordedAnimation.frames.Add(frame);
                    }

                    frame = new FBTRecorder.Frame();
                    frame.keypoints = new List<FBTRecorder.Keypoint>();

                    frame.relPosition = SQLDatabaseManager.ConvertSQLToVector3(frameValues[j + 11]);
                    frame.fps = float.Parse(frameValues[j + 12].ToString());
                    cFrame = frameID;
                }


                FBTRecorder.Keypoint key = new FBTRecorder.Keypoint();
                key.ignore = true;

                key.position = SQLDatabaseManager.ConvertSQLToVector3(frameValues[j + 4]);
                key.localPosition = SQLDatabaseManager.ConvertSQLToVector3(frameValues[j + 5]);
                key.rotation = SQLDatabaseManager.ConvertSQLToQuaternion(frameValues[j + 6]);
                key.localRotation = SQLDatabaseManager.ConvertSQLToQuaternion(frameValues[j + 7]);

                if (frameValues[j + 2].ToString() == "0")
                {
                    key.empty = false;
                }
                else
                {
                    key.empty = true;
                }

                frame.keypoints.Add(key);

            }


        }

        dbcon.Close();

        return recordedAnimation;
    }
}
