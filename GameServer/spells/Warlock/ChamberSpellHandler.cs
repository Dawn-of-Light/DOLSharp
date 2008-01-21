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
using System.Reflection;
using System.Text;
using DOL.AI.Brain;
using DOL.Database2;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using log4net;

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
	[SpellHandlerAttribute("Chamber")]
	public class ChamberSpellHandler : SpellHandler
	{
        public const string CHAMBER_USE_TICK = "ChamberUseTick";

		private Spell m_primaryspell = null;
		private SpellLine m_primaryspellline = null;
		private Spell m_secondaryspell = null;
		private SpellLine m_secondaryspelline = null;
		private int m_effectslot = 0;

		public Spell PrimarySpell
		{
			get
			{
				return m_primaryspell;
			}
			set
			{
				m_primaryspell = value;
			}
		}

		public SpellLine PrimarySpellLine
		{
			get
			{
				return m_primaryspellline;
			}
			set
			{
				m_primaryspellline = value;
			}
		}

		public Spell SecondarySpell
		{
			get
			{
				return m_secondaryspell;
			}
			set
			{
				m_secondaryspell = value;
			}
		}

		public SpellLine SecondarySpellLine
		{
			get
			{
				return m_secondaryspelline;
			}
			set
			{
				m_secondaryspelline = value;
			}
		}

		public int EffectSlot
		{
			get
			{
				return m_effectslot;
			}
			set
			{
				m_effectslot = value;
			}
		}
        public override void InterruptCasting()
        {
            base.InterruptCasting();
            Caster.CurrentSpellHandler = null;
        }
		public override void CastSpell()
		{
            GamePlayer player = (GamePlayer)m_caster;
            long ChamberUseTick = player.TempProperties.getLongProperty(CHAMBER_USE_TICK, 0L);
            long changeTime = player.CurrentRegion.Time - ChamberUseTick;
            if (changeTime < 3000)
            {
                MessageToCaster("You must wait " + ((3000 - changeTime) / 1000).ToString() + " more second to attempt to use a chamber!", eChatType.CT_System);
                return;
            }
            player.TempProperties.setProperty(CHAMBER_USE_TICK, player.CurrentRegion.Time);

            int duration = ((GamePlayer)m_caster).GetSkillDisabledDuration(m_spell);
			if(duration > 0)
			{
				MessageToCaster("You must wait "+(duration/10+1)+" seconds to use this spell!", eChatType.CT_System);
				return;
			}

            GameSpellEffect effect = SpellHandler.FindEffectOnTarget(m_caster, "Chamber", m_spell.Name);
			if(effect != null && m_spell.Name == effect.Spell.Name)
			{
				GamePlayer caster = (GamePlayer)m_caster;
                ISpellHandler spellhandler = null;
                ISpellHandler spellhandler2 = null;
				ChamberSpellHandler chamber = (ChamberSpellHandler)effect.SpellHandler;
				spellhandler = ScriptMgr.CreateSpellHandler(m_caster, chamber.PrimarySpell, chamber.PrimarySpellLine);
                if (player.IsMoving || player.IsStrafing)
                {
                    MessageToCaster("You must be standing still to cast this spell!", eChatType.CT_System);
                    return;
                }
				if(m_caster.TargetObject==null)
				{
					MessageToCaster("You must have a target!", eChatType.CT_SpellResisted);
					return;
				}

				if (WorldMgr.GetDistance(m_caster, m_caster.TargetObject) > ((SpellHandler)spellhandler).CalculateSpellRange()) 
				{
					MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
					return;
				}

				spellhandler.CastSpell();

				if(chamber.SecondarySpell != null)
				{
					spellhandler2 = ScriptMgr.CreateSpellHandler(m_caster, chamber.SecondarySpell, chamber.SecondarySpellLine);
					spellhandler2.CastSpell();
				}
				effect.Cancel(false);

				foreach(Spell oSpell in caster.GetUsableSpellsOfLine(chamber.SpellLine))
				{
					if(oSpell.SpellType.ToLower()=="chamber")
					{
						caster.DisableSkill(oSpell, 4);
					}
				}
			}
			else
			{
				base.CastSpell ();
				if(this.Caster is GamePlayer)
					((GamePlayer)this.Caster).Out.SendMessage("Select the first spell for your " + this.Spell.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Fire bolt
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			
			// endurance
			m_caster.Endurance -= 5;

			// messages
			if (Spell.InstrumentRequirement == 0)
			{
				if(SecondarySpell == null && PrimarySpell == null)
				{
					MessageToCaster("No spells were loaded into " + m_spell.Name + ".", eChatType.CT_Spell);
				}
				else
				{
					MessageToCaster("Your " + m_spell.Name + " is ready for use.", eChatType.CT_Spell);
					//StartSpell(target); // and action
					GameSpellEffect neweffect = CreateSpellEffect(target, 1);
					neweffect.Start(m_caster);
					SendEffectAnimation(m_caster, 0, false, 1);
					((GamePlayer)m_caster).Out.SendWarlockChamberEffect((GamePlayer)m_caster);
				}
				
				foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
				{
					if (player != m_caster)
						player.Out.SendMessage(m_caster.GetName(0, true) + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
			}

			//the quick cast is unallowed whenever you miss the spell	
			//set the time when casting to can not quickcast during a minimum time
			if (m_caster is GamePlayer)
			{
				QuickCastEffect quickcast = (QuickCastEffect) m_caster.EffectList.GetOfType(typeof (QuickCastEffect));
				if (quickcast != null && Spell.CastTime > 0)
				{
					((GamePlayer) m_caster).TempProperties.setProperty(GamePlayer.QUICK_CAST_CHANGE_TICK, m_caster.CurrentRegion.Time);
					((GamePlayer) m_caster).DisableSkill(SkillBase.GetAbility(Abilities.Quickcast), QuickCastAbilityHandler.DISABLE_DURATION);
					quickcast.Cancel(false);
				}
			}

			// disable spells with recasttimer (Disables group of same type with same delay)
			if (m_spell.RecastDelay > 0 && m_startReuseTimer && m_caster is GamePlayer)
			{
				foreach (Spell sp in SkillBase.GetSpellList(m_spellLine.KeyName))
				{
					if (sp.SpellType == m_spell.SpellType && sp.RecastDelay == m_spell.RecastDelay && sp.Group == m_spell.Group)
					{
						((GamePlayer) m_caster).DisableSkill(sp, sp.RecastDelay);
					}
				}
			}
		}


		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{

			((GamePlayer)m_caster).Out.SendWarlockChamberEffect((GamePlayer)effect.Owner);
			return base.OnEffectExpires (effect, noMessages);
		}


		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			return new GameSpellEffect(this, 0, 0, effectiveness);
		}

		public static int GetEffectSlot(string spellName)
		{
			switch(spellName)
			{
				case "Chamber of Minor Fate":
					return 1;
				case "Chamber of Restraint":
					return 2;
				case "Chamber of Destruction":
					return 3;
				case "Chamber of Fate":
					return 4;
				case "Chamber of Greater Fate":
					return 5;
			}

			return 0;
		}

		// constructor
		public ChamberSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
