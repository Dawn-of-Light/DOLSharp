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

namespace DOL.GS.Spells
{
    using System;
    using Database;
    using Events;
    using DOL.GS.PacketHandler;

    [SpellHandler("BeltOfSun")]
    public class BeltOfSun : SummonItemSpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ItemTemplate m_SunSlash;
        private ItemTemplate m_SunThrust;
        private ItemTemplate m_SunTwoHanded;
        private ItemTemplate m_SunCrush;
        private ItemTemplate m_SunFlexScytheClaw;
        private ItemTemplate m_SunAxe;
        private ItemTemplate m_SunLeftAxe;
        private ItemTemplate m_Sun2HAxe;
        private ItemTemplate m_Sun2HCrush;
        private ItemTemplate m_SunBow;
        private ItemTemplate m_SunStaff;
        private ItemTemplate m_SunPolearmSpear;
        private ItemTemplate m_SunMFist;
        private ItemTemplate m_SunMStaff;

        public BeltOfSun(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            if (caster.CurrentRegion.IsNightTime)
            {
                MessageToCaster("The powers of the Belt of Sun, can only be Summon under the Sun light!", eChatType.CT_SpellResisted);
                return;
            }

            GamePlayer player = caster as GamePlayer;

            if (player.CharacterClass.ID == (int)eCharacterClass.Armsman)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded") ?? TwoHanded;
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_SunPolearmSpear = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Polearm") ?? Polearm;
                items.Add(GameInventoryItem.Create(m_SunPolearmSpear));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Friar)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Staff") ?? Staff;
                items.Add(GameInventoryItem.Create(m_SunStaff));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Heretic)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunFlexScytheClaw = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Flex") ?? Flex;
                items.Add(GameInventoryItem.Create(m_SunFlexScytheClaw));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Infiltrator)
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Mercenary)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Minstrel)
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Paladin)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded") ?? TwoHanded;
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Reaver)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunFlexScytheClaw = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Flex") ?? Flex;
                items.Add(GameInventoryItem.Create(m_SunFlexScytheClaw));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Scout)
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunBow = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow") ?? Bow;
                items.Add(GameInventoryItem.Create(m_SunBow));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.MaulerAlb)
            {
                m_SunMFist = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MFist") ?? MFist;
                items.Add(GameInventoryItem.Create(m_SunMFist));

                m_SunMStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MStaff") ?? MStaff;
                items.Add(GameInventoryItem.Create(m_SunMStaff));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Berserker)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HCrush") ?? THCrushM;
                items.Add(GameInventoryItem.Create(m_Sun2HCrush));

                m_Sun2HAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HAxe") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Hunter)
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunPolearmSpear = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Trust") ?? SpearM; // Spear
                items.Add(GameInventoryItem.Create(m_SunPolearmSpear));

                m_SunBow = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow") ?? BowM; //
                items.Add(GameInventoryItem.Create(m_SunBow));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Savage)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Claw") ?? Claw; //
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Shadowblade)
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunLeftAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_LeftAxe") ?? LeftAxe; //
                items.Add(GameInventoryItem.Create(m_SunLeftAxe));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HAxe") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Skald)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HCrush") ?? THCrushM;
                items.Add(GameInventoryItem.Create(m_Sun2HCrush));

                m_Sun2HAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HAxe") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Thane)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HCrush") ?? THCrushM;
                items.Add(GameInventoryItem.Create(m_Sun2HCrush));

                m_Sun2HAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HAxe") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Thane)
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_SunPolearmSpear = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Trust") ?? SpearM; // Spear
                items.Add(GameInventoryItem.Create(m_SunPolearmSpear));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Warrior)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HCrush") ?? THCrushM;
                items.Add(GameInventoryItem.Create(m_Sun2HCrush));

                m_Sun2HAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HAxe") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.MaulerMid)
            {
                m_SunMFist = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MFist") ?? MFist;
                items.Add(GameInventoryItem.Create(m_SunMFist));

                m_SunMStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MStaff") ?? MStaff;
                items.Add(GameInventoryItem.Create(m_SunMStaff));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Bard)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Blademaster)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust") ?? ThrustH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Champion)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust") ?? ThrustH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? TwoHandedH; // LargeWeapon
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Hero)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust") ?? ThrustH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? TwoHandedH; // LargeWeapon
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_SunPolearmSpear = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Spear") ?? SpearH; // Spear
                items.Add(GameInventoryItem.Create(m_SunPolearmSpear));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Nightshade)
            {
                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust") ?? ThrustH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Ranger)
            {
                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust") ?? ThrustH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunBow = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow") ?? BowH; //
                items.Add(GameInventoryItem.Create(m_SunBow));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Valewalker)
            {
                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_FlexScythe") ?? Scythe;
                items.Add(GameInventoryItem.Create(m_SunFlexScytheClaw));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Valewalker)
            {
                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust") ?? ThrustH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Warden)
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.MaulerHib)
            {
                m_SunMFist = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MFist") ?? MFist;
                items.Add(GameInventoryItem.Create(m_SunMFist));

                m_SunMStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MStaff") ?? MStaff;
                items.Add(GameInventoryItem.Create(m_SunMStaff));
            }
            else
            {
                player.Out.SendMessage(string.Empty + player.CharacterClass.Name + "'s cant Summon Light!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        private ItemTemplate Crush
        {
            get
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush");
                if (m_SunCrush == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Crush, loading it ...");
                    }

                    m_SunCrush = new ItemTemplate
                    {
                        Id_nb = "Sun_Crush",
                        Name = "Sun Mace",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 2,
                        Item_Type = 11,
                        Hand = 2,
                        Model = 1916,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 25,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunCrush;
            }
        }

        private ItemTemplate Slash
        {
            get
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash");
                if (m_SunSlash == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Slash, loading it ...");
                    }

                    m_SunSlash = new ItemTemplate
                    {
                        Id_nb = "Sun_Slash",
                        Name = "Sun Sword",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 3,
                        Item_Type = 11,
                        Hand = 2,
                        Model = 1948,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 44,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunSlash;
            }
        }

        private ItemTemplate Thrust
        {
            get
            {
                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust");
                if (m_SunThrust == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Thrust, loading it ...");
                    }

                    m_SunThrust = new ItemTemplate
                    {
                        Id_nb = "Sun_Thrust",
                        Name = "Sun Sword",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 4,
                        Item_Type = 11,
                        Hand = 1,
                        Model = 1948,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 50,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunThrust;
            }
        }

        private ItemTemplate Flex
        {
            get
            {
                m_SunFlexScytheClaw = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Flex");
                if (m_SunFlexScytheClaw == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Flex, loading it ...");
                    }

                    m_SunFlexScytheClaw = new ItemTemplate
                    {
                        Id_nb = "Sun_Flex",
                        Name = "Sun Spiked Flail",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 24,
                        Item_Type = 10,
                        Hand = 0,
                        Model = 1924,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 33,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunFlexScytheClaw;
            }
        }

        private ItemTemplate Polearm
        {
            get
            {
                m_SunPolearmSpear = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Polearm");
                if (m_SunPolearmSpear == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Polearm, loading it ...");
                    }

                    m_SunPolearmSpear = new ItemTemplate
                    {
                        Id_nb = "Sun_Polearm",
                        Name = "Sun Glaive",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 52,
                        Type_Damage = 0,
                        Object_Type = 7,
                        Item_Type = 12,
                        Hand = 1,
                        Model = 1936,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 41,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunPolearmSpear;
            }
        }

        private ItemTemplate TwoHanded
        {
            get
            {
                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded");
                if (m_SunTwoHanded == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_TwoHanded, loading it ...");
                    }

                    m_SunTwoHanded = new ItemTemplate
                    {
                        Id_nb = "Sun_TwoHanded",
                        Name = "Sun Twohanded Sword",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 52,
                        Type_Damage = 0,
                        Object_Type = 6,
                        Item_Type = 12,
                        Hand = 1,
                        Model = 1904,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 20,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunTwoHanded;
            }
        }

        private ItemTemplate Bow
        {
            get
            {
                m_SunBow = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow");
                if (m_SunBow == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Bow, loading it ...");
                    }

                    m_SunBow = new ItemTemplate
                    {
                        Id_nb = "Sun_Bow",
                        Name = "Sun Bow",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 48,
                        Type_Damage = 0,
                        Object_Type = 9,
                        Item_Type = 13,
                        Hand = 1,
                        Model = 1912,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 36,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunBow;
            }
        }

        private ItemTemplate Staff
        {
            get
            {
                m_SunStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Staff");
                if (m_SunStaff == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Staff, loading it ...");
                    }

                    m_SunStaff = new ItemTemplate
                    {
                        Id_nb = "Sun_Staff",
                        Name = "Sun QuarterStaff",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 42,
                        Type_Damage = 0,
                        Object_Type = 8,
                        Item_Type = 12,
                        Hand = 1,
                        Model = 1952,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 48,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunStaff;
            }
        }

        private ItemTemplate MStaff
        {
            get
            {
                m_SunMStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MStaff");
                if (m_SunMStaff == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_MStaff, loading it ...");
                    }

                    m_SunMStaff = new ItemTemplate
                    {
                        Id_nb = "Sun_MStaff",
                        Name = "Sun Maulers QuarterStaff",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 42,
                        Type_Damage = 0,
                        Object_Type = 28,
                        Item_Type = 12,
                        Hand = 1,
                        Model = 1952,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 109,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunMStaff;
            }
        }

        private ItemTemplate MFist
        {
            get
            {
                m_SunMFist = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MFist");
                if (m_SunMFist == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_MFist, loading it ...");
                    }

                    m_SunMFist = new ItemTemplate
                    {
                        Id_nb = "Sun_MFist",
                        Name = "Sun MFist",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 42,
                        Type_Damage = 0,
                        Object_Type = 27,
                        Item_Type = 11,
                        Hand = 2,
                        Model = 2028,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 110,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunMFist;
            }
        }

        private ItemTemplate CrushM
        {
            get
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush");
                if (m_SunCrush == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Crush, loading it ...");
                    }

                    m_SunCrush = new ItemTemplate
                    {
                        Id_nb = "Sun_Crush",
                        Name = "Sun Warhammer",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 12,
                        Item_Type = 10,
                        Hand = 2,
                        Model = 2044,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 53,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunCrush;
            }
        }

        private ItemTemplate SlashM
        {
            get
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash");
                if (m_SunSlash == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Slash, loading it ...");
                    }

                    m_SunSlash = new ItemTemplate
                    {
                        Id_nb = "Sun_Slash",
                        Name = "Sun Sword",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 11,
                        Item_Type = 10,
                        Hand = 2,
                        Model = 2036,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 52,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunSlash;
            }
        }

        private ItemTemplate Axe
        {
            get
            {
                m_SunAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe");
                if (m_SunAxe == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Axe, loading it ...");
                    }

                    m_SunAxe = new ItemTemplate
                    {
                        Id_nb = "Sun_Axe",
                        Name = "Sun Axe",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 13,
                        Item_Type = 10,
                        Hand = 0,
                        Model = 2032,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 54,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunAxe;
            }
        }

        private ItemTemplate LeftAxe
        {
            get
            {
                m_SunLeftAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_LeftAxe");
                if (m_SunLeftAxe == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_LeftAxe, loading it ...");
                    }

                    m_SunLeftAxe = new ItemTemplate
                    {
                        Id_nb = "Sun_LeftAxe",
                        Name = "Sun LeftAxe",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 17,
                        Item_Type = 11,
                        Hand = 2,
                        Model = 2032,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 55,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunLeftAxe;
            }
        }

        private ItemTemplate Claw
        {
            get
            {
                m_SunFlexScytheClaw = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Claw");
                if (m_SunFlexScytheClaw == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Claw, loading it ...");
                    }

                    m_SunFlexScytheClaw = new ItemTemplate
                    {
                        Id_nb = "Sun_Claw",
                        Name = "Sun Claw",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 25,
                        Item_Type = 11,
                        Hand = 2,
                        Model = 2028,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 92,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunFlexScytheClaw;
            }
        }

        private ItemTemplate SpearM
        {
            get
            {
                m_SunPolearmSpear = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Spear");
                if (m_SunPolearmSpear == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Spear, loading it ...");
                    }

                    m_SunPolearmSpear = new ItemTemplate
                    {
                        Id_nb = "Sun_Spear",
                        Name = "Sun Spear",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 48,
                        Type_Damage = 0,
                        Object_Type = 14,
                        Item_Type = 12,
                        Hand = 1,
                        Model = 2048,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 56,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunPolearmSpear;
            }
        }

        private ItemTemplate TwoHandedM
        {
            get
            {
                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded");
                if (m_SunTwoHanded == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_TwoHanded, loading it ...");
                    }

                    m_SunTwoHanded = new ItemTemplate
                    {
                        Id_nb = "Sun_TwoHanded",
                        Name = "Sun Greater Sword",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 52,
                        Type_Damage = 0,
                        Object_Type = 11,
                        Item_Type = 12,
                        Hand = 1,
                        Model = 2060,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 52,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunTwoHanded;
            }
        }

        private ItemTemplate BowM
        {
            get
            {
                m_SunBow = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow");
                if (m_SunBow == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Bow, loading it ...");
                    }

                    m_SunBow = new ItemTemplate
                    {
                        Id_nb = "Sun_Bow",
                        Name = "Sun Bow",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 48,
                        Type_Damage = 0,
                        Object_Type = 15,
                        Item_Type = 13,
                        Hand = 1,
                        Model = 2064,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 68,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunBow;
            }
        }

        private ItemTemplate THCrushM
        {
            get
            {
                m_Sun2HCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HCrush");
                if (m_Sun2HCrush == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_2HCrush, loading it ...");
                    }

                    m_Sun2HCrush = new ItemTemplate
                    {
                        Id_nb = "Sun_2HCrush",
                        Name = "Sun Greater Warhammer",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 52,
                        Type_Damage = 0,
                        Object_Type = 12,
                        Item_Type = 12,
                        Hand = 1,
                        Model = 2056,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 53,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_Sun2HCrush;
            }
        }

        private ItemTemplate THAxe
        {
            get
            {
                m_Sun2HAxe = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HAxe");
                if (m_Sun2HAxe == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_2HAxe, loading it ...");
                    }

                    m_Sun2HAxe = new ItemTemplate
                    {
                        Id_nb = "Sun_2HAxe",
                        Name = "Sun Greater Axe",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 52,
                        Type_Damage = 0,
                        Object_Type = 13,
                        Item_Type = 12,
                        Hand = 1,
                        Model = 2052,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 54,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_Sun2HAxe;
            }
        }

        private ItemTemplate CrushH
        {
            get
            {
                m_SunCrush = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush");
                if (m_SunCrush == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Crush, loading it ...");
                    }

                    m_SunCrush = new ItemTemplate
                    {
                        Id_nb = "Sun_Crush",
                        Name = "Sun Hammer",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 20,
                        Item_Type = 11,
                        Hand = 2,
                        Model = 1988,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 73,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunCrush;
            }
        }

        private ItemTemplate SlashH
        {
            get
            {
                m_SunSlash = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash");
                if (m_SunSlash == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Slash, loading it ...");
                    }

                    m_SunSlash = new ItemTemplate
                    {
                        Id_nb = "Sun_Slash",
                        Name = "Sun Blade",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 19,
                        Item_Type = 11,
                        Hand = 2,
                        Model = 1948,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 72,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunSlash;
            }
        }

        private ItemTemplate ThrustH
        {
            get
            {
                m_SunThrust = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust");
                if (m_SunThrust == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Thrust, loading it ...");
                    }

                    m_SunThrust = new ItemTemplate
                    {
                        Id_nb = "Sun_Thrust",
                        Name = "Sun Sword",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 21,
                        Item_Type = 11,
                        Hand = 2,
                        Model = 1948,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 74,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunThrust;
            }
        }

        private ItemTemplate Scythe
        {
            get
            {
                m_SunFlexScytheClaw = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Scythe");
                if (m_SunFlexScytheClaw == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Scythe, loading it ...");
                    }

                    m_SunFlexScytheClaw = new ItemTemplate
                    {
                        Id_nb = "Sun_Scythe",
                        Name = "Sun Scythe",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Hand = 1,
                        Type_Damage = 0,
                        Object_Type = 26,
                        Item_Type = 12,
                        Model = 2004,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 90,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunFlexScytheClaw;
            }
        }

        private ItemTemplate SpearH
        {
            get
            {
                m_SunPolearmSpear = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Spear");
                if (m_SunPolearmSpear == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Spear, loading it ...");
                    }

                    m_SunPolearmSpear = new ItemTemplate
                    {
                        Id_nb = "Sun_Spear",
                        Name = "Sun Spear",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 52,
                        Type_Damage = 0,
                        Object_Type = 23,
                        Item_Type = 12,
                        Hand = 1,
                        Model = 2008,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 82,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunPolearmSpear;
            }
        }

        private ItemTemplate TwoHandedH
        {
            get
            {
                m_SunTwoHanded = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded");
                if (m_SunTwoHanded == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_TwoHanded, loading it ...");
                    }

                    m_SunTwoHanded = new ItemTemplate
                    {
                        Id_nb = "Sun_TwoHanded",
                        Name = "Sun Large Weapon",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 52,
                        Type_Damage = 0,
                        Object_Type = 22,
                        Item_Type = 12,
                        Hand = 1,
                        Model = 1984,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 75,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunTwoHanded;
            }
        }

        private ItemTemplate BowH
        {
            get
            {
                m_SunBow = GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow");
                if (m_SunBow == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Sun_Bow, loading it ...");
                    }

                    m_SunBow = new ItemTemplate
                    {
                        Id_nb = "Sun_Bow",
                        Name = "Sun Bow",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 48,
                        Type_Damage = 0,
                        Object_Type = 18,
                        Item_Type = 13,
                        Hand = 1,
                        Model = 1996,
                        Bonus1 = 6,
                        Bonus2 = 27,
                        Bonus3 = 2,
                        Bonus4 = 2,
                        Bonus5 = 2,
                        Bonus1Type = 83,
                        Bonus2Type = 1,
                        Bonus3Type = 173,
                        Bonus4Type = 200,
                        Bonus5Type = 155,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 65513
                    };
                }

                return m_SunBow;
            }
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            GameEventMgr.AddHandler(Caster, GamePlayerEvent.Released, OnPlayerReleased);
            GameEventMgr.AddHandler(Caster, GamePlayerEvent.Quit, OnPlayerLeft);
        }

        private static void OnPlayerReleased(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GamePlayer))
            {
                return;
            }

            GamePlayer player = (GamePlayer) sender;

            lock (player.Inventory)
            {
                var items = player.Inventory.GetItemRange(eInventorySlot.MinEquipable, eInventorySlot.LastBackpack);
                foreach (InventoryItem invItem in items)
                {
                    if (player.CurrentRegion.IsNightTime)
                    {

                        if (invItem.Id_nb.Equals("Sun_Crush"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_Slash"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_Thrust"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_Flex"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_TwoHanded"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_Polearm"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_Bow"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_Staff"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_MFist"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_MStaff"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_Axe"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_LeftAxe"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_Claw"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_2HCrush"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_2HAxe"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_MStaff"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_FlexScythe"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        if (invItem.Id_nb.Equals("Sun_Spear"))
                        {
                            player.Inventory.RemoveItem(invItem);
                        }

                        player.Out.SendMessage("The Power of Belt of Sun, has left you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                }
            }

            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Released, OnPlayerReleased);
        }

        private static void OnPlayerLeft(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GamePlayer))
            {
                return;
            }

            GamePlayer player = (GamePlayer) sender;
            lock (player.Inventory)
            {
                var items = player.Inventory.GetItemRange(eInventorySlot.MinEquipable, eInventorySlot.LastBackpack);
                foreach (InventoryItem invItem in items)
                {

                    if (invItem.Id_nb.Equals("Sun_Crush"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_Slash"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_Thrust"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_Flex"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_TwoHanded"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_Polearm"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_Bow"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_Staff"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_MFist"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_MStaff"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_Axe"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_LeftAxe"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_Claw"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_2HCrush"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_2HAxe"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_MStaff"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_FlexScythe"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Sun_Spear"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }
                }
            }

            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Quit, OnPlayerLeft);
        }
    }
}