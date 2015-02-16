using log4net;
using MTechServices.Models;
using MTechServices.Models.DAL;
using MTechServices.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace MTechServices.Controllers
{
    public class FaultCentersController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: /Faults
        public IHttpActionResult Get()
        {
            return Ok();
        }

        //GET: /FaultCenters/GetDeletedFaultCenters?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetDeletedFaultCenters(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedFaultCenters(lastSyncTime);
            else
            {
                logger.InfoFormat("GetDeletedFaultCenters => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }

        //GET: /FaultCenters/GetFaultCenters?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<FaultInfo> GetFaultCenters(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetFaultCenters(lastSyncTime);
            else
            {
                logger.InfoFormat("GetFaultCenters => " + Environment.NewLine + " Invalid Request");
                return new List<FaultInfo>();
            }
        }
    }
}
