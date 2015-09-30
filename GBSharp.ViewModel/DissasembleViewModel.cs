using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
  public class DissasembleViewModel : ViewModelBase
  {
    private readonly ICPU _cpu;
    private readonly ObservableCollection<InstructionViewModel> _instructions = new ObservableCollection<InstructionViewModel>();

    public ObservableCollection<InstructionViewModel> Instructions
    {
      get { return _instructions; }
    }

    public ICommand DissasembleCommand
    {
      get { return new DelegateCommand(Dissasemble); }
    }
    
    public DissasembleViewModel(ICPU cpu)
    {
      _cpu = cpu;
    }


    public void Dissasemble()
    {
      // TODO(aaecheve): This is a tad too slow to run every time (gets old quick)
      //var dissasembledInstructions = _cpu.Dissamble();
      //_instructions.Clear();
      //foreach (var dissasembledInstruction in dissasembledInstructions)
      //{
      //  _instructions.Add(new InstructionViewModel(dissasembledInstruction));
      //}
    }
  }
}