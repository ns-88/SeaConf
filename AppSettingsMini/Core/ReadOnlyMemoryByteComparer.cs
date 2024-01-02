using System;
using System.Collections;
using System.Collections.Generic;

namespace AppSettingsMini.Core
{
    internal class ReadOnlyMemoryByteComparer : IEqualityComparer<ReadOnlyMemory<byte>>, IEqualityComparer
    {
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y)
        {
            return x.Length == y.Length && x.IsEmpty == y.IsEmpty && x.Span.SequenceEqual(y.Span);
        }

        public int GetHashCode(ReadOnlyMemory<byte> obj)
        {
            return obj.GetHashCode();
        }

        bool IEqualityComparer.Equals(object? x, object? y)
        {
            if (x is not ReadOnlyMemory<byte> lhs || y is not ReadOnlyMemory<byte> rhs)
            {
                return false;
            }

            return Equals(lhs, rhs);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }
}