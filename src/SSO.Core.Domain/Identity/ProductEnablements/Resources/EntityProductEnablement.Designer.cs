namespace SSO.Core.Domain.Identity.ProductEnablements.Resources {
    using System;
    public sealed class EntityProductEnablement {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;
        internal EntityProductEnablement() { }
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    resourceMan = new global::System.Resources.ResourceManager("SSO.Core.Domain.Identity.ProductEnablements.Resources.EntityProductEnablement", typeof(EntityProductEnablement).Assembly);
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
        public static string ProductId {
            get { return ResourceManager.GetString("ProductId", resourceCulture); }
        }
    }
}
