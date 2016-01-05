using GBSharp.Cartridge;
using System;
using System.Collections.Generic;

namespace GBSharp
{
  public interface IGameBoy
  {
    event Action StepCompleted;
    event Action FrameCompleted;
    ICPU CPU { get; }
    IMemory Memory { get; }
    ICartridge Cartridge { get; }
    IDisplay Display { get; }
    IAPU APU { get; }
    void LoadCartridge(string filename, byte[] cartridgeData);
    void Run();
    void Pause();
    void Stop();
    void Step(bool ignoreBreakpoints);
    void PressButton(Keypad button);
    void ReleaseButton(Keypad button);
    bool ReleaseButtons { get; set; }

    Dictionary<MemorySpace.MMR, ushort> GetRegisterDic();

    IEnumerable<IInstruction> Disassamble(ushort startAddress, 
                                          bool permissive = true);

    uint[] ScreenFrame { get; }
    object LockObj { get; }

    double FPS { get; }

    void Dispose();
    
  }
}