using System.Windows;
using GBSharp.ViewModel;

namespace GBSharp.View
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private readonly MainWindowViewModel _mainWindowViewModel;
    
    public MainWindow()
    {
      InitializeComponent();
      _mainWindowViewModel = new MainWindowViewModel();
      this.DataContext = _mainWindowViewModel;
    }
  }
}
