using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using CPP.Framework.Diagnostics.Testing;

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
            Assert.IsTrue(Regex.IsMatch(email, RegularExpression.EmailFormat));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNumber()
        {
            var email = "test1@cpp.com";
            Assert.IsTrue(Regex.IsMatch(email, RegularExpression.EmailFormat));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatSpecialCharacter()
        {
            var email = "test!#$%&'*+/=?^_`{|}~-@cpp.com";
            Assert.IsTrue(Regex.IsMatch(email, RegularExpression.EmailFormat));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNoAt()
        {
            var email = "testcpp.com";
            Assert.IsFalse(Regex.IsMatch(email, RegularExpression.EmailFormat));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNoLeftHandSide()
        {
            var email = "@cpp.com";
            Assert.IsFalse(Regex.IsMatch(email, RegularExpression.EmailFormat));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNoRightHandSide()
        {
            var email = "test@";
            Assert.IsFalse(Regex.IsMatch(email, RegularExpression.EmailFormat));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNoAtNoDot()
        {
            var email = "testcppcom";
            Assert.IsFalse(Regex.IsMatch(email, RegularExpression.EmailFormat));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNoDot()
        {
            var email = "test@cppcom";
            Assert.IsFalse(Regex.IsMatch(email, RegularExpression.EmailFormat));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void EmailFormatNoTLD()
        {
            var email = "test@cppcom";
            Assert.IsFalse(Regex.IsMatch(email, RegularExpression.EmailFormat));
        }

        #endregion

        #region CreditCardSecurityCode

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void CreditCardSecurityCodeThreeDigit()
        {
            var code = "123";
            Assert.IsTrue(Regex.IsMatch(code, RegularExpression.CreditCardSecurityCode));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void CreditCardSecurityCodeFourDigit()
        {
            var code = "1234";
            Assert.IsTrue(Regex.IsMatch(code, RegularExpression.CreditCardSecurityCode));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void CreditCardSecurityCodeTwoDigit()
        {
            var code = "12";
            Assert.IsFalse(Regex.IsMatch(code, RegularExpression.CreditCardSecurityCode));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void CreditCardSecurityCodeFiveDigit()
        {
            var code = "12345";
            Assert.IsFalse(Regex.IsMatch(code, RegularExpression.CreditCardSecurityCode));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void CreditCardSecurityCodeTwoDigitOneCharacter()
        {
            var code = "12a";
            Assert.IsFalse(Regex.IsMatch(code, RegularExpression.CreditCardSecurityCode));
        }

        #endregion

        #region CreditCardNumber

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void CreditCard()
        {
            var code = "4556356311967950";
            Assert.IsTrue(Regex.IsMatch(code, RegularExpression.CreditCardNumber));
        }

        #endregion

        #region SharedProductCode

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void SharedProductCode()
        {
            var code = "4556356311967950";
            Assert.IsTrue(Regex.IsMatch(code, RegularExpression.SharedProductCode));
        }

        #endregion

        #region Password Format

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void PasswordFormatUpperLowerNumberSpecialTenLong()
        {
            var password = "Password1!";
            Assert.IsTrue(Regex.IsMatch(password, RegularExpression.PasswordFormat));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void PasswordFormatUpperLowerNumberSpecialSevenLong()
        {
            var password = "Pword1!";
            Assert.IsFalse(Regex.IsMatch(password, RegularExpression.PasswordFormat));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void PasswordFormatLowerNumberSpecialTenLong()
        {
            var password = "password1!";
            Assert.IsFalse(Regex.IsMatch(password, RegularExpression.PasswordFormat));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void PasswordFormatUpperNumberSpecialTenLong()
        {
            var password = "PASSWORD1!";
            Assert.IsFalse(Regex.IsMatch(password, RegularExpression.PasswordFormat));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void PasswordFormatUpperLowerSpecialNineLong()
        {
            var password = "Password!";
            Assert.IsFalse(Regex.IsMatch(password, RegularExpression.PasswordFormat));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Validation)]
        public void PasswordFormatUpperLowerNumberNineLong()
        {
            var password = "Password1";
            Assert.IsFalse(Regex.IsMatch(password, RegularExpression.PasswordFormat));
        }

        #endregion
    }
}
