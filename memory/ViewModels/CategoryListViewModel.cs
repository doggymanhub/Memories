using memory.Models;
using memory.Services;
using memory.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using memory.Views; // AddCategoryDialogを参照するために追加
// TODO: AddCategoryDialogのViewを後で参照
// using memory.Views; 

namespace memory.ViewModels
{
    public class CategoryListViewModel : ObservableObject
    {
        private readonly ICategoryRepository _categoryRepository;

        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();
        
        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public ICommand AddCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        public CategoryListViewModel(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
            LoadCategoriesAsync();

            AddCategoryCommand = new RelayCommand(async () => await ExecuteAddCategoryAsync());
            EditCategoryCommand = new RelayCommand(async () => await ExecuteEditCategoryAsync(), CanExecuteEditOrDeleteCategory);
            DeleteCategoryCommand = new RelayCommand(async () => await ExecuteDeleteCategoryAsync(), CanExecuteEditOrDeleteCategory);
        }

        private async Task LoadCategoriesAsync()
        {
            Categories.Clear();
            var categories = await _categoryRepository.GetAllAsync();
            foreach (var category in categories.OrderBy(c => c.Name))
            {
                Categories.Add(category);
            }
        }

        private bool CanExecuteEditOrDeleteCategory()
        {
            return SelectedCategory != null;
        }

        private async Task ExecuteAddCategoryAsync()
        {
            var dialog = new AddCategoryDialog(); 
            if (Application.Current?.MainWindow != null) dialog.Owner = Application.Current.MainWindow;
            
            if (dialog.ShowDialog() == true)
            {
                var newCategory = new Category 
                { 
                    Id = System.Guid.NewGuid(), 
                    Name = dialog.CategoryName, 
                    Color = string.IsNullOrWhiteSpace(dialog.CategoryColor) ? null : dialog.CategoryColor // 空の場合はnullを設定
                };
                if (!string.IsNullOrWhiteSpace(newCategory.Name))
                {
                    await _categoryRepository.AddAsync(newCategory);
                    await LoadCategoriesAsync();
                }
            }
        }

        private async Task ExecuteEditCategoryAsync()
        {
            if (SelectedCategory == null) return;
            var dialog = new AddCategoryDialog(SelectedCategory.Name, SelectedCategory.Color); 
            if (Application.Current?.MainWindow != null) dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                SelectedCategory.Name = dialog.CategoryName; 
                SelectedCategory.Color = string.IsNullOrWhiteSpace(dialog.CategoryColor) ? null : dialog.CategoryColor; // 空の場合はnullを設定
                if (!string.IsNullOrWhiteSpace(SelectedCategory.Name))
                {
                    await _categoryRepository.UpdateAsync(SelectedCategory);
                    // UI上での即時反映のため、プロパティ変更通知を明示的に行うか、リストを再読み込み
                    // ObservableCollection内のオブジェクトのプロパティ変更は自動では検知されないため、
                    // LoadCategoriesAsync()を呼ぶか、SelectedCategoryの変更を通知する処理が必要
                    // ここでは簡単のため全体を再読み込み
                    var selectedId = SelectedCategory.Id;
                    await LoadCategoriesAsync();
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == selectedId);
                }
            }
        }

        private async Task ExecuteDeleteCategoryAsync()
        {
            if (SelectedCategory == null) return;

            // TODO: カテゴリが人物に紐付いている場合の確認処理を追加 (PersonRepositoryが必要になるかも)
            var result = MessageBox.Show($"カテゴリ '{SelectedCategory.Name}' を本当に削除しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                await _categoryRepository.DeleteAsync(SelectedCategory.Id);
                await LoadCategoriesAsync();
            }
        }
    }
} 