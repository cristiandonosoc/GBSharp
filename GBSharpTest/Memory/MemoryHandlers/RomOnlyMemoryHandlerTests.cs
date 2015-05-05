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
    public void RomOnlyMemoryHandlerWriteWorks()
    {
      // Arrange
      Cartridge cartridge = new Cartridge();
      cartridge.Load(new byte[65536]);
      RomOnlyMemoryHandler handler = new RomOnlyMemoryHandler(cartridge);
      handler.LoadInternalMemory(new byte[65536]);

      // Act
      handler.Write(0x1001, 0x22);

      // Assert
      Assert.AreEqual<byte>(0x22, handler.Read(0x1001));
    }
  }
}
