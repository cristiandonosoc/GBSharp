using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GBSharp.Cartridge;
using GBSharp.MemorySpace.MemoryHandlers;

namespace GBSharpTest.MemorySpace.MemoryHandlers
{
  [TestClass]
  public class RomOnlyMemoryHandlerTests
  {
     /* TODO(Wooo): Fix this test
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
    */

    /* TODO(Wooo): Fix this test
    [TestMethod]
    public void RomOnlyMemoryHandlerDoestWriteInOtherSections()
    {
      // Arrange
      Cartridge cartridge = new Cartridge();
      cartridge.Load(new byte[65536]);
      RomOnlyMemoryHandler handler = new RomOnlyMemoryHandler(cartridge);
      handler.LoadInternalMemory(new byte[65536]);
      ushort[] addresses = {
                             0x8000,
                             0x9010,
                             0xA100,
                             0xB000,
                             0xC468,
                             0xD000,
                             0xFFFF
                           };
      byte writeValue = 0x22;
      // Act
      foreach (ushort address in addresses)
      {
        handler.Write(address, writeValue);
      }

      // Assert
      foreach (ushort address in addresses)
      {
        Assert.AreEqual<byte>(writeValue, handler.Read(address));
      }
    }
    */
  }
}