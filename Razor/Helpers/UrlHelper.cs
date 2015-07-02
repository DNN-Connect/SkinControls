
using System;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;

namespace Connect.DNN.Modules.SkinControls.Razor.Helpers
{
    public class UrlHelper
    {
        private readonly PortalSettings _context;

        public UrlHelper(PortalSettings context)
        {
            _context = context;
        }

        public string PageUrl()
        {
            return Globals.NavigateURL(_context.ActiveTab.TabID);
        }

        public string EncodedPageUrl()
        {
            return HttpUtility.UrlEncode(Globals.NavigateURL(_context.ActiveTab.TabID));
        }

        public string NavigateAction(string action)
        {
            return Globals.NavigateURL(_context.ActiveTab.TabID, action);
        }

        public string NavigateUrl(int tabId, string queryString)
        {
            string res = DotNetNuke.Common.Globals.NavigateURL(tabId);
            res += res.IndexOf("?", StringComparison.Ordinal) > 0 ? "&" : "?";
            return res + queryString;
        }

        public string ApiUrl()
        {
            return Globals.ResolveUrl("~/DesktopModules/Connect/SkinControls/API/");
        }

    }
}