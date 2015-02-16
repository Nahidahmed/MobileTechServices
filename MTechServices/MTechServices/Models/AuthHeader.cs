using System;
using System.Web;
using MTechServices.Models.DAL;

namespace MTechServices.Models
{
    public class AuthHeader {
        public static string Validate(HttpRequest request) {
            if (request == null) {
                throw new Exception("Missing HTTP Request");
            }

            string sessionId = string.Empty;
            sessionId = request.Headers.Get("sessionid");
            if (string.IsNullOrEmpty(sessionId)) {
                throw new Exception("Missing Authentication Header");
            }

            if (!new SessionDAL().IsActiveSession(sessionId)) {
                throw new Exception("Invalid Session");
            }

            return sessionId;
        }
    }
}
