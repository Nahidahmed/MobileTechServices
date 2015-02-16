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
    public class AccountsController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }
       
        // GET: api/Accounts6
        public IHttpActionResult Get()
        {
            return Ok();
        }

        #region Download AccountsSync
        [HttpGet]
        [ActionName("GetAccounts")]
        //GET: /Accounts/GetAccounts?workerID=5&lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<AccountSync> GetAccounts(long workerID, DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetAccounts(lastSyncTime, workerID);
            else
            {
                logger.InfoFormat("GetAccounts => " + Environment.NewLine + " Invalid Request");
                return new List<AccountSync>();
            }
        }


        [HttpGet]
        [ActionName("GetAccountsDeleted")]
        //GET: /Accounts/GetAccountsDeleted?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetAccountsDeleted(long workerID, DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedAccounts(lastSyncTime, workerID);
            else
            {
                logger.InfoFormat("GetAccountsDeleted => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }
        #endregion
    }
}
