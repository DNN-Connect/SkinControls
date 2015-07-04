using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;

namespace Connect.DNN.Modules.SkinControls.Services.Localization
{
    public class LocalizationController
    {
        public static Dictionary<string, Locale> GetLocales(PortalSettings portalSettings)
        {
            Dictionary<string, Locale> res = new Dictionary<string, Locale>();
            foreach (Locale loc in LocaleController.Instance.GetLocales(portalSettings.PortalId).Values)
            {
                if (portalSettings.ContentLocalizationEnabled)
                {
                    string defaultRoles = PortalController.GetPortalSetting(string.Format("DefaultTranslatorRoles-{0}", loc.Code), portalSettings.PortalId, "Administrators");
                    if (PortalSecurity.IsInRoles(portalSettings.AdministratorRoleName) || loc.IsPublished ||
                        PortalSecurity.IsInRoles(defaultRoles))
                    {
                    res.Add(loc.Code, loc);                    
                    }
                }
                else
                {
                    res.Add(loc.Code, loc);                    
                }
            }
            return res;
        }

        public static string NewUrl(PortalSettings portalSettings, string newLanguage)
        {
            var objSecurity = new PortalSecurity();
            var newLocale = LocaleController.Instance.GetLocale(newLanguage);

            //Ensure that the current ActiveTab is the culture of the new language
            var tabId = portalSettings.ActiveTab.TabID;
            var islocalized = false;

            var localizedTab = TabController.Instance.GetTabByCulture(tabId, portalSettings.PortalId, newLocale);
            if (localizedTab != null)
            {
                islocalized = true;
                if (localizedTab.IsDeleted || !TabPermissionController.CanViewPage(localizedTab))
                {
                    var localizedPortal = PortalController.Instance.GetPortal(portalSettings.PortalId, newLocale.Code);
                    tabId = localizedPortal.HomeTabId;
                }
                else
                {
                    var fullurl = string.Empty;
                    switch (localizedTab.TabType)
                    {
                        case TabType.Normal:
                            //normal tab
                            tabId = localizedTab.TabID;
                            break;
                        case TabType.Tab:
                            //alternate tab url                                
                            fullurl = TestableGlobals.Instance.NavigateURL(Convert.ToInt32(localizedTab.Url));
                            break;
                        case TabType.File:
                            //file url
                            fullurl = TestableGlobals.Instance.LinkClick(localizedTab.Url, localizedTab.TabID, Null.NullInteger);
                            break;
                        case TabType.Url:
                            //external url
                            fullurl = localizedTab.Url;
                            break;
                    }
                    if (!string.IsNullOrEmpty(fullurl))
                    {
                        return objSecurity.InputFilter(fullurl, PortalSecurity.FilterFlag.NoScripting);
                    }
                }
            }

            var rawQueryString = string.Empty;
            if (DotNetNuke.Entities.Host.Host.UseFriendlyUrls)
            {
                // Remove returnurl from query parameters to prevent that the language is changed back after the user has logged in
                // Example: Accessing protected page /de-de/Page1 redirects to /de-DE/Login?returnurl=%2f%2fde-de%2fPage1 and changing language to en-us on the login page
                // using the language links won't change the language in the returnurl parameter and the user will be redirected to the de-de version after logging in
                // Assumption: Loosing the returnurl information is better than confusing the user by switching the language back after the login
                var queryParams = HttpUtility.ParseQueryString(new Uri(string.Concat(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority), HttpContext.Current.Request.RawUrl)).Query);
                queryParams.Remove("returnurl");
                var queryString = queryParams.ToString();
                if (queryString.Length > 0) rawQueryString = string.Concat("?", queryString);
            }

            return
                objSecurity.InputFilter(
                    TestableGlobals.Instance.NavigateURL(tabId, portalSettings.ActiveTab.IsSuperTab, portalSettings, HttpContext.Current.Request.QueryString["ctl"], newLanguage, GetQsParams(portalSettings, newLocale.Code, islocalized)) +
                    rawQueryString,
                    PortalSecurity.FilterFlag.NoScripting);
        }

        private static string[] GetQsParams(PortalSettings portalSettings, string newLanguage, bool isLocalized)
        {
            string returnValue = "";
            NameValueCollection queryStringCollection = HttpContext.Current.Request.QueryString;
            var rawQueryStringCollection =
                HttpUtility.ParseQueryString(new Uri(HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.RawUrl).Query);

            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
            string[] arrKeys = queryStringCollection.AllKeys;

            for (int i = 0; i <= arrKeys.GetUpperBound(0); i++)
            {
                if (arrKeys[i] != null)
                {
                    switch (arrKeys[i].ToLowerInvariant())
                    {
                        case "tabid":
                        case "ctl":
                        case "language": //skip parameter
                            break;
                        case "mid":
                        case "moduleid": //start of patch (Manzoni Fausto) gemini 14205 
                            if (isLocalized)
                            {
                                string ModuleIdKey = arrKeys[i].ToLowerInvariant();
                                int moduleID;
                                int tabid;

                                int.TryParse(queryStringCollection[ModuleIdKey], out moduleID);
                                int.TryParse(queryStringCollection["tabid"], out tabid);
                                ModuleInfo localizedModule = ModuleController.Instance.GetModuleByCulture(moduleID, tabid, settings.PortalId, LocaleController.Instance.GetLocale(newLanguage));
                                if (localizedModule != null)
                                {
                                    if (!string.IsNullOrEmpty(returnValue))
                                    {
                                        returnValue += "&";
                                    }
                                    returnValue += ModuleIdKey + "=" + localizedModule.ModuleID;
                                }
                            }
                            break;
                        default:
                            if ((arrKeys[i].ToLowerInvariant() == "portalid") && portalSettings.ActiveTab.IsSuperTab)
                            {
                                //skip parameter
                                //navigateURL adds portalid to querystring if tab is superTab
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(rawQueryStringCollection.Get(arrKeys[i])))
                                {
                                    //skip parameter as it is part of a querystring param that has the following form
                                    // [friendlyURL]/?param=value
                                    // gemini 25516

                                    if (!DotNetNuke.Entities.Host.Host.UseFriendlyUrls)
                                    {
                                        if (!String.IsNullOrEmpty(returnValue))
                                        {
                                            returnValue += "&";
                                        }
                                        returnValue += arrKeys[i] + "=" + HttpUtility.UrlEncode(rawQueryStringCollection.Get(arrKeys[i]));
                                    }


                                }
                                // on localised pages most of the module parameters have no sense and generate duplicate urls for the same content
                                // because we are on a other tab with other modules (example : /en-US/news/articleid/1)
                                else //if (!isLocalized) -- this applies only when a portal "Localized Content" is enabled.
                                {
                                    string[] arrValues = queryStringCollection.GetValues(i);
                                    if (arrValues != null)
                                    {
                                        for (int j = 0; j <= arrValues.GetUpperBound(0); j++)
                                        {
                                            if (!String.IsNullOrEmpty(returnValue))
                                            {
                                                returnValue += "&";
                                            }
                                            var qsv = arrKeys[i];
                                            qsv = qsv.Replace("\"", "");
                                            qsv = qsv.Replace("'", "");
                                            returnValue += qsv + "=" + HttpUtility.UrlEncode(arrValues[j]);
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            if (!settings.ContentLocalizationEnabled && LocaleController.Instance.GetLocales(settings.PortalId).Count > 1 && !settings.EnableUrlLanguage)
            {
                //because useLanguageInUrl is false, navigateUrl won't add a language param, so we need to do that ourselves
                if (returnValue != "")
                {
                    returnValue += "&";
                }
                returnValue += "language=" + newLanguage.ToLower();
            }

            //return the new querystring as a string array
            return returnValue.Split('&');
        }
    }
}