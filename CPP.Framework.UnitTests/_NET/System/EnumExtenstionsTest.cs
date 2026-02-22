using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;
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
        public void GetDescriptionInvalidEnumTest()
        {
            Action act = () =>
            {
                var actual = ((TestEnum) (-1)).GetDescription();
            };
            act.Should().Throw<ArgumentOutOfRangeException>();
        }


        [TestMethod]
        public void GetDescriptionTest()
        {
            var expected = "Test Value 1";
            var actual = TestEnum.TestValue1.GetDescription();

            actual.Should().Be(expected);
        }

        [TestMethod]
        public void GetSingleFlagDescriptionTest()
        {
            var expected = "F 1";
            var actual = ((TestFlags) 1).GetDescription("#");

            actual.Should().Be(expected);
        }

        [TestMethod]
        public void GetMultipleFlagDescriptionWithDefaultSeperatorTest()
        {
            var expected = "F 1,F 2,F 3";
            var actual = ((TestFlags) 7).GetDescription();

            actual.Should().Be(expected);
        }

        [TestMethod]
        public void GetMultipleFlagDescriptionTest()
        {
            var expected = "F 1#F 2#F 3";
            var actual = ((TestFlags) 7).GetDescription("#");

            actual.Should().Be(expected);
        }

        [TestMethod]
        public void GetEditorBrowsableStateWithInvalidEnum()
        {
            Action act = () =>
            {
                ((TestFlags) (-1)).GetEditorBrowsableState();
            };
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void GetEditorBrowsableState()
        {
            TestEnum.TestValue1.GetEditorBrowsableState().Should().Be(EditorBrowsableState.Always);
            TestEnum.TestValue2.GetEditorBrowsableState().Should().Be(EditorBrowsableState.Advanced);
            TestEnum.TestValue3.GetEditorBrowsableState().Should().Be(EditorBrowsableState.Always);
            TestEnum.TestValue4.GetEditorBrowsableState().Should().Be(EditorBrowsableState.Never);
        }

        [TestMethod]
        public void GetEditorBrowsableStateWithCombinedFlags()
        {
            ((TestFlags) 3).GetEditorBrowsableState().Should().Be(EditorBrowsableState.Always);
            ((TestFlags) 7).GetEditorBrowsableState().Should().Be(EditorBrowsableState.Advanced);
            ((TestFlags) 15).GetEditorBrowsableState().Should().Be(EditorBrowsableState.Never);
            ((TestFlags) 11).GetEditorBrowsableState().Should().Be(EditorBrowsableState.Never);
            ((TestFlags) 12).GetEditorBrowsableState().Should().Be(EditorBrowsableState.Never);
        }

        [TestMethod]
        public void GetFullNameTest()
        {
            TestEnum.TestValue1.GetFullName().Should().Be("System.EnumExtenstionsTest+TestEnum.TestValue1");
            TestEnum.TestValue2.GetFullName().Should().Be("System.EnumExtenstionsTest+TestEnum.TestValue2");
            TestEnum.TestValue3.GetFullName().Should().Be("System.EnumExtenstionsTest+TestEnum.TestValue3");
            TestEnum.TestValue4.GetFullName().Should().Be("System.EnumExtenstionsTest+TestEnum.TestValue4");
        }

        [TestMethod]
        public void IsFlagEnumTest()
        {
            EnumExtensions.IsFlagsEnum(typeof(TestEnum)).Should().BeFalse();
            EnumExtensions.IsFlagsEnum(typeof(TestFlags)).Should().BeTrue();

            EnumExtensions.IsFlagsEnum<TestEnum>().Should().BeFalse();
            EnumExtensions.IsFlagsEnum<TestFlags>().Should().BeTrue();
        }

        [TestMethod]
        public void SplitTest()
        {
            var expected = new[] {TestFlags.Flag1, TestFlags.Flag2, TestFlags.Flag3, TestFlags.Flag4};
            var actual = ((TestFlags) 15).Split();

            actual.SequenceEqual(expected).Should().BeTrue();
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