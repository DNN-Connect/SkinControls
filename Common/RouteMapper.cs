using DotNetNuke.Web.Api;

namespace Connect.DNN.Modules.SkinControls.Common
{
    public class RouteMapper : IServiceRouteMapper
    {

        #region IServiceRouteMapper
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Connect/SkinControls", "SkinControlsMap1", "{controller}/{action}", null, null, new[] { "Connect.DNN.Modules.SkinControls.Controllers" });
            mapRouteManager.MapHttpRoute("Connect/SkinControls", "SkinControlsMap2", "{controller}/{action}/{id}", null, new { id = "\\d*" }, new[] { "Connect.DNN.Modules.SkinControls.Controllers" });
        }
        #endregion

    }
}