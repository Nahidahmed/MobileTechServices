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
    public class TimeTypeController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: /TimeType
        public IHttpActionResult Get()
        {
            return Ok();
        }

        #region Download SecuritySync
        //GET: /TimeType/GetTimeTypes?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<SystemCode> GetTimeTypes(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetTimeTypes(lastSyncTime);
            else
            {
                logger.InfoFormat("GetTimeTypes => " + Environment.NewLine + " Invalid Request");
                return new List<SystemCode>();
            }
        }

        //GET: /TimeType/GetTimeTypesDeleted?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetDeletedTimeTypes(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedTimeTypes(lastSyncTime);
            else
            {
                logger.InfoFormat("GetDeletedTimeTypes => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }
        #endregion

    }
}
