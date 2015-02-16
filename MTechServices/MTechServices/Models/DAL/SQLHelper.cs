//===============================================================================
// This file is based on the Microsoft Data Access Application Block for .NET
// For more information please go to 
// http://msdn.microsoft.com/library/en-us/dnbda/html/daab-rm.asp
//===============================================================================

using System;
using System.Configuration;

using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace MTechServices.Models.DAL
{

    /// <summary>
    /// The SqlHelper class is intended to encapsulate high performance, 
    /// scalable best practices for common uses of SqlClient.
    /// </summary>
    public abstract class SqlHelper {

        //Database connection strings
        
        //public static readonly string ConnectionStringLocalTransaction =" Data Source=SCSTESTSERVER;Initial Catalog=Wosyst;Persist Security Info=True;User ID=sa;Password=blue22sky";
        public static readonly string ConnectionStringLocalTransaction = (ConfigurationSettings.AppSettings["SQLServer"]);

        // Hashtable to store cached parameters
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>

        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                    cmd.CommandTimeout = 0;
                    int val = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    return val;
                }
            }
        }

        public static int ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            // this is potentially in a transaction so we cannot close the connection right away
            using (SqlCommand cmd = new SqlCommand())
            {
                PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
                cmd.CommandTimeout = 0;
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the results</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            // NOTE: calling code is responsible for closing the SqlDataReader 
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, null, null, cmdType, cmdText, commandParameters);
                cmd.Connection = new SqlConnection(connectionString);
                cmd.CommandTimeout = 0;
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch
            {
                // need to dispose connection if there is an error
                CloseCommandConnection(cmd);
                throw;
            }
        }

        //Written by vasu
        public static SqlDataAdapter ExecuteAdapter(string cmdText, string connectionString)
        {
            try
            {
                SqlDataAdapter adr = new SqlDataAdapter(cmdText, connectionString);
                adr.SelectCommand.CommandTimeout = 0;
                return adr;
            }
            catch
            {
                throw;
            }
        }

        //Written by vasu
        public static SqlDataAdapter ExecuteAdapter(string connectionString, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            try
            {
                // don't explicit open the connection. If that's done, ADO.NET expects you to close it explicitly too
                // if only the connectionstring is set, when .Fill is executed, it will open and close the connection automatically
                PrepareCommand(cmd, null, null, CommandType.Text, cmdText, commandParameters);
                cmd.Connection = new SqlConnection(connectionString);
                SqlDataAdapter adr = new SqlDataAdapter(cmd);
                adr.SelectCommand.CommandTimeout = 0;
                return adr;
            }
            catch
            {
                // need to dispose connection if there is an error
                CloseCommandConnection(cmd);
                throw;
            }
        }



        //Written by vasu

        /// <summary>
        /// Execute a SqlCommand that returns the first column of the first record against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                    cmd.CommandTimeout = 0;
                    object val = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    return val;
                }
            }
        }

        /// <summary>
        /// Prepare a command for execution
        /// </summary>
        /// <param name="cmd">SqlCommand object</param>
        /// <param name="conn">SqlConnection object</param>
        /// <param name="trans">SqlTransaction object</param>
        /// <param name="cmdType">Cmd type e.g. stored procedure or text</param>
        /// <param name="cmdText">Command text, e.g. Select * from Products</param>
        /// <param name="cmdParms">SqlParameters to use in the command</param>
        // private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms) {
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn != null)
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                cmd.Connection = conn;
            }

            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        /// <summary>
        /// This function is used for inserting records into a table and also 
        /// getting the identity of the inserted record
        /// </summary>
        /// <param name="oled">SqlCommand object</param>
        /// <param name="olec">SqlConnection object</param>
        /// <param name="trans">SqlTransaction object</param>
        /// <param name="cmdType">Cmd type e.g. stored procedure or text</param>
        /// <param name="cmdText">Command text, e.g. Select * from Products</param>
        /// <param name="cmdParms">SqlParameters to use in the command</param>
        public static long InsertAndReturnIdentity(string connString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            long val = 0;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                    cmd.CommandTimeout = 0;
                    val = (long)cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();

                    try
                    {
                        cmd.CommandText = "SELECT @@IDENTITY";
                        cmd.CommandType = CommandType.Text;
                        val = 0;
                        val = long.Parse(cmd.ExecuteScalar().ToString());
                        if (cmd.Connection.State != ConnectionState.Closed) cmd.Connection.Close();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return val;
        }

        /// <summary>
        /// This function is used for replacing the characters which are keywords
        /// in sqlserver and replace with escape characters and return
        /// </summary>
        /// <param name="SqlQuery">string, This is Search Text</param>

        public static string QueryFormetter(string SqlQuery)
        {
            if (SqlQuery != null)
                return (SqlQuery.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]").Replace("'", "''"));
            else
                return SqlQuery;
        }

        /// <summary>
        /// This function is used for replacing the characters and wild card search  which are keywords
        /// in sqlserver and replace with escape characters and return
        /// </summary>
        /// <param name="SqlQuery">string, This is Search Text</param>

        public static string QueryWildCardFormetter(string SqlQuery)
        {

            string tempSqlQuery = string.Empty;
            if (SqlQuery != null)
            {
                bool firstQuote = SqlQuery.StartsWith("\"");
                bool endQuote = SqlQuery.EndsWith("\"");

                if (firstQuote && endQuote)
                {
                    tempSqlQuery = SqlQuery;
                    SqlQuery = SqlQuery.Remove(0, 1);
                    if (SqlQuery.Length > 0)
                    {
                        SqlQuery = SqlQuery.Remove(SqlQuery.Length - 1, 1);
                        SqlQuery = (SqlQuery.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]").Replace("'", "''"));
                    }
                    //else condition added by vinay for double quotes issue in search
                    else
                    {
                        SqlQuery = tempSqlQuery;
                        //commented by vinay for case 7899 
                        //Issue is with 5.2.2. Works as expected in 5.2.0.
                        //SqlQuery =  SqlQuery.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]").Replace("'", "''") ;
                        SqlQuery = "%" + SqlQuery.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]").Replace("'", "''") + "%";

                    }
                }
                else if (SqlQuery == "NULL")
                {
                    SqlQuery = "";

                }
                else if (SqlQuery.IndexOf("*") == -1 && SqlQuery.Length > 0)
                {
                    SqlQuery = SqlQuery.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]").Replace("'", "''");
                }

                else if (SqlQuery.IndexOf("*") != -1 && SqlQuery.Length > 0)
                {

                    SqlQuery = SqlQuery.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]").Replace("'", "''").Replace("*", "%");
                }

                else
                {
                    SqlQuery = "%";
                }
                //commented by vinay for case 7899 
                //Issue is with 5.2.2. Works as expected in 5.2.0.
                ////added by Srikanth.N on 09/29/2010 
                if (SqlQuery.IndexOf("%") == -1 && SqlQuery != "")
                    SqlQuery = "%" + SqlQuery + "%";
            }
            return SqlQuery;
        }


        private static void CloseCommandConnection(SqlCommand cmd)
        {
            if (cmd.Connection != null)
            {
                if (cmd.Connection.State == ConnectionState.Open)
                {
                    cmd.Connection.Close();
                }
            }
        }
        public static string QueryFormetterLIKE(string SqlQuery)
        {
            if (SqlQuery != null)
                return (SqlQuery.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]").Replace("'", "''"));
            else
                return SqlQuery;
        }

    }
}