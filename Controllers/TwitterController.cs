using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Connect.DNN.Modules.SkinControls.Services.Authentication.Twitter;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;
using DotNetNuke.Services.Localization;
using AuthenticationController = Connect.DNN.Modules.SkinControls.Services.Authentication.AuthenticationController;

namespace Connect.DNN.Modules.SkinControls.Controllers
{
    public class TwitterController : AuthenticationController
    {
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Call(int id, string mode, string returnurl)
        {
            SetReturnUrlCookie(returnurl);
            OAuthClient = new TwitterClient(id, ToMode(mode)) { CallbackUri = CallbackUri("Twitter", id, mode) };
            OAuthClient.CallbackUri = new Uri(OAuthClient.CallbackUri + "?state=Twitter");
            AuthorisationResult result = OAuthClient.Authorize();
            if (result == AuthorisationResult.Denied)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable,
                    Localization.GetString("PrivateConfirmationMessage"));
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Reply(int id, string mode)
        {
            OAuthClient = new TwitterClient(id, ToMode(mode)) { CallbackUri = CallbackUri("Twitter", id, mode) };
            bool shouldAuthorize = OAuthClient.IsCurrentService() && OAuthClient.HaveVerificationCode();
            if (ToMode(mode) == AuthMode.Login)
            {
                shouldAuthorize = shouldAuthorize || OAuthClient.IsCurrentUserAuthorized();
            }
            if (shouldAuthorize)
            {
                if (OAuthClient.Authorize() == AuthorisationResult.Authorized)
                {
                    OAuthClient.AuthenticateUser(OAuthClient.GetCurrentUser<TwitterUserData>(), PortalSettings, GetIpAddress(), AddCustomProperties, OnUserAuthenticated);
                    if (AuthResult.User == null && ToMode(mode) == AuthMode.Register)
                    {
                        var newUser = RegisterUser();
                        OAuthClient.AuthenticateUser(OAuthClient.GetCurrentUser<TwitterUserData>(), PortalSettings, GetIpAddress(), AddCustomProperties, OnUserAuthenticated);
                    }
                }
            }
            // redirect
            string returnurl = HttpUtility.UrlDecode(HttpContext.Current.Request.Cookies["returnurl"].Value);
            HttpContext.Current.Response.Redirect(returnurl);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        protected override void AddCustomProperties(System.Collections.Specialized.NameValueCollection properties)
        {
            base.AddCustomProperties(properties);

            properties.Add("Twitter", string.Format("http://twitter.com/{0}", OAuthClient.GetCurrentUser<TwitterUserData>().ScreenName));
        }

    }
}