using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using API.Models;
using Microsoft.SharePoint.Client;

namespace API.Controllers
{
    public class SPSiteController : ApiController
    {
        static SynchronizedCollection<SPSite> sites = new SynchronizedCollection<SPSite>
        { 
            new SPSite { ID = new Guid("{94259729-D1D0-4C3B-86DB-932395EF8535}"), Title = "Central Administration",  Url= "http://vrmu009:8000", CanBeDeleted = false, Type= SPSiteType.Administration }, 
            new SPSite { ID = new Guid("{E35C227D-8ED4-4BB2-A753-D177999463CF}"), Title = "Intranet Root", Url= "http://intranet.vdrmu009.loc", CanBeDeleted = false, Type=SPSiteType.Collaboration }
        };

        /// <summary>
        /// Returns the site collection by id, or all sites
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("sites")]
        [HttpGet]
        [Throttle(Name="GetSites", CallsPerMinute=100)]
        public IHttpActionResult GetSite([FromBody]SPSite site)
        {
            List<SPSite> result = null;
            if (site != null)
            {
                if (site.ID != null && site.ID != Guid.Empty)
                {
                    result = sites.Where((p) => p.ID == site.ID).ToList();
                }
                else if (!string.IsNullOrEmpty(site.Title))
                {
                    result = sites.Where((p) => p.Title.Equals(site.Title, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
                else if (!string.IsNullOrEmpty(site.Url))
                {
                    result = sites.Where((p) => p.Url.Equals(site.Url, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
            }

            if (result == null)
            {
                result = sites.ToList();
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
        public IHttpActionResult PostSite([FromBody]SPSite site)
        {
            //var spCtx = new Microsoft.SharePoint.Client.ClientContext("http://intranet.vdrmu009.loc");
            //spCtx.AuthenticationMode = ClientAuthenticationMode.Default;
            //spCtx.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            //spCtx.ExecuteQuery();
            Guid id = Guid.NewGuid();
            string url = string.Format("http://intranet.vdrmu009.loc/sites/{0}", site.Url);
            //Microsoft.Online.SharePoint.TenantAdministration.Tenant tenant = new Microsoft.Online.SharePoint.TenantAdministration.Tenant(spCtx);
            //id = tenant.CreateSiteCollection(
            //    url,
            //    site.Title,
            //    "vdrmu009\\administrator",
            //    "STS#0",
            //    100,
            //    90,
            //    3,
            //    50,
            //    40,
            //    1031);

            //spCtx.ExecuteQuery();

            sites.Add(new SPSite() { ID = id, Title = site.Title, Url = url, CanBeDeleted = true, Type = site.Type });

            return Ok(id);
        }

        [Route("sites")]
        [HttpDelete]
        public IHttpActionResult DeleteSite([FromBody]SPSite site)
        {
            SPSite toDelete = null;
            if (site.ID != null && site.ID != Guid.Empty) toDelete = (from s in sites where s.ID == site.ID select s).FirstOrDefault();
            else if (!string.IsNullOrEmpty(site.Url)) toDelete = (from s in sites where s.Url.Equals(site.Url, StringComparison.InvariantCultureIgnoreCase) select s).FirstOrDefault();

            if (toDelete == null | toDelete == default(SPSite)) return this.InternalServerError(new Exception("The requested site could not be found. Maybe it was not created by this api!"));
            if (!toDelete.CanBeDeleted) return this.InternalServerError(new Exception("The requested site could be found but cannot be deleted due to internal protection!"));
            
            sites.Remove(toDelete);
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
