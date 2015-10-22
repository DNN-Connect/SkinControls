# DNN Connect Skin Controls

This is a suite of controls that allow you to create some powerful functionality in a skin. 
Central is the use of Razor files. These are loaded as a control into the ascx of your skin 
as follows:

``` asp
<connect:RazorWebControl runat="server" id="ctlLogin" ControlName="Login" ControlPath="Skin" />
```

The ControlName can be either User or Login for now. These then set the property
ControlSource which points to the actual name of the file. If you choose Login then
ControlSource will be either LoginAuthenticated or LoginUnauthenticated. Similarly
for ControlName="User" the ControlSource with be either UserAuthenticated or UserUnauthenticated.
But you can specify your own ControlSource directly of course. It's just that Login and User
have the ability to switch between the state of the user being authenticated or not.

The ControlPath tells it whether it will find the file in the system (DesktopModules) path 
or in the skin folder (under the folder "Controls"). The latter is the preferred method
and keeps everything with the skin.

``` csharp
if (string.IsNullOrEmpty(ControlPath))
{
    ControlPath = "/DesktopModules/Connect/SkinControls/Controls/";
}
if (ControlPath.ToLower() == "skin")
{
    ControlPath = PortalSettings.ActiveTab.SkinPath + "Controls/";
}
```

The ControlSource eventually leads to the Razor file which is expected to be a cshtml.

``` csharp
public RazorEngine Engine
{
    get { return _engine ?? (_engine = new RazorEngine(string.Format("~{0}{1}.cshtml", ControlPath, ControlSource), Attributes, PortalSettings, LocalResourceFile)); }
}
```

The Razor files inherit from Connect.DNN.Modules.SkinControls.Razor.SkinControlWebPage. 
Besides the usual suspects Dnn, Html and Url this also includes a property Ctl which gives access
to the encapsulating control. This way you can pass values through attributes to the Razor file
like this:

``` asp
  <connect:RazorWebControl runat="server" id="ctlBreadCrumbs" ControlName="BreadCrumbs" ControlPath="Skin" RootLevel="0" />
```

And in the Razor file:

``` csharp
 int intRootLevel = 0;
 if (Ctl.Item("RootLevel") != null)
 {
  Int32.TryParse(Ctl.Item("RootLevel"), out intRootLevel);
 }
```
