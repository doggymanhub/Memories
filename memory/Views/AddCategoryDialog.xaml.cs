using ModernWpf.Controls;
using memory.ViewModels;
using System;

namespace memory.Views
{
    public partial class AddCategoryDialog : Window
    {
        public AddCategoryDialog(string existingName = null, string existingColor = null)
        {
            InitializeComponent();
            var viewModel = new AddCategoryDialogViewModel(
                result => 
                {
                    try { DialogResult = result; } 
                    catch (InvalidOperationException) { /* Show()で表示されている場合はDialogResultを設定できないため無視 */ }
                    Close();
                }, 
                existingName,
                existingColor
            );
            DataContext = viewModel;
        }

        public string CategoryName => (DataContext as AddCategoryDialogViewModel)?.CategoryName;
        public string CategoryColor => (DataContext as AddCategoryDialogViewModel)?.CategoryColor;
    }
} 