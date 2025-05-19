using ModernWpf.Controls;
using memory.ViewModels;
using System;

namespace memory.Views
{
    /// <summary>
    /// AddGroupDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class AddGroupDialog : Window
    {
        public AddGroupDialog(string existingGroupName = null)
        {
            InitializeComponent();
            var viewModel = new AddGroupDialogViewModel(result =>
            {
                try
                {
                    DialogResult = result;
                }
                catch (InvalidOperationException)
                {
                    // Show() で表示されている場合は DialogResult を設定できないため無視
                }
                Close();
            }, existingGroupName);
            DataContext = viewModel;
        }

        // ViewModelから直接アクセスできるようにプロパティを追加
        public string GroupName => (DataContext as AddGroupDialogViewModel)?.GroupName;
    }
} 