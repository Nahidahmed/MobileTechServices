using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;

using MTechServices.Models;
using MTechServices.Models.DAL;
using MTechServices.Models.Entity;

namespace MTechServices.Controllers
{
    public class TestController : ApiController
    {

        private SessionDAL sessionDAL;
        private SessionDAL SessionDAL
        {
            get { return sessionDAL ?? (sessionDAL = new SessionDAL()); }
        }

        /*
        private UserDAL userDAL;
        private UserDAL UserDAL
        {
            get { return userDAL ?? (userDAL = new UserDAL()); }
        }
         */
        //List<Sample> samp = new List<Sample>() { new Sample { val = 12, name = "prasad" }, new Sample { val = 13, name = "ganga" } };
        //List<Sample> samp = new List<Sample>() { new Sample() { 12, "prasad" }, new Sample() { 14, "ganga" }, new Sample() { 15, "praveen" } };
        /*
        [HttpGet]
        public IEnumerable<Sample> Get()
        {
            //Sample sampobj = new Sample();
            return samp;
        }
         */
        
        // GET: api/Test
        [HttpGet]
        public IEnumerable<User> Get()
        {
            UserDAL userDAL = new UserDAL();
            return userDAL.GetUsers();
        }
        
        [HttpGet]
        [ActionName("Method1")]
        public string method1(string par1, string par2, string par3)
        {
            return par1 + par2 + par3;
        }

        [HttpGet]
        [ActionName("Method2")]
        public string method2(string par1, string par2, string par3)
        {
            return par1 + par2 + par3;
        }

        /*
        //[HttpGet]
        //[ActionName("UploadExceptionLogFromDevice")]
        [AcceptVerbs("Post")]
       // [Route("AuthenticationServicen/UploadExceptionLogFromDevice")]
        public object UploadExceptionLogFromDevice([FromBody]DeviceLog devLog)
        {
            //SessionDAL sessionDAL = new SessionDAL();
            //sessionDAL.UploadExceptionLogFromDevice(filename, data);
            return devLog;
        }
        */

        [HttpPost]
        public HttpResponseMessage TestPost([FromBody]string strName)
        {
            string name;
            try {
                name = strName;
            }
            catch (Exception ex)
            {

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            return Request.CreateResponse(HttpStatusCode.OK, name);
            //turn name;
        }

        /*
        // GET: api/Test
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Test/5
        public string Get(int id)
        {
            return "value";
        }

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
