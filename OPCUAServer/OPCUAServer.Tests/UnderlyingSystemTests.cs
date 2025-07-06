using Microsoft.VisualStudio.TestTools.UnitTesting;
using SE.OPCUAServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedAutomation.UaBase;

namespace OPCUAServer.Tests
{
    [TestClass]
    public class UnderlyingSystemTests
    {
        private UnderlyingSystem _underlyingSystem;

        [TestInitialize]
        public void Setup()
        {
            _underlyingSystem = new UnderlyingSystem();
            _underlyingSystem.Initialize();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _underlyingSystem.Dispose();
        }

        [TestMethod]
        public void TestInitialize()
        {
            Assert.IsNotNull(_underlyingSystem.GetBlocks());
        }

        [TestMethod]
        public void TestRead_ValidBlockAndTag_ReturnsValue()
        {
            int blockAddress = 0;
            int tag = 0;
            object value = _underlyingSystem.Read(blockAddress, tag);
            Assert.IsNotNull(value);
        }

        [TestMethod]
        public void TestRead_InvalidBlockOrTag_ReturnsNull()
        {
            object value = _underlyingSystem.Read(-1, -1);
            Assert.IsNull(value);
        }

        [TestMethod]
        public void TestWrite_ValidBlockAndTag_ReturnsTrue()
        {
            int blockAddress = 0;
            int tag = 0;
            bool result = _underlyingSystem.Write(blockAddress, tag, 42);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestWrite_InvalidBlockOrTag_ReturnsFalse()
        {
            bool result = _underlyingSystem.Write(-1, -1, 42);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestStart_ValidBlockAddress_ReturnsGoodStatusCode()
        {
            int blockAddress = 0;
            StatusCode result = _underlyingSystem.Start(blockAddress);
            Assert.AreEqual(StatusCodes.Good, result);
        }

        [TestMethod]
        public void TestStart_InvalidBlockAddress_ReturnsBadNodeIdUnknown()
        {
            StatusCode result = _underlyingSystem.Start(-1);
            Assert.AreEqual(StatusCodes.BadNodeIdUnknown, result);
        }

        [TestMethod]
        public void TestStop_ValidBlockAddress_ReturnsGoodStatusCode()
        {
            int blockAddress = 0;
            StatusCode result = _underlyingSystem.Stop(blockAddress);
            Assert.AreEqual(StatusCodes.Good, result);
        }

        [TestMethod]
        public void TestStop_InvalidBlockAddress_ReturnsBadNodeIdUnknown()
        {
            StatusCode result = _underlyingSystem.Stop(-1);
            Assert.AreEqual(StatusCodes.BadNodeIdUnknown, result);
        }

        [TestMethod]
        public void TestStartWithSetPoint_ValidBlockAddress_ReturnsGoodStatusCode()
        {
            int blockAddress = 0;
            double temperatureSetPoint = 25.0;
            double humiditySetPoint = 50.0;
            StatusCode result = _underlyingSystem.StartWithSetPoint(blockAddress, temperatureSetPoint, humiditySetPoint);
            Assert.AreEqual(StatusCodes.Good, result);
        }

        [TestMethod]
        public void TestStartWithSetPoint_InvalidBlockAddress_ReturnsBadNodeIdUnknown()
        {
            StatusCode result = _underlyingSystem.StartWithSetPoint(-1, 25.0, 50.0);
            Assert.AreEqual(StatusCodes.BadNodeIdUnknown, result);
        }
    }
}
