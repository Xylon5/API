using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Configuration;
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
                    var username = WebConfigurationManager.AppSettings["UserName"];
                    var password = WebConfigurationManager.AppSettings["Password"].ToSecureString();
                    var credentials = new SharePointOnlineCredentials(username, password);
                    clientContext.Credentials = credentials;
                }

                Web web = clientContext.Web;
                // Let's first upload the contoso theme to host web, if it does not exist there
                //web.DeployThemeToWeb("SPC",
                //                HostingEnvironment.MapPath(string.Format("~/{0}", "Resources/Themes/SPC/SPCTheme.spcolor")),
                //                null,
                //                HostingEnvironment.MapPath(string.Format("~/{0}", "Resources/Themes/SPC/SPCbg.jpg")),
                //                string.Empty);

                string themeDisplayName = "IPI Theme";
                string themeName = "ipi_theme";
                bool hasBackgroundImage = false;
                bool hasFonts = false;

                string filePath = HostingEnvironment.MapPath(string.Format("~/assets/themes/{0}.spcolor", themeName));
                web.UploadThemeFile(filePath);

                filePath = HostingEnvironment.MapPath(string.Format("~/assets/themes/{0}.png", themeName));
                if (System.IO.File.Exists(filePath))
                {
                    hasBackgroundImage = true;
                    web.UploadThemeFile(filePath);
                }

                filePath = HostingEnvironment.MapPath(string.Format("~/assets/themes/{0}.spfont", themeName));
                if (System.IO.File.Exists(filePath))
                {
                    hasFonts = true;
                    web.UploadThemeFile(filePath);
                }

                web.CreateComposedLookByName(
                    themeDisplayName,
                    string.Format("{0}.spcolor", themeName),
                    hasFonts ? string.Format("{0}.spfont", themeName) : null,
                    hasBackgroundImage ? string.Format("{0}.png", themeName) : null,
                    null);

                // Setting the Contoos theme to host web
                //web.SetThemeToWeb("SPC");
                web.SetComposedLookByUrl(themeDisplayName, resetSubsitesToInherit: true);
                //web.SetComposedLookByUrl(themeDisplayName, resetSubsitesToInherit: true, rootWeb: clientContext.Site.RootWeb);

                //clientContext.Load(clientContext.Web);
                //clientContext.Load(clientContext.Web, w => w.ServerRelativeUrl);
                //clientContext.ExecuteQuery();

                //clientContext.Web.DeployMasterPage(HostingEnvironment.MapPath("~/assets/masterpage/customMaster.master"), "Custom Master Page", "");
                //clientContext.Web.RootFolder.UploadFile("alternate.css", HostingEnvironment.MapPath("~/assets/css/alternate.css"), true);

                //string masterpageUrl = string.Format("{0}/_catalogs/masterpage/customMaster.master", clientContext.Web.ServerRelativeUrl);
                //clientContext.Web.SetMasterPagesByUrl(masterpageUrl, masterpageUrl);
                //clientContext.Web.AlternateCssUrl = string.Format("", clientContext.Web.ServerRelativeUrl);

                //clientContext.ExecuteQuery();



                ///////// CUSTOM CSS //////////              
                filePath = HostingEnvironment.MapPath(string.Format("~/assets/css/{0}.css", themeName));
                List assetLibrary = web.GetListByUrl("SiteAssets");
                assetLibrary.RootFolder.UploadFileWebDav(System.IO.Path.GetFileName(filePath), filePath, true);

                //// Get the path to the file which we are about to deploy
                //string file = System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~/{0}", "CSS/contoso.css"));

                //// Use CSOM to uplaod the file in
                //FileCreationInformation newFile = new FileCreationInformation();
                //newFile.Content = System.IO.File.ReadAllBytes(file);
                //newFile.Url = "contoso.css";
                //newFile.Overwrite = true;
                //Microsoft.SharePoint.Client.File uploadFile = assetLibrary.RootFolder.Files.Add(newFile);
                //clientContext.Load(uploadFile);
                //clientContext.ExecuteQuery();

                // Now, apply a reference to the CSS URL via a custom action
                string actionName = string.Format("{0}CSSLink", themeName);

                // Clean up existing actions that we may have deployed
                var existingActions = web.UserCustomActions;
                clientContext.Load(existingActions);

                // Execute our uploads and initialize the existingActions collection
                clientContext.ExecuteQuery();

                var actions = existingActions.ToArray();
                // Clean up existing custom action with same name, if it exists
                foreach (var existingAction in actions)
                {
                    if (existingAction.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase))
                        existingAction.DeleteObject();
                }
                clientContext.ExecuteQuery();

                // Build a custom action to write a link to our new CSS file
                UserCustomAction cssAction = web.UserCustomActions.Add();
                cssAction.Location = "ScriptLink";
                cssAction.Sequence = 100;
                cssAction.ScriptBlock = string.Format("document.write('<link rel=\"stylesheet\" href=\"{0}/{1}\" />');", assetLibrary.RootFolder.ServerRelativeUrl, System.IO.Path.GetFileName(filePath));
                cssAction.Name = actionName;

                // Apply
                cssAction.Update();
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
