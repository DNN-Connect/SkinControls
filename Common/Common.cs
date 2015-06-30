using System.Web;
using DotNetNuke.Common;

namespace Connect.DNN.Modules.SkinControls.Common
{
    public class Common
    {
        public static string ResolveUrl(string url, bool includeHost)
        {
            url = Globals.ResolveUrl(url);
            if (includeHost && !url.StartsWith("http"))
            {
                if (HttpContext.Current.Request.Url.Port == 80)
                {
                    url = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host,
                        url);
                }
                else
                {
                    url = string.Format("{0}://{1}:{2}{3}", HttpContext.Current.Request.HttpMethod, HttpContext.Current.Request.Url.Host,
                        HttpContext.Current.Request.Url.Port, url);
                }
            }
            return url;
        }
    }
}