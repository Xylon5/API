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
            spCtx.Load(spCtx.Web, w => w.ServerRelativeUrl);
            spCtx.ExecuteQuery();

            string siteAssetUrl = string.Format("{0}/SiteAssets", spCtx.Web.ServerRelativeUrl);

            List siteAssets = spCtx.Web.GetList(siteAssetUrl);
            spCtx.ExecuteQuery();

            FileCreationInformation stylesheet = new FileCreationInformation();
            stylesheet.Url = "alternate.css";
            stylesheet.Overwrite = true;
            stylesheet.Content = System.IO.File.ReadAllBytes(HostingEnvironment.MapPath("~/assets/css/alternate.css"));
            siteAssets.RootFolder.Files.Add(stylesheet);

            FileCreationInformation masterpage = new FileCreationInformation();
            masterpage.Url = "customMaster.master";
            masterpage.Overwrite = true;
            masterpage.Content = System.IO.File.ReadAllBytes(HostingEnvironment.MapPath("~/assets/masterpage/customMaster.master"));
            siteAssets.RootFolder.Files.Add(masterpage);
            
            spCtx.ExecuteQuery();

            spCtx.Web.AlternateCssUrl = siteAssetUrl + "/alternate.css";
            spCtx.Web.MasterUrl = siteAssetUrl + "/customMaster.master";

            //spCtx.Web.AllProperties.FieldValues.Add("custom master version", new Version(0, 0, 1));

            spCtx.Web.Update();
            spCtx.ExecuteQuery();

            return Ok();
        }

        [HttpDelete]
        public IHttpActionResult DeleteBranding(SPSite site)
        {
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
