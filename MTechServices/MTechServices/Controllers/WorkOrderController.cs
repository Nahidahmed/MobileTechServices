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
    public class WorkOrderController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AccountsController));
        private ConfigDAL _configDAL;
        private ConfigDAL configDAL
        {
            get { return _configDAL ?? (_configDAL = new ConfigDAL()); }
        }

        // GET: WorkOrder   
        public IHttpActionResult Get()
        {
            return Ok();
        }

        #region Download WorkOrdersSync
        //GET: /WorkOrder/GetWorkOrders?workerID=5&lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<WorkOrderDetails> GetWorkOrders(long workerID, DateTime lastSyncTime)
        {
            logger.InfoFormat("GetWorkOrders/lastSyncTime => " + Environment.NewLine + lastSyncTime.ToString());
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetWorkOrders(lastSyncTime, workerID, sessionId);
            else
            {
                logger.InfoFormat("GetWorkOrders => " + Environment.NewLine + " Invalid Request");
                return new List<WorkOrderDetails>();
            }
        }

        //GET: /WorkOrder/GetWorkOrdersDeleted?lastSyncTime=01-12-2014 11:04:24
        public IEnumerable<string> GetWorkOrdersDeleted(DateTime lastSyncTime)
        {
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            if (!string.IsNullOrEmpty(sessionId))
                return configDAL.GetDeletedWorkOrders(lastSyncTime);
            else
            {
                logger.InfoFormat("GetWorkOrdersDeleted => " + Environment.NewLine + " Invalid Request");
                return new List<string>();
            }
        }

        #endregion

        #region Upload WorkOrders
        [HttpPost]
        public HttpResponseMessage InsertUpdateWorkOrderDetails([FromBody]WorkOrderDetails workOrderDetails)
        {
            int retVal = 0;
            string sessionId = AuthHeader.Validate(HttpContext.Current.Request);
            SessionDAL sessionDAL = new SessionDAL();

            retVal = configDAL.verifyWorkOrderStateBeforeUpdating(workOrderDetails);

            if (retVal == 1){
                retVal = configDAL.InsertUpdateWorkOrderDetails(workOrderDetails, sessionDAL.GetUserBySession(sessionId, workOrderDetails.ApplicationName));
            }

            /*
             * If retVal = 0; then it is server error and record was not updated.
             * If retVal = 1; then it is successfully Updated to Server.
             * If retVal = 2; then it is Primary worker is changed.
             * If retVal = 3; then it is workorder voided.
             * If retVal = 4; then it is workorder closed.
             */
          
            return Request.CreateResponse(HttpStatusCode.OK, retVal);
            

            /*
            HttpStatusCode httpStatusCode = new HttpStatusCode();
            if (retVal == 0){
                httpStatusCode = HttpStatusCode.NotModified;
            }
            else if (retVal == 1){
                httpStatusCode = HttpStatusCode.OK;
            }else if(retVal == 2){
                httpStatusCode = HttpStatusCode.Conflict;
            }

            return Request.CreateResponse(httpStatusCode, retVal);
             */
        }

        //[HttpPost]
        //public bool TestPut([FromBody]string strName)
        //{
        //    return true;
        //}
        #endregion
    }
}
