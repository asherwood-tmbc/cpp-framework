using System;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.Diagnostics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework
{
    /// <summary>
    /// Unit tests for the <see cref="GuidGeneratorService"/> class.
    /// 
    /// Test GUID generated for the following:
    ///  1) NOT a new GUID object (with all zeros)
    ///  2) The formatting of GUID ToString() is hypenated 32 digits (00000000-0000-0000-0000-000000000000)
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
    public class GuidGeneratorServiceTests
    {
        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void NewGuidTest()
        {
            var actual = GuidGeneratorService.Current.NewGuid();
            var notExpected = new Guid(); // Just making sure it is not an all zero GUID

            Console.WriteLine(actual);
            Verify.AreEqual(actual, Guid.ParseExact(actual.ToString(), "D"));
            Verify.AreNotEqual(notExpected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void NewGuidTestWithTargetObject()
        {
            object mockTarget = new object();
            var actual = GuidGeneratorService.Current.NewGuid(mockTarget);
            var notExpected = new Guid(); // Just making sure it is not an all zero GUID

            Console.WriteLine(actual);
            Verify.AreEqual(actual, Guid.ParseExact(actual.ToString(), "D"));
            Verify.AreNotEqual(notExpected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void NewGuidTestWithNullTargetObject()
        {
            object mockTarget = null;
            var actual = GuidGeneratorService.Current.NewGuid(mockTarget);
            var notExpected = new Guid(); // Just making sure it is not an all zero GUID

            Console.WriteLine(actual);
            Verify.AreEqual(actual, Guid.ParseExact(actual.ToString(), "D"));
            Verify.AreNotEqual(notExpected, actual);
        }
    }
}