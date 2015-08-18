using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace GBSharp.View
{
  /// <summary>
  /// Interaction logic for GameBoyControllerView.xaml
  /// </summary>
  public partial class GameBoyControllerView : UserControl
  {
    public GameBoyControllerView()
    {
      InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
      var openFileDialog = new OpenFileDialog();
      openFileDialog.ShowDialog();

      FileText.Text = openFileDialog.FileName;
    }
  }
}
