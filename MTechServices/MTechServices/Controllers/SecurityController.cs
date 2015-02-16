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
    public class SecurityController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }
               
        // GET: /Security
        public IHttpActionResult Get()
        {
            return Ok();
        }

        #region Download SecuritySync
        //GET: /Security/GetSecurities?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<SecuritiesSync> GetSecurities(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetSecurities(lastSyncTime);
            else
            {
                logger.InfoFormat("GetSecurities => " + Environment.NewLine + " Invalid Request");
                return new List<SecuritiesSync>();
            }
        }

        //GET: /Security/GetSecuritiesDeleted?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetSecuritiesDeleted(DateTime lastSyncTime)
        {   
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedSecurities(lastSyncTime);
            else
            {
                logger.InfoFormat("GetSecuritiesDeleted => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        } 
        #endregion

        /*
        // POST: api/Test
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Test/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Test/5
        public void Delete(int id)
        {
        }
        */
    }
}
