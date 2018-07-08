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

using DOL.Events;

namespace GameServerScripts.Titles
{
    /// <summary>
    /// "Demon Slayer" title granted to everyone who kills Legion 10+ times.
    /// </summary>
    public class DemonSlayerTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsLegionChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Demon.DemonSlayer", "Titles.Kills.Demon.DemonSlayer"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsLegion >= 10 && player.KillsLegion < 25; } }
    }

    /// <summary>
    /// "Demon Bane" title granted to everyone who kills Legion 25+ times.
    /// </summary>
    public class DemonBaneTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsLegionChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Demon.DemonBane", "Titles.Kills.Demon.DemonBane"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsLegion >= 25 && player.KillsLegion < 250; } }
    }

    /// <summary>
    /// "Demon Scourge" title granted to everyone who kills Legion 250+ times.
    /// </summary>
    public class DemonScourgeTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsLegionChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Demon.DemonScourge", "Titles.Kills.Demon.DemonScourge"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsLegion >= 250 && player.KillsLegion < 1000; } }
    }

    /// <summary>
    /// "Dread Vanquisher of Legion" title granted to everyone who kills Legion 1000+ times.
    /// </summary>
    public class DreadVanquisherTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsLegionChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Demon.DreadVanquisher", "Titles.Kills.Demon.DreadVanquisher"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsLegion >= 1000; } }
    }

    /// <summary>
    /// "Dragon Foe" title granted to everyone who kills the dragon 10+ times.
    /// </summary>
    public class DragonFoeTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsDragonChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Dragon.DragonFoe", "Titles.Kills.Dragon.DragonFoe"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsDragon >= 10 && player.KillsDragon < 50; } }
    }

    /// <summary>
    /// "Dragon Scourge" title granted to everyone who kills the dragon 50+ times.
    /// </summary>
    public class DragonScourgeTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsDragonChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Dragon.DragonScourge", "Titles.Kills.Dragon.DragonScourge"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsDragon >= 50 && player.KillsDragon < 100; } }
    }

    /// <summary>
    /// "Dragon Slayer" title granted to everyone who kills the dragon 100+ times.
    /// </summary>
    public class DragonSlayerTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsDragonChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Dragon.DragonSlayer", "Titles.Kills.Dragon.DragonSlayer"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsDragon >= 100 && player.KillsDragon < 500; } }
    }

    /// <summary>
    /// "Dread Vanquisher of Legion" title granted to everyone who the dragon Legion 500+ times.
    /// </summary>
    public class DragonBaneTitle : NoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsDragonChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Dragon Bane", "Dragon Bane"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsDragon >= 500; } }
    }

    /// <summary>
    /// "Epic Challenger" title granted to everyone who kills the Epic Dungeon Boss 10+ times.
    /// </summary>
    public class EpicChallengerTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsEpicBossChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Epic.EpicChallenger", "Titles.Kills.Epic.EpicChallenger"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsEpicBoss >= 10 && player.KillsEpicBoss < 50; } }
    }

    /// <summary>
    /// "Epic Victor" title granted to everyone who kills the Epic Dungeon Boss 50+ times.
    /// </summary>
    public class EpicVictorTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsEpicBossChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Epic.EpicVictor", "Titles.Kills.Epic.EpicVictor"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsEpicBoss >= 50; } }
    }
}
