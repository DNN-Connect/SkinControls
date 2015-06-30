using System.Web.UI;

namespace Connect.DNN.Modules.SkinControls.Razor.Helpers
{
    public class CtlHelper
    {
        protected AttributeCollection ControlAttributes { get; set; }

        public CtlHelper(AttributeCollection attributes)
        {
            ControlAttributes = attributes;
        }

        public string Item(string attributeName)
        {
            if (ControlAttributes == null)
            {
                return "";
            }
            return ControlAttributes[attributeName];
        }

    }
}