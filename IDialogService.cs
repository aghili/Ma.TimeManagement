namespace Ma.TimeManagement.Services
{
    public interface IDialogService
    {
        // Shows a dialog for the given viewmodel. Returns true if the user accepted.
        bool? ShowDialog(object viewModel);
    }
}