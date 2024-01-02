using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces.Core;

namespace AppSettingsMini.Core.Sources
{
	internal abstract class SourceBase<TModel> : ISource<TModel>
		where TModel : class, IModel
	{
		private bool _isLoaded;
		protected DisposableHelper DisposableHelper { get; }

		protected SourceBase()
		{
			DisposableHelper = new DisposableHelper(GetType().Name);
		}

		protected void SetIsLoaded()
		{
			_isLoaded = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void ThrowIfNotLoaded()
		{
			if (!_isLoaded)
			{
				throw new InvalidOperationException(Strings.SourceNotLoaded);
			}
		}

		public abstract ValueTask<IReadOnlyList<INode>> GetRootNodes();

		public async IAsyncEnumerable<TModel> GetModelsAsync(IEnumerable<INode> rootNodes)
		{
			foreach (var rootNode in rootNodes)
			{
				var stack = new Stack<INode>();
				var top = rootNode;

				while (top != null || stack.Count != 0)
				{
					if (stack.Count != 0)
					{
						top = stack.Pop();
					}

					while (top != null)
					{
						var descendants = await top.GetDescendantNodesAsync().ConfigureAwait(false);

						yield return (TModel)top;

						if (descendants == null! || descendants.Count == 0)
						{
							top = null;
							break;
						}

						for (var i = descendants.Count - 1; i > 0; i--)
						{
							stack.Push(descendants[i]);
						}

						top = descendants[0];
					}
				}
			}
		}

		public virtual ValueTask LoadAsync()
		{
			return ValueTask.CompletedTask;
		}

		public virtual ValueTask SaveAsync()
		{
			return ValueTask.CompletedTask;
		}

		public virtual ValueTask DisposeAsync()
		{
			return ValueTask.CompletedTask;
		}
	}
}