using memory.Models;
using memory.Helpers;
using System;
using System.Windows.Input;
using Microsoft.Win32; // OpenFileDialogのため

namespace memory.ViewModels
{
    public class AddMemoryDialogViewModel : ObservableObject
    {
        private MemoryItem _memoryItem;
        public MemoryItem MemoryItem
        {
            get => _memoryItem;
            set => SetProperty(ref _memoryItem, value);
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SelectImageCommand { get; }

        private Action<bool?> _closeAction;
        private Guid _personId;

        public AddMemoryDialogViewModel(Action<bool?> closeAction, MemoryItem memoryToEdit = null, Guid personId = default)
        {
            _closeAction = closeAction;
            _personId = personId;

            if (memoryToEdit == null) // 新規作成モード
            {
                MemoryItem = new MemoryItem 
                { 
                    Id = Guid.NewGuid(), 
                    Date = DateTime.Today, // 初期値を今日の日付に
                    PersonId = _personId 
                };
            }
            else // 編集モード
            {
                MemoryItem = memoryToEdit;
                // PersonIdは編集中は変更しないので、memoryToEditが持つものをそのまま使う
                // _personIdは新規作成時のみ使用される想定
            }

            OkCommand = new RelayCommand(Ok, CanOk);
            CancelCommand = new RelayCommand(Cancel);
            SelectImageCommand = new RelayCommand(ExecuteSelectImage);
        }

        private bool CanOk()
        {
            // タイトルは必須とする
            return MemoryItem != null && !string.IsNullOrWhiteSpace(MemoryItem.Title);
        }

        private void Ok()
        {
            _closeAction?.Invoke(true);
        }

        private void Cancel()
        {
            _closeAction?.Invoke(false);
        }

        private void ExecuteSelectImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "画像ファイル (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|すべてのファイル (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                MemoryItem.ImagePath = openFileDialog.FileName;
                OnPropertyChanged(nameof(MemoryItem)); // MemoryItemオブジェクト自体は変わらないが、そのプロパティが変更されたことを通知
            }
        }
    }
} 