using System.Collections.ObjectModel;
using System.Linq; 
using System.Threading.Tasks;
using memory.Services; 
using memory.Models; // Person, MemoryItem のため
using System;

namespace memory.ViewModels
{
    public class DashboardViewModel : ObservableObject 
    {
        private readonly IPersonRepository _personRepository;
        private readonly IMemoryRepository _memoryRepository;

        private int _totalPersonsCount;
        public int TotalPersonsCount
        {
            get => _totalPersonsCount;
            set => SetProperty(ref _totalPersonsCount, value);
        }

        private int _totalMemoriesCount;
        public int TotalMemoriesCount
        {
            get => _totalMemoriesCount;
            set => SetProperty(ref _totalMemoriesCount, value);
        }

        public ObservableCollection<string> MemoriesRanking { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> RecentMemories { get; } = new ObservableCollection<string>();
        
        public DashboardViewModel(IPersonRepository personRepository, IMemoryRepository memoryRepository)
        {
            _personRepository = personRepository;
            _memoryRepository = memoryRepository;
            LoadDashboardDataAsync();
        }

        public async Task LoadDashboardDataAsync()
        {
            var allPersons = (await _personRepository.GetAllAsync()).ToList();
            var allMemories = (await _memoryRepository.GetAllAsync()).ToList();

            TotalPersonsCount = allPersons.Count;
            TotalMemoriesCount = allMemories.Count;

            MemoriesRanking.Clear();
            // 人物ごとの思い出数を集計し、ランキングを作成 (上位5件)
            var ranking = allPersons
                .Select(p => new
                {
                    PersonName = p.Name,
                    MemoryCount = allMemories.Count(m => m.PersonId == p.Id)
                })
                .Where(x => x.MemoryCount > 0) // 思い出が1件以上ある人物のみ
                .OrderByDescending(x => x.MemoryCount)
                .ThenBy(x => x.PersonName) // 思い出数が同じ場合は名前順
                .Take(5)
                .ToList();

            foreach (var rankItem in ranking)
            {
                MemoriesRanking.Add($"{rankItem.PersonName} ({rankItem.MemoryCount}件)");
            }

            RecentMemories.Clear();
            // 最近登録された思い出を取得 (最新5件)
            // Person名を引くためにallPersonsも利用
            var recent = allMemories
                .OrderByDescending(m => m.Date) // 思い出の日付で降順ソート
                .Take(5)
                .Select(m => 
                {
                    var personName = allPersons.FirstOrDefault(p => p.Id == m.PersonId)?.Name ?? "不明な人物";
                    return $"{m.Date:yyyy/MM/dd} - {m.Title} ({personName})";
                })
                .ToList();
            
            foreach (var recentMemoryString in recent)
            {
                RecentMemories.Add(recentMemoryString);
            }
        }
    }
} 