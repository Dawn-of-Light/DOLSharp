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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using System.Collections;
using System.Reflection;
using DOL.Language;
using System.Collections.Generic;

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
	[SpellHandlerAttribute("Chamber")]
	public class ChamberSpellHandler : SpellHandler
	{
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
		public override bool CastSpell()
		{
			GamePlayer caster = (GamePlayer)m_caster;
			GameLiving target = caster.TargetObject as GameLiving;
			GameSpellEffect effect = SpellHandler.FindEffectOnTarget(caster, "Chamber", m_spell.Name);
			if(effect != null && m_spell.Name == effect.Spell.Name)
			{
				ISpellHandler spellhandler = null;
				ISpellHandler spellhandler2 = null;
				ChamberSpellHandler chamber = (ChamberSpellHandler)effect.SpellHandler;
				GameSpellEffect PhaseShift = SpellHandler.FindEffectOnTarget(target, "Phaseshift");
                SelectiveBlindnessEffect SelectiveBlindness = (SelectiveBlindnessEffect)Caster.EffectList.GetOfType(typeof(SelectiveBlindnessEffect));
				spellhandler = ScriptMgr.CreateSpellHandler(caster, chamber.PrimarySpell, chamber.PrimarySpellLine);

				#region Pre-checks
                int duration = caster.GetSkillDisabledDuration(m_spell);
                if (duration > 0)
                {
                    MessageToCaster("You must wait " + (duration / 1000 + 1) + " seconds to use this spell!", eChatType.CT_System);
                    return false;
                }
				if (caster.IsMoving || caster.IsStrafing)
				{
					MessageToCaster("You must be standing still to cast this spell!", eChatType.CT_System);
					return false;
				}
                if (caster.IsSitting)
                {
                    MessageToCaster("You can't cast this spell while sitting!", eChatType.CT_System);
                    return false;
                }
				if (caster.TargetObject == null)
				{
					MessageToCaster("You must have a target!", eChatType.CT_SpellResisted);
					return false;
				}
				if (!caster.IsAlive)
				{
					MessageToCaster("You cannot cast this dead!", eChatType.CT_SpellResisted);
					return false;
				}
				if (!target.IsAlive)
				{
					MessageToCaster("You cannot cast this on the dead!", eChatType.CT_SpellResisted);
					return false;
				}
				if (caster.IsMezzed || caster.IsStunned || caster.IsSilenced)
				{
					MessageToCaster("You can't use that in your state.", eChatType.CT_System);
					return false;
				}
				if (!caster.TargetInView)
				{
					MessageToCaster("Your target is not in view!", eChatType.CT_System);
					return false;
				}
				if (caster.IsPvPInvulnerability)
				{
					MessageToCaster("Your invunerable at the momment and cannot use that spell!", eChatType.CT_System);
					return false;
				}
				if (target is GamePlayer)
				{
					if ((target as GamePlayer).IsPvPInvulnerability)
					{
						MessageToCaster("Your target is invunerable at the momment and cannot be attacked!", eChatType.CT_System);
						return false;
					}
				}
				if ( !caster.IsWithinRadius( caster.TargetObject, ( (SpellHandler)spellhandler ).CalculateSpellRange() ) )
				{
					MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
					return false;
				}
				if (PhaseShift != null)
				{
					MessageToCaster(target.Name + " is Phaseshifted and can't be attacked!", eChatType.CT_System); return false;
				}
                if (SelectiveBlindness != null)
                {
                    GameLiving EffectOwner = SelectiveBlindness.EffectSource;
                    if (EffectOwner == target)
                    {
                        if (m_caster is GamePlayer)
                            ((GamePlayer)m_caster).Out.SendMessage(string.Format("{0} is invisible to you!", target.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

                        return false;
                    }
                }
				if (target.HasAbility(Abilities.DamageImmunity))
				{
					MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
					return false;
				}
                if (GameServer.ServerRules.IsAllowedToAttack(Caster, target, true) && chamber.PrimarySpell.Target.ToLower() == "realm")
                {
                    MessageToCaster("This spell only works on friendly targets!", eChatType.CT_System);
                    return false;
                }
                if (!GameServer.ServerRules.IsAllowedToAttack(Caster, target, true) && chamber.PrimarySpell.Target.ToLower() != "realm")
                {
                    MessageToCaster("That target isn't attackable at this time!", eChatType.CT_System);
                    return false;
                }
				spellhandler.CastSpell();
				#endregion

				if (chamber.SecondarySpell != null)
				{
					spellhandler2 = ScriptMgr.CreateSpellHandler(caster, chamber.SecondarySpell, chamber.SecondarySpellLine);
					spellhandler2.CastSpell();
				}
				effect.Cancel(false);

				foreach (Spell oSpell in SkillBase.GetSpellList(chamber.SpellLine.KeyName))
				{
                    caster.DisableSkill(oSpell, Spell.RecastDelay);
				}
			}
			else
			{
				base.CastSpell ();
                int duration = caster.GetSkillDisabledDuration(m_spell);
				if(Caster is GamePlayer && duration == 0)
					((GamePlayer)Caster).Out.SendMessage("Select the first spell for your " + Spell.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			return true;
		}

		/// <summary>
		/// Fire bolt
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			
			// endurance
			m_caster.Endurance -= 5;

			// messages
            GamePlayer caster = (GamePlayer)m_caster;
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
        		#region Devle Info
        public override IList<string> DelveInfo
        {
            get
            {
            	var list = new List<string>();

                //Name
                list.Add("Name: " + Spell.Name);
                list.Add("");

                //Description
                list.Add("Description: " + Spell.Description);
                list.Add("");

                //SpellType
                if (!Spell.AllowBolt)
                    list.Add("Type: Any but bolts");
                if (Spell.AllowBolt)
                    list.Add("Type: Any");

                //Cast
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
                //Recast
                if (Spell.RecastDelay > 60000)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.RecastTime") + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
                else if (Spell.RecastDelay > 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.RecastTime") + (Spell.RecastDelay / 1000).ToString() + " sec");
                return list;
            }
        }
			#endregion
		// constructor
		public ChamberSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
