using ModernWpf.Controls;
using memory.ViewModels;
using System;
using memory.Models; // Personのため
using System.Linq; // Selectのため
using System.Collections.Generic; // Listのため

namespace memory.Views
{
    public partial class AddPersonDialog : Window
    {
        public AddPersonDialog(Person personToEdit = null, List<Category> allCategories = null)
        {
            InitializeComponent();
            var viewModel = new AddPersonDialogViewModel(
                result =>
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
                },
                personToEdit, // 編集対象のPersonオブジェクト
                allCategories // 利用可能な全カテゴリのリスト
            );
            DataContext = viewModel;
        }

        // ViewModelから直接アクセスできるようにプロパティを追加 (主に編集後のPersonオブジェクト取得のため)
        public Person GetPerson() => (DataContext as AddPersonDialogViewModel)?.Person;
    }
} 