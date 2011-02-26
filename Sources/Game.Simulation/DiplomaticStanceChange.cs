using System;
using System.ComponentModel;
using Orion.Engine;

namespace Orion.Game.Simulation
{
	/// <summary>
	/// Encapsulates information about a change in <see cref="DiplomaticStance"/> between two <see cref="Faction"/>s.
	/// </summary>
	[Serializable]
	[ImmutableObject(true)]
	public struct DiplomaticStanceChange
	{
		#region Fields
		private readonly Faction sourceFaction;
		private readonly Faction targetFaction;
		private readonly DiplomaticStance oldDiplomaticStance;
		private readonly DiplomaticStance newDiplomaticStance;
		#endregion
		
		#region Constructors
		public DiplomaticStanceChange(Faction sourceFaction, Faction targetFaction,
		    DiplomaticStance oldDiplomaticStance, DiplomaticStance newDiplomaticStance)
		{
			Argument.EnsureNotNull(sourceFaction, "sourceFaction");
			Argument.EnsureNotNull(targetFaction, "targetFaction");
			
			this.sourceFaction = sourceFaction;
			this.targetFaction = targetFaction;
			this.oldDiplomaticStance = oldDiplomaticStance;
			this.newDiplomaticStance = newDiplomaticStance;
		}
		#endregion
		
		#region Properties
		/// <summary>
		/// The <see cref="Faction"/> whose <see cref="DiplomaticStance"/> changed.
		/// </summary>
		public Faction SourceFaction
		{
			get { return sourceFaction; }
		}
		
		/// <summary>
		/// The <see cref="Faction"/> with regard to which the <see cref="DiplomaticStance"/> changed.
		/// </summary>
		public Faction TargetFaction
		{
			get { return targetFaction; }
		}
		
		/// <summary>
		/// The previous <see cref="DiplomaticStance"/> value.
		/// </summary>
		public DiplomaticStance OldDiplomaticStance
		{
			get { return oldDiplomaticStance; }
		}
		
		/// <summary>
		/// The new <see cref="DiplomaticStance"/> value.
		/// </summary>
		public DiplomaticStance NewDiplomaticStance
		{
			get { return newDiplomaticStance; }
		}
		#endregion
	}
}
