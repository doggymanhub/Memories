using ModernWpf.Controls;
using memory.ViewModels;
using System;
using memory.Models; // MemoryItemのため

namespace memory.Views
{
    public partial class AddMemoryDialog : Window
    {
        // コンストラクタで編集対象のMemoryItemと、紐づくPersonのIDを受け取る
        public AddMemoryDialog(MemoryItem memoryToEdit = null, Guid personId = default)
        {
            InitializeComponent();
            var viewModel = new AddMemoryDialogViewModel(
                result => 
                {
                    try { DialogResult = result; } 
                    catch (InvalidOperationException) { /* Show()で表示されている場合はDialogResultを設定できないため無視 */ }
                    Close();
                },
                memoryToEdit,
                personId 
            );
            DataContext = viewModel;
        }

        // ViewModelから直接アクセスできるようにプロパティを追加
        public MemoryItem GetMemoryItem() => (DataContext as AddMemoryDialogViewModel)?.MemoryItem;
    }
} 