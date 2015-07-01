using System;
using System.Web.UI;
using ClientDependency.Core.Controls;

namespace Connect.DNN.Modules.SkinControls
{
    public class DefaultCssExclude : Control
    {
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var filePath = string.Concat(DotNetNuke.Common.Globals.HostPath, "default.css");
            var loader = Page.FindControl("ClientResourceIncludes");
            if (loader != null)
            {
                ClientDependencyInclude ctlToRemove = null;
                foreach (ClientDependencyInclude ctl in loader.Controls)
                {
                    if (ctl.FilePath == filePath)
                    {
                        ctlToRemove = ctl;
                        break;
                    }
                }
                if (ctlToRemove != null)
                {
                    loader.Controls.Remove(ctlToRemove);
                }
            }
        }
    }
}