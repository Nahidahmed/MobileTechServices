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
    public class UrgencyCodesController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: api/UrgencyCodes
        public IHttpActionResult Get()
        {
            return Ok();
        }

        #region Download UrgencyCodesSync
        //GET: /UrgencyCodes/GetUrgencyCodes?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<UrgencyInfo> GetUrgencyCodes(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetUrgencyCodes(lastSyncTime);
            else
            {
                logger.InfoFormat("GetUrgencyCodes => " + Environment.NewLine + " Invalid Request");
                return new List<UrgencyInfo>();
            }
        }

        //GET: /UrgencyCodes/GetUrgencyCodesDeleted?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetUrgencyCodesDeleted(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedUrgency(lastSyncTime);
            else
            {
                logger.InfoFormat("GetUrgencyCodesDeleted => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }
        #endregion
    }
}
