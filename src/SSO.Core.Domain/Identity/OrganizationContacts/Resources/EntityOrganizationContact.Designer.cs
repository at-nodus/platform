namespace SSO.Core.Domain.Identity.OrganizationContacts.Resources {
    using System;
    public sealed class EntityOrganizationContact {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;
        internal EntityOrganizationContact() { }
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    resourceMan = new global::System.Resources.ResourceManager("SSO.Core.Domain.Identity.OrganizationContacts.Resources.EntityOrganizationContact", typeof(EntityOrganizationContact).Assembly);
                }
                return resourceMan;
            }
        }
        public static global::System.Globalization.CultureInfo Culture {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }
        public static string OrganizationId {
            get { return ResourceManager.GetString("OrganizationId", resourceCulture); }
        }
        public static string Name {
            get { return ResourceManager.GetString("Name", resourceCulture); }
        }
        public static string Email {
            get { return ResourceManager.GetString("Email", resourceCulture); }
        }
        public static string Phone {
            get { return ResourceManager.GetString("Phone", resourceCulture); }
        }
        public static string Title {
            get { return ResourceManager.GetString("Title", resourceCulture); }
        }
        public static string IsPrimary {
            get { return ResourceManager.GetString("IsPrimary", resourceCulture); }
        }
    }
}
