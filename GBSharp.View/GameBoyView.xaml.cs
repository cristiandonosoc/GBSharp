﻿using System.Windows;
using GBSharp.ViewModel;

namespace GBSharp.View
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private readonly GameBoyViewModel _mainWindowViewModel;
    private readonly IGameBoy _gameBoy;

    public MainWindow()
    {
      InitializeComponent();
      _gameBoy = new GameBoy();
      _mainWindowViewModel = new GameBoyViewModel(_gameBoy);
      this.DataContext = _mainWindowViewModel;
    }
  }
}