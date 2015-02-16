using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Web.Http;

using log4net;
using MTechServices.Models;
using MTechServices.Models.DAL;
using MTechServices.Models.Entity;

namespace MTechServices.Controllers
{
    public class ResultsController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: /Results
        public IHttpActionResult Get()
        {
            return Ok();
        }

        #region Download ResultsSync
        //GET: /Results/GetResults?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<SystemCode> GetResults(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetResults(lastSyncTime);
            else
            {
                logger.InfoFormat("GetResults => " + Environment.NewLine + " Invalid Request");
                return new List<SystemCode>();
            }
        }

        //GET: /Results/GetResultsDeleted?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetResultsDeleted(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedResults(lastSyncTime);
            else
            {
                logger.InfoFormat("GetResultsDeleted => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }
        #endregion
    }
}
