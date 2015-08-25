using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
 
  public class MemoryViewModel : ViewModelBase
  {
    private readonly IMemory _memory;
    private readonly ObservableCollection<MemoryFormatViewModel> _addressFormats = new ObservableCollection<MemoryFormatViewModel>();
    private MemoryFormatViewModel _selectedAddressFormat;
    private readonly ObservableCollection<MemoryFormatViewModel> _valueFormats = new ObservableCollection<MemoryFormatViewModel>();
    private MemoryFormatViewModel _selectedValueFormat;
    private readonly ObservableCollection<MemoryWordViewModel> _memoryWords = new ObservableCollection<MemoryWordViewModel>();
    private MemorySectionViewModel _selectedSection;
    private readonly ObservableCollection<MemorySectionViewModel> _memorySections = new ObservableCollection<MemorySectionViewModel>();

    private string _name;
    
    public ObservableCollection<MemoryWordViewModel> MemoryWords
    {
      get { return _memoryWords; }
    }
    
    public ObservableCollection<MemoryFormatViewModel> AddressFormats
    {
      get { return _addressFormats; }
    }

    public ObservableCollection<MemoryFormatViewModel> ValueFormats
    {
      get { return _valueFormats; }
    }

    public ObservableCollection<MemorySectionViewModel> MemorySections
    {
      get { return _memorySections; }
    }

    public MemoryFormatViewModel SelectedAddressFormat
    {
      get { return _selectedAddressFormat; }
      set
      {
        if (_selectedAddressFormat != value)
        {
          _selectedAddressFormat = value;
          OnPropertyChanged(() => SelectedAddressFormat);
          UpdateMemoryWords();
        }
      }
    }

    public MemoryFormatViewModel SelectedValueFormat
    {
      get { return _selectedValueFormat; }
      set
      {
        if (_selectedValueFormat != value)
        {
          _selectedValueFormat = value;
          OnPropertyChanged(() => SelectedValueFormat);
          UpdateMemoryWords();
        }
      }
    }

    public MemorySectionViewModel SelectedSection
    {
      get { return _selectedSection; }
      set
      {
        if (_selectedSection != value)
        {
          _selectedSection = value;
          OnPropertyChanged(() => SelectedSection);
          OnSelectedSectionUpdated();
        }
      }
    }

  

    public string Name
    {
      get { return _name; }
    }

    public ICommand ReadCommand
    {
      get { return new DelegateCommand(CopyFromDomain); }
    }

    public ICommand WriteCommand
    {
      get { return new DelegateCommand(CopyToDomain); }
    }

    
    public MemoryViewModel(IMemory memory, string name, int initialAddress=0, int finalAddress=-1)
    {
      if (memory == null) throw new ArgumentNullException("memory");
      _memory = memory;
      _name = name;
      
      Init();
    }

    public void Update()
    {
      CopyFromDomain();
    }

    private void Init()
    {
      _addressFormats.Add(new MemoryFormatViewModel("Hexadecimal", MemoryWordFormat.Hexa));
      _addressFormats.Add(new MemoryFormatViewModel("Decimal", MemoryWordFormat.Decimal));
      SelectedAddressFormat = _addressFormats.First();

      _valueFormats.Add(new MemoryFormatViewModel("Hexadecimal", MemoryWordFormat.Hexa));
      _valueFormats.Add(new MemoryFormatViewModel("Decimal", MemoryWordFormat.Decimal));
      SelectedValueFormat = _valueFormats.First();

      _memorySections.Add(new MemorySectionViewModel("Lil Internal RAM", 0xFF80, 0xFFFF));
      _memorySections.Add(new MemorySectionViewModel("IO Ports", 0xFF00, 0xFF4C));
      _memorySections.Add(new MemorySectionViewModel("OAM", 0xFE00, 0xFEA0));
      _memorySections.Add(new MemorySectionViewModel("Internal RAM", 0xC000, 0xE000));
      _memorySections.Add(new MemorySectionViewModel("Switchable RAM", 0xA000, 0xC000));
      _memorySections.Add(new MemorySectionViewModel("VRAM", 0x8000, 0xC000));
      _memorySections.Add(new MemorySectionViewModel("ROM", 0x0000, 0x8000));
      SelectedSection = _memorySections.First();
    }

    private void UpdateMemoryWords()
    {
      foreach (var memoryWord in _memoryWords)
      {
        memoryWord.UpdateAddressFormat(_selectedAddressFormat.WordFormat);
        memoryWord.UpdateValueFormat(_selectedValueFormat.WordFormat);
      }
    }

    private void OnSelectedSectionUpdated()
    {
      CopyFromDomain();
    }

  
    private void CopyFromDomain()
    {
      _memoryWords.Clear();
      for (uint address = _selectedSection.InitialAddress; address < _selectedSection.FinalAddress; address++)
      {
        _memoryWords.Add(new MemoryWordViewModel(address, _memory.Data[address]));
      }
      UpdateMemoryWords();
    }

    private void CopyToDomain()
    {
      throw new NotImplementedException();
    }

  
  
  }
}