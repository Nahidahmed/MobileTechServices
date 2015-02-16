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
    public class FacilitiesController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: api/Facilities
        public IHttpActionResult Get()
        {
            return Ok();
        }

        //GET: /Facilities/GetFacilitiesDeleted?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetFacilitiesDeleted(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedFacilities(lastSyncTime);
            else
            {
                logger.InfoFormat("GetFacilitiesDeleted => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }

        //GET: /Facilities/GetFacilities?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<Facility> GetFacilities(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetFacilities(lastSyncTime, sessionId);
            else
            {
                logger.InfoFormat("GetFacilities => " + Environment.NewLine + " Invalid Request");
                return new List<Facility>();
            }
        }




    }
}
