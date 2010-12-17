using System;

namespace Orion.Engine.Gui2
{
	/// <summary>
	/// Specifies the invalidation state of a <see cref="UIElement"/>'s layout.
	/// </summary>
	public enum LayoutState
	{
		/// <summary>
		/// Specifies that the <see cref="UIElement"/> needs to be measured and arranged.
		/// </summary>
		Invalidated,
		
		/// <summary>
		/// Specifies that the <see cref="UIElement"/> has been measured and needs to be arranged.
		/// </summary>
		Measured,
		
		/// <summary>
		/// Specifies that the <see cref="UIElement"/> has been measured and arranged.
		/// </summary>
		Arranged
	}
}
