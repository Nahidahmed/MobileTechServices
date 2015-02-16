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
    public class OpenWorkStatusCodesController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: api/OpenWorkStatusCodes
        public IHttpActionResult Get()
        {
            return Ok();
        }

        #region Download OpenWorkStatusSync
        //GET: /OpenWorkStatusCodes/GetOpenWorkStatuses?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<OpenWorkStatusInfo> GetOpenWorkStatuses(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetOpenWorkStatuses(lastSyncTime);
            else
            {
                logger.InfoFormat("GetOpenWorkStatuses => " + Environment.NewLine + " Invalid Request");
                return new List<OpenWorkStatusInfo>();
            }
        }

        //GET: /OpenWorkStatusCodes/GetOpenWorkStatusesDeleted?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetOpenWorkStatusesDeleted(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedOpenWorkStatuses(lastSyncTime);
            else
            {
                logger.InfoFormat("GetOpenWorkStatusesDeleted => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }
        #endregion
    }
}
