using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Web.Api;
using AuthenticationController = Connect.DNN.Modules.SkinControls.Services.Authentication.AuthenticationController;

namespace Connect.DNN.Modules.SkinControls.Controllers
{
    public class DnnController : AuthenticationController
    {
        public class loginDTO
        {
            public int PortalId { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public bool sc { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public HttpResponseMessage Login(loginDTO postData)
        {
            UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
            string userName = new PortalSecurity().InputFilter(postData.Username,
                PortalSecurity.FilterFlag.NoScripting |
                PortalSecurity.FilterFlag.NoAngleBrackets |
                PortalSecurity.FilterFlag.NoMarkup);
            var objUser = UserController.ValidateUser(postData.PortalId, userName, postData.Password, "DNN", string.Empty, PortalSettings.PortalName, AuthenticationLoginBase.GetIPAddress(), ref loginStatus);
            switch (loginStatus)
            {
                case UserLoginStatus.LOGIN_SUCCESS:
                case UserLoginStatus.LOGIN_SUPERUSER:
                case UserLoginStatus.LOGIN_INSECUREADMINPASSWORD:
                case UserLoginStatus.LOGIN_INSECUREHOSTPASSWORD:
                    UserController.UserLogin(postData.PortalId, objUser, "", AuthenticationLoginBase.GetIPAddress(), postData.sc);
                    return Request.CreateResponse(HttpStatusCode.OK, loginStatus);
                default:
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, loginStatus);
            }
        }
    }
}