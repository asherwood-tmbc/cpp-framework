using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CPP.Framework.Data;
using CPP.Framework.UnitTests.Testing;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CPP.Framework.Serialization
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ConfidentialControlResolverTests
    {
        private const string AllowedValueProperty = "AllowedValue";
        private const string ConfidentialProperty = "Confidential";

        [TestMethod]
        public void VerifyConfidentialPropertyDeserialized()
        {
            var expect = Guid.NewGuid().ToString("N");
            var source = $"{{ \"{AllowedValueProperty}\": \"{Guid.NewGuid().ToString("N")}\", \"{ConfidentialProperty}\": \"{expect}\" }}";
            var actual = JsonConvert.DeserializeObject<SampleModel>(source);
            actual.Confidential.Should().Be(expect);
        }

        [TestMethod]
        public void VerifyConfidentialPropertyNotSerialized()
        {
            var model = new SampleModel
            {
                AllowedValue = Guid.NewGuid().ToString("N"),
                Confidential = Guid.NewGuid().ToString("N"),
            };
            var expect = $"{{\"{AllowedValueProperty}\":\"{model.AllowedValue}\"}}";
            var actual = JsonConvert.SerializeObject(model, CreateSerializerSettings());

            actual.Should().Be(expect);
        }

        [TestMethod]
        public void VerifyConfidentialPropertyRemoved()
        {
            var model = new SampleModel
            {
                AllowedValue = Guid.NewGuid().ToString("N"),
                Confidential = Guid.NewGuid().ToString("N"),
            };
            var actual = JObject.FromObject(model, CreateSerializer());

            actual.Property(ConfidentialProperty).Should().BeNull();
            actual.Property(AllowedValueProperty).Should().NotBeNull();
        }

        #region Internal Helper Functions

        private JsonSerializerSettings CreateSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new ConfidentialContractResolver(),
            };
            return settings;
        }

        private JsonSerializer CreateSerializer()
        {
            var settings = CreateSerializerSettings();
            return JsonSerializer.Create(settings);
        }

        #endregion

        private class SampleModel
        {
            [JsonProperty(AllowedValueProperty)]
            public string AllowedValue { get; set; }

            [JsonProperty(ConfidentialProperty)]
            [Confidential]
            public string Confidential { get; set; }
        }
    }
}
