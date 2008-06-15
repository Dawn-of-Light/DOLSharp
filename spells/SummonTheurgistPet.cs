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
using System.Text;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using DOL.GS.Effects;
using log4net;
using System.Reflection;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Summon a theurgist pet.
	/// </summary>
	/// <author>Aredhel</author>
	[SpellHandler("SummonTheurgistPet")]
	public class SummonTheurgistPet : SpellHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public SummonTheurgistPet(GameLiving caster, Spell spell, SpellLine line) 
			: base(caster, spell, line) { }

		/// <summary>
		/// Check whether it's possible to summon a pet.
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <returns></returns>
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			GamePlayer player = Caster as GamePlayer;
			if (player == null)
				return false;

			if (player.PetCounter >= 16)
			{
				MessageToCaster("You have too many controlled creatures!", eChatType.CT_SpellResisted);
				return false;
			}

			return base.CheckBeginCast(selectedTarget);
		}

		/// <summary>
		/// A summon cannot be resisted.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

		/// <summary>
		/// Summon the pet.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			GamePlayer player = Caster as GamePlayer;
			if (player == null)
				return;

			INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.LifeDrainReturn);
			if (template == null)
			{
				String errorMessage = String.Format("NPC template {0} is missing, spell ID = {1}", 
					Spell.LifeDrainReturn, Spell.ID);

				if (log.IsWarnEnabled)
					log.Warn(errorMessage);
				if (player.Client.Account.PrivLevel > 1)
					MessageToCaster(errorMessage, eChatType.CT_Skill);
				
				return;
			}

			int x, y, z;
			Caster.GetSpotFromHeading(64, out x, out y);
			z = Caster.Z;

			GameNPC summoned = new TheurgistPet(template);
			summoned.SetOwnBrain(new TheurgistPetBrain(player));
			summoned.HealthMultiplicator = true;
			summoned.X = x;
			summoned.Y = y;
			summoned.Z = z;
			summoned.CurrentRegion = Caster.CurrentRegion;
			summoned.Heading = Caster.Heading;
			summoned.Realm = Caster.Realm;
			summoned.CurrentSpeed = 0;

			// Set pet level.

			if (Spell.Damage < 0) 
				summoned.Level = (byte)(Caster.Level * Spell.Damage * -0.01);
			else 
				summoned.Level = (byte)Spell.Damage;

			if (summoned.Level > Spell.Value) 
				summoned.Level = (byte)Spell.Value;

			GameSpellEffect effect = CreateSpellEffect(target, effectiveness);

			summoned.AddToWorld();
			
			(summoned.Brain as IAggressiveBrain).AddToAggroList(target, 1);
			(summoned.Brain as TheurgistPetBrain).Think();

			player.PetCounter++;
			effect.Start(summoned);
		}

		/// <summary>
		/// Deduct mana.
		/// </summary>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// Despawn pet.
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="noMessages"></param>
		/// <returns>Immunity timer (in milliseconds).</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			GamePlayer player = Caster as GamePlayer;
			if (player == null)
				return 0;

			if (player.PetCounter > 0)
				player.PetCounter--;

			effect.Owner.Health = 0;
			effect.Owner.Delete();
			return 0;
		}
	}
}

namespace DOL.GS
{
	public class TheurgistPet : GameNPC
	{
		public override int MaxHealth
        {
            get { return Level*20; }
        }
		public override void OnAttackedByEnemy(AttackData ad) { }
		public TheurgistPet(INpcTemplate npcTemplate) : base(npcTemplate) { }
	}
}
