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
    public class WorkersController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: api/Workers
        public IHttpActionResult Get()
        {
            return Ok();
        }

        //GET: /Workers/GetWorkersDeleted?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetWorkersDeleted(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedWorkers(lastSyncTime);
            else
            {
                logger.InfoFormat("GetWorkersDeleted => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }

        //GET: /Worers/GetOpenWorkStatuses?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<Worker> GetWorkers(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetWorkers(lastSyncTime, sessionId);
            else
            {
                logger.InfoFormat("GetWorkers => " + Environment.NewLine + " Invalid Request");
                return new List<Worker>();
            }
        }



    }
}
