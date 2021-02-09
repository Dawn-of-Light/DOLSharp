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
using DOL.AI.Brain;
using DOL.GS.Effects;

namespace DOL.GS.Spells.Atlantis
{
	/// <summary>
	/// All stats debuff spell handler
	/// </summary>
	[SpellHandlerAttribute("AllStatsDebuff")]
	public class AllStatsDebuff : SpellHandler
	{
		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			effect.Owner.DebuffCategory[(int)eProperty.Dexterity] += (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Strength] += (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Constitution] += (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Acuity] += (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Piety] += (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Empathy] += (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Quickness] += (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Intelligence] += (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Charisma] += (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.ArmorAbsorption] += (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.MagicAbsorption] += (int)m_spell.Value;

			if (effect.Owner is GamePlayer)
			{
				GamePlayer player = effect.Owner as GamePlayer;
				player.Out.SendCharStatsUpdate();
				player.UpdateEncumberance();
				player.UpdatePlayerStatus();
				player.Out.SendUpdatePlayer();
			}
		}
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			effect.Owner.DebuffCategory[(int)eProperty.Dexterity] -= (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Strength] -= (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Constitution] -= (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Acuity] -= (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Piety] -= (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Empathy] -= (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Quickness] -= (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Intelligence] -= (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.Charisma] -= (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.ArmorAbsorption] -= (int)m_spell.Value;
			effect.Owner.DebuffCategory[(int)eProperty.MagicAbsorption] -= (int)m_spell.Value;

			if (effect.Owner is GamePlayer)
			{
				GamePlayer player = effect.Owner as GamePlayer;
				player.Out.SendCharStatsUpdate();
				player.UpdateEncumberance();
				player.UpdatePlayerStatus();
				player.Out.SendUpdatePlayer();
			}
			return base.OnEffectExpires(effect, noMessages);
		}

		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);
			if (target.Realm == 0 || Caster.Realm == 0)
			{
				target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
				Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
			}
			else
			{
				target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
				Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
			}
			if (target is GameNPC)
			{
				var aggroBrain = ((GameNPC)target).Brain as StandardMobBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, (int)Spell.Value);
			}
		}
		public AllStatsDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}