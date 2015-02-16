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
    public class PDADBIDController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        public IEnumerable<PDADBID> GetPDADBID(string username)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDBIDs(username);
            else
            {
                logger.InfoFormat("GetPDADBID => " + Environment.NewLine + " Invalid Request");
                return new List<PDADBID>();
            }
        }

        #region Upload UpdatePDADBID
        [HttpPost]
        public HttpResponseMessage UpdatePDADBID([FromBody]PDADBID pdaDBIDObj)
        {
            bool flag = false;
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            SessionDAL sessionDAL = new SessionDAL();
            flag = configDAL.UpdatePDADBItems(pdaDBIDObj);
            if (flag)
                return Request.CreateResponse(HttpStatusCode.OK, flag);
            else
                return Request.CreateResponse(HttpStatusCode.NotModified, flag);
        }
        #endregion


    }
}
