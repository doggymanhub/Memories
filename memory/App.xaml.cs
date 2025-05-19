using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using memory.Services; // ApplicationDbContext を参照するために追加
using Microsoft.EntityFrameworkCore; // AddDbContext, UseSqlite のために追加

namespace memory;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // DbContextの登録
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("Data Source=memory.db"));

        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IMemoryRepository, MemoryRepository>();
        services.AddScoped<IImageService, ImageService>();

        // ViewModelの登録
        services.AddSingleton<MainWindowViewModel>(); 
        services.AddTransient<DashboardViewModel>(); 
        services.AddTransient<GroupPersonExplorerViewModel>(); 
        services.AddTransient<CategoryListViewModel>();    
        services.AddTransient<SettingsViewModel>();        
        
        // MainWindowをDIに追加
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // マイグレーションの自動適用 (開発時のみ推奨)
        // EnsureDatabaseCreated(ServiceProvider);

        var mainWindow = ServiceProvider.GetService<MainWindow>(); 
        if (mainWindow != null) 
        {
            mainWindow.Show();
        }
        else
        {
            MessageBox.Show("MainWindowを起動できませんでした。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            Current.Shutdown(); 
        }
    }

    // private void EnsureDatabaseCreated(IServiceProvider serviceProvider)
    // {
    //     using (var scope = serviceProvider.CreateScope())
    //     {
    //         var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //         dbContext.Database.Migrate(); // マイグレーションを適用
    //         // dbContext.Database.EnsureCreated(); // マイグレーションを使わない場合はこちら
    //     }
    // }
}

