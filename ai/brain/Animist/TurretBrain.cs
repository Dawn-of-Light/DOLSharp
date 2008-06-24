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
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.AI.Brain
{
	public class TurretBrain : ControlledNpc
	{
		private GameLiving m_target;

		public TurretBrain(GameLiving owner) : base(owner) { }

		public override int ThinkInterval
		{
			get { return 1000; }
		}

		protected override bool CheckDefensiveSpells(Spell spell)
		{
			switch (spell.SpellType)
			{
				case "HeatColdMatterBuff":
				case "BodySpiritEnergyBuff":
				case "ArmorAbsorbtionBuff":
					if (Body.TargetObject == null || !(Body.TargetObject as GameLiving).IsAlive || LivingHasEffect(Body.TargetObject as GameLiving, spell))
					{
						Body.TargetObject = null;

						List<GameLiving> list = new List<GameLiving>();

						foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)spell.Range))
						{
							if (GameServer.ServerRules.IsSameRealm(Body, player, true) && !LivingHasEffect(player, spell))
								list.Add(player);
						}

						foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)spell.Range))
						{
							if (GameServer.ServerRules.IsSameRealm(Body, npc, true) && !LivingHasEffect(npc, spell))
								list.Add(npc);
						}

						if (list.Count > 0)
							Body.TargetObject = list[Util.Random(list.Count - 1)];
					}
					break;
				default:
					return false;
			}

			if (Body.TargetObject != null)
			{
				if (!Body.CastSpell(spell, m_mobSpellLine))
					return false;
				if (Body.TargetObject != Body && spell.CastTime > 0)
					Body.TurnTo(Body.TargetObject);
				return true;
			}

			return false;
		}

		protected override bool CheckOffensiveSpells(Spell spell)
		{
			switch (spell.SpellType)
			{
				case "DirectDamage":
				case "DamageSpeedDecrease":
				case "SpeedDecrease":
				case "MeleeDamageDebuff":
				case "Taunt":
					if (Body.TargetObject == null || !(Body.TargetObject as GameLiving).IsAlive)
					{
						Body.TargetObject = null;

						List<GameLiving> list = new List<GameLiving>();

						foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)spell.Range))
						{
							if (GameServer.ServerRules.IsAllowedToAttack(Body, player, true) && !player.IsStealthed && !LivingHasEffect(player, spell))
								list.Add(player);
						}

						foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)spell.Range))
						{
							if (GameServer.ServerRules.IsAllowedToAttack(Body, npc, true) && !LivingHasEffect(npc, spell))
								list.Add(npc);
						}

						if (list.Count > 0)
							Body.TargetObject = list[Util.Random(list.Count - 1)];
					}
					break;
				default:
					return false;
			}

			if (Body.TargetObject != null)
			{
				if (!Body.CastSpell(spell, m_mobSpellLine))
					return false;
				if (Body.TargetObject != Body && spell.CastTime > 0)
					Body.TurnTo(Body.TargetObject);
				return true;
			}

			return false;
		}

		#region Think
		public override void Think()
		{
			CheckSpells(eCheckSpellType.Offensive);
			CheckSpells(eCheckSpellType.Defensive);
		}
		#endregion
	}
}