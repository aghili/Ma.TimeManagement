namespace Ma.TimeManagement.Services
{
    public interface IDialogService
    {
        // MVVM-friendly dialog service; returns true if accepted
        bool? ShowDialog(object viewModel);
        Task<bool?> ShowDialogAsync(object viewModel);
    }
}