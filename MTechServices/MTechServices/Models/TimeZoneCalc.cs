using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Web;
using System.Data;
using System.Configuration;
using System.Collections;
//using log4net;

namespace MTechServices.Models
{
    public static class TimeZoneCalc
    {
  //      private static readonly ILog logger = LogManager.GetLogger(typeof(TimeZoneCalc));
        // if stored timestamp is UTC
        public static DateTime GetDateTimeUtc(this SqlDataReader reader, string name)
        {
            int fieldOrdinal = reader.GetOrdinal(name); 
            return GetDateTimeUtc(reader, fieldOrdinal);
        }

        // if stored timestamp is UTC
        public static DateTime GetDateTimeUtc(this SqlDataReader reader, int fieldOrdinal)
        {
            DateTime unspecified = reader.GetDateTime(fieldOrdinal);
            return DateTime.SpecifyKind(unspecified, DateTimeKind.Utc);
        }

        // if stored timestamp is UTC
        public static DateTime GetDateTimeClient1(this SqlDataReader reader, int fieldOrdinal)
        {
            DateTime client = reader.GetDateTime(fieldOrdinal);
            if (Convert.ToString(HttpContext.Current.Session["AdjustForTimeZone"]) == "true")
            {
                string timezone = Convert.ToString(HttpContext.Current.Session["ClientTimeZone"]);
                Debug.WriteLine("GetDateTimeClient for " + timezone);
                client = ConvertDateTimeFromUtc(DateTime.SpecifyKind(client, DateTimeKind.Utc), timezone);
            }
            return client;
        }

        // if stored timestamp is "server time"
        public static DateTime GetDateTimeLocal(this SqlDataReader reader, string name)
        {
            int fieldOrdinal = reader.GetOrdinal(name);
            return GetDateTimeLocal(reader, fieldOrdinal);
        }

        // if stored timestamp is "server time"
        public static DateTime GetDateTimeLocal(this SqlDataReader reader, int fieldOrdinal)
        {
            DateTime unspecified = reader.GetDateTime(fieldOrdinal);
            return DateTime.SpecifyKind(unspecified, DateTimeKind.Local);
        }

        // if stored timestamp is "server time"
        public static DateTime GetDateTimeClient(this SqlDataReader reader, int fieldOrdinal)
        {
            DateTime client = reader.GetDateTime(fieldOrdinal);
            if (Convert.ToString(HttpContext.Current.Session["AdjustForTimeZone"]) == "true")
            {
                string timezone = Convert.ToString(HttpContext.Current.Session["ClientTimeZone"]);
                Debug.WriteLine("GetDateTimeClient for " + timezone);
                client = ConvertDateTimeFromServer(DateTime.SpecifyKind(client, DateTimeKind.Local), timezone);
            }
            return client;
        }

        public static DateTime ConvertDateTimeFromUtc(DateTime utc, string clientTimeZone)
        {
            Debug.WriteLine("ConvertDateTimeFromUtc to " + clientTimeZone);
            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                tz = TimeZoneInfo.Local;
            }
            catch (InvalidTimeZoneException)
            {
                tz = TimeZoneInfo.Local;
            }
            DateTime dtClient = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
            return dtClient;
        }

        public static DateTime ConvertDateTimeToUTC(DateTime clientTime, string clientTimeZone)
        {
            Debug.WriteLine("ConvertDateTimeToUTC from " + clientTimeZone);
            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                tz = TimeZoneInfo.Local;
            }
            catch (InvalidTimeZoneException)
            {
                tz = TimeZoneInfo.Local;
            }
            DateTime dtUTC = TimeZoneInfo.ConvertTimeToUtc(clientTime, tz);
            return dtUTC;
        }

        public static DateTime ConvertDateTimeFromServer(DateTime serverTime)
        {
            try{
            if (serverTime != DateTime.MinValue)
            {
                string clientTimeZone = Convert.ToString(HttpContext.Current.Session["ClientTimeZone"]);
                //logger.ErrorFormat("clientTimeZone = ", clientTimeZone);
                return ConvertDateTimeFromServer(serverTime, clientTimeZone);
            }else{
                return serverTime;
            }} catch (Exception ex) {
                //logger.ErrorFormat("TimeZoneCalc.ConvertDateTimeFromServer => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

        }

        public static DateTime ConvertDateTimeFromServerForMobileTech(DateTime serverTime)
        {
            try
            {
                if (serverTime != DateTime.MinValue)
                {
                    string clientTimeZone = getClientTimeZone();
                    //logger.ErrorFormat("clientTimeZone = ", clientTimeZone);
                    return ConvertDateTimeFromServer(serverTime, clientTimeZone);
                }
                else
                {
                    return serverTime;
                }
            }
            catch (Exception ex)
            {
                //logger.ErrorFormat("TimeZoneCalc.ConvertDateTimeFromServer => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }

        }

        public static string getClientTimeZone()
        {
            ArrayList alTimeZones = new ArrayList();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"Select [Key], TimeZone from timezonesettings";

                    SqlDataReader rReader = null;
                    try
                    {
                        rReader = command.ExecuteReader();

                        while (rReader.Read())
                        {
                            if (Convert.ToInt32(rReader["Key"]) == Convert.ToInt32(0))
                            {
                                alTimeZones.Add(rReader["TimeZone"].ToString());
                            }
                            else if (Convert.ToInt32(rReader["Key"]) == Convert.ToInt32(1))
                            {
                                alTimeZones.Add(rReader["TimeZone"].ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Writelog("getTimeZoneSettings exception: " + ex.Message);
                    }
                    finally
                    {
                        if (rReader != null)
                        {
                            rReader.Close();
                            rReader.Dispose();
                        }
                    }
                }
                connection.Close();

                return alTimeZones[1].ToString();
            }
        }


        public static DateTime ConvertDateTimeFromServer(DateTime serverTime, string clientTimeZone)
        {
            //Debug.WriteLine("ConvertDateTimeFromServer to " + clientTimeZone);
            try
            {
                if (serverTime != DateTime.MinValue)
                {
                    TimeZoneInfo tz;
                    try
                    {
                        tz = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        tz = TimeZoneInfo.Local;
                    }
                    catch (InvalidTimeZoneException)
                    {
                        tz = TimeZoneInfo.Local;
                    }
                    //TimeZoneInfo tziServerZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                    DateTime dtUTC = TimeZoneInfo.ConvertTimeToUtc(serverTime, TimeZoneInfo.Local);
                    DateTime dtClient = TimeZoneInfo.ConvertTimeFromUtc(dtUTC, tz);
                    return dtClient;
                }
                else
                {
                    return serverTime;
                }
            }
            catch (Exception ex)
            {
                //logger.ErrorFormat("TimeZoneCalc.ConvertDateTimeFromServer(0,0) => {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public static DateTime ConvertDateTimeToServer(DateTime clientTime, string clientTimeZone)
        {
            Debug.WriteLine("ConvertDateTimeToServer from " + clientTimeZone);
            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                tz = TimeZoneInfo.Local;
            }
            catch (InvalidTimeZoneException)
            {
                tz = TimeZoneInfo.Local;
            }
            DateTime dtUTC = TimeZoneInfo.ConvertTimeToUtc(clientTime, tz);
            DateTime dtServer = TimeZoneInfo.ConvertTimeFromUtc(dtUTC, TimeZoneInfo.Local);
            return dtServer;
        }

        public static void ConvertTableDateTime(DataTable dtData)
        {
            if (Convert.ToString(HttpContext.Current.Session["AdjustForTimeZone"]) == "true" &&
                HttpContext.Current.Request.Headers["Content-Type"].ToLower().IndexOf("application/json") == -1)
            {
                foreach (DataColumn dc in dtData.Columns)
                {
                    if (dc.DataType == Type.GetType("System.DateTime"))
                    {
                        foreach (DataRow dr in dtData.Rows)
                        {
                            object value = dr[dc.Ordinal];
                            if (value != DBNull.Value)
                            {
                                dr[dc.Ordinal] = TimeZoneCalc.ConvertDateTimeFromServer(Convert.ToDateTime(dr[dc.Ordinal]));
                            }
                        }
                    }
                }
            }
        }

        public static void ConvertTableDateTimeForMobileTech(DataTable dtData)
        {
            //if (HttpContext.Current.Request.Headers["Content-Type"].ToLower().IndexOf("application/json") == -1)
            //{
                foreach (DataColumn dc in dtData.Columns)
                {
                    if (dc.DataType == Type.GetType("System.DateTime") && dc.ColumnName != "LastSyncDateTime")
                    {
                        foreach (DataRow dr in dtData.Rows)
                        {
                            object value = dr[dc.Ordinal];
                            if (value != DBNull.Value)
                            {
                                dr[dc.Ordinal] = TimeZoneCalc.ConvertDateTimeFromServerForMobileTech(Convert.ToDateTime(dr[dc.Ordinal]));
                            }
                        }
                    }
                }
            //}
        }

    }
}
