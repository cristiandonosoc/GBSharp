﻿using System;
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

namespace GBSharp.View
{
  /// <summary>
  /// Interaction logic for DissasembleView.xaml
  /// </summary>
  public partial class DissasembleView : UserControl
  {
    public DissasembleView()
    {
      InitializeComponent();
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (e.AddedItems.Count == 0) { return; }
      try
      {
        ((ListBox)sender).ScrollIntoView(e.AddedItems[0]);
      }
      catch(Exception)
      {

      }
    }
  }
}
