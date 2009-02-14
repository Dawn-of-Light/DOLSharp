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
//Eden - 1.94 RR5 Paladin

using System;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Effects
{
	public class SelflessDevotionEffect : TimedEffect
	{
		public SelflessDevotionEffect() : base(15000)
		{
			m_healpulse = 5;
		}

		private GamePlayer owner;
		private RegionTimer m_timer = null;
		private int m_healpulse;
		private Dictionary<eProperty, int> m_debuffs;

		public override void Start(GameLiving target)
		{
			base.Start(target);

			owner = target as GamePlayer;
			if (owner == null) return;

			foreach (GamePlayer p in owner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				p.Out.SendSpellEffectAnimation(owner, owner, Icon, 0, false, 1);

			m_debuffs = new Dictionary<eProperty, int>(1+eProperty.Stat_Last-eProperty.Stat_First);
			
			for (eProperty property = eProperty.Stat_First; property <= eProperty.Stat_Last; property++)
			{
				m_debuffs.Add(property, (int)(owner.GetModified(property) * 0.25));
				owner.DebuffCategory[(int)property] += m_debuffs[property];
			}

			owner.Out.SendCharStatsUpdate();
			
			m_timer = new RegionTimer(owner, new RegionTimerCallback(HealPulse));
			m_timer.Start(1);
		}
		
		public int HealPulse(RegionTimer timer)
		{
			if (m_healpulse > 0)
			{
				m_healpulse--;
				
				GamePlayer player = Owner as GamePlayer;
				if (player == null) return 0;
				if (player.Group == null) return 3000;
				
				foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
				{
					if (p.Health < p.MaxHealth && player.IsWithinRadius(p, 750) && p.IsAlive)
					{
						if (player.IsStealthed)
							player.Stealth(false);

						int heal = 300;
						
						if (p.Health + heal > p.MaxHealth)
							heal = p.MaxHealth - p.Health;
							
						p.ChangeHealth(player, GameLiving.eHealthChangeType.Regenerate, heal);
						
						player.Out.SendMessage("You heal " + p.Name + " for " + heal.ToString() + " hit points.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
						p.Out.SendMessage(player.Name + " heals you for " + heal.ToString() + " hit points.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
				}
				return 3000;
			}
			return 0;
		}

		public override void Stop()
		{
			base.Stop();
			
			if (owner != null)
			{
				for (eProperty property = eProperty.Stat_First; property <= eProperty.Stat_Last; property++)
				{
					if (m_debuffs.ContainsKey(property))
						owner.DebuffCategory[(int)property] -= m_debuffs[property];
				}
				owner.Out.SendCharStatsUpdate();
			}
			
			if (m_timer != null)
			{
				m_timer.Stop();
				m_timer = null;
			}
		}

		public override string Name { get { return "Selfless Devotion"; } }

		public override ushort Icon { get { return 3038; } }

		public int SpellEffectiveness { get { return 100; } }

		public override System.Collections.IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
				list.Add("Decrease Paladin stats by 25%, and pulse a 300 points group heal with a 750 units range every 3 seconds for 15 seconds total.");
				return list;
			}
		}
	}
}