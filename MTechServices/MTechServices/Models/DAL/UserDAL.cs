using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using MTechServices.Models.Entity;

namespace MTechServices.Models.DAL{
    public class UserDAL {
        private static readonly ASCIIEncoding encoder = new ASCIIEncoding();

        private SessionDAL sessionDAL;
        private SessionDAL SessionDAL
        {
            get { return sessionDAL ?? (sessionDAL = new SessionDAL()); }
        }

        private string connectionString;
        protected string ConnectionString {
            get {
                if (string.IsNullOrEmpty(connectionString)) {
                    connectionString = ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString;
                }

                return connectionString;
            }
        }

        public User GetUserBySession(string sessionId, string appName)
        {
            User user = null;
           

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string username = string.Empty;

                // end any open sessions for the user first
                using (SqlCommand command = connection.CreateCommand())
                {
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

                    if (data.Rows.Count == 1)
                    {
                        username = Convert.ToString(data.Rows[0]["user"]);
                    }
                }

                if (!string.IsNullOrEmpty(username))
                {
                    user = GetUser(username);
                }
            }

            return user;
        }

        public User GetUser(string username) {
            User user = null;

            using (SqlConnection connection = new SqlConnection(ConnectionString)) {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand()) {
                    command.CommandText = "SELECT [SecPrimaryId], [User], [LastUpdateTime] FROM [dbo].[Security] WHERE lower([User]) = lower(@username)";
                    command.Parameters.Add(new SqlParameter { ParameterName = "@username", DbType = DbType.String, Value = username });

                    DataTable data = new DataTable();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                    dataAdapter.Fill(data);

                    if (data.Rows.Count == 1) {
                        user = new User {
                            ID = Convert.ToInt64(data.Rows[0]["SecPrimaryId"]),
                            Username = Convert.ToString(data.Rows[0]["User"]),
                            LastUpdateDateTime = Convert.ToDateTime(data.Rows[0]["LastUpdateTime"]),
                            FinalDateTime = null
                        };
                    }
                }
            }

            return user;
        }

        public User[] GetUsers(DateTime lastUpdateTime) {
            List<User> users = new List<User>();

            using (SqlConnection connection = new SqlConnection(ConnectionString)) {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand()) {
                    command.CommandText = "SELECT [SecPrimaryId], [User], [LastUpdateTime] FROM [dbo].[Security]";

                    if (lastUpdateTime != DateTime.MinValue) {
                        command.CommandText += " WHERE [LastUpdateTime] >= @last_update_time";
                        command.Parameters.Add(new SqlParameter { ParameterName = "@last_update_time", DbType = DbType.DateTime, Value = lastUpdateTime });
                    }

                    DataTable data = new DataTable();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                    dataAdapter.Fill(data);

                    users.AddRange(from DataRow row in data.Rows
                                   select new User {
                                       ID = Convert.ToInt64(row["SecPrimaryId"]),
                                       Username = Convert.ToString(row["User"]),
                                       LastUpdateDateTime = Convert.ToDateTime(data.Rows[0]["LastUpdateTime"]),
                                   });
                }
            }

            return users.ToArray();
        }

        public long[] GetDeletedUsers(DateTime lastUpdateTime) {
            List<long> userIds = new List<long>();

            using (SqlConnection connection = new SqlConnection(ConnectionString)) {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand()) {
                    command.CommandText = "SELECT [SecPrimaryId] FROM [dbo].[Security] WHERE [LastUpdateTime] >= @last_update_time";
                    command.Parameters.Add(new SqlParameter { ParameterName = "@last_update_time", DbType = DbType.DateTime, Value = lastUpdateTime });

                    DataTable data = new DataTable();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                    dataAdapter.Fill(data);

                    userIds.AddRange(from DataRow row in data.Rows
                                     select Convert.ToInt64(row["SecPrimaryId"]));
                }
            }
             
            return userIds.ToArray();
        }

        public string GetPassword(string username) {
            string password = string.Empty;
            string userArchived = string.Empty;
            using (SqlConnection connection = new SqlConnection(ConnectionString)) {
                connection.Open();

                // end any open sessions for the user first
                using (SqlCommand command = connection.CreateCommand()) {
                    command.CommandText = "SELECT [Password],[Archived] FROM [dbo].[Security] u WHERE lower([User]) = @username";
                    command.Parameters.Add(new SqlParameter { ParameterName = "@username", DbType = DbType.String, Value = username.ToLower() });

                    DataTable data = new DataTable();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                    dataAdapter.Fill(data);

                    if (data.Rows.Count == 1) {
                        password = encoder.GetString((byte[])data.Rows[0]["Password"]);
                        userArchived = data.Rows[0]["Archived"].ToString();
                    }
                }
            }

            if (userArchived == "Archived")
                return "User is archived";
            else
                return password;
        }

        public long GetUserId(string username) {
            using (SqlConnection connection = new SqlConnection(ConnectionString)) {
                connection.Open();

                return GetUserId(connection, username);
            }
        }

        public string UserAuthentication(string username, string password, string appName)
        {
            string retVal = string.Empty;
            SecurityDetails userObj = GetUserSecPrimaryid(username);

            if (userObj.SecPrimaryId != 0){
                if (userObj.RoleMTech != null){
                    if (userObj.RoleMTech == 1){
                        //Check Authentication Setting is on
                        //Check if for the Secprimaryid, a windows user is configured
                        ActiveDirectorySettings rsi = GetSystemSettings();
                        if (rsi.ActiveDirectoryEnabled == true && userObj.AuthenticateType == 0){
                            if (username.ToLower().Trim() == userObj.WindowsUser.ToLower().Trim()){
                                //Perform AD Authentication
                                if (CheckDomainUserForUserValidate(rsi, username, password)){
                                    retVal = BasicAuthentication(userObj.UserName, string.Empty, appName, userObj.AuthenticateType);
                                }else{
                                    return "Invalid username/password";
                                }
                            }else{
                                return "This User is mapped to a Windows User. Please use Windows User account name/password.";
                            }
                        }else{
                            //Perform Normal Authentication
                            retVal = BasicAuthentication(username, password, appName, userObj.AuthenticateType);
                        }
                    }else{
                        //Invalid MAE User
                        return "User does not have MTech access";
                    }
                }else{
                    //Invalid MAE User
                    return "User does not have MTech access";
                }
            }else{
                return "Invalid username/password";
            }
            return retVal;
        }

        private SecurityDetails GetUserSecPrimaryid(string username)
        {
            SecurityDetails retObj = new SecurityDetails();

            string sqlStatement = @"Select SECPrimaryID, RoleMobileTech, Authenticatetype, Windowsuser, [User] from Security where lower([user]) = lower(@username) 
           or lower(windowsuser) = lower(@username) ";
            SqlParameter parameters = new SqlParameter { ParameterName = "@username", DbType = DbType.String, Value = username };


            using (SqlDataReader sqlReader = SqlHelper.ExecuteReader(SqlHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlStatement, parameters))
            {
                ASCIIEncoding encoding = new ASCIIEncoding();
                while (sqlReader.Read()){
                    retObj.SecPrimaryId = sqlReader["SECPrimaryID"] != DBNull.Value ? sqlReader.GetInt64(0) : Convert.ToInt64(0);
                    retObj.RoleMTech =  sqlReader["RoleMobileTech"] != DBNull.Value ? sqlReader.GetByte(1) : Convert.ToByte(0);
                    retObj.AuthenticateType = sqlReader.GetBoolean(2) == true ? Convert.ToByte(1) : Convert.ToByte(0);
                    retObj.WindowsUser = sqlReader["Windowsuser"] != DBNull.Value ? sqlReader.GetString(3) : Convert.ToString(0);
                    retObj.UserName = sqlReader["User"] != DBNull.Value ? sqlReader.GetString(4) : Convert.ToString(0);                   
                }
                sqlReader.Close();
            }


             return retObj;
        }

        public ActiveDirectorySettings GetSystemSettings()
        {
            ActiveDirectorySettings sysADSettings = GetSystenSettings();

            return sysADSettings;
                
        }

        public string BasicAuthenticaton(string username, string password, string appName, string app_AppVersion, string app_WSVersion, byte authenticateType)
        {
            if (CheckPermission(username, appName))
            {
                string userPassword = GetPassword(username);
                if (userPassword == "User is archived")
                {
                     return "User is archived";
                }
                else
                {
                    if (authenticateType == 1)
                    {
                        string passwordHash = AuthUtility.HashPassword(password);
                        if (userPassword != passwordHash)
                        {
                            return "Invalid username/password";
                        }
                    }

                    //Author/Date/Issue - M.D.Prasad/8th July 2014/Version compatibility/Version Validation
                    string sessionID = string.Empty;
                    bool isCompatibilityVersion = SessionDAL.VersionValidate(appName, app_AppVersion, app_WSVersion);
                    if (isCompatibilityVersion)
                        sessionID = SessionDAL.CreateSession(username, appName);
                    else
                    {
                        string strExceptionTrace = "This application is not compatible with the specified Web Service. Please install the correct application or connect to correct web service.";
                        return strExceptionTrace;
                    }

                    return sessionID;
                }
            }
            else
            {
                return "User does not have MTech access";
            }
        }

        public string BasicAuthentication(string username, string password, string appName, byte authenticateType)
        {
            string retVal = string.Empty;

            if (CheckPermission(username, appName)){
                if (CheckSyncParams(username)){
                    string userPassword = GetPassword(username);
                    if (userPassword == "User is archived"){
                        return "User is archived" ;
                    }else{
                        if (authenticateType == 1)
                        {
                            string passwordHash = AuthUtility.HashPassword(password);
                            if (userPassword != passwordHash)
                            {
                                return "Invalid username/password";
                            }
                        }

                        return SessionDAL.CreateSession(username, appName);
                    }
                }else{
                    return "Sync Params Not Set" ;
                }
            }else{               
                    return "User does not have MTech access";                
            }

            return retVal;
        }

        public string UserAuthentication(string username, string password, string appName, string app_AppVersion, string app_WSVersion)
        {
            string retVal = string.Empty;
            SecurityDetails userObj = GetUserSecPrimaryid(username);

            if (userObj.SecPrimaryId != 0){
                if (userObj.RoleMTech != null){
                    if (userObj.RoleMTech == 1)
                    {
                        //Check Authentication Setting is on
                        //Check if for the Secprimaryid, a windows user is configured
                        ActiveDirectorySettings rsi = GetSystemSettings();
                        if (rsi.ActiveDirectoryEnabled == true && userObj.AuthenticateType == 0){
                            if (username.ToLower().Trim() == userObj.WindowsUser.ToLower().Trim()){
                                //Perform AD Authentication
                                if (CheckDomainUserForUserValidate(rsi, username, password)){
                                    retVal = BasicAuthenticaton(userObj.UserName, string.Empty, appName, app_AppVersion, app_WSVersion, userObj.AuthenticateType);
                                }else{
                                    return "Invalid username/password";
                                }
                            }else{
                                return "This User is mapped to a Windows User. Please use Windows User account name/password.";
                            }
                        }else{
                            //Perform Normal Authentication
                            retVal = BasicAuthenticaton(userObj.UserName, password, appName, app_AppVersion, app_WSVersion, userObj.AuthenticateType);
                        }
                    }else{                        
                        return "User does not have MTech access";
                    }
                }else{
                    //Invalid MTech User
                    return "User does not have MTech access";
                }
            }else{
                return "Invalid username/password";
            }
            return retVal;
        }



        public ActiveDirectorySettings GetSystenSettings()
        {
            ActiveDirectorySettings globalSettings = new ActiveDirectorySettings();

//            string sqlQry = @" SELECT  obj.query('.')  AS  result
//                               FROM  SystemSetup cross  apply  settings.nodes('/Global/ActiveDirectory')  ActiveDirectory(obj)  
//                               WHERE  Key_Type='D'";

//            using (SqlDataReader drWcSettings = SqlHelper.ExecuteReader(SqlHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlQry, null)){
//                if (drWcSettings.Read()){
//                    globalSettings = (ActiveDirectorySettings)Deserializer(drWcSettings.GetString(0), typeof(ActiveDirectorySettings));
//                }
//                drWcSettings.Close();
//            }

            StringBuilder sqlStatment = new StringBuilder(@"DECLARE @XML XML
                                 SET @XML = (SELECT settings FROM DBO.SystemSetup WHERE key_type='d') ");

            sqlStatment.Append("SELECT  @XML.value('(/Global/ActiveDirectory/PageSize)[1]', 'varchar(200)')  AS PageSize");
            globalSettings.PageSize = Convert.ToInt32(SqlHelper.ExecuteScalar(SqlHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlStatment.ToString(), null));
            sqlStatment.Replace("SELECT  @XML.value('(/Global/ActiveDirectory/PageSize)[1]', 'varchar(200)')  AS PageSize", string.Empty);

            sqlStatment.Append("SELECT  @XML.value('(/Global/ActiveDirectory/DomainUserPassword)[1]', 'varchar(200)')  AS DomainUserPassword");
            globalSettings.DomainUserPassword = Convert.ToString(SqlHelper.ExecuteScalar(SqlHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlStatment.ToString(), null));
            sqlStatment.Replace("SELECT  @XML.value('(/Global/ActiveDirectory/DomainUserPassword)[1]', 'varchar(200)')  AS DomainUserPassword", string.Empty);

            sqlStatment.Append("SELECT  @XML.value('(/Global/ActiveDirectory/DomainUserName)[1]', 'varchar(200)')  AS DomainUserName");
            globalSettings.DomainUserName = Convert.ToString(SqlHelper.ExecuteScalar(SqlHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlStatment.ToString(), null));
            sqlStatment.Replace("SELECT  @XML.value('(/Global/ActiveDirectory/DomainUserName)[1]', 'varchar(200)')  AS DomainUserName", string.Empty);

            sqlStatment.Append("SELECT  @XML.value('(/Global/ActiveDirectory/Domain)[1]', 'varchar(200)')  AS Domain");
            globalSettings.Domain = Convert.ToString(SqlHelper.ExecuteScalar(SqlHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlStatment.ToString(), null));
            sqlStatment.Replace("SELECT  @XML.value('(/Global/ActiveDirectory/Domain)[1]', 'varchar(200)')  AS Domain", string.Empty);

            sqlStatment.Append("SELECT  @XML.value('(/Global/ActiveDirectory/OrganizationUnit)[1]', 'varchar(200)')  AS OrganizationUnit");
            globalSettings.OrganizationUnit = Convert.ToString(SqlHelper.ExecuteScalar(SqlHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlStatment.ToString(), null));
            sqlStatment.Replace("SELECT  @XML.value('(/Global/ActiveDirectory/OrganizationUnit)[1]', 'varchar(200)')  AS OrganizationUnit", string.Empty);

            sqlStatment.Append("SELECT  @XML.value('(/Global/ActiveDirectory/LDAPServer)[1]', 'varchar(200)')  AS LDAPServer");
            globalSettings.LDAPServer = Convert.ToString(SqlHelper.ExecuteScalar(SqlHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlStatment.ToString(), null));
            sqlStatment.Replace("SELECT  @XML.value('(/Global/ActiveDirectory/LDAPServer)[1]', 'varchar(200)')  AS LDAPServer", string.Empty);

            sqlStatment.Append("SELECT  @XML.value('(/Global/ActiveDirectory/SRSCMUserGroup)[1]', 'varchar(200)')  AS SRSCMUserGroup");
            globalSettings.SRSCMUserGroup = Convert.ToString(SqlHelper.ExecuteScalar(SqlHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlStatment.ToString(), null));
            sqlStatment.Replace("SELECT  @XML.value('(/Global/ActiveDirectory/SRSCMUserGroup)[1]', 'varchar(200)')  AS SRSCMUserGroup", string.Empty);

            sqlStatment.Append("SELECT  @XML.value('(/Global/ActiveDirectory/AESMLMUserGroup)[1]', 'varchar(200)')  AS AESMLMUserGroup");
            globalSettings.AESMLMUserGroup = Convert.ToString(SqlHelper.ExecuteScalar(SqlHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlStatment.ToString(), null));
            sqlStatment.Replace("SELECT  @XML.value('(/Global/ActiveDirectory/AESMLMUserGroup)[1]', 'varchar(200)')  AS AESMLMUserGroup", string.Empty);

            sqlStatment.Append("SELECT  @XML.value('(/Global/ActiveDirectory/ActiveDirectoryLoginBypass)[1]', 'varchar(200)')  AS ActiveDirectoryLoginBypass");
            globalSettings.ActiveDirectoryLoginBypass = Convert.ToBoolean(SqlHelper.ExecuteScalar(SqlHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlStatment.ToString(), null));
            sqlStatment.Replace("SELECT  @XML.value('(/Global/ActiveDirectory/ActiveDirectoryLoginBypass)[1]', 'varchar(200)')  AS ActiveDirectoryLoginBypass", string.Empty);

            sqlStatment.Append("SELECT  @XML.value('(/Global/ActiveDirectory/ActiveDirectoryEnabled)[1]', 'varchar(200)')  AS ActiveDirectoryEnabled");
            globalSettings.ActiveDirectoryEnabled = Convert.ToBoolean(SqlHelper.ExecuteScalar(SqlHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlStatment.ToString(), null));
            sqlStatment.Replace("SELECT  @XML.value('(/Global/ActiveDirectory/ActiveDirectoryEnabled)[1]', 'varchar(200)')  AS ActiveDirectoryEnabled", string.Empty);

            return globalSettings;
        }

        public static object Deserializer(string Xml, Type t)
        {
            object objSearalize = null;
            try
            {

                XmlSerializer serializer = new XmlSerializer(t);

                // Create an XmlSerializerNamespaces object.
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

                //Create the XmlNamespaceManager.
                NameTable nameTable = new NameTable();
                XmlNamespaceManager nameSpaceMGR = new XmlNamespaceManager(nameTable);
                nameSpaceMGR.AddNamespace("SCS.AE.SystemSettings", "http://www.SCS.AE.SystemSettings.com");

                //Create the XmlParserContext.
                XmlParserContext xmlContext = new XmlParserContext(null, nameSpaceMGR, null, XmlSpace.None);

                XmlReader reader = new XmlTextReader(Xml, XmlNodeType.Element, xmlContext);

                // Deserialize using the XmlTextWriter.
                objSearalize = (object)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return objSearalize;
        }

        public bool CheckDomainUserForUserValidate(ActiveDirectorySettings rsi, string UserName, string password)
        {
            bool result = false;
            try
            {

                string memberOf = "";
                List<string> lstRoots = new List<string>();

                using (DirectoryEntry deRoot = new DirectoryEntry("LDAP://" + rsi.LDAPServer, rsi.DomainUserName, base64Decode(rsi.DomainUserPassword)))
                {
                    using (DirectorySearcher deSrch = new DirectorySearcher(deRoot))
                    {
                        deSrch.PageSize = rsi.PageSize; ;
                        deSrch.SearchScope = SearchScope.Subtree;

                        // if OU is specified, change the root of directory
                        if (rsi.OrganizationUnit != "")
                        {
                            deSrch.Filter = "(&(objectClass=organizationalUnit)(ou=" + rsi.OrganizationUnit + "))";
                            SearchResultCollection ouResults = deSrch.FindAll();
                            if (ouResults.Count != 0)
                            {
                                foreach (SearchResult sr in ouResults)
                                {
                                    lstRoots.Add(sr.Path);
                                }
                            }
                            else
                            {
                                // no matching OU is found. stop searching
                                return result;
                            }
                        }

                        //if Group is specified, search using memberOf filter
                        //if (rsi.SRSCMUserGroup != "")
                        //{
                        //    deSrch.Filter = "(&(objectCategory=Group)(cn=" + rsi.SRSCMUserGroup + "))";
                        //    SearchResultCollection dnResults = deSrch.FindAll();
                        //    if (dnResults.Count != 0)
                        //    {
                        //        memberOf = dnResults[0].Properties["DistinguishedName"][0].ToString();
                        //    }
                        //    else
                        //    {
                        //        // no matching group is found. stop searching
                        //        return result;
                        //    }
                        //}
                    }
                    if (lstRoots.Count == 0)
                    {
                        lstRoots.Add(deRoot.Path);
                    }
                }

                foreach (string srRoot in lstRoots)
                {
                    using (DirectoryEntry deUserRoot = new DirectoryEntry(srRoot, rsi.DomainUserName, base64Decode(rsi.DomainUserPassword)))
                    {
                        using (DirectorySearcher userSearcher = new DirectorySearcher(deUserRoot))
                        {
                            userSearcher.PageSize = rsi.PageSize;
                            userSearcher.SearchScope = SearchScope.Subtree;
                            userSearcher.CacheResults = false;
                            userSearcher.PropertiesToLoad.Clear();
                            userSearcher.PropertiesToLoad.Add("SAMAccountName");

                            //if (memberOf != "")
                            //{
                            //    userSearcher.Filter = "(&(objectCategory=Person)(objectClass=User)(memberOf=" + memberOf + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                            //}
                            //else
                            //{
                            //    userSearcher.Filter = "(&(objectCategory=Person)(objectClass=User)(SAMAccountName=" + UserName + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                            //}

                            SearchResultCollection srcUsers = userSearcher.FindAll();
                            if (srcUsers.Count != 0) result = true;
                        }
                    }
                }
            }
            catch (DirectoryServicesCOMException DSCex)
            {
                return false;
            }
            catch (Exception ex)
            {
                // some other failure
                return false;
            }
            if (password == string.Empty)
                return true;
            else if (result)
                return AuthenticateUser(rsi, UserName, password);

            return result;

            //Get the Gloabls Settings from AE

        }

        public static string base64Decode(string sData)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(sData);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string result = new String(decoded_char);
            return result;
        }

        public bool AuthenticateUser(ActiveDirectorySettings rsi, string UserName, string Password)
        {

            //“ ISS - 2845 AD Integration - not concatenating domain name with user name in login page
            bool authenticated = false;

            string strDomainName = string.Empty;
            if (rsi.Domain == string.Empty)
            {
                if (rsi.DomainUserName.IndexOf("\\") != -1)
                {

                    string[] splitText = rsi.DomainUserName.Split('\\');
                    if (splitText.Length > 0)
                        strDomainName = splitText[0].ToString();
                    UserName = strDomainName + "\\" + UserName;
                }
                else
                {
                    if (rsi.DomainUserName.IndexOf(".") != -1)
                    {

                        string[] splitText = rsi.DomainUserName.Split('@');
                        if (splitText.Length > 0)
                            strDomainName = splitText[1].ToString();

                        UserName = UserName + "@" + strDomainName;
                    }
                }



            }
            else
            {
                UserName = rsi.Domain + "\\" + UserName;
            }

            DirectoryEntry entry = new DirectoryEntry("LDAP://" + rsi.LDAPServer, UserName, Password);
            try
            {


                object nativeObject = entry.NativeObject;
                authenticated = true;
            }
            catch (Exception ex)
            {
                string k = ex.Message;
                return false;
            }
            finally
            {
                entry.Close();
                entry = null;

            }
            return authenticated;
        }


        public bool CheckPermission(string username,string appName)
        {
            int retVal = 0;
            string sqlStatement = string.Empty;
            if ((appName).ToLower() == ("mTech.iOS").ToLower())
            {
                sqlStatement = @" SELECT [RoleMobileTech] FROM [dbo].[Security] WHERE lower([User]) = lower(@username) ";
            }
            else
            {
                sqlStatement = @" SELECT [RoleMobileTech] FROM [dbo].[Security] WHERE lower([User]) = lower(@username) ";
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString)){
                connection.Open();
                using (SqlCommand sqlQuery = connection.CreateCommand()){
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@username", DbType = DbType.String, Value = username });
                    sqlQuery.CommandText = sqlStatement;
                    SqlDataReader rReader = null;
                    try{
                        rReader = sqlQuery.ExecuteReader();

                        while (rReader.Read()){
                            if ((appName).ToLower() == ("mTech.iOS").ToLower())
                            {
                                retVal = Convert.ToInt16(rReader.GetValue(rReader.GetOrdinal("RoleMobileTech")) == DBNull.Value ? 0 : rReader.GetValue(rReader.GetOrdinal("RoleMobileTech")));
                            }
                            else
                            {
                                retVal = Convert.ToInt16(rReader.GetValue(rReader.GetOrdinal("RoleMobileTech")) == DBNull.Value ? 0 : rReader.GetValue(rReader.GetOrdinal("RoleMobileTech")));
                            }
                        }
                    }finally{
                        if (rReader != null){
                            rReader.Close();
                            rReader.Dispose();
                        }
                    }
                }
                connection.Close();
            }

            return retVal == 1 ? true : false;
        }

        public bool CheckSyncParams(string username)
        {
            long retVal = 0;
           
            const string sqlStatement = @" select stnprimaryid from mobilesynchronizationparameters 
            where secprimaryid =(SELECT secprimaryid FROM [dbo].[Security] WHERE lower([User]) = lower(@username)) and TYPE = 1";

            using (SqlConnection connection = new SqlConnection(ConnectionString)){
                connection.Open();
                using (SqlCommand sqlQuery = connection.CreateCommand()){
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@username", DbType = DbType.String, Value = username });
                    sqlQuery.CommandText = sqlStatement;
                    SqlDataReader rReader = null;
                    try
                    {
                        rReader = sqlQuery.ExecuteReader();

                        while (rReader.Read()){
                            retVal = Convert.ToInt64(rReader.GetValue(rReader.GetOrdinal("stnprimaryid")) == DBNull.Value ? 0 : rReader.GetValue(rReader.GetOrdinal("stnprimaryid")));
                        }
                    }finally{
                        if (rReader != null){
                            rReader.Close();
                            rReader.Dispose();
                        }
                    }
                }
                connection.Close();
            }            
            return retVal == 0 ? false : true;
        }

        private static long GetUserId(SqlConnection connection, string username) {
            long userId;

            // get the id of the user 
            using (SqlCommand command = connection.CreateCommand()) {
                command.CommandText = "SELECT [SecPrimaryId] FROM [dbo].[Security] WHERE lower([User]) = lower(@username)";
                command.Parameters.Add(new SqlParameter { ParameterName = "@username", DbType = DbType.String, Value = username });

                userId = Convert.ToInt64(command.ExecuteScalar());
            }

            if (userId == 0) {
                throw new Exception(string.Format("Unknown user {0}", username));
            }

            return userId;
        }

        //ToDo/M.D.Prasad: This method has to delete, Testing purpose we have created.
        public IEnumerable<User> GetUsers()
        {
            List<User> users = new List<User>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [SecPrimaryId], [User], [LastUpdateTime] FROM [dbo].[Security]";
                    /*
                    if (lastUpdateTime != DateTime.MinValue)
                    {
                        command.CommandText += " WHERE [LastUpdateTime] >= @last_update_time";
                        command.Parameters.Add(new SqlParameter { ParameterName = "@last_update_time", DbType = DbType.DateTime, Value = lastUpdateTime });
                    }
                    */
                    DataTable data = new DataTable();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                    dataAdapter.Fill(data);

                    users.AddRange(from DataRow row in data.Rows
                                   select new User
                                   {
                                       ID = Convert.ToInt64(row["SecPrimaryId"]),
                                       Username = Convert.ToString(row["User"]),
                                       LastUpdateDateTime = Convert.ToDateTime(data.Rows[0]["LastUpdateTime"]),
                                   });
                }
            }

            return users;
        }

    }
}
