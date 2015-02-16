using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;


namespace MTechServices.Models
{
    public class Utilities
    {
        public bool checkTimeZoneSettings()
        {
            bool AdjustForTimeZone = false;

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

                if (alTimeZones.Count == 2)
                {
                    AdjustForTimeZone = true;
                }

                return AdjustForTimeZone;
            }
        }

        public DateTime getTimeZoneSettings(DateTime lastUpdateTime)
        {
            //Writelog("B4: Conversion =" + lastUpdateTime);
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
                //return alTimeZones;
                //Writelog("alTimeZones.Count = " + alTimeZones.Count);
                bool AdjustForTimeZone = false;
                if (alTimeZones.Count == 2)
                {
                    AdjustForTimeZone = true;
                }

                if (AdjustForTimeZone)
                {
                    DateTime dt = (DateTime)SqlDateTime.MinValue;
                    if (lastUpdateTime > dt)
                    {
                        string ClientTimeZone = alTimeZones[1].ToString();
                        lastUpdateTime = TimeZoneCalc.ConvertDateTimeToServer(lastUpdateTime, ClientTimeZone);
                        //Writelog("After: Conversion =" + lastUpdateTime);
                    }
                }
                return lastUpdateTime;
            }
        }

        public  void Writelog(string e)
        {
            if (ConfigurationManager.AppSettings["Log"] == "DEBUG")
            {
                string path = HostingEnvironment.ApplicationPhysicalPath;
                if (Directory.Exists(path + "\\Logs")) { }
                else
                {
                    Directory.CreateDirectory(path + "\\Logs");
                }
                string folderPath = path + "\\Logs";

                //set up a filestream
                FileStream fs = new FileStream(folderPath + "\\" + "MTechServices_Upload.txt", FileMode.OpenOrCreate, FileAccess.Write);

                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine("-----------------------------------------------------------------------------------------------------------------------------");
                sw.WriteLine(DateTime.Now + " " + e);
                sw.Flush();
                sw.Close();
            }
        }

        public  string Remove(string allStuff, string whatToRemove)
        {
          StringBuilder returnString = new StringBuilder();
          string[] arr = allStuff.Split(',');

           foreach (var item in arr){
             if(!item.Equals(whatToRemove)){
                 returnString.Append(item);
                 returnString.Append(", ");
            }
           }
          return returnString.ToString().Trim().TrimEnd(',');
        }


    }
}