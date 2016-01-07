using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBSharp.CPUSpace.Dictionaries;

namespace GBSharp.CPUSpace
{
  class Disassembler : IDisassembler
  {
    public List<IInstruction> Disassembly { get { return _disInstructions; } }

    internal List<IInstruction> _disInstructions;
    internal HashSet<ushort> _disVisitedAddresses;
    internal Stack<ushort> _disAddressToVisit;

    public int DisassembledCount { get; private set; }
    private byte[][] _disassembledMatrix;
    public byte[][] DisassembledMatrix
    {
      get
      {
        return _disassembledMatrix;
      }
    }


    CPU _cpu;
    MemorySpace.Memory _memory;

    public Disassembler(CPU cpu, MemorySpace.Memory memory)
    {
      _cpu = cpu;
      _memory = memory;

      _disInstructions = new List<IInstruction>();
      _disVisitedAddresses = new HashSet<ushort>();
      _disAddressToVisit = new Stack<ushort>();

      _disassembledMatrix = new byte[0xFFFF][];
      for (int i = 0; i < 0xFFFF; ++i)
      {
        _disassembledMatrix[i] = new byte[5];
      }
    }

    private Instruction DisassembleInstruction(ushort address)
    {
      var inst = _cpu.FetchAndDecode(address);
      _disassembledMatrix[address][0] = inst.Length;
      _disassembledMatrix[address][4] = 1;
      switch (inst.Length)
      {
        case 1:
          _disassembledMatrix[address][1] = (byte)inst.OpCode;
          _disassembledMatrix[address][2] = 0;
          _disassembledMatrix[address][3] = 0;
          break;
        case 2:
          if (inst.CB)
          {
            _disassembledMatrix[address][1] = (byte)(inst.OpCode >> 8);
            _disassembledMatrix[address][2] = (byte)inst.OpCode;
            _disassembledMatrix[address][3] = 0;
          }
          else
          {
            _disassembledMatrix[address][1] = (byte)inst.OpCode;
            _disassembledMatrix[address][2] = (byte)inst.Literal;
            _disassembledMatrix[address][3] = 0;
          }
          break;
        case 3:
          _disassembledMatrix[address][1] = (byte)inst.OpCode;
          _disassembledMatrix[address][2] = (byte)(inst.Literal >> 8);
          _disassembledMatrix[address][3] = (byte)inst.Literal;
          break;
        default:
          return null;
      }

      return inst;
    }

    public void PoorManDisassemble()
    {
      // We mark all the addresses as unvisited
      for (int i = 0; i < 0xFFFF; ++i)
      {
        _disassembledMatrix[i][4] = 0;
      }

      LinkedList<ushort> visitQueue = new LinkedList<ushort>();
      visitQueue.AddFirst(0x100);
      while (visitQueue.Count > 0)
      {
        ushort address = visitQueue.First.Value;
        visitQueue.RemoveFirst();

        // We check that we didn't visit this address before
        if(_disassembledMatrix[address][4] != 0) { continue; }

        // We disassemble it and add it to out map
        Instruction inst = DisassembleInstruction(address);
        // Invalid address
        if (inst == null) { continue; }

        // This are returns to functions
        if (_stoppers.Contains(inst.OpCode))
        {
          // If full disassemble, should add the next instruction
          // to the second priority queue
          continue;
        }

        // Direct Jumps. Conditionals add the next instruction
        else if (_directJumps.ContainsKey(inst.OpCode))
        {
          if (_directJumps[inst.OpCode])
          {
            visitQueue.AddFirst(inst.Literal);
          }
          else
          {
            visitQueue.AddFirst(inst.Literal);
            ushort next = (ushort)(address + inst.Length);
            visitQueue.AddFirst(next);
          }
        }
        // Relative Jumps. Conditionals add the next instruction
        else if (_relativeJumps.ContainsKey(inst.OpCode))
        {
          if (_relativeJumps[inst.OpCode])
          {
            sbyte signedLiteral;
            unchecked { signedLiteral = (sbyte)inst.Literal; }
            ushort relTarget = (ushort)(inst.Address + signedLiteral + inst.Length);
            visitQueue.AddFirst(relTarget);
          }
          else
          {
            sbyte signedLiteral;
            unchecked { signedLiteral = (sbyte)inst.Literal; }
            ushort relTarget = (ushort)(inst.Address + signedLiteral + inst.Length);
            visitQueue.AddFirst(relTarget);

            var next = (ushort)(address + inst.Length);
            visitQueue.AddFirst(next);
          }
        }

        // Restarts are a direct jump
        else if (_restarts.ContainsKey(inst.OpCode))
        {
          visitQueue.AddFirst(_restarts[inst.OpCode]);
        }
        else
        {
          // It's a normal instruction and simply continues
          ushort target = (ushort)(address + inst.Length);
          visitQueue.AddFirst(target);
        }
      }
    }

    HashSet<ushort> _stoppers = GetShowStoppers();
    Dictionary<ushort, bool> _directJumps = GetDirectJumps();
    Dictionary<ushort, bool> _relativeJumps = GetRelativeJumps();
    Dictionary<ushort, ushort> _restarts = GetRestarts();

    /// <summary>
    /// Poor man's dissambly (for now)
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IInstruction> Disassamble(ushort startAddress, bool permissive = true)
    {
      // TODO(Cristian): Complete the on-demand disassembly
      var stoppers = GetShowStoppers();
      var dirJumps = GetDirectJumps();
      var relJumps = GetRelativeJumps();
      var restarts = GetRestarts();

      _disAddressToVisit.Push(startAddress);     // Initial address

      while (_disAddressToVisit.Count > 0)
      {
        ushort address = _disAddressToVisit.Pop();
        // If we already saw this address, we move on
        if (_disVisitedAddresses.Contains(address)) { continue; }
        _disVisitedAddresses.Add(address);

        // NOTE(Cristian): The 0xCB external opcodes all exists, so we just need to check
        //                 that the first byte is 0xCB to know that the instruction is valid
        if (CPUInstructionLengths.Get((byte)_memory.Read(address)) != 0)
        {
          try
          {
            // Get the instruction and added to the instruction list
            var inst = _cpu.FetchAndDecode(address);
            _disInstructions.Add(inst);

            // We get a list of possible next addresses to check
            var candidates = new List<ushort>();

            // A show-stopper doesn't add any more instructions
            if (stoppers.Contains(inst.OpCode)) { continue; }

            if (dirJumps.ContainsKey(inst.OpCode))
            {
              //if(dirJumps[inst.OpCode])
              //{
              //  candidates.Add(inst.Literal);
              //}
              //// If permissive, we also permit conditional jumps (ant the next inst)
              //else if(permissive)
              //{
              //  candidates.Add(inst.Literal);
              //  var next = (ushort)(address + inst.Length);
              //  candidates.Add(next);
              //}
            }

            else if (relJumps.ContainsKey(inst.OpCode))
            {
              //if(relJumps[inst.OpCode])
              //{
              //  sbyte signedLiteral;
              //  unchecked { signedLiteral = (sbyte)inst.Literal; }
              //  ushort target = (ushort)(inst.Address + signedLiteral + inst.Length);
              //  candidates.Add(target);
              //}
              //else if(permissive)
              //{
              //  sbyte signedLiteral;
              //  unchecked { signedLiteral = (sbyte)inst.Literal; }
              //  ushort target = (ushort)(inst.Address + signedLiteral + inst.Length);
              //  candidates.Add(target);

              //  var next = (ushort)(address + inst.Length);
              //  candidates.Add(next);
              //}
            }
            else if(restarts.ContainsKey(inst.OpCode))
            {
              candidates.Add(restarts[inst.OpCode]);
            }
            else // It's an instruction that continues
            {
              ushort target = (ushort)(address + inst.Length);
              candidates.Add(target);
            }

            // We add the candidate instructions into the list
            foreach (var candidateNextInstruction in candidates)
            {
              // If any of the candidates was already visited, we do not visit it 
              if(_disVisitedAddresses.Contains(candidateNextInstruction)) 
              { 
                continue; 
              }
              _disAddressToVisit.Push(candidateNextInstruction);
            }

          }
          catch (Exception)
          {
           
          }
        }

      }
      return _disInstructions.OrderBy(i => i.Address);
    }

    /// <summary>
    /// Return the jumps, classified by whether,
    /// the next instruction should not be followed
    /// (its an unconditional jump)
    /// </summary>
    /// <returns></returns>
    private static Dictionary<ushort, bool> GetDirectJumps()
    {
      // Instructions in this set
      var jumps = new Dictionary<ushort, bool>();
      // JP nn 
      jumps.Add(0xC3, true);
      // CALL nn
      jumps.Add(0xCD, false);
      // JP NZ, JP Z, JP NC, JP C
      jumps.Add(0xC2, false);
      jumps.Add(0xCA, false);
      jumps.Add(0xD2, false);
			jumps.Add(0xDA, false);
      // CALL NZ, CALL Z, CALL NC, CALL C       
			jumps.Add(0xC4, false);
			jumps.Add(0xCC, false);
			jumps.Add(0xD4, false);
			jumps.Add(0xDC, false);

      return jumps;
    }

    /// <summary>
    /// Return the relatives jump, classified by whether,
    /// the next instruction should not be followed
    /// (its an unconditional jump)
    /// </summary>
    /// <returns></returns>
    private static Dictionary<ushort, bool> GetRelativeJumps()
    {
      var jumps = new Dictionary<ushort, bool>();
      jumps.Add(0x18, true);    // JR n
      jumps.Add(0x20, false);   // JR NZ, n
      jumps.Add(0x28, false);   // JR Z, n
      jumps.Add(0x30, false);   // JR NC, n
      jumps.Add(0x38, false);   // JR C, n
      return jumps;
    }

    /// <summary>
    /// These are the instructions from which the disassembler
    /// *stricly* speaking cannot continue disassembling, because
    /// there is no guarantee that the next instruction will ever be called
    /// (and perhaps the program depends on that fact)
    /// </summary>
    /// <returns>HashSet of the instructions that stop the disassembler</returns>
    private static HashSet<ushort> GetShowStoppers()
    {
      // Instructions in this set
      // RET NZ, RET Z, RET NC, RET C,      
			// RET, RETI
			// JP (HL)
      var jumps = new HashSet<ushort>() { 
        0xC0, 0xC8, 0xD0, 0xD8, 
        0xC9, 0xD9, 
        0xE9, 
      };
      return jumps;
    }

    private static Dictionary<ushort, ushort> GetRestarts()
    {
      var restarts = new Dictionary<ushort, ushort>();
      restarts.Add(0xC7, 0x00);   // RST 00
      restarts.Add(0xCF, 0x08);   // RST 08
      restarts.Add(0xD7, 0x10);   // RST 10
      restarts.Add(0xDF, 0x18);   // RST 18
      restarts.Add(0xE7, 0x20);   // RST 20
      restarts.Add(0xEF, 0x28);   // RST 28
      restarts.Add(0xF7, 0x30);   // RST 30
      restarts.Add(0xFF, 0x38);   // RST 38
      return restarts;
    }


  }
}
