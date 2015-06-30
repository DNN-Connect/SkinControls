
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

        public string NavigateToPage()
        {
            return Globals.NavigateURL(_context.ActiveTab.TabID);
        }

        public string NavigateAction(string action)
        {
            return Globals.NavigateURL(_context.ActiveTab.TabID, action);
        }

        public string ApiUrl()
        {
            return Globals.ResolveUrl("~/DesktopModules/Connect/SkinControls/API/");
        }

    }
}