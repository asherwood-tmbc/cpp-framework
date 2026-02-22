using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.WindowsAzure.Storage
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class AzureStoragePathTests
    {
        [TestMethod]
        public void CombineWithFourPaths()
        {
            const string expect = "first/second/third/fourth";
            var actual = AzureStoragePath.Combine("first/", "/second/", "/third/", "/fourth/");
            actual.Should().Be(expect);
        }

        [Ignore]
        [TestMethod]
        public void CombineWithFourPathsAndInvalidPath1()
        {
            Action act = () => AzureStoragePath.Combine("first:/", "/second/", "/third/", "/fourth/");
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("path1");
        }

        [Ignore]
        [TestMethod]
        public void CombineWithFourPathsAndInvalidPath2()
        {
            Action act = () => AzureStoragePath.Combine("first/", "/second:/", "/third/", "/fourth/");
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("path2");
        }

        [Ignore]
        [TestMethod]
        public void CombineWithFourPathsAndInvalidPath3()
        {
            Action act = () => AzureStoragePath.Combine("first/", "/second/", "/third:/", "/fourth/");
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("path3");
        }

        [Ignore]
        [TestMethod]
        public void CombineWithFourPathsAndInvalidPath4()
        {
            Action act = () => AzureStoragePath.Combine("first/", "/second/", "/third/", "/fourth:/");
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("path4");
        }

        [TestMethod]
        public void CombineWithFourPathsAndNullPath1()
        {
            Action act = () => AzureStoragePath.Combine(null, "/second/", "/third/", "/fourth/");
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("path1");
        }

        [TestMethod]
        public void CombineWithFourPathsAndNullPath2()
        {
            Action act = () => AzureStoragePath.Combine("first/", null, "/third/", "/fourth/");
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("path2");
        }

        [TestMethod]
        public void CombineWithFourPathsAndNullPath3()
        {
            Action act = () => AzureStoragePath.Combine("first/", "/second/", null, "/fourth/");
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("path3");
        }

        [TestMethod]
        public void CombineWithFourPathsAndNullPath4()
        {
            Action act = () => AzureStoragePath.Combine("first/", "/second/", "/third/", null);
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("path4");
        }

        [TestMethod]
        public void CombineWithParams()
        {
            const string expect = "first/second/third/fourth/fifth";
            var actual = AzureStoragePath.Combine("first/", "/second/", "/third/", "/fourth/", "/fifth/");
            actual.Should().Be(expect);
        }

        [Ignore]
        [TestMethod]
        public void CombineWithParamsAndInvalidPath()
        {
            Action act = () => AzureStoragePath.Combine("first/", "/second/", "/third:/", "/fourth/", "/fifth/");
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("paths");
        }

        [TestMethod]
        public void CombineWithParamsAndNullPath()
        {
            Action act = () => AzureStoragePath.Combine("first/", "/second/", null, "/fourth/", "/fifth/");
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("paths");
        }

        [TestMethod]
        public void CombineWithThreePaths()
        {
            const string expect = "first/second/third";
            var actual = AzureStoragePath.Combine("first/", "/second/", "/third/");
            actual.Should().Be(expect);
        }

        [Ignore]
        [TestMethod]
        public void CombineWithThreePathsAndInvalidPath1()
        {
            Action act = () => AzureStoragePath.Combine("first:/", "/second/", "/third/");
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("path1");
        }

        [Ignore]
        [TestMethod]
        public void CombineWithThreePathsAndInvalidPath2()
        {
            Action act = () => AzureStoragePath.Combine("first/", "/second:/", "/third/");
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("path2");
        }

        [Ignore]
        [TestMethod]
        public void CombineWithThreePathsAndInvalidPath3()
        {
            Action act = () => AzureStoragePath.Combine("first/", "/second/", "/third:/");
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("path3");
        }

        [TestMethod]
        public void CombineWithThreePathsAndNullPath1()
        {
            Action act = () => AzureStoragePath.Combine(null, "/second/", "/third/");
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("path1");
        }

        [TestMethod]
        public void CombineWithThreePathsAndNullPath2()
        {
            Action act = () => AzureStoragePath.Combine("first/", null, "/third/");
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("path2");
        }

        [TestMethod]
        public void CombineWithThreePathsAndNullPath3()
        {
            Action act = () => AzureStoragePath.Combine("first/", "/second/", null);
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("path3");
        }

        [TestMethod]
        public void CombineWithTwoPaths()
        {
            const string expect = "first/second";
            var actual = AzureStoragePath.Combine("first/", "/second");
            actual.Should().Be(expect);
        }

        [Ignore]
        [TestMethod]
        public void CombineWithTwoPathsAndInvalidPath1()
        {
            Action act = () => AzureStoragePath.Combine("first:/", "/second/");
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("path1");
        }

        [Ignore]
        [TestMethod]
        public void CombineWithTwoPathsAndInvalidPath2()
        {
            Action act = () => AzureStoragePath.Combine("first/", "/second:/");
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("path2");
        }

        [TestMethod]
        public void CombineWithTwoPathsAndNullPath1()
        {
            Action act = () => AzureStoragePath.Combine(null, "/second/");
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("path1");
        }

        [TestMethod]
        public void CombineWithTwoPathsAndNullPath2()
        {
            Action act = () => AzureStoragePath.Combine("first/", null);
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("path2");
        }

        [TestMethod]
        public void GetBlobFileName()
        {
            const string expect = "my-blob-file.pdf";
            var path = AzureStoragePath.Combine("container", "location/folder", "my-blob-file.pdf");
            var actual = AzureStoragePath.GetBlobFileName(path);
            actual.Should().Be(expect);
        }

        [TestMethod]
        public void GetBlobFileNameWithContainerOnly()
        {
            const string expect = "";
            var path = AzureStoragePath.Combine("container", "");
            var actual = AzureStoragePath.GetBlobFileName(path);
            actual.Should().Be(expect);
        }

        [TestMethod]
        public void GetBlobFilePath()
        {
            const string expect = "location/folder/my-blob-file.pdf";
            var path = AzureStoragePath.Combine("container", "location/folder", "my-blob-file.pdf");
            var actual = AzureStoragePath.GetBlobFilePath(path);
            actual.Should().Be(expect);
        }

        [TestMethod]
        public void GetBlobFilePathWithContainerOnly()
        {
            const string expect = "";
            var path = AzureStoragePath.Combine("container", "");
            var actual = AzureStoragePath.GetBlobFilePath(path);
            actual.Should().Be(expect);
        }

        [TestMethod]
        public void GetContainerName()
        {
            const string expect = "container";
            var path = AzureStoragePath.Combine("container", "location/folder", "my-blob-file.pdf");
            var actual = AzureStoragePath.GetContainerName(path);
            actual.Should().Be(expect);
        }

        [TestMethod]
        public void HasInvalidPathChars()
        {
            var expect = String.Format("reports-out/organization/{0:D}/{1:D}-john-doe-ENTP.pdf", Guid.NewGuid(), Guid.NewGuid());
            AzureStoragePath.HasInvalidPathChars(expect).Should().BeFalse();
        }

        [Ignore]
        [TestMethod]
        public void HasInvalidPathCharsWithInvalidChars()
        {
            AzureStoragePath.HasInvalidPathChars("$()%^&!@#*").Should().BeTrue();
        }

        [TestMethod]
        public void IsContainerNameValid()
        {
            AzureStoragePath.IsContainerNameValid("test-path-123").Should().BeTrue();
        }

        [TestMethod]
        public void IsContainerValidWithDoubleDashes()
        {
            AzureStoragePath.IsContainerNameValid("test--path").Should().BeFalse();
        }

        [TestMethod]
        public void IsContainerNameValidWithInvalidChars()
        {
            AzureStoragePath.IsContainerNameValid("test-path#").Should().BeFalse();
        }

        [TestMethod]
        public void IsContainerValidWithLeadingDash()
        {
            AzureStoragePath.IsContainerNameValid("-test-path").Should().BeFalse();
        }

        [TestMethod]
        public void RemoveInvalidPathChars()
        {
            const string expect = "container/org/project";
            var sample = "container/:org/>project";
            var actual = AzureStoragePath.RemoveInvalidChars(sample);
            actual.Should().Be(expect);
        }

        [TestMethod]
        public void RemoveInvalidPathChars2()
        {
            const string expect = @"container\org\project";
            var sample = @"container\:org\>project";
            var actual = AzureStoragePath.RemoveInvalidChars(sample);
            actual.Should().Be(expect);
        }
    }
}
