using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;

namespace CPP.Framework.Text
{
    [TestClass]
    public class RegularExpressionTests
    {
        #region Email Format

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatPlain()
        {
            var email = "test@cpp.com";
            Regex.IsMatch(email, RegularExpression.EmailFormat).Should().BeTrue();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNumber()
        {
            var email = "test1@cpp.com";
            Regex.IsMatch(email, RegularExpression.EmailFormat).Should().BeTrue();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatSpecialCharacter()
        {
            var email = "test!#$%&'*+/=?^_`{|}~-@cpp.com";
            Regex.IsMatch(email, RegularExpression.EmailFormat).Should().BeTrue();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNoAt()
        {
            var email = "testcpp.com";
            Regex.IsMatch(email, RegularExpression.EmailFormat).Should().BeFalse();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNoLeftHandSide()
        {
            var email = "@cpp.com";
            Regex.IsMatch(email, RegularExpression.EmailFormat).Should().BeFalse();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNoRightHandSide()
        {
            var email = "test@";
            Regex.IsMatch(email, RegularExpression.EmailFormat).Should().BeFalse();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNoAtNoDot()
        {
            var email = "testcppcom";
            Regex.IsMatch(email, RegularExpression.EmailFormat).Should().BeFalse();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNoDot()
        {
            var email = "test@cppcom";
            Regex.IsMatch(email, RegularExpression.EmailFormat).Should().BeFalse();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNoTLD()
        {
            var email = "test@cppcom";
            Regex.IsMatch(email, RegularExpression.EmailFormat).Should().BeFalse();
        }

        #endregion

        #region CreditCardSecurityCode

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void CreditCardSecurityCodeThreeDigit()
        {
            var code = "123";
            Regex.IsMatch(code, RegularExpression.CreditCardSecurityCode).Should().BeTrue();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void CreditCardSecurityCodeFourDigit()
        {
            var code = "1234";
            Regex.IsMatch(code, RegularExpression.CreditCardSecurityCode).Should().BeTrue();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void CreditCardSecurityCodeTwoDigit()
        {
            var code = "12";
            Regex.IsMatch(code, RegularExpression.CreditCardSecurityCode).Should().BeFalse();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void CreditCardSecurityCodeFiveDigit()
        {
            var code = "12345";
            Regex.IsMatch(code, RegularExpression.CreditCardSecurityCode).Should().BeFalse();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void CreditCardSecurityCodeTwoDigitOneCharacter()
        {
            var code = "12a";
            Regex.IsMatch(code, RegularExpression.CreditCardSecurityCode).Should().BeFalse();
        }

        #endregion

        #region CreditCardNumber

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void CreditCard()
        {
            var code = "4556356311967950";
            Regex.IsMatch(code, RegularExpression.CreditCardNumber).Should().BeTrue();
        }

        #endregion

        #region SharedProductCode

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void SharedProductCode()
        {
            var code = "4556356311967950";
            Regex.IsMatch(code, RegularExpression.SharedProductCode).Should().BeTrue();
        }

        #endregion

        #region Password Format

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void PasswordFormatUpperLowerNumberSpecialTenLong()
        {
            var password = "Password1!";
            Regex.IsMatch(password, RegularExpression.PasswordFormat).Should().BeTrue();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void PasswordFormatUpperLowerNumberSpecialSevenLong()
        {
            var password = "Pword1!";
            Regex.IsMatch(password, RegularExpression.PasswordFormat).Should().BeFalse();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void PasswordFormatLowerNumberSpecialTenLong()
        {
            var password = "password1!";
            Regex.IsMatch(password, RegularExpression.PasswordFormat).Should().BeFalse();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void PasswordFormatUpperNumberSpecialTenLong()
        {
            var password = "PASSWORD1!";
            Regex.IsMatch(password, RegularExpression.PasswordFormat).Should().BeFalse();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void PasswordFormatUpperLowerSpecialNineLong()
        {
            var password = "Password!";
            Regex.IsMatch(password, RegularExpression.PasswordFormat).Should().BeFalse();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void PasswordFormatUpperLowerNumberNineLong()
        {
            var password = "Password1";
            Regex.IsMatch(password, RegularExpression.PasswordFormat).Should().BeFalse();
        }

        #endregion
    }
}
