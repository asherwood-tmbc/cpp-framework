using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.Diagnostics.Testing
{
    /// <summary>
    /// Class that is used to specify the category of a unit or integration test.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    [ExcludeFromCodeCoverage]
    public class TestGroupAttribute : TestCategoryBaseAttribute
    {
        private readonly IList<string> _TestCategories;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="group">The target group for the test.</param>
        public TestGroupAttribute(TestGroupTarget group)
        {
            _TestCategories = new[] { Enum.GetName(typeof(TestGroupTarget), group) };
        }

        /// <summary>
        /// Gets the test category that has been applied to the test.
        /// </summary>
        /// <returns>An <see cref="IList{T}"/> that contains the test category.</returns>
        public override IList<string> TestCategories { get { return _TestCategories; } }
    }
}
