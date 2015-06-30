
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;

namespace Connect.DNN.Modules.SkinControls.Razor.Helpers
{
    public class DnnHelper
    {
        private readonly PortalSettings _context;

        public DnnHelper(PortalSettings context)
        {
            _context = context;
        }

        public TabInfo Tab
        {
            get
            {
                return _context.ActiveTab;
            }
        }

        public PortalSettings Portal
        {
            get
            {
                return _context;
            }
        }

        public UserInfo User
        {
            get
            {
                return _context.UserInfo;
            }
        }
    }
}