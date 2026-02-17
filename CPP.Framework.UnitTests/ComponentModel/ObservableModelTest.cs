using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CPP.Framework.ComponentModel;
using CPP.Framework.Diagnostics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CPP.Framework.ComponentModel
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class ObservableModelTest
    {
        public List<PropertyChangedEventArgs> ChangedArgs { get; set; }
        public bool PropertyChangedExpected { get; set; }

        private MockObservableModel _model;

        private class MockObservableModel : ObservableModel
        {
            public Guid GuidProperty { get; set; }

            public MockObservableModel()
            {
                GuidProperty = Guid.NewGuid();
            }
            
            public void OnPropertyChange(string propertyName)
            {
                base.OnPropertyChanged(propertyName);
            }

            public new string GetPropertyValue(Expression<Func<string>> expression)
            {
                return base.GetPropertyValue(expression);
            }

            public new TValue GetPropertyValue<TValue>(Expression<Func<TValue>> expression) where TValue : new()
            {
                return base.GetPropertyValue(expression);
            }

            public new TValue GetPropertyValue<TValue>(Expression<Func<TValue>> expression, Func<TValue> factory)
            {
                return base.GetPropertyValue(expression, factory);
            }


            public new string SetPropertyValue(Expression<Func<string>> expression, string value)
            {
                return base.SetPropertyValue(expression, value);
            }

            public new TValue SetPropertyValue<TValue>(Expression<Func<TValue>> expression, TValue value) where TValue : new()
            {
                return base.SetPropertyValue(expression, value);
            }

            public new TValue SetPropertyValue<TValue>(Expression<Func<TValue>> expression, TValue value,
                Func<TValue> factory)
            {
                return SetPropertyValue(expression, value, factory);
            }
        }

        public void RegisterPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            ArgumentValidator.ValidateNotNull(() => args);

            Verify.IsTrue(PropertyChangedExpected);
            ChangedArgs.Add(args); 

        }

        [TestInitialize]
        public void Initialize()
        {
            ChangedArgs = new List<PropertyChangedEventArgs>();
            
            _model = new MockObservableModel();
            _model.SetPropertyValue(() => _model.GuidProperty, Guid.NewGuid());


            _model.PropertyChanged += RegisterPropertyChanged;
        }


        [TestMethod]
        public void OnPropertyChangedTest()
        {
            PropertyChangedExpected = true;
            var expectedChangedPropertyName = "Fake";

            _model.OnPropertyChange(expectedChangedPropertyName);
        
            Verify.AreEqual(1, ChangedArgs.Count);
            Verify.AreEqual(expectedChangedPropertyName, ChangedArgs[0].PropertyName);
        }

        [TestMethod]
        public void GetPropertyTest()
        {
            PropertyChangedExpected = false;

        }
    }
}
