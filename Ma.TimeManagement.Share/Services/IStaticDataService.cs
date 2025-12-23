namespace Ma.TimeManagement.Services
{
    public interface IStaticDataService
    {
        string PathApplicationData { get; set; }
        string PathConfiguration { get; set; }
        string PathFullDatabase { get; set; }
        string PathFullSettings { get; set; }
    }
}