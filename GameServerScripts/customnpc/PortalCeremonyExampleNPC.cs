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
using System;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// PortalCeremonyExampleNPC is a sample Ceremony using "111500001" NpcTemplate and teleporting players every minutes to Emain Macha ! Using Warning Effects !
	/// </summary>
	public class PortalCeremonyExampleNPC : PortalCeremonyBaseNPC
	{
		protected override int PortalTeleportersTemplateID
		{
			get {return 111500001;}
		}
		
		protected override ushort PortalTeleporterEffect
		{
			get {return 4310;}
		}

		protected override ushort PortalTeleporterEffectWarn
		{
			get {return 3278;}
		}

		protected override ushort PortalTeleporterEffectCritic
		{
			get {return 105;}
		}
		
		protected override int PortalDestinationX
		{
			get {return 457839;}
		}

		protected override int PortalDestinationY
		{
			get {return 518114;}
		}

		protected override int PortalDestinationZ
		{
			get {return 7940;}
		}

		protected override int PortalDestinationRegion
		{
			get {return 163;}
		}

		protected override int TeleportTimerCallback(RegionTimer respawnTimer)
		{
			// Check any Keep State / Tower State here
			// This is called 16 times between each teleport !
			// You can change teleport speed using
			// this.PortalTeleportInterval (in ms !!)
			
			return base.TeleportTimerCallback(respawnTimer);
		} 
		
	}
}
