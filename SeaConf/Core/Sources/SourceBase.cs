using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SeaConf.Infrastructure;
using SeaConf.Interfaces.Core;

namespace SeaConf.Core.Sources
{
    /// <summary>
	/// Base class of the configuration source.
	/// </summary>
	/// <typeparam name="TModel">Model type.</typeparam>
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

        /// <summary>
        /// Getting all data models from configuration.
        /// </summary>
        /// <param name="rootNodes">Root configuration elements.</param>
        /// <returns>All data models from configuration.</returns>
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

        /// <summary>
        /// Getting root configuration elements.
        /// </summary>
        /// <returns>Root configuration elements.</returns>
        public abstract ValueTask<IReadOnlyList<INode>> GetRootNodesAsync();
    }
}