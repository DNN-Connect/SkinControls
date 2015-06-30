﻿using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Connect.DNN.Modules.SkinControls.Services.Authentication.Facebook;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;
using DotNetNuke.Services.Localization;
using AuthenticationController = Connect.DNN.Modules.SkinControls.Services.Authentication.AuthenticationController;

namespace Connect.DNN.Modules.SkinControls.Controllers
{
    public class FacebookController : AuthenticationController
    {
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Call(int id, string mode, string returnurl)
        {
            SetReturnUrlCookie(returnurl);
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
                    OAuthClient.AuthenticateUser(OAuthClient.GetCurrentUser<FacebookUserData>(), PortalSettings, GetIpAddress(), AddCustomProperties, OnUserAuthenticated);
                    if (AuthResult.User == null && ToMode(mode) == AuthMode.Register)
                    {
                        var newUser = RegisterUser();
                        OAuthClient.AuthenticateUser(OAuthClient.GetCurrentUser<FacebookUserData>(), PortalSettings, GetIpAddress(), AddCustomProperties, OnUserAuthenticated);
                    }
                }
            }
            // redirect
            string returnurl = HttpUtility.UrlDecode(HttpContext.Current.Request.Cookies["returnurl"].Value);
            HttpContext.Current.Response.Redirect(returnurl);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        protected override void AddCustomProperties(NameValueCollection properties)
        {
            base.AddCustomProperties(properties);

            properties.Add("Facebook", OAuthClient.GetCurrentUser<FacebookUserData>().Link.ToString());
        }
    }
}