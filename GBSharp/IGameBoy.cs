using GBSharp.Cartridge;
using System;
using System.Collections.Generic;

namespace GBSharp
{
  public interface IGameBoy
  {
    event Action StepFinished;
    ICPU CPU { get; }
    IMemory Memory { get; }
    ICartridge Cartridge { get; }
    IDisplay Display { get; }
    void LoadCartridge(byte[] cartridgeData);
    void Run();
    void Pause();
    void Stop();
    void Step(bool ignoreBreakpoints);
    void PressButton(Keypad button);
    void ReleaseButton(Keypad button);

    Dictionary<MemorySpace.MemoryMappedRegisters, ushort> GetRegisterDic();
  }
}