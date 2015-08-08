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
    using DOL.GS.Utils;
    using System.Collections.Generic;

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

            #region Alb
            if (player.CharacterClass.ID == (int)eCharacterClass.Armsman)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Thrust'") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunTwoHanded = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_TwoHanded'") ?? TwoHanded;
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_SunPolearmSpear = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Polearm'") ?? Polearm;
                items.Add(GameInventoryItem.Create(m_SunPolearmSpear));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Friar)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Staff'") ?? Staff;
                items.Add(GameInventoryItem.Create(m_SunStaff));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Heretic)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunFlexScytheClaw = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Flex'") ?? Flex;
                items.Add(GameInventoryItem.Create(m_SunFlexScytheClaw));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Infiltrator)
            {
                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Thrust'") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Mercenary)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Thrust'") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Minstrel)
            {
                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Thrust'") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Paladin)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Thrust'") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunTwoHanded = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_TwoHanded'") ?? TwoHanded;
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Reaver)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? Crush;
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Thrust'") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunFlexScytheClaw = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Flex'") ?? Flex;
                items.Add(GameInventoryItem.Create(m_SunFlexScytheClaw));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Scout)
            {
                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? Slash;
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunThrust = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Thrust'") ?? Thrust;
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunBow = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Bow'") ?? Bow;
                items.Add(GameInventoryItem.Create(m_SunBow));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.MaulerAlb)
            {
                m_SunMFist = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_MFist'") ?? MFist;
                items.Add(GameInventoryItem.Create(m_SunMFist));

                m_SunMStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_MStaff'") ?? MStaff;
                items.Add(GameInventoryItem.Create(m_SunMStaff));
                return;
            }
            #endregion Alb

            #region Mid
            if (player.CharacterClass.ID == (int)eCharacterClass.Berserker)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Axe'") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunTwoHanded = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_TwoHanded'") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_2HCrush'") ?? THCrushM;
                items.Add(GameInventoryItem.Create(m_Sun2HCrush));

                m_Sun2HAxe = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_2HAxe'") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Hunter)
            {
                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunPolearmSpear = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Trust'") ?? SpearM; // Spear
                items.Add(GameInventoryItem.Create(m_SunPolearmSpear));

                m_SunBow = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Bow'") ?? BowM; //
                items.Add(GameInventoryItem.Create(m_SunBow));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Savage)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Axe'") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunThrust = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Claw'") ?? Claw; //
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Shadowblade)
            {
                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Axe'") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunLeftAxe = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_LeftAxe'") ?? LeftAxe; //
                items.Add(GameInventoryItem.Create(m_SunLeftAxe));

                m_SunTwoHanded = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_TwoHanded'") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HAxe = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_2HAxe'") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Skald)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Axe'") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunTwoHanded = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_TwoHanded'") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_2HCrush'") ?? THCrushM;
                items.Add(GameInventoryItem.Create(m_Sun2HCrush));

                m_Sun2HAxe = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_2HAxe'") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Thane)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Axe'") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunTwoHanded = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_TwoHanded'") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_2HCrush'") ?? THCrushM;
                items.Add(GameInventoryItem.Create(m_Sun2HCrush));

                m_Sun2HAxe = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_2HAxe'") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Thane)
            {
                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunTwoHanded = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_TwoHanded'") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_SunPolearmSpear = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Trust'") ?? SpearM; // Spear
                items.Add(GameInventoryItem.Create(m_SunPolearmSpear));
                return;
            }


            if (player.CharacterClass.ID == (int)eCharacterClass.Warrior)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? CrushM; //
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunSlash = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashM; //
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunAxe = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Axe'") ?? Axe; //
                items.Add(GameInventoryItem.Create(m_SunAxe));

                m_SunTwoHanded = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_TwoHanded'") ?? TwoHandedM; // 2handed Sword
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_Sun2HCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_2HCrush'") ?? THCrushM;
                items.Add(GameInventoryItem.Create(m_Sun2HCrush));

                m_Sun2HAxe = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_2HAxe'") ?? THAxe;
                items.Add(GameInventoryItem.Create(m_Sun2HAxe));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.MaulerMid)
            {
                m_SunMFist = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_MFist'") ?? MFist;
                items.Add(GameInventoryItem.Create(m_SunMFist));

                m_SunMStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_MStaff'") ?? MStaff;
                items.Add(GameInventoryItem.Create(m_SunMStaff));
                return;
            }

            #endregion Mid

            #region Hib
            if (player.CharacterClass.ID == (int)eCharacterClass.Bard)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Blademaster)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Thrust'") ?? ThrustH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Champion)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Thrust'") ?? ThrustH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? TwoHandedH; // LargeWeapon
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Hero)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Thrust'") ?? ThrustH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? TwoHandedH; // LargeWeapon
                items.Add(GameInventoryItem.Create(m_SunTwoHanded));

                m_SunPolearmSpear = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Spear'") ?? SpearH; // Spear
                items.Add(GameInventoryItem.Create(m_SunPolearmSpear));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Nightshade)
            {
                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Thrust'") ?? ThrustH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Ranger)
            {
                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));

                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Thrust'") ?? ThrustH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunThrust));

                m_SunBow = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Bow'") ?? BowH; //
                items.Add(GameInventoryItem.Create(m_SunBow));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Valewalker)
            {
                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_FlexScythe'") ?? Scythe;
                items.Add(GameInventoryItem.Create(m_SunFlexScytheClaw));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Valewalker)
            {
                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Thrust'") ?? ThrustH; // Piercing
                items.Add(GameInventoryItem.Create(m_SunThrust));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.Warden)
            {
                m_SunCrush = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Crush'") ?? CrushH; // Blunt
                items.Add(GameInventoryItem.Create(m_SunCrush));

                m_SunStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_Slash'") ?? SlashH; // Blades
                items.Add(GameInventoryItem.Create(m_SunSlash));
                return;
            }

            if (player.CharacterClass.ID == (int)eCharacterClass.MaulerHib)
            {
                m_SunMFist = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_MFist'") ?? MFist;
                items.Add(GameInventoryItem.Create(m_SunMFist));

                m_SunMStaff = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Sun_MStaff'") ?? MStaff;
                items.Add(GameInventoryItem.Create(m_SunMStaff));
                return;
            }

            else
            {
                player.Out.SendMessage("" + player.CharacterClass.Name + "'s cant Summon Light!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
        }
            #endregion Hib

        #region Sun Albion Weapons
        private ItemTemplate Crush
        {
            get
            {
                m_SunCrush = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush");
                if (m_SunCrush == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Crush, loading it ...");
                    m_SunCrush = new ItemTemplate();
                    m_SunCrush.Id_nb = "Sun_Crush";
                    m_SunCrush.Name = "Sun Mace";
                    m_SunCrush.Level = 50;
                    m_SunCrush.Durability = 50000;
                    m_SunCrush.MaxDurability = 50000;
                    m_SunCrush.Condition = 50000;
                    m_SunCrush.MaxCondition = 50000;
                    m_SunCrush.Quality = 100;
                    m_SunCrush.DPS_AF = 150;
                    m_SunCrush.SPD_ABS = 35;
                    m_SunCrush.Type_Damage = 0;
                    m_SunCrush.Object_Type = 2;
                    m_SunCrush.Item_Type = 11;
                    m_SunCrush.Hand = 2;
                    m_SunCrush.Model = 1916;
                    m_SunCrush.Bonus1 = 6;
                    m_SunCrush.Bonus2 = 27;
                    m_SunCrush.Bonus3 = 2;
                    m_SunCrush.Bonus4 = 2;
                    m_SunCrush.Bonus5 = 2;
                    m_SunCrush.Bonus1Type = 25;
                    m_SunCrush.Bonus2Type = 1;
                    m_SunCrush.Bonus3Type = 173;
                    m_SunCrush.Bonus4Type = 200;
                    m_SunCrush.Bonus5Type = 155;
                    m_SunCrush.IsPickable = false;
                    m_SunCrush.IsDropable = false;
                    m_SunCrush.CanDropAsLoot = false;
                    m_SunCrush.IsTradable = false;
                    m_SunCrush.MaxCount = 1;
                    m_SunCrush.PackSize = 1;
                    m_SunCrush.ProcSpellID = 65513;

                }
                return m_SunCrush;
            }
        }

        private ItemTemplate Slash
        {
            get
            {
                m_SunSlash = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash");
                if (m_SunSlash == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Slash, loading it ...");
                    m_SunSlash = new ItemTemplate();
                    m_SunSlash.Id_nb = "Sun_Slash";
                    m_SunSlash.Name = "Sun Sword";
                    m_SunSlash.Level = 50;
                    m_SunSlash.Durability = 50000;
                    m_SunSlash.MaxDurability = 50000;
                    m_SunSlash.Condition = 50000;
                    m_SunSlash.MaxCondition = 50000;
                    m_SunSlash.Quality = 100;
                    m_SunSlash.DPS_AF = 150;
                    m_SunSlash.SPD_ABS = 35;
                    m_SunSlash.Type_Damage = 0;
                    m_SunSlash.Object_Type = 3;
                    m_SunSlash.Item_Type = 11;
                    m_SunSlash.Hand = 2;
                    m_SunSlash.Model = 1948;
                    m_SunSlash.Bonus1 = 6;
                    m_SunSlash.Bonus2 = 27;
                    m_SunSlash.Bonus3 = 2;
                    m_SunSlash.Bonus4 = 2;
                    m_SunSlash.Bonus5 = 2;
                    m_SunSlash.Bonus1Type = 44;
                    m_SunSlash.Bonus2Type = 1;
                    m_SunSlash.Bonus3Type = 173;
                    m_SunSlash.Bonus4Type = 200;
                    m_SunSlash.Bonus5Type = 155;
                    m_SunSlash.IsPickable = false;
                    m_SunSlash.IsDropable = false;
                    m_SunSlash.CanDropAsLoot = false;
                    m_SunSlash.IsTradable = false;
                    m_SunSlash.MaxCount = 1;
                    m_SunSlash.PackSize = 1;
                    m_SunSlash.ProcSpellID = 65513;

                }
                return m_SunSlash;
            }
        }

        private ItemTemplate Thrust
        {
            get
            {
                m_SunThrust = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust");
                if (m_SunThrust == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Thrust, loading it ...");
                    m_SunThrust = new ItemTemplate();
                    m_SunThrust.Id_nb = "Sun_Thrust";
                    m_SunThrust.Name = "Sun Sword";
                    m_SunThrust.Level = 50;
                    m_SunThrust.Durability = 50000;
                    m_SunThrust.MaxDurability = 50000;
                    m_SunThrust.Condition = 50000;
                    m_SunThrust.MaxCondition = 50000;
                    m_SunThrust.Quality = 100;
                    m_SunThrust.DPS_AF = 150;
                    m_SunThrust.SPD_ABS = 35;
                    m_SunThrust.Type_Damage = 0;
                    m_SunThrust.Object_Type = 4;
                    m_SunThrust.Item_Type = 11;
                    m_SunThrust.Hand = 1;
                    m_SunThrust.Model = 1948;
                    m_SunThrust.Bonus1 = 6;
                    m_SunThrust.Bonus2 = 27;
                    m_SunThrust.Bonus3 = 2;
                    m_SunThrust.Bonus4 = 2;
                    m_SunThrust.Bonus5 = 2;
                    m_SunThrust.Bonus1Type = 50;
                    m_SunThrust.Bonus2Type = 1;
                    m_SunThrust.Bonus3Type = 173;
                    m_SunThrust.Bonus4Type = 200;
                    m_SunThrust.Bonus5Type = 155;
                    m_SunThrust.IsPickable = false;
                    m_SunThrust.IsDropable = false;
                    m_SunThrust.CanDropAsLoot = false;
                    m_SunThrust.IsTradable = false;
                    m_SunThrust.MaxCount = 1;
                    m_SunThrust.PackSize = 1;
                    m_SunThrust.ProcSpellID = 65513;

                }
                return m_SunThrust;
            }
        }

        private ItemTemplate Flex
        {
            get
            {
                m_SunFlexScytheClaw = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Flex");
                if (m_SunFlexScytheClaw == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Flex, loading it ...");
                    m_SunFlexScytheClaw = new ItemTemplate();
                    m_SunFlexScytheClaw.Id_nb = "Sun_Flex";
                    m_SunFlexScytheClaw.Name = "Sun Spiked Flail";
                    m_SunFlexScytheClaw.Level = 50;
                    m_SunFlexScytheClaw.Durability = 50000;
                    m_SunFlexScytheClaw.MaxDurability = 50000;
                    m_SunFlexScytheClaw.Condition = 50000;
                    m_SunFlexScytheClaw.MaxCondition = 50000;
                    m_SunFlexScytheClaw.Quality = 100;
                    m_SunFlexScytheClaw.DPS_AF = 150;
                    m_SunFlexScytheClaw.SPD_ABS = 35;
                    m_SunFlexScytheClaw.Type_Damage = 0;
                    m_SunFlexScytheClaw.Object_Type = 24;
                    m_SunFlexScytheClaw.Item_Type = 10;
                    m_SunFlexScytheClaw.Hand = 0;
                    m_SunFlexScytheClaw.Model = 1924;
                    m_SunFlexScytheClaw.Bonus1 = 6;
                    m_SunFlexScytheClaw.Bonus2 = 27;
                    m_SunFlexScytheClaw.Bonus3 = 2;
                    m_SunFlexScytheClaw.Bonus4 = 2;
                    m_SunFlexScytheClaw.Bonus5 = 2;
                    m_SunFlexScytheClaw.Bonus1Type = 33;
                    m_SunFlexScytheClaw.Bonus2Type = 1;
                    m_SunFlexScytheClaw.Bonus3Type = 173;
                    m_SunFlexScytheClaw.Bonus4Type = 200;
                    m_SunFlexScytheClaw.Bonus5Type = 155;
                    m_SunFlexScytheClaw.IsPickable = false;
                    m_SunFlexScytheClaw.IsDropable = false;
                    m_SunFlexScytheClaw.CanDropAsLoot = false;
                    m_SunFlexScytheClaw.IsTradable = false;
                    m_SunFlexScytheClaw.MaxCount = 1;
                    m_SunFlexScytheClaw.PackSize = 1;
                    m_SunFlexScytheClaw.ProcSpellID = 65513;

                }
                return m_SunFlexScytheClaw;
            }
        }

        private ItemTemplate Polearm
        {
            get
            {
                m_SunPolearmSpear = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Polearm");
                if (m_SunPolearmSpear == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Polearm, loading it ...");
                    m_SunPolearmSpear = new ItemTemplate();
                    m_SunPolearmSpear.Id_nb = "Sun_Polearm";
                    m_SunPolearmSpear.Name = "Sun Glaive";
                    m_SunPolearmSpear.Level = 50;
                    m_SunPolearmSpear.Durability = 50000;
                    m_SunPolearmSpear.MaxDurability = 50000;
                    m_SunPolearmSpear.Condition = 50000;
                    m_SunPolearmSpear.MaxCondition = 50000;
                    m_SunPolearmSpear.Quality = 100;
                    m_SunPolearmSpear.DPS_AF = 150;
                    m_SunPolearmSpear.SPD_ABS = 52;
                    m_SunPolearmSpear.Type_Damage = 0;
                    m_SunPolearmSpear.Object_Type = 7;
                    m_SunPolearmSpear.Item_Type = 12;
                    m_SunPolearmSpear.Hand = 1;
                    m_SunPolearmSpear.Model = 1936;
                    m_SunPolearmSpear.Bonus1 = 6;
                    m_SunPolearmSpear.Bonus2 = 27;
                    m_SunPolearmSpear.Bonus3 = 2;
                    m_SunPolearmSpear.Bonus4 = 2;
                    m_SunPolearmSpear.Bonus5 = 2;
                    m_SunPolearmSpear.Bonus1Type = 41;
                    m_SunPolearmSpear.Bonus2Type = 1;
                    m_SunPolearmSpear.Bonus3Type = 173;
                    m_SunPolearmSpear.Bonus4Type = 200;
                    m_SunPolearmSpear.Bonus5Type = 155;
                    m_SunPolearmSpear.IsPickable = false;
                    m_SunPolearmSpear.IsDropable = false;
                    m_SunPolearmSpear.CanDropAsLoot = false;
                    m_SunPolearmSpear.IsTradable = false;
                    m_SunPolearmSpear.MaxCount = 1;
                    m_SunPolearmSpear.PackSize = 1;
                    m_SunPolearmSpear.ProcSpellID = 65513;

                }
                return m_SunPolearmSpear;
            }
        }

        private ItemTemplate TwoHanded
        {
            get
            {
                m_SunTwoHanded = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded");
                if (m_SunTwoHanded == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_TwoHanded, loading it ...");
                    m_SunTwoHanded = new ItemTemplate();
                    m_SunTwoHanded.Id_nb = "Sun_TwoHanded";
                    m_SunTwoHanded.Name = "Sun Twohanded Sword";
                    m_SunTwoHanded.Level = 50;
                    m_SunTwoHanded.Durability = 50000;
                    m_SunTwoHanded.MaxDurability = 50000;
                    m_SunTwoHanded.Condition = 50000;
                    m_SunTwoHanded.MaxCondition = 50000;
                    m_SunTwoHanded.Quality = 100;
                    m_SunTwoHanded.DPS_AF = 150;
                    m_SunTwoHanded.SPD_ABS = 52;
                    m_SunTwoHanded.Type_Damage = 0;
                    m_SunTwoHanded.Object_Type = 6;
                    m_SunTwoHanded.Item_Type = 12;
                    m_SunTwoHanded.Hand = 1;
                    m_SunTwoHanded.Model = 1904;
                    m_SunTwoHanded.Bonus1 = 6;
                    m_SunTwoHanded.Bonus2 = 27;
                    m_SunTwoHanded.Bonus3 = 2;
                    m_SunTwoHanded.Bonus4 = 2;
                    m_SunTwoHanded.Bonus5 = 2;
                    m_SunTwoHanded.Bonus1Type = 20;
                    m_SunTwoHanded.Bonus2Type = 1;
                    m_SunTwoHanded.Bonus3Type = 173;
                    m_SunTwoHanded.Bonus4Type = 200;
                    m_SunTwoHanded.Bonus5Type = 155;
                    m_SunTwoHanded.IsPickable = false;
                    m_SunTwoHanded.IsDropable = false;
                    m_SunTwoHanded.CanDropAsLoot = false;
                    m_SunTwoHanded.IsTradable = false;
                    m_SunTwoHanded.MaxCount = 1;
                    m_SunTwoHanded.PackSize = 1;
                    m_SunTwoHanded.ProcSpellID = 65513;

                }
                return m_SunTwoHanded;
            }
        }

        private ItemTemplate Bow
        {
            get
            {
                m_SunBow = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow");
                if (m_SunBow == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Bow, loading it ...");
                    m_SunBow = new ItemTemplate();
                    m_SunBow.Id_nb = "Sun_Bow";
                    m_SunBow.Name = "Sun Bow";
                    m_SunBow.Level = 50;
                    m_SunBow.Durability = 50000;
                    m_SunBow.MaxDurability = 50000;
                    m_SunBow.Condition = 50000;
                    m_SunBow.MaxCondition = 50000;
                    m_SunBow.Quality = 100;
                    m_SunBow.DPS_AF = 150;
                    m_SunBow.SPD_ABS = 48;
                    m_SunBow.Type_Damage = 0;
                    m_SunBow.Object_Type = 9;
                    m_SunBow.Item_Type = 13;
                    m_SunBow.Hand = 1;
                    m_SunBow.Model = 1912;
                    m_SunBow.Bonus1 = 6;
                    m_SunBow.Bonus2 = 27;
                    m_SunBow.Bonus3 = 2;
                    m_SunBow.Bonus4 = 2;
                    m_SunBow.Bonus5 = 2;
                    m_SunBow.Bonus1Type = 36;
                    m_SunBow.Bonus2Type = 1;
                    m_SunBow.Bonus3Type = 173;
                    m_SunBow.Bonus4Type = 200;
                    m_SunBow.Bonus5Type = 155;
                    m_SunBow.IsPickable = false;
                    m_SunBow.IsDropable = false;
                    m_SunBow.CanDropAsLoot = false;
                    m_SunBow.IsTradable = false;
                    m_SunBow.MaxCount = 1;
                    m_SunBow.PackSize = 1;
                    m_SunBow.ProcSpellID = 65513;

                }
                return m_SunBow;
            }
        }

        private ItemTemplate Staff
        {
            get
            {
                m_SunStaff = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Staff");
                if (m_SunStaff == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Staff, loading it ...");
                    m_SunStaff = new ItemTemplate();
                    m_SunStaff.Id_nb = "Sun_Staff";
                    m_SunStaff.Name = "Sun QuarterStaff";
                    m_SunStaff.Level = 50;
                    m_SunStaff.Durability = 50000;
                    m_SunStaff.MaxDurability = 50000;
                    m_SunStaff.Condition = 50000;
                    m_SunStaff.MaxCondition = 50000;
                    m_SunStaff.Quality = 100;
                    m_SunStaff.DPS_AF = 150;
                    m_SunStaff.SPD_ABS = 42;
                    m_SunStaff.Type_Damage = 0;
                    m_SunStaff.Object_Type = 8;
                    m_SunStaff.Item_Type = 12;
                    m_SunStaff.Hand = 1;
                    m_SunStaff.Model = 1952;
                    m_SunStaff.Bonus1 = 6;
                    m_SunStaff.Bonus2 = 27;
                    m_SunStaff.Bonus3 = 2;
                    m_SunStaff.Bonus4 = 2;
                    m_SunStaff.Bonus5 = 2;
                    m_SunStaff.Bonus1Type = 48;
                    m_SunStaff.Bonus2Type = 1;
                    m_SunStaff.Bonus3Type = 173;
                    m_SunStaff.Bonus4Type = 200;
                    m_SunStaff.Bonus5Type = 155;
                    m_SunStaff.IsPickable = false;
                    m_SunStaff.IsDropable = false;
                    m_SunStaff.CanDropAsLoot = false;
                    m_SunStaff.IsTradable = false;
                    m_SunStaff.MaxCount = 1;
                    m_SunStaff.PackSize = 1;
                    m_SunStaff.ProcSpellID = 65513;

                }
                return m_SunStaff;
            }
        }

        private ItemTemplate MStaff
        {
            get
            {
                m_SunMStaff = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MStaff");
                if (m_SunMStaff == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_MStaff, loading it ...");
                    m_SunMStaff = new ItemTemplate();
                    m_SunMStaff.Id_nb = "Sun_MStaff";
                    m_SunMStaff.Name = "Sun Maulers QuarterStaff";
                    m_SunMStaff.Level = 50;
                    m_SunMStaff.Durability = 50000;
                    m_SunMStaff.MaxDurability = 50000;
                    m_SunMStaff.Condition = 50000;
                    m_SunMStaff.MaxCondition = 50000;
                    m_SunMStaff.Quality = 100;
                    m_SunMStaff.DPS_AF = 150;
                    m_SunMStaff.SPD_ABS = 42;
                    m_SunMStaff.Type_Damage = 0;
                    m_SunMStaff.Object_Type = 28;
                    m_SunMStaff.Item_Type = 12;
                    m_SunMStaff.Hand = 1;
                    m_SunMStaff.Model = 1952;
                    m_SunMStaff.Bonus1 = 6;
                    m_SunMStaff.Bonus2 = 27;
                    m_SunMStaff.Bonus3 = 2;
                    m_SunMStaff.Bonus4 = 2;
                    m_SunMStaff.Bonus5 = 2;
                    m_SunMStaff.Bonus1Type = 109;
                    m_SunMStaff.Bonus2Type = 1;
                    m_SunMStaff.Bonus3Type = 173;
                    m_SunMStaff.Bonus4Type = 200;
                    m_SunMStaff.Bonus5Type = 155;
                    m_SunMStaff.IsPickable = false;
                    m_SunMStaff.IsDropable = false;
                    m_SunMStaff.CanDropAsLoot = false;
                    m_SunMStaff.IsTradable = false;
                    m_SunMStaff.MaxCount = 1;
                    m_SunMStaff.PackSize = 1;
                    m_SunMStaff.ProcSpellID = 65513;

                }
                return m_SunMStaff;
            }
        }

        private ItemTemplate MFist
        {
            get
            {
                m_SunMFist = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_MFist");
                if (m_SunMFist == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_MFist, loading it ...");
                    m_SunMFist = new ItemTemplate();
                    m_SunMFist.Id_nb = "Sun_MFist";
                    m_SunMFist.Name = "Sun MFist";
                    m_SunMFist.Level = 50;
                    m_SunMFist.Durability = 50000;
                    m_SunMFist.MaxDurability = 50000;
                    m_SunMFist.Condition = 50000;
                    m_SunMFist.MaxCondition = 50000;
                    m_SunMFist.Quality = 100;
                    m_SunMFist.DPS_AF = 150;
                    m_SunMFist.SPD_ABS = 42;
                    m_SunMFist.Type_Damage = 0;
                    m_SunMFist.Object_Type = 27;
                    m_SunMFist.Item_Type = 11;
                    m_SunMFist.Hand = 2;
                    m_SunMFist.Model = 2028;
                    m_SunMFist.Bonus1 = 6;
                    m_SunMFist.Bonus2 = 27;
                    m_SunMFist.Bonus3 = 2;
                    m_SunMFist.Bonus4 = 2;
                    m_SunMFist.Bonus5 = 2;
                    m_SunMFist.Bonus1Type = 110;
                    m_SunMFist.Bonus2Type = 1;
                    m_SunMFist.Bonus3Type = 173;
                    m_SunMFist.Bonus4Type = 200;
                    m_SunMFist.Bonus5Type = 155;
                    m_SunMFist.IsPickable = false;
                    m_SunMFist.IsDropable = false;
                    m_SunMFist.CanDropAsLoot = false;
                    m_SunMFist.IsTradable = false;
                    m_SunMFist.MaxCount = 1;
                    m_SunMFist.PackSize = 1;
                    m_SunMFist.ProcSpellID = 65513;

                }
                return m_SunMFist;
            }
        }
        #endregion Alb Weapons

        #region Sun Midgard Weapons
        private ItemTemplate CrushM
        {
            get
            {
                m_SunCrush = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush");
                if (m_SunCrush == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Crush, loading it ...");
                    m_SunCrush = new ItemTemplate();
                    m_SunCrush.Id_nb = "Sun_Crush";
                    m_SunCrush.Name = "Sun Warhammer";
                    m_SunCrush.Level = 50;
                    m_SunCrush.Durability = 50000;
                    m_SunCrush.MaxDurability = 50000;
                    m_SunCrush.Condition = 50000;
                    m_SunCrush.MaxCondition = 50000;
                    m_SunCrush.Quality = 100;
                    m_SunCrush.DPS_AF = 150;
                    m_SunCrush.SPD_ABS = 35;
                    m_SunCrush.Type_Damage = 0;
                    m_SunCrush.Object_Type = 12;
                    m_SunCrush.Item_Type = 10;
                    m_SunCrush.Hand = 2;
                    m_SunCrush.Model = 2044;
                    m_SunCrush.Bonus1 = 6;
                    m_SunCrush.Bonus2 = 27;
                    m_SunCrush.Bonus3 = 2;
                    m_SunCrush.Bonus4 = 2;
                    m_SunCrush.Bonus5 = 2;
                    m_SunCrush.Bonus1Type = 53;
                    m_SunCrush.Bonus2Type = 1;
                    m_SunCrush.Bonus3Type = 173;
                    m_SunCrush.Bonus4Type = 200;
                    m_SunCrush.Bonus5Type = 155;
                    m_SunCrush.IsPickable = false;
                    m_SunCrush.IsDropable = false;
                    m_SunCrush.CanDropAsLoot = false;
                    m_SunCrush.IsTradable = false;
                    m_SunCrush.MaxCount = 1;
                    m_SunCrush.PackSize = 1;
                    m_SunCrush.ProcSpellID = 65513;

                }
                return m_SunCrush;
            }
        }

        private ItemTemplate SlashM
        {
            get
            {
                m_SunSlash = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash");
                if (m_SunSlash == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Slash, loading it ...");
                    m_SunSlash = new ItemTemplate();
                    m_SunSlash.Id_nb = "Sun_Slash";
                    m_SunSlash.Name = "Sun Sword";
                    m_SunSlash.Level = 50;
                    m_SunSlash.Durability = 50000;
                    m_SunSlash.MaxDurability = 50000;
                    m_SunSlash.Condition = 50000;
                    m_SunSlash.MaxCondition = 50000;
                    m_SunSlash.Quality = 100;
                    m_SunSlash.DPS_AF = 150;
                    m_SunSlash.SPD_ABS = 35;
                    m_SunSlash.Type_Damage = 0;
                    m_SunSlash.Object_Type = 11;
                    m_SunSlash.Item_Type = 10;
                    m_SunSlash.Hand = 2;
                    m_SunSlash.Model = 2036;
                    m_SunSlash.Bonus1 = 6;
                    m_SunSlash.Bonus2 = 27;
                    m_SunSlash.Bonus3 = 2;
                    m_SunSlash.Bonus4 = 2;
                    m_SunSlash.Bonus5 = 2;
                    m_SunSlash.Bonus1Type = 52;
                    m_SunSlash.Bonus2Type = 1;
                    m_SunSlash.Bonus3Type = 173;
                    m_SunSlash.Bonus4Type = 200;
                    m_SunSlash.Bonus5Type = 155;
                    m_SunSlash.IsPickable = false;
                    m_SunSlash.IsDropable = false;
                    m_SunSlash.CanDropAsLoot = false;
                    m_SunSlash.IsTradable = false;
                    m_SunSlash.MaxCount = 1;
                    m_SunSlash.PackSize = 1;
                    m_SunSlash.ProcSpellID = 65513;

                }
                return m_SunSlash;
            }
        }

        private ItemTemplate Axe
        {
            get
            {
                m_SunAxe = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Axe");
                if (m_SunAxe == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Axe, loading it ...");
                    m_SunAxe = new ItemTemplate();
                    m_SunAxe.Id_nb = "Sun_Axe";
                    m_SunAxe.Name = "Sun Axe";
                    m_SunAxe.Level = 50;
                    m_SunAxe.Durability = 50000;
                    m_SunAxe.MaxDurability = 50000;
                    m_SunAxe.Condition = 50000;
                    m_SunAxe.MaxCondition = 50000;
                    m_SunAxe.Quality = 100;
                    m_SunAxe.DPS_AF = 150;
                    m_SunAxe.SPD_ABS = 35;
                    m_SunAxe.Type_Damage = 0;
                    m_SunAxe.Object_Type = 13;
                    m_SunAxe.Item_Type = 10;
                    m_SunAxe.Hand = 0;
                    m_SunAxe.Model = 2032;
                    m_SunAxe.Bonus1 = 6;
                    m_SunAxe.Bonus2 = 27;
                    m_SunAxe.Bonus3 = 2;
                    m_SunAxe.Bonus4 = 2;
                    m_SunAxe.Bonus5 = 2;
                    m_SunAxe.Bonus1Type = 54;
                    m_SunAxe.Bonus2Type = 1;
                    m_SunAxe.Bonus3Type = 173;
                    m_SunAxe.Bonus4Type = 200;
                    m_SunAxe.Bonus5Type = 155;
                    m_SunAxe.IsPickable = false;
                    m_SunAxe.IsDropable = false;
                    m_SunAxe.CanDropAsLoot = false;
                    m_SunAxe.IsTradable = false;
                    m_SunAxe.MaxCount = 1;
                    m_SunAxe.PackSize = 1;
                    m_SunAxe.ProcSpellID = 65513;

                }
                return m_SunAxe;
            }
        }

        private ItemTemplate LeftAxe
        {
            get
            {
                m_SunLeftAxe = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_LeftAxe");
                if (m_SunLeftAxe == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_LeftAxe, loading it ...");
                    m_SunLeftAxe = new ItemTemplate();
                    m_SunLeftAxe.Id_nb = "Sun_LeftAxe";
                    m_SunLeftAxe.Name = "Sun LeftAxe";
                    m_SunLeftAxe.Level = 50;
                    m_SunLeftAxe.Durability = 50000;
                    m_SunLeftAxe.MaxDurability = 50000;
                    m_SunLeftAxe.Condition = 50000;
                    m_SunLeftAxe.MaxCondition = 50000;
                    m_SunLeftAxe.Quality = 100;
                    m_SunLeftAxe.DPS_AF = 150;
                    m_SunLeftAxe.SPD_ABS = 35;
                    m_SunLeftAxe.Type_Damage = 0;
                    m_SunLeftAxe.Object_Type = 17;
                    m_SunLeftAxe.Item_Type = 11;
                    m_SunLeftAxe.Hand = 2;
                    m_SunLeftAxe.Model = 2032;
                    m_SunLeftAxe.Bonus1 = 6;
                    m_SunLeftAxe.Bonus2 = 27;
                    m_SunLeftAxe.Bonus3 = 2;
                    m_SunLeftAxe.Bonus4 = 2;
                    m_SunLeftAxe.Bonus5 = 2;
                    m_SunLeftAxe.Bonus1Type = 55;
                    m_SunLeftAxe.Bonus2Type = 1;
                    m_SunLeftAxe.Bonus3Type = 173;
                    m_SunLeftAxe.Bonus4Type = 200;
                    m_SunLeftAxe.Bonus5Type = 155;
                    m_SunLeftAxe.IsPickable = false;
                    m_SunLeftAxe.IsDropable = false;
                    m_SunLeftAxe.CanDropAsLoot = false;
                    m_SunLeftAxe.IsTradable = false;
                    m_SunLeftAxe.MaxCount = 1;
                    m_SunLeftAxe.PackSize = 1;
                    m_SunLeftAxe.ProcSpellID = 65513;

                }
                return m_SunLeftAxe;
            }
        }

        private ItemTemplate Claw
        {
            get
            {
                m_SunFlexScytheClaw = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Claw");
                if (m_SunFlexScytheClaw == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Claw, loading it ...");
                    m_SunFlexScytheClaw = new ItemTemplate();
                    m_SunFlexScytheClaw.Id_nb = "Sun_Claw";
                    m_SunFlexScytheClaw.Name = "Sun Claw";
                    m_SunFlexScytheClaw.Level = 50;
                    m_SunFlexScytheClaw.Durability = 50000;
                    m_SunFlexScytheClaw.MaxDurability = 50000;
                    m_SunFlexScytheClaw.Condition = 50000;
                    m_SunFlexScytheClaw.MaxCondition = 50000;
                    m_SunFlexScytheClaw.Quality = 100;
                    m_SunFlexScytheClaw.DPS_AF = 150;
                    m_SunFlexScytheClaw.SPD_ABS = 35;
                    m_SunFlexScytheClaw.Type_Damage = 0;
                    m_SunFlexScytheClaw.Object_Type = 25;
                    m_SunFlexScytheClaw.Item_Type = 11;
                    m_SunFlexScytheClaw.Hand = 2;
                    m_SunFlexScytheClaw.Model = 2028;
                    m_SunFlexScytheClaw.Bonus1 = 6;
                    m_SunFlexScytheClaw.Bonus2 = 27;
                    m_SunFlexScytheClaw.Bonus3 = 2;
                    m_SunFlexScytheClaw.Bonus4 = 2;
                    m_SunFlexScytheClaw.Bonus5 = 2;
                    m_SunFlexScytheClaw.Bonus1Type = 92;
                    m_SunFlexScytheClaw.Bonus2Type = 1;
                    m_SunFlexScytheClaw.Bonus3Type = 173;
                    m_SunFlexScytheClaw.Bonus4Type = 200;
                    m_SunFlexScytheClaw.Bonus5Type = 155;
                    m_SunFlexScytheClaw.IsPickable = false;
                    m_SunFlexScytheClaw.IsDropable = false;
                    m_SunFlexScytheClaw.CanDropAsLoot = false;
                    m_SunFlexScytheClaw.IsTradable = false;
                    m_SunFlexScytheClaw.MaxCount = 1;
                    m_SunFlexScytheClaw.PackSize = 1;
                    m_SunFlexScytheClaw.ProcSpellID = 65513;

                }
                return m_SunFlexScytheClaw;
            }
        }

        private ItemTemplate SpearM
        {
            get
            {
                m_SunPolearmSpear = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Spear");
                if (m_SunPolearmSpear == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Spear, loading it ...");
                    m_SunPolearmSpear = new ItemTemplate();
                    m_SunPolearmSpear.Id_nb = "Sun_Spear";
                    m_SunPolearmSpear.Name = "Sun Spear";
                    m_SunPolearmSpear.Level = 50;
                    m_SunPolearmSpear.Durability = 50000;
                    m_SunPolearmSpear.MaxDurability = 50000;
                    m_SunPolearmSpear.Condition = 50000;
                    m_SunPolearmSpear.MaxCondition = 50000;
                    m_SunPolearmSpear.Quality = 100;
                    m_SunPolearmSpear.DPS_AF = 150;
                    m_SunPolearmSpear.SPD_ABS = 48;
                    m_SunPolearmSpear.Type_Damage = 0;
                    m_SunPolearmSpear.Object_Type = 14;
                    m_SunPolearmSpear.Item_Type = 12;
                    m_SunPolearmSpear.Hand = 1;
                    m_SunPolearmSpear.Model = 2048;
                    m_SunPolearmSpear.Bonus1 = 6;
                    m_SunPolearmSpear.Bonus2 = 27;
                    m_SunPolearmSpear.Bonus3 = 2;
                    m_SunPolearmSpear.Bonus4 = 2;
                    m_SunPolearmSpear.Bonus5 = 2;
                    m_SunPolearmSpear.Bonus1Type = 56;
                    m_SunPolearmSpear.Bonus2Type = 1;
                    m_SunPolearmSpear.Bonus3Type = 173;
                    m_SunPolearmSpear.Bonus4Type = 200;
                    m_SunPolearmSpear.Bonus5Type = 155;
                    m_SunPolearmSpear.IsPickable = false;
                    m_SunPolearmSpear.IsDropable = false;
                    m_SunPolearmSpear.CanDropAsLoot = false;
                    m_SunPolearmSpear.IsTradable = false;
                    m_SunPolearmSpear.MaxCount = 1;
                    m_SunPolearmSpear.PackSize = 1;
                    m_SunPolearmSpear.ProcSpellID = 65513;

                }
                return m_SunPolearmSpear;
            }
        }

        private ItemTemplate TwoHandedM
        {
            get
            {
                m_SunTwoHanded = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded");
                if (m_SunTwoHanded == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_TwoHanded, loading it ...");
                    m_SunTwoHanded = new ItemTemplate();
                    m_SunTwoHanded.Id_nb = "Sun_TwoHanded";
                    m_SunTwoHanded.Name = "Sun Greater Sword";
                    m_SunTwoHanded.Level = 50;
                    m_SunTwoHanded.Durability = 50000;
                    m_SunTwoHanded.MaxDurability = 50000;
                    m_SunTwoHanded.Condition = 50000;
                    m_SunTwoHanded.MaxCondition = 50000;
                    m_SunTwoHanded.Quality = 100;
                    m_SunTwoHanded.DPS_AF = 150;
                    m_SunTwoHanded.SPD_ABS = 52;
                    m_SunTwoHanded.Type_Damage = 0;
                    m_SunTwoHanded.Object_Type = 11;
                    m_SunTwoHanded.Item_Type = 12;
                    m_SunTwoHanded.Hand = 1;
                    m_SunTwoHanded.Model = 2060;
                    m_SunTwoHanded.Bonus1 = 6;
                    m_SunTwoHanded.Bonus2 = 27;
                    m_SunTwoHanded.Bonus3 = 2;
                    m_SunTwoHanded.Bonus4 = 2;
                    m_SunTwoHanded.Bonus5 = 2;
                    m_SunTwoHanded.Bonus1Type = 52;
                    m_SunTwoHanded.Bonus2Type = 1;
                    m_SunTwoHanded.Bonus3Type = 173;
                    m_SunTwoHanded.Bonus4Type = 200;
                    m_SunTwoHanded.Bonus5Type = 155;
                    m_SunTwoHanded.IsPickable = false;
                    m_SunTwoHanded.IsDropable = false;
                    m_SunTwoHanded.CanDropAsLoot = false;
                    m_SunTwoHanded.IsTradable = false;
                    m_SunTwoHanded.MaxCount = 1;
                    m_SunTwoHanded.PackSize = 1;
                    m_SunTwoHanded.ProcSpellID = 65513;

                }
                return m_SunTwoHanded;
            }
        }

        private ItemTemplate BowM
        {
            get
            {
                m_SunBow = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow");
                if (m_SunBow == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Bow, loading it ...");
                    m_SunBow = new ItemTemplate();
                    m_SunBow.Id_nb = "Sun_Bow";
                    m_SunBow.Name = "Sun Bow";
                    m_SunBow.Level = 50;
                    m_SunBow.Durability = 50000;
                    m_SunBow.MaxDurability = 50000;
                    m_SunBow.Condition = 50000;
                    m_SunBow.MaxCondition = 50000;
                    m_SunBow.Quality = 100;
                    m_SunBow.DPS_AF = 150;
                    m_SunBow.SPD_ABS = 48;
                    m_SunBow.Type_Damage = 0;
                    m_SunBow.Object_Type = 15;
                    m_SunBow.Item_Type = 13;
                    m_SunBow.Hand = 1;
                    m_SunBow.Model = 2064;
                    m_SunBow.Bonus1 = 6;
                    m_SunBow.Bonus2 = 27;
                    m_SunBow.Bonus3 = 2;
                    m_SunBow.Bonus4 = 2;
                    m_SunBow.Bonus5 = 2;
                    m_SunBow.Bonus1Type = 68;
                    m_SunBow.Bonus2Type = 1;
                    m_SunBow.Bonus3Type = 173;
                    m_SunBow.Bonus4Type = 200;
                    m_SunBow.Bonus5Type = 155;
                    m_SunBow.IsPickable = false;
                    m_SunBow.IsDropable = false;
                    m_SunBow.CanDropAsLoot = false;
                    m_SunBow.IsTradable = false;
                    m_SunBow.MaxCount = 1;
                    m_SunBow.PackSize = 1;
                    m_SunBow.ProcSpellID = 65513;

                }
                return m_SunBow;
            }
        }

        private ItemTemplate THCrushM
        {
            get
            {
                m_Sun2HCrush = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HCrush");
                if (m_Sun2HCrush == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_2HCrush, loading it ...");
                    m_Sun2HCrush = new ItemTemplate();
                    m_Sun2HCrush.Id_nb = "Sun_2HCrush";
                    m_Sun2HCrush.Name = "Sun Greater Warhammer";
                    m_Sun2HCrush.Level = 50;
                    m_Sun2HCrush.Durability = 50000;
                    m_Sun2HCrush.MaxDurability = 50000;
                    m_Sun2HCrush.Condition = 50000;
                    m_Sun2HCrush.MaxCondition = 50000;
                    m_Sun2HCrush.Quality = 100;
                    m_Sun2HCrush.DPS_AF = 150;
                    m_Sun2HCrush.SPD_ABS = 52;
                    m_Sun2HCrush.Type_Damage = 0;
                    m_Sun2HCrush.Object_Type = 12;
                    m_Sun2HCrush.Item_Type = 12;
                    m_Sun2HCrush.Hand = 1;
                    m_Sun2HCrush.Model = 2056;
                    m_Sun2HCrush.Bonus1 = 6;
                    m_Sun2HCrush.Bonus2 = 27;
                    m_Sun2HCrush.Bonus3 = 2;
                    m_Sun2HCrush.Bonus4 = 2;
                    m_Sun2HCrush.Bonus5 = 2;
                    m_Sun2HCrush.Bonus1Type = 53;
                    m_Sun2HCrush.Bonus2Type = 1;
                    m_Sun2HCrush.Bonus3Type = 173;
                    m_Sun2HCrush.Bonus4Type = 200;
                    m_Sun2HCrush.Bonus5Type = 155;
                    m_Sun2HCrush.IsPickable = false;
                    m_Sun2HCrush.IsDropable = false;
                    m_Sun2HCrush.CanDropAsLoot = false;
                    m_Sun2HCrush.IsTradable = false;
                    m_Sun2HCrush.MaxCount = 1;
                    m_Sun2HCrush.PackSize = 1;
                    m_Sun2HCrush.ProcSpellID = 65513;

                }
                return m_Sun2HCrush;
            }
        }

        private ItemTemplate THAxe
        {
            get
            {
                m_Sun2HAxe = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_2HAxe");
                if (m_Sun2HAxe == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_2HAxe, loading it ...");
                    m_Sun2HAxe = new ItemTemplate();
                    m_Sun2HAxe.Id_nb = "Sun_2HAxe";
                    m_Sun2HAxe.Name = "Sun Greater Axe";
                    m_Sun2HAxe.Level = 50;
                    m_Sun2HAxe.Durability = 50000;
                    m_Sun2HAxe.MaxDurability = 50000;
                    m_Sun2HAxe.Condition = 50000;
                    m_Sun2HAxe.MaxCondition = 50000;
                    m_Sun2HAxe.Quality = 100;
                    m_Sun2HAxe.DPS_AF = 150;
                    m_Sun2HAxe.SPD_ABS = 52;
                    m_Sun2HAxe.Type_Damage = 0;
                    m_Sun2HAxe.Object_Type = 13;
                    m_Sun2HAxe.Item_Type = 12;
                    m_Sun2HAxe.Hand = 1;
                    m_Sun2HAxe.Model = 2052;
                    m_Sun2HAxe.Bonus1 = 6;
                    m_Sun2HAxe.Bonus2 = 27;
                    m_Sun2HAxe.Bonus3 = 2;
                    m_Sun2HAxe.Bonus4 = 2;
                    m_Sun2HAxe.Bonus5 = 2;
                    m_Sun2HAxe.Bonus1Type = 54;
                    m_Sun2HAxe.Bonus2Type = 1;
                    m_Sun2HAxe.Bonus3Type = 173;
                    m_Sun2HAxe.Bonus4Type = 200;
                    m_Sun2HAxe.Bonus5Type = 155;
                    m_Sun2HAxe.IsPickable = false;
                    m_Sun2HAxe.IsDropable = false;
                    m_Sun2HAxe.CanDropAsLoot = false;
                    m_Sun2HAxe.IsTradable = false;
                    m_Sun2HAxe.MaxCount = 1;
                    m_Sun2HAxe.PackSize = 1;
                    m_Sun2HAxe.ProcSpellID = 65513;

                }
                return m_Sun2HAxe;
            }
        }

        #endregion Mid Weapons

        #region Sun Hibernia Weapons
        private ItemTemplate CrushH
        {
            get
            {
                m_SunCrush = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Crush");
                if (m_SunCrush == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Crush, loading it ...");
                    m_SunCrush = new ItemTemplate();
                    m_SunCrush.Id_nb = "Sun_Crush";
                    m_SunCrush.Name = "Sun Hammer";
                    m_SunCrush.Level = 50;
                    m_SunCrush.Durability = 50000;
                    m_SunCrush.MaxDurability = 50000;
                    m_SunCrush.Condition = 50000;
                    m_SunCrush.MaxCondition = 50000;
                    m_SunCrush.Quality = 100;
                    m_SunCrush.DPS_AF = 150;
                    m_SunCrush.SPD_ABS = 35;
                    m_SunCrush.Type_Damage = 0;
                    m_SunCrush.Object_Type = 20;
                    m_SunCrush.Item_Type = 11;
                    m_SunCrush.Hand = 2;
                    m_SunCrush.Model = 1988;
                    m_SunCrush.Bonus1 = 6;
                    m_SunCrush.Bonus2 = 27;
                    m_SunCrush.Bonus3 = 2;
                    m_SunCrush.Bonus4 = 2;
                    m_SunCrush.Bonus5 = 2;
                    m_SunCrush.Bonus1Type = 73;
                    m_SunCrush.Bonus2Type = 1;
                    m_SunCrush.Bonus3Type = 173;
                    m_SunCrush.Bonus4Type = 200;
                    m_SunCrush.Bonus5Type = 155;
                    m_SunCrush.IsPickable = false;
                    m_SunCrush.IsDropable = false;
                    m_SunCrush.CanDropAsLoot = false;
                    m_SunCrush.IsTradable = false;
                    m_SunCrush.MaxCount = 1;
                    m_SunCrush.PackSize = 1;
                    m_SunCrush.ProcSpellID = 65513;

                }
                return m_SunCrush;
            }
        }

        private ItemTemplate SlashH
        {
            get
            {
                m_SunSlash = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Slash");
                if (m_SunSlash == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Slash, loading it ...");
                    m_SunSlash = new ItemTemplate();
                    m_SunSlash.Id_nb = "Sun_Slash";
                    m_SunSlash.Name = "Sun Blade";
                    m_SunSlash.Level = 50;
                    m_SunSlash.Durability = 50000;
                    m_SunSlash.MaxDurability = 50000;
                    m_SunSlash.Condition = 50000;
                    m_SunSlash.MaxCondition = 50000;
                    m_SunSlash.Quality = 100;
                    m_SunSlash.DPS_AF = 150;
                    m_SunSlash.SPD_ABS = 35;
                    m_SunSlash.Type_Damage = 0;
                    m_SunSlash.Object_Type = 19;
                    m_SunSlash.Item_Type = 11;
                    m_SunSlash.Hand = 2;
                    m_SunSlash.Model = 1948;
                    m_SunSlash.Bonus1 = 6;
                    m_SunSlash.Bonus2 = 27;
                    m_SunSlash.Bonus3 = 2;
                    m_SunSlash.Bonus4 = 2;
                    m_SunSlash.Bonus5 = 2;
                    m_SunSlash.Bonus1Type = 72;
                    m_SunSlash.Bonus2Type = 1;
                    m_SunSlash.Bonus3Type = 173;
                    m_SunSlash.Bonus4Type = 200;
                    m_SunSlash.Bonus5Type = 155;
                    m_SunSlash.IsPickable = false;
                    m_SunSlash.IsDropable = false;
                    m_SunSlash.CanDropAsLoot = false;
                    m_SunSlash.IsTradable = false;
                    m_SunSlash.MaxCount = 1;
                    m_SunSlash.PackSize = 1;
                    m_SunSlash.ProcSpellID = 65513;

                }
                return m_SunSlash;
            }
        }

        private ItemTemplate ThrustH
        {
            get
            {
                m_SunThrust = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Thrust");
                if (m_SunThrust == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Thrust, loading it ...");
                    m_SunThrust = new ItemTemplate();
                    m_SunThrust.Id_nb = "Sun_Thrust";
                    m_SunThrust.Name = "Sun Sword";
                    m_SunThrust.Level = 50;
                    m_SunThrust.Durability = 50000;
                    m_SunThrust.MaxDurability = 50000;
                    m_SunThrust.Condition = 50000;
                    m_SunThrust.MaxCondition = 50000;
                    m_SunThrust.Quality = 100;
                    m_SunThrust.DPS_AF = 150;
                    m_SunThrust.SPD_ABS = 35;
                    m_SunThrust.Type_Damage = 0;
                    m_SunThrust.Object_Type = 21;
                    m_SunThrust.Item_Type = 11;
                    m_SunThrust.Hand = 2;
                    m_SunThrust.Model = 1948;
                    m_SunThrust.Bonus1 = 6;
                    m_SunThrust.Bonus2 = 27;
                    m_SunThrust.Bonus3 = 2;
                    m_SunThrust.Bonus4 = 2;
                    m_SunThrust.Bonus5 = 2;
                    m_SunThrust.Bonus1Type = 74;
                    m_SunThrust.Bonus2Type = 1;
                    m_SunThrust.Bonus3Type = 173;
                    m_SunThrust.Bonus4Type = 200;
                    m_SunThrust.Bonus5Type = 155;
                    m_SunThrust.IsPickable = false;
                    m_SunThrust.IsDropable = false;
                    m_SunThrust.CanDropAsLoot = false;
                    m_SunThrust.IsTradable = false;
                    m_SunThrust.MaxCount = 1;
                    m_SunThrust.PackSize = 1;
                    m_SunThrust.ProcSpellID = 65513;

                }
                return m_SunThrust;
            }
        }


        private ItemTemplate Scythe
        {
            get
            {
                m_SunFlexScytheClaw = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Scythe");
                if (m_SunFlexScytheClaw == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Scythe, loading it ...");
                    m_SunFlexScytheClaw = new ItemTemplate();
                    m_SunFlexScytheClaw.Id_nb = "Sun_Scythe";
                    m_SunFlexScytheClaw.Name = "Sun Scythe";
                    m_SunFlexScytheClaw.Level = 50;
                    m_SunFlexScytheClaw.Durability = 50000;
                    m_SunFlexScytheClaw.MaxDurability = 50000;
                    m_SunFlexScytheClaw.Condition = 50000;
                    m_SunFlexScytheClaw.MaxCondition = 50000;
                    m_SunFlexScytheClaw.Quality = 100;
                    m_SunFlexScytheClaw.DPS_AF = 150;
                    m_SunFlexScytheClaw.SPD_ABS = 35;
                    m_SunFlexScytheClaw.Hand = 1;
                    m_SunFlexScytheClaw.Type_Damage = 0;
                    m_SunFlexScytheClaw.Object_Type = 26;
                    m_SunFlexScytheClaw.Item_Type = 12;
                    m_SunFlexScytheClaw.Model = 2004;
                    m_SunFlexScytheClaw.Bonus1 = 6;
                    m_SunFlexScytheClaw.Bonus2 = 27;
                    m_SunFlexScytheClaw.Bonus3 = 2;
                    m_SunFlexScytheClaw.Bonus4 = 2;
                    m_SunFlexScytheClaw.Bonus5 = 2;
                    m_SunFlexScytheClaw.Bonus1Type = 90;
                    m_SunFlexScytheClaw.Bonus2Type = 1;
                    m_SunFlexScytheClaw.Bonus3Type = 173;
                    m_SunFlexScytheClaw.Bonus4Type = 200;
                    m_SunFlexScytheClaw.Bonus5Type = 155;
                    m_SunFlexScytheClaw.IsPickable = false;
                    m_SunFlexScytheClaw.IsDropable = false;
                    m_SunFlexScytheClaw.CanDropAsLoot = false;
                    m_SunFlexScytheClaw.IsTradable = false;
                    m_SunFlexScytheClaw.MaxCount = 1;
                    m_SunFlexScytheClaw.PackSize = 1;
                    m_SunFlexScytheClaw.ProcSpellID = 65513;

                }
                return m_SunFlexScytheClaw;
            }
        }

        private ItemTemplate SpearH
        {
            get
            {
                m_SunPolearmSpear = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Spear");
                if (m_SunPolearmSpear == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Spear, loading it ...");
                    m_SunPolearmSpear = new ItemTemplate();
                    m_SunPolearmSpear.Id_nb = "Sun_Spear";
                    m_SunPolearmSpear.Name = "Sun Spear";
                    m_SunPolearmSpear.Level = 50;
                    m_SunPolearmSpear.Durability = 50000;
                    m_SunPolearmSpear.MaxDurability = 50000;
                    m_SunPolearmSpear.Condition = 50000;
                    m_SunPolearmSpear.MaxCondition = 50000;
                    m_SunPolearmSpear.Quality = 100;
                    m_SunPolearmSpear.DPS_AF = 150;
                    m_SunPolearmSpear.SPD_ABS = 52;
                    m_SunPolearmSpear.Type_Damage = 0;
                    m_SunPolearmSpear.Object_Type = 23;
                    m_SunPolearmSpear.Item_Type = 12;
                    m_SunPolearmSpear.Hand = 1;
                    m_SunPolearmSpear.Model = 2008;
                    m_SunPolearmSpear.Bonus1 = 6;
                    m_SunPolearmSpear.Bonus2 = 27;
                    m_SunPolearmSpear.Bonus3 = 2;
                    m_SunPolearmSpear.Bonus4 = 2;
                    m_SunPolearmSpear.Bonus5 = 2;
                    m_SunPolearmSpear.Bonus1Type = 82;
                    m_SunPolearmSpear.Bonus2Type = 1;
                    m_SunPolearmSpear.Bonus3Type = 173;
                    m_SunPolearmSpear.Bonus4Type = 200;
                    m_SunPolearmSpear.Bonus5Type = 155;
                    m_SunPolearmSpear.IsPickable = false;
                    m_SunPolearmSpear.IsDropable = false;
                    m_SunPolearmSpear.CanDropAsLoot = false;
                    m_SunPolearmSpear.IsTradable = false;
                    m_SunPolearmSpear.MaxCount = 1;
                    m_SunPolearmSpear.PackSize = 1;
                    m_SunPolearmSpear.ProcSpellID = 65513;

                }
                return m_SunPolearmSpear;
            }
        }

        private ItemTemplate TwoHandedH
        {
            get
            {
                m_SunTwoHanded = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_TwoHanded");
                if (m_SunTwoHanded == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_TwoHanded, loading it ...");
                    m_SunTwoHanded = new ItemTemplate();
                    m_SunTwoHanded.Id_nb = "Sun_TwoHanded";
                    m_SunTwoHanded.Name = "Sun Large Weapon";
                    m_SunTwoHanded.Level = 50;
                    m_SunTwoHanded.Durability = 50000;
                    m_SunTwoHanded.MaxDurability = 50000;
                    m_SunTwoHanded.Condition = 50000;
                    m_SunTwoHanded.MaxCondition = 50000;
                    m_SunTwoHanded.Quality = 100;
                    m_SunTwoHanded.DPS_AF = 150;
                    m_SunTwoHanded.SPD_ABS = 52;
                    m_SunTwoHanded.Type_Damage = 0;
                    m_SunTwoHanded.Object_Type = 22;
                    m_SunTwoHanded.Item_Type = 12;
                    m_SunTwoHanded.Hand = 1;
                    m_SunTwoHanded.Model = 1984;
                    m_SunTwoHanded.Bonus1 = 6;
                    m_SunTwoHanded.Bonus2 = 27;
                    m_SunTwoHanded.Bonus3 = 2;
                    m_SunTwoHanded.Bonus4 = 2;
                    m_SunTwoHanded.Bonus5 = 2;
                    m_SunTwoHanded.Bonus1Type = 75;
                    m_SunTwoHanded.Bonus2Type = 1;
                    m_SunTwoHanded.Bonus3Type = 173;
                    m_SunTwoHanded.Bonus4Type = 200;
                    m_SunTwoHanded.Bonus5Type = 155;
                    m_SunTwoHanded.IsPickable = false;
                    m_SunTwoHanded.IsDropable = false;
                    m_SunTwoHanded.CanDropAsLoot = false;
                    m_SunTwoHanded.IsTradable = false;
                    m_SunTwoHanded.MaxCount = 1;
                    m_SunTwoHanded.PackSize = 1;
                    m_SunTwoHanded.ProcSpellID = 65513;

                }
                return m_SunTwoHanded;
            }
        }

        private ItemTemplate BowH
        {
            get
            {
                m_SunBow = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Sun_Bow");
                if (m_SunBow == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Sun_Bow, loading it ...");
                    m_SunBow = new ItemTemplate();
                    m_SunBow.Id_nb = "Sun_Bow";
                    m_SunBow.Name = "Sun Bow";
                    m_SunBow.Level = 50;
                    m_SunBow.Durability = 50000;
                    m_SunBow.MaxDurability = 50000;
                    m_SunBow.Condition = 50000;
                    m_SunBow.MaxCondition = 50000;
                    m_SunBow.Quality = 100;
                    m_SunBow.DPS_AF = 150;
                    m_SunBow.SPD_ABS = 48;
                    m_SunBow.Type_Damage = 0;
                    m_SunBow.Object_Type = 18;
                    m_SunBow.Item_Type = 13;
                    m_SunBow.Hand = 1;
                    m_SunBow.Model = 1996;
                    m_SunBow.Bonus1 = 6;
                    m_SunBow.Bonus2 = 27;
                    m_SunBow.Bonus3 = 2;
                    m_SunBow.Bonus4 = 2;
                    m_SunBow.Bonus5 = 2;
                    m_SunBow.Bonus1Type = 83;
                    m_SunBow.Bonus2Type = 1;
                    m_SunBow.Bonus3Type = 173;
                    m_SunBow.Bonus4Type = 200;
                    m_SunBow.Bonus5Type = 155;
                    m_SunBow.IsPickable = false;
                    m_SunBow.IsDropable = false;
                    m_SunBow.CanDropAsLoot = false;
                    m_SunBow.IsTradable = false;
                    m_SunBow.MaxCount = 1;
                    m_SunBow.PackSize = 1;
                    m_SunBow.ProcSpellID = 65513;

                }
                return m_SunBow;
            }
        }

        #endregion Hib Weapons


        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            GameEventMgr.AddHandler(Caster, GamePlayerEvent.Released, OnPlayerReleased);
            GameEventMgr.AddHandler(Caster, GamePlayerEvent.Quit, OnPlayerLeft);
        }


        private static void OnPlayerReleased(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GamePlayer))
                return;

            GamePlayer player = sender as GamePlayer;

            lock (player.Inventory)
            {
                var items = player.Inventory.GetItemRange(eInventorySlot.MinEquipable, eInventorySlot.LastBackpack);
                foreach (InventoryItem invItem in items)
                {
                    if (player.CurrentRegion.IsNightTime)
                    {

                        if (invItem.Id_nb.Equals("Sun_Crush"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_Slash"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_Thrust"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_Flex"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_TwoHanded"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_Polearm"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_Bow"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_Staff"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_MFist"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_MStaff"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_Axe"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_LeftAxe"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_Claw"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_2HCrush"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_2HAxe"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_MStaff"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_FlexScythe"))
                            player.Inventory.RemoveItem(invItem);

                        if (invItem.Id_nb.Equals("Sun_Spear"))
                            player.Inventory.RemoveItem(invItem);

                        player.Out.SendMessage("The Power of Belt of Sun, has left you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                }
            }
            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Released, OnPlayerReleased);
        }

        private static void OnPlayerLeft(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GamePlayer))
                return;

            GamePlayer player = sender as GamePlayer;
            lock (player.Inventory)
            {
                var items = player.Inventory.GetItemRange(eInventorySlot.MinEquipable, eInventorySlot.LastBackpack);
                foreach (InventoryItem invItem in items)
                {

                    if (invItem.Id_nb.Equals("Sun_Crush"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_Slash"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_Thrust"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_Flex"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_TwoHanded"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_Polearm"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_Bow"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_Staff"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_MFist"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_MStaff"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_Axe"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_LeftAxe"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_Claw"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_2HCrush"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_2HAxe"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_MStaff"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_FlexScythe"))
                        player.Inventory.RemoveItem(invItem);

                    if (invItem.Id_nb.Equals("Sun_Spear"))
                        player.Inventory.RemoveItem(invItem);

                }
            }
            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Quit, OnPlayerLeft);
        }
    }
}