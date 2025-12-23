namespace Ma.TimeManagement.Services
{
    public interface ICurrentUserPatService
    {
        Task<string> GetPatAsync(CancellationToken cancellationToken); // throws if missing
    }
}
