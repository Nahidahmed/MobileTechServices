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
    public class RequestCentersController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: /RequestCenters
        public IHttpActionResult Get()
        {
            return Ok();
        }

        //GET: /RequestCenters/GetDeletedRequestCenters?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetDeletedRequestCenters(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedRequestCenters(lastSyncTime);
            else
            {
                logger.InfoFormat("GetDeletedRequestCenters => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }

        //GET: /RequestCenters/GetRequestCenters?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<RequestInfo> GetRequestCenters(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetRequestCenters(lastSyncTime);
            else
            {
                logger.InfoFormat("GetResultCenters => " + Environment.NewLine + " Invalid Request");
                return new List<RequestInfo>();
            }
        }

    }
}
