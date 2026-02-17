namespace CPP.Framework.Text
{
    /// <summary>
    /// Defines format patterns used to validate the contents of various strings using regular 
    /// expressions.
    /// </summary>
    public class RegularExpression
    {
        /// <summary>
        /// The email format
        /// </summary>
        public const string EmailFormat = "^(([^<>()\\[\\]\\.,;:\\s@\"]+(\\.[^<>()\\[\\]\\\\.,;:\\s@\"]+)*)|(\".+\"))@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}])|(([a-zA-Z\\-0-9]+\\.)+[a-zA-Z]{2,}))$";

        /// <summary>
        /// The password format
        /// </summary>
        public const string PasswordFormat = "^(?=.{8,})(?=.*[a-z])(?=.*[A-Z])(?=.*[\\d])(?=.*[\\W\\p{Pc}]).*$";

        /// <summary>
        /// The credit card security code
        /// </summary>
        public const string CreditCardSecurityCode = "^\\d{3,4}$";

        /// <summary>
        /// The credit card number
        /// </summary>
        public const string CreditCardNumber = "^\\d+$";

        /// <summary>
        /// The shared product code
        /// </summary>
        public const string SharedProductCode = "(?:[A-Z]{1,2}-){0,2}([0-9]+)(?:-[0-9A-Z]+){0,}";   // this constant is used in both jscript and C#, which do not fully support the same syntax, so be careful changing it

        /// <summary>
        /// The phone number
        /// </summary>
        public const string PhoneNumber = "^[^0-9]?([0-9]{3})[^0-9]?[^0-9]?([0-9]{3})[^0-9]?([0-9]{4})$"; // US only, may need new UI for International

        /// <summary>
        /// The gift code
        /// </summary>
        public const string GiftCode = "^[a-zA-Z0-9]{10}$"; 
    }
}
