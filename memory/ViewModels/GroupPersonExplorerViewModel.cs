using memory.Models;
using memory.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input; 
using memory.Helpers; // RelayCommandのため
using memory.Views; // AddGroupDialogのため

namespace memory.ViewModels
{
    public class GroupPersonExplorerViewModel : ObservableObject
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IImageService _imageService;
        private readonly IMemoryRepository _memoryRepository;

        public ObservableCollection<GroupTreeItemViewModel> Groups { get; } = new ObservableCollection<GroupTreeItemViewModel>();
        
        private GroupTreeItemViewModel _selectedGroup;
        public GroupTreeItemViewModel SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value) && value != null)
                {
                    LoadPersonsInSelectedGroupAsync(value.Id);
                }
                else if (value == null) 
                {
                    PersonsInSelectedGroup.Clear();
                }
                SelectedPerson = null;
            }
        }

        public ObservableCollection<Person> PersonsInSelectedGroup { get; } = new ObservableCollection<Person>();

        private Person _selectedPerson;
        public Person SelectedPerson
        {
            get => _selectedPerson;
            set
            {
                if (SetProperty(ref _selectedPerson, value))
                {
                    if (value != null)
                    {
                        LoadMemoriesOfSelectedPersonAsync(value.Id);
                    }
                    else
                    {
                        MemoriesOfSelectedPerson.Clear();
                    }
                }
            }
        }

        public ObservableCollection<MemoryItem> MemoriesOfSelectedPerson { get; } = new ObservableCollection<MemoryItem>();

        private MemoryItem _selectedMemory;
        public MemoryItem SelectedMemory
        {
            get => _selectedMemory;
            set => SetProperty(ref _selectedMemory, value);
        }

        public ICommand AddGroupCommand { get; }
        public ICommand AddPersonCommand { get; }
        public ICommand EditGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand EditPersonCommand { get; private set; }
        public ICommand DeletePersonCommand { get; private set; }
        public ICommand AddMemoryCommand { get; private set; }
        public ICommand EditMemoryCommand { get; private set; }
        public ICommand DeleteMemoryCommand { get; private set; }

        public GroupPersonExplorerViewModel(IGroupRepository groupRepository, IPersonRepository personRepository, ICategoryRepository categoryRepository, IImageService imageService, IMemoryRepository memoryRepository)
        {
            _groupRepository = groupRepository;
            _personRepository = personRepository;
            _categoryRepository = categoryRepository;
            _imageService = imageService;
            _memoryRepository = memoryRepository;

            AddGroupCommand = new RelayCommand(async () => await ExecuteAddGroupAsync());
            AddPersonCommand = new RelayCommand(async () => await ExecuteAddPersonAsync(), CanExecuteAddPerson);
            EditGroupCommand = new RelayCommand(async () => await ExecuteEditGroupAsync(), CanExecuteEditOrDeleteGroup);
            DeleteGroupCommand = new RelayCommand(async () => await ExecuteDeleteGroupAsync(), CanExecuteEditOrDeleteGroup);
            EditPersonCommand = new RelayCommand(async () => await ExecuteEditPersonAsync(), CanExecuteEditOrDeletePerson);
            DeletePersonCommand = new RelayCommand(async () => await ExecuteDeletePersonAsync(), CanExecuteEditOrDeletePerson);
            AddMemoryCommand = new RelayCommand(async () => await ExecuteAddMemoryAsync(), CanExecuteAddMemory);
            EditMemoryCommand = new RelayCommand(async () => await ExecuteEditMemoryAsync(), CanExecuteEditOrDeleteMemory);
            DeleteMemoryCommand = new RelayCommand(async () => await ExecuteDeleteMemoryAsync(), CanExecuteEditOrDeleteMemory);
            LoadGroupsAsync();
        }

        private bool CanExecuteAddPerson()
        {
            return SelectedGroup != null;
        }

        private bool CanExecuteEditOrDeleteGroup()
        {
            return SelectedGroup != null;
        }

        private bool CanExecuteEditOrDeletePerson()
        {
            return SelectedPerson != null;
        }

        private bool CanExecuteAddMemory() => SelectedPerson != null;
        private bool CanExecuteEditOrDeleteMemory() => SelectedMemory != null;

        private async Task ExecuteAddPersonAsync()
        {
            if (SelectedGroup == null) return;

            var allCategories = (await _categoryRepository.GetAllAsync()).ToList();
            var dialog = new AddPersonDialog(null, allCategories);

            if (System.Windows.Application.Current != null && System.Windows.Application.Current.MainWindow != null)
            {
                dialog.Owner = System.Windows.Application.Current.MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                Person newPerson = dialog.GetPerson();
                if (newPerson != null && !string.IsNullOrWhiteSpace(newPerson.Name))
                {
                    newPerson.GroupId = SelectedGroup.Id;

                    if (!string.IsNullOrWhiteSpace(newPerson.ImagePath))
                    {
                        if (System.IO.File.Exists(newPerson.ImagePath))
                        {
                            try
                            {
                                string savedImagePath = await _imageService.SaveImageAsync(newPerson.ImagePath, newPerson.Id.ToString());
                                newPerson.ImagePath = savedImagePath;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"画像保存失敗: {ex.Message}");
                                newPerson.ImagePath = null; 
                            }
                        }
                        else
                        {
                             System.Diagnostics.Debug.WriteLine($"指定された画像パスが見つかりません: {newPerson.ImagePath}");
                             newPerson.ImagePath = null;
                        }
                    }

                    await _personRepository.AddAsync(newPerson);
                    await LoadPersonsInSelectedGroupAsync(SelectedGroup.Id);
                }
            }
        }

        private async Task ExecuteAddGroupAsync()
        {
            var dialog = new AddGroupDialog();
            // ダイアログのオーナーを現在のアプリケーションのメインウィンドウに設定
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.MainWindow != null)
            {
                dialog.Owner = System.Windows.Application.Current.MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                string newGroupName = dialog.GroupName;
                if (!string.IsNullOrWhiteSpace(newGroupName))
                {
                    Guid? parentId = SelectedGroup?.Id;
                    var newGroup = new Group
                    {
                        Id = Guid.NewGuid(),
                        Name = newGroupName,
                        ParentId = parentId
                    };

                    await _groupRepository.AddAsync(newGroup);
                    await LoadGroupsAsync(); // グループリストを再読み込みしてUIを更新

                    // 追加したグループを選択状態にする (任意)
                    SelectGroupById(newGroup.Id);
                }
            }
        }

        private void SelectGroupById(Guid groupId)
        {
            var groupToSelect = FindGroupViewModelRecursive(Groups, groupId);
            if (groupToSelect != null)
            {
                SelectedGroup = groupToSelect;
                // 必要であればツリービューで該当項目を展開する処理も追加
            }
        }

        private GroupTreeItemViewModel FindGroupViewModelRecursive(IEnumerable<GroupTreeItemViewModel> groupViewModels, Guid groupId)
        {
            foreach (var groupVm in groupViewModels)
            {
                if (groupVm.Id == groupId) return groupVm;
                var foundInChild = FindGroupViewModelRecursive(groupVm.ChildGroups, groupId);
                if (foundInChild != null) return foundInChild;
            }
            return null;
        }

        private async Task LoadGroupsAsync()
        {
            Groups.Clear();
            var allGroups = (await _groupRepository.GetAllAsync()).ToList(); // ToList()で実体化
            var rootGroups = allGroups.Where(g => g.ParentId == null)
                                      .Select(g => new GroupTreeItemViewModel(g))
                                      .ToList();

            foreach (var rootGroupVm in rootGroups)
            {
                AddChildGroupsRecursive(rootGroupVm, allGroups);
                Groups.Add(rootGroupVm);
            }
            
            if (Groups.Any() && SelectedGroup == null) // SelectedGroupがまだ設定されていなければ最初のグループを選択
            {
                SelectedGroup = Groups.First();
            }
            else if (!Groups.Any()) // グループが一つもなければ人物リストはクリア
            {
                 PersonsInSelectedGroup.Clear();
            }
        }

        private void AddChildGroupsRecursive(GroupTreeItemViewModel parentGroupVm, List<Group> allGroups)
        {
            var childDomainModels = allGroups.Where(g => g.ParentId == parentGroupVm.Id).ToList();
            foreach (var childDomainModel in childDomainModels)
            {
                var childVm = new GroupTreeItemViewModel(childDomainModel);
                AddChildGroupsRecursive(childVm, allGroups); 
                parentGroupVm.ChildGroups.Add(childVm);
            }
        }

        private async Task LoadPersonsInSelectedGroupAsync(Guid groupId)
        {
            PersonsInSelectedGroup.Clear();
            if (groupId == Guid.Empty) return; // 無効なIDの場合は何もしない

            var persons = await _personRepository.GetByGroupIdAsync(groupId);
            foreach (var person in persons)
            {
                PersonsInSelectedGroup.Add(person);
            }
        }

        private async Task ExecuteEditGroupAsync()
        {
            if (SelectedGroup == null) return;

            var dialog = new AddGroupDialog(SelectedGroup.Name); 
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.MainWindow != null)
            {
                dialog.Owner = System.Windows.Application.Current.MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                string updatedGroupName = dialog.GroupName;
                if (!string.IsNullOrWhiteSpace(updatedGroupName) && updatedGroupName != SelectedGroup.Name)
                {
                    var groupToUpdate = await _groupRepository.GetByIdAsync(SelectedGroup.Id);
                    if (groupToUpdate != null)
                    {
                        groupToUpdate.Name = updatedGroupName;
                        await _groupRepository.UpdateAsync(groupToUpdate);
                        
                        Guid oldSelectedGroupId = SelectedGroup.Id;
                        await LoadGroupsAsync(); 

                        SelectGroupById(oldSelectedGroupId);
                    }
                }
            }
        }

        private async Task ExecuteDeleteGroupAsync()
        {
            if (SelectedGroup == null) return;

            // 削除対象グループに所属する人物がいるか確認
            var personsInGroup = await _personRepository.GetByGroupIdAsync(SelectedGroup.Id);
            if (personsInGroup.Any())
            {
                System.Windows.MessageBox.Show("このグループには人物が所属しているため、削除できません。先に人物を別のグループに移動するか、削除してください。", "削除不可", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // 削除対象グループに子グループが存在するか確認
            // GroupTreeItemViewModelのChildGroupsを見るか、リポジトリから直接確認する
            // ここではSelectedGroup (GroupTreeItemViewModel) のChildGroupsで判定（UI上の状態に依存）
            // より厳密には _groupRepository.GetAllAsync() などで全グループ情報を取得し、ParentId が SelectedGroup.Id であるものを探す
            bool hasChildGroups = SelectedGroup.ChildGroups.Any(); 
            // もしDBから直接確認する場合:
            // var allGroups = await _groupRepository.GetAllAsync();
            // bool hasChildGroups = allGroups.Any(g => g.ParentId == SelectedGroup.Id);
            if (hasChildGroups)
            {
                System.Windows.MessageBox.Show("このグループには子グループが存在するため、削除できません。先に子グループを削除または移動してください。", "削除不可", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var result = System.Windows.MessageBox.Show($"グループ '{SelectedGroup.Name}' を本当に削除しますか？この操作は元に戻せません。", "確認", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                await _groupRepository.DeleteAsync(SelectedGroup.Id);
                SelectedGroup = null; // 選択を解除
                await LoadGroupsAsync(); // グループリストを再読み込み
            }
        }

        private async Task ExecuteEditPersonAsync()
        {
            if (SelectedPerson == null) return;

            // 編集対象のPersonオブジェクトをリポジトリから再取得して、関連データも確実に読み込む
            var personToEdit = await _personRepository.GetByIdAsync(SelectedPerson.Id);
            if (personToEdit == null) return; // 念のため

            var originalImagePath = personToEdit.ImagePath; // 元の画像パスを保持

            var allCategories = (await _categoryRepository.GetAllAsync()).ToList();
            var dialog = new AddPersonDialog(personToEdit, allCategories);

            if (System.Windows.Application.Current?.MainWindow != null)
            {
                dialog.Owner = System.Windows.Application.Current.MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                Person updatedPerson = dialog.GetPerson(); // ダイアログから更新されたPersonオブジェクトを取得
                if (updatedPerson != null)
                {
                    // 画像パスが変更されたか、または新規設定されたか確認
                    if (updatedPerson.ImagePath != originalImagePath && !string.IsNullOrWhiteSpace(updatedPerson.ImagePath))
                    {
                        if (System.IO.File.Exists(updatedPerson.ImagePath)){
                            try
                            {
                                // 新しい画像を保存
                                string savedImagePath = await _imageService.SaveImageAsync(updatedPerson.ImagePath, updatedPerson.Id.ToString());
                                updatedPerson.ImagePath = savedImagePath;
                                // TODO: 古い画像が不要であれば削除するロジック (ImageService側で管理する方が良いかも)
                                if (!string.IsNullOrWhiteSpace(originalImagePath) && originalImagePath != savedImagePath)
                                {
                                     _imageService.DeleteImageAsync(originalImagePath); // 古い画像を削除
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"編集中の画像保存失敗: {ex.Message}");
                                updatedPerson.ImagePath = originalImagePath; //失敗したら元に戻す
                            }
                        } else {
                            System.Diagnostics.Debug.WriteLine($"指定された新しい画像パスが見つかりません: {updatedPerson.ImagePath}");
                            updatedPerson.ImagePath = originalImagePath; // パスが無効な場合は元に戻す
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(updatedPerson.ImagePath) && !string.IsNullOrWhiteSpace(originalImagePath))
                    {
                        // 画像がクリアされた場合、古い画像を削除
                         _imageService.DeleteImageAsync(originalImagePath);
                    }

                    await _personRepository.UpdateAsync(updatedPerson);
                    await LoadPersonsInSelectedGroupAsync(SelectedGroup.Id); // 人物リストを再読み込み
                }
            }
            else // ダイアログがキャンセルされた場合、変更を元に戻すために再読込
            {
                await LoadPersonsInSelectedGroupAsync(SelectedGroup.Id);
            }
        }

        private async Task ExecuteDeletePersonAsync()
        {
            if (SelectedPerson == null) return;

            // TODO: 削除対象人物に思い出が紐づいている場合の考慮 (今回はまず人物のみ削除)
            var result = System.Windows.MessageBox.Show($"人物 '{SelectedPerson.Name}' を本当に削除しますか？この操作は元に戻せません。", "確認", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // 画像も削除
                if (!string.IsNullOrWhiteSpace(SelectedPerson.ImagePath))
                {
                    _imageService.DeleteImageAsync(SelectedPerson.ImagePath);
                }
                await _personRepository.DeleteAsync(SelectedPerson.Id);
                await LoadPersonsInSelectedGroupAsync(SelectedGroup.Id); // 人物リストを再読み込み
            }
        }

        private async Task LoadMemoriesOfSelectedPersonAsync(Guid personId)
        {
            MemoriesOfSelectedPerson.Clear();
            if (personId == Guid.Empty) return;

            var memories = await _memoryRepository.GetByPersonIdAsync(personId);
            foreach (var memory in memories.OrderByDescending(m => m.Date))
            {
                MemoriesOfSelectedPerson.Add(memory);
            }
        }

        private async Task ExecuteAddMemoryAsync()
        {
            if (SelectedPerson == null) return;

            var dialog = new AddMemoryDialog(null, SelectedPerson.Id);
            if (System.Windows.Application.Current?.MainWindow != null)
            {
                dialog.Owner = System.Windows.Application.Current.MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                MemoryItem newMemory = dialog.GetMemoryItem();
                if (newMemory != null && !string.IsNullOrWhiteSpace(newMemory.Title))
                {
                    if (!string.IsNullOrWhiteSpace(newMemory.ImagePath) && System.IO.File.Exists(newMemory.ImagePath))
                    {
                        try
                        {
                            string savedImagePath = await _imageService.SaveImageAsync(newMemory.ImagePath, newMemory.Id.ToString());
                            newMemory.ImagePath = savedImagePath;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"思い出の画像保存失敗: {ex.Message}");
                            newMemory.ImagePath = null;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(newMemory.ImagePath))
                    {
                        // 指定された画像パスが存在しない場合はクリア
                        newMemory.ImagePath = null;
                    }

                    await _memoryRepository.AddAsync(newMemory);
                    await LoadMemoriesOfSelectedPersonAsync(SelectedPerson.Id);
                }
            }
        }

        private async Task ExecuteEditMemoryAsync()
        {
            if (SelectedMemory == null || SelectedPerson == null) return;

            var memoryToEdit = await _memoryRepository.GetByIdAsync(SelectedMemory.Id); // DBから最新の状態を取得
            if (memoryToEdit == null) return;

            var originalImagePath = memoryToEdit.ImagePath;
            var dialog = new AddMemoryDialog(memoryToEdit, SelectedPerson.Id); // PersonIdは表示や参照用として渡す

            if (System.Windows.Application.Current?.MainWindow != null)
            {
                dialog.Owner = System.Windows.Application.Current.MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                MemoryItem updatedMemory = dialog.GetMemoryItem();
                if (updatedMemory != null)
                {
                    if (updatedMemory.ImagePath != originalImagePath && !string.IsNullOrWhiteSpace(updatedMemory.ImagePath))
                    {
                        if (System.IO.File.Exists(updatedMemory.ImagePath))
                        {
                            try
                            {
                                string savedImagePath = await _imageService.SaveImageAsync(updatedMemory.ImagePath, updatedMemory.Id.ToString());
                                updatedMemory.ImagePath = savedImagePath;
                                if (!string.IsNullOrWhiteSpace(originalImagePath) && originalImagePath != savedImagePath)
                                {
                                    await _imageService.DeleteImageAsync(originalImagePath);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"思い出の画像更新失敗: {ex.Message}");
                                updatedMemory.ImagePath = originalImagePath;
                            }
                        }
                        else
                        {
                            updatedMemory.ImagePath = originalImagePath; // 無効なパスなら元に戻す
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(updatedMemory.ImagePath) && !string.IsNullOrWhiteSpace(originalImagePath))
                    {
                        await _imageService.DeleteImageAsync(originalImagePath);
                    }

                    await _memoryRepository.UpdateAsync(updatedMemory);
                    await LoadMemoriesOfSelectedPersonAsync(SelectedPerson.Id);
                }
            }
            else // キャンセル時もリストを再読み込みして編集中に行ったかもしれないUI上の変更を元に戻す
            {
                await LoadMemoriesOfSelectedPersonAsync(SelectedPerson.Id);
            }
        }

        private async Task ExecuteDeleteMemoryAsync()
        {
            if (SelectedMemory == null || SelectedPerson == null) return;

            var result = System.Windows.MessageBox.Show($"思い出 '{SelectedMemory.Title}' を本当に削除しますか？", "確認", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                if (!string.IsNullOrWhiteSpace(SelectedMemory.ImagePath))
                {
                    await _imageService.DeleteImageAsync(SelectedMemory.ImagePath);
                }
                await _memoryRepository.DeleteAsync(SelectedMemory.Id);
                await LoadMemoriesOfSelectedPersonAsync(SelectedPerson.Id);
            }
        }
    }
} 