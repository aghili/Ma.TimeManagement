using System.IO;

namespace Ma.TimeManagement.Services
{
    public class StaticDataService : IStaticDataService
    {
        public StaticDataService()
        {
            PathApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ma.TimeManagement");
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
