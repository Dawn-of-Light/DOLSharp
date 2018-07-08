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
using DOL.GS.Spells;
using System.Collections.Generic;
using log4net;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Minion Rescue RA
    /// </summary>
    public class BlissfulIgnoranceAbility : RR5RealmAbility
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public BlissfulIgnoranceAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            if (living is GamePlayer player)
            {
                Hashtable tableSpells = new Hashtable();
                foreach (Spell spell in SkillBase.GetSpellList("Savagery"))
                {
                    if (spell.Group == 0 || spell.Target.ToLower() != "self")
                    {
                        continue;
                    }

                    if (spell.Level <= player.GetSpellLine("Savagery").Level)
                    {
                        if (!tableSpells.ContainsKey(spell.Group))
                        {
                            tableSpells.Add(spell.Group, spell);
                        }
                        else
                        {
                            Spell oldspell = (Spell)tableSpells[spell.Group];
                            if (spell.Level > oldspell.Level)
                            {
                                tableSpells[spell.Group] = spell;
                            }
                        }
                    }
                }

                foreach (object obj in tableSpells.Values)
                {
                    if (!(obj is Spell spell))
                    {
                        continue;
                    }

                    try
                    {
                        DBSpell db = new DBSpell
                        {
                            ClientEffect = spell.ClientEffect,
                            Icon = spell.Icon,
                            Name = spell.Name,
                            Description = spell.Description,
                            Duration = spell.Duration / 1000,
                            Power = 0,
                            Value = spell.Value,
                            Message1 = string.Empty,
                            Message2 = string.Empty,
                            Message3 = string.Empty,
                            Message4 = string.Empty,
                            Type = spell.SpellType,
                            Target = "Self",
                            MoveCast = true,
                            Uninterruptible = true
                        };

                        SpellHandler handler = new SpellHandler(player, new Spell(db, 0), SkillBase.GetSpellLine("Savagery"));
                        handler.CastSpell();
                    }
                    catch (Exception e)
                    {
                        if (Log.IsErrorEnabled)
                        {
                            Log.Error("RR5 Savage : use spell, ", e);
                        }
                    }
                }
            }

            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 300;
        }

        public override void AddEffectsInfo(IList<string> list)
        {
            list.Add("No penality Hit from self buffs. 30s duration, 5min RUT.");
            list.Add(string.Empty);
            list.Add("Target: Self");
            list.Add("Duration: 30s");
            list.Add("Casting time: Instant");
        }
    }
}
