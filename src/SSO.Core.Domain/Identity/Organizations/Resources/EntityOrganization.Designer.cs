namespace SSO.Core.Domain.Identity.Organizations.Resources {
    using System;
    public sealed class EntityOrganization {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;
        internal EntityOrganization() { }
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    resourceMan = new global::System.Resources.ResourceManager("SSO.Core.Domain.Identity.Organizations.Resources.EntityOrganization", typeof(EntityOrganization).Assembly);
                }
                return resourceMan;
            }
        }
        public static global::System.Globalization.CultureInfo Culture {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }
        public static string Name { get { return ResourceManager.GetString("Name", resourceCulture); } }
        public static string Code { get { return ResourceManager.GetString("Code", resourceCulture); } }
        public static string LegalName { get { return ResourceManager.GetString("LegalName", resourceCulture); } }
        public static string TradeName { get { return ResourceManager.GetString("TradeName", resourceCulture); } }
        public static string TaxId { get { return ResourceManager.GetString("TaxId", resourceCulture); } }
        public static string Segment { get { return ResourceManager.GetString("Segment", resourceCulture); } }
        public static string Description { get { return ResourceManager.GetString("Description", resourceCulture); } }
        public static string PostalCode { get { return ResourceManager.GetString("PostalCode", resourceCulture); } }
        public static string Street { get { return ResourceManager.GetString("Street", resourceCulture); } }
        public static string Number { get { return ResourceManager.GetString("Number", resourceCulture); } }
        public static string Complement { get { return ResourceManager.GetString("Complement", resourceCulture); } }
        public static string City { get { return ResourceManager.GetString("City", resourceCulture); } }
        public static string State { get { return ResourceManager.GetString("State", resourceCulture); } }
    }
}
