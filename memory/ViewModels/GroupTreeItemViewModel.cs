using memory.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace memory.ViewModels
{
    public class GroupTreeItemViewModel : ObservableObject
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private Guid _id;
        public Guid Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            // Note: IsSelectedのsetterで親VMに通知したり、関連アクションを実行することもできる
            set => SetProperty(ref _isSelected, value); 
        }
        
        private bool _isExpanded = true; 
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        // このGroupTreeItemViewModelに紐づく実際のGroupモデル (必要な場合)
        // public Group Model { get; private set; }

        public ObservableCollection<GroupTreeItemViewModel> ChildGroups { get; } = new ObservableCollection<GroupTreeItemViewModel>();

        public GroupTreeItemViewModel(Group group)
        {
            // Model = group; // 元のモデルを保持する場合
            Id = group.Id;
            Name = group.Name;
        }
    }
} 