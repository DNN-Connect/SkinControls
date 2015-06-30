using System;
using System.Runtime.Serialization;
using DotNetNuke.Services.Authentication.OAuth;

namespace Connect.DNN.Modules.SkinControls.Services.Authentication.Facebook
{
    [DataContract]
    public class FacebookUserData : UserData
    {
        #region Overrides

        public override string FirstName
        {
            get { return FacebookFirstName; }
            set { }
        }

        public override string LastName
        {
            get { return FacebookLastName; }
            set { }
        }

        #endregion

        [DataMember(Name = "birthday")]
        public string Birthday { get; set; }

        [DataMember(Name = "link")]
        public Uri Link { get; set; }

        [DataMember(Name = "first_name")]
        public string FacebookFirstName { get; set; }

        [DataMember(Name = "last_name")]
        public string FacebookLastName { get; set; }
    }
}