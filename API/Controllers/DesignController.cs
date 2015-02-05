using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Hosting;
using System.Web.Http;
using API.Models;
using Microsoft.SharePoint.Client;

namespace API.Controllers
{
    public class DesignController : ApiController
    {
        [HttpPut]
        public IHttpActionResult PutBranding(SPSite site)
        {
            var siteUri = new Uri(site.Url);
            var spCtx = new Microsoft.SharePoint.Client.ClientContext(siteUri);
            spCtx.AuthenticationMode = ClientAuthenticationMode.Default;
            spCtx.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            spCtx.ExecuteQuery();

            FileCreationInformation stylesheet = new FileCreationInformation();
            stylesheet.Url = "/netto.global.css";
            stylesheet.Overwrite = true;
            stylesheet.Content = System.IO.File.ReadAllBytes(HostingEnvironment.MapPath("~/assets/css/netto.global.css"));
            spCtx.Web.RootFolder.Files.Add(stylesheet);

            FileCreationInformation masterpage = new FileCreationInformation();
            masterpage.Url = "/nettocollaboration.master";
            masterpage.Overwrite = true;
            masterpage.Content = System.IO.File.ReadAllBytes(HostingEnvironment.MapPath("~/assets/masterpage/nettocollaboration.master"));
            spCtx.Web.RootFolder.Files.Add(masterpage);

            spCtx.ExecuteQuery();

            spCtx.Web.AlternateCssUrl = "/netto.global.css";
            spCtx.Web.MasterUrl = "/nettocollaboration.master";

            spCtx.ExecuteQuery();

            return Ok();
        }
    }
}
