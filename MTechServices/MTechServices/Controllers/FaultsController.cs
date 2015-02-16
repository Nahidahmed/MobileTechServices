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
    public class FaultsController : ApiController
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

        //GET: /Faults/GetDeletedFaults?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetDeletedFaults(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedFaults(lastSyncTime);
            else
            {
                logger.InfoFormat("GetDeletedFaults => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }

        //GET: /Faults/GetFaults?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<SystemCode> GetFaults(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetFaults(lastSyncTime);
            else
            {
                logger.InfoFormat("GetFaults => " + Environment.NewLine + " Invalid Request");
                return new List<SystemCode>();
            }
        }


    }
}
