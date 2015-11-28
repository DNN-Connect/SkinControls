using System;
using System.IO;
using System.Web.UI;
using Connect.DNN.Modules.SkinControls.Razor;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

namespace Connect.DNN.Modules.SkinControls
{
    public class RazorWebControl : UserControlBase
    {
        public string ControlName { get; set; }
        public string ControlPath { get; set; }
        private string ControlSource { get; set; }

        public string LocalResourceFile
        {
            get
            {
                return string.IsNullOrEmpty(_localResourceFile) ? this.Parent.AppRelativeTemplateSourceDirectory + Localization.LocalResourceDirectory + "/SharedResources.resx" : _localResourceFile;
            }
            set { _localResourceFile = value; }
        }

        private RazorEngine _engine;
        private string _localResourceFile;

        public RazorEngine Engine
        {
            get { return _engine ?? (_engine = new RazorEngine(string.Format("~{0}{1}.cshtml", ControlPath, ControlSource), Attributes, PortalSettings, LocalResourceFile)); }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            try
            {
                if (!(string.IsNullOrEmpty(ControlName)))
                {
                    if (string.IsNullOrEmpty(ControlPath))
                    {
                        ControlPath = "/DesktopModules/Connect/SkinControls/Controls/";
                    }
                    if (ControlPath.ToLower() == "skin")
                    {
                        ControlPath = PortalSettings.ActiveTab.SkinPath + "Controls/";
                    }

                    ControlSource = ControlName;
                    var writer = new StringWriter();
                    switch (ControlName.ToLower())
                    {
                        case "user":
                            if (Request.IsAuthenticated)
                            {
                                ControlSource = "UserAuthenticated";
                                Engine.Render(writer, UserController.GetCurrentUserInfo());
                            }
                            else
                            {
                                ControlSource = "UserUnauthenticated";
                                Engine.Render(writer);
                            }
                            break;
                        case "login":
                            if (Request.IsAuthenticated)
                            {
                                ControlSource = "LoginAuthenticated";
                                Engine.Render(writer, UserController.GetCurrentUserInfo());
                            }
                            else
                            {
                                ControlSource = "LoginUnauthenticated";
                                Engine.Render(writer);
                            }
                            break;
                        default:
                            Engine.Render(writer);
                            break;
                    }
                    Controls.Add(new LiteralControl(writer.ToString()));
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

    }
}