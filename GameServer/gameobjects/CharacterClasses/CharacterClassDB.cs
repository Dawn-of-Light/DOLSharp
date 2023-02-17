using System;
using System.Collections.Generic;
using System.Linq;
using DOL.Database;
using DOL.GS.Realm;

namespace DOL.GS
{
    public class CharacterClassDB
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Load()
        {
            var dbClasses = DOLDB<DBCharacterClass>.SelectAllObjects().ToDictionary(c => c.ID, c => c);
            foreach (var classID in defaultClasses.Keys.ToList().Union(dbClasses.Keys))
            {
                CharacterClass charClass;
                if (dbClasses.TryGetValue(classID, out var databaseEntry))
                {
                    try
                    {
                        charClass = CharacterClass.Create(databaseEntry);
                    }
                    catch (Exception e)
                    {
                        log.Error($"CharacterClass with ID {classID} could not be loaded from database. Load default instead.:\n{e}");
                        charClass = CharacterClass.Create(defaultClasses[classID]);
                    }
                }
                else
                {
                    var dbClass = defaultClasses[classID];
                    dbClass.AllowAdd = true;
                    GameServer.Database.AddObject(dbClass);
                    charClass = CharacterClass.Create(dbClass);
                }

                CharacterClass.AddOrReplace(charClass);
            }
        }

        private static Dictionary<byte, DBCharacterClass> defaultClasses = new Dictionary<byte, DBCharacterClass>()
        {
            {
                (byte)eCharacterClass.Paladin,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Paladin,
                    Name = "Paladin",
                    BaseClassID = (byte)eCharacterClass.Fighter,
                    ProfessionTranslationID = "PlayerClass.Profession.ChurchofAlbion",
                    SpecPointMultiplier = 20,
                    AutoTrainSkills = $"{Specs.Slash},{Specs.Chants}",
                    PrimaryStat = (int)eStat.CON,
                    SecondaryStat = (int)eStat.PIE,
                    TertiaryStat = (int)eStat.STR,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Highlander.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    BaseHP = 760,
                    BaseWeaponSkill = 380,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Armsman,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Armsman,
                    Name = "Armsman",
                    BaseClassID = (byte)eCharacterClass.Fighter,
                    FemaleName = "Armswoman",
                    ProfessionTranslationID = "PlayerClass.Profession.DefendersofAlbion",
                    SpecPointMultiplier = 20,
                    AutoTrainSkills = $"{Specs.Slash},{Specs.Thrust}",
                    PrimaryStat = (int)eStat.STR,
                    SecondaryStat = (int)eStat.CON,
                    TertiaryStat = (int)eStat.DEX,
                    EligibleRaces = $"{(int)(PlayerRace.Korazh.ID)},{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.HalfOgre.ID)},{(int)(PlayerRace.Highlander.ID)},{(int)(PlayerRace.Inconnu.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Scout,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Scout,
                    Name = "Scout",
                    BaseClassID = (byte)eCharacterClass.AlbionRogue,
                    ProfessionTranslationID = "PlayerClass.Profession.DefendersofAlbion",
                    SpecPointMultiplier = 20,
                    AutoTrainSkills = $"{Specs.Archery},{Specs.Longbow}",
                    PrimaryStat = (int)eStat.DEX,
                    SecondaryStat = (int)eStat.QUI,
                    TertiaryStat = (int)eStat.STR,
                    ManaStat = (int)eStat.DEX,
                    EligibleRaces = $"{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Highlander.ID)},{(int)(PlayerRace.Inconnu.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Minstrel,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Minstrel,
                    Name = "Minstrel",
                    BaseClassID = (byte)eCharacterClass.AlbionRogue,
                    ProfessionTranslationID = "PlayerClass.Profession.Academy",
                    SpecPointMultiplier = 15,
                    AutoTrainSkills = $"{Specs.Instruments}",
                    PrimaryStat = (int)eStat.CHR,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.STR,
                    ManaStat = (int)eStat.CHR,
                    EligibleRaces = $"{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Highlander.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 380,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Theurgist,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Theurgist,
                    Name = "Theurgist",
                    BaseClassID = (byte)eCharacterClass.Elementalist,
                    ProfessionTranslationID = "PlayerClass.Profession.DefendersofAlbion",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.INT,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.HalfOgre.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Cleric,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Cleric,
                    Name = "Cleric",
                    BaseClassID = (byte)eCharacterClass.Acolyte,
                    ProfessionTranslationID = "PlayerClass.Profession.ChurchofAlbion",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.PIE,
                    SecondaryStat = (int)eStat.CON,
                    TertiaryStat = (int)eStat.STR,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Highlander.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 320,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Wizard,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Wizard,
                    Name = "Wizard",
                    BaseClassID = (byte)eCharacterClass.Elementalist,
                    ProfessionTranslationID = "PlayerClass.Profession.Academy",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.INT,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.HalfOgre.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 240,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Sorcerer,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Sorcerer,
                    Name = "Sorcerer",
                    BaseClassID = (byte)eCharacterClass.Mage,
                    FemaleName = "Sorceress",
                    ProfessionTranslationID = "PlayerClass.Profession.Academy",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.INT,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.HalfOgre.ID)},{(int)(PlayerRace.Inconnu.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Infiltrator,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Infiltrator,
                    Name = "Infiltrator",
                    BaseClassID = (byte)eCharacterClass.AlbionRogue,
                    ProfessionTranslationID = "PlayerClass.Profession.GuildofShadows",
                    SpecPointMultiplier = 25,
                    AutoTrainSkills = $"{Specs.Stealth}",
                    PrimaryStat = (int)eStat.DEX,
                    SecondaryStat = (int)eStat.QUI,
                    TertiaryStat = (int)eStat.STR,
                    EligibleRaces = $"{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Highlander.ID)},{(int)(PlayerRace.Inconnu.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    CanUseLeftHandedWeapon = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Friar,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Friar,
                    Name = "Friar",
                    BaseClassID = (byte)eCharacterClass.Acolyte,
                    ProfessionTranslationID = "PlayerClass.Profession.DefendersofAlbion",
                    SpecPointMultiplier = 18,
                    PrimaryStat = (int)eStat.PIE,
                    SecondaryStat = (int)eStat.CON,
                    TertiaryStat = (int)eStat.STR,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Highlander.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Mercenary,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Mercenary,
                    Name = "Mercenary",
                    BaseClassID = (byte)eCharacterClass.Fighter,
                    ProfessionTranslationID = "PlayerClass.Profession.GuildofShadows",
                    SpecPointMultiplier = 20,
                    AutoTrainSkills = $"{Specs.Slash},{Specs.Thrust}",
                    PrimaryStat = (int)eStat.STR,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.CON,
                    EligibleRaces = $"{(int)(PlayerRace.Korazh.ID)},{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.HalfOgre.ID)},{(int)(PlayerRace.Highlander.ID)},{(int)(PlayerRace.Inconnu.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    CanUseLeftHandedWeapon = true,
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Necromancer,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Necromancer,
                    Name = "Necromancer",
                    BaseClassID = (byte)eCharacterClass.Disciple,
                    ProfessionTranslationID = "PlayerClass.Profession.TempleofArawn",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.INT,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Inconnu.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Cabalist,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Cabalist,
                    Name = "Cabalist",
                    BaseClassID = (byte)eCharacterClass.Mage,
                    ProfessionTranslationID = "PlayerClass.Profession.GuildofShadows",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.INT,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.HalfOgre.ID)},{(int)(PlayerRace.Inconnu.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Fighter,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Fighter,
                    Name = "Fighter",
                    BaseClassID = (byte)eCharacterClass.Fighter,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    EligibleRaces = $"{(int)(PlayerRace.Korazh.ID)},{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.HalfOgre.ID)},{(int)(PlayerRace.Highlander.ID)},{(int)(PlayerRace.Inconnu.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Elementalist,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Elementalist,
                    Name = "Elementalist",
                    BaseClassID = (byte)eCharacterClass.Elementalist,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.HalfOgre.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Acolyte,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Acolyte,
                    Name = "Acolyte",
                    BaseClassID = (byte)eCharacterClass.Acolyte,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Korazh.ID)},{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Highlander.ID)},{(int)(PlayerRace.Inconnu.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 320,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.AlbionRogue,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.AlbionRogue,
                    Name = "Rogue",
                    BaseClassID = (byte)eCharacterClass.AlbionRogue,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    EligibleRaces = $"{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Highlander.ID)},{(int)(PlayerRace.Inconnu.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Mage,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Mage,
                    Name = "Mage",
                    BaseClassID = (byte)eCharacterClass.Mage,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.HalfOgre.ID)},{(int)(PlayerRace.Inconnu.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Reaver,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Reaver,
                    Name = "Reaver",
                    BaseClassID = (byte)eCharacterClass.Fighter,
                    ProfessionTranslationID = "PlayerClass.Profession.TempleofArawn",
                    SpecPointMultiplier = 20,
                    AutoTrainSkills = $"{Specs.Slash},{Specs.Flexible}",
                    PrimaryStat = (int)eStat.STR,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.PIE,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Inconnu.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    BaseHP = 760,
                    BaseWeaponSkill = 380,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Disciple,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Disciple,
                    Name = "Disciple",
                    BaseClassID = (byte)eCharacterClass.Disciple,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Inconnu.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Thane,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Thane,
                    Name = "Thane",
                    BaseClassID = (byte)eCharacterClass.Viking,
                    ProfessionTranslationID = "PlayerClass.Profession.HouseofThor",
                    SpecPointMultiplier = 20,
                    PrimaryStat = (int)eStat.STR,
                    SecondaryStat = (int)eStat.PIE,
                    TertiaryStat = (int)eStat.CON,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Frostalf.ID)},{(int)(PlayerRace.Deifrang.ID)},{(int)(PlayerRace.Norseman.ID)},{(int)(PlayerRace.Troll.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Warrior,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Warrior,
                    Name = "Warrior",
                    BaseClassID = (byte)eCharacterClass.Viking,
                    ProfessionTranslationID = "PlayerClass.Profession.HouseofTyr",
                    SpecPointMultiplier = 20,
                    AutoTrainSkills = $"{Specs.Axe},{Specs.Hammer},{Specs.Sword}",
                    PrimaryStat = (int)eStat.STR,
                    SecondaryStat = (int)eStat.CON,
                    TertiaryStat = (int)eStat.DEX,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Deifrang.ID)},{(int)(PlayerRace.Norseman.ID)},{(int)(PlayerRace.Troll.ID)},{(int)(PlayerRace.Valkyn.ID)}",
                    BaseHP = 880,
                    BaseWeaponSkill = 460,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Shadowblade,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Shadowblade,
                    Name = "Shadowblade",
                    BaseClassID = (byte)eCharacterClass.MidgardRogue,
                    ProfessionTranslationID = "PlayerClass.Profession.Loki",
                    SpecPointMultiplier = 22,
                    AutoTrainSkills = $"{Specs.Stealth}",
                    PrimaryStat = (int)eStat.DEX,
                    SecondaryStat = (int)eStat.QUI,
                    TertiaryStat = (int)eStat.STR,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Frostalf.ID)},{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Norseman.ID)},{(int)(PlayerRace.Valkyn.ID)}",
                    CanUseLeftHandedWeapon = true,
                    BaseHP = 760,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Skald,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Skald,
                    Name = "Skald",
                    BaseClassID = (byte)eCharacterClass.Viking,
                    ProfessionTranslationID = "PlayerClass.Profession.HouseofBragi",
                    SpecPointMultiplier = 15,
                    PrimaryStat = (int)eStat.CHR,
                    SecondaryStat = (int)eStat.STR,
                    TertiaryStat = (int)eStat.CON,
                    ManaStat = (int)eStat.CHR,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Norseman.ID)},{(int)(PlayerRace.Troll.ID)}",
                    BaseHP = 760,
                    BaseWeaponSkill = 380,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Hunter,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Hunter,
                    Name = "Hunter",
                    BaseClassID = (byte)eCharacterClass.MidgardRogue,
                    FemaleName = "Huntress",
                    ProfessionTranslationID = "PlayerClass.Profession.HouseofSkadi",
                    SpecPointMultiplier = 20,
                    AutoTrainSkills = $"{Specs.Archery},{Specs.CompositeBow}",
                    PrimaryStat = (int)eStat.DEX,
                    SecondaryStat = (int)eStat.QUI,
                    TertiaryStat = (int)eStat.STR,
                    ManaStat = (int)eStat.DEX,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Frostalf.ID)},{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Norseman.ID)},{(int)(PlayerRace.Valkyn.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 380,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Healer,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Healer,
                    Name = "Healer",
                    BaseClassID = (byte)eCharacterClass.Seer,
                    ProfessionTranslationID = "PlayerClass.Profession.HouseofEir",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.PIE,
                    SecondaryStat = (int)eStat.CON,
                    TertiaryStat = (int)eStat.STR,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Frostalf.ID)},{(int)(PlayerRace.Norseman.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Spiritmaster,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Spiritmaster,
                    Name = "Spiritmaster",
                    BaseClassID = (byte)eCharacterClass.Mystic,
                    ProfessionTranslationID = "PlayerClass.Profession.HouseofHel",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.PIE,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Frostalf.ID)},{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Norseman.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Shaman,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Shaman,
                    Name = "Shaman",
                    BaseClassID = (byte)eCharacterClass.Seer,
                    ProfessionTranslationID = "PlayerClass.Profession.HouseofYmir",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.PIE,
                    SecondaryStat = (int)eStat.CON,
                    TertiaryStat = (int)eStat.STR,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Frostalf.ID)},{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Troll.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Runemaster,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Runemaster,
                    Name = "Runemaster",
                    BaseClassID = (byte)eCharacterClass.Mystic,
                    ProfessionTranslationID = "PlayerClass.Profession.HouseofOdin",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.PIE,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Frostalf.ID)},{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Norseman.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Bonedancer,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Bonedancer,
                    Name = "Bonedancer",
                    BaseClassID = (byte)eCharacterClass.Mystic,
                    ProfessionTranslationID = "PlayerClass.Profession.HouseofBodgar",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.PIE,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Troll.ID)},{(int)(PlayerRace.Valkyn.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Berserker,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Berserker,
                    Name = "Berserker",
                    BaseClassID = (byte)eCharacterClass.Viking,
                    ProfessionTranslationID = "PlayerClass.Profession.HouseofModi",
                    SpecPointMultiplier = 20,
                    PrimaryStat = (int)eStat.STR,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.CON,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Deifrang.ID)},{(int)(PlayerRace.Norseman.ID)},{(int)(PlayerRace.Troll.ID)},{(int)(PlayerRace.Valkyn.ID)}",
                    CanUseLeftHandedWeapon = true,
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Savage,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Savage,
                    Name = "Savage",
                    BaseClassID = (byte)eCharacterClass.Viking,
                    ProfessionTranslationID = "PlayerClass.Profession.HouseofKelgor",
                    SpecPointMultiplier = 15,
                    PrimaryStat = (int)eStat.DEX,
                    SecondaryStat = (int)eStat.QUI,
                    TertiaryStat = (int)eStat.STR,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Norseman.ID)},{(int)(PlayerRace.Troll.ID)},{(int)(PlayerRace.Valkyn.ID)}",
                    CanUseLeftHandedWeapon = true,
                    BaseHP = 880,
                    BaseWeaponSkill = 400,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Heretic,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Heretic,
                    Name = "Heretic",
                    BaseClassID = (byte)eCharacterClass.Acolyte,
                    ProfessionTranslationID = "PlayerClass.Profession.TempleofArawn",
                    SpecPointMultiplier = 20,
                    PrimaryStat = (int)eStat.PIE,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.CON,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Korazh.ID)},{(int)(PlayerRace.Avalonian.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Inconnu.ID)},{(int)(PlayerRace.Saracen.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Valkyrie,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Valkyrie,
                    Name = "Valkyrie",
                    BaseClassID = (byte)eCharacterClass.Viking,
                    ProfessionTranslationID = "PlayerClass.Profession.HouseofOdin",
                    SpecPointMultiplier = 20,
                    PrimaryStat = (int)eStat.CON,
                    SecondaryStat = (int)eStat.STR,
                    TertiaryStat = (int)eStat.DEX,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Frostalf.ID)},{(int)(PlayerRace.Norseman.ID)},{(int)(PlayerRace.Valkyn.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Viking,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Viking,
                    Name = "Viking",
                    BaseClassID = (byte)eCharacterClass.Viking,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Frostalf.ID)},{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Deifrang.ID)},{(int)(PlayerRace.Norseman.ID)},{(int)(PlayerRace.Troll.ID)},{(int)(PlayerRace.Valkyn.ID)}",
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Mystic,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Mystic,
                    Name = "Mystic",
                    BaseClassID = (byte)eCharacterClass.Mystic,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Frostalf.ID)},{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Norseman.ID)},{(int)(PlayerRace.Troll.ID)},{(int)(PlayerRace.Valkyn.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Seer,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Seer,
                    Name = "Seer",
                    BaseClassID = (byte)eCharacterClass.Seer,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Frostalf.ID)},{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Norseman.ID)},{(int)(PlayerRace.Troll.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.MidgardRogue,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.MidgardRogue,
                    Name = "Rogue",
                    BaseClassID = (byte)eCharacterClass.MidgardRogue,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    EligibleRaces = $"{(int)(PlayerRace.Dwarf.ID)},{(int)(PlayerRace.Frostalf.ID)},{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Norseman.ID)},{(int)(PlayerRace.Valkyn.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Bainshee,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Bainshee,
                    Name = "Bainshee",
                    BaseClassID = (byte)eCharacterClass.Magician,
                    ProfessionTranslationID = "PlayerClass.Profession.PathofAffinity",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.INT,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.CON,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Elf.ID)},{(int)(PlayerRace.Lurikeen.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Eldritch,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Eldritch,
                    Name = "Eldritch",
                    BaseClassID = (byte)eCharacterClass.Magician,
                    ProfessionTranslationID = "PlayerClass.Profession.PathofFocus",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.INT,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Elf.ID)},{(int)(PlayerRace.Lurikeen.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Enchanter,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Enchanter,
                    Name = "Enchanter",
                    BaseClassID = (byte)eCharacterClass.Magician,
                    FemaleName = "Enchantress",
                    ProfessionTranslationID = "PlayerClass.Profession.PathofEssence",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.INT,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Elf.ID)},{(int)(PlayerRace.Lurikeen.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Mentalist,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Mentalist,
                    Name = "Mentalist",
                    BaseClassID = (byte)eCharacterClass.Magician,
                    ProfessionTranslationID = "PlayerClass.Profession.PathofHarmony",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.INT,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Elf.ID)},{(int)(PlayerRace.Lurikeen.ID)},{(int)(PlayerRace.Shar.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Blademaster,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Blademaster,
                    Name = "Blademaster",
                    BaseClassID = (byte)eCharacterClass.Guardian,
                    ProfessionTranslationID = "PlayerClass.Profession.PathofHarmony",
                    SpecPointMultiplier = 20,
                    PrimaryStat = (int)eStat.STR,
                    SecondaryStat = (int)eStat.DEX,
                    TertiaryStat = (int)eStat.CON,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Elf.ID)},{(int)(PlayerRace.Firbolg.ID)},{(int)(PlayerRace.Graoch.ID)},{(int)(PlayerRace.Shar.ID)}",
                    CanUseLeftHandedWeapon = true,
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Hero,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Hero,
                    Name = "Hero",
                    BaseClassID = (byte)eCharacterClass.Guardian,
                    FemaleName = "Heroine",
                    ProfessionTranslationID = "PlayerClass.Profession.PathofFocus",
                    SpecPointMultiplier = 20,
                    PrimaryStat = (int)eStat.STR,
                    SecondaryStat = (int)eStat.CON,
                    TertiaryStat = (int)eStat.DEX,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Firbolg.ID)},{(int)(PlayerRace.Graoch.ID)},{(int)(PlayerRace.Lurikeen.ID)},{(int)(PlayerRace.Shar.ID)},{(int)(PlayerRace.Sylvan.ID)}",
                    BaseHP = 880,
                    BaseWeaponSkill = 440,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Champion,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Champion,
                    Name = "Champion",
                    BaseClassID = (byte)eCharacterClass.Guardian,
                    ProfessionTranslationID = "PlayerClass.Profession.PathofEssence",
                    SpecPointMultiplier = 20,
                    PrimaryStat = (int)eStat.STR,
                    SecondaryStat = (int)eStat.INT,
                    TertiaryStat = (int)eStat.DEX,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Elf.ID)},{(int)(PlayerRace.Graoch.ID)},{(int)(PlayerRace.Lurikeen.ID)},{(int)(PlayerRace.Shar.ID)}",
                    BaseHP = 760,
                    BaseWeaponSkill = 380,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Warden,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Warden,
                    Name = "Warden",
                    BaseClassID = (byte)eCharacterClass.Naturalist,
                    ProfessionTranslationID = "PlayerClass.Profession.PathofFocus",
                    SpecPointMultiplier = 18,
                    PrimaryStat = (int)eStat.EMP,
                    SecondaryStat = (int)eStat.STR,
                    TertiaryStat = (int)eStat.CON,
                    ManaStat = (int)eStat.EMP,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Firbolg.ID)},{(int)(PlayerRace.Graoch.ID)},{(int)(PlayerRace.Sylvan.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Druid,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Druid,
                    Name = "Druid",
                    BaseClassID = (byte)eCharacterClass.Naturalist,
                    ProfessionTranslationID = "PlayerClass.Profession.PathofHarmony",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.EMP,
                    SecondaryStat = (int)eStat.CON,
                    TertiaryStat = (int)eStat.STR,
                    ManaStat = (int)eStat.EMP,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Firbolg.ID)},{(int)(PlayerRace.Sylvan.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 320,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Bard,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Bard,
                    Name = "Bard",
                    BaseClassID = (byte)eCharacterClass.Naturalist,
                    ProfessionTranslationID = "PlayerClass.Profession.PathofEssence",
                    SpecPointMultiplier = 15,
                    PrimaryStat = (int)eStat.CHR,
                    SecondaryStat = (int)eStat.EMP,
                    TertiaryStat = (int)eStat.CON,
                    ManaStat = (int)eStat.CHR,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Firbolg.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Nightshade,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Nightshade,
                    Name = "Nightshade",
                    BaseClassID = (byte)eCharacterClass.Stalker,
                    ProfessionTranslationID = "PlayerClass.Profession.PathofEssence",
                    SpecPointMultiplier = 22,
                    AutoTrainSkills = $"{Specs.Stealth}",
                    PrimaryStat = (int)eStat.DEX,
                    SecondaryStat = (int)eStat.QUI,
                    TertiaryStat = (int)eStat.STR,
                    ManaStat = (int)eStat.DEX,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Elf.ID)},{(int)(PlayerRace.Lurikeen.ID)}",
                    CanUseLeftHandedWeapon = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Ranger,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Ranger,
                    Name = "Ranger",
                    BaseClassID = (byte)eCharacterClass.Stalker,
                    ProfessionTranslationID = "PlayerClass.Profession.PathofFocus",
                    SpecPointMultiplier = 20,
                    AutoTrainSkills = $"{Specs.Archery},{Specs.RecurveBow}",
                    PrimaryStat = (int)eStat.DEX,
                    SecondaryStat = (int)eStat.QUI,
                    TertiaryStat = (int)eStat.STR,
                    ManaStat = (int)eStat.DEX,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Elf.ID)},{(int)(PlayerRace.Lurikeen.ID)},{(int)(PlayerRace.Shar.ID)},{(int)(PlayerRace.Sylvan.ID)}",
                    CanUseLeftHandedWeapon = true,
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Magician,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Magician,
                    Name = "Magician",
                    BaseClassID = (byte)eCharacterClass.Magician,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Elf.ID)},{(int)(PlayerRace.Lurikeen.ID)},{(int)(PlayerRace.Shar.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Guardian,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Guardian,
                    Name = "Guardian",
                    BaseClassID = (byte)eCharacterClass.Guardian,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Elf.ID)},{(int)(PlayerRace.Firbolg.ID)},{(int)(PlayerRace.Graoch.ID)},{(int)(PlayerRace.Lurikeen.ID)},{(int)(PlayerRace.Shar.ID)},{(int)(PlayerRace.Sylvan.ID)}",
                    BaseHP = 880,
                    BaseWeaponSkill = 400,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Naturalist,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Naturalist,
                    Name = "Naturalist",
                    BaseClassID = (byte)eCharacterClass.Naturalist,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    ManaStat = (int)eStat.EMP,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Firbolg.ID)},{(int)(PlayerRace.Sylvan.ID)},{(int)(PlayerRace.Graoch.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.Stalker,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Stalker,
                    Name = "Stalker",
                    BaseClassID = (byte)eCharacterClass.Stalker,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Elf.ID)},{(int)(PlayerRace.Lurikeen.ID)},{(int)(PlayerRace.Shar.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 360,
                    ClassType = (byte)eClassType.PureTank,
                }
            },
            {
                (byte)eCharacterClass.Animist,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Animist,
                    Name = "Animist",
                    BaseClassID = (byte)eCharacterClass.Forester,
                    ProfessionTranslationID = "PlayerClass.Profession.PathofAffinity",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.INT,
                    SecondaryStat = (int)eStat.CON,
                    TertiaryStat = (int)eStat.DEX,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Firbolg.ID)},{(int)(PlayerRace.Sylvan.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Valewalker,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Valewalker,
                    Name = "Valewalker",
                    BaseClassID = (byte)eCharacterClass.Forester,
                    ProfessionTranslationID = "PlayerClass.Profession.PathofAffinity",
                    SpecPointMultiplier = 15,
                    PrimaryStat = (int)eStat.STR,
                    SecondaryStat = (int)eStat.INT,
                    TertiaryStat = (int)eStat.CON,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Firbolg.ID)},{(int)(PlayerRace.Sylvan.ID)}",
                    BaseHP = 720,
                    BaseWeaponSkill = 400,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Forester,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Forester,
                    Name = "Forester",
                    BaseClassID = (byte)eCharacterClass.Forester,
                    ProfessionTranslationID = "",
                    SpecPointMultiplier = 10,
                    ManaStat = (int)eStat.INT,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Firbolg.ID)},{(int)(PlayerRace.Sylvan.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Vampiir,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Vampiir,
                    Name = "Vampiir",
                    BaseClassID = (byte)eCharacterClass.Stalker,
                    ProfessionTranslationID = "PlayerClass.Profession.PathofAffinity",
                    SpecPointMultiplier = 15,
                    PrimaryStat = (int)eStat.CON,
                    SecondaryStat = (int)eStat.STR,
                    TertiaryStat = (int)eStat.DEX,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Lurikeen.ID)},{(int)(PlayerRace.Shar.ID)}",
                    BaseHP = 878,
                    BaseWeaponSkill = 440,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.Warlock,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.Warlock,
                    Name = "Warlock",
                    BaseClassID = (byte)eCharacterClass.Mystic,
                    ProfessionTranslationID = "PlayerClass.Profession.HouseofHel",
                    SpecPointMultiplier = 10,
                    PrimaryStat = (int)eStat.PIE,
                    SecondaryStat = (int)eStat.CON,
                    TertiaryStat = (int)eStat.DEX,
                    ManaStat = (int)eStat.PIE,
                    EligibleRaces = $"{(int)(PlayerRace.Frostalf.ID)},{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Norseman.ID)}",
                    BaseHP = 560,
                    BaseWeaponSkill = 280,
                    ClassType = (byte)eClassType.ListCaster,
                }
            },
            {
                (byte)eCharacterClass.MaulerAlb,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.MaulerAlb,
                    Name = "Mauler",
                    BaseClassID = (byte)eCharacterClass.Fighter,
                    ProfessionTranslationID = "PlayerClass.Profession.TempleofIronFist",
                    SpecPointMultiplier = 15,
                    PrimaryStat = (int)eStat.STR,
                    SecondaryStat = (int)eStat.CON,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.STR,
                    EligibleRaces = $"{(int)(PlayerRace.Korazh.ID)},{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Inconnu.ID)}",
                    CanUseLeftHandedWeapon = true,
                    BaseHP = 600,
                    BaseWeaponSkill = 440,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.MaulerMid,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.MaulerMid,
                    Name = "Mauler",
                    BaseClassID = (byte)eCharacterClass.Viking,
                    ProfessionTranslationID = "PlayerClass.Profession.TempleofIronFist",
                    SpecPointMultiplier = 15,
                    PrimaryStat = (int)eStat.STR,
                    SecondaryStat = (int)eStat.CON,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.STR,
                    EligibleRaces = $"{(int)(PlayerRace.Kobold.ID)},{(int)(PlayerRace.Deifrang.ID)},{(int)(PlayerRace.Norseman.ID)}",
                    CanUseLeftHandedWeapon = true,
                    BaseHP = 600,
                    BaseWeaponSkill = 440,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
            {
                (byte)eCharacterClass.MaulerHib,
                new DBCharacterClass()
                {
                    ID = (byte)eCharacterClass.MaulerHib,
                    Name = "Mauler",
                    BaseClassID = (byte)eCharacterClass.Guardian,
                    ProfessionTranslationID = "PlayerClass.Profession.TempleofIronFist",
                    SpecPointMultiplier = 15,
                    PrimaryStat = (int)eStat.STR,
                    SecondaryStat = (int)eStat.CON,
                    TertiaryStat = (int)eStat.QUI,
                    ManaStat = (int)eStat.STR,
                    EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Graoch.ID)},{(int)(PlayerRace.Lurikeen.ID)}",
                    CanUseLeftHandedWeapon = true,
                    BaseHP = 600,
                    BaseWeaponSkill = 440,
                    ClassType = (byte)eClassType.Hybrid,
                }
            },
        };
    }
}
