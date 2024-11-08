using System;

namespace SeaConf.Models
{
    /// <summary>
	/// An attribute for specifying the data model.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ModelAttribute : Attribute
	{
		/// <summary>
		/// Name.
		/// </summary>
		public readonly string? Name;

		public ModelAttribute(string? name = null)
		{
			Name = name;
		}
	}
}