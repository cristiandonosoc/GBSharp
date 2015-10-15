using System.Windows.Controls;

namespace GBSharp.View
{
  /// <summary>
  /// Interaction logic for MemoryView.xaml
  /// </summary>
  public partial class MemoryView : UserControl
  {
    public MemoryView()
    {
      InitializeComponent();
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      //if(e.AddedItems.Count > 0)
      //{
      //  ((ListBox)sender).ScrollIntoView(e.AddedItems[0]);
      //}
    }
  }
}
