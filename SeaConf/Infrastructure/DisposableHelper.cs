using System;
using System.Runtime.CompilerServices;

namespace SeaConf.Infrastructure
{
	internal struct DisposableHelper
	{
		private readonly string _objectName;
		public bool IsDisposed { get; private set; }

		public DisposableHelper(string objectName)
		{
			_objectName = objectName;
		}

		public void SetIsDisposed()
		{
			IsDisposed = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void ThrowIfDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(_objectName);
			}
		}
	}
}