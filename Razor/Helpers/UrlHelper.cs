
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Common.Utilities;

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

        public string UserAvatarUrl(UserInfo userInfo, int size)
        {
            var url = string.Format(Globals.UserProfilePicRelativeUrl(false), userInfo.UserID, size, size);
            if (userInfo.Profile != null)
            {
                var photoProperty = userInfo.Profile.GetProperty("Photo");

                int photoFileId;
                if (photoProperty != null && int.TryParse(photoProperty.PropertyValue, out photoFileId))
                {
                    var photoFile = FileManager.Instance.GetFile(photoFileId);
                    if (photoFile != null)
                    {
                        return url + "&cdv=" + photoFile.LastModificationTime.Ticks;
                    }
                }
            }
            return url;
        }

        public int SearchTabId()
        {
            var searchTabId = _context.SearchTabId;
            if (searchTabId == Null.NullInteger)
            {
                ArrayList arrModules = ModuleController.Instance.GetModulesByDefinition(_context.PortalId, "Search Results");
                if (arrModules.Count > 1)
                {
                    foreach (ModuleInfo searchModule in arrModules)
                    {
                        if (searchModule.CultureCode == _context.CultureCode)
                        {
                            searchTabId = searchModule.TabID;
                        }
                    }
                }
                else if (arrModules.Count == 1)
                {
                    searchTabId = ((ModuleInfo)arrModules[0]).TabID;
                }
            }
            return searchTabId;
        }

        public string MessagesUrl(int userId)
        {
            return Globals.NavigateURL(GetMessageTabId(), "", string.Format("userId={0}", userId));
        }

        public string NotificationsUrl(int userId)
        {
            return Globals.NavigateURL(GetMessageTabId(), "", string.Format("userId={0}", userId), "view=notifications", "action=notifications");
        }

        public int GetMessageTabId()
        {
            var cacheKey = string.Format("MessageCenterTab:{0}:{1}", _context.PortalId, _context.CultureCode);
            var messageTabId = DataCache.GetCache<int>(cacheKey);
            if (messageTabId > 0)
                return messageTabId;

            //Find the Message Tab
            messageTabId = FindMessageTab();

            //save in cache
            //NOTE - This cache is not being cleared. There is no easy way to clear this, except Tools->Clear Cache
            DataCache.SetCache(cacheKey, messageTabId, TimeSpan.FromMinutes(20));

            return messageTabId;
        }

        private int FindMessageTab()
        {
            //On brand new install the new Message Center Module is on the child page of User Profile Page 
            //On Upgrade to 6.2.0, the Message Center module is on the User Profile Page
            var profileTab = TabController.Instance.GetTab(_context.UserTabId, _context.PortalId, false);
            if (profileTab != null)
            {
                var childTabs = TabController.Instance.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
                foreach (TabInfo tab in childTabs)
                {
                    foreach (KeyValuePair<int, ModuleInfo> kvp in ModuleController.Instance.GetTabModules(tab.TabID))
                    {
                        var module = kvp.Value;
                        if (module.DesktopModule.FriendlyName == "Message Center" && !module.IsDeleted)
                        {
                            return tab.TabID;
                        }
                    }
                }
            }

            //default to User Profile Page
            return _context.UserTabId;
        }

    }
}