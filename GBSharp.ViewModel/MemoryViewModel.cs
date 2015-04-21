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

  public class MemoryViewModel : ViewModelBase, IDisposable
  {
    private readonly IMemory _memory;
    private MemoryWordFormat _memoryWordValueFormat = MemoryWordFormat.Decimal;
    private MemoryWordFormat _memoryWordAddressFormat = MemoryWordFormat.Decimal;
    private readonly ObservableCollection<MemoryFormatViewModel> _addressFormats = new ObservableCollection<MemoryFormatViewModel>();
    private MemoryFormatViewModel _selectedAddressFormat;
    private readonly ObservableCollection<MemoryFormatViewModel> _valueFormats = new ObservableCollection<MemoryFormatViewModel>();
    private MemoryFormatViewModel _selectedValueFormat;
    private readonly ObservableCollection<MemoryWordViewModel> _memoryWords = new ObservableCollection<MemoryWordViewModel>(); 
    
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

    public MemoryViewModel(IMemory memory)
    {
      if (memory == null) throw new ArgumentNullException("memory");
      _memory = memory;

      InitMemoryFormats();
      CopyFromDomain();
      RegisterEvents();
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

    private void RegisterEvents()
    {
      _memory.ValuesChanged += MemoryValuesChanged;
    }

    private void MemoryValuesChanged()
    {
      CopyFromDomain();
    }

    private void CopyFromDomain()
    {
      _memoryWords.Clear();
      for (int address = 0; address < _memory.Values.Length; address++)
      {
        _memoryWords.Add(new MemoryWordViewModel(address, _memory.Values[address]));
      }
    }


    public void Dispose()
    {
      UnregisterEvents();
    }

    private void UnregisterEvents()
    {
      if (_memory != null)
        _memory.ValuesChanged -= MemoryValuesChanged;
    }
  }
}