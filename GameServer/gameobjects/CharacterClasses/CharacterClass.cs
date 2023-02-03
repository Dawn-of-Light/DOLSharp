using System;
using System.Collections.Generic;
using System.Linq;
using DOL.Database;
using DOL.GS.Realm;
using DOL.Language;

namespace DOL.GS
{
    public class CharacterClass
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public int ID { get; protected set; }

        public string Name { get; protected set; }
        public string BaseName { get; protected set; }
        public string FemaleName { get; protected set; }

        public string ProfessionTranslationId { get; protected set; } = "";
        public int SpecPointsMultiplier { get; protected set; }
        public IEnumerable<string> AutoTrainSkills { get; protected set; }
        public (eStat Primary, eStat Secondary, eStat Tertiary) LeveledStats { get; protected set; }
        public eStat ManaStat { get; protected set; }
        public IEnumerable<PlayerRace> EligibleRaces { get; protected set; }
        public bool CanUseLeftHandedWeapon { get; protected set; }
        public bool HasAdvancedFromBaseClass { get; protected set; }
        public int BaseHP { get; protected set; }
        public int BaseWeaponSkill { get; protected set; }
        public eClassType ClassType { get; protected set; }
        public int ChampionTrainerID { get; protected set; }

        public ushort MaxPulsingSpells { get; protected set; } = 2;

        public string GetSalutation(GamePlayer player)
        {
                var femaleName = FemaleName;
                var useFemaleName = (player != null && player.Gender == eGender.Female && !Util.IsEmpty(femaleName));
                if (useFemaleName) return femaleName;
                else return Name;
        }

        public string GetTitle(GamePlayer player, int level)
        {
            if (!HasAdvancedFromBaseClass) level = 0;

            // Clamp level in 5 by 5 steps - 50 is the max available translation for now
            int clamplevel = Math.Min(50, (level / 5) * 5);

            string none = LanguageMgr.TryTranslateOrDefault(player, "!None!", "PlayerClass.GetTitle.none");

            if (clamplevel > 0)
                return LanguageMgr.TryTranslateOrDefault(player, string.Format("!{0}!", Name), string.Format("PlayerClass.{0}.GetTitle.{1}", Name, clamplevel));

            return none;
        }

        public static CharacterClass GetClass(int classID)
        {
            allClasses.TryGetValue(classID, out var characterClass);
            if (characterClass == null) return Unknown;
            return characterClass;
        }

        public CharacterClass GetBaseClass()
        {
            if(Equals(CharacterClass.Unknown)) return CharacterClass.Unknown;

            return allClasses.Values.Where(c => c.Name == BaseName).First();
        }

        public static void LoadCustomizationsFromDatabase()
        {
            var customClasses = DOLDB<DBCharacterClass>.SelectAllObjects();

            foreach (var customClass in customClasses)
            {
                var characterClass = GetClass(customClass.ID);
                var newSpecMulti = customClass.SpecPointMultiplier;
                var newBaseHP = customClass.BaseHP;
                var newBaseWS = customClass.BaseWeaponSkill;
                var newAutoTrainSkills = customClass.AutoTrainSkills
                    .Split(';', ',').Where(s => !string.IsNullOrEmpty(s));

                if (newSpecMulti > 0) characterClass.SpecPointsMultiplier = newSpecMulti;
                if (newBaseHP > 0) characterClass.BaseHP = newBaseHP;
                if (newBaseWS > 0) characterClass.BaseWeaponSkill = newBaseWS;
                if (newAutoTrainSkills.Any()) characterClass.AutoTrainSkills = newAutoTrainSkills;
                try
                {
                    var newElibibleRaces = customClass.EligibleRaces
                        .Split(';', ',').Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => Convert.ToInt32(s))
                        .Select(i => PlayerRace.GetRace(i));
                    if(newElibibleRaces.Any()) characterClass.EligibleRaces = newElibibleRaces;

                }
                catch (Exception e)
                {
                    log.Error($"Failed to load EligibleRaces for class with id {customClass.ID} with error:\n{e}");
                }
            }
        }

        public static CharacterClass Unknown
            => new CharacterClass()
            {
                ID = 0,
                Name = "Unknown Class",
                BaseName = "Unknown Base Class",
                FemaleName = "",
                ProfessionTranslationId = "",
                SpecPointsMultiplier = 10,
                AutoTrainSkills = new string[] { },
                LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                ManaStat = eStat.UNDEFINED,
                EligibleRaces = PlayerRace.AllRaces.ToArray(),
                CanUseLeftHandedWeapon = false,
                HasAdvancedFromBaseClass = true,
                BaseHP = 600,
                BaseWeaponSkill = 400,
                ClassType = eClassType.ListCaster,
                ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.None,
            };

        public override bool Equals(object obj)
        {
            if(obj is CharacterClass characterClass)
            {
                return characterClass.ID == ID;
            }
            return false;
        }

        public override int GetHashCode()
            => ID;

        #region CharacterClass(Base) creation shortcuts
        public static ICharacterClass None => CharacterClassBase.GetClass((int)eCharacterClass.Unknown);
        //alb
        public static ICharacterClass Armsman => CharacterClassBase.GetClass((int)eCharacterClass.Armsman);
		public static ICharacterClass Cabalist => CharacterClassBase.GetClass((int)eCharacterClass.Cabalist);
		public static ICharacterClass Cleric => CharacterClassBase.GetClass((int)eCharacterClass.Cleric);
		public static ICharacterClass Friar => CharacterClassBase.GetClass((int)eCharacterClass.Friar);
		public static ICharacterClass Heretic => CharacterClassBase.GetClass((int)eCharacterClass.Heretic);
		public static ICharacterClass Infiltrator => CharacterClassBase.GetClass((int)eCharacterClass.Infiltrator);
		public static ICharacterClass Mercenary => CharacterClassBase.GetClass((int)eCharacterClass.Mercenary);
		public static ICharacterClass Minstrel => CharacterClassBase.GetClass((int)eCharacterClass.Minstrel);
		public static ICharacterClass Necromancer => CharacterClassBase.GetClass((int)eCharacterClass.Necromancer);
		public static ICharacterClass Paladin => CharacterClassBase.GetClass((int)eCharacterClass.Paladin);
		public static ICharacterClass Reaver => CharacterClassBase.GetClass((int)eCharacterClass.Reaver);
		public static ICharacterClass Scout => CharacterClassBase.GetClass((int)eCharacterClass.Scout);
		public static ICharacterClass Sorcerer => CharacterClassBase.GetClass((int)eCharacterClass.Sorcerer);
		public static ICharacterClass Theurgist => CharacterClassBase.GetClass((int)eCharacterClass.Theurgist);
		public static ICharacterClass Wizard => CharacterClassBase.GetClass((int)eCharacterClass.Wizard);
		public static ICharacterClass MaulerAlb => CharacterClassBase.GetClass((int)eCharacterClass.MaulerAlb);
		//mid
		public static ICharacterClass Berserker => CharacterClassBase.GetClass((int)eCharacterClass.Berserker);
		public static ICharacterClass Bonedancer => CharacterClassBase.GetClass((int)eCharacterClass.Bonedancer);
		public static ICharacterClass Healer => CharacterClassBase.GetClass((int)eCharacterClass.Healer);
		public static ICharacterClass Hunter => CharacterClassBase.GetClass((int)eCharacterClass.Hunter);
		public static ICharacterClass Runemaster => CharacterClassBase.GetClass((int)eCharacterClass.Runemaster);
		public static ICharacterClass Savage => CharacterClassBase.GetClass((int)eCharacterClass.Savage);
		public static ICharacterClass Shadowblade => CharacterClassBase.GetClass((int)eCharacterClass.Shadowblade);
		public static ICharacterClass Shaman => CharacterClassBase.GetClass((int)eCharacterClass.Shaman);
		public static ICharacterClass Skald => CharacterClassBase.GetClass((int)eCharacterClass.Skald);
		public static ICharacterClass Spiritmaster => CharacterClassBase.GetClass((int)eCharacterClass.Spiritmaster);
		public static ICharacterClass Thane => CharacterClassBase.GetClass((int)eCharacterClass.Thane);
		public static ICharacterClass Valkyrie => CharacterClassBase.GetClass((int)eCharacterClass.Valkyrie);
		public static ICharacterClass Warlock => CharacterClassBase.GetClass((int)eCharacterClass.Warlock);
		public static ICharacterClass Warrior => CharacterClassBase.GetClass((int)eCharacterClass.Warrior);
		public static ICharacterClass MaulerMid => CharacterClassBase.GetClass((int)eCharacterClass.MaulerMid);
		//hib
		public static ICharacterClass Animist => CharacterClassBase.GetClass((int)eCharacterClass.Animist);
		public static ICharacterClass Bainshee => CharacterClassBase.GetClass((int)eCharacterClass.Bainshee);
		public static ICharacterClass Bard => CharacterClassBase.GetClass((int)eCharacterClass.Bard);
		public static ICharacterClass Blademaster => CharacterClassBase.GetClass((int)eCharacterClass.Blademaster);
		public static ICharacterClass Champion => CharacterClassBase.GetClass((int)eCharacterClass.Champion);
		public static ICharacterClass Druid => CharacterClassBase.GetClass((int)eCharacterClass.Druid);
		public static ICharacterClass Eldritch => CharacterClassBase.GetClass((int)eCharacterClass.Eldritch);
		public static ICharacterClass Enchanter => CharacterClassBase.GetClass((int)eCharacterClass.Enchanter);
		public static ICharacterClass Hero => CharacterClassBase.GetClass((int)eCharacterClass.Hero);
		public static ICharacterClass Mentalist => CharacterClassBase.GetClass((int)eCharacterClass.Mentalist);
		public static ICharacterClass Nightshade => CharacterClassBase.GetClass((int)eCharacterClass.Nightshade);
		public static ICharacterClass Ranger => CharacterClassBase.GetClass((int)eCharacterClass.Ranger);
		public static ICharacterClass Valewalker => CharacterClassBase.GetClass((int)eCharacterClass.Valewalker);
		public static ICharacterClass Vampiir => CharacterClassBase.GetClass((int)eCharacterClass.Vampiir);
		public static ICharacterClass Warden => CharacterClassBase.GetClass((int)eCharacterClass.Warden);
		public static ICharacterClass MaulerHib => CharacterClassBase.GetClass((int)eCharacterClass.MaulerHib);
        #endregion

        #region Default Database
        private static Dictionary<int, CharacterClass> allClasses = new Dictionary<int, CharacterClass>()
        {
            {
                (int)eCharacterClass.Paladin,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Paladin,
                    Name = "Paladin",
                    BaseName = "Fighter",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.ChurchofAlbion",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Slash, Specs.Chants },
                    LeveledStats = (eStat.CON, eStat.PIE, eStat.STR),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.Highlander,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 760,
                    BaseWeaponSkill = 380,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Armsman,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Armsman,
                    Name = "Armsman",
                    BaseName = "Fighter",
                    FemaleName = "Armswoman",
                    ProfessionTranslationId = "PlayerClass.Profession.DefendersofAlbion",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Slash, Specs.Thrust },
                    LeveledStats = (eStat.STR, eStat.CON, eStat.DEX),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Korazh,
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Scout,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Scout,
                    Name = "Scout",
                    BaseName = "Rogue",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.DefendersofAlbion",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Archery, Specs.Longbow },
                    LeveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.DEX,
                    EligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Stalker,
                }
            },
            {
                (int)eCharacterClass.Minstrel,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Minstrel,
                    Name = "Minstrel",
                    BaseName = "Rogue",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.Academy",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new[] { Specs.Instruments },
                    LeveledStats = (eStat.CHR, eStat.DEX, eStat.STR),
                    ManaStat = eStat.CHR,
                    EligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Highlander,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 380,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Stalker,
                }
            },
            {
                (int)eCharacterClass.Theurgist,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Theurgist,
                    Name = "Theurgist",
                    BaseName = "Elementalist",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.DefendersofAlbion",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Elementalist,
                }
            },
            {
                (int)eCharacterClass.Cleric,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Cleric,
                    Name = "Cleric",
                    BaseName = "Acolyte",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.ChurchofAlbion",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.PIE, eStat.CON, eStat.STR),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.Highlander },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 320,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Acolyte,
                }
            },
            {
                (int)eCharacterClass.Wizard,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Wizard,
                    Name = "Wizard",
                    BaseName = "Elementalist",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.Academy",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 240,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Elementalist,
                }
            },
            {
                (int)eCharacterClass.Sorcerer,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Sorcerer,
                    Name = "Sorcerer",
                    BaseName = "Mage",
                    FemaleName = "Sorceress",
                    ProfessionTranslationId = "PlayerClass.Profession.Academy",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Mage,
                }
            },
            {
                (int)eCharacterClass.Infiltrator,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Infiltrator,
                    Name = "Infiltrator",
                    BaseName = "Rogue",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.GuildofShadows",
                    SpecPointsMultiplier = 25,
                    AutoTrainSkills = new[] { Specs.Stealth },
                    LeveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = true,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Stalker,
                }
            },
            {
                (int)eCharacterClass.Friar,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Friar,
                    Name = "Friar",
                    BaseName = "Acolyte",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.DefendersofAlbion",
                    SpecPointsMultiplier = 18,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.PIE, eStat.CON, eStat.STR),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.Highlander },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Acolyte,
                }
            },
            {
                (int)eCharacterClass.Mercenary,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Mercenary,
                    Name = "Mercenary",
                    BaseName = "Fighter",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.GuildofShadows",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Slash, Specs.Thrust },
                    LeveledStats = (eStat.STR, eStat.DEX, eStat.CON),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Korazh,
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = true,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Necromancer,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Necromancer,
                    Name = "Necromancer",
                    BaseName = "Disciple",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.TempleofArawn",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Disciple,
                }
            },
            {
                (int)eCharacterClass.Cabalist,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Cabalist,
                    Name = "Cabalist",
                    BaseName = "Mage",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.GuildofShadows",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Mage,
                }
            },
            {
                (int)eCharacterClass.Fighter,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Fighter,
                    Name = "Fighter",
                    BaseName = "Fighter",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Korazh,
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Elementalist,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Elementalist,
                    Name = "Elementalist",
                    BaseName = "Elementalist",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Elementalist,
                }
            },
            {
                (int)eCharacterClass.Acolyte,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Acolyte,
                    Name = "Acolyte",
                    BaseName = "Acolyte",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Korazh,
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 720,
                    BaseWeaponSkill = 320,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Acolyte,
                }
            },
            {
                (int)eCharacterClass.AlbionRogue,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.AlbionRogue,
                    Name = "Rogue",
                    BaseName = "Rogue",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Stalker,
                }
            },
            {
                (int)eCharacterClass.Mage,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Mage,
                    Name = "Mage",
                    BaseName = "Mage",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Mage,
                }
            },
            {
                (int)eCharacterClass.Reaver,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Reaver,
                    Name = "Reaver",
                    BaseName = "Fighter",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.TempleofArawn",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Slash, Specs.Flexible },
                    LeveledStats = (eStat.STR, eStat.DEX, eStat.PIE),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 760,
                    BaseWeaponSkill = 380,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Disciple,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Disciple,
                    Name = "Disciple",
                    BaseName = "Disciple",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Disciple,
                }
            },
            {
                (int)eCharacterClass.Thane,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Thane,
                    Name = "Thane",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofThor",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.STR, eStat.PIE, eStat.CON),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Deifrang,
                        PlayerRace.Norseman,
                        PlayerRace.Troll },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Warrior,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Warrior,
                    Name = "Warrior",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofTyr",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Axe, Specs.Hammer, Specs.Sword },
                    LeveledStats = (eStat.STR, eStat.CON, eStat.DEX),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Kobold,
                        PlayerRace.Deifrang,
                        PlayerRace.Norseman,
                        PlayerRace.Troll,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 880,
                    BaseWeaponSkill = 460,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Shadowblade,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Shadowblade,
                    Name = "Shadowblade",
                    BaseName = "MidgardRogue",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.Loki",
                    SpecPointsMultiplier = 22,
                    AutoTrainSkills = new[] { Specs.Stealth },
                    LeveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = true,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 760,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.MidgardRogue,
                }
            },
            {
                (int)eCharacterClass.Skald,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Skald,
                    Name = "Skald",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofBragi",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.CHR, eStat.STR, eStat.CON),
                    ManaStat = eStat.CHR,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Troll },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 760,
                    BaseWeaponSkill = 380,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Hunter,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Hunter,
                    Name = "Hunter",
                    BaseName = "MidgardRogue",
                    FemaleName = "Huntress",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofSkadi",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Archery, Specs.CompositeBow },
                    LeveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.DEX,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 380,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.MidgardRogue,
                }
            },
            {
                (int)eCharacterClass.Healer,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Healer,
                    Name = "Healer",
                    BaseName = "Seer",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofEir",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.PIE, eStat.CON, eStat.STR),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Norseman },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Seer,
                }
            },
            {
                (int)eCharacterClass.Spiritmaster,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Spiritmaster,
                    Name = "Spiritmaster",
                    BaseName = "Mystic",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofHel",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.PIE, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Mystic,
                }
            },
            {
                (int)eCharacterClass.Shaman,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Shaman,
                    Name = "Shaman",
                    BaseName = "Seer",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofYmir",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.PIE, eStat.CON, eStat.STR),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Troll },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Seer,
                }
            },
            {
                (int)eCharacterClass.Runemaster,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Runemaster,
                    Name = "Runemaster",
                    BaseName = "Mystic",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofOdin",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.PIE, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Mystic,
                }
            },
            {
                (int)eCharacterClass.Bonedancer,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Bonedancer,
                    Name = "Bonedancer",
                    BaseName = "Mystic",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofBodgar",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.PIE, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Kobold,
                        PlayerRace.Troll,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Mystic,
                }
            },
            {
                (int)eCharacterClass.Berserker,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Berserker,
                    Name = "Berserker",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofModi",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.STR, eStat.DEX, eStat.CON),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Deifrang,
                        PlayerRace.Norseman,
                        PlayerRace.Troll,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = true,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Savage,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Savage,
                    Name = "Savage",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofKelgor",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Troll,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = true,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 880,
                    BaseWeaponSkill = 400,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Heretic,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Heretic,
                    Name = "Heretic",
                    BaseName = "Acolyte",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.TempleofArawn",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.PIE, eStat.DEX, eStat.CON),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Korazh,
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Acolyte,
                }
            },
            {
                (int)eCharacterClass.Valkyrie,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Valkyrie,
                    Name = "Valkyrie",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofOdin",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.CON, eStat.STR, eStat.DEX),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Norseman,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Viking,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Viking,
                    Name = "Viking",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Deifrang,
                        PlayerRace.Norseman,
                        PlayerRace.Troll,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Mystic,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Mystic,
                    Name = "Mystic",
                    BaseName = "Mystic",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Troll,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Mystic,
                }
            },
            {
                (int)eCharacterClass.Seer,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Seer,
                    Name = "Seer",
                    BaseName = "Seer",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Troll },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Seer,
                }
            },
            {
                (int)eCharacterClass.MidgardRogue,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.MidgardRogue,
                    Name = "Rogue",
                    BaseName = "Rogue",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.MidgardRogue,
                }
            },
            {
                (int)eCharacterClass.Bainshee,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Bainshee,
                    Name = "Bainshee",
                    BaseName = "Magician",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofAffinity",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.INT, eStat.DEX, eStat.CON),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Magician,
                }
            },
            {
                (int)eCharacterClass.Eldritch,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Eldritch,
                    Name = "Eldritch",
                    BaseName = "Magician",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofFocus",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Magician,
                }
            },
            {
                (int)eCharacterClass.Enchanter,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Enchanter,
                    Name = "Enchanter",
                    BaseName = "Magician",
                    FemaleName = "Enchantress",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofEssence",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Magician,
                }
            },
            {
                (int)eCharacterClass.Mentalist,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Mentalist,
                    Name = "Mentalist",
                    BaseName = "Magician",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofHarmony",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Magician,
                }
            },
            {
                (int)eCharacterClass.Blademaster,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Blademaster,
                    Name = "Blademaster",
                    BaseName = "Guardian",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofHarmony",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.STR, eStat.DEX, eStat.CON),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Firbolg,
                        PlayerRace.Graoch,
                        PlayerRace.Shar },
                    CanUseLeftHandedWeapon = true,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Hero,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Hero,
                    Name = "Hero",
                    BaseName = "Guardian",
                    FemaleName = "Heroine",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofFocus",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.STR, eStat.CON, eStat.DEX),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Graoch,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Champion,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Champion,
                    Name = "Champion",
                    BaseName = "Guardian",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofEssence",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.STR, eStat.INT, eStat.DEX),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Graoch,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 760,
                    BaseWeaponSkill = 380,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Warden,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Warden,
                    Name = "Warden",
                    BaseName = "Naturalist",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofFocus",
                    SpecPointsMultiplier = 18,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.EMP, eStat.STR, eStat.CON),
                    ManaStat = eStat.EMP,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Graoch,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Naturalist,
                }
            },
            {
                (int)eCharacterClass.Druid,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Druid,
                    Name = "Druid",
                    BaseName = "Naturalist",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofHarmony",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.EMP, eStat.CON, eStat.STR),
                    ManaStat = eStat.EMP,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 320,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Naturalist,
                }
            },
            {
                (int)eCharacterClass.Bard,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Bard,
                    Name = "Bard",
                    BaseName = "Naturalist",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofEssence",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.CHR, eStat.EMP, eStat.CON),
                    ManaStat = eStat.CHR,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Naturalist,
                }
            },
            {
                (int)eCharacterClass.Nightshade,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Nightshade,
                    Name = "Nightshade",
                    BaseName = "Stalker",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofEssence",
                    SpecPointsMultiplier = 22,
                    AutoTrainSkills = new[] { Specs.Stealth },
                    LeveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.DEX,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen },
                    CanUseLeftHandedWeapon = true,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Stalker,
                }
            },
            {
                (int)eCharacterClass.Ranger,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Ranger,
                    Name = "Ranger",
                    BaseName = "Stalker",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofFocus",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Archery, Specs.RecurveBow },
                    LeveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.DEX,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = true,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Stalker,
                }
            },
            {
                (int)eCharacterClass.Magician,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Magician,
                    Name = "Magician",
                    BaseName = "Magician",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Magician,
                }
            },
            {
                (int)eCharacterClass.Guardian,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Guardian,
                    Name = "Guardian",
                    BaseName = "Guardian",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Firbolg,
                        PlayerRace.Graoch,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 880,
                    BaseWeaponSkill = 400,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.Naturalist,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Naturalist,
                    Name = "Naturalist",
                    BaseName = "Naturalist",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.EMP,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Sylvan,
                        PlayerRace.Graoch },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Naturalist,
                }
            },
            {
                (int)eCharacterClass.Stalker,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Stalker,
                    Name = "Stalker",
                    BaseName = "Stalker",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = eClassType.PureTank,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Stalker,
                }
            },
            {
                (int)eCharacterClass.Animist,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Animist,
                    Name = "Animist",
                    BaseName = "Forester",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofAffinity",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.INT, eStat.CON, eStat.DEX),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Forester,
                }
            },
            {
                (int)eCharacterClass.Valewalker,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Valewalker,
                    Name = "Valewalker",
                    BaseName = "Forester",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofAffinity",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.STR, eStat.INT, eStat.CON),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 400,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Forester,
                }
            },
            {
                (int)eCharacterClass.Forester,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Forester,
                    Name = "Forester",
                    BaseName = "Forester",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.INT,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = false,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Forester,
                }
            },
            {
                (int)eCharacterClass.Vampiir,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Vampiir,
                    Name = "Vampiir",
                    BaseName = "Stalker",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofAffinity",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.CON, eStat.STR, eStat.DEX),
                    ManaStat = eStat.UNDEFINED,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 878,
                    BaseWeaponSkill = 440,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Stalker,
                }
            },
            {
                (int)eCharacterClass.Warlock,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Warlock,
                    Name = "Warlock",
                    BaseName = "Mystic",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofHel",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.PIE, eStat.CON, eStat.DEX),
                    ManaStat = eStat.PIE,
                    EligibleRaces = new[] {
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman },
                    CanUseLeftHandedWeapon = false,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = eClassType.ListCaster,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Mystic,
                }
            },
            {
                (int)eCharacterClass.MaulerAlb,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.MaulerAlb,
                    Name = "Mauler",
                    BaseName = "Fighter",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.TempleofIronFist",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.STR, eStat.CON, eStat.QUI),
                    ManaStat = eStat.STR,
                    EligibleRaces = new[] {
                        PlayerRace.Korazh,
                        PlayerRace.Briton,
                        PlayerRace.Inconnu },
                    CanUseLeftHandedWeapon = true,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 600,
                    BaseWeaponSkill = 440,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.MaulerMid,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.MaulerMid,
                    Name = "Mauler",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.TempleofIronFist",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.STR, eStat.CON, eStat.QUI),
                    ManaStat = eStat.STR,
                    EligibleRaces = new[] {
                        PlayerRace.Kobold,
                        PlayerRace.Deifrang,
                        PlayerRace.Norseman },
                    CanUseLeftHandedWeapon = true,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 600,
                    BaseWeaponSkill = 440,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
            {
                (int)eCharacterClass.MaulerHib,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.MaulerHib,
                    Name = "Mauler",
                    BaseName = "Guardian",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.TempleofIronFist",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    LeveledStats = (eStat.STR, eStat.CON, eStat.QUI),
                    ManaStat = eStat.STR,
                    EligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Graoch,
                        PlayerRace.Lurikeen },
                    CanUseLeftHandedWeapon = true,
                    HasAdvancedFromBaseClass = true,
                    BaseHP = 600,
                    BaseWeaponSkill = 440,
                    ClassType = eClassType.Hybrid,
                    ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.Fighter,
                }
            },
        };
        #endregion
    }
}