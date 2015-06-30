using System.IO;
using DotNetNuke.Entities.Portals;

namespace Connect.DNN.Modules.SkinControls.Razor
{
    public class RazorControl
    {
        public string RazorFile { get; set; }
        public string LocalResourceFile { get; set; }
        public PortalSettings Context { get; set; }

        public RazorControl(PortalSettings context, string razorFile, string localResourceFile)
        {
            RazorFile = razorFile;
            LocalResourceFile = localResourceFile;
            Context = context;
        }

        private RazorEngine _engine;
        public RazorEngine Engine
        {
            get { return _engine ?? (_engine = new RazorEngine(RazorFile, null, Context, LocalResourceFile)); }
        }

        public string RenderObject<T>(T model)
        {
            using (StringWriter tw = new StringWriter())
            {
                Engine.Render(tw, model);
                return tw.ToString();
            }
        }
    }
}