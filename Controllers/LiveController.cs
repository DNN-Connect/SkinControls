﻿using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Connect.DNN.Modules.SkinControls.Services.Authentication.Live;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;
using DotNetNuke.Services.Localization;
using AuthenticationController = Connect.DNN.Modules.SkinControls.Services.Authentication.AuthenticationController;

namespace Connect.DNN.Modules.SkinControls.Controllers
{
    public class LiveController : AuthenticationController
    {
        public override string Service
        {
            get { return "Live"; }
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Call(int id, string mode, string returnurl, bool keep)
        {
            SetReturnUrlCookie(returnurl);
            OAuthClient = new LiveClient(id, ToMode(mode)) { CallbackUri = CallbackUri("Live", id, mode, keep) };
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
        public HttpResponseMessage Reply(int id, string mode, bool keep)
        {
            OAuthClient = new LiveClient(id, ToMode(mode)) { CallbackUri = CallbackUri("Live", id, mode, keep) };
            bool shouldAuthorize = OAuthClient.IsCurrentService() && OAuthClient.HaveVerificationCode();
            KeepLoggedIn = keep;
            if (ToMode(mode) == AuthMode.Login)
            {
                shouldAuthorize = shouldAuthorize || OAuthClient.IsCurrentUserAuthorized();
            }
            if (shouldAuthorize)
            {
                if (AuthResult.User == null && (ToMode(mode) == AuthMode.Register | mode.ToLower() == "mixed"))
                {
                    OAuthClient.AuthenticateUser(OAuthClient.GetCurrentUser<LiveUserData>(), PortalSettings, GetIpAddress(), AddCustomProperties, OnUserAuthenticated);
                    if (AuthResult.User == null && ToMode(mode) == AuthMode.Register)
                    {
                        var newUser = RegisterUser();
                        OAuthClient.AuthenticateUser(OAuthClient.GetCurrentUser<LiveUserData>(), PortalSettings, GetIpAddress(), AddCustomProperties, OnUserAuthenticated);
                    }
                }
            }
            // redirect
            string returnurl = HttpUtility.UrlDecode(HttpContext.Current.Request.Cookies["returnurl"].Value);
            HttpContext.Current.Response.Redirect(returnurl);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

    }
}