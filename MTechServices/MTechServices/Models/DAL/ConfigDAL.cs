using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;
using log4net;
using MTechServices.Models.Entity;

namespace MTechServices.Models.DAL
{
    public class ConfigDAL
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ConfigDAL));

        private string connectionString;
        protected string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString;
                }

                return connectionString;
            }
        }

        Utilities utility = new Utilities();

        #region Download Section

        #region Filter

        public IEnumerable<FilterInfo> GetFilters(long SECPrimaryID)
        {
            int start = Environment.TickCount;

            List<FilterInfo> filters = new List<FilterInfo>();

            try
            {
                DataTable data = GetFilterData(SECPrimaryID);

                filters.AddRange(from DataRow row in data.Rows
                                 select new FilterInfo
                                    {
                                        MTFPrimaryId = Convert.ToInt64(row["MTFPrimaryId"]),
                                        FilterName = Convert.ToString(row["FilterName"]),
                                        Active = Convert.ToByte(row["Active"]),
                                        FACPrimaryIds = Convert.ToString(row["FACPrimaryIds"]),
                                        ACCPrimaryIds = Convert.ToString(row["ACCPrimaryIds"]),
                                        WKRPrimaryIds = Convert.ToString(row["WKRPrimaryIds"])
                                    });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetFilters => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetFilters({0}) Execute Time: {1} ms", SECPrimaryID.ToString(), Environment.TickCount - start);

            return filters;
        }

        private DataTable GetFilterData(long SECPrimaryID)
        {
            DataTable data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;

                        command.CommandText += @"Select MTFPrimaryId,FilterName, Active, FACPrimaryIds, ACCPrimaryIds,WKRPrimaryIds  
                                                 from [dbo].[MobileTechFilters] where SECPrimaryId = @SECPrimaryId ";

                        command.Parameters.Add(new SqlParameter { ParameterName = "@SECPrimaryId", DbType = DbType.Int64, Value = SECPrimaryID });
                        //Writelog(command.CommandText);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            data.Load(dr);
                            dr.Close();
                        }
                    }
                    connection.Close();
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetFilterData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        #endregion

        #region Facility

        public IEnumerable<string> GetDeletedFacilities(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> facilities = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT [FACPrimaryId] FROM [dbo].[AccountFacility]  WHERE  [LastUpdateTime] >= @lastUpdateTime";

                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            facilities.AddRange(from DataRow row in data.Rows
                                             select Convert.ToString(row["FACPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedFacilities => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedFacilities({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return facilities;
        }

        private DataTable GetFacilitiesData(DateTime lastUpdateTime, string sessionId)
        {
            DataTable data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;

                        command.CommandText += @"SELECT FACPrimaryId, Facility,getdate() as LastSyncDate from AccountFacility A
                          WHERE EXISTS
                          (
                                SELECT FCTPrimaryId FROM AccountFacilityCenter B WHERE FACPrimaryId = A.FACPrimaryId AND Archived = ''
                                AND EXISTS
                                (
                                    Select WKCPrimaryId from WorkerCenter WHERE WCCPrimaryId = B.WCCPrimaryId
                                    AND (WorkerCenter.[Status] = 'Worker' OR WorkerCenter.[Status] = 'Virtual' or WorkerCenter.[Status] = 'Vendor')
                                    AND WKRPrimaryId in (Select wkrprimaryid from security where secprimaryid in (select userid from UserSessions where SessionId = @SessionId))
                                )
                          ) ";

                        command.Parameters.Add(new SqlParameter { ParameterName = "@SessionId", DbType = DbType.String, Value = sessionId });

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            command.CommandText += @" AND lastupdatetime >= @lastUpdateTime ";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }

                        //Writelog(command.CommandText);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            data.Load(dr);
                            dr.Close();
                        }
                    }
                    connection.Close();
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetFacilitiesData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<Facility> GetFacilities(DateTime lastUpdateTime, string sessionId)
        {
            int start = Environment.TickCount;

            List<Facility> facilities = new List<Facility>();

            try
            {
                DataTable data = GetFacilitiesData(lastUpdateTime, sessionId);

                facilities.AddRange(from DataRow row in data.Rows
                                 select new Facility
                                 {
                                     FacPrimaryId = Convert.ToInt64(row["FACPrimaryId"]),
                                     Name = Convert.ToString(row["Facility"]),
                                     LastSyncDate = Convert.ToDateTime(row["LastSyncDate"])
                                 });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetFacilities => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetFacilities({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return facilities;
        }

        #endregion

        #region Workers

        public IEnumerable<Worker> GetWorkers(DateTime lastUpdateTime, string sessionId)
        {
            int start = Environment.TickCount;

            List<Worker> workers = new List<Worker>();

            try
            {
                DataTable data = GetWorkersData(lastUpdateTime, sessionId);

                workers.AddRange(from DataRow row in data.Rows
                                 select new Worker
                                  {
                                      WKRPrimaryId = Convert.ToInt64(row["WKRPrimaryid"]),
                                      Name = Convert.ToString(row["Name"]),
                                      LastSyncDate = Convert.ToDateTime(row["LastSyncDate"])
                                  });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetWorkers => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetWorkers({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return workers;
        }

        private DataTable GetWorkersData(DateTime lastUpdateTime, string sessionId)
        {
            DataTable data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;

                        command.CommandText += @"Select Distinct A.WKRPrimaryid,A.Name,getdate() as LastSyncDate  from Workers A
                                INNER JOIN WorkerCenter B ON A.WKRPrimaryId = B.WKRPrimaryId
                                WHERE (B.[Status] = 'Worker' OR B.[Status] = 'Virtual' OR B.[Status] = 'Vendor')   
                                AND EXISTS
                                      (
                                            Select C.WKCPrimaryId from WorkerCenter C WHERE  C.WCCPrimaryId = B.WCCPrimaryId
                                            AND (C.[Status] = 'Worker' OR C.[Status] = 'Virtual' OR C.[Status] = 'Vendor')     
                                            AND C.WKRPrimaryId in (Select wkrprimaryid from security where secprimaryid in (select userid from UserSessions 
                                            where SessionId = @SessionId))
                                      )";
                        
                        command.Parameters.Add(new SqlParameter { ParameterName = "@SessionId", DbType = DbType.String, Value = sessionId });

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            command.CommandText += @" AND A.lastupdatetime >= @lastUpdateTime ";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }

                        //Writelog(command.CommandText);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            data.Load(dr);
                            dr.Close();
                        }
                    }
                    connection.Close();
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetWorkersData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> GetDeletedWorkers(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> workers = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT [WKRPrimaryId] FROM [dbo].[Workers]  WHERE  [LastUpdateTime] >= @lastUpdateTime";

                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            workers.AddRange(from DataRow row in data.Rows
                                               select Convert.ToString(row["WKRPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedWorkers => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedWorkers({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return workers;
        }

        #endregion

        #region Codes_Urgency
        public IEnumerable<UrgencyInfo> GetUrgencyCodes(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<UrgencyInfo> urgencyCodes = new List<UrgencyInfo>();
            try
            {
                DataTable data = GetUrgencyData(lastUpdateTime);

                urgencyCodes.AddRange(from DataRow row in data.Rows
                                      select new UrgencyInfo
                                      {
                                          PrimaryId = Convert.ToInt64(row["URGPrimaryId"]),
                                          Code = Convert.ToInt16(row["Code"] == DBNull.Value ? 0 : row["Code"]),
                                          Description = Convert.ToString(row["Description"] == DBNull.Value ? "" : row["Description"]),
                                      });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetUrgencyCodes => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetUrgencyCodes({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);
            return urgencyCodes;
        }

        private DataTable GetUrgencyData(DateTime lastUpdateTime)
        {
            DataTable data = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;


                        command.CommandText += @"SELECT URGPrimaryId, Code,[Description] FROM Codes_Urgency
                                                 WHERE AppliesWO = 1 ANd Archived = '' ";

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            command.CommandText += @" and lastupdatetime >= @lastUpdateTime ";
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }


                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        dataAdapter.Fill(data);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetUrgencyData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> GetDeletedUrgency(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> urgencies = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT [URGPrimaryId] FROM [dbo].[Codes_Urgency]  WHERE Archived = '' AND [LastUpdateTime] >= @lastUpdateTime";

                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            urgencies.AddRange(from DataRow row in data.Rows
                                               select Convert.ToString(row["URGPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedUrgency => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedUrgency({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return urgencies;
        }
        #endregion

        #region Codes_Requests
        public IEnumerable<SystemCodeInfo> GetRequests(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<SystemCodeInfo> requests = new List<SystemCodeInfo>();

            try
            {
                DataTable data = GetRequestsData(lastUpdateTime);

                requests.AddRange(from DataRow row in data.Rows
                                  select new SystemCodeInfo
                                  {
                                      PrimaryId = Convert.ToInt64(row["ReqPrimaryId"]),
                                      Code = Convert.ToInt16(row["RequestCode"]),
                                      Description = Convert.ToString(row["Description"])
                                  });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetRequests => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetRequests({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return requests;
        }

        private DataTable GetRequestsData(DateTime lastUpdateTime)
        {
            DataTable data = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;

                        command.CommandText += @"SELECT REQPrimaryId, RequestCode, [Description] FROM Codes_Request";

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            command.CommandText += @" where lastupdatetime >= @lastUpdateTime ";
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }

                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        dataAdapter.Fill(data);
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetRequestsData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> GetDeletedRequestCodes(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> requests = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT [REQPrimaryId] FROM [dbo].[Codes_Request]  WHERE [LastUpdateTime] >= @lastUpdateTime";

                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            requests.AddRange(from DataRow row in data.Rows
                                              select Convert.ToString(row["REQPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedRequests => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedRequests({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return requests;
        }
        #endregion

        #region Codes_OpenWorkStatus
        public IEnumerable<OpenWorkStatusInfo> GetOpenWorkStatuses(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<OpenWorkStatusInfo> openWorkStatuses = new List<OpenWorkStatusInfo>();
            try
            {
                DataTable data = GetOpenWorkStatusesData(lastUpdateTime);

                openWorkStatuses.AddRange(from DataRow row in data.Rows
                                          select new OpenWorkStatusInfo
                                          {
                                              PrimaryId = Convert.ToInt64(row["OwsPrimaryId"]),
                                              Code = Convert.ToInt16(row["OpenStatusCode"]),
                                              Description = Convert.ToString(row["Description"]),
                                              RequestsActionCode = Convert.ToInt16(row["RequestActionCode"] == DBNull.Value ? Convert.ToInt16(0) : row["RequestActionCode"])
                                          });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetOpenWorkStatuses => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetOpenWorkStatuses({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return openWorkStatuses;
        }

        private DataTable GetOpenWorkStatusesData(DateTime lastUpdateTime)
        {
            DataTable data = new DataTable();
            //lastUpdateTime = getTimeZoneSettings(lastUpdateTime);
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;


                        command.CommandText += @"SELECT Codes_OpenWorkStatus.OWSPrimaryId, OpenStatusCode, Codes_OpenWorkStatus.Description
                        ,codes_requestsaction.Code as RequestActionCode
                        FROM Codes_OpenWorkStatus 
                        left outer join codes_requestsaction on codes_requestsaction.CACPrimaryid  = Codes_OpenWorkStatus.CACPrimaryid 
                        WHERE Codes_OpenWorkStatus.Archived = '' ";

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += @" and Codes_OpenWorkStatus.LastUpdateTime >= @lastUpdateTime ";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }

                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        dataAdapter.Fill(data);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetOpenWorkStatusesData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> GetDeletedOpenWorkStatuses(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> openWorkStatuses = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT [OWSPrimaryId] FROM [dbo].[Codes_OpenWorkStatus]  WHERE Archived = '' AND [LastUpdateTime] >= @lastUpdateTime";

                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            openWorkStatuses.AddRange(from DataRow row in data.Rows
                                                      select Convert.ToString(row["OWSPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedOpenWorkStatuses => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedOpenWorkStatuses({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return openWorkStatuses;
        }
        #endregion

        #region Control Center
        public IEnumerable<SystemCode> GetControlCenters(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<SystemCode> assetCenters = new List<SystemCode>();
            try
            {
                DataTable data = GetControlCentersData(lastUpdateTime);

                assetCenters.AddRange(from DataRow row in data.Rows
                                      select new SystemCode
                                      {
                                          PrimaryId = Convert.ToInt64(row["CCCPrimaryId"]),
                                          Code = Convert.ToInt16(row["ControlCenterCode"]),
                                          Description = Convert.ToString(row["Description"]),
                                      });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetControlCenters => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetControlCenters({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return assetCenters;
        }

        private DataTable GetControlCentersData(DateTime lastUpdateTime)
        {
            DataTable data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;


                        command.CommandText += @"SELECT CCCPrimaryId, ControlCenterCode, [Description] FROM Codes_ControlCenter WHERE Archived = '' ";


                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += @" AND [LastUpdateTime] >= @lastUpdateTime ";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }


                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        dataAdapter.Fill(data);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetControlCentersData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> GetDeletedControlCenters(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> controlCenters = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT [CCCPrimaryId] FROM [dbo].[Codes_ControlCenter] WHERE Archived = '' AND [LastUpdateTime] >= @lastUpdateTime";

                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            controlCenters.AddRange(from DataRow row in data.Rows
                                                    select Convert.ToString(row["CCCPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedControlCenters => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedControlCenters({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return controlCenters;
        }
        #endregion

        #region Account
        public IEnumerable<AccountSync> GetAccounts(DateTime lastUpdateTime, long workerID)
        {
            int start = Environment.TickCount;
            List<AccountSync> accounts = new List<AccountSync>();
            try
            {
                DataTable data = GetAccountData(lastUpdateTime, workerID);
                accounts.AddRange(from DataRow row in data.Rows
                                  select new AccountSync
                                  {
                                      AccPrimaryId = Convert.ToInt64(row["ACCPrimaryId"]),
                                      AccountId = Convert.ToString(row["AccountId"]),
                                      DeptName = Convert.ToString(row["DeptName"]),
                                      FacPrimaryId = Convert.ToInt64(row["FacPrimaryId"])
                                      
                                  });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetAccounts => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetAccounts({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return accounts;
        }

        private DataTable GetAccountData(DateTime lastUpdateTime, long workerID)
        {
            DataTable data = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;

                        command.CommandText += @"SELECT C.ACCPrimaryId, C.AccountId, C.FACPrimaryId, C.DeptName,C.AcctType From Account C
                        WHERE EXISTS
                        (
                             SELECT FACPrimaryId  from AccountFacility A
                              WHERE EXISTS
                              (
                                SELECT FCTPrimaryId FROM AccountFacilityCenter B WHERE FACPrimaryId = A.FACPrimaryId AND Archived = ''
                                AND EXISTS
                                (
                                    Select WKCPrimaryId from WorkerCenter WHERE WCCPrimaryId = B.WCCPrimaryId
                                    AND (WorkerCenter.[Status] = 'Worker' OR WorkerCenter.[Status] = 'Virtual' or WorkerCenter.[Status] = 'Vendor')
                                    AND WKRPrimaryId = @WKRPrimaryId
                                )
                            )   
                        )
                        AND EXISTS
                        (
                            SELECT ACTPrimaryId FROM AccountCenter B WHERE ACCPrimaryId = C.ACCPrimaryId AND Archived = ''
                            AND EXISTS
                            (
                                Select WKCPrimaryId from WorkerCenter WHERE WCCPrimaryId = B.WCCPrimaryId
                                AND (WorkerCenter.[Status] = 'Worker' OR WorkerCenter.[Status] = 'Virtual' or WorkerCenter.[Status] = 'Vendor')
                                AND WKRPrimaryId = @WKRPrimaryId 
                            )
                        )";

                        

                        /*   WHERE EXISTS
	                            (
		                            SELECT wkOrders.EquipId FROM wkOrders 
		                            INNER JOIN workerCenter ON wkOrders.WKRPrimaryId = WorkerCenter.WKRPrimaryId 
		                            WHERE wkOrders.WKRPrimaryId = @WKRPrimaryId AND wkOrders.WCCPrimaryId = WorkerCenter.WCCPrimaryId
		                            AND (WorkerCenter.[Status] = 'Worker' OR WorkerCenter.[Status] = 'Virtual' or WorkerCenter.[Status] = 'Vendor')
		                            AND OpenFlag = 1 AND wkOrders.ACCPrimaryId = B.ACCPrimaryId ";*/


                        command.Parameters.Add(new SqlParameter { ParameterName = "@WKRPrimaryId", DbType = DbType.Int64, Value = workerID });

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += @" and LastUpdateTime >= @lastUpdateTime ";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }

                        //command.CommandText += " ) ";
                        //Writelog(command.CommandText);
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        dataAdapter.Fill(data);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetAccountData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> GetDeletedAccounts(DateTime lastUpdateTime, long workerID)
        {
            int start = Environment.TickCount;

            List<string> accounts = new List<string>();
            try
            {
                //DateTime dt = (DateTime)SqlDateTime.MinValue;
                //if (lastUpdateTime > dt)
                //{
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            //command.CommandText = @"SELECT [ACCPrimaryId] FROM [dbo].[Account] WHERE [LastUpdateTime] >= @lastUpdateTime";

                            //lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            //command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            command.CommandText += @"SELECT B.ACCPrimaryId 
                            FROM Account B
                            WHERE EXISTS
	                            (
		                            SELECT wkOrders.EquipId FROM wkOrders 
		                            INNER JOIN workerCenter ON wkOrders.WKRPrimaryId = WorkerCenter.WKRPrimaryId 
		                            WHERE wkOrders.WKRPrimaryId = @WKRPrimaryId AND wkOrders.WCCPrimaryId = WorkerCenter.WCCPrimaryId
		                            AND (WorkerCenter.[Status] = 'Worker' OR WorkerCenter.[Status] = 'Virtual' or WorkerCenter.[Status] = 'Vendor')
		                            AND OpenFlag = 1 AND wkOrders.ACCPrimaryId = B.ACCPrimaryId ";


                            command.Parameters.Add(new SqlParameter { ParameterName = "@WKRPrimaryId", DbType = DbType.Int64, Value = workerID });

                            DateTime dt = (DateTime)SqlDateTime.MinValue;
                            if (lastUpdateTime != dt)
                            {
                                lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                                command.CommandText += @" AND wkOrders.LastUpdateTime >= @lastUpdateTime ";
                                command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                            }

                            command.CommandText += " ) ";


                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            accounts.AddRange(from DataRow row in data.Rows
                                              select Convert.ToString(row["ACCPrimaryId"]));
                        }
                    }
                //}
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedAccounts => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedAccounts({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return accounts;
        }
        #endregion

        #region Work Orders

        private FilterInfo GetFilter(string sessionId)
        {
            FilterInfo retObj = new FilterInfo();

            string sqlStatement = @"Select FACPrimaryIds,ACCPrimaryIds,WKRPrimaryIds from [dbo].[MobileTechFilters] 
                                   where SECPrimaryId in (Select UserId From UserSessions where SessionId = @SessionId) and active = 1 ";
            SqlParameter parameters = new SqlParameter { ParameterName = "@SessionId", DbType = DbType.String, Value = sessionId };


            using (SqlDataReader sqlReader = SqlHelper.ExecuteReader(SqlHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlStatement, parameters))
            {
                while (sqlReader.Read())
                {
                    retObj.FACPrimaryIds = sqlReader["FACPrimaryIds"] != DBNull.Value ? sqlReader.GetString(0) : string.Empty;
                    retObj.ACCPrimaryIds = sqlReader["ACCPrimaryIds"] != DBNull.Value ? sqlReader.GetString(1) : string.Empty;
                    retObj.WKRPrimaryIds = sqlReader["WKRPrimaryIds"] != DBNull.Value ? sqlReader.GetString(2) : string.Empty;
                }
                sqlReader.Close();
            }

            return retObj;
        }


        public IEnumerable<WorkOrderDetails> GetWorkOrders(DateTime lastUpdateTime, long workerID, string sessionId)
        {
            int start = Environment.TickCount;

            List<WorkOrderDetails> workOrders = new List<WorkOrderDetails>();
            try
            {
                SyncParams syncParams = new SyncParams();

                using (DataTable data = GetWorkOrdersData(lastUpdateTime, workerID, sessionId))
                {

                    workOrders.AddRange(from DataRow row in data.Rows
                                        select new WorkOrderDetails
                                        {
                                            WorPrimaryId = Convert.ToInt64(row["WORPrimaryId"]),
                                            Number = Convert.ToString(row["WorkOrder"]),
                                            WODescription = Convert.ToString(row["WODescription"] == DBNull.Value ? string.Empty : row["WODescription"]),
                                            ServiceCenter = new SystemCodeInfo { PrimaryId = Convert.ToInt64(row["WCCPrimaryId"]), Code = Convert.ToInt16(row["WorkCenterCode"]) },
                                            Worker = new WorkerInfo { WKRPrimaryId = Convert.ToInt64(row["WkrPrimaryId"] == DBNull.Value ? 0 : row["WkrPrimaryId"]) },
                                            Request = new RequestInfo { PrimaryId = Convert.ToInt64(row["REQPrimaryId"] == DBNull.Value ? 0 : row["REQPrimaryId"]) },
                                            Result = new SystemCodeInfo { PrimaryId = Convert.ToInt64(row["RESPrimaryId"] == DBNull.Value ? 0 : row["RESPrimaryId"]) },
                                            Fault = new SystemCodeInfo { PrimaryId = Convert.ToInt64(row["FLTPrimaryId"] == DBNull.Value ? 0 : row["FLTPrimaryId"]) },
                                            IssueDate = Convert.ToDateTime(row["DateIssue"] == DBNull.Value ? DateTime.MinValue : row["DateIssue"]),
                                            StartDate = Convert.ToDateTime(row["DateStarted"] == DBNull.Value ? DateTime.MinValue : row["DateStarted"]),
                                            CompleteDate = Convert.ToDateTime(row["DateComplete"] == DBNull.Value ? DateTime.MinValue : row["DateComplete"]),
                                            //ReportDate = Convert.ToDateTime(row["RPTDATE"] == DBNull.Value ? DateTime.MinValue : row["RPTDATE"]),
                                            //PmLevel = Convert.ToByte(row["PM_LEVEL"] == DBNull.Value ? 0 : row["PM_LEVEL"]),
                                            AssetDetails = new AssetDetails { EquipId = Convert.ToInt64(row["EquipId"] == DBNull.Value ? 0 : row["EquipId"]) },
                                            Account = new Account { AccPrimaryId = Convert.ToInt64(row["ACCPrimaryId"] == DBNull.Value ? 0 : row["ACCPrimaryId"]) },
                                            //TotalHours = Convert.ToDecimal(row["TotalHours"] == DBNull.Value ? 0 : row["TotalHours"]),
                                            //Rate = Convert.ToDecimal(row["RATE"] == DBNull.Value ? 0 : row["RATE"]),
                                            //LaborCost = Convert.ToDecimal(row["LABORCOST"] == DBNull.Value ? 0 : row["LABORCOST"]),
                                            //PartsCost = Convert.ToDecimal(row["PARTSCOST"] == DBNull.Value ? 0 : row["PARTSCOST"]),
                                            //MiscCosts = Convert.ToDecimal(row["MISCCOSTS"] == DBNull.Value ? 0 : row["MISCCOSTS"]),
                                            //Credits = Convert.ToDecimal(row["CREDITS"] == DBNull.Value ? 0 : row["CREDITS"]),
                                            //Tax = Convert.ToDecimal(row["TAX"] == DBNull.Value ? 0 : row["TAX"]),
                                            //TaxRate = Convert.ToDecimal(row["TAXRATE"] == DBNull.Value ? 0 : row["TAXRATE"]),
                                            SafetyTest = Convert.ToString(row["SFTest"] == DBNull.Value ? string.Empty : row["SFTest"]),
                                            OpenFlag = Convert.ToByte(row["OpenFlag"]),
                                            //PostingDate = Convert.ToDateTime(row["POSTINGDATE"] == DBNull.Value ? DateTime.MinValue : row["POSTINGDATE"]),
                                            IssueTime = Convert.ToDateTime(row["IssueTime"] == DBNull.Value ? DateTime.MinValue : row["IssueTime"]),
                                            StartTime = Convert.ToDateTime(row["StartTime"] == DBNull.Value ? DateTime.MinValue : row["StartTime"]),
                                            CompleteTime = Convert.ToDateTime(row["CompleteTime"] == DBNull.Value ? DateTime.MinValue : row["CompleteTime"]),
                                            //AlternateWorkOrderNumber = Convert.ToString(row["AlternateWoNumber"] == DBNull.Value ? string.Empty : row["AlternateWoNumber"]),
                                            DateDue = Convert.ToDateTime(row["DateDue"] == DBNull.Value ? DateTime.MinValue : row["DateDue"]),
                                            //DueTime = Convert.ToDateTime(row["TimeDue"] == DBNull.Value ? DateTime.MinValue : row["TimeDue"]),
                                            //Affiliation = Convert.ToString(row["AFFILIATION"] == DBNull.Value ? string.Empty : row["AFFILIATION"]),
                                            //RateCategory = new SystemCode { PrimaryId = Convert.ToInt64(row["RGCPrimaryId"] == DBNull.Value ? 0 : row["RGCPrimaryId"]) },
                                            //WorkerType = new WorkerTypeInfo { PrimaryId = Convert.ToInt64(row["WTCPrimaryId"] == DBNull.Value ? 0 : row["WTCPrimaryId"]) },
                                            //AssetStatus = new SystemCode { PrimaryId = Convert.ToInt64(row["ESCPrimaryId"] == DBNull.Value ? 0 : row["ESCPrimaryId"]) },
                                            //LaborBill = Convert.ToString(row["LABORBILL"] == DBNull.Value ? string.Empty : row["LABORBILL"]),
                                            //LaborTax = Convert.ToString(row["LABORTAX"] == DBNull.Value ? string.Empty : row["LABORTAX"]),
                                            //PartsBill = Convert.ToString(row["PARTSBILL"] == DBNull.Value ? string.Empty : row["PARTSBILL"]),
                                            //PartsTax = Convert.ToString(row["PARTSTAX"] == DBNull.Value ? string.Empty : row["PARTSTAX"]),
                                            //MiscBill = Convert.ToString(row["MISCBILL"] == DBNull.Value ? string.Empty : row["MISCBILL"]),
                                            //MiscTax = Convert.ToString(row["MISCTAX"] == DBNull.Value ? string.Empty : row["MISCTAX"]),
                                            OpenWorkOrderStatus = new SystemCode { PrimaryId = Convert.ToInt64(row["OWSPrimaryId"] == DBNull.Value ? 0 : row["OWSPrimaryId"]) },
                                            Urgency = new UrgencyInfo { PrimaryId = Convert.ToInt64(row["URGPrimaryId"] == DBNull.Value ? 0 : row["URGPrimaryId"]) },
                                            //DownTime = Convert.ToDecimal(row["Downtime"] == DBNull.Value ? 0 : row["Downtime"]),
                                            //GroupWorkOrder = new GroupWorkOrder { PrimaryId = Convert.ToInt64(row["GwoPrimaryId"] == DBNull.Value ? 0 : row["GwoPrimaryId"]) },
                                            //OriginalWorkOrder = new WorkOrder { WorPrimaryId = Convert.ToInt64(row["Orig_WORPrimaryId"] == DBNull.Value ? 0 : row["Orig_WORPrimaryId"]) },
                                            LastSyncDateTime = Convert.ToDateTime(row["LastSyncDateTime"] == DBNull.Value ? DateTime.MinValue : row["LastSyncDateTime"]),
                                            canWOBeClosed = Convert.ToByte(row["canWOBeClosed"]),
                                            LatestDispatchActionCode = Convert.ToInt16(row["LatestDispatchActionCode"] == DBNull.Value ? Convert.ToInt16(0) : row["LatestDispatchActionCode"])
                                        });
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetWorkOrders => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
            logger.InfoFormat("GetWorkOrders({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);
            return workOrders;
        }

        private DataTable GetWorkOrdersData(DateTime lastUpdateTime, long workerID,string sessionID)
        {
            DataTable data = new DataTable();
            try
            {
                FilterInfo filter = GetFilter(sessionID);
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;

                        command.CommandText += @"SELECT wkOrders.WORPrimaryId, WorkOrder, wkOrders.REQPrimaryId, RESPrimaryId, FLTPrimaryId, wkOrders.EquipId, wkOrders.ACCPrimaryId, 
                        DateIssue, DateStarted, IssueTime, StartTime, CompleteTime, DateDue, DateComplete, wkOrders.OWSPrimaryId, SFTest, wkOrders.URGPrimaryId, wkOrders.WCCPrimaryId, 
                        wkOrders.WKRPrimaryId, OpenFlag, WODescription, codes_workcenter.workcentercode, getdate() as LastSyncDateTime, 
                        dbo.fnGetWorkOrderCloseValidationsForMobileTech(wkOrders.WORPrimaryId) as canWOBeClosed, 
                        (Select Codes_RequestsAction.Code From Codes_RequestsAction WHERE CACPrimaryId = 
                        dbo.fnGetDispatchLatestOperativeActionForAssignedUserForMobileTech(RQSPrimaryID, (Select UserId From UserSessions where SessionId = @SessionId))) as LatestDispatchActionCode      
                        FROM wkOrders 
                        INNER JOIN workerCenter ON wkOrders.WKRPrimaryId = WorkerCenter.WKRPrimaryId
                        INNER JOIN codes_workcenter on wkOrders.WCCPrimaryId  = codes_workcenter.WCCPrimaryid
                        LEFT OUTER JOIN Codes_OpenWorkStatus ON wkOrders.OWSPrimaryId = Codes_OpenWorkStatus.OWSPrimaryId
                        LEFT OUTER JOIN Codes_RequestsAction ON Codes_OpenWorkStatus.CACPrimaryId = Codes_RequestsAction.CACPrimaryId
                        LEFT OUTER JOIN Requests ON wkOrders.WORPrimaryID = Requests.WORPrimaryId
                        WHERE wkOrders.WCCPrimaryId = WorkerCenter.WCCPrimaryId
                        AND (WorkerCenter.[Status] = 'Worker' OR WorkerCenter.[Status] = 'Virtual' or WorkerCenter.[Status] = 'Vendor')
                        AND OpenFlag = 1 AND ( Codes_OpenWorkStatus.CACPrimaryId IS NULL OR Codes_RequestsAction.Code IN (11, 12,31,33,34) ) ";

                        if (filter.WKRPrimaryIds != null)
                        {
                            if (filter.WKRPrimaryIds.Trim().Contains("Unassigned"))
                            {
                                string workers = utility.Remove(filter.WKRPrimaryIds.Trim(), "Unassigned");


                                if (workers.Trim() != string.Empty)
                                {
                                    command.CommandText += " AND ( wkOrders.WKRPrimaryId in ('" + workers.Replace(",", "','") + "' ) or wkOrders.WKRPrimaryId is null)";
                                }
                                else
                                {
                                    command.CommandText += " AND  wkOrders.WKRPrimaryId is null ";
                                }
                            }
                            else
                            {
                                command.CommandText += " AND wkOrders.WKRPrimaryId in ('" + filter.WKRPrimaryIds.Replace(",", "','") + "' ) ";
                            }

                            command.CommandText += " AND wkOrders.ACCPrimaryId in ('" + filter.ACCPrimaryIds.Replace(",", "','") + "' ) ";
                        }
                        else
                        {
                            command.CommandText += " AND  wkOrders.WKRPrimaryId = @WKRPrimaryId ";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@WKRPrimaryId", DbType = DbType.Int64, Value = workerID });
                        }

                        command.Parameters.Add(new SqlParameter { ParameterName = "@SessionId", DbType = DbType.String, Value = sessionID });
                        

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            //Nahid Ahmed Jan 19,2015 Since the last sync time is always fetched from server it no longer needs to be converted for time zone
                            //lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += @" AND wkOrders.lastupdatetime >= @lastUpdateTime ";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }

                        //Writelog(command.CommandText);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            data.Load(dr);
                            dr.Close();
                        }
                    }
                    connection.Close();
                }

                if (utility.checkTimeZoneSettings())
                {
                    utility.Writelog("Convert Server to client Time");
                    TimeZoneCalc.ConvertTableDateTimeForMobileTech(data);
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetWorkOrdersData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> GetDeletedWorkOrders(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> workOrders = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT [WORPrimaryId] FROM [dbo].[wkOrders] WHERE [LastUpdateTime] >= @lastUpdateTime";

                            //Nahid Ahmed Jan 20,2015 Since the last sync time is always fetched from server it no longer needs to be converted for time zone
                            //lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            workOrders.AddRange(from DataRow row in data.Rows
                                                select Convert.ToString(row["WORPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedWorkOrders => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedWorkOrders({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return workOrders;
        }
        #endregion

        #region Assets
        public IEnumerable<AssetDetails> GetAssets(DateTime lastUpdateTime, long workerID, string sessionId)
        {
            int start = Environment.TickCount;

            List<AssetDetails> equipments = new List<AssetDetails>();
            try
            {

                using (DataTable data = GetEquipmentData(lastUpdateTime, workerID, sessionId))
                {
                    equipments.AddRange(from DataRow row in data.Rows
                                        select new AssetDetails
                                        {
                                            LastSyncDateTime = Convert.ToDateTime(row["LastSyncDate"]),
                                            EquipId = Convert.ToInt64(row["EquipId"]),
                                            //AssetStatus = new AssetStatusInfo { PrimaryId = Convert.ToInt64(row["ESCPrimaryId"]) },
                                            //StatusDate = Convert.ToDateTime(row["StatusDate"]),
                                            //BDGPrimaryId = Convert.ToInt64(row["BDGPrimaryId"] == DBNull.Value ? 0 : row["BDGPrimaryId"]),
                                            //BadgeNumber = Convert.ToString(row["BadgeNumber"] == DBNull.Value ? "" : row["BadgeNumber"]),
                                            AssetCenter = new SystemCode { PrimaryId = Convert.ToInt64(row["CCCPrimaryId"]), },
                                            OwnerAccount = new AccountInfo { AccPrimaryId = Convert.ToInt64(row["Owner"]) },
                                            LocationAccount = new AccountInfo { AccPrimaryId = Convert.ToInt64(row["Location"]) },
                                            Control = Convert.ToString(row["Control"]),

                                            //Control2 = Convert.ToString(row["Control2"] == DBNull.Value ? "" : row["Control2"]),
                                            //SystemNumber = Convert.ToString(row["SystemNumber"] == DBNull.Value ? "" : row["SystemNumber"]),
                                            //PurchaseOrderNumber = Convert.ToString(row["PONumber"] == DBNull.Value ? "" : row["PONumber"]),
                                            //RateCategory = new SystemCode { PrimaryId = Convert.ToInt64(row["RGCPrimaryId"]), },

                                            Model = new ModelInfo { ModelName = Convert.ToString(row["MDL"] == DBNull.Value ? "" : row["MDL"]).Trim(), DeviceCategory = new DeviceCategoryInfo { DevCategory = Convert.ToString(row["DC"] == DBNull.Value ? "" : row["DC"]).Trim() } }


                                            //SerialNumber = Convert.ToString(row["Serial"] == DBNull.Value ? "" : row["Serial"]),
                                            //IncomingDate = Convert.ToDateTime(row["InDate"]),
                                            //PurchaseDate = (DateTime?)(row["PURCHDATE"] == DBNull.Value ? null : row["PURCHDATE"]),
                                            //Warranty = Convert.ToDecimal(row["WARRANTY"] == DBNull.Value ? 0 : row["WARRANTY"]),
                                            //PurchaseCost = Convert.ToDecimal(row["PURCHCOST"] == DBNull.Value ? 0 : row["PURCHCOST"]),
                                            //PurchaseSource = new Source { SouPrimaryId = Convert.ToInt64(row["PURCHSRCE"] == DBNull.Value ? 0 : row["PURCHSRCE"]), },
                                            //PropertyId = Convert.ToString(row["PROPERTYID"]),
                                            //Archived = Convert.ToString(row["Archived"]),
                                            //Notes = Convert.ToString(row["Notes"] == DBNull.Value ? "" : row["Notes"]),
                                            //RoomNo = Convert.ToString(row["ROOM"] == DBNull.Value ? "" : row["ROOM"]),
                                            //MobileEnabled = Convert.ToBoolean(row["Mobile"] == DBNull.Value ? 0 : row["Mobile"]),
                                            //Room = new RoomInfo { PrimaryId = Convert.ToInt64(row["ROMPrimaryId"] == DBNull.Value ? 0 : row["ROMPrimaryId"]) },
                                        });
                }
            }

            catch (Exception ex)
            {
                logger.ErrorFormat("GetAssets => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
            logger.InfoFormat("GetAssets({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);
            return equipments;
        }

        private DataTable GetEquipmentData(DateTime lastUpdateTime, long workerID, string sessionId)
        {
            DataTable data = new DataTable();
            try
            {
                FilterInfo filter = GetFilter(sessionId);
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;
                        command.CommandTimeout = 0;

                        command.CommandText += @"SELECT A.EquipId, A.CCCPrimaryId, A.[Control], A.DC, A.MDL,A.[Owner],A.Location,getdate() as LastSyncDate
                        FROM Equipment A
                        WHERE EXISTS
	                        (
		                        SELECT wkOrders.EquipId FROM wkOrders 
			                        INNER JOIN workerCenter ON wkOrders.WKRPrimaryId = WorkerCenter.WKRPrimaryId 
			                        WHERE wkOrders.WCCPrimaryId = WorkerCenter.WCCPrimaryId
			                        AND (WorkerCenter.[Status] = 'Worker' OR WorkerCenter.[Status] = 'Virtual' or WorkerCenter.[Status] = 'Vendor')
			                        AND OpenFlag = 1 AND wkOrders.EquipId = A.EquipId ";

                        if (filter.WKRPrimaryIds != null)
                        {
                            if (filter.WKRPrimaryIds.Trim().Contains("Unassigned"))
                            {
                                string workers = utility.Remove(filter.WKRPrimaryIds.Trim(), "Unassigned");


                                if (workers.Trim() != string.Empty)
                                {
                                    command.CommandText += " AND ( wkOrders.WKRPrimaryId in ('" + workers.Replace(",", "','") + "' ) or wkOrders.WKRPrimaryId is null)";
                                }
                                else
                                {
                                    command.CommandText += " AND  wkOrders.WKRPrimaryId is null ";
                                }
                            }
                            else
                            {
                                command.CommandText += " AND wkOrders.WKRPrimaryId in ('" + filter.WKRPrimaryIds.Replace(",", "','") + "' ) ";
                            }
                            command.CommandText += " AND wkOrders.ACCPrimaryId in ('" + filter.ACCPrimaryIds.Replace(",", "','") + "' ) ";
                        }
                        else
                        {
                            command.CommandText += " AND  wkOrders.WKRPrimaryId = @WKRPrimaryId ";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@WKRPrimaryId", DbType = DbType.Int64, Value = workerID });
                        }

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += @" AND wkOrders.lastupdatetime >= @lastUpdateTime ";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }

                        command.CommandText += " ) ";

                        //utility.Writelog(command.CommandText.Trim());

                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            data.Load(dr);
                            dr.Close();
                        }
                    }
                    connection.Close();
                }
                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetWorkOrdersData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> GetDeletedAssets(DateTime lastUpdateTime, long workerID)
        {
            int start = Environment.TickCount;

            List<string> assets = new List<string>();
            try
            {
                //DateTime dt = (DateTime)SqlDateTime.MinValue;
                //if (lastUpdateTime > dt)
                //{
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            //command.CommandText = @"SELECT [EquipId] FROM [dbo].[Equipment] WHERE [LastUpdateTime] >= @lastUpdateTime";

                            //lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            //command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            command.CommandText += @"SELECT A.EquipId 
                        FROM Equipment A
                        WHERE EXISTS
	                        (
		                        SELECT wkOrders.EquipId FROM wkOrders 
			                        INNER JOIN workerCenter ON wkOrders.WKRPrimaryId = WorkerCenter.WKRPrimaryId 
			                        WHERE wkOrders.WKRPrimaryId = @WKRPrimaryId AND wkOrders.WCCPrimaryId = WorkerCenter.WCCPrimaryId
			                        AND (WorkerCenter.[Status] = 'Worker' OR WorkerCenter.[Status] = 'Virtual' or WorkerCenter.[Status] = 'Vendor')
			                        AND OpenFlag = 1 AND wkOrders.EquipId = A.EquipId ";

                            command.Parameters.Add(new SqlParameter { ParameterName = "@WKRPrimaryId", DbType = DbType.Int64, Value = workerID });

                            DateTime dt = (DateTime)SqlDateTime.MinValue;
                            if (lastUpdateTime != dt)
                            {
                                lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                                command.CommandText += @" AND wkOrders.lastupdatetime >= @lastUpdateTime ";
                                command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                            }

                            command.CommandText += " ) ";

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            assets.AddRange(from DataRow row in data.Rows
                                            select Convert.ToString(row["EquipId"]));
                        }
                    }
                //}
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedAssets => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedAssets({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return assets;
        }
        #endregion

        #region Security
        public IEnumerable<SecuritiesSync> GetSecurities(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<SecuritiesSync> securities = new List<SecuritiesSync>();
            try
            {
                DataTable data = GetSecuritiesData(lastUpdateTime);

                securities.AddRange(from DataRow row in data.Rows
                                    select new SecuritiesSync
                                    {
                                        SecPrimaryId = Convert.ToInt64(row["SecPrimaryId"]),
                                        UserName = Convert.ToString(row["User"]),
                                        UserId = Convert.ToInt32(row["UserId"]),
                                        Password = ((byte[])row["Password"]),
                                        CodeKey = Convert.ToByte(row["CodeKey"]),
                                        AccessToWosyst = Convert.ToByte(row["RoleWosyst"] == DBNull.Value ? 0 : row["RoleWosyst"]),
                                        AssociatedWorker = Convert.ToInt64(row["WkrPrimaryId"] == DBNull.Value ? 0 : row["WkrPrimaryId"]),
                                        RTLSEnabled = Convert.ToByte(row["RTLSEnabled"] == DBNull.Value ? 0 : row["RTLSEnabled"]),
                                        //Modified by Srikanth Nuvvula on 14th Nov'2014 for ISS - 4736(If Windows user tries to Re-Open WO, its raising an alert & it need to be fixed.)
                                        //Added WindowsUser, unable to fetch security details when logged with Windows User
                                        WindowsUser = Convert.ToString(row["WindowsUser"] == DBNull.Value ? string.Empty : row["WindowsUser"])
                                    });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetSecurities => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetSecurities({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);
            return securities;
        }

        private DataTable GetSecuritiesData(DateTime lastUpdateTime)
        {
            DataTable data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;
                        command.CommandText += @"SELECT [SecPrimaryId],
                                                   [User],
                                                   [UserId],
                                                   [Password],
                                                   [CodeKey],
                                                   [WkrPrimaryId],
                                                   [RoleWosyst],
                                                   [RTLSEnabled], 
                                                   [WindowsUser],                 
                                                   ROW_NUMBER() OVER (ORDER BY SecPrimaryId) as RowNum
                                              FROM [dbo].[Security] where RoleMobileTech=1 and archived = '' ";

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += " and [LastUpdateTime] >= @lastUpdateTime";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        dataAdapter.Fill(data);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetSecuritiesData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> GetDeletedSecurities(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> securities = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT SecPrimaryId FROM [dbo].[Security]  WHERE RoleMobileTech=1 and archived = '' 
                                                     AND [LastUpdateTime] >= @lastUpdateTime";


                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            securities.AddRange(from DataRow row in data.Rows
                                                select Convert.ToString(row["SecPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedSecurities => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedSecurities({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return securities;
        }
        #endregion

        #region VersionDetails
        public IEnumerable<SystemSetting> GetVersionDetails()
        {
            List<SystemSetting> versionDetails = new List<SystemSetting>();

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"select top 1 build from databaseversion order by dbvprimaryid desc";

                        DataTable data = new DataTable();
                        SqlDataAdapter dataAdaptar = new SqlDataAdapter(command);

                        dataAdaptar.Fill(data);

                        versionDetails.AddRange(from DataRow row in data.Rows
                                                select new SystemSetting
                                                {
                                                    SettingField = "DB_Version",
                                                    SettingValue = Convert.ToString(row[0]).Trim()
                                                });


                        int buildNo = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build;

                        SystemSetting webServiceVersionDetails = new SystemSetting();
                        webServiceVersionDetails.SettingField = "WS_Version";
                        webServiceVersionDetails.SettingValue = buildNo.ToString();
                        versionDetails.Add(webServiceVersionDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetVersionDetails => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            return versionDetails;
        }
        #endregion

        #region Results
        public IEnumerable<SystemCode> GetResults(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<SystemCode> results = new List<SystemCode>();
            try
            {
                DataTable data = GetResultsData(lastUpdateTime);

                results.AddRange(from DataRow row in data.Rows
                                 select new SystemCode
                                 {
                                     PrimaryId = Convert.ToInt64(row["ResPrimaryId"]),
                                     Code = Convert.ToInt16(row["ResultCode"]),
                                     Description = Convert.ToString(row["Description"])
                                 });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetResults => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }


            logger.InfoFormat("GetResults({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);
            return results;
        }

        private DataTable GetResultsData(DateTime lastUpdateTime)
        {
            DataTable data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;
                        command.CommandText += @"SELECT ResPrimaryId, ResultCode, [Description] FROM Codes_Result";

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += " where [LastUpdateTime] >= @lastUpdateTime";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        dataAdapter.Fill(data);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetResultsData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> GetDeletedResults(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> results = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT ResPrimaryId FROM [dbo].[Codes_Result]  WHERE [LastUpdateTime] >= @lastUpdateTime";


                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            results.AddRange(from DataRow row in data.Rows
                                             select Convert.ToString(row["ResPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedResults => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedResults({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return results;
        }
        #endregion

        #region Result Center
        public IEnumerable<string> GetDeletedResultCenters(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> results = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT RSCPrimaryId FROM [dbo].[Codes_ResultCenter]  WHERE [LastUpdateTime] >= @lastUpdateTime";


                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            results.AddRange(from DataRow row in data.Rows
                                             select Convert.ToString(row["RSCPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedResultCenters => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedResultCenters({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return results;
        }

        private DataTable GetResultCentersData(DateTime lastUpdateTime)
        {
            DataTable data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;
                        command.CommandText += @"SELECT RSCPrimaryId, RESPrimaryId, WCCPrimaryId, Archived FROM Codes_ResultCenter
                                                 WHERE Archived = '' ";

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += " and [LastUpdateTime] >= @lastUpdateTime";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        dataAdapter.Fill(data);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetResultCentersData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<ResultInfo> GetResultCenters(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<ResultInfo> resultCenters = new List<ResultInfo>();
            try
            {
                DataTable data = GetResultCentersData(lastUpdateTime);

                resultCenters.AddRange(from DataRow row in data.Rows
                                       select new ResultInfo
                                       {
                                           PrimaryId = Convert.ToInt64(row["RESPrimaryId"]),
                                           Center = new ResultCenter
                                           {
                                               RSCPrimaryId = Convert.ToInt64(row["RSCPrimaryId"]),
                                               Archived = Convert.ToString(row["Archived"]),
                                               ServiceCenter = new SystemCode { PrimaryId = Convert.ToInt64(row["WCCPrimaryId"]) }
                                           }
                                       });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetResultCenters => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }


            logger.InfoFormat("GetResultCenters({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);
            return resultCenters;
        }
        #endregion

        #region RequestCenters
        public IEnumerable<RequestInfo> GetRequestCenters(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<RequestInfo> requestCenters = new List<RequestInfo>();
            try
            {
                DataTable data = GetRequestCentersData(lastUpdateTime);

                requestCenters.AddRange(from DataRow row in data.Rows
                                        select new RequestInfo
                                        {
                                            PrimaryId = Convert.ToInt64(row["REQPrimaryId"]),
                                            Center = new RequestCenter
                                            {
                                                RqcPrimaryId = Convert.ToInt64(row["RQCPrimaryId"]),
                                                RequireControl = Convert.ToString(row["RequireControl"]),
                                                RequireFaultCode = Convert.ToString(row["RequireFaultCode"]),
                                                ServiceCenter = new SystemCode { PrimaryId = Convert.ToInt64(row["WCCPrimaryId"]) }
                                            }
                                        });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetRequestCenters => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetRequestCenters({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);
            return requestCenters;
        }

        private DataTable GetRequestCentersData(DateTime lastUpdateTime)
        {
            DataTable data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;

                        command.CommandText += @"SELECT RQCPrimaryId, REQPrimaryId, WCCPrimaryId, RequireFaultCode, RequireControl FROM Codes_RequestCenter
                                                         WHERE Archived = '' ";

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += " and [LastUpdateTime] >= @lastUpdateTime";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        dataAdapter.Fill(data);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Codes_RequestCenter => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> GetDeletedRequestCenters(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> results = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT RQCPrimaryId FROM [dbo].[Codes_RequestCenter]  WHERE [LastUpdateTime] >= @lastUpdateTime";

                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            results.AddRange(from DataRow row in data.Rows
                                             select Convert.ToString(row["RQCPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedRequestCenters => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedRequestCenters({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return results;
        }
        #endregion

        #region Codes_Fault
        public IEnumerable<string> GetDeletedFaults(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> results = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT FLTPrimaryId FROM [dbo].[Codes_Fault]  WHERE [LastUpdateTime] >= @lastUpdateTime";


                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            results.AddRange(from DataRow row in data.Rows
                                             select Convert.ToString(row["FLTPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedFaults => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedFaults({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return results;
        }

        private DataTable GetFaultsData(DateTime lastUpdateTime)
        {
            DataTable data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;
                        command.CommandText += @"SELECT FLTPrimaryId, FaultCode, [Description] FROM Codes_Fault";

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += " where [LastUpdateTime] >= @lastUpdateTime";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        dataAdapter.Fill(data);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetFaultsData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<SystemCode> GetFaults(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<SystemCode> results = new List<SystemCode>();
            try
            {
                DataTable data = GetFaultsData(lastUpdateTime);

                results.AddRange(from DataRow row in data.Rows
                                 select new SystemCode
                                 {
                                     PrimaryId = Convert.ToInt64(row["FLTPrimaryId"]),
                                     Code = Convert.ToInt16(row["FaultCode"]),
                                     Description = Convert.ToString(row["Description"])
                                 });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetFaults => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }


            logger.InfoFormat("GetFaults({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);
            return results;
        }

        #endregion

        #region FaultCenters
        public IEnumerable<string> GetDeletedFaultCenters(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> faults = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT FLCPrimaryId FROM [dbo].[Codes_FaultCenter]  WHERE [LastUpdateTime] >= @lastUpdateTime";

                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            faults.AddRange(from DataRow row in data.Rows
                                            select Convert.ToString(row["FLCPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedFaultCenters => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedFaultCenters({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return faults;
        }

        private DataTable GetFaultCentersData(DateTime lastUpdateTime)
        {
            DataTable data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;
                        command.CommandText += @"SELECT FLCPrimaryId, FLTPrimaryId, WCCPrimaryId, Archived FROM Codes_FaultCenter
                                                WHERE Archived = '' ";

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += " and [LastUpdateTime] >= @lastUpdateTime";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        dataAdapter.Fill(data);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetFaultsData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<FaultInfo> GetFaultCenters(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<FaultInfo> faultCenters = new List<FaultInfo>();
            try
            {
                DataTable data = GetFaultCentersData(lastUpdateTime);

                faultCenters.AddRange(from DataRow row in data.Rows
                                      select new FaultInfo
                                      {
                                          PrimaryId = Convert.ToInt64(row["FLTPrimaryId"]),
                                          Center = new FaultCenter
                                          {
                                              FLCPrimaryId = Convert.ToInt64(row["FLCPrimaryId"]),
                                              Archived = Convert.ToString(row["Archived"]),
                                              ServiceCenter = new SystemCode { PrimaryId = Convert.ToInt64(row["WCCPrimaryId"]) }
                                          }
                                      });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetRequestCenters => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetRequestCenters({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);
            return faultCenters;
        }
        #endregion

        #region Codes_ValidRequestsResults
        public IEnumerable<string> GetDeletedValidRequestsResults(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> validReqResults = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT VRRPrimaryId FROM [dbo].[Codes_ValidRequestsResults]  WHERE [LastUpdateTime] >= @lastUpdateTime";

                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            validReqResults.AddRange(from DataRow row in data.Rows
                                                     select Convert.ToString(row["VRRPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedValidRequestsResults => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedValidRequestsResults({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return validReqResults;
        }

        private DataTable GetValidRequestsResultsData(DateTime lastUpdateTime)
        {
            DataTable data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;
                        command.CommandText += @"SELECT VRRPrimaryId, WCCPrimaryId, REQPrimaryId, RESPrimaryId FROM Codes_ValidRequestsResults ";

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += " where [LastUpdateTime] >= @lastUpdateTime";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        dataAdapter.Fill(data);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetValidRequestsResultsData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<ValidRequestsResult> GetValidRequestsResults(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<ValidRequestsResult> validRequestsResults = new List<ValidRequestsResult>();
            try
            {
                DataTable data = GetValidRequestsResultsData(lastUpdateTime);

                validRequestsResults.AddRange(from DataRow row in data.Rows
                                              select new ValidRequestsResult
                                              {
                                                  VRRPrimaryId = Convert.ToInt64(row["VRRPrimaryId"]),
                                                  ServiceCenter = new SystemCode { PrimaryId = Convert.ToInt64(row["WCCPrimaryId"]) },
                                                  Request = new SystemCode { PrimaryId = Convert.ToInt64(row["REQPrimaryId"]) },
                                                  Result = new SystemCode { PrimaryId = Convert.ToInt64(row["RESPrimaryId"]) }
                                              });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetValidRequestsResults => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetValidRequestsResults({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);
            return validRequestsResults;
        }

        #endregion

        #region WOText
        public IEnumerable<WorkOrderText> GetWOTexts(DateTime lastUpdateTime, long workerID, string sessionId)
        {
            int start = Environment.TickCount;

            List<WorkOrderText> woTexts = new List<WorkOrderText>();
            try
            {
                SyncParams syncParams = new SyncParams();

                using (DataTable data = GetWOTextData(lastUpdateTime, workerID, sessionId))
                {
                    System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

                    woTexts.AddRange(from DataRow row in data.Rows
                                     select new WorkOrderText
                                     {
                                         WorkOrder = new WorkOrder { WorPrimaryId = long.Parse(row["WORPrimaryId"].ToString()) },
                                         Text = encoding.GetBytes(Convert.ToString(row.IsNull("TextField") ? string.Empty : row["TextField"].ToString()).Trim()),
                                         ToolTipText = String.IsNullOrEmpty(row["TextField"].ToString()) ? string.Empty : row["TextField"].ToString().Trim().Length > 53 ? row["TextField"].ToString().Trim().Substring(0, 53) : row["TextField"].ToString().Trim()
                                     });
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetWOTexts => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
            logger.InfoFormat("GetWOTexts({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);
            return woTexts;
        }

        private DataTable GetWOTextData(DateTime lastUpdateTime, long workerID,string sessionID)
        {
            DataTable data = new DataTable();
            try
            {
                FilterInfo filter = GetFilter(sessionID);
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;

                        command.CommandText += @"SELECT [WORPrimaryId], substring([TextField],0,7999) as TextField FROM [DBO].[Wotext] WHERE WORPrimaryId IN 
		                                                (SELECT WORPrimaryId FROM wkOrders 
                                                                INNER JOIN workerCenter ON wkOrders.WKRPrimaryId = WorkerCenter.WKRPrimaryId 
                                                                WHERE wkOrders.WCCPrimaryId = WorkerCenter.WCCPrimaryId
                                                                AND (WorkerCenter.[Status] = 'Worker' OR WorkerCenter.[Status] = 'Virtual' or WorkerCenter.[Status] = 'Vendor')
                                                                AND OpenFlag = 1  ";

                        if (filter.WKRPrimaryIds != null)
                        {
                            if (filter.WKRPrimaryIds.Trim().Contains("Unassigned"))
                            {
                                string workers = utility.Remove(filter.WKRPrimaryIds.Trim(), "Unassigned");
                                if (workers.Trim() != string.Empty)
                                {
                                    command.CommandText += " AND ( wkOrders.WKRPrimaryId in ('" + workers.Replace(",", "','") + "' ) or wkOrders.WKRPrimaryId is null)";
                                }
                                else
                                {
                                    command.CommandText += " AND  wkOrders.WKRPrimaryId is null ";
                                }
                            }
                            else
                            {
                                command.CommandText += " AND wkOrders.WKRPrimaryId in ('" + filter.WKRPrimaryIds.Replace(",", "','") + "' ) ";
                            }

                            command.CommandText += " AND wkOrders.ACCPrimaryId in ('" + filter.ACCPrimaryIds.Replace(",", "','") + "' ) ";
                        }
                        else
                        {
                            command.CommandText += " AND  wkOrders.WKRPrimaryId = @WKRPrimaryId";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@WKRPrimaryId", DbType = DbType.Int64, Value = workerID });
                        }


                        

                        command.CommandText += " )";

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += @" AND lastupdatetime >= @lastUpdateTime ";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }

                        //Writelog(command.CommandText);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            data.Load(dr);
                            dr.Close();
                        }
                    }
                    connection.Close();
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetWOTextData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> GetDeletedWOTexts(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> woTexts = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT [WORPrimaryId] FROM [dbo].[Wotext] WHERE WORPrimaryid IN (SELECT WORPrimaryid FROM wkorders WHERE openflag = 1) And [LastUpdateTime] >= @lastUpdateTime";

                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            woTexts.AddRange(from DataRow row in data.Rows
                                             select Convert.ToString(row["WORPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedWOTexts => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedWOTexts({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return woTexts;
        }
        #endregion

        #region TimeType
        public IEnumerable<SystemCode> GetTimeTypes(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<SystemCode> timeTypes = new List<SystemCode>();
            try
            {
                DataTable data = GetTimeTypesData(lastUpdateTime);

                timeTypes.AddRange(from DataRow row in data.Rows
                                   select new SystemCode
                                   {
                                       PrimaryId = Convert.ToInt64(row["WtbPrimaryId"]),
                                       Description = Convert.ToString(row["Description"])
                                   });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetTimeTypes => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }


            logger.InfoFormat("GetTimeTypes({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);
            return timeTypes;
        }

        private DataTable GetTimeTypesData(DateTime lastUpdateTime)
        {
            DataTable data = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Empty;
//                        command.CommandText += @"SELECT WTBPrimaryId, CAST(TimeTypeCode as nvarchar) + ' ' + Description as [Description] 
//                                                    FROM Codes_WorkOrderTimeType order by TimeTypeCode";

                        command.CommandText += @"SELECT WTBPrimaryId, Description as [Description] FROM Codes_WorkOrderTimeType ";

                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        if (lastUpdateTime != dt)
                        {
                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.CommandText += " where [LastUpdateTime] >= @lastUpdateTime";
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });
                        }

                        command.CommandText += " order by TimeTypeCode";
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        dataAdapter.Fill(data);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetTimeTypesData => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> GetDeletedTimeTypes(DateTime lastUpdateTime)
        {
            int start = Environment.TickCount;

            List<string> timeTypes = new List<string>();
            try
            {
                DateTime dt = (DateTime)SqlDateTime.MinValue;
                if (lastUpdateTime > dt)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // end any open sessions for the user first
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"SELECT WTBPrimaryId FROM [dbo].[Codes_WorkOrderTimeType]  WHERE [LastUpdateTime] >= @lastUpdateTime";


                            lastUpdateTime = utility.getTimeZoneSettings(lastUpdateTime);
                            command.Parameters.Add(new SqlParameter { ParameterName = "@lastUpdateTime", DbType = DbType.DateTime, Value = lastUpdateTime });

                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            dataAdapter.Fill(data);

                            timeTypes.AddRange(from DataRow row in data.Rows
                                               select Convert.ToString(row["WtbPrimaryId"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDeletedTimeTypes => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDeletedTimeTypes({0}) Execute Time: {1} ms", lastUpdateTime, Environment.TickCount - start);

            return timeTypes;
        }
        #endregion
        #endregion

        #region Upload Section
        #region Work Order Upload

        public int verifyWorkOrderStateBeforeUpdating(WorkOrderDetails workOrderDetails)
        {
            int retVal = 1;
            Int64 wkrPrimaryID = 0;
            //Int64 reqPrimaryID = 0;
            int requestCode = 0;
            byte openFlag = 1;

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT WKRPrimaryID, Codes_Request.RequestCode, OpenFlag from [dbo].[WkOrders] " +
                            "Inner Join Codes_Request on WkOrders.REQPrimaryID = Codes_Request.REQPrimaryID WHERE WORPrimaryID = @WORPrimaryID";
                        command.Parameters.Add(new SqlParameter { ParameterName = "@WORPrimaryID", DbType = DbType.Int64, Value = workOrderDetails.WorPrimaryId });

                        SqlDataReader rReader = command.ExecuteReader();
                        if (rReader.Read())
                        {
                            wkrPrimaryID = Convert.ToInt64(rReader["WKRPrimaryID"] == DBNull.Value ? 0 : rReader["WKRPrimaryID"]);
                            //reqPrimaryID = Convert.ToInt64(rReader["REQPrimaryID"]);
                            requestCode = Convert.ToInt16(rReader["RequestCode"]);
                            openFlag = Convert.ToByte(rReader["OpenFlag"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("checkPrimaryWorkerOfWorkOrder => {0}\n{1}", ex.Message, ex.StackTrace);
                retVal = 0;
                throw;
            }

            if (requestCode == 99)
                retVal = 3;
            else if (openFlag == Convert.ToByte(0))
                retVal = 4;
            else if (wkrPrimaryID != workOrderDetails.Worker.WKRPrimaryId)
                retVal = 2;

            return retVal;
        }

        public int InsertUpdateWorkOrderDetails(WorkOrderDetails workOrderDetails, User loginUser)
        {
            int retVal = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        DateTime dt = (DateTime)SqlDateTime.MinValue;
                        #region Insert/Update WorkOrder

                        using (SqlCommand sqlQuery = connection.CreateCommand())
                        {
                            sqlQuery.Transaction = transaction;
                            sqlQuery.CommandText = "dbo.prcInsertUpdateWkOrdersFromMobileTech";
                            sqlQuery.CommandType = CommandType.StoredProcedure;
                            SqlParameter[] WoParams = GetWorkOrderParamters();
                            SetWorkOrderParameters(WoParams, workOrderDetails);
                            sqlQuery.Parameters.AddRange(WoParams);
                            sqlQuery.ExecuteNonQuery();
                        }

                        #endregion

                        #region Insert/Update WoText value

                        using (SqlCommand cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = CheckWOTextExist(workOrderDetails) ? @" Update WOTEXT Set TEXTFIELD=@TEXTFIELD Where WORPRIMARYID=@WORPRIMARYID " : @" INSERT INTO WOTEXT (WORPRIMARYID, TEXTFIELD) VALUES (@WORPRIMARYID, @TEXTFIELD) ";
                            cmd.Transaction = transaction;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add(new SqlParameter { ParameterName = "@WORPRIMARYID", DbType = DbType.Int64, Value = workOrderDetails.WorPrimaryId });
                            cmd.Parameters.Add(new SqlParameter { ParameterName = "@TEXTFIELD", DbType = DbType.String, Value = string.IsNullOrEmpty(workOrderDetails.WOTextNotes) ? "" : workOrderDetails.WOTextNotes.Replace("\0", "") }); //we are getting "\0" for empty string
                            cmd.ExecuteNonQuery();
                        }
                        #endregion

                        #region Insert/Update WoTime entries

                        string strWOMPrimaryIds = string.Empty;
                        //Insert/Update TimeEntry Details to WOTime table
                        if (workOrderDetails.TimeEntries != null)
                        {
                            //Nahid Ahmed Jan 15, 2015: Unless downloading of all time entries is implemented, deleting before inserting should not be allowed
                            /*
                            //Delete All Deleted Wo Time Entries before Insert/Update
                            using (SqlCommand cmd = connection.CreateCommand())
                            {
                                foreach (WorkOrderTime WoTimeEntry in workOrderDetails.TimeEntries)
                                {
                                    if (WoTimeEntry.PrimaryId != 0)
                                    {
                                        if (strWOMPrimaryIds == string.Empty)
                                        {
                                            strWOMPrimaryIds += WoTimeEntry.PrimaryId.ToString();
                                        }
                                        else
                                        {
                                            strWOMPrimaryIds += "," + WoTimeEntry.PrimaryId;
                                        }
                                    }
                                }

                                if (strWOMPrimaryIds != string.Empty)
                                {
                                    cmd.CommandText = @" DELETE FROM WOTime WHERE WOMPrimaryId NOT IN (" + strWOMPrimaryIds + ") AND WORPrimaryId=@WORPRIMARYID";
                                }
                                else
                                {
                                    cmd.CommandText = @" DELETE FROM WOTime WHERE WORPrimaryId=@WORPRIMARYID";
                                }
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Add(new SqlParameter { ParameterName = "@WORPRIMARYID", DbType = DbType.Int64, Value = workOrderDetails.WorPrimaryId });
                                cmd.ExecuteNonQuery();
                            }
                            */

                            foreach (WorkOrderTime WoTimeEntry in workOrderDetails.TimeEntries)
                            {
                                using (SqlCommand cmd = connection.CreateCommand())
                                {
                                    cmd.Transaction = transaction;
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@w_WOMPrimaryid", DbType = DbType.Int64, Value = WoTimeEntry.PrimaryId });
                                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@w_WORPrimaryid", DbType = DbType.Int64, Value = workOrderDetails.WorPrimaryId });

                                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@w_LogDate", DbType = DbType.DateTime, Value = utility.getTimeZoneSettings( WoTimeEntry.LogDate ) });

                                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@w_WkrPrimaryid", DbType = DbType.Int64, Value = WoTimeEntry.Worker.WKRPrimaryId });
                                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@w_WTBPrimaryid", DbType = DbType.Int64, Value = WoTimeEntry.TimeType.PrimaryId });
                                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@w_ActualTime", DbType = DbType.Decimal, Value = WoTimeEntry.ActualTime });
                                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@w_notes", DbType = DbType.String, Value = string.IsNullOrEmpty(WoTimeEntry.strNotes) ? Convert.DBNull : WoTimeEntry.strNotes.Replace("\0", "") }); //we are getting "\0" for empty string
                                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@CreatedUser", DbType = DbType.String, Value = "Ml_User" });
                                    cmd.CommandText = "dbo.prcWOTimeUploadFromMobileTech";
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        if (workOrderDetails.DeletedTimeEntries != null)
                        {
                            using (SqlCommand cmd = connection.CreateCommand())
                            {
                                cmd.Parameters.Add("@WOMPRIMARYID", DbType.Int64);
                                foreach (WorkOrderDeletedTimeEntries WoTimeEntry in workOrderDetails.DeletedTimeEntries)
                                {
                                    cmd.Transaction = transaction;
                                    cmd.CommandText = @" DELETE FROM WOTime WHERE WOMPrimaryId=@WOMPRIMARYID";
                                    cmd.Parameters["@WOMPRIMARYID"].Value = WoTimeEntry.PrimaryId;
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        #endregion


                        #region Insert WorkOrder Audit Log Entry

                        //Insert WorkOrder Audit Log Entry when a change is made on WorkOrder
                        //insertUpdateFlag=true for Create and false for Update/Close
                        using (SqlCommand cmd = connection.CreateCommand())
                        {
                            string strOperation = workOrderDetails.insertUpdateFlag && workOrderDetails.OpenFlag == 0 ? "Create & Close Record" : (workOrderDetails.insertUpdateFlag ? "Create Record" : (workOrderDetails.OpenFlag == 0 ? "Close Record" : "Edited Record"));

                            cmd.CommandText = "[dbo].[prcInsertAuditLogWorkorder]";
                            cmd.Transaction = transaction;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter { ParameterName = "@WORPrimaryid", DbType = DbType.Int64, Value = workOrderDetails.WorPrimaryId });
                            cmd.Parameters.Add(new SqlParameter { ParameterName = "@Operation", DbType = DbType.String, Value = strOperation });
                            cmd.Parameters.Add(new SqlParameter { ParameterName = "@UserName", DbType = DbType.String, Value = loginUser != null ? loginUser.Username : workOrderDetails.ApplicationName + " User" });

                            if (workOrderDetails.ApplicationName == "mTech.iOS")
                            {
                                cmd.Parameters.Add(new SqlParameter { ParameterName = "@SourceSystem", DbType = DbType.String, Value = "MobileTech" });
                            }
                            else
                            {
                                cmd.Parameters.Add(new SqlParameter { ParameterName = "@SourceSystem", DbType = DbType.String, Value = "iPad Asset Enterprise" });
                            }
                            cmd.Parameters.Add(new SqlParameter { ParameterName = "@Notes", DbType = DbType.String, Value = Convert.DBNull });
                            cmd.ExecuteNonQuery();
                        }

                        #endregion

                        transaction.Commit();
                        utility.Writelog(workOrderDetails.OpenWorkOrderStatus.Description + ": added action successfully to workorder : " + workOrderDetails.Number);
                        retVal = 1;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        utility.Writelog("Exception/InsertUpdateWorkOrderDetails:- msg: " + ex.Message + " StackTrace:" + ex.StackTrace);
                        throw;
                    }
                }
            }
            return retVal;
        }

        private static SqlParameter[] GetWorkOrderParamters()
        {
            try
            {
                SqlParameter[] parameters = new[] {
                    new SqlParameter("@REQPrimaryId", SqlDbType.BigInt),
                    new SqlParameter("@RESPrimaryId", SqlDbType.BigInt),
                    new SqlParameter("@FLTPrimaryId", SqlDbType.BigInt),
                    new SqlParameter("@WORprimaryid", SqlDbType.BigInt),
                    new SqlParameter("@OWSPrimaryid", SqlDbType.BigInt),
                    new SqlParameter("@SFTEST", SqlDbType.VarChar, 1),
                    new SqlParameter("@DATESTARTED", SqlDbType.DateTime),
                    new SqlParameter("@DATECOMPLETE", SqlDbType.DateTime),
                    new SqlParameter("@OPENFLAG", SqlDbType.TinyInt),
                    new SqlParameter("@CreatedUser", SqlDbType.VarChar, 60),
                    
                };
                return parameters;
            }
            catch (Exception ex)
            {
                Utilities utility = new Utilities();
                utility.Writelog("Exception in GetWorkOrderParamters(): " + ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private static void SetWorkOrderParameters(SqlParameter[] Params, WorkOrderDetails workOrderDetails)
        {
            try
            {
                Params[0].Value = workOrderDetails.Request != null && workOrderDetails.Request.PrimaryId != 0 ? workOrderDetails.Request.PrimaryId : Convert.DBNull;
                Params[1].Value = workOrderDetails.Result != null && workOrderDetails.Result.PrimaryId != 0 ? workOrderDetails.Result.PrimaryId : Convert.DBNull;
                Params[2].Value = workOrderDetails.Fault != null && workOrderDetails.Fault.PrimaryId != 0 ? workOrderDetails.Fault.PrimaryId : Convert.DBNull;
                Params[3].Value = workOrderDetails.WorPrimaryId;
                Params[4].Value = workOrderDetails.OpenWorkOrderStatus != null && workOrderDetails.OpenWorkOrderStatus.PrimaryId != 0 ? workOrderDetails.OpenWorkOrderStatus.PrimaryId : Convert.DBNull;
                Params[5].Value = string.IsNullOrEmpty(workOrderDetails.SafetyTest) ? Convert.DBNull : workOrderDetails.SafetyTest;

                Utilities utility = new Utilities();
                Params[6].Value = workOrderDetails.StartDate == DateTime.MinValue ? Convert.DBNull : utility.getTimeZoneSettings(workOrderDetails.StartDate);
                Params[7].Value = workOrderDetails.CompleteDate == DateTime.MinValue ? Convert.DBNull : utility.getTimeZoneSettings(workOrderDetails.CompleteDate);
                Params[8].Value = workOrderDetails.OpenFlag;
                Params[9].Value = "Ml_User";

            }
            catch (Exception ex)
            {
                Utilities utility = new Utilities();
                utility.Writelog("Exception in SetWorkOrderParameters(): " + ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public bool CheckWOTextExist(WorkOrder workOrder)
        {
            bool exist = false;
            const string sqlStatement = @" SELECT Count(WORPrimaryId) FROM WOText WITH (NOLOCK) Where WorPrimaryId = @WorPrimaryId ";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand sqlQuery = connection.CreateCommand())
                {
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@WorPrimaryId", DbType = DbType.Int64, Value = workOrder.WorPrimaryId });
                    sqlQuery.CommandText = sqlStatement;
                    object result = sqlQuery.ExecuteScalar();

                    if (result != null)
                    {
                        if (Convert.ToInt32(result) > 0)
                        {
                            exist = true;
                        }
                    }
                }
            }
            return exist;
        }

        public bool CheckTimeEntryExist(WorkOrderTime woTime, WorkOrder workOrder)
        {
            bool exist = false;
            const string sqlStatement = @" SELECT Count(WOMPrimaryId) FROM WOTime WITH (NOLOCK) Where WOMPrimaryId = @WOMPrimaryId and WorPrimaryId = @WorPrimaryId ";
            if (woTime.PrimaryId != 0)
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand sqlQuery = connection.CreateCommand())
                    {
                        sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@WOMPrimaryId", DbType = DbType.Int64, Value = woTime.PrimaryId });
                        sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@WorPrimaryId", DbType = DbType.Int64, Value = workOrder.WorPrimaryId });
                        sqlQuery.CommandText = sqlStatement;
                        object result = sqlQuery.ExecuteScalar();

                        if (result != null)
                        {
                            if (Convert.ToInt32(result) > 0)
                            {
                                exist = true;
                            }
                        }
                    }
                }
            }
            return exist;
        }

        #endregion
        #endregion

        #region PDADBID
        public PDADBID[] GetDBIDs(string username)
        {
            int start = Environment.TickCount;

            List<PDADBID> dbIDs = new List<PDADBID>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // end any open sessions for the user first
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"SELECT [dbId]
                                              FROM [dbo].[PDADbid]
                                             WHERE [username] = @username";

                        command.Parameters.Add(new SqlParameter { ParameterName = "@username", DbType = DbType.String, Value = username });

                        object result = command.ExecuteScalar();

                        if (result == null)
                        {
                            using (SqlCommand cmdInsert = connection.CreateCommand())
                            {
                                cmdInsert.CommandText = @"INSERT INTO [dbo].[PDADBId] ([Username]) VALUES (@Username);";
                                cmdInsert.Parameters.Add(new SqlParameter { ParameterName = "@Username", DbType = DbType.String, Value = username });
                                cmdInsert.ExecuteNonQuery();
                            }
                        }
                        
                        using (SqlCommand cmdSelect = connection.CreateCommand())
                        {
                            cmdSelect.Parameters.Add(new SqlParameter { ParameterName = "@username", DbType = DbType.String, Value = username });
                            cmdSelect.CommandText = @"SELECT [Dbid],[username],[WORPrimaryid],[WOMPrimaryid],[WOPPrimaryid]
                                                 ,[DevPrimaryid],[MKMDid],[SouPrimaryid],[UDDPrimaryid],[WOTPrimaryid],[WOMCHPrimaryid],[WOMCRPrimaryid],[PPOPrimaryid] 
                                                 from [dbo].[PDADBID] WHERE [username]= @usrname";
                            cmdSelect.Parameters.Add(new SqlParameter { ParameterName = "@usrname", DbType = DbType.String, Value = username });
                            DataTable data = new DataTable();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmdSelect);
                            dataAdapter.Fill(data);

                            dbIDs.AddRange(from DataRow row in data.Rows
                                           select new PDADBID
                                           {
                                               WORPrimaryId = Convert.ToInt64(row["WORPrimaryid"] == DBNull.Value ? 0 : row["WORPrimaryid"]),
                                               WOMPrimaryid = Convert.ToInt64(row["WOMPrimaryid"] == DBNull.Value ? 0 : row["WOMPrimaryid"]),
                                               WOPPrimaryid = Convert.ToInt64(row["WOPPrimaryid"] == DBNull.Value ? 0 : row["WOPPrimaryid"]),
                                               DevPrimaryid = Convert.ToInt64(row["DevPrimaryid"] == DBNull.Value ? 0 : row["DevPrimaryid"]),
                                               SouPrimaryid = Convert.ToInt64(row["SouPrimaryid"] == DBNull.Value ? 0 : row["SouPrimaryid"]),
                                               UDDPrimaryid = Convert.ToInt64(row["UDDPrimaryid"] == DBNull.Value ? (Convert.ToInt32(row["Dbid"]) * 1000000000L) : row["UDDPrimaryid"]),
                                               MKMDid = Convert.ToInt64(row["MKMDid"] == DBNull.Value ? 0 : row["MKMDid"]),
                                               Dbid = Convert.ToInt32(row["Dbid"] == DBNull.Value ? 0 : row["Dbid"]),
                                               username = Convert.ToString(row["username"] == DBNull.Value ? string.Empty : row["username"]),
                                               WOTPrimaryid = Convert.ToInt64(row["WOTPrimaryid"] == DBNull.Value ? (Convert.ToInt32(row["Dbid"]) * 1000000000L) : row["WOTPrimaryid"]),
                                               WOMCHPrimaryid = Convert.ToInt64(row["WOMCHPrimaryid"] == DBNull.Value ? 0 : row["WOMCHPrimaryid"]),
                                               WOMCRPrimaryid = Convert.ToInt64(row["WOMCRPrimaryid"] == DBNull.Value ? 0 : row["WOMCRPrimaryid"]),
                                               PPOPrimaryid = Convert.ToInt64(row["PPOPrimaryid"] == DBNull.Value ? 0 : row["PPOPrimaryid"])
                                           }
                                );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetDBIDs => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.InfoFormat("GetDBIDs({0}) Execute Time: {1} ms", username, Environment.TickCount - start);

            return dbIDs.ToArray();
        }

        public bool UpdatePDADBItems(PDADBID pdaDbID)
        {

            const string sqlStatement = @" Update [PDADBID] Set [WORPrimaryid]=@WORPrimaryId
            , [WOMPrimaryid]=@WOMPrimaryId, [WOPPrimaryid]=@WOPPrimaryId, [UDDPrimaryid]=@UDDPrimaryid
            , [WOTPrimaryid]=@WOTPrimaryId, [WOMCRPrimaryid] = @WOMCRPrimaryid
            , [WOMCHPrimaryid] = @WOMCHPrimaryid,[PPOPrimaryid] = @PPOPrimaryid Where [UserName] = @UserName ";
            //Added temporarily for UDD Issue
            utility.Writelog("PDADBID Update: WORPrimaryid/UDDPrimaryId/DBID: " + pdaDbID.WORPrimaryId + " / " + pdaDbID.UDDPrimaryid + " / " + pdaDbID.Dbid);

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand sqlQuery = connection.CreateCommand())
                {
                    sqlQuery.CommandText = sqlStatement;

                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@WORPrimaryId", DbType = DbType.Int64, Value = pdaDbID.WORPrimaryId });
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@WOMPrimaryId", DbType = DbType.Int64, Value = pdaDbID.WOMPrimaryid });
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@WOPPrimaryId", DbType = DbType.Int64, Value = pdaDbID.WOPPrimaryid });
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@UDDPrimaryid", DbType = DbType.Int64, Value = pdaDbID.UDDPrimaryid });
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@WOTPrimaryid", DbType = DbType.Int64, Value = pdaDbID.WOTPrimaryid });
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@UserName", DbType = DbType.String, Value = pdaDbID.username });
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@WOMCHPrimaryid", DbType = DbType.Int64, Value = pdaDbID.WOMCHPrimaryid });
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@WOMCRPrimaryid", DbType = DbType.Int64, Value = pdaDbID.WOMCRPrimaryid });
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@PPOPrimaryid", DbType = DbType.Int64, Value = pdaDbID.PPOPrimaryid });
                    sqlQuery.ExecuteNonQuery();

                    return true;
                }
            }
        }

        public bool InsertUpdateMobileTechFilters(FilterInfo filter)
        {

            const string sqlInsertStatement = @"Insert into MobileTechFilters (SECPrimaryId,FilterName,Active,FACPrimaryids,ACCPrimaryids,WKRPrimaryids)
              values (@SECPrimaryId,@FilterName,@Active,@FACPrimaryids,@ACCPrimaryids,@WKRPrimaryids)                                 ";

            const string sqlUpdateStatement = @" Update [MobileTechFilters] Set 
             [Active]=@Active, [FACPrimaryids]=@FACPrimaryids, [ACCPrimaryids]=@ACCPrimaryids
            , [WKRPrimaryids]=@WKRPrimaryids
              where MTFPrimaryid = @MTFPrimaryid ";
            
            utility.Writelog("MobileTechFilters Update: FilterName: " + filter.FilterName);

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand sqlQuery = connection.CreateCommand())
                {
                    if (filter.MTFPrimaryId == 0) {
                        sqlQuery.CommandText = sqlInsertStatement;
                        sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@FilterName", DbType = DbType.String, Value = filter.FilterName });
                        sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@SECPrimaryId", DbType = DbType.Int64, Value = filter.SECPrimaryId });
                    }else{
                        sqlQuery.CommandText = sqlUpdateStatement;
                        sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@MTFPrimaryid", DbType = DbType.Int64, Value = filter.MTFPrimaryId });
                    }
                    
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@Active", DbType = DbType.Byte, Value = filter.Active});
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@FACPrimaryids", DbType = DbType.String, Value = filter.FACPrimaryIds });
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@ACCPrimaryids", DbType = DbType.String, Value = filter.ACCPrimaryIds });
                    sqlQuery.Parameters.Add(new SqlParameter { ParameterName = "@WKRPrimaryids", DbType = DbType.String, Value = filter.WKRPrimaryIds });
                    sqlQuery.ExecuteNonQuery();

                    return true;
                }
            }
        }

        #endregion

    }
}