using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.UnitTests.Testing
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    [ExcludeFromCodeCoverage]
    public class TestGroupAttribute : TestCategoryBaseAttribute
    {
        private readonly IList<string> _TestCategories;

        public TestGroupAttribute(TestGroupTarget group)
        {
            _TestCategories = new[] { Enum.GetName(typeof(TestGroupTarget), group) };
        }

        public override IList<string> TestCategories { get { return _TestCategories; } }
    }
}
