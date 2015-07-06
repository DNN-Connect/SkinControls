using System;
using System.Globalization;
using System.IO;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Connect.DNN.Modules.SkinControls.Razor.Helpers;
using System.Web.WebPages;
using System.Web.Compilation;
using System.Runtime.CompilerServices;
using System.Web.UI;

namespace Connect.DNN.Modules.SkinControls.Razor
{
    public class RazorEngine
    {
        public RazorEngine(string razorScriptFile, AttributeCollection controlAttributes, PortalSettings portalContext, string localResourceFile)
        {
            RazorScriptFile = razorScriptFile;
            ControlAttributes = controlAttributes;
            PortalContext = portalContext;
            LocalResourceFile = localResourceFile ?? Path.Combine(Path.GetDirectoryName(razorScriptFile), Localization.LocalResourceDirectory, Path.GetFileName(razorScriptFile) + ".resx");

            try
            {
                InitWebpage();
            }
            catch (HttpParseException)
            {
                throw;
            }
            catch (HttpCompileException)
            {
                throw;
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        protected AttributeCollection ControlAttributes { get; set; }
        protected string RazorScriptFile { get; set; }
        protected PortalSettings PortalContext { get; set; }
        protected string LocalResourceFile { get; set; }
        public SkinControlWebPage Webpage { get; set; }

        protected HttpContextBase HttpContext
        {
            get { return new HttpContextWrapper(System.Web.HttpContext.Current); }
        }

        public Type RequestedModelType()
        {
            if (Webpage != null)
            {
                var webpageType = Webpage.GetType();
                if (webpageType.BaseType.IsGenericType)
                {
                    return webpageType.BaseType.GetGenericArguments()[0];
                }
            }
            return null;
        }

        public void Render<T>(TextWriter writer, T model)
        {
            try
            {
                if ((Webpage) is SkinControlWebPage<T>)
                {
                    var mv = (SkinControlWebPage<T>)Webpage;
                    mv.Model = model;
                }
                Webpage.ExecutePageHierarchy(new WebPageContext(HttpContext, Webpage, model), writer, Webpage);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        public void Render(TextWriter writer)
        {
            try
            {
                Webpage.ExecutePageHierarchy(new WebPageContext(HttpContext, Webpage, null), writer, Webpage);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        private object CreateWebPageInstance()
        {
            var compiledType = BuildManager.GetCompiledType(RazorScriptFile);
            object objectValue = null;
            if (((compiledType != null)))
            {
                objectValue = RuntimeHelpers.GetObjectValue(Activator.CreateInstance(compiledType));
            }
            return objectValue;
        }

        private void InitHelpers(SkinControlWebPage webPage)
        {
            webPage.Ctl = new CtlHelper(ControlAttributes);
            webPage.Dnn = new DnnHelper(PortalContext);
            webPage.Html = new HtmlHelper(LocalResourceFile);
            webPage.Url = new UrlHelper(PortalContext);
        }

        private void InitWebpage()
        {
            if (!string.IsNullOrEmpty(RazorScriptFile))
            {
                var objectValue = RuntimeHelpers.GetObjectValue(CreateWebPageInstance());
                if ((objectValue == null))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage found at '{0}' was not created.", new object[] { RazorScriptFile }));
                }
                Webpage = objectValue as SkinControlWebPage;
                if ((Webpage == null))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage at '{0}' must derive from SkinControlWebPage.", new object[] { RazorScriptFile }));
                }
                Webpage.Context = HttpContext;
                Webpage.VirtualPath = VirtualPathUtility.GetDirectory(RazorScriptFile);
                InitHelpers(Webpage);
            }
        }
    }
}