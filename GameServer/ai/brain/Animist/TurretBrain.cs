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
using System.Collections.Generic;
using DOL.GS;

namespace DOL.AI.Brain
{
	public class TurretBrain : ControlledNpcBrain
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
			get { return 1500; }
		}


		/// <summary>
		/// [Ganrod] Nidel:
		/// Cast only Offensive or Defensive spell.
		/// <para>If Offensive spell is true, Defensive spell isn't casted.</para>
		/// </summary>
		public override void Think()
		{
		  GamePlayer playerowner = GetPlayerOwner();
		  if (!playerowner.CurrentUpdateArray[Body.ObjectID - 1])
		  {
			playerowner.Out.SendObjectUpdate(Body);
			playerowner.CurrentUpdateArray[Body.ObjectID - 1] = true;
		  }

		  if(!CheckSpells(eCheckSpellType.Defensive))
		  {
		  	AttackMostWanted();
		  }
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
			Spell spell = ((TurretPet) Body).TurretSpell;

			switch (type)
			{
				case eCheckSpellType.Defensive:
					return CheckDefensiveSpells(spell);
				case eCheckSpellType.Offensive:
					return CheckOffensiveSpells(spell);
			}
			return false;
		}

		protected override bool CheckDefensiveSpells(Spell spell)
		{
			switch(spell.SpellType)
			{
				case "HeatColdMatterBuff":
				case "BodySpiritEnergyBuff":
				case "ArmorAbsorptionBuff":
				case "AblativeArmor":
				  TrustCast(spell, eCheckSpellType.Defensive);
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
					TrustCast(spell, eCheckSpellType.Offensive);
					return true;
			}
			return false;
		}

		protected override void AttackMostWanted()
		{
			CheckSpells(eCheckSpellType.Offensive);
		}

		public bool TrustCast(Spell spell, eCheckSpellType type)
		{
			if(AggressionState == eAggressionState.Passive)
				return false;
			if(Body.GetSkillDisabledDuration(spell) != 0)
			{
				return false;
			}

			if(spell.Radius == 0 && spell.Range > 0)
			{
				GameLiving target;
				if(type == eCheckSpellType.Defensive)
				{
					target = GetDefensiveTarget(spell);
				}
				else
				{
					CheckPlayerAggro();
					CheckNPCAggro();
					target = CalculateNextAttackTarget();
				}
                if ( target != null && Body.IsWithinRadius( target, spell.Range ) )
				{
					if(!Body.IsAttacking || target != Body.TargetObject)
					{
						Body.TargetObject = target;
						if(spell.CastTime > 0)
						{
							Body.TurnTo(Body.TargetObject);
						}
						Body.CastSpell(spell, m_mobSpellLine);
					}
				}
				else
				{
					if(Body.IsAttacking)
					{
						Body.StopAttack();
					}
					if(Body.SpellTimer != null && Body.SpellTimer.IsAlive)
					{
						Body.SpellTimer.Stop();
					}
					return false;
				}
			}
			else //Radius spell don't need target
			{
				Body.CastSpell(spell, m_mobSpellLine);
			}
			return true;
		}

		/// <summary>
		/// [Ganrod] Nidel: Find and get random target in radius for Defensive spell, like 1.90 EU off servers.
		/// <para>Get target only if:</para>
		/// <para>- same realm (based on ServerRules)</para>
		/// <para>- don't have effect</para>
		/// <para>- is alive</para>
		/// </summary>
		/// <param name="spell"></param>
		/// <returns></returns>
		public GameLiving GetDefensiveTarget(Spell spell)
		{
			foreach(GamePlayer player in Body.GetPlayersInRadius((ushort) spell.Range))
			{
				if(!GameServer.ServerRules.IsSameRealm(Body, player, false))
					continue;

				if(!player.IsAlive)
					continue;

				if(LivingHasEffect(player, spell))
				{
					if(ListDefensiveTarget.Contains(player))
					{
						ListDefensiveTarget.Remove(player);
					}
					continue;
				}

				if(player == GetPlayerOwner())
					return player;

				ListDefensiveTarget.Add(player);
			}
			foreach(GameNPC npc in Body.GetNPCsInRadius((ushort) spell.Range))
			{
				if(!GameServer.ServerRules.IsSameRealm(Body, npc, false))
					continue;

				if(!npc.IsAlive)
					continue;

				if(LivingHasEffect(npc, spell))
				{
					if(ListDefensiveTarget.Contains(npc))
					{
						ListDefensiveTarget.Remove(npc);
					}
					continue;
				}

				if(npc == Body)
				{
					return Body;
				}

				ListDefensiveTarget.Add(npc);
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