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
            _mainWindowViewModel = new GameBoyViewModel(_gameBoy,
                                                        new DispatcherAdapter(this),
                                                        new WindowAdapter(this),
                                                        new OpenFileDialogAdapterFactory(),
                                                        _keyboardHandler);
            this.DataContext = _mainWindowViewModel;
        }

        protected override void OnClosed(System.EventArgs e)
        {
            _mainWindowViewModel.OnClosed();
            _gameBoy.Dispose();
            base.OnClosed(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            _keyboardHandler.NotifyKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            _keyboardHandler.NotifyKeyUp(e);
        }

        private void DockingManager_Loaded(object sender, RoutedEventArgs e)
        {
            Background.ToggleAutoHide();
            Breakpoints.ToggleAutoHide();
            CPU.ToggleAutoHide();
            //DisplayTiming.ToggleAutoHide();
            Interrupts.ToggleAutoHide();
            InstructionHistogram.ToggleAutoHide();
            IORegisters.ToggleAutoHide();
            Memory.ToggleAutoHide();
            MemoryImage.ToggleAutoHide();
            //SoundChannelInternals.ToggleAutoHide();
            //Spectogram.ToggleAutoHide();
            SpriteLayer.ToggleAutoHide();
            //Sprites.ToggleAutoHide();
            SoundRecording.ToggleAutoHide();
            TileMap.ToggleAutoHide();
            Window.ToggleAutoHide();
        }
    }
}
