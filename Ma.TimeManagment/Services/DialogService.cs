using System;
using System.Windows;
using Ma.TimeManagement.ViewModels;
using Ma.TimeManagement.Views;

namespace Ma.TimeManagement.Services
{
    public class DialogService : IDialogService
    {
        public bool? ShowDialog(object viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

            // Map viewmodels to dialogs
            if (viewModel is TaskSelectionViewModel)
            {
                var dlg = new TaskSelectionDialog
                {
                    Owner = Application.Current?.MainWindow,
                    DataContext = viewModel
                };
                return dlg.ShowDialog();
            }

            throw new InvalidOperationException($"No dialog mapping for view model type: {viewModel.GetType()}");
        }
    }
}