using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ModernWpf.Controls;
using memory.ViewModels;

namespace memory;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            string tag = null;
            if (args.IsSettingsInvoked) 
            {
                // FooterMenuItemsのSettingsのTagプロパティを参照する
                // NavigationViewのSettingsItemはInvokedItemContainerが直接取得できない場合があるため、
                // IsSettingsInvokedで判断し、固定のTag（"Settings"など）をViewModelのNavigateメソッドに渡す。
                // XAML側で<ui:NavigationView IsSettingsVisible="True" SettingsItem="{ui:NavigationViewItem Content=設定 Icon=Setting Tag=Settings}" /> のように
                // 明示的に SettingsItem を定義してTagをつける方法もあるが、デフォルトのSettings挙動ではTagを直接取得しにくい。
                tag = "Settings"; // XAMLのFooterにある設定アイテムのTagと合わせる
            }
            else if (args.InvokedItemContainer is NavigationViewItem item)
            {
                tag = item.Tag as string;
            }

            if (!string.IsNullOrEmpty(tag))
            {
                viewModel.Navigate(tag);
            }
        }
    }
}