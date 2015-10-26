using System.Windows;
using System.Windows.Input;
using GBSharp.ViewModel;
using Microsoft.Win32;

namespace GBSharp.View
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private readonly GameBoyViewModel _mainWindowViewModel;
    private readonly IGameBoy _gameBoy;
    private readonly KeyboardHandler _keyboardHandler;

    public MainWindow()
    {
      InitializeComponent();
      _gameBoy = new GameBoy();
      _keyboardHandler = new KeyboardHandler();
      _mainWindowViewModel = new GameBoyViewModel(_gameBoy, new DispatcherAdapter(this), new WindowAdapter(this), new OpenFileDialogAdapterFactory(), _keyboardHandler);
      this.DataContext = _mainWindowViewModel;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      _keyboardHandler.NotifyKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
      _keyboardHandler.NotifyKeyUp(e);
    }
  }
}
