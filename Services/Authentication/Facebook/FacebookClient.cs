using System;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;

namespace Connect.DNN.Modules.SkinControls.Services.Authentication.Facebook
{
    public class FacebookClient : OAuthClientBase
    {
        #region Constructors

        public FacebookClient(int portalId, AuthMode mode)
            : base(portalId, mode, "Facebook")
        {
            TokenEndpoint = new Uri("https://graph.facebook.com/oauth/access_token");
            TokenMethod = HttpMethod.GET;
            AuthorizationEndpoint = new Uri("https://graph.facebook.com/oauth/authorize");
            MeGraphEndpoint = new Uri("https://graph.facebook.com/me");

            Scope = "email";

            AuthTokenName = "FacebookUserToken";

            OAuthVersion = "2.0";

            LoadTokenCookie(String.Empty);
        }

        #endregion

        protected override TimeSpan GetExpiry(string responseText)
        {
            TimeSpan expiry = TimeSpan.MinValue;
            if (!String.IsNullOrEmpty(responseText))
            {
                foreach (string token in responseText.Split('&'))
                {
                    if (token.StartsWith("expires"))
                    {
                        expiry = new TimeSpan(0, 0, Convert.ToInt32(token.Replace("expires=", "")));
                    }
                }
            }
            return expiry;
        }

        protected override string GetToken(string responseText)
        {
            string authToken = String.Empty;
            if (!String.IsNullOrEmpty(responseText))
            {
                foreach (string token in responseText.Split('&'))
                {
                    if (token.StartsWith("access_token"))
                    {
                        authToken = token.Replace("access_token=", "");
                    }
                }
            }
            return authToken;
        }
    }
}