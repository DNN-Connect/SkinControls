using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;

namespace Connect.DNN.Modules.SkinControls.Services.Authentication
{
    public class AuthenticationController : DnnApiController
    {

        public OAuthClientBase OAuthClient { get; set; }
        public UserAuthenticatedEventArgs AuthResult { get; set; }

        public Uri CallbackUri(string provider, int id, string mode)
        {
            return new Uri(String.Format("{0}{1}/Reply/{2}?mode={3}", ApiUrl(), provider, id, mode));
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

        public static void SetReturnUrlCookie(string returnurl)
        {
            HttpContext.Current.Response.Cookies.Set(new HttpCookie("returnurl", returnurl)
            {
                Expires = DateTime.Now.AddMinutes(5),
                Path =
                    (!String.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/")
            });
        }

        protected virtual void AddCustomProperties(NameValueCollection properties)
        {
        }

        protected virtual void OnUserAuthenticated(UserAuthenticatedEventArgs ea)
        {
            AuthResult = ea;
            if (ea.User != null)
            {
                if (ea.User.Profile != null && ea.User.Profile.PreferredLocale != null)
                {
                    Localization.SetLanguage(ea.User.Profile.PreferredLocale);
                }
                UserController.UserLogin(PortalSettings.PortalId, ea.User, PortalSettings.PortalName, GetIpAddress(),
                    false);
            }
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

        public UserInfo RegisterUser()
        {
            NameValueCollection profileProperties = AuthResult.Profile;

            UserInfo userToRegister = new UserInfo();
            userToRegister.PortalID = PortalSettings.PortalId;

            foreach (string key in profileProperties)
            {
                switch (key)
                {
                    case "FirstName":
                        userToRegister.FirstName = profileProperties[key];
                        break;
                    case "LastName":
                        userToRegister.LastName = profileProperties[key];
                        break;
                    case "Email":
                        userToRegister.Email = profileProperties[key];
                        break;
                    case "DisplayName":
                        userToRegister.DisplayName = profileProperties[key];
                        break;
                    default:
                        userToRegister.Profile.SetProfileProperty(key, profileProperties[key]);
                        break;
                }
            }

            // we cannot add a user without an email address
            if (String.IsNullOrEmpty(userToRegister.Email))
            {
                return null;
            }

            userToRegister.Username = PortalSettings.Registration.UseEmailAsUserName ? userToRegister.Email : AuthResult.UserToken;

            // let's check if we already have a user with this email address
            int total = 0;
            var existingUsers = UserController.GetUsersByEmail(PortalSettings.PortalId, userToRegister.Email, 1, 1, ref total);
            if (existingUsers.Count > 0)
            {
                userToRegister = (UserInfo)existingUsers[0];
            }
            else
            {
                if (!String.IsNullOrEmpty(PortalSettings.Registration.DisplayNameFormat))
                {
                    userToRegister.UpdateDisplayName(PortalSettings.Registration.DisplayNameFormat);
                }
                userToRegister.Membership.Password = UserController.GeneratePassword();
                userToRegister.Membership.Approved = PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PublicRegistration;
                var CreateStatus = UserController.CreateUser(ref userToRegister);
                DataCache.ClearPortalCache(PortalSettings.PortalId, true);
                if (CreateStatus != UserCreateStatus.Success)
                {
                    throw new Exception(CreateStatus.ToString());
                }
                CompleteUserCreation(CreateStatus, userToRegister, true, true);
                if (PortalSettings.Registration.UseEmailAsUserName)
                {
                    UserController.ChangeUsername(userToRegister.UserID, userToRegister.Email);
                }
            }
            if (!String.IsNullOrEmpty(AuthResult.AuthenticationType))
            {
                //string token = Service + "-" + AuthResult.Id;
                string token = AuthResult.UserToken;
                DotNetNuke.Services.Authentication.AuthenticationController.AddUserAuthentication(userToRegister.UserID, AuthResult.AuthenticationType, token);
            }

            return userToRegister;

        }

        protected string CompleteUserCreation(UserCreateStatus createStatus, UserInfo newUser, bool notify, bool register)
        {
            var strMessage = "";
            if (register)
            {
                //send notification to portal administrator of new user registration
                //check the receive notification setting first, but if register type is Private, we will always send the notification email.
                //because the user need administrators to do the approve action so that he can continue use the website.
                if (PortalSettings.EnableRegisterNotification || PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PrivateRegistration)
                {
                    strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationAdmin, PortalSettings);
                    SendAdminNotification(newUser, PortalSettings);
                }

                var loginStatus = UserLoginStatus.LOGIN_FAILURE;

                //complete registration
                switch (PortalSettings.UserRegistration)
                {
                    case (int)Globals.PortalRegistrationType.PrivateRegistration:
                        strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationPrivate, PortalSettings);

                        //show a message that a portal administrator has to verify the user credentials
                        if (String.IsNullOrEmpty(strMessage))
                        {
                            strMessage += Localization.GetString("PrivateConfirmationMessage", Localization.SharedResourceFile);
                        }
                        break;
                    case (int)Globals.PortalRegistrationType.PublicRegistration:
                        Mail.SendMail(newUser, MessageType.UserRegistrationPublic, PortalSettings);
                        UserController.UserLogin(PortalSettings.PortalId, newUser.Username, newUser.Membership.Password, "", PortalSettings.PortalName, "", ref loginStatus, false);
                        break;
                    case (int)Globals.PortalRegistrationType.VerifiedRegistration:
                        Mail.SendMail(newUser, MessageType.UserRegistrationVerified, PortalSettings);
                        UserController.UserLogin(PortalSettings.PortalId, newUser.Username, newUser.Membership.Password, "", PortalSettings.PortalName, "", ref loginStatus, false);
                        break;
                }
                //store preferredlocale in cookie
                Localization.SetLanguage(newUser.Profile.PreferredLocale);
            }
            else
            {
                if (notify)
                {
                    //Send Notification to User
                    if (PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                    {
                        strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationVerified, PortalSettings);
                    }
                    else
                    {
                        strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationPublic, PortalSettings);
                    }
                }
            }

            return strMessage;
        }

        private void SendAdminNotification(UserInfo newUser, PortalSettings portalSettings)
        {
            var notificationType = newUser.Membership.Approved ? "NewUserRegistration" : "NewUnauthorizedUserRegistration";
            var locale = LocaleController.Instance.GetDefaultLocale(portalSettings.PortalId).Code;
            var notification = new Notification
            {
                NotificationTypeID = NotificationsController.Instance.GetNotificationType(notificationType).NotificationTypeId,
                IncludeDismissAction = newUser.Membership.Approved,
                SenderUserID = portalSettings.AdministratorId,
                Subject = GetNotificationSubject(locale, newUser, portalSettings),
                Body = GetNotificationBody(locale, newUser, portalSettings),
                Context = newUser.UserID.ToString(CultureInfo.InvariantCulture)
            };
            var adminrole = RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.AdministratorRoleId);
            var roles = new List<RoleInfo> { adminrole };
            NotificationsController.Instance.SendNotification(notification, portalSettings.PortalId, roles, new List<UserInfo>());
        }

        private string GetNotificationBody(string locale, UserInfo newUser, PortalSettings portalSettings)
        {
            const string text = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_BODY";
            return LocalizeNotificationText(text, locale, newUser, portalSettings);
        }

        private string LocalizeNotificationText(string text, string locale, UserInfo user, PortalSettings portalSettings)
        {
            //This method could need a custom ArrayList in future notification types. Currently it is null
            return Localization.GetSystemMessage(locale, portalSettings, text, user, Localization.GlobalResourceFile, null, "", portalSettings.AdministratorId);
        }

        private string GetNotificationSubject(string locale, UserInfo newUser, PortalSettings portalSettings)
        {
            const string text = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_SUBJECT";
            return LocalizeNotificationText(text, locale, newUser, portalSettings);
        }

        public static List<AuthenticationInfo> GetActiveAuthenticationProviders(int portalId)
        {
            return CBO.GetCachedObject<List<AuthenticationInfo>>(
        new CacheItemArgs("GetActiveAuthenticationProviders" + portalId, DataCache.AuthenticationServicesCacheTimeOut, DataCache.AuthenticationServicesCachePriority, portalId),
        GetActiveAuthenticationProvidersCb);
        }

        private static List<AuthenticationInfo> GetActiveAuthenticationProvidersCb(CacheItemArgs cacheItemArgs)
        {
            var enabled = new List<AuthenticationInfo>();
            foreach (AuthenticationInfo authService in DotNetNuke.Services.Authentication.AuthenticationController.GetEnabledAuthenticationServices())
            {
                if (authService.AuthenticationType == "DNN")
                {
                    if (AuthenticationConfig.GetConfig((int)cacheItemArgs.Params[0]).Enabled)
                    {
                        enabled.Add(authService);
                    }
                }
                else
                {
                    if (OAuthConfigBase.GetConfig(authService.AuthenticationType, (int)cacheItemArgs.Params[0]).Enabled)
                    {
                        enabled.Add(authService);
                    }
                }
            }
            return enabled;
        }

    }
}