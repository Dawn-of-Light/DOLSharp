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
using DOL.GS.Spells;
using System.Collections.Generic;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Minion Rescue RA
    /// </summary>
    public class BlissfulIgnoranceAbility : RR5RealmAbility
    {
        public const int DURATION = 30 * 1000;

        public BlissfulIgnoranceAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

            GamePlayer player = living as GamePlayer;
            if (player != null)
            {
                /*BlissfulIgnoranceEffect BlissfulIgnorance = (BlissfulIgnoranceEffect)player.EffectList.GetOfType(typeof(BlissfulIgnoranceEffect));
                if (BlissfulIgnorance != null)
                    BlissfulIgnorance.Cancel(false);

                new BlissfulIgnoranceEffect().Start(player);*/

                Hashtable table_spells = new Hashtable();
				foreach (Spell spell in SkillBase.GetSpellList("Savagery"))
				{
                    if(spell.Group==0 || spell.Target.ToLower()!="self") continue;

                    if (spell.Level <= player.GetSpellLine("Savagery").Level)
					{
						if (!table_spells.ContainsKey(spell.Group))
                            table_spells.Add(spell.Group, spell);
						else
						{
                            Spell oldspell = (Spell)table_spells[spell.Group];
							if (spell.Level > oldspell.Level)
                                table_spells[spell.Group] = spell;
						}
					}
				}
				foreach (object obj in table_spells.Values)
				{
                    if (obj == null || !(obj is Spell)) continue;
                    Spell spell = obj as Spell;
                    try
					{
						DBSpell db = new DBSpell();
                        db.ClientEffect = spell.ClientEffect;
						db.Icon = spell.Icon;
						db.Name = spell.Name;
						db.Description = spell.Description;
						db.Duration = spell.Duration / 1000;
                        db.Power = 0;
						db.Value = spell.Value;
	                    db.Message1 = "";
	                    db.Message2 = "";
	                    db.Message3 = "";
	                    db.Message4 = "";
                        db.Type = spell.SpellType;
                        db.Target = "Self";
                        db.MoveCast = true;
                        db.Uninterruptible = true;
						
						SpellHandler handler = new SpellHandler(player, new Spell(db, 0), SkillBase.GetSpellLine("Savagery"));
                        if(handler!=null)
                            handler.CastSpell();
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("RR5 Savage : use spell, ", e);
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
            list.Add("");
            list.Add("Target: Self");
            list.Add("Duration: 30s");
            list.Add("Casting time: Instant");
        }

    }
}
