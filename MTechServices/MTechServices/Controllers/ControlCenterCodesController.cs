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
    public class ControlCenterCodesController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: api/ControlCenterCodes
        public IHttpActionResult Get()
        {
            return Ok();
        }

        #region Download ControlCenterCodesSync
        //GET: /ControlCenterCodes/GetControlCenterCodes?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<SystemCode> GetControlCenterCodes(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetControlCenters(lastSyncTime);
            else
            {
                logger.InfoFormat("GetControlCenterCodes => " + Environment.NewLine + " Invalid Request");
                return new List<SystemCode>();
            }
        }

        //GET: /ControlCenterCodes/GetControlCenterCodesDeleted?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetControlCenterCodesDeleted(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedControlCenters(lastSyncTime);
            else
            {
                logger.InfoFormat("GetControlCenterCodesDeleted => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }
        #endregion
    }
}
