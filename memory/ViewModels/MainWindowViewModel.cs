using Microsoft.Extensions.DependencyInjection; // App.ServiceProvider を使うなら必要
using ModernWpf.Controls;
using System;

namespace memory.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private ObservableObject _currentViewModel;
        public ObservableObject CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        private string _windowTitle = "人物・思い出管理アプリ";
        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }

        private readonly IServiceProvider _serviceProvider;

        public MainWindowViewModel(IServiceProvider serviceProvider, DashboardViewModel initialViewModel)
        {
            _serviceProvider = serviceProvider;
            CurrentViewModel = initialViewModel;
        }

        public void Navigate(string viewTag)
        {
            switch (viewTag)
            {
                case "Dashboard":
                    CurrentViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
                    break;
                case "Explorer":
                    CurrentViewModel = _serviceProvider.GetRequiredService<GroupPersonExplorerViewModel>();
                    break;
                case "Categories":
                    CurrentViewModel = _serviceProvider.GetRequiredService<CategoryListViewModel>();
                    break;
                case "Settings":
                    CurrentViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
                    break;
                default:
                    break;
            }
        }

        // ナビゲーションコマンドなどもここに追加予定
    }
} 