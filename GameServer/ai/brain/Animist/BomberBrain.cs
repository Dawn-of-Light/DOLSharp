/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

using DOL.Events;
using DOL.GS;

namespace DOL.AI.Brain
{
	public class BomberBrain : ControlledNpc
	{
		public BomberBrain(GameLiving owner) : base(owner) { }

		public override int ThinkInterval
		{
			get { return 700; }
		}

		protected override bool CheckDefensiveSpells(Spell spell)
		{
			return true;
		}

		protected override bool CheckOffensiveSpells(Spell spell)
		{
			return true;
		}

		#region Think
		public override void Think()
		{
			GameLiving living = Body.TempProperties.getObjectProperty("bombertarget", null) as GameLiving;
			if(living == null) return;
			if(Body.IsWithinRadius( living, 150 ))
			{
				Body.Notify(GameNPCEvent.ArriveAtTarget, Body);
			}
		}

		/// <summary>
    /// Updates the pet window
    /// </summary>
    public override void UpdatePetWindow() { }
		#endregion
	}
}