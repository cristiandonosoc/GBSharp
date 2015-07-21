using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GBSharp.ViewModel
{
  public enum MemoryWordFormat
  {
    Decimal,
    Binary,
    Hexa
  }

  public class MemoryViewModel : ViewModelBase
  {
    private readonly IMemory _memory;
    private MemoryWordFormat _memoryWordValueFormat = MemoryWordFormat.Decimal;
    private MemoryWordFormat _memoryWordAddressFormat = MemoryWordFormat.Decimal;
    private readonly ObservableCollection<MemoryFormatViewModel> _addressFormats = new ObservableCollection<MemoryFormatViewModel>();
    private MemoryFormatViewModel _selectedAddressFormat;
    private readonly ObservableCollection<MemoryFormatViewModel> _valueFormats = new ObservableCollection<MemoryFormatViewModel>();
    private MemoryFormatViewModel _selectedValueFormat;
    private readonly ObservableCollection<MemoryWordViewModel> _memoryWords = new ObservableCollection<MemoryWordViewModel>();

    private int _selectedAddress;
    private string _name;
    private readonly int _initialAddress;
    private int _finalAddress;

    public MemoryWordFormat MemoryWordValueFormat
    {
      get { return _memoryWordValueFormat; }
      set { _memoryWordValueFormat = value; }
    }

    public MemoryWordFormat MemoryWordAddressFormat
    {
      get { return _memoryWordAddressFormat; }
      set { _memoryWordAddressFormat = value; }
    }

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

    public int SelectedAddress
    {
      get { return _selectedAddress; }
      set
      {
        if (_selectedAddress != value)
        {
          _selectedAddress = value;
          OnPropertyChanged(() => SelectedAddress);
        }
      }
    }

    public string Name
    {
      get { return _name; }
    }

    public MemoryViewModel(IMemory memory, string name, int initialAddress=0, int finalAddress=-1)
    {
      if (memory == null) throw new ArgumentNullException("memory");
      _memory = memory;
      _name = name;
      _initialAddress = initialAddress;
      _finalAddress = finalAddress;
      
      InitMemoryFormats();
    }

    public void Update()
    {
      CopyFromDomain();
    }

    private void InitMemoryFormats()
    {
      _addressFormats.Add(new MemoryFormatViewModel("Decimal", MemoryWordFormat.Decimal));
      _addressFormats.Add(new MemoryFormatViewModel("Binary", MemoryWordFormat.Binary));
      _addressFormats.Add(new MemoryFormatViewModel("Hexadecimal", MemoryWordFormat.Hexa));
      SelectedAddressFormat = _addressFormats.First();

      _valueFormats.Add(new MemoryFormatViewModel("Decimal", MemoryWordFormat.Decimal));
      _valueFormats.Add(new MemoryFormatViewModel("Binary", MemoryWordFormat.Binary));
      _valueFormats.Add(new MemoryFormatViewModel("Hexadecimal", MemoryWordFormat.Hexa));
      SelectedValueFormat = _valueFormats.First();
    }

    private void UpdateMemoryWords()
    {
      foreach (var memoryWord in _memoryWords)
      {
        memoryWord.UpdateAddressFormat(_selectedAddressFormat.WordFormat);
        memoryWord.UpdateValueFormat(_selectedValueFormat.WordFormat);
      }
    }

   
  
    private void CopyFromDomain()
    {
      _memoryWords.Clear();
      if (_finalAddress == -1)
        _finalAddress = _memory.Data.Length;
      for (int address = _initialAddress; address < _finalAddress; address++)
      {
        _memoryWords.Add(new MemoryWordViewModel(address, _memory.Data[address]));
      }
    }


  
  
  }
}