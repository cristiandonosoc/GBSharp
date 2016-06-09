using System;
using System.Collections.Generic;

namespace GBSharp
{
    public delegate void ErrorDelegate(string message);
    public interface IGameBoy
    {
        event Action StepCompleted;
        event Action FrameCompleted;
        event Action PauseRequested;
        event Action StopRequested;
        event ErrorDelegate ErrorEvent;

        bool LoadCartridge(string filename, byte[] cartridgeData);

        void Run();
        void Reset();
        void Pause();
        void Stop();
        void Step(bool ignoreBreakpoints);

        void SaveState();
        void LoadState();

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

        double FPS { get; }
        void Dispose();

        ulong DebugTickCounter { get; set; }
    }
}