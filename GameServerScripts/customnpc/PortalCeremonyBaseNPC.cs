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
using System.Collections.Generic;
using DOL.GS.Geometry;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Portal Ceremony NPC summon Disciple based on a pre-set template ID.
	/// It then start a timer with animation at different state to finally teleport player withing range.
	/// Override the Destination Attribute for your own subclasses.
	/// </summary>
	public class PortalCeremonyBaseNPC : GameNPC
	{
        private readonly Position destinationDummy = Position.Create(regionID: 0, x: 0, y: 0, z: 0, heading: 0);
		/// <summary>
		/// Portal Pad Model id to override
		/// </summary>
		protected virtual ushort PortalWorldObjectModel
		{
			get {return 0;}
		}
		
		/// <summary>
		/// Portal count of Teleporter NPC
		/// </summary>
		protected virtual byte PortalTeleporterCount
		{
			get {return 6;}
		}
		
		/// <summary>
		/// Portal range for teleporting and spawning NPC
		/// </summary>
		protected virtual ushort PortalCeremonyRange
		{
			get {return 325;}
		}
		
		/// <summary>
		/// Portal Summoned NPC templateID
		/// </summary>
		protected virtual int PortalTeleportersTemplateID
		{
			get {return 0;}
		}
		
		/// <summary>
		/// Portal Spell Effect on Teleport
		/// </summary>
		protected virtual ushort PortalTeleporterEffect
		{
			get {return 4310;}
		}
		/// <summary>
		/// Portal Spell Effect on first Warning
		/// </summary>
		protected virtual ushort PortalTeleporterEffectWarn
		{
			get {return 0;}
		}
		/// <summary>
		/// Portal Spell Effect on last Warning
		/// </summary>
		protected virtual ushort PortalTeleporterEffectCritic
		{
			get {return 0;}
		}

        protected virtual Position PortalDestination
            => destinationDummy;

		protected virtual int PortalDestinationX => PortalDestination.X;
		protected virtual int PortalDestinationY => PortalDestination.Y;
		protected virtual int PortalDestinationZ => PortalDestination.Z;
		protected virtual int PortalDestinationRegion => PortalDestination.RegionID;

		/// <summary>
		/// Interval between teleport in milliseconds
		/// </summary>
		protected virtual long PortalTeleportInterval
		{
			get {return 60000;}
		}
		
		/// <summary>
		/// Region Timer for Teleport Event
		/// </summary>
		private RegionTimer m_teleportTimer;
		/// <summary>
		/// Region Timer count for Elapsed Events
		/// </summary>
		private ulong m_intervalCount = 0;
		
		/// <summary>
		/// Collection of Teleporters NPC
		/// </summary>
		protected List<GameNPC> m_teleporters = new List<GameNPC>();

		/// <summary>
		/// Reference to the pad Static Item.
		/// </summary>
		protected GameStaticItem m_worldObject = new GameStaticItem();
		
		/// <summary>
		/// Always show Teleporter Indicator for Ceremony Master
		/// </summary>
		public override bool ShowTeleporterIndicator {
			get { return true; }
		}
		
		/// <summary>
		/// Override Add To World to Spawn Teleporters in Circle
		/// And start Timer.
		/// </summary>
		/// <returns></returns>
		public override bool AddToWorld()
		{
			if (!base.AddToWorld())
				return false;
			
			// Add the Item Pad
            m_worldObject.Position = Position;
			m_worldObject.Model = PortalWorldObjectModel;
			m_worldObject.AddToWorld();
			
			// Add the teleporters
			NpcTemplate teleporters = NpcTemplateMgr.GetTemplate(PortalTeleportersTemplateID);
			ushort divisor = (ushort)(4096/PortalTeleporterCount);
			for (int cnt = 0 ; cnt < PortalTeleporterCount ; cnt++)
			{
				GameNPC teleporter = new GameNPC(teleporters);
                var assistantOrientation = Orientation + Angle.Heading(cnt * divisor + 2048);
                var assistantLocation = Position + Vector.Create(assistantOrientation, length: PortalCeremonyRange);
                teleporter.Position = assistantLocation.With(orientation: assistantOrientation);
				teleporter.CurrentRegion = CurrentRegion;
				m_teleporters.Add(teleporter);
				teleporter.AddToWorld();
			}
			
			// Start Timer.
			m_teleportTimer = new RegionTimer(this);
			m_teleportTimer.Callback = new RegionTimerCallback(TeleportTimerCallback);
			m_teleportTimer.Start((int)(PortalTeleportInterval >> 4));
			
			return true;
		}
		
		/// <summary>
		/// Teleport Event Callback for Animation and Teleportation
		/// </summary>
		/// <param name="respawnTimer"></param>
		/// <returns></returns>
		protected virtual int TeleportTimerCallback(RegionTimer respawnTimer)
		{
			
			m_intervalCount++;
			
			if (m_intervalCount % 16 == 0)
			{
				// Player animation (short duration state)
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					GamePlayer ply = player;
					if (ply != null)
					{
						ply.Out.SendSpellEffectAnimation(this, this, PortalTeleporterEffect, 0, false, 1);
						foreach (GameNPC tps in m_teleporters)
						{
							GameNPC teleprt = tps;
							ply.Out.SendSpellEffectAnimation(teleprt, teleprt, PortalTeleporterEffect, 0, false, 1);
						}
					}
				}
				
				// we need to port the players
				foreach (GamePlayer player in GetPlayersInRadius((ushort)(PortalCeremonyRange+50)))
				{
					GamePlayer ply = player;
					if (ply != null)
						ply.MoveTo(PortalDestination.With(player.Orientation));
				}
				
			}
			else if (m_intervalCount % 16 == 15)
			{
				// Player teleport animation here
				
				// Player animation (short duration state)
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					GamePlayer ply = player;
					if (ply != null)
					{
						ply.Out.SendSpellCastAnimation(this, PortalTeleporterEffect, (ushort)((PortalTeleportInterval >> 4)/100));
						foreach (GameNPC tps in m_teleporters)
						{
							GameNPC teleprt = tps;
							ply.Out.SendSpellCastAnimation(teleprt, PortalTeleporterEffect, (ushort)((PortalTeleportInterval >> 4)/100));
						}
					}
				}
			}
			else if (m_intervalCount % 16 == 14 && PortalTeleporterEffectCritic > 0)
			{
				// Player animation (short duration state)
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					GamePlayer ply = player;
					if (ply != null)
					{
						ply.Out.SendSpellEffectAnimation(this, this, PortalTeleporterEffectCritic, 0, false, 1);
						foreach (GameNPC tps in m_teleporters)
						{
							GameNPC teleprt = tps;
							ply.Out.SendSpellEffectAnimation(teleprt, teleprt, PortalTeleporterEffectCritic, 0, false, 1);
						}
					}
				}
				
			}
			else if (m_intervalCount % 16 == 13 && PortalTeleporterEffectCritic > 0)
			{
				// last warning
				// Player animation
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					GamePlayer ply = player;
					if (ply != null)
					{
						ply.Out.SendSpellCastAnimation(this, PortalTeleporterEffectCritic, (ushort)((PortalTeleportInterval >> 4)/100));
						foreach (GameNPC tps in m_teleporters)
						{
							GameNPC teleprt = tps;
							ply.Out.SendSpellCastAnimation(teleprt, PortalTeleporterEffectCritic, (ushort)((PortalTeleportInterval >> 4)/100));
						}
					}
				}
			}
			else if (m_intervalCount % 16 == 12 && PortalTeleporterEffectWarn > 0)
			{
				// Player animation (short duration state)
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					GamePlayer ply = player;
					if (ply != null)
					{
						ply.Out.SendSpellEffectAnimation(this, this, PortalTeleporterEffectWarn, 0, false, 1);
						foreach (GameNPC tps in m_teleporters)
						{
							GameNPC teleprt = tps;
							ply.Out.SendSpellEffectAnimation(teleprt, teleprt, PortalTeleporterEffectWarn, 0, false, 1);
						}
					}
				}
				
			}
			else if (m_intervalCount % 16 == 11 && PortalTeleporterEffectWarn > 0)
			{
				// first warning
				// Player animation
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					GamePlayer ply = player;
					if (ply != null)
					{
						ply.Out.SendSpellCastAnimation(this, PortalTeleporterEffectWarn, (ushort)((PortalTeleportInterval >> 4)/100));
						foreach (GameNPC tps in m_teleporters)
						{
							GameNPC teleprt = tps;
							ply.Out.SendSpellCastAnimation(teleprt, PortalTeleporterEffectWarn, (ushort)((PortalTeleportInterval >> 4)/100));
						}
					}
				}
			}
			
			return (int)(PortalTeleportInterval >> 4);
		}
		
		public override bool RemoveFromWorld()
		{
			// remove teleporters
			foreach (GameNPC tp in m_teleporters)
			{
				GameNPC teleporter = tp;
				teleporter.RemoveFromWorld();
			}
			
			m_teleporters.Clear();
			
			// remove pad
			m_worldObject.RemoveFromWorld();

			// Stop Timer !
			m_teleportTimer.Stop();
			m_teleportTimer = null;
			
			return base.RemoveFromWorld();
		}
	}
}
