using memory.Models;
using memory.Services;
using memory.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32; // OpenFileDialogのため
using System.Collections.Generic;

namespace memory.ViewModels
{
    // カテゴリ選択用ヘルパーViewModel
    public class SelectableCategoryViewModel : ObservableObject
    {
        private bool _isSelectedInDialog;
        public bool IsSelectedInDialog
        {
            get => _isSelectedInDialog;
            set => SetProperty(ref _isSelectedInDialog, value);
        }
        public Category Category { get; }

        public SelectableCategoryViewModel(Category category, bool isSelected = false)
        {
            Category = category;
            _isSelectedInDialog = isSelected;
        }
    }

    public class AddPersonDialogViewModel : ObservableObject
    {
        private Person _person;
        public Person Person // Personオブジェクトを直接公開してViewでバインド
        {
            get => _person;
            set => SetProperty(ref _person, value);
        }

        private string _newSnsLink;
        public string NewSnsLink
        {
            get => _newSnsLink;
            set => SetProperty(ref _newSnsLink, value);
        }

        public ObservableCollection<SelectableCategoryViewModel> AvailableCategories { get; }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SelectImageCommand { get; }
        public ICommand AddSnsLinkCommand { get; }
        public ICommand RemoveSnsLinkCommand { get; }

        private Action<bool?> _closeAction;

        public AddPersonDialogViewModel(Action<bool?> closeAction, Person personToEdit = null, List<Category> allCategories = null)
        {
            _closeAction = closeAction;

            if (personToEdit == null)
            {
                Person = new Person { Id = Guid.NewGuid(), SnsLinks = new ObservableCollection<SnsLink>(), PersonCategories = new Collection<PersonCategory>() }; // SnsLinksとPersonCategoriesを初期化
            }
            else
            {
                Person = personToEdit; // 編集モード
                // SnsLinksとPersonCategoriesがnullの場合に初期化
                if (Person.SnsLinks == null) Person.SnsLinks = new ObservableCollection<SnsLink>();
                if (Person.PersonCategories == null) Person.PersonCategories = new Collection<PersonCategory>();
            }

            AvailableCategories = new ObservableCollection<SelectableCategoryViewModel>();
            if (allCategories != null)
            {
                foreach (var cat in allCategories)
                {
                    bool isSelected = Person.PersonCategories?.Any(pc => pc.CategoryId == cat.Id) ?? false;
                    AvailableCategories.Add(new SelectableCategoryViewModel(cat, isSelected));
                }
            }

            OkCommand = new RelayCommand(Ok, CanOk);
            CancelCommand = new RelayCommand(Cancel);
            SelectImageCommand = new RelayCommand(ExecuteSelectImage);
            AddSnsLinkCommand = new RelayCommand(ExecuteAddSnsLink, CanExecuteAddSnsLink);
            RemoveSnsLinkCommand = new RelayCommand<SnsLink>(ExecuteRemoveSnsLink, CanExecuteRemoveSnsLink);
        }

        private bool CanOk()
        {
            return !string.IsNullOrWhiteSpace(Person?.Name); // 最低限、名前は必須
        }

        private void Ok()
        {
            // 選択されたカテゴリをPerson.PersonCategoriesに反映
            Person.PersonCategories.Clear();
            foreach (var selectableCat in AvailableCategories.Where(sc => sc.IsSelectedInDialog))
            {
                Person.PersonCategories.Add(new PersonCategory { PersonId = Person.Id, CategoryId = selectableCat.Category.Id });
            }
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
                Person.ImagePath = openFileDialog.FileName;
            }
        }

        private bool CanExecuteAddSnsLink()
        {
            return !string.IsNullOrWhiteSpace(NewSnsLink) && Uri.IsWellFormedUriString(NewSnsLink, UriKind.Absolute);
        }

        private void ExecuteAddSnsLink()
        {
            if (Person.SnsLinks == null) Person.SnsLinks = new ObservableCollection<SnsLink>();
            Person.SnsLinks.Add(new SnsLink { PersonId = Person.Id, Url = NewSnsLink });
            NewSnsLink = string.Empty; // 入力欄をクリア
        }
        
        private bool CanExecuteRemoveSnsLink(SnsLink link)
        {
            return link != null && Person.SnsLinks.Contains(link);
        }

        private void ExecuteRemoveSnsLink(SnsLink link)
        {
             if (link != null && Person.SnsLinks.Contains(link))
            {
                Person.SnsLinks.Remove(link);
            }
        }
    }
} 