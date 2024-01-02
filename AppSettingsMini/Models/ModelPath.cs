using System;
using System.Text;

namespace AppSettingsMini.Models
{
	public readonly struct ModelPath
	{
		private readonly int _hashCode;
		private readonly string[] _modelNames;

		public int Count { get; }

		public ModelPath(string modelName)
		{
			_hashCode = modelName.GetHashCode(StringComparison.Ordinal);
			_modelNames = new string[1];
			_modelNames[0] = modelName;

			Count = 1;
		}

		public ModelPath(string modelName, ModelPath path)
		{
			_hashCode = HashCode.Combine(modelName.GetHashCode(StringComparison.Ordinal), path._hashCode);
			_modelNames = new string[path.Count + 1];
			_modelNames[path.Count] = modelName;

			Array.Copy(path._modelNames, _modelNames, path.Count);

			Count = path.Count + 1;
		}

		public string this[int index] => _modelNames[index];

		public override bool Equals(object? obj)
		{
			return obj is ModelPath other && Equals(other);
		}

		public bool Equals(ModelPath other)
		{
			if (Count != other.Count)
			{
				return false;
			}

			for (var i = 0; i < Count; i++)
			{
				if (!_modelNames[i].Equals(other._modelNames[i], StringComparison.Ordinal))
				{
					return false;
				}
			}

			return true;
		}

		public override int GetHashCode()
		{
			return _hashCode;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			for (var i = 0; i < Count; i++)
			{
				sb.Append($"{_modelNames[i]}\\");
			}

			sb = sb.Remove(sb.Length - 1, 1);

			return sb.ToString();
		}
	}
}