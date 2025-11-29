using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Ma.TimeManagement.ViewModels;
using Ma.TimeManagement.Views;

namespace Ma.TimeManagement.Services
{
    public class DialogService : IDialogService
    {
        public async Task<bool?> ShowDialogAsync(object viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

            bool? result = false;

            await Task.Factory.StartNew(() =>
            {
                result = ShowDialog(viewModel);
            });
            return result;
        }
        public bool? ShowDialog(object viewModel)
        {
            bool? result = false;
            // Map viewmodels to dialogs
            if (viewModel is TaskSelectionViewModel)
            {
                var dlg = new TaskSelectionDialog
                {
                    Owner = Application.Current?.MainWindow,
                    DataContext = viewModel
                };
                result = dlg.ShowDialog();
                return result;
            }
            else if (viewModel is TaskDiscussionViewModel)
            {
                var dlg = new TaskDiscussionDialog
                {
                    Owner = Application.Current?.MainWindow,
                    DataContext = viewModel
                };
                result = dlg.ShowDialog();
                return result;
            }
            throw new InvalidOperationException($"No dialog mapping for view model type: {viewModel.GetType()}");
        }
    }
}