using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Connect.DNN.Modules.SkinControls.Services.Authentication.Facebook;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;
using DotNetNuke.Services.Localization;

namespace Connect.DNN.Modules.SkinControls.Controllers
{
    public class FacebookController : AuthController
    {
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Call(int id, string mode, string returnurl)
        {
            HttpContext.Current.Response.Cookies.Set(new HttpCookie("returnurl", returnurl)
            {
                Expires = DateTime.Now.AddMinutes(5),
                Path = (!string.IsNullOrEmpty(DotNetNuke.Common.Globals.ApplicationPath) ? DotNetNuke.Common.Globals.ApplicationPath : "/")
            });
            OAuthClient = new FacebookClient(id, ToMode(mode)) { CallbackUri = CallbackUri("Facebook", id, mode) };
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
            OAuthClient = new FacebookClient(id, ToMode(mode)) { CallbackUri = CallbackUri("Facebook", id, mode) };
            bool shouldAuthorize = OAuthClient.IsCurrentService() && OAuthClient.HaveVerificationCode();
            if (ToMode(mode) == AuthMode.Login)
            {
                shouldAuthorize = shouldAuthorize || OAuthClient.IsCurrentUserAuthorized();
            }
            if (shouldAuthorize)
            {
                if (OAuthClient.Authorize() == AuthorisationResult.Authorized)
                {
                    OAuthClient.AuthenticateUser(OAuthClient.GetCurrentUser<FacebookUserData>(), PortalSettings, GetIpAddress(), AddFacebookProperties, OnUserAuthenticated);
                    if (AuthResult.User == null)
                    {
                        var newUser = RegisterUser();
                        OAuthClient.AuthenticateUser(OAuthClient.GetCurrentUser<FacebookUserData>(), PortalSettings, GetIpAddress(), AddFacebookProperties, OnUserAuthenticated);
                    }
                }
            }
            // redirect
            string returnurl = HttpUtility.UrlDecode(HttpContext.Current.Request.Cookies["returnurl"].Value);
            HttpContext.Current.Response.Redirect(returnurl);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        protected virtual void AddFacebookProperties(NameValueCollection properties)
        {
            properties.Add("Facebook", OAuthClient.GetCurrentUser<FacebookUserData>().Link.ToString());
        }

    }
}