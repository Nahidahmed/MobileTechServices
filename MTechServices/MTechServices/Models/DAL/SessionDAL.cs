using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Data.SqlClient;

using log4net;
using MTechServices.Models.Entity;

namespace MTechServices.Models.DAL{
    public class SessionDAL {
        private string connectionString;
        protected string ConnectionString {
            get {
                if (string.IsNullOrEmpty(connectionString)) {
                    connectionString = ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString;
                }

                return connectionString;
            }
        }

        private static readonly ILog logger = LogManager.GetLogger(typeof(SessionDAL));

        private UserDAL userDAL;
        protected UserDAL UserDAL {
            get { return userDAL ?? (userDAL = new UserDAL()); }
        }

        public string CreateSession(string username, string appName)
        {
            string sessionId = string.Empty;
            using (SqlConnection connection = new SqlConnection(ConnectionString)) {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction()) {
                    long userId = UserDAL.GetUserId(username);
                    try {
                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand()) {
                            command.CommandText = "UPDATE [dbo].[UserSessions] SET [EndDateTime] = CURRENT_TIMESTAMP WHERE [UserID] = @user_id and ApplicationName = @applicationName";
                            command.Parameters.Add(new SqlParameter {ParameterName = "@user_id", DbType = DbType.Int64, Value = userId});
                            command.Parameters.Add(new SqlParameter { ParameterName = "@applicationName", DbType = DbType.String, Value = appName });
                            command.Transaction = transaction;

                            command.ExecuteNonQuery();
                        }
                        // create a new session for the user
                        using (SqlCommand command = connection.CreateCommand()) {
                            sessionId = Guid.NewGuid().ToString();
                            command.CommandText = "INSERT INTO [dbo].[UserSessions] ([SessionID], [UserID], [StartDateTime],[ApplicationName]) values (@session_id, @user_id, CURRENT_TIMESTAMP,@applicationName)";
                            command.Parameters.Add(new SqlParameter {ParameterName = "@session_id", DbType = DbType.String, Value = sessionId});
                            command.Parameters.Add(new SqlParameter {ParameterName = "@user_id", DbType = DbType.Int64, Value = userId});
                            command.Parameters.Add(new SqlParameter { ParameterName = "@applicationName", DbType = DbType.String, Value = appName });
                            command.Transaction = transaction;
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    } catch (Exception) {
                        transaction.Rollback();
                    }
                }
            }

            return sessionId;
        }

        public void EndSession(string sessionId) {
            using (SqlConnection connection = new SqlConnection(ConnectionString)) {
                connection.Open();

                // end any open sessions for the user first
                using (SqlCommand command = connection.CreateCommand()) {
                    command.CommandText = "UPDATE [dbo].[UserSessions] SET [EndDateTime] = CURRENT_TIMESTAMP WHERE [SessionID] = @session_id AND [EndDateTime] IS NULL";
                    command.Parameters.Add(new SqlParameter { ParameterName = "@session_id", DbType = DbType.String, Value = sessionId });

                    int result = command.ExecuteNonQuery();

                    if (result != 1) {
                        throw new Exception("Invalid Session ID: " + sessionId);
                    }
                }
            }
        }

        public bool IsActiveSession(string sessionId) {
            int count;

            using (SqlConnection connection = new SqlConnection(ConnectionString)) {
                connection.Open();

                // end any open sessions for the user first
                using (SqlCommand command = connection.CreateCommand()) {
                    command.CommandText = "SELECT count(*) FROM [dbo].[UserSessions] WHERE [SessionID] = @session_id AND [EndDateTime] IS NULL";
                    command.Parameters.Add(new SqlParameter { ParameterName = "@session_id", DbType = DbType.String, Value = sessionId });

                    count = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            return count == 1;
        }

        public User GetUserBySession(string sessionId, string appName)
        {
            User user = null;

            using (SqlConnection connection = new SqlConnection(ConnectionString)) {
                connection.Open();

                string username = string.Empty;

                // end any open sessions for the user first
                using (SqlCommand command = connection.CreateCommand()) {
                    //Modified on 29th Jan 2014 for ISS - 5153 by Srikanth Nuvvula: For the given session ID, if the end date time in usersessions table
                    //is null only then the user should be validated else consider that its invalid session.(given by Nahid)
                    //command.CommandText = "SELECT u.[user] FROM [dbo].[Security] u, [dbo].[UserSessions] s WHERE s.[SessionID] = @session_id and u.[SecPrimaryId] = s.[UserID]";                    
                    command.CommandText = @"SELECT u.[user] FROM [dbo].[Security] u, [dbo].[UserSessions] s 
                    WHERE s.[SessionID] = @session_id 
                    and u.[SecPrimaryId] = s.[UserID] 
                    and s.[ApplicationName] = @applicationName   
                    and s.[enddatetime] is  null";
                    command.Parameters.Add(new SqlParameter { ParameterName = "@session_id", DbType = DbType.String, Value = sessionId });
                    command.Parameters.Add(new SqlParameter { ParameterName = "@applicationName", DbType = DbType.String, Value = appName });

                    DataTable data = new DataTable();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                    dataAdapter.Fill(data);

                    if (data.Rows.Count == 1) {
                        username = Convert.ToString(data.Rows[0]["user"]);
                    }
                }

                if (!string.IsNullOrEmpty(username)) {
                    user = UserDAL.GetUser(username);
                }
            }

            return user;
        }

        public string GetCurrentSession(string username) {
            string sessionId;

            using (SqlConnection connection = new SqlConnection(ConnectionString)) {
                connection.Open();

                long userId = UserDAL.GetUserId(username);

                // end any open sessions for the user first
                using (SqlCommand command = connection.CreateCommand()) {
                    command.CommandText = "SELECT [SessionID] FROM [dbo].[UserSessions] WHERE [UserID] = @user_id AND [EndDateTime] IS NULL";
                    command.Parameters.Add(new SqlParameter { ParameterName = "@user_id", DbType = DbType.String, Value = userId });

                    sessionId = Convert.ToString(command.ExecuteScalar());
                }
            }

            return sessionId;
        }

        //Author/Date/Issue - M.D.Prasad/8th July 2014/Version compatibility/Version Validation
        public bool VersionValidate(string appName, string app_AppVersion, string app_WSVersion)
        {
            bool isCompatibilityVersion = false;
            string ws_AppVersion = string.Empty;

            if (appName.Equals("mTech.iOS"))
                ws_AppVersion = Resource.iOSAPPVERSION;
            else if (appName.Equals("mTech.Android"))
                ws_AppVersion = Resource.ANDROIDAPPVERSION;

            string ws_WSVersion = Resource.WSVERSION;

            //string ws_AppVersion = ConfigurationManager.AppSettings["Ser_AppVer"].ToString();
            //string ws_WSVersion = ConfigurationManager.AppSettings["Ser_SerVer"].ToString();
            if (app_AppVersion == ws_AppVersion)
                isCompatibilityVersion = true;
            else if (app_WSVersion == ws_WSVersion)
                isCompatibilityVersion = true;
            return isCompatibilityVersion;
        }

        //Author/Date/Issue - M.D.Prasad/12th June 2014/ ISS - 5395 Upload iGotIt device logs
        public void UploadExceptionLogFromDevice(string filename, byte[] data)
        {
            try
            {
                MemoryStream ms = new MemoryStream(data);
                string fname = string.Format("{0}\\{1}", System.Web.Hosting.HostingEnvironment.MapPath("~/Logs"), filename);
                FileStream fs = new FileStream(fname, FileMode.Create);
                ms.WriteTo(fs);
                fs.Close();
                fs.Dispose();
                ms.Flush();
                ms.Close();
            }
            catch (Exception ex)
            {
                
                logger.InfoFormat(Environment.NewLine
                        + "UploadExceptionLogFromDevice Exception=> " + Environment.NewLine
                        + ex.Message);
            }
        }

        //private void Writelog(string e)
        //{
        //    string folderPath = "D:\\MLM\\Thread#2\\MAEServices\\StCroixWebApp\\Logs";

        //    //set up a filestream
        //    FileStream fs = new FileStream(folderPath + "\\" + "Log.txt", FileMode.OpenOrCreate, FileAccess.Write);

        //    StreamWriter sw = new StreamWriter(fs);
        //    sw.BaseStream.Seek(0, SeekOrigin.End);
        //    sw.WriteLine(DateTime.Now + " " + e);
        //    sw.Flush();
        //    sw.Close();
        //}
    }
}
