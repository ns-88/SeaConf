using System;
using SeaConf.Infrastructure;

namespace SeaConf.Core
{
    /// <summary>
    /// Model data - type and name.
    /// </summary>
    public readonly struct ModelData : IEquatable<ModelData>
    {
        /// <summary>
        /// Type.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// Name.
        /// </summary>
        public readonly string Name;

        public ModelData(string name, Type type)
        {
            Name = Guard.ThrowIfEmptyString(name);
            Type = Guard.ThrowIfNull(type);
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(ModelData other)
        {
            return Name.Equals(other.Name, StringComparison.Ordinal) && Type == other.Type;
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? obj)
        {
            return obj is ModelData other && Equals(other);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Type);
        }

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}