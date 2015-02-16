using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace MTechServices.Models
{
    public static class Resource
    {
        public const string iOSAPPVERSION = "5.5.114.0";
        public const string ANDROIDAPPVERSION = "1.0.0.0";
        public const string WSVERSION = "5.5.112.0";
    }

    public class DeviceLog
    {
        public string filename { get; set; }
        public byte[] data { get; set; }
    }
}