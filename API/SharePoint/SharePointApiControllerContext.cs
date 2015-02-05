using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.IdentityModel.S2S.Protocols.OAuth2;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SharePoint.Client;
using System;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Configuration;

namespace API
{
    public static class TokenHelperApiController
    {
        public static string GetContextTokenFromRequest(HttpRequestMessage request)        
        {
            string appContextToken = "appContextToken";            
            string token = request.GetCookie(appContextToken);
            if(String.IsNullOrEmpty(token))                
                return null;
            return token;
        }

    }

    /// <summary>
    /// Encapsulates all the information from SharePoint.
    /// </summary>
    public abstract class SharePointApiControllerContext
    {
        public const string SPHostUrlKey = "SPHostUrl";
        public const string SPAppWebUrlKey = "SPAppWebUrl";
        public const string SPLanguageKey = "SPLanguage";
        public const string SPClientTagKey = "SPClientTag";
        public const string SPProductNumberKey = "SPProductNumber";

        protected static readonly TimeSpan AccessTokenLifetimeTolerance = TimeSpan.FromMinutes(5.0);

        private readonly Uri spHostUrl;
        private readonly Uri spAppWebUrl;
        private readonly string spLanguage;
        private readonly string spClientTag;
        private readonly string spProductNumber;

        // <AccessTokenString, UtcExpiresOn>
        protected Tuple<string, DateTime> userAccessTokenForSPHost;
        protected Tuple<string, DateTime> userAccessTokenForSPAppWeb;
        protected Tuple<string, DateTime> appOnlyAccessTokenForSPHost;
        protected Tuple<string, DateTime> appOnlyAccessTokenForSPAppWeb;

        /// <summary>
        /// The SharePoint host url.
        /// </summary>
        public Uri SPHostUrl
        {
            get { return this.spHostUrl; }
        }

        /// <summary>
        /// The SharePoint app web url.
        /// </summary>
        public Uri SPAppWebUrl
        {
            get { return this.spAppWebUrl; }
        }

        /// <summary>
        /// The SharePoint language.
        /// </summary>
        public string SPLanguage
        {
            get { return this.spLanguage; }
        }

        /// <summary>
        /// The SharePoint client tag.
        /// </summary>
        public string SPClientTag
        {
            get { return this.spClientTag; }
        }

        /// <summary>
        /// The SharePoint product number.
        /// </summary>
        public string SPProductNumber
        {
            get { return this.spProductNumber; }
        }

        /// <summary>
        /// The user access token for the SharePoint host.
        /// </summary>
        public abstract string UserAccessTokenForSPHost
        {
            get;
        }

        /// <summary>
        /// The user access token for the SharePoint app web.
        /// </summary>
        public abstract string UserAccessTokenForSPAppWeb
        {
            get;
        }

        /// <summary>
        /// The app only access token for the SharePoint host.
        /// </summary>
        public abstract string AppOnlyAccessTokenForSPHost
        {
            get;
        }

        /// <summary>
        /// The app only access token for the SharePoint app web.
        /// </summary>
        public abstract string AppOnlyAccessTokenForSPAppWeb
        {
            get;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="spHostUrl">The SharePoint host url.</param>
        /// <param name="spAppWebUrl">The SharePoint app web url.</param>
        /// <param name="spLanguage">The SharePoint language.</param>
        /// <param name="spClientTag">The SharePoint client tag.</param>
        /// <param name="spProductNumber">The SharePoint product number.</param>
        protected SharePointApiControllerContext(Uri spHostUrl, Uri spAppWebUrl, string spLanguage, string spClientTag, string spProductNumber)
        {
            if (spHostUrl == null)
            {
                throw new ArgumentNullException("spHostUrl");
            }

            if (string.IsNullOrEmpty(spLanguage))
            {
                throw new ArgumentNullException("spLanguage");
            }

            if (string.IsNullOrEmpty(spClientTag))
            {
                throw new ArgumentNullException("spClientTag");
            }

            if (string.IsNullOrEmpty(spProductNumber))
            {
                throw new ArgumentNullException("spProductNumber");
            }

            this.spHostUrl = spHostUrl;
            this.spAppWebUrl = spAppWebUrl;
            this.spLanguage = spLanguage;
            this.spClientTag = spClientTag;
            this.spProductNumber = spProductNumber;
        }

        /// <summary>
        /// Creates a user ClientContext for the SharePoint host.
        /// </summary>
        /// <returns>A ClientContext instance.</returns>
        public ClientContext CreateUserClientContextForSPHost()
        {
            return CreateClientContext(this.SPHostUrl, this.UserAccessTokenForSPHost);
        }

        /// <summary>
        /// Creates a user ClientContext for the SharePoint app web.
        /// </summary>
        /// <returns>A ClientContext instance.</returns>
        public ClientContext CreateUserClientContextForSPAppWeb()
        {
            return CreateClientContext(this.SPAppWebUrl, this.UserAccessTokenForSPAppWeb);
        }

        /// <summary>
        /// Creates app only ClientContext for the SharePoint host.
        /// </summary>
        /// <returns>A ClientContext instance.</returns>
        public ClientContext CreateAppOnlyClientContextForSPHost()
        {
            return CreateClientContext(this.SPHostUrl, this.AppOnlyAccessTokenForSPHost);
        }

        /// <summary>
        /// Creates an app only ClientContext for the SharePoint app web.
        /// </summary>
        /// <returns>A ClientContext instance.</returns>
        public ClientContext CreateAppOnlyClientContextForSPAppWeb()
        {
            return CreateClientContext(this.SPAppWebUrl, this.AppOnlyAccessTokenForSPAppWeb);
        }

        /// <summary>
        /// Gets the database connection string from SharePoint for autohosted app.
        /// </summary>
        /// <returns>The database connection string. Returns <c>null</c> if the app is not autohosted or there is no database.</returns>
        public string GetDatabaseConnectionString()
        {
            string dbConnectionString = null;

            using (ClientContext clientContext = CreateAppOnlyClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    var result = AppInstance.RetrieveAppDatabaseConnectionString(clientContext);

                    clientContext.ExecuteQuery();

                    dbConnectionString = result.Value;
                }
            }

            if (dbConnectionString == null)
            {
                const string LocalDBInstanceForDebuggingKey = "LocalDBInstanceForDebugging";

                var dbConnectionStringSettings = WebConfigurationManager.ConnectionStrings[LocalDBInstanceForDebuggingKey];

                dbConnectionString = dbConnectionStringSettings != null ? dbConnectionStringSettings.ConnectionString : null;
            }

            return dbConnectionString;
        }

        /// <summary>
        /// Determines if the specified access token is valid.
        /// It considers an access token as not valid if it is null, or it has expired.
        /// </summary>
        /// <param name="accessToken">The access token to verify.</param>
        /// <returns>True if the access token is valid.</returns>
        protected static bool IsAccessTokenValid(Tuple<string, DateTime> accessToken)
        {
            return accessToken != null &&
                   !string.IsNullOrEmpty(accessToken.Item1) &&
                   accessToken.Item2 > DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a ClientContext with the specified SharePoint site url and the access token.
        /// </summary>
        /// <param name="spSiteUrl">The site url.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns>A ClientContext instance.</returns>
        private static ClientContext CreateClientContext(Uri spSiteUrl, string accessToken)
        {
            if (spSiteUrl != null && !string.IsNullOrEmpty(accessToken))
            {
                return TokenHelper.GetClientContextWithAccessToken(spSiteUrl.AbsoluteUri, accessToken);
            }

            return null;
        }
        /// <summary>
        /// Gets the SharePoint host url from QueryString of the specified HTTP request.
        /// </summary>
        /// <param name="httpRequest">The specified HTTP request.</param>
        /// <returns>The SharePoint host url. Returns <c>null</c> if the HTTP request doesn't contain the SharePoint host url.</returns>

        public static Uri GetSPHostUrl(HttpRequestMessage requestContext)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }

            var spHostQuerystring = requestContext.GetQueryString(SPHostUrlKey);

            string spHostUrlString = TokenHelper.EnsureTrailingSlash(spHostQuerystring);
            Uri spHostUrl;
            if (Uri.TryCreate(spHostUrlString, UriKind.Absolute, out spHostUrl) &&
                (spHostUrl.Scheme == Uri.UriSchemeHttp || spHostUrl.Scheme == Uri.UriSchemeHttps))
            {
                return spHostUrl;
            }

            return null;
        }
    }


    public enum ContextStatus
    {
        Ok,
        NotOk
    }

    /// <summary>
    /// Provides SharePointApiControllerContext instances.
    /// </summary>
    public abstract class SharePointApiControllerContextProvider
    {
        private static SharePointApiControllerContextProvider current;

        /// <summary>
        /// The current SharePointApiControllerContextProvider instance.
        /// </summary>
        public static SharePointApiControllerContextProvider Current
        {
            get { return SharePointApiControllerContextProvider.current; }
        }

        /// <summary>
        /// Initializes the default SharePointApiControllerContextProvider instance.
        /// </summary>
        static SharePointApiControllerContextProvider()
        {
            if (!TokenHelper.IsHighTrustApp())
            {
                SharePointApiControllerContextProvider.current = new SharePointApiControllerAcsContextProvider();
            }
            else
            {
                SharePointApiControllerContextProvider.current = new SharePointApiControllerHighTrustContextProvider();
            }
        }

        /// <summary>
        /// Registers the specified SharePointApiControllerContextProvider instance as current.
        /// It should be called by Application_Start() in Global.asax.
        /// </summary>
        /// <param name="provider">The SharePointApiControllerContextProvider to be set as current.</param>
        public static void Register(SharePointApiControllerContextProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            SharePointApiControllerContextProvider.current = provider;
        }

        public SharePointApiControllerContext CreateSharePointContext(HttpControllerContext httpControllerContext)
        {
            if (httpControllerContext == null)
            {
                throw new ArgumentNullException("httpControllerContext");
            }

            // SPHostUrl
            Uri spHostUrl = SharePointApiControllerContext.GetSPHostUrl(httpControllerContext.Request);
            if (spHostUrl == null)
            {
                return null;
            }

            // SPAppWebUrl            
            string spAppWebUrlString = TokenHelper.EnsureTrailingSlash(httpControllerContext.Request.GetQueryString(SharePointApiControllerContext.SPAppWebUrlKey));
            Uri spAppWebUrl;
            if (!Uri.TryCreate(spAppWebUrlString, UriKind.Absolute, out spAppWebUrl) ||
                !(spAppWebUrl.Scheme == Uri.UriSchemeHttp || spAppWebUrl.Scheme == Uri.UriSchemeHttps))
            {
                spAppWebUrl = null;
            }

            // SPLanguage            
            string spLanguage = httpControllerContext.Request.GetQueryString(SharePointApiControllerContext.SPLanguageKey);
            if (string.IsNullOrEmpty(spLanguage))
            {
                return null;
            }

            // SPClientTag
            string spClientTag = httpControllerContext.Request.GetQueryString(SharePointApiControllerContext.SPClientTagKey);
            if (string.IsNullOrEmpty(spClientTag))
            {
                return null;
            }

            // SPProductNumber
            string spProductNumber = httpControllerContext.Request.GetQueryString(SharePointApiControllerContext.SPProductNumberKey);
            if (string.IsNullOrEmpty(spProductNumber))
            {
                return null;
            }

            return CreateSharePointContext(spHostUrl, spAppWebUrl, spLanguage, spClientTag, spProductNumber, httpControllerContext);
        }        

        public SharePointApiControllerContext GetSharePointContext(HttpControllerContext httpControllerContextContext)
        {
            if (httpControllerContextContext == null)
            {
                throw new ArgumentNullException("httpControllerContextContext");
            }

            Uri spHostUrl = SharePointApiControllerContext.GetSPHostUrl(httpControllerContextContext.Request);
            if (spHostUrl == null)
            {
                return null;
            }

            SharePointApiControllerContext spContext = CreateSharePointContext(httpControllerContextContext);                            

            return spContext;
        }

        /// <summary>
        /// Creates a SharePointApiControllerContext instance.
        /// </summary>
        /// <param name="spHostUrl">The SharePoint host url.</param>
        /// <param name="spAppWebUrl">The SharePoint app web url.</param>
        /// <param name="spLanguage">The SharePoint language.</param>
        /// <param name="spClientTag">The SharePoint client tag.</param>
        /// <param name="spProductNumber">The SharePoint product number.</param>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns>The SharePointApiControllerContext instance. Returns <c>null</c> if errors occur.</returns>        
        protected abstract SharePointApiControllerContext CreateSharePointContext(Uri spHostUrl, Uri spAppWebUrl, string spLanguage, string spClientTag, string spProductNumber, HttpControllerContext httpControllerContext);        

        public static ContextStatus CheckContextStatus(HttpControllerContext controllerContext, out Uri redirectUrl)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            redirectUrl = null;

            if (SharePointApiControllerContextProvider.Current.GetSharePointContext(controllerContext) != null)
            {
                return ContextStatus.Ok;
            }

            const string SPHasRedirectedToSharePointKey = "SPHasRedirectedToSharePoint";

            if (!string.IsNullOrEmpty(controllerContext.Request.GetQueryString(SPHasRedirectedToSharePointKey)))
            {
                return ContextStatus.NotOk;
            }

            Uri spHostUrl = SharePointApiControllerContext.GetSPHostUrl(controllerContext.Request);

            if (spHostUrl == null)
            {
                return ContextStatus.NotOk;
            }

            if (StringComparer.OrdinalIgnoreCase.Equals(controllerContext.Request.Method, "POST"))
            {
                return ContextStatus.NotOk;
            }

            return ContextStatus.NotOk;           
        }
    }

    #region ACS

    /// <summary>
    /// Encapsulates all the information from SharePoint in ACS mode.
    /// </summary>
    public class SharePointApiControllerAcsContext : SharePointApiControllerContext
    {
        private readonly string contextToken;
        private readonly SharePointContextToken contextTokenObj;

        /// <summary>
        /// The context token.
        /// </summary>
        public string ContextToken
        {
            get { return this.contextTokenObj.ValidTo > DateTime.UtcNow ? this.contextToken : null; }
        }

        /// <summary>
        /// The context token's "CacheKey" claim.
        /// </summary>
        public string CacheKey
        {
            get { return this.contextTokenObj.ValidTo > DateTime.UtcNow ? this.contextTokenObj.CacheKey : null; }
        }

        /// <summary>
        /// The context token's "refreshtoken" claim.
        /// </summary>
        public string RefreshToken
        {
            get { return this.contextTokenObj.ValidTo > DateTime.UtcNow ? this.contextTokenObj.RefreshToken : null; }
        }

        public override string UserAccessTokenForSPHost
        {
            get
            {
                return GetAccessTokenString(ref this.userAccessTokenForSPHost,
                                            () => TokenHelper.GetAccessToken(this.contextTokenObj, this.SPHostUrl.Authority));
            }
        }

        public override string UserAccessTokenForSPAppWeb
        {
            get
            {
                if (this.SPAppWebUrl == null)
                {
                    return null;
                }

                return GetAccessTokenString(ref this.userAccessTokenForSPAppWeb,
                                            () => TokenHelper.GetAccessToken(this.contextTokenObj, this.SPAppWebUrl.Authority));
            }
        }

        public override string AppOnlyAccessTokenForSPHost
        {
            get
            {
                return GetAccessTokenString(ref this.appOnlyAccessTokenForSPHost,
                                            () => TokenHelper.GetAppOnlyAccessToken(TokenHelper.SharePointPrincipal, this.SPHostUrl.Authority, TokenHelper.GetRealmFromTargetUrl(this.SPHostUrl)));
            }
        }

        public override string AppOnlyAccessTokenForSPAppWeb
        {
            get
            {
                if (this.SPAppWebUrl == null)
                {
                    return null;
                }

                return GetAccessTokenString(ref this.appOnlyAccessTokenForSPAppWeb,
                                            () => TokenHelper.GetAppOnlyAccessToken(TokenHelper.SharePointPrincipal, this.SPAppWebUrl.Authority, TokenHelper.GetRealmFromTargetUrl(this.SPAppWebUrl)));
            }
        }

        public SharePointApiControllerAcsContext(Uri spHostUrl, Uri spAppWebUrl, string spLanguage, string spClientTag, string spProductNumber, string contextToken, SharePointContextToken contextTokenObj)
            : base(spHostUrl, spAppWebUrl, spLanguage, spClientTag, spProductNumber)
        {
            if (string.IsNullOrEmpty(contextToken))
            {
                throw new ArgumentNullException("contextToken");
            }

            if (contextTokenObj == null)
            {
                throw new ArgumentNullException("contextTokenObj");
            }

            this.contextToken = contextToken;
            this.contextTokenObj = contextTokenObj;
        }

        /// <summary>
        /// Ensures the access token is valid and returns it.
        /// </summary>
        /// <param name="accessToken">The access token to verify.</param>
        /// <param name="tokenRenewalHandler">The token renewal handler.</param>
        /// <returns>The access token string.</returns>
        private static string GetAccessTokenString(ref Tuple<string, DateTime> accessToken, Func<OAuth2AccessTokenResponse> tokenRenewalHandler)
        {
            RenewAccessTokenIfNeeded(ref accessToken, tokenRenewalHandler);

            return IsAccessTokenValid(accessToken) ? accessToken.Item1 : null;
        }

        /// <summary>
        /// Renews the access token if it is not valid.
        /// </summary>
        /// <param name="accessToken">The access token to renew.</param>
        /// <param name="tokenRenewalHandler">The token renewal handler.</param>
        private static void RenewAccessTokenIfNeeded(ref Tuple<string, DateTime> accessToken, Func<OAuth2AccessTokenResponse> tokenRenewalHandler)
        {
            if (IsAccessTokenValid(accessToken))
            {
                return;
            }

            try
            {
                OAuth2AccessTokenResponse oAuth2AccessTokenResponse = tokenRenewalHandler();

                DateTime expiresOn = oAuth2AccessTokenResponse.ExpiresOn;

                if ((expiresOn - oAuth2AccessTokenResponse.NotBefore) > AccessTokenLifetimeTolerance)
                {
                    // Make the access token get renewed a bit earlier than the time when it expires
                    // so that the calls to SharePoint with it will have enough time to complete successfully.
                    expiresOn -= AccessTokenLifetimeTolerance;
                }

                accessToken = Tuple.Create(oAuth2AccessTokenResponse.AccessToken, expiresOn);
            }
            catch (WebException)
            {
            }
        }
    }

    /// <summary>
    /// Default provider for SharePointApiControllerAcsContext.
    /// </summary>
    public class SharePointApiControllerAcsContextProvider : SharePointApiControllerContextProvider
    {
        private const string SPContextKey = "SPContext";
        private const string SPCacheKeyKey = "SPCacheKey";        

        protected override SharePointApiControllerContext CreateSharePointContext(Uri spHostUrl, Uri spAppWebUrl, string spLanguage, string spClientTag,
            string spProductNumber, HttpControllerContext httpControllerContext)
        {
            string contextTokenString = TokenHelperApiController.GetContextTokenFromRequest(httpControllerContext.Request);            
            if (string.IsNullOrEmpty(contextTokenString))
            {
                return null;
            }

            SharePointContextToken contextToken = null;
            try
            {
                contextToken = TokenHelper.ReadAndValidateContextToken(contextTokenString, httpControllerContext.Request.RequestUri.Authority);
            }
            catch (WebException)
            {
                return null;
            }
            catch (AudienceUriValidationFailedException)
            {
                return null;
            }

            return new SharePointApiControllerAcsContext(spHostUrl, spAppWebUrl, spLanguage, spClientTag, spProductNumber, contextTokenString, contextToken);
        }        
    }

    #endregion ACS

    #region HighTrust

    /// <summary>
    /// Encapsulates all the information from SharePoint in HighTrust mode.
    /// </summary>
    public class SharePointApiControllerHighTrustContext : SharePointApiControllerContext
    {
        private readonly WindowsIdentity logonUserIdentity;

        /// <summary>
        /// The Windows identity for the current user.
        /// </summary>
        public WindowsIdentity LogonUserIdentity
        {
            get { return this.logonUserIdentity; }
        }

        public override string UserAccessTokenForSPHost
        {
            get
            {
                return GetAccessTokenString(ref this.userAccessTokenForSPHost,
                                            () => TokenHelper.GetS2SAccessTokenWithWindowsIdentity(this.SPHostUrl, this.LogonUserIdentity));
            }
        }

        public override string UserAccessTokenForSPAppWeb
        {
            get
            {
                if (this.SPAppWebUrl == null)
                {
                    return null;
                }

                return GetAccessTokenString(ref this.userAccessTokenForSPAppWeb,
                                            () => TokenHelper.GetS2SAccessTokenWithWindowsIdentity(this.SPAppWebUrl, this.LogonUserIdentity));
            }
        }

        public override string AppOnlyAccessTokenForSPHost
        {
            get
            {
                return GetAccessTokenString(ref this.appOnlyAccessTokenForSPHost,
                                            () => TokenHelper.GetS2SAccessTokenWithWindowsIdentity(this.SPHostUrl, null));
            }
        }

        public override string AppOnlyAccessTokenForSPAppWeb
        {
            get
            {
                if (this.SPAppWebUrl == null)
                {
                    return null;
                }

                return GetAccessTokenString(ref this.appOnlyAccessTokenForSPAppWeb,
                                            () => TokenHelper.GetS2SAccessTokenWithWindowsIdentity(this.SPAppWebUrl, null));
            }
        }

        public SharePointApiControllerHighTrustContext(Uri spHostUrl, Uri spAppWebUrl, string spLanguage, string spClientTag, string spProductNumber, WindowsIdentity logonUserIdentity)
            : base(spHostUrl, spAppWebUrl, spLanguage, spClientTag, spProductNumber)
        {
            if (logonUserIdentity == null)
            {
                throw new ArgumentNullException("logonUserIdentity");
            }

            this.logonUserIdentity = logonUserIdentity;
        }

        /// <summary>
        /// Ensures the access token is valid and returns it.
        /// </summary>
        /// <param name="accessToken">The access token to verify.</param>
        /// <param name="tokenRenewalHandler">The token renewal handler.</param>
        /// <returns>The access token string.</returns>
        private static string GetAccessTokenString(ref Tuple<string, DateTime> accessToken, Func<string> tokenRenewalHandler)
        {
            RenewAccessTokenIfNeeded(ref accessToken, tokenRenewalHandler);

            return IsAccessTokenValid(accessToken) ? accessToken.Item1 : null;
        }

        /// <summary>
        /// Renews the access token if it is not valid.
        /// </summary>
        /// <param name="accessToken">The access token to renew.</param>
        /// <param name="tokenRenewalHandler">The token renewal handler.</param>
        private static void RenewAccessTokenIfNeeded(ref Tuple<string, DateTime> accessToken, Func<string> tokenRenewalHandler)
        {
            if (IsAccessTokenValid(accessToken))
            {
                return;
            }

            DateTime expiresOn = DateTime.UtcNow.Add(TokenHelper.HighTrustAccessTokenLifetime);

            if (TokenHelper.HighTrustAccessTokenLifetime > AccessTokenLifetimeTolerance)
            {
                // Make the access token get renewed a bit earlier than the time when it expires
                // so that the calls to SharePoint with it will have enough time to complete successfully.
                expiresOn -= AccessTokenLifetimeTolerance;
            }

            accessToken = Tuple.Create(tokenRenewalHandler(), expiresOn);
        }
    }

    /// <summary>
    /// Default provider for SharePointApiControllerHighTrustContext.
    /// </summary>
    public class SharePointApiControllerHighTrustContextProvider : SharePointApiControllerContextProvider
    {
        private const string SPContextKey = "SPContext";

        protected override SharePointApiControllerContext CreateSharePointContext(Uri spHostUrl, Uri spAppWebUrl, string spLanguage, string spClientTag,
            string spProductNumber, HttpControllerContext httpControllerContext)
        {
            WindowsIdentity logonUserIdentity = (WindowsIdentity)httpControllerContext.RequestContext.Principal.Identity;
            if (logonUserIdentity == null || !logonUserIdentity.IsAuthenticated || logonUserIdentity.IsGuest || logonUserIdentity.User == null)
            {
                return null;
            }

            return new SharePointApiControllerHighTrustContext(spHostUrl, spAppWebUrl, spLanguage, spClientTag, spProductNumber, logonUserIdentity);
        }        
    }

    #endregion HighTrust
}
