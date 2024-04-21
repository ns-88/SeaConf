using System;

namespace AppSettingsMini.Models
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ModelAttribute : Attribute
	{
		public readonly string? Name;

		public ModelAttribute(string? name = null)
		{
			Name = name;
		}
	}
}