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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.Language;

namespace DOL.GS.Spells
{
    /// <summary>
    ///
    /// </summary>
    [SpellHandler("Chamber")]
    public class ChamberSpellHandler : SpellHandler
    {
        public Spell PrimarySpell { get; set; }

        public SpellLine PrimarySpellLine { get; set; }

        public Spell SecondarySpell { get; set; }

        public SpellLine SecondarySpellLine { get; set; }

        public int EffectSlot { get; set; }

        public override void InterruptCasting()
        {
            base.InterruptCasting();
            Caster.CurrentSpellHandler = null;
        }

        public override bool CastSpell()
        {
            GamePlayer caster = (GamePlayer)Caster;
            m_spellTarget = caster.TargetObject as GameLiving;
            GameSpellEffect effect = FindEffectOnTarget(caster, "Chamber", Spell.Name);
            if (effect != null && Spell.Name == effect.Spell.Name)
            {
                ChamberSpellHandler chamber = (ChamberSpellHandler)effect.SpellHandler;
                GameSpellEffect PhaseShift = FindEffectOnTarget(m_spellTarget, "Phaseshift");
                SelectiveBlindnessEffect SelectiveBlindness = Caster.EffectList.GetOfType<SelectiveBlindnessEffect>();
                var spellhandler = ScriptMgr.CreateSpellHandler(caster, chamber.PrimarySpell, chamber.PrimarySpellLine);

                int duration = caster.GetSkillDisabledDuration(Spell);
                if (duration > 0)
                {
                    MessageToCaster($"You must wait {duration / 1000 + 1} seconds to use this spell!", eChatType.CT_System);
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

                if (m_spellTarget == null)
                {
                    MessageToCaster("You must have a target!", eChatType.CT_SpellResisted);
                    return false;
                }

                if (!caster.IsAlive)
                {
                    MessageToCaster("You cannot cast this dead!", eChatType.CT_SpellResisted);
                    return false;
                }

                if (!m_spellTarget.IsAlive)
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
                    MessageToCaster("Your target is not visible!", eChatType.CT_SpellResisted);
                    return false;
                }

                if (caster.IsObjectInFront(m_spellTarget, 180) == false)
                {
                    MessageToCaster("Your target is not in view!", eChatType.CT_SpellResisted);
                    return false;
                }

                if (caster.IsInvulnerableToAttack)
                {
                    MessageToCaster("Your invunerable at the momment and cannot use that spell!", eChatType.CT_System);
                    return false;
                }

                if (m_spellTarget is GamePlayer)
                {
                    if ((m_spellTarget as GamePlayer).IsInvulnerableToAttack)
                    {
                        MessageToCaster("Your target is invunerable at the momment and cannot be attacked!", eChatType.CT_System);
                        return false;
                    }
                }

                if (!caster.IsWithinRadius(m_spellTarget, ((SpellHandler)spellhandler).CalculateSpellRange()))
                {
                    MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                    return false;
                }

                if (PhaseShift != null)
                {
                    MessageToCaster(m_spellTarget.Name + " is Phaseshifted and can't be attacked!", eChatType.CT_System); return false;
                }

                if (SelectiveBlindness != null)
                {
                    GameLiving EffectOwner = SelectiveBlindness.EffectSource;
                    if (EffectOwner == m_spellTarget)
                    {
                        ((GamePlayer)Caster).Out.SendMessage($"{m_spellTarget.GetName(0, true)} is invisible to you!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

                        return false;
                    }
                }

                if (m_spellTarget.HasAbility(Abilities.DamageImmunity))
                {
                    MessageToCaster($"{m_spellTarget.Name} is immune to this effect!", eChatType.CT_SpellResisted);
                    return false;
                }

                if (GameServer.ServerRules.IsAllowedToAttack(Caster, m_spellTarget, true) && chamber.PrimarySpell.Target.ToLower() == "realm")
                {
                    MessageToCaster("This spell only works on friendly targets!", eChatType.CT_System);
                    return false;
                }

                if (!GameServer.ServerRules.IsAllowedToAttack(Caster, m_spellTarget, true) && chamber.PrimarySpell.Target.ToLower() != "realm")
                {
                    MessageToCaster("That target isn't attackable at this time!", eChatType.CT_System);
                    return false;
                }

                spellhandler.CastSpell();

                if (chamber.SecondarySpell != null)
                {
                    var spellhandler2 = ScriptMgr.CreateSpellHandler(caster, chamber.SecondarySpell, chamber.SecondarySpellLine);
                    spellhandler2.CastSpell();
                }

                effect.Cancel(false);
                
                GamePlayer player_Caster = Caster as GamePlayer;
                foreach (SpellLine spellline in player_Caster.GetSpellLines())
                {
                    foreach (Spell sp in SkillBase.GetSpellList(spellline.KeyName))
                    {
                        if (sp.SpellType == Spell.SpellType)
                        {
                            Caster.DisableSkill(sp, sp.RecastDelay);
                        }
                    }
                }
            }
            else
            {
                base.CastSpell();
                int duration = caster.GetSkillDisabledDuration(Spell);
                if (Caster is GamePlayer player && duration == 0)
                {
                    player.Out.SendMessage($"Select the first spell for your {Spell.Name}.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }

            return true;
        }

        /// <summary>
        /// Fire bolt
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);

            // endurance
            Caster.Endurance -= 5;

            // messages
            if (Spell.InstrumentRequirement == 0)
            {
                if (SecondarySpell == null && PrimarySpell == null)
                {
                    MessageToCaster($"No spells were loaded into {Spell.Name}.", eChatType.CT_Spell);
                }
                else
                {
                    MessageToCaster($"Your {Spell.Name} is ready for use.", eChatType.CT_Spell);

                    // StartSpell(target); // and action
                    GameSpellEffect neweffect = CreateSpellEffect(target, 1);
                    neweffect.Start(Caster);
                    SendEffectAnimation(Caster, 0, false, 1);
                    ((GamePlayer)Caster).Out.SendWarlockChamberEffect((GamePlayer)Caster);
                }

                foreach (GamePlayer player in Caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
                {
                    if (player != Caster)
                    {
                        player.MessageFromArea(Caster, $"{Caster.GetName(0, true)} casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    }
                }
            }

            // the quick cast is unallowed whenever you miss the spell
            // set the time when casting to can not quickcast during a minimum time
            if (Caster is GamePlayer)
            {
                QuickCastEffect quickcast = Caster.EffectList.GetOfType<QuickCastEffect>();
                if (quickcast != null && Spell.CastTime > 0)
                {
                    Caster.TempProperties.setProperty(GamePlayer.QUICK_CAST_CHANGE_TICK, Caster.CurrentRegion.Time);
                    Caster.DisableSkill(SkillBase.GetAbility(Abilities.Quickcast), QuickCastAbilityHandler.DISABLE_DURATION);
                    quickcast.Cancel(false);
                }
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {

            ((GamePlayer)Caster).Out.SendWarlockChamberEffect((GamePlayer)effect.Owner);
            return base.OnEffectExpires(effect, noMessages);
        }

        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
            return new GameSpellEffect(this, 0, 0, effectiveness);
        }

        public static int GetEffectSlot(string spellName)
        {
            switch (spellName)
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

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    $"Name: {Spell.Name}",
                    string.Empty,
                    $"Description: {Spell.Description}",
                    string.Empty
                };

                if (!Spell.AllowBolt)
                {
                    list.Add("Type: Any but bolts");
                }

                if (Spell.AllowBolt)
                {
                    list.Add("Type: Any");
                }

                // Cast
                list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));

                // Recast
                if (Spell.RecastDelay > 60000)
                {
                    list.Add($"{LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.RecastTime")}{Spell.RecastDelay / 60000}:{Spell.RecastDelay % 60000 / 1000:00} min");
                }
                else if (Spell.RecastDelay > 0)
                {
                    list.Add($"{LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.RecastTime")}{Spell.RecastDelay / 1000} sec");
                }

                return list;
            }
        }

        // constructor
        public ChamberSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
