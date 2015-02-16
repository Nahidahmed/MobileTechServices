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
    public class AssetsController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: api/Assets
        public IHttpActionResult Get()
        {
            return Ok();
        }

        #region Download AssetsSync
        [HttpGet]
        [ActionName("GetAssets")]
        //GET: /Assets/GetAssets?workerID=5&lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<AssetDetails> GetAssets(long workerID, DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetAssets(lastSyncTime, workerID, sessionId);
            else
            {
                logger.InfoFormat("GetAssets => " + Environment.NewLine + " Invalid Request");
                return new List<AssetDetails>();
            }
        }

        [HttpGet]
        [ActionName("GetAssetsDeleted")]
        //GET: /Assets/GetAssetsDeleted?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetAssetsDeleted(long workerID, DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedAssets(lastSyncTime, workerID);
            else
            {
                logger.InfoFormat("GetAssetsDeleted => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }
        #endregion
    }
}
