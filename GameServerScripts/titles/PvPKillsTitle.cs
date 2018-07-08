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
    /// "Bane of Albion" title granted to everyone who killed 2000+ alb players.
    /// </summary>
    public class BaneOfAlbionTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsAlbionPlayersChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Albion.BaneOfAlbion", "Titles.Kills.Albion.BaneOfAlbion"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsAlbionPlayers >= 2000 && player.KillsAlbionPlayers < 25000; } }
    }

    /// <summary>
    /// "Scourge of Albion" title granted to everyone who killed 25000+ alb players.
    /// </summary>
    public class ScourgeOfAlbionTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsAlbionPlayersChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Albion.ScourgeOfAlbion", "Titles.Kills.Albion.ScourgeOfAlbion"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsAlbionPlayers >= 25000 && player.KillsAlbionPlayers < 100000; } }
    }

    /// <summary>
    /// "Slayer of Albion" title granted to everyone who killed 100000+ alb players.
    /// </summary>
    public class SlayerOfAlbionTitle : NoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsAlbionPlayersChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Slayer of Albion", "Slayer of Albion"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsAlbionPlayers >= 100000; } }
    }

    /// <summary>
    /// "Bane of Midgard" title granted to everyone who killed 2000+ mid players.
    /// </summary>
    public class BaneOfMidgardTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsMidgardPlayersChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Midgard.BaneOfMidgard", "Titles.Kills.Midgard.BaneOfMidgard"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsMidgardPlayers >= 2000 && player.KillsMidgardPlayers < 25000; } }
    }

    /// <summary>
    /// "Scourge of Midgard" title granted to everyone who killed 25000+ mid players.
    /// </summary>
    public class ScourgeOfMidgardTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsMidgardPlayersChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Midgard.ScourgeOfMidgard", "Titles.Kills.Midgard.ScourgeOfMidgard"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsMidgardPlayers >= 25000 && player.KillsMidgardPlayers < 100000; } }
    }

    /// <summary>
    /// "Slayer of Midgard" title granted to everyone who killed 100000+ mid players.
    /// </summary>
    public class SlayerOfMidgardTitle : NoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsMidgardPlayersChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Slayer of Midgard", "Slayer of Midgard"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsMidgardPlayers >= 100000; } }
    }

    /// <summary>
    /// "Bane of Hibernia" title granted to everyone who killed 2000+ hib players.
    /// </summary>
    public class BaneOfHiberniaTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsHiberniaPlayersChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Hibernia.BaneOfHibernia", "Titles.Kills.Hibernia.BaneOfHibernia"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsHiberniaPlayers >= 2000 && player.KillsHiberniaPlayers < 25000; } }
    }

    /// <summary>
    /// "Scourge of Hibernia" title granted to everyone who killed 25000+ hib players.
    /// </summary>
    public class ScourgeOfHiberniaTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsHiberniaPlayersChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.Hibernia.ScourgeOfHibernia", "Titles.Kills.Hibernia.ScourgeOfHibernia"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsHiberniaPlayers >= 25000 && player.KillsHiberniaPlayers < 100000; } }
    }

    /// <summary>
    /// "Slayer of Hibernia" title granted to everyone who killed 100000+ hib players.
    /// </summary>
    public class SlayerOfHiberniaTitle : NoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsHiberniaPlayersChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Slayer of Hibernia", "Slayer of Hibernia"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => player.KillsHiberniaPlayers >= 100000; } }
    }

    /// <summary>
    /// "Master Soldier" title granted to everyone who killed 2000+ players.
    /// </summary>
    public class MasterSoldierTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsTotalPlayersChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.All.MasterSoldier", "Titles.Kills.All.MasterSoldier"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => (player.KillsHiberniaPlayers + player.KillsMidgardPlayers + player.KillsAlbionPlayers) >= 2000 && (player.KillsHiberniaPlayers + player.KillsMidgardPlayers + player.KillsAlbionPlayers) < 25000; } }
    }

    /// <summary>
    /// "Master Enforcer" title granted to everyone who killed 25000+ players.
    /// </summary>
    public class MasterEnforcerTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsTotalPlayersChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.All.MasterEnforcer", "Titles.Kills.All.MasterEnforcer"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => (player.KillsHiberniaPlayers + player.KillsMidgardPlayers + player.KillsAlbionPlayers) >= 25000 && (player.KillsHiberniaPlayers + player.KillsMidgardPlayers + player.KillsAlbionPlayers) < 100000; } }
    }

    /// <summary>
    /// "Master Assassine" title granted to everyone who killed 100000+ players.
    /// </summary>
    public class MasterAssassineTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsTotalPlayersChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Kills.All.MasterAssassine", "Titles.Kills.All.MasterAssassine"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => (player.KillsHiberniaPlayers + player.KillsMidgardPlayers + player.KillsAlbionPlayers) >= 100000; } }
    }

    /// <summary>
    /// "Battle Enforcer" title granted to everyone who killed 2000+ players with death blow.
    /// </summary>
    public class BattleEnforcerTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsTotalDeathBlowsChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Deathblows.BattleEnforcer", "Titles.Deathblows.BattleEnforcer"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => (player.KillsAlbionDeathBlows + player.KillsMidgardDeathBlows + player.KillsHiberniaDeathBlows) >= 2000 && (player.KillsAlbionDeathBlows + player.KillsMidgardDeathBlows + player.KillsHiberniaDeathBlows) < 25000; } }
    }

    /// <summary>
    /// "Battle Master" title granted to everyone who killed 25000+ players with death blow.
    /// </summary>
    public class BattleMasterTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsTotalDeathBlowsChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Deathblows.BattleMaster", "Titles.Deathblows.BattleMaster"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => (player.KillsAlbionDeathBlows + player.KillsMidgardDeathBlows + player.KillsHiberniaDeathBlows) >= 25000; } }
    }

    /// <summary>
    /// "Lone Enforcer" title granted to everyone who solo killed 2000+ players.
    /// </summary>
    public class LoneEnforcerTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsTotalSoloChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Solokills.LoneEnforcer", "Titles.Solokills.LoneEnforcer"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => (player.KillsAlbionSolo + player.KillsMidgardSolo + player.KillsHiberniaSolo) >= 2000 && (player.KillsAlbionSolo + player.KillsMidgardSolo + player.KillsHiberniaSolo) < 25000; } }
    }

    /// <summary>
    /// "Duel Master" title granted to everyone who solo killed 25000+ players.
    /// </summary>
    public class DuelMasterTitle : TranslatedNoGenderGenericEventPlayerTitle
    {
        public override DOLEvent Event { get { return GamePlayerEvent.KillsTotalSoloChanged; } }

        protected override Tuple<string, string> DescriptionValue { get { return new Tuple<string, string>("Titles.Solokills.DuelMaster", "Titles.Solokills.DuelMaster"); } }

        protected override Func<DOL.GS.GamePlayer, bool> SuitableMethod { get { return player => (player.KillsAlbionSolo + player.KillsMidgardSolo + player.KillsHiberniaSolo) >= 25000; } }
    }
}
