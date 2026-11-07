using System;
using System.ComponentModel;

namespace AiUnity.Common.Attributes;

[AttributeUsage(AttributeTargets.All)]
public sealed class ResourceAttribute : DescriptionAttribute
{
	public Uri HelpUri { get; private set; }

	public ResourceAttribute(string description, string helpLink = null)
		: base(description)
	{
		HelpUri = ((helpLink != null) ? new Uri(helpLink) : null);
	}
}
