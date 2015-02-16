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
    public class FilterController : ApiController
    { 
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: Filter   
        public IHttpActionResult Get()
        {
            return Ok();
        }

        public IEnumerable<FilterInfo> GetFilters()
        {
            
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            SessionDAL sessionDAL = new SessionDAL();
            User user = sessionDAL.GetUserBySession(sessionId, "mTech.iOS");
            logger.InfoFormat("GetFilters/secprimaryID => " + Environment.NewLine + user.ID.ToString());

            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetFilters(user.ID);
            else
            {
                logger.InfoFormat("GetFilters => " + Environment.NewLine + " Invalid Request");
                return new List<FilterInfo>();
            }
        }

        #region Upload UpdateMobileTechFilters
        [HttpPost]
        public HttpResponseMessage UpdateMobileTechFilters([FromBody]FilterInfo filterObj)
        {
            bool flag = false;
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            SessionDAL sessionDAL = new SessionDAL();
            User user = sessionDAL.GetUserBySession(sessionId, "mTech.iOS");
            filterObj.SECPrimaryId = user.ID;
            flag = configDAL.InsertUpdateMobileTechFilters(filterObj
                );
            if (flag)
                return Request.CreateResponse(HttpStatusCode.OK, flag);
            else
                return Request.CreateResponse(HttpStatusCode.NotModified, flag);
        }
        #endregion


    }
}
