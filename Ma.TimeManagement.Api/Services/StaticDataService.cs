using System.IO;

namespace Ma.TimeManagement.Services
{
    public class StaticDataService : IStaticDataService
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        public StaticDataService(IWebHostEnvironment  webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
            PathApplicationData = Path.Combine(webHostEnvironment.ContentRootPath, "APP_DATA");
            PathConfiguration = PathApplicationData;
            PathFullDatabase = Path.Combine(PathApplicationData, "database.db");
            PathFullSettings = Path.Combine(PathApplicationData, "appsettings.json");

            if (!Directory.Exists(PathApplicationData))
                Directory.CreateDirectory(PathApplicationData);
        }
        public string PathApplicationData { get; set; }
        public string PathConfiguration { get; set; }
        public string PathFullDatabase { get; set; }
        public string PathFullSettings { get; set; }
    }
}
