using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.WindowsAzure.ServiceBus
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:PartialElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class AzureMessagePropertyAttributeTests
    {
        private const string CustomPropertyName = "CustomProperty";

        [TestMethod]
        public void ReadMessageWithBooleanProperty()
        {
            var reader = MessagePropertyReader.GetPropertyReader<CustomMessage<bool>>();
            var message = new CustomMessage<bool> { MessageProperty = true };
            var properties = reader.GetPropertyValues(message).ToList();
            properties.Count.Should().Be(1);
            properties[0].Name.Should().Be(CustomPropertyName);
            properties[0].Value.Should().Be("true");
        }

        #region CustomMessage Class Declaration

        [AzureMessageProperty(CustomPropertyName, "{" + nameof(MessageProperty) + "}", FormatString = "{0}", LowerCase = true)]
        private class CustomMessage<T>
        {
            public T MessageProperty { get; set; }
        }

        #endregion // CustomMessage Class Declaration
    }
}
