using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GBSharp.Catridge;
using GBSharp.Memory.MemoryHandlers;

namespace GBSharpTest.Memory.MemoryHandlers
{
  [TestClass]
  public class RomOnlyMemoryHandlerTests
  {
    [TestMethod]
    public void RomOnlyMemoryHandlerDoesntWriteInRomSection()
    {
      // Arrange
      Cartridge cartridge = new Cartridge();
      cartridge.Load(new byte[65536]);
      RomOnlyMemoryHandler handler = new RomOnlyMemoryHandler(cartridge);
      handler.LoadInternalMemory(new byte[65536]);
      ushort[] addresses = {
                             0x0000,
                             0x0010,
                             0x0100,
                             0x1000,
                             0x2468,
                             0x5000,
                             0x7FFF
                           };
      // Act
      foreach (ushort address in addresses)
      {
        handler.Write(address, 0x22);
      }

      // Assert
      foreach (ushort address in addresses)
      {
        Assert.AreEqual<byte>(0, handler.Read(address));
      }
    }
  }
}
