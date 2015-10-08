using System;
using System.Collections.ObjectModel;

namespace GBSharp.ViewModel
{
  public class MemoryWordGroupViewModel : ViewModelBase
  {
    private readonly ObservableCollection<MemoryWordViewModel> _memoryWords = new ObservableCollection<MemoryWordViewModel>();
    private readonly uint _firstAddress;
    private readonly uint _lastAddress;

    private string _addressRangeFormatString;

    public ObservableCollection<MemoryWordViewModel> MemoryWords
    {
      get { return _memoryWords; }
    }

    public uint FirstAddress
    {
      get { return _firstAddress; }
    }

    public uint LastAddress
    {
      get { return _lastAddress; }
    }

    public string AddressRange
    {
      get { return _addressRangeFormatString; }
    }

    public MemoryWordGroupViewModel(uint firstAddress, uint lastAddress, IMemory memory)
    {
      _firstAddress = firstAddress;
      _lastAddress = lastAddress;


      ushort lastChangedStart = memory.LastChangedStart;
      ushort lastChangedEnd = memory.LastChangedEnd;
      for (uint address = firstAddress; address <= lastAddress; address++)
      {
        bool changed = false;
        if((lastChangedStart<= address) && (address <= lastChangedEnd))
        {
          changed = true;
        }
        _memoryWords.Add(new MemoryWordViewModel(address, memory.Data[address], changed));
      }
    }

    public void UpdateMemoryWords(MemoryWordFormat addressFormat, MemoryWordFormat valueFormat)
    {
      _addressRangeFormatString = GetFormat(addressFormat, _firstAddress) + "-" + GetFormat(addressFormat, _lastAddress);
      foreach (var memoryWord in _memoryWords)
      {
        memoryWord.UpdateAddressFormat(addressFormat);
        memoryWord.UpdateValueFormat(valueFormat);
      }
      
    }

    private string GetFormat(MemoryWordFormat format, uint value, bool usePrefix = true)
    {
      switch (format)
      {
        case MemoryWordFormat.Decimal:
          return "" + value;
        case MemoryWordFormat.Hexa:
          var s = "";
          if (usePrefix)
            s = "0x";
          return s + value.ToString("x2");
        case MemoryWordFormat.Binary:
          return Convert.ToString(value, 2);

      }
      return "";
    }
  }
}