using System;
using System.Text;

namespace SeaConf.Core
{
    /// <summary>
    /// Path to configuration data model.
    /// </summary>
    public readonly struct ModelPath
    {
        private readonly int _hashCode;
        private readonly string[] _parts;

        /// <summary>
        /// Part count.
        /// </summary>
        public int Count { get; }

        public ModelPath(string modelName)
        {
            _hashCode = modelName.GetHashCode(StringComparison.Ordinal);
            _parts = new string[1];
            _parts[0] = modelName;

            Count = 1;
        }

        public ModelPath(string modelName, ModelPath path)
        {
            _hashCode = HashCode.Combine(modelName.GetHashCode(StringComparison.Ordinal), path._hashCode);
            _parts = new string[path.Count + 1];
            _parts[path.Count] = modelName;

            Array.Copy(path._parts, _parts, path.Count);

            Count = path.Count + 1;
        }

        /// <summary>
        /// Checks if the specified path is included in the given.
        /// </summary>
        /// <param name="other">Another path.</param>
        /// <param name="isIncludeSelf">Should we consider that the path is included in itself.</param>
        /// <param name="limit">Limiting the number of elements in included paths.</param>
        /// <returns>Is included.</returns>
        public bool IsIncluded(ModelPath other, bool isIncludeSelf = false, uint? limit = null)
        {
            if (limit == 0)
            {
                throw new ArgumentException(nameof(limit));
            }

            if (other.Count > Count)
            {
                return false;
            }

            if (other.Count == Count)
            {
                return isIncludeSelf && _hashCode == other.GetHashCode() && Equals(other);
            }

            for (var i = 0; i < other.Count; i++)
            {
                if (!other._parts[i].Equals(_parts[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            if (limit == null)
            {
                return true;
            }

            return Count - other.Count == limit;
        }

        /// <summary>
        /// Getting part of path by index.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <returns>Path part.</returns>
        public string this[int index] => _parts[index];

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? obj)
        {
            return obj is ModelPath other && Equals(other);
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(ModelPath other)
        {
            if (Count != other.Count)
            {
                return false;
            }

            for (var i = 0; i < Count; i++)
            {
                if (!_parts[i].Equals(other._parts[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < Count; i++)
            {
                sb.Append($"{_parts[i]}\\");
            }

            sb = sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}