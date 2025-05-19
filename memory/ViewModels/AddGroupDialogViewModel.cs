using System.Windows.Input;
using memory.Helpers; // RelayCommandのため

namespace memory.ViewModels
{
    public class AddGroupDialogViewModel : ObservableObject
    {
        private string _groupName;
        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        private System.Action<bool?> _closeAction; // ダイアログの結果を通知するためのAction
        private string _originalGroupName; // 編集時の元のグループ名保持用 (任意)

        public AddGroupDialogViewModel(System.Action<bool?> closeAction, string existingGroupName = null)
        {
            _closeAction = closeAction;
            _originalGroupName = existingGroupName;
            GroupName = existingGroupName; // 編集のために初期値を設定

            OkCommand = new RelayCommand(Ok, CanOk);
            CancelCommand = new RelayCommand(Cancel);
        }

        private bool CanOk()
        {
            return !string.IsNullOrWhiteSpace(GroupName);
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