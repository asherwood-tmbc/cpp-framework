using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace CPP.Framework.Security
{
    /// <summary>
    /// Compares <see cref="Claim"/> objects based on their claim types and values.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class ClaimByTypeComparer : IComparer<Claim>, IEqualityComparer<Claim>
    {
        /// <inheritdoc />
        public int Compare(Claim x, Claim y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            return (string.Compare(x.Type, y.Type, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public bool Equals(Claim x, Claim y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            return (GetHashCode(x) == GetHashCode(y));
        }

        /// <inheritdoc />
        public int GetHashCode(Claim obj) => ($"{obj.Type}({obj.Value})").GetHashCode();
    }
}
