namespace CampusQ.MVP.Data
{

    public static class WebAppConfig
    {
        /// Base URL where CampusQ.Web is hosted. Update this to match the deployed environment
        /// (e.g. "https://campusq.example.edu" or "https://localhost:7278" for local testing).
        public static string BaseUrl { get; set; } = "https://localhost:7278";
    }
}
