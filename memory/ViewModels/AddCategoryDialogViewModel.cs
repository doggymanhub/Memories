using memory.Helpers;
using System;
using System.Windows.Input;

namespace memory.ViewModels
{
    public class AddCategoryDialogViewModel : ObservableObject
    {
        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set => SetProperty(ref _categoryName, value);
        }

        private string _categoryColor;
        public string CategoryColor
        {
            get => _categoryColor;
            set => SetProperty(ref _categoryColor, value);
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        private Action<bool?> _closeAction;

        public AddCategoryDialogViewModel(Action<bool?> closeAction, string existingName = null, string existingColor = null)
        {
            _closeAction = closeAction;
            CategoryName = existingName;
            CategoryColor = existingColor;

            OkCommand = new RelayCommand(Ok, CanOk);
            CancelCommand = new RelayCommand(Cancel);
        }

        private bool CanOk()
        {
            return !string.IsNullOrWhiteSpace(CategoryName);
        }

        private void Ok()
        {
            _closeAction?.Invoke(true);
        }

        private void Cancel()
        {
            _closeAction?.Invoke(false);
        }
    }
} 