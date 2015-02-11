using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using API.Models;
using Microsoft.SharePoint.Client;
using Microsoft.Online.SharePoint;
using System.Security;
using OfficeDevPnP.Core.Utilities;

namespace API.Controllers
{
    public class SPSiteController : ApiController
    {
        private ApiDBContext dbCtx = new ApiDBContext();

        /// <summary>
        /// Returns the site collection by id, or all sites
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("sites")]
        [HttpGet]
        [Throttle(Name="GetSites", CallsPerMinute=100)]
        public IHttpActionResult GetSite([FromBody]Website site)
        {
            List<Website> result = null;
            if (site != null)
            {
                if (site.ID != null && site.ID != Guid.Empty)
                {
                    result = dbCtx.Websites.Where((p) => p.ID == site.ID).ToList();
                }
                else if (!string.IsNullOrEmpty(site.Title))
                {
                    result = dbCtx.Websites.Where((p) => p.Title.Equals(site.Title, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
                else if (!string.IsNullOrEmpty(site.Url))
                {
                    result = dbCtx.Websites.Where((p) => p.Url.Equals(site.Url, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
            }

            if (result == null)
            {
                result = dbCtx.Websites.ToList();
            }
            return Ok(result);
        }

        /// <summary>
        /// Creates a new site collection
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        [Route("sites")]
        [HttpPost]
        public IHttpActionResult PostSite([FromBody]Website site)
        {
            List<Url> baseUrls = new List<Url>();
            foreach (var item in dbCtx.Configurations)
            {
                baseUrls = item.Urls.Where(u => u.SiteCreationEnabled == true).ToList();
            }

            if (baseUrls.Count == 0) return this.InternalServerError(new Exception("No base url with site creation enabled could be found!"));
            var realBaseUrl = (from u in baseUrls where u.Path.Equals(site.BaseUrl, StringComparison.InvariantCultureIgnoreCase) select u).ToList();

            if (realBaseUrl.Count > 1) return this.InternalServerError(new Exception("Multiple matched base urls found! Please specify only one base url!"));

            // :)
            Url realRealBaseUrl = realBaseUrl.First();

            using (ClientContext ctx = new ClientContext(realRealBaseUrl.Path))
            {
                ICredentials credentials;
                if (realRealBaseUrl.IsOffice365)
                {
                    credentials = CredentialManager.GetSharePointOnlineCredential("rmumsdn");
                } else
                {
                    credentials = CredentialManager.GetCredential("vdrmu009");
                }

                if (credentials == null) return this.InternalServerError(new Exception("No valid credentials found!"));
                ctx.Credentials = credentials;
                
                Microsoft.Online.SharePoint.TenantAdministration.Tenant tenant = new Microsoft.Online.SharePoint.TenantAdministration.Tenant(ctx);
                site.ID = tenant.CreateSiteCollection(site.Url, site.Title, site.OwnerLogin,
                    site.Type == WebsiteType.Collaboration ? "STS#0" : "STS#0",
                    100, 90, 3, 50, 40, site.Locale);

                ctx.ExecuteQuery();
            }

            dbCtx.Websites.Add(site);
            dbCtx.SaveChangesAsync();

            return Ok(site.ID);
        }

        [Route("sites")]
        [HttpDelete]
        public IHttpActionResult DeleteSite([FromBody]Website site)
        {
            Website toDelete = null;
            if (site.ID != null && site.ID != Guid.Empty) toDelete = (from s in dbCtx.Websites where s.ID == site.ID select s).FirstOrDefault();
            else if (!string.IsNullOrEmpty(site.Url)) toDelete = (from s in dbCtx.Websites where s.Url.Equals(site.Url, StringComparison.InvariantCultureIgnoreCase) select s).FirstOrDefault();

            if (toDelete == null | toDelete == default(Website)) return this.InternalServerError(new Exception("The requested site could not be found. Maybe it was not created by this api!"));
            if (!toDelete.CanBeDeleted) return this.InternalServerError(new Exception("The requested site could be found but cannot be deleted due to internal protection!"));

            dbCtx.Websites.Remove(toDelete);
            return Ok();
        }

        [Route("sites")]
        [HttpPatch]
        public IHttpActionResult UpdateSite([FromBody]UpdateSiteParam parameters)
        {
            string options = string.Format("All: {0} | Pilot: {1} | Single: {2} | ID: {3}", parameters.All, parameters.Pilot, parameters.Single, parameters.Id);
            return Ok(options);
        }

    }
}
