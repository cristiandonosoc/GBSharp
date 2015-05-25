using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using GBSharp.MemorySpace.MemoryHandlers;

namespace GBSharpTest.CPU
{
  [TestClass]
  public class CPUTest
  {
    // NOTE(Cristián): We need the damned accessor
    // because it will be called to assing the context
    // Microsoft, nice way of not documenting it!
    private TestContext testContext
    public TestContext TestContext
    {
      get{ return testContext }
      set{ testContext = value; }
    }        

    [DataSource(
      "Microsoft.VisualStudio.TestTools.DataSource.CSV",
      "|DataDirectory|\\CPU\\tests.csv",
      "tests#csv", 
      DataAccessMethod.Sequential),
    DeploymentItem("CPU\\tests.csv"),
    TestMethod]
    public void TestMethod1()
    {
      // Arrange
      DataRow row = TestContext.DataRow;
      byte val1 = Convert.ToByte(row["COLUMN1"] as String, 16);
      byte val2 = Convert.ToByte(row["COLUMN1"] as String, 16);
    }
  }
}
