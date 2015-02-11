using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Hosting;
using System.Web.Http;
using API.Models;
using Microsoft.SharePoint.Client;

namespace API.Controllers
{
    public class DesignController : ApiController
    {
        internal ClientContext GetContext(Website site)
        {
            var spCtx = new Microsoft.SharePoint.Client.ClientContext(site.Url);
            spCtx.AuthenticationMode = ClientAuthenticationMode.Default;
            spCtx.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            spCtx.ExecuteQuery();
            spCtx.Load(spCtx.Web, w => w.ServerRelativeUrl);
            spCtx.ExecuteQuery();
            return spCtx;
        }

        [HttpPut]
        public IHttpActionResult PutBranding(Website site)
        {
            using (ClientContext clientContext = new ClientContext(site.Url))
            {
                if (site.IsOffice365)
                {
                    //only relevant if SharePoint Online
                    var username = "dev@mkamsdn.onmicrosoft.com";
                    var password = "Start111!".ToSecureString();
                    var credentials = new SharePointOnlineCredentials(username, password);
                    clientContext.Credentials = credentials;
                }

                clientContext.Load(clientContext.Web);
                clientContext.Load(clientContext.Web, w => w.ServerRelativeUrl);
                clientContext.ExecuteQuery();

                clientContext.Web.DeployMasterPage(HostingEnvironment.MapPath("~/assets/masterpage/customMaster.master"), "Custom Master Page", "");
                clientContext.Web.RootFolder.UploadFile("alternate.css", HostingEnvironment.MapPath("~/assets/css/alternate.css"), true);

                string masterpageUrl = string.Format("{0}/_catalogs/masterpage/customMaster.master", clientContext.Web.ServerRelativeUrl);
                clientContext.Web.SetMasterPagesByUrl(masterpageUrl, masterpageUrl);
                clientContext.Web.AlternateCssUrl = string.Format("", clientContext.Web.ServerRelativeUrl);

                clientContext.ExecuteQuery();
            }

            return Ok();
        }

        [HttpDelete]
        public IHttpActionResult DeleteBranding(Website site)
        {
            //RemoveFiles(clientContext, branding);
            //RemoveMasterPages(clientContext, branding);
            //RemovePageLayouts(clientContext, branding);
            var siteUri = new Uri(site.Url);
            var spCtx = new Microsoft.SharePoint.Client.ClientContext(siteUri);
            spCtx.AuthenticationMode = ClientAuthenticationMode.Default;
            spCtx.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            spCtx.ExecuteQuery();

            spCtx.Load(spCtx.Web, w => w.ServerRelativeUrl);
            spCtx.ExecuteQuery();

            spCtx.Web.AlternateCssUrl = "";
            spCtx.Web.MasterUrl = spCtx.Web.ServerRelativeUrl + "/_catalogs/masterpage/seattle.master";
            spCtx.Web.Update();
            spCtx.ExecuteQuery();

            return Ok();
        }
    }
}
