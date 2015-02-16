using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Web.Http;

using MTechServices.Models;
using MTechServices.Models.DAL;
using MTechServices.Models.Entity;

namespace MTechServices.Controllers
{
    public class AuthenticationServiceController : ApiController
    {
        // GET: /AuthenticationService
        public IHttpActionResult Get()
        {
            return Ok();
        }
        
        [HttpGet]
        [ActionName("SignOn")]
        //Get: /AuthenticationService/SignOn?username=iaeuserDM&password=123&appName=iae
        public IHttpActionResult SignOn(string username, string password, string appName)
        {
            string signOnOutput = string.Empty;
            UserDAL userDAL = new UserDAL();

            if (appName.ToLower() == "iae")
            {
                signOnOutput = userDAL.UserAuthentication(username, password, appName);
            }
            else
            {
                signOnOutput = userDAL.BasicAuthentication(username, password, appName, 1);
            }

            return Ok(signOnOutput);
        }

        [HttpGet]
        [ActionName("SignOnWithVersion")]
        //Get: /AuthenticationService/SignOnWithVersion?username=iaeuserDM&password=123&appName=iae&
        public IHttpActionResult SignOnWithVersion(string username, string password, string appName, string app_AppVersion, string app_WSVersion)
        {
            string signOnOutput = string.Empty;
            UserDAL userDAL = new UserDAL();
            signOnOutput = userDAL.UserAuthentication(username, password, appName, app_AppVersion, app_WSVersion);
            return Ok(signOnOutput);
        }
        
        [Route("AuthenticationService/SignOff")]
        [AcceptVerbs("Get")]
        //Get: /AuthenticationService/SignOff
        public void SignOff()
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            SessionDAL sessionDAL = new SessionDAL();
            sessionDAL.EndSession(sessionId);
        }

        //[AcceptVerbs("Post")]
        [HttpPost]
        public void UploadExceptionLogFromDevice([FromBody]DeviceLog devLog)
        {
            if (devLog != null)
            {
                SessionDAL sessionDAL = new SessionDAL();
                sessionDAL.UploadExceptionLogFromDevice(devLog.filename, devLog.data);
            }
        }

        [HttpGet]
        [ActionName("GetCurrentUser")]
        public IHttpActionResult GetCurrentUser(string appName)
        {
            User usr = null;
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            UserDAL userDAL = new UserDAL();
            usr = userDAL.GetUserBySession(sessionId, appName);
            return Ok(usr);
        }


        /*
        // GET: api/AuthenticationService/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/AuthenticationService
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/AuthenticationService/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/AuthenticationService/5
        public void Delete(int id)
        {
        }
        */
    }
}
