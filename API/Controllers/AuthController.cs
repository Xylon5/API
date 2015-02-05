using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API.Controllers
{
    public class AuthController : ApiController
    {
        [HttpGet]
        [Route("testauthentication")]
        public IHttpActionResult TestAutentication()
        {
            Debug.Write("AuthenticationType:" + User.Identity.AuthenticationType);
            Debug.Write("IsAuthenticated:" + User.Identity.IsAuthenticated);
            Debug.Write("Name:" + User.Identity.Name);

            if (User.Identity.IsAuthenticated)
            {
                return Ok("Authenticated: " + User.Identity.Name);
            }
            else
            {
                return BadRequest("Not authenticated");
            }
        }
    }
}
