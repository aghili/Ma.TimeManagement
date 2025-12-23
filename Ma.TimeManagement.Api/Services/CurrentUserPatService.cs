namespace Ma.TimeManagement.Services
{
    public class CurrentUserPatService : ICurrentUserPatService
    {
        private readonly IUserService _userService;

        public CurrentUserPatService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<string> GetPatAsync(CancellationToken cancellationToken)
        {
            return await _userService.GetCurrentUserPatAsync(cancellationToken);
        }
    }
}
