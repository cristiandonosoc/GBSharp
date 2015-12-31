using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using GBSharp.MemorySpace;
using GBSharp.CPUSpace;
using GBSharp;

namespace GBSharpTest.CPUSpace
{
  [TestClass]
  public class CPUTest
  {
    #region HELPER METHODS

    internal byte GetByte(Object element)
    {
      return Convert.ToByte((String)element, 16);
    }

    internal ushort GetUShort(Object element)
    {
      return Convert.ToUInt16((String)element, 16);
    }

    #endregion

    // NOTE(Cristián): We need the damned accessor
    // because it will be called to assing the context
    // Microsoft, nice way of not documenting it!
    private TestContext testContext;
    public TestContext TestContext
    {
      get { return testContext; }
      set{ testContext = value; }
    }        

    [DataSource(
      "Microsoft.VisualStudio.TestTools.DataSource.CSV",
      "|DataDirectory|\\CPU\\tests.csv",
      "tests#csv", 
      DataAccessMethod.Sequential),
    DeploymentItem("CPU\\tests.csv"),
    TestMethod]
    public void TestInstructionsViaExcel()
    {
      // Arrange
      var gameboy = new GameBoy();
      var memory = (Memory)gameboy.Memory;
      var cpu = (CPU)gameboy.CPU;

      var row = TestContext.DataRow;
      LoadRow(row, cpu, memory, gameboy);

      // Act
      var steps = (int)row["Steps"];
      for (int i = 0;
           i < steps;
           i++)
      {
        cpu.Step(true);
      }

      // Assert
      TestFlagsAndMemory(row, cpu);
    }

    internal void LoadRow(DataRow row, CPU cpu, Memory memory, GameBoy gameboy)
    {
      // We load the registers
      cpu.registers.A = GetByte(row["Ai"]);
      cpu.registers.F = GetByte(row["Fi"]);
      cpu.registers.B = GetByte(row["Bi"]);
      cpu.registers.C = GetByte(row["Ci"]);
      cpu.registers.D = GetByte(row["Di"]);
      cpu.registers.E = GetByte(row["Ei"]);
      cpu.registers.H = GetByte(row["Hi"]);
      cpu.registers.L = GetByte(row["Li"]);
      cpu.registers.PC = GetUShort(row["PCi"]);
      cpu.registers.SP = GetUShort(row["SPi"]);

      // We load the rom
      ushort memoryAddress = 0x0000;
      int columnCount = row.Table.Columns.Count;
      int romColumnIndex = row.Table.Columns["ROM"].Ordinal;
      
      // Create a dummy cartridge
      var cartridgeData = new byte[columnCount - romColumnIndex];
      
      for (int columnIndex = romColumnIndex;
          columnIndex < columnCount;
          columnIndex++, memoryAddress++)
      {
        var element = row[columnIndex];
        if (element is System.DBNull) { break; }
        var value = Convert.ToByte((String)element, 16);
        cartridgeData[memoryAddress] = value;
      }

      gameboy.LoadCartridge("test", cartridgeData);
    }

    internal void TestFlagsAndMemory(DataRow row, CPU cpu)
    {
      var testName = (String)row["Test"];

      // We test the flags
      Assert.AreEqual<byte>(GetByte(row["Af"]), cpu.registers.A, testName + " Register A");
      Assert.AreEqual<byte>(GetByte(row["Ff"]), cpu.registers.F, testName + " Register F");
      Assert.AreEqual<byte>(GetByte(row["Bf"]), cpu.registers.B, testName + " Replace B");
      Assert.AreEqual<byte>(GetByte(row["Cf"]), cpu.registers.C, testName + " Replace C");
      Assert.AreEqual<byte>(GetByte(row["Df"]), cpu.registers.D, testName + " Replace D");
      Assert.AreEqual<byte>(GetByte(row["Ef"]), cpu.registers.E, testName + " Replace E");
      Assert.AreEqual<byte>(GetByte(row["Hf"]), cpu.registers.H, testName + " Replace H");
      Assert.AreEqual<byte>(GetByte(row["Lf"]), cpu.registers.L, testName + " Replace L");
      Assert.AreEqual<ushort>(GetUShort(row["PCf"]), cpu.registers.PC, testName + " PC");
      Assert.AreEqual<ushort>(GetUShort(row["SPf"]), cpu.registers.SP, testName + " SP");

      // We test the memory address
      var testMemoryAddress = GetUShort(row["TestAddr"]);
      var testValue = GetByte(row["TestValue"]);
      Assert.AreEqual<byte>(
        cpu.memory.Read(testMemoryAddress),
        testValue,
        testName + " Memory Test");
    }
  }
}
