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
    public class VersionDetailsController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        /*
        // GET: api/VersionDetails
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        */

        // GET: /VersionDetails
        public IEnumerable<SystemSetting> GetVersionDetails()
        {
            return configDAL.GetVersionDetails();
        }

        /*
        // GET: api/VersionDetails/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/VersionDetails
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/VersionDetails/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/VersionDetails/5
        public void Delete(int id)
        {
        }
        */
    }
}
