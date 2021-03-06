﻿using System.Windows;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
    public class GameBoyViewModel : ViewModelBase
    {
        private IDispatcher _dispatcher;
        private readonly IWindow _window;
        private readonly IKeyboardHandler _keyboardHandler;
        private readonly IGameBoy _gameBoy;

        private readonly MemoryViewModel _memory;
        private readonly CPUViewModel _cpu;
        private readonly InterruptManagerViewModel _interrupt;
        private readonly IORegistersManagerViewModel _ioRegisters;
        private readonly SoundChannelInternalsViewModel _soundChannelInternals;
        private readonly DisplayViewModel _display;
        private readonly GameBoyContollerViewModel _gameBoyController;
        private readonly GameBoyGamePadViewModel _gameBoyGamePad;
        private readonly DissasembleViewModel _dissasemble;
        private readonly BreakpointsViewModel _breakpoints;
        private readonly InstructionHistogramViewModel _instructionHistogram;
        private readonly APUViewModel _apu;
        private readonly MemoryImageViewModel _memoryImage;
        private readonly SoundRecordingViewModel _soundRecording;
        private readonly ControlsViewModel _controls;

        public MemoryViewModel Memory { get { return _memory; } }
        public CPUViewModel CPU { get { return _cpu; } }
        public InterruptManagerViewModel Interrupt { get { return _interrupt; } }
        public DisplayViewModel Display { get { return _display; } }
        public GameBoyContollerViewModel GameBoyController { get { return _gameBoyController; } }
        public GameBoyGamePadViewModel GameBoyGamePad { get { return _gameBoyGamePad; } }
        public DissasembleViewModel Dissasemble { get { return _dissasemble; } }
        public BreakpointsViewModel Breakpoints { get { return _breakpoints; } }
        public IORegistersManagerViewModel IORegisters { get { return _ioRegisters; } }
        public SoundChannelInternalsViewModel SoundChannelInternals { get { return _soundChannelInternals; } }
        public InstructionHistogramViewModel InstructionHistogram { get { return _instructionHistogram; } }
        public APUViewModel APU { get { return _apu; } }
        public MemoryImageViewModel MemoryImage { get { return _memoryImage; } }
        public SoundRecordingViewModel SoundRecording { get { return _soundRecording; } }
        public ControlsViewModel Controls { get { return _controls; } }

        private readonly ButtonMapping _buttonMapping;

        public GameBoyViewModel(IGameBoy gameBoy, IDispatcher dispatcher, IWindow window, 
                                IOpenFileDialogFactory fileDialogFactory, 
                                IKeyboardHandler keyboardHandler)
        {
            _gameBoy = gameBoy;
            _dispatcher = dispatcher;
            _window = window;
            _keyboardHandler = keyboardHandler;
            _keyboardHandler.KeyDown += OnKeyDown;
            _keyboardHandler.KeyUp += OnKeyUp;
            _window.OnClosing += HandleClosing;

            _buttonMapping = new ButtonMapping();

            _memory = new MemoryViewModel(_gameBoy.Memory, "Memory View");
            _cpu = new CPUViewModel(_gameBoy, _dispatcher);

            // TODO(aaecheve): Should this be another function handling this?
            _gameBoy.CPU.BreakpointFound += BreakpointHandler;
            _gameBoy.CPU.InterruptHappened += InterruptHandler;
            _gameBoy.ErrorEvent += _gameBoy_ErrorEvent;

            _interrupt = new InterruptManagerViewModel(_gameBoy, _dispatcher);
            _ioRegisters = new IORegistersManagerViewModel(_gameBoy, _dispatcher);
            _soundChannelInternals = new SoundChannelInternalsViewModel(_gameBoy);
            _display = new DisplayViewModel(_gameBoy, _gameBoy.Display, _gameBoy.Memory, _dispatcher);
            _gameBoyGamePad = new GameBoyGamePadViewModel(_gameBoy, _dispatcher);
            _breakpoints = new BreakpointsViewModel(_gameBoy);
            _dissasemble = new DissasembleViewModel(_breakpoints, _gameBoy);
            _instructionHistogram = new InstructionHistogramViewModel(_gameBoy, _dispatcher);
            _apu = new APUViewModel(_gameBoy, _dispatcher);
            _memoryImage = new MemoryImageViewModel(_gameBoy, _dispatcher);
            _soundRecording = new SoundRecordingViewModel(_gameBoy);
            _controls = new ControlsViewModel(this, _buttonMapping);

            // Gameboy Controller events
            _gameBoyController = new GameBoyContollerViewModel(_gameBoy, fileDialogFactory, _breakpoints);
            _gameBoyController.OnFileLoaded += FileLoadedHandler;
            _gameBoyController.OnStep += StepHandler;
            _gameBoyController.OnRun += RunHandler;
            _gameBoyController.OnPause += PauseHandler;
        }

        private void _gameBoy_ErrorEvent(string message)
        {
            MessageBox.Show(message);
        }

        public void OnClosed()
        {
            _gameBoyController.OnClosed();
        }

        private void OnKeyUp(KeyEventArgs args)
        {
            if (_controls.SetMode)
            {
                _controls.SetMapping(args.Key);
            }
            else
            {
                _gameBoyGamePad.KeyUp(_buttonMapping, args);
            }
        }

        private void OnKeyDown(KeyEventArgs args)
        {
            if (_controls.SetMode) { return; }
            // NOTE(Cristian): Very hacky F10 step
            if (args.Key == Key.System)
            {
                if (args.SystemKey == Key.F10)
                {
                    _gameBoyController.Step();
                }
            }

            _gameBoyGamePad.KeyDown(_buttonMapping, args);
        }

        private void InterruptHandler(GBSharp.CPUSpace.Interrupts interrupt)
        {
            // TODO(Cristian, aaecheve): Do something special with each interrupt?
            _dispatcher.Invoke(StepHandler);
        }

        private void BreakpointHandler()
        {
            // NOTE(Cristian): This handler comes from an event from the _gameboy, thus
            //                 it happens in the gameboy's thread. If we want to modify UI
            //                 constructs, we need to run them in the UI thread
            _dispatcher.Invoke(StepHandler);
        }

        // TODO(Cristian): Check if every viewmodel needs te step event or can it be all handled here
        private void StepHandler()
        {
            _cpu.CopyFromDomain();
            _ioRegisters.CopyFromDomain();
            _display.CopyFromDomain();
            _dissasemble.SetCurrentInstructionToPC();
            _gameBoyController.SetCurrentInstructionToPC();
            _interrupt.CopyFromDomain();
            _memory.StepHandler();
            _breakpoints.StepHandler();
            _soundChannelInternals.Reload();
        }

        private void FileLoadedHandler()
        {
            _ioRegisters.CopyFromDomain();
            _memory.CopyFromDomain();
            _display.CopyFromDomain();
            _cpu.CopyFromDomain();
            _dissasemble.DissasembleCommandWrapper();
            _gameBoyController.DissasembleCommandWrapper();
            _soundChannelInternals.Reload();
            _soundRecording.CartridgeLoaded = true;
        }

        private void HandleClosing()
        {
            _display.Dispose();
            _interrupt.Dispose();
            _ioRegisters.Dispose();
            _gameBoyGamePad.Dispose();
            _instructionHistogram.Dispose();
            _apu.Dispose();
            _memoryImage.Dispose();
            _cpu.Dispose();
            _gameBoyController.OnFileLoaded -= FileLoadedHandler;
            _gameBoyController.OnStep -= StepHandler;
            _gameBoyController.Dispose();
            _gameBoy.Stop();

            _window.OnClosing -= HandleClosing;
        }

        private void RunHandler()
        {
            _breakpoints.RunHandler();
            _dissasemble.ClearCurrentInstruction();
            _gameBoyController.ClearCurrentInstruction();
            // TODO(Cristian): Clear the other components?
        }

        private void PauseHandler()
        {
            _cpu.CopyFromDomain();
            _ioRegisters.CopyFromDomain();
            _interrupt.CopyFromDomain();
            _dissasemble.SetCurrentInstructionToPC();
            _gameBoyController.SetCurrentInstructionToPC();
            _soundChannelInternals.Reload();
        }
    }
}