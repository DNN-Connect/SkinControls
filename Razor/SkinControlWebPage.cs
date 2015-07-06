using Connect.DNN.Modules.SkinControls.Razor.Helpers;
using System.Web.WebPages;

namespace Connect.DNN.Modules.SkinControls.Razor
{
    public abstract class SkinControlWebPage : WebPageBase
    {
        #region Helpers

        protected internal CtlHelper Ctl { get; internal set; }

        protected internal DnnHelper Dnn { get; internal set; }

        protected internal HtmlHelper Html { get; internal set; }

        protected internal UrlHelper Url { get; internal set; }

        #endregion

        #region BaseClass Overrides

        protected override void ConfigurePage(WebPageBase parentPage)
        {
            base.ConfigurePage(parentPage);
            Context = parentPage.Context;

            SkinControlWebPage parent = (SkinControlWebPage)parentPage;
            Ctl = parent.Ctl;
            Dnn = parent.Dnn;
            Html = parent.Html;
            Url = parent.Url;

        }

        #endregion
    }

    public abstract class SkinControlWebPage<T> : SkinControlWebPage
    {
        #region BaseClass Overrides

        public override void ExecutePageHierarchy()
        {
            if (PageData.Count > 0)
            {
                Model = PageData[0];
            }
            base.ExecutePageHierarchy();
        }

        #endregion

        public T Model { get; set; }
    }
}