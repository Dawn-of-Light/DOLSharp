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
using DOL.Database;
using DOL.Events;

namespace DOL.GS
{
    public class StealtherAbilities
    {
        [ScriptLoadedEvent]
        public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
        {
            GameEventMgr.AddHandler(GamePlayerEvent.KillsTotalDeathBlowsChanged, new DOLEventHandler(AssassinsAbilities));
        }

        private static void AssassinsAbilities(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GamePlayer player))
            {
                return;
            }

            // Shadowblade-Blood Rage
            if (player.HasAbility(Abilities.BloodRage))
            {
                player.CastSpell(Br, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
            }

            // Infiltrator-Heightened Awareness
            if (player.HasAbility(Abilities.HeightenedAwareness))
            {
                player.CastSpell(Ha, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
            }

            // Nightshade-Subtle Kills
            if (player.HasAbility(Abilities.SubtleKills))
            {
                player.CastSpell(Sk, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
            }
        }

        private static Spell _bloodRage;

        private static Spell Br
        {
            get
            {
                if (_bloodRage == null)
                {
                    DBSpell spell = new DBSpell
                    {
                        AllowAdd = true,
                        CastTime = 0,
                        Uninterruptible = true,
                        Icon = 10541,
                        ClientEffect = 10541,
                        Description = "Movement speed of the player in stealth is increased by 25% for 1 minute after they get a killing blow on a realm enemy.",
                        Name = "Blood Rage",
                        Range = 0,
                        Value = 25,
                        Duration = 60,
                        SpellID = 900090,
                        Target = "Self",
                        Type = "BloodRage"
                    };

                    _bloodRage = new Spell(spell, 50);
                    SkillBase.AddScriptedSpell(GlobalSpellsLines.Reserved_Spells, _bloodRage);
                }

                return _bloodRage;
            }
        }

        private static Spell _heightenedAwareness;

        private static Spell Ha
        {
            get
            {
                if (_heightenedAwareness == null)
                {
                    DBSpell spell = new DBSpell
                    {
                        AllowAdd = true,
                        CastTime = 0,
                        Uninterruptible = true,
                        Icon = 10541,
                        ClientEffect = 10541,
                        Description = "Greater Chance to Detect Stealthed Enemies for 1 minute after executing a klling blow on a realm opponent.",
                        Name = "Heightened Awareness",
                        Range = 0,
                        Value = 25,
                        Duration = 60,
                        SpellID = 900091,
                        Target = "Self",
                        Type = "HeightenedAwareness"
                    };

                    _heightenedAwareness = new Spell(spell, 50);
                    SkillBase.AddScriptedSpell(GlobalSpellsLines.Reserved_Spells, _heightenedAwareness);
                }

                return _heightenedAwareness;
            }
        }

        private static Spell _subtleKills;

        private static Spell Sk
        {
            get
            {
                if (_subtleKills == null)
                {
                    DBSpell spell = new DBSpell
                    {
                        AllowAdd = true,
                        CastTime = 0,
                        Uninterruptible = true,
                        Icon = 10541,
                        ClientEffect = 10541,
                        Description = "Greater chance of remaining hidden while stealthed for 1 minute after executing a killing blow on a realm opponent.",
                        Name = "Subtle Kills",
                        Range = 0,
                        Value = 25,
                        Duration = 60,
                        SpellID = 900092,
                        Target = "Self",
                        Type = "SubtleKills"
                    };
                    _subtleKills = new Spell(spell, 50);
                    SkillBase.AddScriptedSpell(GlobalSpellsLines.Reserved_Spells, _subtleKills);
                }

                return _subtleKills;
            }
        }
    }
}
