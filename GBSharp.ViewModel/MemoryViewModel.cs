﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace GBSharp.ViewModel
{

  public class MemoryViewModel : ViewModelBase
  {
    private IMemory _memory;
    private readonly ObservableCollection<MemoryFormatViewModel> _addressFormats = new ObservableCollection<MemoryFormatViewModel>();
    private MemoryFormatViewModel _selectedAddressFormat;
    private readonly ObservableCollection<MemoryFormatViewModel> _valueFormats = new ObservableCollection<MemoryFormatViewModel>();
    private MemoryFormatViewModel _selectedValueFormat;
    private readonly ObservableCollection<MemoryWordGroupViewModel> _memoryWordGroups = new ObservableCollection<MemoryWordGroupViewModel>();
    private MemorySectionViewModel _selectedSection;
    private readonly ObservableCollection<MemorySectionViewModel> _memorySections = new ObservableCollection<MemorySectionViewModel>();

    private string _name;
    private uint _numberOfWordsPerLine = 16;
    private readonly List<uint> _numberOfWordsOptions = new List<uint>();

    // NOTE(Cristian): the initial range doesn't highlight anything :)
    private ushort _highlightAddressStart = 0xFFFF;
    private ushort _highlightAddressEnd = 0x0000;

    public ObservableCollection<MemoryWordGroupViewModel> MemoryWordGroups
    {
      get { return _memoryWordGroups; }
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

    private MemoryWordGroupViewModel _currentMemoryWordGroup;
    public MemoryWordGroupViewModel CurrentMemoryWordGroup
    {
      get { return _currentMemoryWordGroup; }
      set
      {
        _currentMemoryWordGroup = value;
        OnPropertyChanged(() => CurrentMemoryWordGroup);
      }
    }

    public ICommand ReadCommand
    {
      get { return new DelegateCommand(CopyFromDomain); }
    }

    public ICommand WriteCommand
    {
      get { return new DelegateCommand(CopyToDomain); }
    }

    public uint NumberOfWordsPerLine
    {
      get { return _numberOfWordsPerLine; }
      set
      {
        if (_numberOfWordsPerLine != value)
        {
          _numberOfWordsPerLine = value;
          OnNumerOfWordsPerLineChanged();
          OnPropertyChanged(() => NumberOfWordsPerLine);
        }
      }
    }

    public List<uint> NumberOfWordsOptions
    {
      get { return _numberOfWordsOptions; }
    }


    public MemoryViewModel(IMemory memory, string name)
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
      SelectedSection = _memorySections.Last();

      _numberOfWordsOptions.Add(1);
      _numberOfWordsOptions.Add(2);
      _numberOfWordsOptions.Add(4);
      _numberOfWordsOptions.Add(8);
      _numberOfWordsOptions.Add(16);
      _numberOfWordsOptions.Add(32);
      _numberOfWordsOptions.Add(64);
      _numberOfWordsOptions.Add(128);
      _numberOfWordsOptions.Add(256);
      _numberOfWordsOptions.Add(512);
      _numberOfWordsOptions.Add(1024);
      NumberOfWordsPerLine = 16;
    }

    private void UpdateMemoryWords()
    {
      foreach (var memoryWordGroup in _memoryWordGroups)
      {
        memoryWordGroup.UpdateMemoryWords(_selectedAddressFormat.WordFormat, _selectedValueFormat.WordFormat);
      }
    }

    private void OnSelectedSectionUpdated()
    {
      CopyFromDomain();
    }

    private void OnNumerOfWordsPerLineChanged()
    {
      CopyFromDomain();
    }

    public void MemoryWrittenHandler(ushort addressStart, ushort addressEnd)
    {

    }

    public void StepHandler()
    {
      if((_highlightAddressStart == _memory.MemoryChangedLow) &&
         (_highlightAddressEnd == _memory.MemoryChangedHigh))
      {
        return;
      }

      _highlightAddressStart = _memory.MemoryChangedLow;
      _highlightAddressEnd = _memory.MemoryChangedHigh;

      // We search for the correct section
      foreach(MemorySectionViewModel section in _memorySections)
      {
        // NOTE(Cristian): We only change section if the *whole* of the highlight range
        //                 is within the range of a section
        if((section.InitialAddress <= _highlightAddressStart) &&
           (_highlightAddressEnd <= section.FinalAddress))
        {
          // NOTE(Cristian): We use the private member because the update
          //                 would trigger the CopyFromDomain again!
          _selectedSection = section;
        }
      }

    }

    public void CopyFromDomain()
    {
      _memoryWordGroups.Clear();
      for (uint address = _selectedSection.InitialAddress; 
           address < _selectedSection.FinalAddress; 
           address+= _numberOfWordsPerLine)
      {
        uint addressStart = address;
        uint addressEnd = address + _numberOfWordsPerLine - 1;
        var group = new MemoryWordGroupViewModel(addressStart, addressEnd,
                                                 _memory,
                                                 _highlightAddressStart, _highlightAddressEnd);
        _memoryWordGroups.Add(group);
        
        // We see if we want to change the current word group
        // NOTE(Cristian): The first check is the way to set that there is no range
        //                 to highlight. Probably better to change to a flag...
        if((_highlightAddressStart <= _highlightAddressEnd) &&
           (addressStart <= _highlightAddressStart) &&
           (_highlightAddressEnd <= addressEnd))
        {
          CurrentMemoryWordGroup = group;
        }
      }

      // TODO(aaecheve): When I use the CurrentMemoryWordGroup (event for changing the scrolling)
      //                 the address words for the current visible addresses are not drawn
      //                 IDK why... :(
      UpdateMemoryWords();
    }

    private void CopyToDomain()
    {
      throw new NotImplementedException();
    }


 
  }
}