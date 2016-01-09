﻿using GBSharp.Cartridge;
using System;
using System.Collections.Generic;

namespace GBSharp
{
  public interface IGameBoy
  {
    event Action StepCompleted;
    event Action FrameCompleted;
    event Action PauseRequested;
    event Action StopRequested;

    void LoadCartridge(string filename, byte[] cartridgeData);

    void Run();
    void Reset();
    void Pause();
    void Stop();
    void Step(bool ignoreBreakpoints);

    void PressButton(Keypad button);
    void ReleaseButton(Keypad button);
    bool ReleaseButtons { get; set; }

    // Components
    ICPU CPU { get; }
    IMemory Memory { get; }
    ICartridge Cartridge { get; }
    IDisplay Display { get; }
    IAPU APU { get; }
    IDisassembler Disassembler { get; }

    Dictionary<MemorySpace.MMR, ushort> GetRegisterDic();

    uint[] ScreenFrame { get; }
    object LockObj { get; }

    double FPS { get; }
    void Dispose();
    
  }
}