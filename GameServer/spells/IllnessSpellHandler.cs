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
using System.Collections;

using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Pve Resurrection Illness
	/// </summary>
	[SpellHandler(GlobalSpells.PvERessurectionIllnessSpellType)]
	public class PveResurrectionIllness : AbstractIllnessSpellHandler
	{
		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			GamePlayer player = effect.Owner as GamePlayer;
			if (player != null)
			{
				player.Effectiveness -= Spell.Value * 0.01;
				player.Out.SendUpdateWeaponAndArmorStats();
				player.Out.SendStatusUpdate();
			}
		}
		
		/// <summary>
		/// When an applied effect expires.
		/// Duration spells only.
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		/// <returns>immunity duration in milliseconds</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			GamePlayer player = effect.Owner as GamePlayer;
			if (player != null)
			{
				player.Effectiveness += Spell.Value * 0.01;
				player.Out.SendUpdateWeaponAndArmorStats();
				player.Out.SendStatusUpdate();
			}
			return 0;
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo 
		{
			get 
			{
				/*
				<Begin Info: Rusurrection Illness>
 
				The player's effectiveness is greatly reduced due to being recently resurrected.
 
				- Effectiveness penality: 50%
				- 4:56 remaining time
 
				<End Info>
				*/
				ArrayList list = new ArrayList();

				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				list.Add("- Effectiveness penality: "+Spell.Value+"%");
				return list;
			}
		}

		public override void OnEffectRestored(GameSpellEffect effect, int[] vars)
		{
			OnEffectStart(effect);
		}

		public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
		{
			return OnEffectExpires(effect, false);
		}

		public override PlayerXEffect getSavedEffect(GameSpellEffect e)
		{
			PlayerXEffect eff = new PlayerXEffect();
			eff.Var1 = Spell.ID;
			eff.Duration = e.RemainingTime;
			eff.IsHandler = true;
			eff.SpellLine = SpellLine.KeyName;
			return eff;
		}

		public PveResurrectionIllness(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	
	}

	/// <summary>
	/// Contains all common code for illness spell handlers (and negative spell effects without animation) 
	/// </summary>
	public class AbstractIllnessSpellHandler : SpellHandler
	{
		public override bool HasPositiveEffect 
		{
			get 
			{ 
				return false;
			}
		}

		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			double modifier = 1.0;
			RealmAbilities.VeilRecoveryAbility ab = target.GetAbility(typeof(RealmAbilities.VeilRecoveryAbility)) as RealmAbilities.VeilRecoveryAbility;
			if (ab != null)
				modifier -= ((double)ab.Amount / 100);

			return (int)((double)Spell.Duration * modifier); 
		}

		public AbstractIllnessSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	
	}
}
