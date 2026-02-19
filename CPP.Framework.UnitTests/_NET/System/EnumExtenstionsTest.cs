using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CPP.Framework.Diagnostics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace

namespace System
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class EnumExtenstionsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetDescriptionInvalidEnumTest()
        {
            var actual = ((TestEnum) (-1)).GetDescription();
        }


        [TestMethod]
        public void GetDescriptionTest()
        {
            var expected = "Test Value 1";
            var actual = TestEnum.TestValue1.GetDescription();

            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetSingleFlagDescriptionTest()
        {
            var expected = "F 1";
            var actual = ((TestFlags) 1).GetDescription("#");

            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetMultipleFlagDescriptionWithDefaultSeperatorTest()
        {
            var expected = "F 1,F 2,F 3";
            var actual = ((TestFlags) 7).GetDescription();

            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetMultipleFlagDescriptionTest()
        {
            var expected = "F 1#F 2#F 3";
            var actual = ((TestFlags) 7).GetDescription("#");

            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetEditorBrowsableStateWithInvalidEnum()
        {
            ((TestFlags) (-1)).GetEditorBrowsableState();
        }

        [TestMethod]
        public void GetEditorBrowsableState()
        {
            Verify.AreEqual(EditorBrowsableState.Always, TestEnum.TestValue1.GetEditorBrowsableState());
            Verify.AreEqual(EditorBrowsableState.Advanced, TestEnum.TestValue2.GetEditorBrowsableState());
            Verify.AreEqual(EditorBrowsableState.Always, TestEnum.TestValue3.GetEditorBrowsableState());
            Verify.AreEqual(EditorBrowsableState.Never, TestEnum.TestValue4.GetEditorBrowsableState());
        }

        [TestMethod]
        public void GetEditorBrowsableStateWithCombinedFlags()
        {
            Verify.AreEqual(EditorBrowsableState.Always, ((TestFlags) 3).GetEditorBrowsableState());
            Verify.AreEqual(EditorBrowsableState.Advanced, ((TestFlags) 7).GetEditorBrowsableState());
            Verify.AreEqual(EditorBrowsableState.Never, ((TestFlags) 15).GetEditorBrowsableState());
            Verify.AreEqual(EditorBrowsableState.Never, ((TestFlags) 11).GetEditorBrowsableState());
            Verify.AreEqual(EditorBrowsableState.Never, ((TestFlags) 12).GetEditorBrowsableState());
        }

        [TestMethod]
        public void GetFullNameTest()
        {
            Verify.AreEqual("System.EnumExtenstionsTest+TestEnum.TestValue1", TestEnum.TestValue1.GetFullName());
            Verify.AreEqual("System.EnumExtenstionsTest+TestEnum.TestValue2", TestEnum.TestValue2.GetFullName());
            Verify.AreEqual("System.EnumExtenstionsTest+TestEnum.TestValue3", TestEnum.TestValue3.GetFullName());
            Verify.AreEqual("System.EnumExtenstionsTest+TestEnum.TestValue4", TestEnum.TestValue4.GetFullName());
        }

        [TestMethod]
        public void IsFlagEnumTest()
        {
            Verify.IsFalse(EnumExtensions.IsFlagsEnum(typeof(TestEnum)));
            Verify.IsTrue(EnumExtensions.IsFlagsEnum(typeof(TestFlags)));

            Verify.IsFalse(EnumExtensions.IsFlagsEnum<TestEnum>());
            Verify.IsTrue(EnumExtensions.IsFlagsEnum<TestFlags>());
        }

        [TestMethod]
        public void SplitTest()
        {
            var expected = new[] {TestFlags.Flag1, TestFlags.Flag2, TestFlags.Flag3, TestFlags.Flag4};
            var actual = ((TestFlags) 15).Split();

            Verify.IsTrue(actual.SequenceEqual(expected));
        }

        private enum TestEnum
        {
            [ComponentModel.Description("Test Value 1")] TestValue1,
            [ComponentModel.Description("Test Value 2")] [EditorBrowsable(EditorBrowsableState.Advanced)] TestValue2,
            [ComponentModel.Description("Test Value 3")] [EditorBrowsable(EditorBrowsableState.Always)] TestValue3,
            [ComponentModel.Description("Test Value 4")] [EditorBrowsable(EditorBrowsableState.Never)] TestValue4
        }

        [Flags]
        private enum TestFlags
        {
            [ComponentModel.Description("F 1")] Flag1 = 1,
            [ComponentModel.Description("F 2")] [EditorBrowsable(EditorBrowsableState.Always)] Flag2 = 2,
            [ComponentModel.Description("F 3")] [EditorBrowsable(EditorBrowsableState.Advanced)] Flag3 = 4,
            [ComponentModel.Description("F 4")] [EditorBrowsable(EditorBrowsableState.Never)] Flag4 = 8
        }
    }
}