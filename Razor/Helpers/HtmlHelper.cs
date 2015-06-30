using System.Web;
using DotNetNuke.Services.Localization;

namespace Connect.DNN.Modules.SkinControls.Razor.Helpers
{
    public class HtmlHelper
    {
        private readonly string _resourceFile;

        public HtmlHelper(string resourcefile)
        {
            _resourceFile = resourcefile;
        }

        public object GetLocalizedString(string key)
        {
            return Localization.GetString(key, _resourceFile);
        }

        public object GetLocalizedString(string key, string culture)
        {
            return Localization.GetString(key, _resourceFile, culture);
        }

        public HtmlString Raw(string text)
        {
            return new HtmlString(text);
        }
    }
}