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
using System.Collections;

using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.GS.Keeps;

namespace DOL.GS
{
	/// <summary>
	/// GameMovingObject is a base class for boats and siege weapons.
	/// </summary>
	public class GameSiegeCauldron : GameSiegeWeapon
	{
		public GameKeepComponent Component = null;

		public GameSiegeCauldron()
			: base()
		{
			MeleeDamageType = eDamageType.Heat;
			Name = "cauldron of boiling oil";
			AmmoType = 0x3B;
			EnableToMove = false;
			Effect = 0x8A1;
			Model = 0xA2F;
			CurrentState = eState.Aimed;
			SetGroundTarget(X, Y, Z - 100);
			ActionDelay = new int[]
                {
                    0, //none
                    0, //aiming
                    15000, //arming
                    0, //loading
                    1000 //fireing
                }; //en ms
		}

		public override bool AddToWorld()
		{
			SetGroundTarget(X, Y, Component.Keep.Z);
			return base.AddToWorld();
		}

		public override void DoDamage()
		{
			//todo remove ammo + spell in db and uncomment
			//m_spellHandler.StartSpell(player);
			base.DoDamage(); //anim mut be called after damage
			CastSpell(OilSpell, SiegeSpellLine);
		}

		private static Spell m_OilSpell;

		public static Spell OilSpell
		{
			get
			{
				if (m_OilSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 2;
					spell.ClientEffect = 2209; //2209? 5909? 7086? 7091?
					spell.Damage = 1000;
					spell.DamageType = (int)eDamageType.Heat;
					spell.Name = "Boiling Oil";
					spell.Radius = 350;
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 50005;
					spell.Target = "Area";
					spell.Type = "SiegeDirectDamage";
					m_OilSpell = new Spell(spell, 50);
				}
				return m_OilSpell;
			}
		}
	}
}

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
	[SpellHandlerAttribute("SiegeDirectDamage")]
	public class SiegeDirectDamageSpellHandler : DirectDamageSpellHandler
	{

		/// <summary>
		/// Calculates chance of spell getting resisted
		/// </summary>
		/// <param name="target">the target of the spell</param>
		/// <returns>chance that spell will be resisted for specific target</returns>
		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

		public override int CalculateToHitChance(GameLiving target)
		{
			return 100;
		}

		public override bool CasterIsAttacked(GameLiving attacker)
		{
			return false;
		}

		public override void CalculateDamageVariance(GameLiving target, out double min, out double max)
		{
			min = 1;
			max = 1;
		}

		public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
		{
			AttackData ad = base.CalculateDamageToTarget(target, effectiveness);
			if (target is GamePlayer)
			{
				GamePlayer player = target as GamePlayer;
				int id = player.CharacterClass.ID;
				//50% reduction for tanks
				if (id == (int)eCharacterClass.Armsman || id == (int)eCharacterClass.Warrior || id == (int)eCharacterClass.Hero)
					ad.Damage /= 2;
				//3000 spec
				//ram protection
				//lvl 0 50%
				//lvl 1 60%
				//lvl 2 70%
				//lvl 3 80%
				if (player.IsRiding && player.Steed is GameSiegeRam)
				{
					ad.Damage = (int)((double)ad.Damage * (1.0 - (50.0 + (double)player.Steed.Level * 10.0) / 100.0));
				}
			}
			return ad;
		}

		public override void SendDamageMessages(AttackData ad)
		{
			string modmessage = "";
			if (ad.Modifier > 0)
				modmessage = " (+" + ad.Modifier + ")";
			if (ad.Modifier < 0)
				modmessage = " (" + ad.Modifier + ")";

			if (Caster is GameSiegeWeapon)
			{
				GameSiegeWeapon siege = (Caster as GameSiegeWeapon);
				if (siege.Owner != null)
				{
					siege.Owner.Out.SendMessage(string.Format("You hit {0} for {1}{2} damage!", ad.Target.GetName(0, false), ad.Damage, modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				}
			}
		}

		public override void DamageTarget(AttackData ad, bool showEffectAnimation, int attackResult)
		{
			if (Caster is GameSiegeWeapon)
			{
				GameSiegeWeapon siege = (Caster as GameSiegeWeapon);
				if (siege.Owner != null)
				{
					ad.Attacker = siege.Owner;
				}
			}
			base.DamageTarget(ad, showEffectAnimation, attackResult);
		}

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			return true;
		}

		public override bool CheckEndCast(GameLiving target)
		{
			return true;
		}


		// constructor
		public SiegeDirectDamageSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}