using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Connect.DNN.Modules.SkinControls.Services.Authentication.Google;
using Connect.DNN.Modules.SkinControls.Services.Authentication.Live;
using Connect.DNN.Modules.SkinControls.Services.Authentication.Twitter;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;

namespace Connect.DNN.Modules.SkinControls.Controllers
{
    public class AuthController : DnnApiController
    {

        public OAuthClientBase OAuthClient { get; set; }
        public UserAuthenticatedEventArgs AuthResult { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Google(int id, string mode)
        {
            OAuthClient = new GoogleClient(id, ToMode(mode)) { CallbackUri = CallbackUri("Google", id, mode) };
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
        public HttpResponseMessage Live(int id, string mode)
        {
            OAuthClient = new LiveClient(id, ToMode(mode)) { CallbackUri = CallbackUri("Live", id, mode) };
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
        public HttpResponseMessage Twitter(int id, string mode)
        {
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

        public Uri CallbackUri(string provider, int id, string mode)
        {
            return new Uri(string.Format("{0}{1}/Reply/{2}?mode={3}", ApiUrl(), provider, id, mode));
        }

        public string ApiUrl()
        {
            return Common.Common.ResolveUrl("~/DesktopModules/Connect/SkinControls/API/", true);
        }

        public AuthMode ToMode(string mode)
        {
            if (mode.ToLower() == "register")
            {
                return AuthMode.Register;
            }
            return AuthMode.Login;
        }

        protected virtual void OnUserAuthenticated(UserAuthenticatedEventArgs ea)
        {
            AuthResult = ea;
        }

        public static string GetIpAddress()
        {
            string ipAddress = Null.NullString;
            if (HttpContext.Current.Request.UserHostAddress != null)
            {
                ipAddress = HttpContext.Current.Request.UserHostAddress;
            }
            return ipAddress;
        }

    }
}