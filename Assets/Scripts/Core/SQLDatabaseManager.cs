using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mono.Data.Sqlite;
using System.Globalization;

using System.Linq;
using System.IO;
using System.Data;

/// <summary>
/// Helper class to access the sqlite database. Note that all the functions are static and therefore no instance should be created for this class
/// </summary>
public class SQLDatabaseManager
{
    /// <summary>
    /// Establish a database connections and returns it
    /// </summary>
    /// <returns>IDbConnection of the connected database</returns>
    public static IDbConnection GetDatabaseConnection()
    {
        string connection = "URI=file:" + Application.persistentDataPath + "/UPMdatabase";

        IDbConnection dbcon = new SqliteConnection(connection);
        dbcon.Open();

        return dbcon;
    }

    /// <summary>
    /// Converts a quaternion to a string
    /// </summary>
    /// <param name="quat">Quaternion that should be converted</param>
    /// <returns>string of the converted quaternion</returns>
    public static string QuaternionToString(Quaternion quat)
    {
        return quat.x.ToString(CultureInfo.InvariantCulture) + " " + quat.y.ToString(CultureInfo.InvariantCulture) + " " + quat.z.ToString(CultureInfo.InvariantCulture) + " " + quat.w.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Convers a vector3 to a string
    /// </summary>
    /// <param name="vec">Vector3 that should be converted</param>
    /// <returns>string of the converted vector3</returns>
    public static string VectorToString(Vector3 vec)
    {
        return vec.x.ToString(CultureInfo.InvariantCulture) + " " + vec.y.ToString(CultureInfo.InvariantCulture) + " " + vec.z.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts a list of integers to a string
    /// </summary>
    /// <param name="l">List of integers that should be converted</param>
    /// <returns>string of the integer list</returns>
    public static string IntegerListToString(List<int> l)
    {
        string result = "";

        for (int i = 0; i < l.Count; i++)
        {
            result += l[i].ToString() + " ";
        }

        result = result.Remove(result.Length - 1);
        return result;
    }

    /// <summary>
    /// Converts a given string to an integer list
    /// </summary>
    /// <param name="input">string that should be converted</param>
    /// <returns>List of integers</returns>
    public static List<int> StringToIntegerList(string input)
    {
        List<int> result = new List<int>();

        string[] values = input.Split(' ');


        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == "")
            {
                continue;
            }

            result.Add(System.Int32.Parse(values[i]));
        }

        return result;
    }

    /// <summary>
    /// Converts back a sql vector object to a vector3
    /// </summary>
    /// <param name="vector">System.Object of that vector that should be converted</param>
    /// <returns>Vector3</returns>
    public static Vector3 ConvertSQLToVector3(System.Object vector)
    {
        string asString = System.Text.Encoding.Default.GetString((byte[])vector);
        string[] values = asString.Split(' ');

        if (values.Length != 3)
        {
            return new Vector3();
        }
        else
        {
            return new Vector3(float.Parse(values[0], CultureInfo.InvariantCulture), float.Parse(values[1], CultureInfo.InvariantCulture), float.Parse(values[2], CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Converts back a sql quaternion to a quaternion
    /// </summary>
    /// <param name="vector">System.Object of that quaternion that should be converted</param>
    /// <returns>Quaternion</returns>
    public static Quaternion ConvertSQLToQuaternion(System.Object vector)
    {
        string asString = System.Text.Encoding.Default.GetString((byte[])vector);
        string[] values = asString.Split(' ');

        if (values.Length != 4)
        {
            return new Quaternion();
        }
        else
        {
            return new Quaternion(float.Parse(values[0], CultureInfo.InvariantCulture), float.Parse(values[1], CultureInfo.InvariantCulture), float.Parse(values[2], CultureInfo.InvariantCulture), float.Parse(values[3], CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Executes an SQL command and returns the values from that command
    /// </summary>
    /// <param name="sqlCommand">string of the sql command</param>
    /// <param name="dbcon">IDbConnection of the conencted database</param>
    /// <param name="entryPerColumn">How many columns the database have we performing the query on</param>
    /// <returns></returns>
    public static List<System.Object> ExecuteSQLReturnCommand(string sqlCommand, IDbConnection dbcon, int entryPerColumn)
    {
        List<System.Object> variables = new List<System.Object>();

        IDbCommand dbcmd = dbcon.CreateCommand();
        dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = sqlCommand;

        IDataReader reader = dbcmd.ExecuteReader();

        while (reader.Read() == true)
        {
            for (int i = 0; i < entryPerColumn; i++)
            {
                variables.Add(reader[i]);
            }

        }


        return variables;
    }

    /// <summary>
    /// Executre the given sql command
    /// </summary>
    /// <param name="sqlCommand">string of the sql command</param>
    /// <param name="dbcon">IDbConnection of the established connection</param>
    /// <returns>Returns the stored id of the new entry. -1 otherwise </returns>
    public static int ExecuteSQLCommand(string sqlCommand, IDbConnection dbcon)
    {
        IDbCommand dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = sqlCommand;

        dbcmd.ExecuteReader();

        dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = "select last_insert_rowid()";
        IDataReader reader = dbcmd.ExecuteReader();

        if (reader.Read() == true)
        {
            return System.Int32.Parse(reader[0].ToString());
        }

        return -1;
    }

    /// <summary>
    /// Helper function to start a bulk insert
    /// </summary>
    /// <param name="sqlQuery">string of the sql query</param>
    /// <param name="dbcon">IDbConnection of the established database connection</param>
    /// <param name="entryPerRow">How my columns the table have we performing the query on</param>
    /// <param name="paramName">The name of the parameters used to bind the values</param>
    /// <returns></returns>
    public static (IDbTransaction, IDbCommand, List<IDbDataParameter>) BeginBulkInsert(string sqlQuery, IDbConnection dbcon, int entryPerRow, string paramName = "$value")
    {
        var transaction = dbcon.BeginTransaction();
        var command = dbcon.CreateCommand();
        command.CommandText = sqlQuery;

        List<IDbDataParameter> parameters = new List<IDbDataParameter>();

        for (int i = 0; i < entryPerRow; i++)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = paramName + i.ToString();
            command.Parameters.Add(parameter);
            parameters.Add(parameter);
        }

        return (transaction, command, parameters);
    }

    /// <summary>
    /// Helper function to end a bulk insert
    /// </summary>
    /// <param name="transaction">IDbTransaction of the started transaction</param>
    public static void EndBulkInsert(IDbTransaction transaction)
    {
        transaction.Commit();
    }

    /// <summary>
    /// Performs the bulks insert
    /// </summary>
    /// <param name="sqlQuery">string of the sql query</param>
    /// <param name="dbcon">IDbConnection of the established database connection</param>
    /// <param name="values">The string array of the values that should be stored in table/database</param>
    /// <param name="entryPerRow">How my columns the table have we performing the query on</param>
    /// <param name="paramName">The name of the parameters used to bind the values</param>
    public static void BulkInsert(string sqlQuery, IDbConnection dbcon, string[] values, int entryPerRow, string paramName = "$value")
    {
        var transaction = dbcon.BeginTransaction();
        var command = dbcon.CreateCommand();
        command.CommandText = sqlQuery;

        List<IDbDataParameter> parameters = new List<IDbDataParameter>();

        for(int i = 0; i < entryPerRow; i++)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = paramName + i.ToString();
            command.Parameters.Add(parameter);
            parameters.Add(parameter);
        }

        int rows = values.Length / entryPerRow;

        for(int i = 0; i < rows; i++)
        {
            for(int j = 0; j < entryPerRow; j++)
            {
                int idx = i * entryPerRow + j;

                parameters[j].Value = values[idx];
            }

            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    /// <summary>
    /// Performs a sql query as an transaction
    /// </summary>
    /// <param name="sqlCommands">string of the sqll commands that should be executed</param>
    /// <param name="dbcon">IDbConnection of the database connection</param>
    public static void ExecuteTransaction(List<string> sqlCommands, IDbConnection dbcon)
    {
        var transaction = dbcon.BeginTransaction();
        var transactionCMD = dbcon.CreateCommand();
        

        for (int j = 0; j < sqlCommands.Count; j++)
        {
            transactionCMD.CommandText = sqlCommands[j];
            transactionCMD.ExecuteNonQuery();

        }

        transaction.Commit();
    }
}
