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
    public class RequestCodesController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: api/RequestCodes
        public IHttpActionResult Get()
        {
            return Ok();
        }

        #region Download RequestCodesSync
        //GET: /RequestCodes/GetRequestCodes?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<SystemCodeInfo> GetRequestCodes(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetRequests(lastSyncTime);
            else
            {
                logger.InfoFormat("GetRequestCodes => " + Environment.NewLine + " Invalid Request");
                return new List<SystemCodeInfo>();
            }
        }

        //GET: /RequestCodes/GetRequestCodesDeleted?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetRequestCodesDeleted(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedRequestCodes(lastSyncTime);
            else
            {
                logger.InfoFormat("GetRequestCodesDeleted => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }
        #endregion
    }
}
