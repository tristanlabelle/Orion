using System;

namespace Orion.Engine.Gui2
{
	/// <summary>
	/// Specifies the invalidation state of a <see cref="Control"/>'s layout.
	/// </summary>
	public enum LayoutState
	{
		/// <summary>
		/// Specifies that the <see cref="Control"/> needs to be measured and arranged.
		/// </summary>
		Invalidated,
		
		/// <summary>
		/// Specifies that the <see cref="Control"/> has been measured and needs to be arranged.
		/// </summary>
		Measured,
		
		/// <summary>
		/// Specifies that the <see cref="Control"/> has been measured and arranged.
		/// </summary>
		Arranged
	}
}
