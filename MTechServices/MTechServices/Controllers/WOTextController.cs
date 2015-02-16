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
    public class WOTextController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: WOText
        public IHttpActionResult Get()
        {
            return Ok();
        }

        #region Download WOTextSync
        //GET: /WOText/GetWOTexts?workerID=5&lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<WorkOrderText> GetWOTexts(long workerID, DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetWOTexts(lastSyncTime, workerID, sessionId);
            else
            {
                logger.InfoFormat("GetWOTexts => " + Environment.NewLine + " Invalid Request");
                return new List<WorkOrderText>();
            }
        }

        //GET: /WOText/GetWOTextsDeleted?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetWOTextsDeleted(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedWOTexts(lastSyncTime);
            else
            {
                logger.InfoFormat("GetWOTextsDeleted => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }
        #endregion
    }
}
