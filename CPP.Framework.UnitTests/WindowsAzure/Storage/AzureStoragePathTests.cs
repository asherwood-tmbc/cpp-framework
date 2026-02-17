using System;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.Diagnostics.Testing;
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
            Verify.AreEqual(expect, actual);
        }

        [Ignore]
        [ExpectedArgumentException("path1")]
        [TestMethod]
        public void CombineWithFourPathsAndInvalidPath1()
        {
            AzureStoragePath.Combine("first:/", "/second/", "/third/", "/fourth/");
        }

        [Ignore]
        [ExpectedArgumentException("path2")]
        [TestMethod]
        public void CombineWithFourPathsAndInvalidPath2()
        {
            AzureStoragePath.Combine("first/", "/second:/", "/third/", "/fourth/");
        }

        [Ignore]
        [ExpectedArgumentException("path3")]
        [TestMethod]
        public void CombineWithFourPathsAndInvalidPath3()
        {
            AzureStoragePath.Combine("first/", "/second/", "/third:/", "/fourth/");
        }

        [Ignore]
        [ExpectedArgumentException("path4")]
        [TestMethod]
        public void CombineWithFourPathsAndInvalidPath4()
        {
            AzureStoragePath.Combine("first/", "/second/", "/third/", "/fourth:/");
        }

        [ExpectedArgumentNullException("path1")]
        [TestMethod]
        public void CombineWithFourPathsAndNullPath1()
        {
            AzureStoragePath.Combine(null, "/second/", "/third/", "/fourth/");
        }

        [ExpectedArgumentNullException("path2")]
        [TestMethod]
        public void CombineWithFourPathsAndNullPath2()
        {
            AzureStoragePath.Combine("first/", null, "/third/", "/fourth/");
        }

        [ExpectedArgumentNullException("path3")]
        [TestMethod]
        public void CombineWithFourPathsAndNullPath3()
        {
            AzureStoragePath.Combine("first/", "/second/", null, "/fourth/");
        }

        [ExpectedArgumentNullException("path4")]
        [TestMethod]
        public void CombineWithFourPathsAndNullPath4()
        {
            AzureStoragePath.Combine("first/", "/second/", "/third/", null);
        }

        [TestMethod]
        public void CombineWithParams()
        {
            const string expect = "first/second/third/fourth/fifth";
            var actual = AzureStoragePath.Combine("first/", "/second/", "/third/", "/fourth/", "/fifth/");
            Verify.AreEqual(expect, actual);
        }

        [Ignore]
        [ExpectedArgumentException("paths")]
        [TestMethod]
        public void CombineWithParamsAndInvalidPath()
        {
            AzureStoragePath.Combine("first/", "/second/", "/third:/", "/fourth/", "/fifth/");
        }

        [ExpectedArgumentNullException("paths")]
        [TestMethod]
        public void CombineWithParamsAndNullPath()
        {
            AzureStoragePath.Combine("first/", "/second/", null, "/fourth/", "/fifth/");
        }

        [TestMethod]
        public void CombineWithThreePaths()
        {
            const string expect = "first/second/third";
            var actual = AzureStoragePath.Combine("first/", "/second/", "/third/");
            Verify.AreEqual(expect, actual);
        }

        [Ignore]
        [ExpectedArgumentException("path1")]
        [TestMethod]
        public void CombineWithThreePathsAndInvalidPath1()
        {
            AzureStoragePath.Combine("first:/", "/second/", "/third/");
        }

        [Ignore]
        [ExpectedArgumentException("path2")]
        [TestMethod]
        public void CombineWithThreePathsAndInvalidPath2()
        {
            AzureStoragePath.Combine("first/", "/second:/", "/third/");
        }

        [Ignore]
        [ExpectedArgumentException("path3")]
        [TestMethod]
        public void CombineWithThreePathsAndInvalidPath3()
        {
            AzureStoragePath.Combine("first/", "/second/", "/third:/");
        }

        [ExpectedArgumentNullException("path1")]
        [TestMethod]
        public void CombineWithThreePathsAndNullPath1()
        {
            AzureStoragePath.Combine(null, "/second/", "/third/");
        }

        [ExpectedArgumentNullException("path2")]
        [TestMethod]
        public void CombineWithThreePathsAndNullPath2()
        {
            AzureStoragePath.Combine("first/", null, "/third/");
        }

        [ExpectedArgumentNullException("path3")]
        [TestMethod]
        public void CombineWithThreePathsAndNullPath3()
        {
            AzureStoragePath.Combine("first/", "/second/", null);
        }

        [TestMethod]
        public void CombineWithTwoPaths()
        {
            const string expect = "first/second";
            var actual = AzureStoragePath.Combine("first/", "/second");
            Verify.AreEqual(expect, actual);
        }

        [Ignore]
        [ExpectedArgumentException("path1")]
        [TestMethod]
        public void CombineWithTwoPathsAndInvalidPath1()
        {
            AzureStoragePath.Combine("first:/", "/second/");
        }

        [Ignore]
        [ExpectedArgumentException("path2")]
        [TestMethod]
        public void CombineWithTwoPathsAndInvalidPath2()
        {
            AzureStoragePath.Combine("first/", "/second:/");
        }

        [ExpectedArgumentNullException("path1")]
        [TestMethod]
        public void CombineWithTwoPathsAndNullPath1()
        {
            AzureStoragePath.Combine(null, "/second/");
        }

        [ExpectedArgumentNullException("path2")]
        [TestMethod]
        public void CombineWithTwoPathsAndNullPath2()
        {
            AzureStoragePath.Combine("first/", null);
        }

        [TestMethod]
        public void GetBlobFileName()
        {
            const string expect = "my-blob-file.pdf";
            var path = AzureStoragePath.Combine("container", "location/folder", "my-blob-file.pdf");
            var actual = AzureStoragePath.GetBlobFileName(path);
            Verify.AreEqual(expect, actual);
        }

        [TestMethod]
        public void GetBlobFileNameWithContainerOnly()
        {
            const string expect = "";
            var path = AzureStoragePath.Combine("container", "");
            var actual = AzureStoragePath.GetBlobFileName(path);
            Verify.AreEqual(expect, actual);
        }

        [TestMethod]
        public void GetBlobFilePath()
        {
            const string expect = "location/folder/my-blob-file.pdf";
            var path = AzureStoragePath.Combine("container", "location/folder", "my-blob-file.pdf");
            var actual = AzureStoragePath.GetBlobFilePath(path);
            Verify.AreEqual(expect, actual);
        }

        [TestMethod]
        public void GetBlobFilePathWithContainerOnly()
        {
            const string expect = "";
            var path = AzureStoragePath.Combine("container", "");
            var actual = AzureStoragePath.GetBlobFilePath(path);
            Verify.AreEqual(expect, actual);
        }

        [TestMethod]
        public void GetContainerName()
        {
            const string expect = "container";
            var path = AzureStoragePath.Combine("container", "location/folder", "my-blob-file.pdf");
            var actual = AzureStoragePath.GetContainerName(path);
            Verify.AreEqual(expect, actual);
        }

        [TestMethod]
        public void HasInvalidPathChars()
        {
            var expect = String.Format("reports-out/organization/{0:D}/{1:D}-john-doe-ENTP.pdf", Guid.NewGuid(), Guid.NewGuid());
            Verify.IsFalse(AzureStoragePath.HasInvalidPathChars(expect));
        }

        [Ignore]
        [TestMethod]
        public void HasInvalidPathCharsWithInvalidChars()
        {
            Verify.IsTrue(AzureStoragePath.HasInvalidPathChars("$()%^&!@#*"));
        }

        [TestMethod]
        public void IsContainerNameValid()
        {
            Verify.IsTrue(AzureStoragePath.IsContainerNameValid("test-path-123"));
        }

        [TestMethod]
        public void IsContainerValidWithDoubleDashes()
        {
            Verify.IsFalse(AzureStoragePath.IsContainerNameValid("test--path"));
        }

        [TestMethod]
        public void IsContainerNameValidWithInvalidChars()
        {
            Verify.IsFalse(AzureStoragePath.IsContainerNameValid("test-path#"));
        }

        [TestMethod]
        public void IsContainerValidWithLeadingDash()
        {
            Verify.IsFalse(AzureStoragePath.IsContainerNameValid("-test-path"));
        }

        [TestMethod]
        public void RemoveInvalidPathChars()
        {
            const string expect = "container/org/project";
            var sample = "container/:org/>project";
            var actual = AzureStoragePath.RemoveInvalidChars(sample);
            Verify.AreEqual(expect, actual);
        }

        [TestMethod]
        public void RemoveInvalidPathChars2()
        {
            const string expect = @"container\org\project";
            var sample = @"container\:org\>project";
            var actual = AzureStoragePath.RemoveInvalidChars(sample);
            Verify.AreEqual(expect, actual);
        }
    }
}
