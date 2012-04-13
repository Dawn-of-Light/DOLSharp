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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("Chamber")]
	public class ChamberSpellHandler : SpellHandler
	{
		public const string CHAMBER_USE_TICK = "ChamberUseTick";
		private Spell m_primaryspell = null;
		private SpellLine m_primaryspellline = null;
		private Spell m_secondaryspell = null;
		private SpellLine m_secondaryspelline = null;
		private bool m_calculated = false;

		public override int CalculateCastingTime()
		{
			int ticks = m_spell.CastTime;

			if (ticks > 0)
			{
				if ((m_caster.LastCombatTickPvP > 0 && m_caster.CurrentRegion.Time - m_caster.LastCombatTickPvP < 20000)
				|| (m_caster.LastCombatTickPvE > 0 && m_caster.CurrentRegion.Time - m_caster.LastCombatTickPvE < 20000))
				{
					if (m_caster is GamePlayer && (m_caster as GamePlayer).Client.Account.PrivLevel == 3) return 3000;
					return ticks;
				}
				{
					if (!m_calculated)
					{
						//[Eden] that's custom, but absolutely not a big deal
						MessageToCaster("As you were not in combat the last 20 seconds, the chamber cast time is reduced.", eChatType.CT_SpellExpires);
						m_calculated = true;
					}
					return 3000;
				}
			}

			return 0;
		}

		public Spell PrimarySpell
		{
			get { return m_primaryspell; }
			set { m_primaryspell = value; }
		}

		public SpellLine PrimarySpellLine
		{
			get { return m_primaryspellline; }
			set { m_primaryspellline = value; }
		}

		public Spell SecondarySpell
		{
			get { return m_secondaryspell; }
			set { m_secondaryspell = value; }
		}

		public SpellLine SecondarySpellLine
		{
			get { return m_secondaryspelline; }
			set { m_secondaryspelline = value; }
		}

		public int EffectSlot
		{
			get { return GetEffectSlot(Spell.Name); }
		}

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget))
				return false;

			GamePlayer player = (GamePlayer)m_caster;
			if (player == null) return false;

			if (player.IsMoving)
			{
				MessageToCaster("You move and interrupt your spellcast!", eChatType.CT_System);
				return false;
			}

			long ChamberUseTick = player.TempProperties.getProperty<long>(CHAMBER_USE_TICK, 0L);
			long changeTime = player.CurrentRegion.Time - ChamberUseTick;
			if (Spell.CastTime == 0)
			{
				if (changeTime < 10000)
				{
					MessageToCaster("You must wait " + ((10000 - changeTime) / 1000).ToString() + " more second to attempt to use a chamber!", eChatType.CT_System);
					return false;
				}
			}

			int duration = ((GamePlayer)m_caster).GetSkillDisabledDuration(m_spell);
			if (duration > 0)
			{
				MessageToCaster("You must wait " + (duration / 10 + 1) + " seconds to use this spell!", eChatType.CT_System);
				return false;
			}

			GameSpellEffect effect = SpellHandler.FindEffectOnTarget(m_caster, "Chamber", m_spell.Name);
			if (effect != null && m_spell.Name == effect.Spell.Name)
			{
				if (selectedTarget == null)
				{
					MessageToCaster("You must have a target!", eChatType.CT_SpellResisted);
					return false;
				}
				ISpellHandler spellhandler1 = null;
				ISpellHandler spellhandler2 = null;
				ChamberSpellHandler chamber = (ChamberSpellHandler)effect.SpellHandler;
				spellhandler1 = ScriptMgr.CreateSpellHandler(player, chamber.PrimarySpell, chamber.PrimarySpellLine);
				if (!Caster.IsWithinRadius(selectedTarget, ((SpellHandler)spellhandler1).CalculateSpellRange()))
				{
					MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
					return false;
				}
				bool casted = false;
				if (spellhandler1 != null)
					casted = spellhandler1.CastSpell(selectedTarget);
				if (chamber.SecondarySpell != null)
				{
					spellhandler2 = ScriptMgr.CreateSpellHandler(player, chamber.SecondarySpell, chamber.SecondarySpellLine);
					if (spellhandler2 != null)
						casted |= spellhandler2.CastSpell(selectedTarget);
				}
				if (casted)
				{
					player.TempProperties.setProperty(CHAMBER_USE_TICK, player.CurrentRegion.Time);

					effect.Cancel(false);
					foreach (KeyValuePair<Spell, SpellLine> oPair in player.GetUsableSpells(new List<SpellLine> { chamber.SpellLine }, false).Values)// .GetUsableSpellsOfLine(chamber.SpellLine))
					{
						if (oPair.Key.SpellType.ToLower() == "chamber")
						{
							player.DisableSkill(oPair.Key, 4);
						}
					}
				}
				return false;
			}
			return true;
		}

		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);

			if (SecondarySpell == null && PrimarySpell == null)
			{
				MessageToCaster("No spells were loaded into " + m_spell.Name + ".", eChatType.CT_Spell);
			}
			else
			{
				MessageToCaster("Your " + m_spell.Name + " is ready for use.", eChatType.CT_Spell);
				GameSpellEffect neweffect = CreateSpellEffect(target, 1);
				neweffect.Start(m_caster);
				SendEffectAnimation(m_caster, 0, false, 1);
				foreach (GamePlayer plr in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					plr.Out.SendWarlockChamberEffect((GamePlayer)m_caster);
			}
			foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (player != m_caster)
					player.Out.SendMessage(m_caster.GetName(0, true) + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			}
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			foreach (GamePlayer plr in ((GamePlayer)effect.Owner).GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				plr.Out.SendWarlockChamberEffect((GamePlayer)effect.Owner);
			return base.OnEffectExpires(effect, noMessages);
		}


		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			return new GameSpellEffect(this, 0, 0, effectiveness);
		}

		public int GetEffectSlot(string spellName)
		{
			switch (spellName)
			{
				case "Chamber of Restraint":
					return 1;
				case "Chamber of Creation":
					return 2;
				case "Chamber of Destruction":
					return 3;
				case "Chamber of Decimation":
					return 4;
				case "Chamber of Lesser Fate":
					return 5;
				case "Chamber of Greater Fate":
					return 6;
			}

			return 0;
		}

		public ChamberSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}
