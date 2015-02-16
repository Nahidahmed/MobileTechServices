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
    public class ResultCentersController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: /ResultCenters
        public IHttpActionResult Get()
        {
            return Ok();
        }

        //GET: /ResultCenters/GetDeletedResultCenters?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetDeletedResultCenters(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedResultCenters(lastSyncTime);
            else
            {
                logger.InfoFormat("GetDeletedResultCenters => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }

        //GET: /ResultCenters/GetResultCenters?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<ResultInfo> GetResultCenters(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetResultCenters(lastSyncTime);
            else
            {
                logger.InfoFormat("GetResultCenters => " + Environment.NewLine + " Invalid Request");
                return new List<ResultInfo>();
            }
        }
    }
}
