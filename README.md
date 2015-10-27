# GBSharp
A whole Gameboy Emulator in C#

The project intends to create a simple gameboy's interface to be usable in more contexts that plain playing.
Hence, if you look to the gameboy interface in the GBSharp project, you can see several "debug" functionalities
that are helpful in exploring other sides of the Gameboy (disassembly, separated bitmaps for each step of display).

The UI is implemented in WPF (the main reason for choosing C#) is very much a work in progress,
but manages to show several of the core functionalities:
- Breakpoints on address and interrupt
- Disassembly and step through disassembly
- Show display timing while stepping
- Show all separated display steps (background, window, sprites)
- Keep track of the last changed memory address

The emulator itself is being heavily worked on and game support is *extremely* low. For now, only tetris is playable without issues.

Installation
------------

The project should work out of the box. Just clone and run.
