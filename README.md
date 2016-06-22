# GBSharp
A whole Gameboy Emulator in C#

The project intends to create a simple gameboy's interface to be usable in more contexts that plain playing.
Hence, if you look to the gameboy interface in the GBSharp project, you can see several "debug" functionalities
that are helpful in exploring other sides of the Gameboy (disassembly, separated bitmaps for each step of display).

The UI is implemented in WPF (the main reason for choosing C#).
The emulator manages to show several of the core functionalities:
- Breakpoints on address and interrupt
- Disassembly and step through disassembly (break on execute, read, write and jump)
- Show display timing while stepping
- Show all separated display steps (background, window, sprites)
- Keep track of the last changed memory address
- Snapshot save/restore

The project is mainly separated in three parts (though it consists of 5 C# projects):
- GBSharp: It is the emulator per se. The API is intented that this project has no dependencies,
so that it can be used for other views (text-based, web, you name it)
- GBSharp.View(Model): WPF view for the emulator
- GBShart.Test: Initial testing project that was abandoned in favor of the great testing ROMs.

Emulator Accuracy
-----------------
GBSharp passes the following test roms.
- CPU instructions
- Instruction timing
- Memory timing

Sound tests passes quite a few of them

Installation
------------

The project should work out of the box. Just clone and run.
The main project is GBSharp.View

There is a binary in the bin/ directory (windows only).

Current Status
--------------
Wait! Are there issues?!

Yeah... Quite a few of them actually...

The purpose of GB# was to learn how to do an emulator, objective which was more than accomplished.
I actually implemented much more than what I initially set out to do.  

GB# is now in the project phase where all the features are implemented but now requires a considerable amount
of energy to fix all the issues that arise with compatibility: essentially every game out there works as a testcase
for some obscure hardware kink. Seeing as there are a couple of emulators that have been developed far longer (and by more people),
I figured that there is no real usecase for GB#, as there are emulators with greater compatibility (i.e. Virtualboy, BGB).

So I decided to leave the project until this point. 

Continue?
---------
If you're interested in working in compatibility (or plain learning gameboy emulation) I can tell you this much:

GB# is among the most understandable emulators out there. I had to look quite a few to find out how some weird kink
was developed and I feel this one is much easier to navigate and read (then again I wrote it, so take that with a grain of salt). 

In any case, I'd gladly share what I have learn in the process to anyone interested.
