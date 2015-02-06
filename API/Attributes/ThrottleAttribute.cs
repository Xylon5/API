using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Caching;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace API
{
    public class ThrottleAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// A unique name for this Throttle.
        /// </summary>
        /// <remarks>
        /// We'll be inserting a Cache record based on this name and client IP, e.g. "Name-192.168.0.1"
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// The number of allowed calls per minute
        /// </summary>
        public int CallsPerMinute { get; set; }

        /// <summary>
        /// A text message that will be sent to the client upon throttling.  You can include the token {n} to
        /// show this.Seconds in the message, e.g. "Wait {n} seconds before trying again".
        /// </summary>
        public string Message { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var key = string.Concat(Name, "-", GetClientIp(actionContext.Request));
            var allowExecute = false;

            if (HttpRuntime.Cache[key] != null)
            {
                int count = Convert.ToInt32(HttpRuntime.Cache[key]);
                HttpRuntime.Cache[key] = count++;

                if (count > this.CallsPerMinute) allowExecute = false;
                else allowExecute = true;
            }
            else
            {
                HttpRuntime.Cache.Add(key,
                    (int)1, // is this the smallest data we can have?
                    null, // no dependencies
                    DateTime.Now.AddSeconds(60), // absolute expiration
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Low,
                    null); // no callback

                allowExecute = true;
            }

            if (!allowExecute)
            {
                if (string.IsNullOrEmpty(Message))
                {
                    Message = "You may only perform {n} calls per minute.";
                }

                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Conflict,
                    Message.Replace("{n}", CallsPerMinute.ToString())
                );
            }
        }

        private string GetClientIp(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                RemoteEndpointMessageProperty prop;
                prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            else
            {
                return null;
            }
        }
    }
}