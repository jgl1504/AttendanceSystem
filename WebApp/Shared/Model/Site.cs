namespace WebApp.Shared.Model
{

    public class Site : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        // Optional, for display only
        public string Address { get; set; } = string.Empty;

        // For future Google Maps / Waze links
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Store the raw map URL or plus code if you prefer
        public string? MapUrl { get; set; }

        // For admin to choose default map app later
        public string? PreferredMapApp { get; set; }  // "Google", "Waze", null

        public bool IsActive { get; set; } = true;
    }
}
