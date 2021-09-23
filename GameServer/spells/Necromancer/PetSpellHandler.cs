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
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.Language;
using DOL.GS.PacketHandler.Client.v168;

namespace DOL.GS.Spells
{
	[SpellHandler("PetSpell")]
	class PetSpellHandler : SpellHandler
	{
		public override bool CastSpell()
		{
			m_spellTarget = Caster.TargetObject as GameLiving;
			bool casted = true;

			if (GameServer.ServerRules.IsAllowedToCastSpell(Caster, m_spellTarget, Spell, SpellLine) && CheckBeginCast(m_spellTarget))
			{
				if (Spell.CastTime > 0)
				{
					StartCastTimer(m_spellTarget);
				}
				else
				{
					FinishSpellCast(m_spellTarget);
				}
			}
			else
				casted = false;

			if (!IsCasting)
				OnAfterSpellCastSequence();

			return casted;
		}

		public override int CalculateCastingTime()
		{
			int ticks = m_spell.CastTime;
			ticks = (int)(ticks * Math.Max(m_caster.CastingSpeedReductionCap, m_caster.DexterityCastTimeReduction));
			if (ticks < m_caster.MinimumCastingSpeed)
				ticks = m_caster.MinimumCastingSpeed;
			return ticks;
		}

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget))
				return false;

			if (Caster.ControlledBrain == null)
			{
				MessageToCaster(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "PetSpellHandler.CheckBeginCast.NoControlledBrainForCast"), eChatType.CT_SpellResisted);
				return false;
			}
			return true;
		}

		public override void FinishSpellCast(GameLiving target)
		{
			GamePlayer player = Caster as GamePlayer;

			if (player == null || player.ControlledBrain == null)
				return;

			// No power cost, we'll drain power on the caster when
			// the pet actually starts casting it.
			// If there is an ID, create a sub spell for the pet.

			ControlledNpcBrain petBrain = player.ControlledBrain as ControlledNpcBrain;
			if (petBrain != null && Spell.SubSpellID > 0)
			{
				Spell spell = SkillBase.GetSpellByID(Spell.SubSpellID);
				if (spell != null && spell.SubSpellID == 0)
				{
					spell.Level = Spell.Level;
					petBrain.Notify(GameNPCEvent.PetSpell, this,
						new PetSpellEventArgs(spell, SpellLine, target));
				}
			}

			// Facilitate Painworking.

			if (Spell.RecastDelay > 0 && m_startReuseTimer)
			{
				foreach (Spell spell in SkillBase.GetSpellList(SpellLine.KeyName))
				{
					if (spell.SpellType == Spell.SpellType &&
						spell.RecastDelay == Spell.RecastDelay
						&& spell.Group == Spell.Group)
						Caster.DisableSkill(spell, spell.RecastDelay);
				}
			}
		}

		public PetSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine) { }

		public override string ShortDescription
		{
			get
			{
				var subSpell = ScriptMgr.CreateSpellHandler(m_caster, SkillBase.GetSpellByID(Spell.SubSpellID), null);
				var subSpellDelveInfo = "";
				foreach (var line in subSpell.DelveInfo) subSpellDelveInfo += line + "\n";
				return "Pet Spell: \n"
				+ subSpellDelveInfo;
			}
		}
    }
}
