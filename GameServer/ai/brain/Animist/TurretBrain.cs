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
/*
 * [Ganrod] Nidel 2008-07-08
 * - useless using removed
 * - list renamed to listTarget
 * - List listTarget initialized when brain created
 * - Clear listTarget when target is selected
 * - Add GetTarget, TrustCast methods
 * - Turret doesn't need target for a PBAoE spell
 */
using System.Collections.Generic;
using DOL.GS;

namespace DOL.AI.Brain
{
	public class TurretBrain : ControlledNpc
	{
		private readonly List<GameLiving> listDefensiveTarget;

		public TurretBrain(GameLiving owner) : base(owner)
		{
			listDefensiveTarget = new List<GameLiving>();
		}

		public List<GameLiving> ListDefensiveTarget
		{
			get { return listDefensiveTarget; }
		}

		public override int ThinkInterval
		{
			get { return 1000; }
		}

		public override bool CheckSpells(eCheckSpellType type)
		{
			if(Body == null || ((TurretPet) Body).TurretSpell == null)
			{
				return false;
			}

			if(Body.IsCasting)
			{
				return true;
			}

			if(type == eCheckSpellType.Defensive)
			{
				return CheckDefensiveSpells(((TurretPet) Body).TurretSpell);
			}
			// Offensive
			return CheckOffensiveSpells(((TurretPet) Body).TurretSpell);
		}

		protected override bool CheckDefensiveSpells(Spell spell)
		{
			switch(spell.SpellType)
			{
				case "HeatColdMatterBuff":
				case "BodySpiritEnergyBuff":
				case "ArmorAbsorbtionBuff":
				case "AblativeArmor":
					// don't need target for radius spell
					if(((TurretPet) Body).TurretSpell.Radius == 0)
					{
						Body.TargetObject = GetDefensiveTarget(((TurretPet) Body).TurretSpell);
					}
					TrustCast(((TurretPet) Body).TurretSpell);
					return true;
			}
			return false;
		}

		protected override bool CheckOffensiveSpells(Spell spell)
		{
			switch(spell.SpellType)
			{
				case "DirectDamage":
				case "DamageSpeedDecrease":
				case "SpeedDecrease":
				case "Taunt":
				case "MeleeDamageDebuff":
					AttackMostWanted();
					return true;
			}
			return false;
		}

		public bool TrustCast(Spell spell)
		{
			if(Body.GetSkillDisabledDuration(((TurretPet) Body).TurretSpell) != 0)
			{
				return false;
			}

			if(spell.Radius > 0)
			{
				Body.CastSpell(spell, m_mobSpellLine);
				return true;
			}

			if(Body.TargetObject != null)
			{
				if(Body.TargetObject != Body && spell.CastTime > 0)
				{
					Body.TurnTo(Body.TargetObject);
				}
				Body.CastSpell(spell, m_mobSpellLine);
				return true;
			}

			if(Body.SpellTimer != null && Body.SpellTimer.IsAlive)
			{
				Body.SpellTimer.Stop();
			}
			return false;
		}

		/// <summary>
		/// [Ganrod] Nidel: Find and get random target in radius for Defensive spell, like 1.90 EU off servers.
		/// </summary>
		/// <param name="spell"></param>
		/// <returns></returns>
		public GameLiving GetDefensiveTarget(Spell spell)
		{
			if(Body.TargetObject == null || !((GameLiving) Body.TargetObject).IsAlive || LivingHasEffect(Body.TargetObject as GameLiving, spell))
			{
				Body.TargetObject = null;

				foreach(GamePlayer player in Body.GetPlayersInRadius((ushort) spell.Range))
				{
					//Buff owner at first time if in radius
					if(player == GetPlayerOwner() && !LivingHasEffect(player, spell))
					{
						return player;
					}
					if(ListDefensiveTarget.Contains(player) || LivingHasEffect(player, spell))
					{
						if(LivingHasEffect(player, spell))
						{
							ListDefensiveTarget.Remove(player);
						}
						continue;
					}
					if(GameServer.ServerRules.IsSameRealm(Body, player, false))
					{
						ListDefensiveTarget.Add(player);
					}
				}
				foreach(GameNPC npc in Body.GetNPCsInRadius((ushort) spell.Range))
				{
					if(npc == Body && !LivingHasEffect(Body, spell))
					{
						return Body;
					}
					if(ListDefensiveTarget.Contains(npc) || LivingHasEffect(npc, spell))
					{
						if(LivingHasEffect(npc, spell))
						{
							ListDefensiveTarget.Remove(npc);
						}
						continue;
					}
					if(GameServer.ServerRules.IsSameRealm(Body, npc, false))
					{
						ListDefensiveTarget.Add(npc);
					}
				}
			}
			// Get one random target.
			return ListDefensiveTarget.Count > 0 ? ListDefensiveTarget[Util.Random(ListDefensiveTarget.Count - 1)] : null;
		}

		public override bool Stop()
		{
			ClearAggroList();
			ListDefensiveTarget.Clear();
			return base.Stop();
		}

		#region AI

		public override void FollowOwner()
		{
		}

		public override void Follow(GameObject target)
		{
		}

		protected override void OnFollowLostTarget(GameObject target)
		{
		}

		public override void Goto(GameObject target)
		{
		}

		public override void ComeHere()
		{
		}

		public override void Stay()
		{
		}

		#endregion
	}
}