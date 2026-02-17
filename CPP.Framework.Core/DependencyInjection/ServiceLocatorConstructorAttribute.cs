using System;

namespace CPP.Framework.DependencyInjection
{
    /// <summary>
    /// Applied to a constructor to declare to the <see cref="ServiceLocator"/> that it should be
    /// used when creating new instances of the object. Please note that only one constructor per
    /// class should be marked with this attribute, otherwise an exception will be thrown for the
    /// code requesting the object. However, applying this attribute to a constructor that's marked
    /// as <see langword="private"/>, <see langword="protected"/>, or <see langword="internal"/> is
    /// supported.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class ServiceLocatorConstructorAttribute : Attribute { }
}
