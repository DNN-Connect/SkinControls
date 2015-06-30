using DotNetNuke.Web.Api;

namespace Connect.DNN.Modules.SkinControls.Common
{
    public class SkinControlsApiController : DnnApiController
    {
        private ContextSecurity _security;
        public ContextSecurity Security
        {
            get { return _security ?? (_security = new ContextSecurity(ActiveModule)); }
        }

    }
}