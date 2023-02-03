using System;
using System.Collections.Generic;
using System.Linq;
using DOL.Database;
using DOL.GS.Realm;
using DOL.Language;

namespace DOL.GS
{
    public class CharacterClass : ICharacterClass
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private GamePlayer player;
        private string name;
        private bool hasAdvancedFromBaseClass;
        private IEnumerable<PlayerRace> eligibleRaces;
        private (eStat Primary, eStat Secondary, eStat Tertiary) leveledStats;

        public int ID { get; private set; }
        public string BaseName { get; private set; }
        public string FemaleName { get; private set; }
        public string ProfessionTranslationId { get; private set; } = "";
        public int SpecPointsMultiplier { get; private set; }
        public IEnumerable<string> AutoTrainSkills { get; private set; }
        public eStat ManaStat { get; private set; }
        public bool CanUseLeftHandedWeapon { get; private set; }
        public int BaseHP { get; private set; }
        public int BaseWeaponSkill { get; private set; }
        public eClassType ClassType { get; private set; }
        public int ChampionTrainerID { get; private set; }
        public ushort MaxPulsingSpells { get; private set; } = 2;

        public string Name => GetSalutation(player);
        public string Profession
            => LanguageMgr.TryTranslateOrDefault(player, ProfessionTranslationId, ProfessionTranslationId);
        public int AdjustedSpecPointsMultiplier => SpecPointsMultiplier;
        public eStat PrimaryStat => leveledStats.Primary;
        public eStat SecondaryStat => leveledStats.Secondary;
        public eStat TertiaryStat => leveledStats.Tertiary;
        public int WeaponSkillBase => BaseWeaponSkill;
        public IList<string> GetAutotrainableSkills()
            => AutoTrainSkills.ToList();
        public GameTrainer.eChampionTrainerType ChampionTrainerType()
            => (GameTrainer.eChampionTrainerType)ChampionTrainerID;
        public bool CanUseLefthandedWeapon
            => CanUseLeftHandedWeapon;
        public bool HasAdvancedFromBaseClass()
            => hasAdvancedFromBaseClass;
        public List<PlayerRace> EligibleRaces => eligibleRaces.ToList();

        private CharacterClass() { }

        private CharacterClass(GamePlayer player) 
        { 
            this.player = player;
        }

        public static CharacterClass Create(GamePlayer player, int classID)
        {
            var characterClass = GetClass(classID);
            characterClass.player = player;
            return characterClass;
        }

        public string GetSalutation(GamePlayer player)
        {
            var femaleName = FemaleName;
            var useFemaleName = (player != null && player.Gender == eGender.Female && !Util.IsEmpty(femaleName));
            if (useFemaleName) return femaleName;
            else return name;
        }

        public string GetTitle(GamePlayer player, int level)
        {
            if (!hasAdvancedFromBaseClass) level = 0;

            // Clamp level in 5 by 5 steps - 50 is the max available translation for now
            int clamplevel = Math.Min(50, (level / 5) * 5);

            string none = LanguageMgr.TryTranslateOrDefault(player, "!None!", "PlayerClass.GetTitle.none");

            if (clamplevel > 0)
                return LanguageMgr.TryTranslateOrDefault(player, string.Format("!{0}!", name), string.Format("PlayerClass.{0}.GetTitle.{1}", name, clamplevel));

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
            if (Equals(CharacterClass.Unknown)) return CharacterClass.Unknown;

            return allClasses.Values.Where(c => c.name == BaseName).First();
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
                    if (newElibibleRaces.Any()) characterClass.eligibleRaces = newElibibleRaces;

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
                name = "Unknown Class",
                BaseName = "Unknown Base Class",
                FemaleName = "",
                ProfessionTranslationId = "",
                SpecPointsMultiplier = 10,
                AutoTrainSkills = new string[] { },
                leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                ManaStat = eStat.UNDEFINED,
                eligibleRaces = PlayerRace.AllRaces,
                CanUseLeftHandedWeapon = false,
                hasAdvancedFromBaseClass = true,
                BaseHP = 600,
                BaseWeaponSkill = 400,
                ClassType = eClassType.ListCaster,
                ChampionTrainerID = (int)GameTrainer.eChampionTrainerType.None,
            };

        public override bool Equals(object obj)
        {
            if (obj is CharacterClass characterClass)
            {
                return characterClass.ID == ID;
            }
            return false;
        }

        public override int GetHashCode()
            => ID;

        #region CharacterClass(Base) creation shortcuts
        public static ICharacterClass None => CharacterClass.GetClass((int)eCharacterClass.Unknown);
        //alb
        public static ICharacterClass Armsman => CharacterClass.GetClass((int)eCharacterClass.Armsman);
        public static ICharacterClass Cabalist => CharacterClass.GetClass((int)eCharacterClass.Cabalist);
        public static ICharacterClass Cleric => CharacterClass.GetClass((int)eCharacterClass.Cleric);
        public static ICharacterClass Friar => CharacterClass.GetClass((int)eCharacterClass.Friar);
        public static ICharacterClass Heretic => CharacterClass.GetClass((int)eCharacterClass.Heretic);
        public static ICharacterClass Infiltrator => CharacterClass.GetClass((int)eCharacterClass.Infiltrator);
        public static ICharacterClass Mercenary => CharacterClass.GetClass((int)eCharacterClass.Mercenary);
        public static ICharacterClass Minstrel => CharacterClass.GetClass((int)eCharacterClass.Minstrel);
        public static ICharacterClass Necromancer => CharacterClass.GetClass((int)eCharacterClass.Necromancer);
        public static ICharacterClass Paladin => CharacterClass.GetClass((int)eCharacterClass.Paladin);
        public static ICharacterClass Reaver => CharacterClass.GetClass((int)eCharacterClass.Reaver);
        public static ICharacterClass Scout => CharacterClass.GetClass((int)eCharacterClass.Scout);
        public static ICharacterClass Sorcerer => CharacterClass.GetClass((int)eCharacterClass.Sorcerer);
        public static ICharacterClass Theurgist => CharacterClass.GetClass((int)eCharacterClass.Theurgist);
        public static ICharacterClass Wizard => CharacterClass.GetClass((int)eCharacterClass.Wizard);
        public static ICharacterClass MaulerAlb => CharacterClass.GetClass((int)eCharacterClass.MaulerAlb);
        //mid
        public static ICharacterClass Berserker => CharacterClass.GetClass((int)eCharacterClass.Berserker);
        public static ICharacterClass Bonedancer => CharacterClass.GetClass((int)eCharacterClass.Bonedancer);
        public static ICharacterClass Healer => CharacterClass.GetClass((int)eCharacterClass.Healer);
        public static ICharacterClass Hunter => CharacterClass.GetClass((int)eCharacterClass.Hunter);
        public static ICharacterClass Runemaster => CharacterClass.GetClass((int)eCharacterClass.Runemaster);
        public static ICharacterClass Savage => CharacterClass.GetClass((int)eCharacterClass.Savage);
        public static ICharacterClass Shadowblade => CharacterClass.GetClass((int)eCharacterClass.Shadowblade);
        public static ICharacterClass Shaman => CharacterClass.GetClass((int)eCharacterClass.Shaman);
        public static ICharacterClass Skald => CharacterClass.GetClass((int)eCharacterClass.Skald);
        public static ICharacterClass Spiritmaster => CharacterClass.GetClass((int)eCharacterClass.Spiritmaster);
        public static ICharacterClass Thane => CharacterClass.GetClass((int)eCharacterClass.Thane);
        public static ICharacterClass Valkyrie => CharacterClass.GetClass((int)eCharacterClass.Valkyrie);
        public static ICharacterClass Warlock => CharacterClass.GetClass((int)eCharacterClass.Warlock);
        public static ICharacterClass Warrior => CharacterClass.GetClass((int)eCharacterClass.Warrior);
        public static ICharacterClass MaulerMid => CharacterClass.GetClass((int)eCharacterClass.MaulerMid);
        //hib
        public static ICharacterClass Animist => CharacterClass.GetClass((int)eCharacterClass.Animist);
        public static ICharacterClass Bainshee => CharacterClass.GetClass((int)eCharacterClass.Bainshee);
        public static ICharacterClass Bard => CharacterClass.GetClass((int)eCharacterClass.Bard);
        public static ICharacterClass Blademaster => CharacterClass.GetClass((int)eCharacterClass.Blademaster);
        public static ICharacterClass Champion => CharacterClass.GetClass((int)eCharacterClass.Champion);
        public static ICharacterClass Druid => CharacterClass.GetClass((int)eCharacterClass.Druid);
        public static ICharacterClass Eldritch => CharacterClass.GetClass((int)eCharacterClass.Eldritch);
        public static ICharacterClass Enchanter => CharacterClass.GetClass((int)eCharacterClass.Enchanter);
        public static ICharacterClass Hero => CharacterClass.GetClass((int)eCharacterClass.Hero);
        public static ICharacterClass Mentalist => CharacterClass.GetClass((int)eCharacterClass.Mentalist);
        public static ICharacterClass Nightshade => CharacterClass.GetClass((int)eCharacterClass.Nightshade);
        public static ICharacterClass Ranger => CharacterClass.GetClass((int)eCharacterClass.Ranger);
        public static ICharacterClass Valewalker => CharacterClass.GetClass((int)eCharacterClass.Valewalker);
        public static ICharacterClass Vampiir => CharacterClass.GetClass((int)eCharacterClass.Vampiir);
        public static ICharacterClass Warden => CharacterClass.GetClass((int)eCharacterClass.Warden);
        public static ICharacterClass MaulerHib => CharacterClass.GetClass((int)eCharacterClass.MaulerHib);
        #endregion

        #region Default Database
        private static Dictionary<int, CharacterClass> allClasses = new Dictionary<int, CharacterClass>()
        {
            {
                (int)eCharacterClass.Paladin,
                new CharacterClass()
                {
                    ID = (int)eCharacterClass.Paladin,
                    name = "Paladin",
                    BaseName = "Fighter",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.ChurchofAlbion",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Slash, Specs.Chants },
                    leveledStats = (eStat.CON, eStat.PIE, eStat.STR),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.Highlander,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Armsman",
                    BaseName = "Fighter",
                    FemaleName = "Armswoman",
                    ProfessionTranslationId = "PlayerClass.Profession.DefendersofAlbion",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Slash, Specs.Thrust },
                    leveledStats = (eStat.STR, eStat.CON, eStat.DEX),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Korazh,
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Scout",
                    BaseName = "Rogue",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.DefendersofAlbion",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Archery, Specs.Longbow },
                    leveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.DEX,
                    eligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Minstrel",
                    BaseName = "Rogue",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.Academy",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new[] { Specs.Instruments },
                    leveledStats = (eStat.CHR, eStat.DEX, eStat.STR),
                    ManaStat = eStat.CHR,
                    eligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Highlander,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Theurgist",
                    BaseName = "Elementalist",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.DefendersofAlbion",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Cleric",
                    BaseName = "Acolyte",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.ChurchofAlbion",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.PIE, eStat.CON, eStat.STR),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.Highlander },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Wizard",
                    BaseName = "Elementalist",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.Academy",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Sorcerer",
                    BaseName = "Mage",
                    FemaleName = "Sorceress",
                    ProfessionTranslationId = "PlayerClass.Profession.Academy",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Infiltrator",
                    BaseName = "Rogue",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.GuildofShadows",
                    SpecPointsMultiplier = 25,
                    AutoTrainSkills = new[] { Specs.Stealth },
                    leveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = true,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Friar",
                    BaseName = "Acolyte",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.DefendersofAlbion",
                    SpecPointsMultiplier = 18,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.PIE, eStat.CON, eStat.STR),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.Highlander },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Mercenary",
                    BaseName = "Fighter",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.GuildofShadows",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Slash, Specs.Thrust },
                    leveledStats = (eStat.STR, eStat.DEX, eStat.CON),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Korazh,
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = true,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Necromancer",
                    BaseName = "Disciple",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.TempleofArawn",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Cabalist",
                    BaseName = "Mage",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.GuildofShadows",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Fighter",
                    BaseName = "Fighter",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Korazh,
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Elementalist",
                    BaseName = "Elementalist",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Acolyte",
                    BaseName = "Acolyte",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Korazh,
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Rogue",
                    BaseName = "Rogue",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Highlander,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Mage",
                    BaseName = "Mage",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.HalfOgre,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Reaver",
                    BaseName = "Fighter",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.TempleofArawn",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Slash, Specs.Flexible },
                    leveledStats = (eStat.STR, eStat.DEX, eStat.PIE),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Disciple",
                    BaseName = "Disciple",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Briton,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Thane",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofThor",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.STR, eStat.PIE, eStat.CON),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Deifrang,
                        PlayerRace.Norseman,
                        PlayerRace.Troll },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Warrior",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofTyr",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Axe, Specs.Hammer, Specs.Sword },
                    leveledStats = (eStat.STR, eStat.CON, eStat.DEX),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Kobold,
                        PlayerRace.Deifrang,
                        PlayerRace.Norseman,
                        PlayerRace.Troll,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Shadowblade",
                    BaseName = "MidgardRogue",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.Loki",
                    SpecPointsMultiplier = 22,
                    AutoTrainSkills = new[] { Specs.Stealth },
                    leveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = true,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Skald",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofBragi",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.CHR, eStat.STR, eStat.CON),
                    ManaStat = eStat.CHR,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Troll },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Hunter",
                    BaseName = "MidgardRogue",
                    FemaleName = "Huntress",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofSkadi",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Archery, Specs.CompositeBow },
                    leveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.DEX,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Healer",
                    BaseName = "Seer",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofEir",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.PIE, eStat.CON, eStat.STR),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Norseman },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Spiritmaster",
                    BaseName = "Mystic",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofHel",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.PIE, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Shaman",
                    BaseName = "Seer",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofYmir",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.PIE, eStat.CON, eStat.STR),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Troll },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Runemaster",
                    BaseName = "Mystic",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofOdin",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.PIE, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Bonedancer",
                    BaseName = "Mystic",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofBodgar",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.PIE, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Kobold,
                        PlayerRace.Troll,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Berserker",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofModi",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.STR, eStat.DEX, eStat.CON),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Deifrang,
                        PlayerRace.Norseman,
                        PlayerRace.Troll,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = true,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Savage",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofKelgor",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Troll,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = true,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Heretic",
                    BaseName = "Acolyte",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.TempleofArawn",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.PIE, eStat.DEX, eStat.CON),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Korazh,
                        PlayerRace.Avalonian,
                        PlayerRace.Briton,
                        PlayerRace.Inconnu,
                        PlayerRace.Saracen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Valkyrie",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofOdin",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.CON, eStat.STR, eStat.DEX),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Norseman,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Viking",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Deifrang,
                        PlayerRace.Norseman,
                        PlayerRace.Troll,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Mystic",
                    BaseName = "Mystic",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Troll,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Seer",
                    BaseName = "Seer",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Troll },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Rogue",
                    BaseName = "Rogue",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Dwarf,
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman,
                        PlayerRace.Valkyn },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Bainshee",
                    BaseName = "Magician",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofAffinity",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.INT, eStat.DEX, eStat.CON),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Eldritch",
                    BaseName = "Magician",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofFocus",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Enchanter",
                    BaseName = "Magician",
                    FemaleName = "Enchantress",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofEssence",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Mentalist",
                    BaseName = "Magician",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofHarmony",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.INT, eStat.DEX, eStat.QUI),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Blademaster",
                    BaseName = "Guardian",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofHarmony",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.STR, eStat.DEX, eStat.CON),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Firbolg,
                        PlayerRace.Graoch,
                        PlayerRace.Shar },
                    CanUseLeftHandedWeapon = true,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Hero",
                    BaseName = "Guardian",
                    FemaleName = "Heroine",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofFocus",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.STR, eStat.CON, eStat.DEX),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Graoch,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Champion",
                    BaseName = "Guardian",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofEssence",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.STR, eStat.INT, eStat.DEX),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Graoch,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Warden",
                    BaseName = "Naturalist",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofFocus",
                    SpecPointsMultiplier = 18,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.EMP, eStat.STR, eStat.CON),
                    ManaStat = eStat.EMP,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Graoch,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Druid",
                    BaseName = "Naturalist",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofHarmony",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.EMP, eStat.CON, eStat.STR),
                    ManaStat = eStat.EMP,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Bard",
                    BaseName = "Naturalist",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofEssence",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.CHR, eStat.EMP, eStat.CON),
                    ManaStat = eStat.CHR,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Nightshade",
                    BaseName = "Stalker",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofEssence",
                    SpecPointsMultiplier = 22,
                    AutoTrainSkills = new[] { Specs.Stealth },
                    leveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.DEX,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen },
                    CanUseLeftHandedWeapon = true,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Ranger",
                    BaseName = "Stalker",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofFocus",
                    SpecPointsMultiplier = 20,
                    AutoTrainSkills = new[] { Specs.Archery, Specs.RecurveBow },
                    leveledStats = (eStat.DEX, eStat.QUI, eStat.STR),
                    ManaStat = eStat.DEX,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = true,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Magician",
                    BaseName = "Magician",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Guardian",
                    BaseName = "Guardian",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Firbolg,
                        PlayerRace.Graoch,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Naturalist",
                    BaseName = "Naturalist",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.EMP,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Sylvan,
                        PlayerRace.Graoch },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Stalker",
                    BaseName = "Stalker",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Elf,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Animist",
                    BaseName = "Forester",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofAffinity",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.INT, eStat.CON, eStat.DEX),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Valewalker",
                    BaseName = "Forester",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofAffinity",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.STR, eStat.INT, eStat.CON),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Forester",
                    BaseName = "Forester",
                    FemaleName = "",
                    ProfessionTranslationId = "",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.UNDEFINED, eStat.UNDEFINED, eStat.UNDEFINED),
                    ManaStat = eStat.INT,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Firbolg,
                        PlayerRace.Sylvan },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = false,
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
                    name = "Vampiir",
                    BaseName = "Stalker",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.PathofAffinity",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.CON, eStat.STR, eStat.DEX),
                    ManaStat = eStat.UNDEFINED,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Lurikeen,
                        PlayerRace.Shar },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Warlock",
                    BaseName = "Mystic",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.HouseofHel",
                    SpecPointsMultiplier = 10,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.PIE, eStat.CON, eStat.DEX),
                    ManaStat = eStat.PIE,
                    eligibleRaces = new[] {
                        PlayerRace.Frostalf,
                        PlayerRace.Kobold,
                        PlayerRace.Norseman },
                    CanUseLeftHandedWeapon = false,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Mauler",
                    BaseName = "Fighter",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.TempleofIronFist",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.STR, eStat.CON, eStat.QUI),
                    ManaStat = eStat.STR,
                    eligibleRaces = new[] {
                        PlayerRace.Korazh,
                        PlayerRace.Briton,
                        PlayerRace.Inconnu },
                    CanUseLeftHandedWeapon = true,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Mauler",
                    BaseName = "Viking",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.TempleofIronFist",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.STR, eStat.CON, eStat.QUI),
                    ManaStat = eStat.STR,
                    eligibleRaces = new[] {
                        PlayerRace.Kobold,
                        PlayerRace.Deifrang,
                        PlayerRace.Norseman },
                    CanUseLeftHandedWeapon = true,
                    hasAdvancedFromBaseClass = true,
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
                    name = "Mauler",
                    BaseName = "Guardian",
                    FemaleName = "",
                    ProfessionTranslationId = "PlayerClass.Profession.TempleofIronFist",
                    SpecPointsMultiplier = 15,
                    AutoTrainSkills = new string[] {  },
                    leveledStats = (eStat.STR, eStat.CON, eStat.QUI),
                    ManaStat = eStat.STR,
                    eligibleRaces = new[] {
                        PlayerRace.Celt,
                        PlayerRace.Graoch,
                        PlayerRace.Lurikeen },
                    CanUseLeftHandedWeapon = true,
                    hasAdvancedFromBaseClass = true,
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