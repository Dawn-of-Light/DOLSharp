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
using System.Linq;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Spells
{
	/// <summary>
	///Handlers for the savage's special endurance heal that takes health instead of mana
	/// </summary>
	[SpellHandlerAttribute("SavageEnduranceHeal")]
	public class SavageEnduranceHeal : EnduranceHealSpellHandler
	{
		public SavageEnduranceHeal(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		protected override void RemoveFromStat(int value)
		{
			m_caster.Health -= value;
		}

		public override int PowerCost(GameLiving target)
		{
			int cost = 0;
			if (m_spell.Power < 0)
				cost = (int)(m_caster.MaxHealth * Math.Abs(m_spell.Power) * 0.01);
			else
				cost = m_spell.Power;
			return cost;
		}

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			int cost = PowerCost(Caster);
			if (Caster.Health < cost)
			{
                MessageToCaster(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SavageEnduranceHeal.CheckBeginCast.InsuffiscientHealth"), eChatType.CT_SpellResisted);
                return false;
			}
			return base.CheckBeginCast(Caster);
		}

        /// <summary>
		/// Return the given Delve Writer with added keyvalue pairs.
        /// Add cost type equal 2 to replace power cost by health cost 
		/// </summary>
		/// <param name="dw"></param>
		/// <param name="id"></param>
		public override void TooltipDelve(ref MiniDelveWriter dw, int id, GameClient client)
        {
            if (dw == null)
                return;

            int level = Spell.Level;
            int spellID = Spell.ID;

            foreach (SpellLine line in client.Player.GetSpellLines())
            {
                Spell s = SkillBase.GetSpellList(line.KeyName).Where(o => o.ID == spellID).FirstOrDefault();
                if (s != null)
                {
                    level = s.Level;
                    break;
                }
            }

            dw.AddKeyValuePair("Function", "light");

            dw.AddKeyValuePair("Index", unchecked((short)id));
            dw.AddKeyValuePair("Name", Spell.Name);

            if (Spell.CastTime > 2000)
                dw.AddKeyValuePair("cast_timer", Spell.CastTime - 2000);
            else if (!Spell.IsInstantCast)
                dw.AddKeyValuePair("cast_timer", 0);

            if (Spell.IsInstantCast)
                dw.AddKeyValuePair("instant", "1");

            if ((int)Spell.DamageType > 0)
                dw.AddKeyValuePair("damage_type", Spell.GetDelveDamageType());

            if (Spell.Level > 0)
                dw.AddKeyValuePair("level", level);
            if (Spell.CostPower)
            {
                dw.AddKeyValuePair("power_cost", Spell.Power);
                dw.AddKeyValuePair("cost_type", 2);
            }


            if (Spell.Range > 0)
                dw.AddKeyValuePair("range", Spell.Range);
            if (Spell.Duration > 0)
                dw.AddKeyValuePair("duration", Spell.Duration / 1000);
            if (GetDurationType() > 0)
                dw.AddKeyValuePair("dur_type", GetDurationType());

            if (Spell.HasRecastDelay)
                dw.AddKeyValuePair("timer_value", Spell.RecastDelay / 1000);

            if (GetSpellTargetType() > 0)
                dw.AddKeyValuePair("target", GetSpellTargetType());

            string description = string.Empty;
            if (!string.IsNullOrEmpty(Spell.Description))
                description = Spell.Description;

            if (Spell.Damage > 0)
            {
                description += string.Format(" Value: ({0})", Spell.Damage);
            }
            else if (Spell.Value > 0)
            {
                description += string.Format(" Value: ({0})", Spell.Value);
            }


            dw.AddKeyValuePair("description_string", description);
            if (Spell.IsAoE)
                dw.AddKeyValuePair("radius", Spell.Radius);
            if (Spell.IsConcentration)
                dw.AddKeyValuePair("concentration_points", Spell.Concentration);
        }
	}
}
