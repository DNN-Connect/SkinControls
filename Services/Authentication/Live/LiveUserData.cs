using System;
using System.Runtime.Serialization;
using DotNetNuke.Services.Authentication.OAuth;

namespace Connect.DNN.Modules.SkinControls.Services.Authentication.Live
{
    [DataContract]
    public class LiveUserData : UserData
    {
        #region Overrides

        public override string FirstName
        {
            get { return LiveFirstName; }
            set { }
        }

        public override string LastName
        {
            get { return LiveLastName; }
            set { }
        }

        #endregion

        [DataMember(Name = "link")]
        public Uri Link { get; set; }

        [DataMember(Name = "first_name")]
        public string LiveFirstName { get; set; }

        [DataMember(Name = "last_name")]
        public string LiveLastName { get; set; }
    }
}