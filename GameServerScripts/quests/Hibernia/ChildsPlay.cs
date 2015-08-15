
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

/*
* Author:	Lamyuras
* Edited by: k109
* Date:		11/26/07
*
* Notes: Changed this quest to work like live server.
* UPDATE:  You must edit the Database if you want this quest to work correctly.
* remove any reference in the DB to "Statue Demons Breach", there will be 200+ if using rev 818DB
* also run this script: update mob set aggrolevel = 0 where flags = 12 and region = 489;
* the ambient corpses are all agro, and will attack if you get to close. 
*/

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Behaviour;
using DOL.GS.Behaviour.Attributes;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using DOL.Language;
using log4net;

namespace DOL.GS.Quests.Hibernia
{
    public class childsplay : BaseQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        ///
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected static string questTitle = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "ChildsPlay.QuestTitle");

        protected const int minimumLevel = 1;
        protected const int maximumLevel = 4;

        private static GameNPC Charles = null;
        private static ItemTemplate daringpaddedboots_hib = null;
        private static ItemTemplate daringpaddedcap_hib = null;
        private static ItemTemplate daringpaddedgloves_hib = null;
        private static ItemTemplate daringpaddedpants_hib = null;
        private static ItemTemplate daringpaddedsleeves_hib = null;
        private static ItemTemplate daringpaddedvest_hib = null;
        private static ItemTemplate daringleatherboots_hib = null;
        private static ItemTemplate daringleathercap_hib = null;
        private static ItemTemplate daringleathergloves_hib = null;
        private static ItemTemplate daringleatherjerkin_hib = null;
        private static ItemTemplate daringleatherleggings_hib = null;
        private static ItemTemplate daringleathersleeves_hib = null;
        private static ItemTemplate daringstuddedboots_hib = null;
        private static ItemTemplate daringstuddedcap_hib = null;
        private static ItemTemplate daringstuddedgloves_hib = null;
        private static ItemTemplate daringstuddedjerkin_hib = null;
        private static ItemTemplate daringstuddedleggings_hib = null;
        private static ItemTemplate daringstuddedsleeves_hib = null;
        private static GameLocation Hib_Statue = new GameLocation("Childs Play (Hib)", 489, 27580, 40006, 14483);

        private static IArea Hib_Statue_Area = null;

        public childsplay()
            : base()
        {
        }

        public childsplay(GamePlayer questingPlayer)
            : this(questingPlayer, 1)
        {
        }

        public childsplay(GamePlayer questingPlayer, int step)
            : base(questingPlayer, step)
        {
        }

        public childsplay(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest)
        {
        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" (Hib) initializing ...");

            #region defineNPCs
            GameNPC[] npcs;

            npcs = WorldMgr.GetNPCsByName("Charles", (eRealm)3);
            if (npcs.Length == 0)
            {
                Charles = new DOL.GS.GameNPC();
                Charles.Model = 381;
                Charles.Name = "Charles";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Charles.Name + ", creating ...");
                Charles.Realm = eRealm.Hibernia;
                Charles.CurrentRegionID = 200;
                Charles.Size = 37;
                Charles.Level = 1;
                Charles.MaxSpeedBase = 191;
                Charles.Faction = FactionMgr.GetFactionByID(0);
                Charles.X = 348034;
                Charles.Y = 490940;
                Charles.Z = 5200;
                Charles.Heading = 1149;
                Charles.RespawnInterval = -1;
                Charles.BodyType = 0;

                StandardMobBrain brain = new StandardMobBrain();
                brain.AggroLevel = 0;
                brain.AggroRange = 500;
                Charles.SetOwnBrain(brain);

                
                Charles.AddQuestObjectToDatabase();

                Charles.AddToWorld();

            }
            else
            {
                Charles = npcs[0];
            }


            #endregion

            #region defineItems

            daringpaddedboots_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringpaddedboots_hib");
            if (daringpaddedboots_hib == null)
            {
                daringpaddedboots_hib = new ItemTemplate();
                daringpaddedboots_hib.Name = "Daring Padded Boots";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedboots_hib.Name + ", creating it ...");
                daringpaddedboots_hib.Level = 5;
                daringpaddedboots_hib.Weight = 8;
                daringpaddedboots_hib.Model = 382;
                daringpaddedboots_hib.Object_Type = 32;
                daringpaddedboots_hib.Item_Type = 23;
                daringpaddedboots_hib.Id_nb = "daringpaddedboots_hib";
                daringpaddedboots_hib.Hand = 0;
                daringpaddedboots_hib.Price = 0;
                daringpaddedboots_hib.IsPickable = true;
                daringpaddedboots_hib.IsDropable = true;
                daringpaddedboots_hib.IsTradable = true;
                daringpaddedboots_hib.CanDropAsLoot = false;
                daringpaddedboots_hib.Color = 0;
                daringpaddedboots_hib.Bonus = 0; // default bonus
                daringpaddedboots_hib.ClearTemplateBonuses();
                daringpaddedboots_hib.SetTemplateBonuses(eProperty.Constitution, 4);
                daringpaddedboots_hib.Effect = 0;
                daringpaddedboots_hib.DPS_AF = 6;
                daringpaddedboots_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringpaddedboots_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringpaddedboots_hib.SPD_ABS = 0;
                daringpaddedboots_hib.Realm = 0;
                daringpaddedboots_hib.MaxCount = 1;
                daringpaddedboots_hib.PackSize = 1;
                daringpaddedboots_hib.Extension = 0;
                daringpaddedboots_hib.Quality = 89;
                daringpaddedboots_hib.Condition = 50000;
                daringpaddedboots_hib.MaxCondition = 50000;
                daringpaddedboots_hib.Durability = 50000;
                daringpaddedboots_hib.MaxDurability = 50000;
                daringpaddedboots_hib.PoisonCharges = 0;
                daringpaddedboots_hib.PoisonMaxCharges = 0;
                daringpaddedboots_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringpaddedboots_hib.AddQuestObjectToDatabase();
            }
            daringpaddedcap_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringpaddedcap_hib");
            if (daringpaddedcap_hib == null)
            {
                daringpaddedcap_hib = new ItemTemplate();
                daringpaddedcap_hib.Name = "Daring Padded Cap";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedcap_hib.Name + ", creating it ...");
                daringpaddedcap_hib.Level = 5;
                daringpaddedcap_hib.Weight = 8;
                daringpaddedcap_hib.Model = 826;
                daringpaddedcap_hib.Object_Type = 32;
                daringpaddedcap_hib.Item_Type = 21;
                daringpaddedcap_hib.Id_nb = "daringpaddedcap_hib";
                daringpaddedcap_hib.Hand = 0;
                daringpaddedcap_hib.Price = 0;
                daringpaddedcap_hib.IsPickable = true;
                daringpaddedcap_hib.IsDropable = true;
                daringpaddedcap_hib.IsTradable = true;
                daringpaddedcap_hib.CanDropAsLoot = false;
                daringpaddedcap_hib.Color = 0;
                daringpaddedcap_hib.Bonus = 0; // default bonus
                daringpaddedcap_hib.ClearTemplateBonuses();
                daringpaddedcap_hib.SetTemplateBonuses(eProperty.Dexterity, 4);
                daringpaddedcap_hib.Effect = 0;
                daringpaddedcap_hib.DPS_AF = 6;
                daringpaddedcap_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringpaddedcap_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringpaddedcap_hib.SPD_ABS = 0;
                daringpaddedcap_hib.Realm = 0;
                daringpaddedcap_hib.MaxCount = 1;
                daringpaddedcap_hib.PackSize = 1;
                daringpaddedcap_hib.Extension = 0;
                daringpaddedcap_hib.Quality = 89;
                daringpaddedcap_hib.Condition = 50000;
                daringpaddedcap_hib.MaxCondition = 50000;
                daringpaddedcap_hib.Durability = 50000;
                daringpaddedcap_hib.MaxDurability = 50000;
                daringpaddedcap_hib.PoisonCharges = 0;
                daringpaddedcap_hib.PoisonMaxCharges = 0;
                daringpaddedcap_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringpaddedcap_hib.AddQuestObjectToDatabase();
            }
            daringpaddedgloves_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringpaddedgloves_hib");
            if (daringpaddedgloves_hib == null)
            {
                daringpaddedgloves_hib = new ItemTemplate();
                daringpaddedgloves_hib.Name = "Daring Padded Gloves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedgloves_hib.Name + ", creating it ...");
                daringpaddedgloves_hib.Level = 5;
                daringpaddedgloves_hib.Weight = 8;
                daringpaddedgloves_hib.Model = 381;
                daringpaddedgloves_hib.Object_Type = 32;
                daringpaddedgloves_hib.Item_Type = 22;
                daringpaddedgloves_hib.Id_nb = "daringpaddedgloves_hib";
                daringpaddedgloves_hib.Hand = 0;
                daringpaddedgloves_hib.Price = 0;
                daringpaddedgloves_hib.IsPickable = true;
                daringpaddedgloves_hib.IsDropable = true;
                daringpaddedgloves_hib.IsTradable = true;
                daringpaddedgloves_hib.CanDropAsLoot = false;
                daringpaddedgloves_hib.Color = 0;
                daringpaddedgloves_hib.Bonus = 0; // default bonus
                daringpaddedgloves_hib.ClearTemplateBonuses();
                daringpaddedgloves_hib.SetTemplateBonuses(eProperty.Constitution, 4);
                daringpaddedgloves_hib.Effect = 0;
                daringpaddedgloves_hib.DPS_AF = 6;
                daringpaddedgloves_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringpaddedgloves_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringpaddedgloves_hib.SPD_ABS = 0;
                daringpaddedgloves_hib.Realm = 0;
                daringpaddedgloves_hib.MaxCount = 1;
                daringpaddedgloves_hib.PackSize = 1;
                daringpaddedgloves_hib.Extension = 0;
                daringpaddedgloves_hib.Quality = 89;
                daringpaddedgloves_hib.Condition = 50000;
                daringpaddedgloves_hib.MaxCondition = 50000;
                daringpaddedgloves_hib.Durability = 50000;
                daringpaddedgloves_hib.MaxDurability = 50000;
                daringpaddedgloves_hib.PoisonCharges = 0;
                daringpaddedgloves_hib.PoisonMaxCharges = 0;
                daringpaddedgloves_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringpaddedgloves_hib.AddQuestObjectToDatabase();
            }
            daringpaddedpants_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringpaddedpants_hib");
            if (daringpaddedpants_hib == null)
            {
                daringpaddedpants_hib = new ItemTemplate();
                daringpaddedpants_hib.Name = "Daring Padded Pants";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedpants_hib.Name + ", creating it ...");
                daringpaddedpants_hib.Level = 5;
                daringpaddedpants_hib.Weight = 14;
                daringpaddedpants_hib.Model = 379;
                daringpaddedpants_hib.Object_Type = 32;
                daringpaddedpants_hib.Item_Type = 27;
                daringpaddedpants_hib.Id_nb = "daringpaddedpants_hib";
                daringpaddedpants_hib.Hand = 0;
                daringpaddedpants_hib.Price = 0;
                daringpaddedpants_hib.IsPickable = true;
                daringpaddedpants_hib.IsDropable = true;
                daringpaddedpants_hib.IsTradable = true;
                daringpaddedpants_hib.CanDropAsLoot = false;
                daringpaddedpants_hib.Color = 0;
                daringpaddedpants_hib.Bonus = 0; // default bonus
                daringpaddedpants_hib.ClearTemplateBonuses();
                daringpaddedpants_hib.SetTemplateBonuses(eProperty.Dexterity, 4);
                daringpaddedpants_hib.Effect = 0;
                daringpaddedpants_hib.DPS_AF = 6;
                daringpaddedpants_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringpaddedpants_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringpaddedpants_hib.SPD_ABS = 0;
                daringpaddedpants_hib.Realm = 0;
                daringpaddedpants_hib.MaxCount = 1;
                daringpaddedpants_hib.PackSize = 1;
                daringpaddedpants_hib.Extension = 0;
                daringpaddedpants_hib.Quality = 89;
                daringpaddedpants_hib.Condition = 50000;
                daringpaddedpants_hib.MaxCondition = 50000;
                daringpaddedpants_hib.Durability = 50000;
                daringpaddedpants_hib.MaxDurability = 50000;
                daringpaddedpants_hib.PoisonCharges = 0;
                daringpaddedpants_hib.PoisonMaxCharges = 0;
                daringpaddedpants_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringpaddedpants_hib.AddQuestObjectToDatabase();
            }
            daringpaddedsleeves_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringpaddedsleeves_hib");
            if (daringpaddedsleeves_hib == null)
            {
                daringpaddedsleeves_hib = new ItemTemplate();
                daringpaddedsleeves_hib.Name = "Daring Padded Sleeves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedsleeves_hib.Name + ", creating it ...");
                daringpaddedsleeves_hib.Level = 5;
                daringpaddedsleeves_hib.Weight = 12;
                daringpaddedsleeves_hib.Model = 380;
                daringpaddedsleeves_hib.Object_Type = 32;
                daringpaddedsleeves_hib.Item_Type = 28;
                daringpaddedsleeves_hib.Id_nb = "daringpaddedsleeves_hib";
                daringpaddedsleeves_hib.Hand = 0;
                daringpaddedsleeves_hib.Price = 0;
                daringpaddedsleeves_hib.IsPickable = true;
                daringpaddedsleeves_hib.IsDropable = true;
                daringpaddedsleeves_hib.IsTradable = true;
                daringpaddedsleeves_hib.CanDropAsLoot = false;
                daringpaddedsleeves_hib.Color = 0;
                daringpaddedsleeves_hib.Bonus = 0; // default bonus
                daringpaddedsleeves_hib.ClearTemplateBonuses();
                daringpaddedsleeves_hib.SetTemplateBonuses(eProperty.Piety, 4);
                daringpaddedsleeves_hib.Effect = 0;
                daringpaddedsleeves_hib.DPS_AF = 6;
                daringpaddedsleeves_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringpaddedsleeves_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringpaddedsleeves_hib.SPD_ABS = 0;
                daringpaddedsleeves_hib.Realm = 0;
                daringpaddedsleeves_hib.MaxCount = 1;
                daringpaddedsleeves_hib.PackSize = 1;
                daringpaddedsleeves_hib.Extension = 0;
                daringpaddedsleeves_hib.Quality = 89;
                daringpaddedsleeves_hib.Condition = 50000;
                daringpaddedsleeves_hib.MaxCondition = 50000;
                daringpaddedsleeves_hib.Durability = 50000;
                daringpaddedsleeves_hib.MaxDurability = 50000;
                daringpaddedsleeves_hib.PoisonCharges = 0;
                daringpaddedsleeves_hib.PoisonMaxCharges = 0;
                daringpaddedsleeves_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringpaddedsleeves_hib.AddQuestObjectToDatabase();
            }
            daringpaddedvest_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringpaddedvest_hib");
            if (daringpaddedvest_hib == null)
            {
                daringpaddedvest_hib = new ItemTemplate();
                daringpaddedvest_hib.Name = "Daring Padded Vest";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedvest_hib.Name + ", creating it ...");
                daringpaddedvest_hib.Level = 5;
                daringpaddedvest_hib.Weight = 20;
                daringpaddedvest_hib.Model = 378;
                daringpaddedvest_hib.Object_Type = 32;
                daringpaddedvest_hib.Item_Type = 25;
                daringpaddedvest_hib.Id_nb = "daringpaddedvest_hib";
                daringpaddedvest_hib.Hand = 0;
                daringpaddedvest_hib.Price = 0;
                daringpaddedvest_hib.IsPickable = true;
                daringpaddedvest_hib.IsDropable = true;
                daringpaddedvest_hib.IsTradable = true;
                daringpaddedvest_hib.CanDropAsLoot = false;
                daringpaddedvest_hib.Color = 0;
                daringpaddedvest_hib.Bonus = 0; // default bonus
                daringpaddedvest_hib.ClearTemplateBonuses();
                daringpaddedvest_hib.SetTemplateBonuses(eProperty.MaxHealth, 12);
                daringpaddedvest_hib.Effect = 0;
                daringpaddedvest_hib.DPS_AF = 6;
                daringpaddedvest_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringpaddedvest_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringpaddedvest_hib.SPD_ABS = 0;
                daringpaddedvest_hib.Realm = 0;
                daringpaddedvest_hib.MaxCount = 1;
                daringpaddedvest_hib.PackSize = 1;
                daringpaddedvest_hib.Extension = 0;
                daringpaddedvest_hib.Quality = 89;
                daringpaddedvest_hib.Condition = 50000;
                daringpaddedvest_hib.MaxCondition = 50000;
                daringpaddedvest_hib.Durability = 50000;
                daringpaddedvest_hib.MaxDurability = 50000;
                daringpaddedvest_hib.PoisonCharges = 0;
                daringpaddedvest_hib.PoisonMaxCharges = 0;
                daringpaddedvest_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringpaddedvest_hib.AddQuestObjectToDatabase();
            }
            daringleatherboots_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringleatherboots_hib");
            if (daringleatherboots_hib == null)
            {
                daringleatherboots_hib = new ItemTemplate();
                daringleatherboots_hib.Name = "Daring Leather Boots";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleatherboots_hib.Name + ", creating it ...");
                daringleatherboots_hib.Level = 5;
                daringleatherboots_hib.Weight = 16;
                daringleatherboots_hib.Model = 397;
                daringleatherboots_hib.Object_Type = 33;
                daringleatherboots_hib.Item_Type = 23;
                daringleatherboots_hib.Id_nb = "daringleatherboots_hib";
                daringleatherboots_hib.Hand = 0;
                daringleatherboots_hib.Price = 0;
                daringleatherboots_hib.IsPickable = true;
                daringleatherboots_hib.IsDropable = true;
                daringleatherboots_hib.IsTradable = true;
                daringleatherboots_hib.CanDropAsLoot = false;
                daringleatherboots_hib.Color = 0;
                daringleatherboots_hib.Bonus = 0; // default bonus
                daringleatherboots_hib.ClearTemplateBonuses();
                daringleatherboots_hib.SetTemplateBonuses(eProperty.Constitution, 4);
                daringleatherboots_hib.Effect = 0;
                daringleatherboots_hib.DPS_AF = 10;
                daringleatherboots_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringleatherboots_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringleatherboots_hib.SPD_ABS = 10;
                daringleatherboots_hib.Realm = 0;
                daringleatherboots_hib.MaxCount = 1;
                daringleatherboots_hib.PackSize = 1;
                daringleatherboots_hib.Extension = 0;
                daringleatherboots_hib.Quality = 89;
                daringleatherboots_hib.Condition = 50000;
                daringleatherboots_hib.MaxCondition = 50000;
                daringleatherboots_hib.Durability = 50000;
                daringleatherboots_hib.MaxDurability = 50000;
                daringleatherboots_hib.PoisonCharges = 0;
                daringleatherboots_hib.PoisonMaxCharges = 0;
                daringleatherboots_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringleatherboots_hib.AddQuestObjectToDatabase();
            }
            daringleathercap_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringleathercap_hib");
            if (daringleathercap_hib == null)
            {
                daringleathercap_hib = new ItemTemplate();
                daringleathercap_hib.Name = "Daring Leather Cap";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleathercap_hib.Name + ", creating it ...");
                daringleathercap_hib.Level = 5;
                daringleathercap_hib.Weight = 16;
                daringleathercap_hib.Model = 823;
                daringleathercap_hib.Object_Type = 33;
                daringleathercap_hib.Item_Type = 21;
                daringleathercap_hib.Id_nb = "daringleathercap_hib";
                daringleathercap_hib.Hand = 0;
                daringleathercap_hib.Price = 0;
                daringleathercap_hib.IsPickable = true;
                daringleathercap_hib.IsDropable = true;
                daringleathercap_hib.IsTradable = true;
                daringleathercap_hib.CanDropAsLoot = false;
                daringleathercap_hib.Color = 0;
                daringleathercap_hib.Bonus = 0; // default bonus
                daringleathercap_hib.ClearTemplateBonuses();
                daringleathercap_hib.SetTemplateBonuses(eProperty.Constitution, 4);
                daringleathercap_hib.Effect = 0;
                daringleathercap_hib.DPS_AF = 10;
                daringleathercap_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringleathercap_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringleathercap_hib.SPD_ABS = 10;
                daringleathercap_hib.Realm = 0;
                daringleathercap_hib.MaxCount = 1;
                daringleathercap_hib.PackSize = 1;
                daringleathercap_hib.Extension = 0;
                daringleathercap_hib.Quality = 89;
                daringleathercap_hib.Condition = 50000;
                daringleathercap_hib.MaxCondition = 50000;
                daringleathercap_hib.Durability = 50000;
                daringleathercap_hib.MaxDurability = 50000;
                daringleathercap_hib.PoisonCharges = 0;
                daringleathercap_hib.PoisonMaxCharges = 0;
                daringleathercap_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringleathercap_hib.AddQuestObjectToDatabase();
            }
            daringleathergloves_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringleathergloves_hib");
            if (daringleathergloves_hib == null)
            {
                daringleathergloves_hib = new ItemTemplate();
                daringleathergloves_hib.Name = "Daring Leather Gloves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleathergloves_hib.Name + ", creating it ...");
                daringleathergloves_hib.Level = 5;
                daringleathergloves_hib.Weight = 16;
                daringleathergloves_hib.Model = 396;
                daringleathergloves_hib.Object_Type = 33;
                daringleathergloves_hib.Item_Type = 22;
                daringleathergloves_hib.Id_nb = "daringleathergloves_hib";
                daringleathergloves_hib.Hand = 0;
                daringleathergloves_hib.Price = 0;
                daringleathergloves_hib.IsPickable = true;
                daringleathergloves_hib.IsDropable = true;
                daringleathergloves_hib.IsTradable = true;
                daringleathergloves_hib.CanDropAsLoot = false;
                daringleathergloves_hib.Color = 0;
                daringleathergloves_hib.Bonus = 0; // default bonus
                daringleathergloves_hib.ClearTemplateBonuses();
                daringleathergloves_hib.SetTemplateBonuses(eProperty.Dexterity, 4);
                daringleathergloves_hib.Effect = 0;
                daringleathergloves_hib.DPS_AF = 10;
                daringleathergloves_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringleathergloves_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringleathergloves_hib.SPD_ABS = 10;
                daringleathergloves_hib.Realm = 0;
                daringleathergloves_hib.MaxCount = 1;
                daringleathergloves_hib.PackSize = 1;
                daringleathergloves_hib.Extension = 0;
                daringleathergloves_hib.Quality = 89;
                daringleathergloves_hib.Condition = 50000;
                daringleathergloves_hib.MaxCondition = 50000;
                daringleathergloves_hib.Durability = 50000;
                daringleathergloves_hib.MaxDurability = 50000;
                daringleathergloves_hib.PoisonCharges = 0;
                daringleathergloves_hib.PoisonMaxCharges = 0;
                daringleathergloves_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringleathergloves_hib.AddQuestObjectToDatabase();
            }
            daringleatherjerkin_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringleatherjerkin_hib");
            if (daringleatherjerkin_hib == null)
            {
                daringleatherjerkin_hib = new ItemTemplate();
                daringleatherjerkin_hib.Name = "Daring Leather Jerkin";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleatherjerkin_hib.Name + ", creating it ...");
                daringleatherjerkin_hib.Level = 5;
                daringleatherjerkin_hib.Weight = 16;
                daringleatherjerkin_hib.Model = 393;
                daringleatherjerkin_hib.Object_Type = 33;
                daringleatherjerkin_hib.Item_Type = 25;
                daringleatherjerkin_hib.Id_nb = "daringleatherjerkin_hib";
                daringleatherjerkin_hib.Hand = 0;
                daringleatherjerkin_hib.Price = 0;
                daringleatherjerkin_hib.IsPickable = true;
                daringleatherjerkin_hib.IsDropable = true;
                daringleatherjerkin_hib.IsTradable = true;
                daringleatherjerkin_hib.CanDropAsLoot = false;
                daringleatherjerkin_hib.Color = 0;
                daringleatherjerkin_hib.Bonus = 0; // default bonus
                daringleatherjerkin_hib.ClearTemplateBonuses();
                daringleatherjerkin_hib.SetTemplateBonuses(eProperty.MaxHealth, 12);
                daringleatherjerkin_hib.Effect = 0;
                daringleatherjerkin_hib.DPS_AF = 10;
                daringleatherjerkin_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringleatherjerkin_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringleatherjerkin_hib.SPD_ABS = 10;
                daringleatherjerkin_hib.Realm = 0;
                daringleatherjerkin_hib.MaxCount = 1;
                daringleatherjerkin_hib.PackSize = 1;
                daringleatherjerkin_hib.Extension = 0;
                daringleatherjerkin_hib.Quality = 89;
                daringleatherjerkin_hib.Condition = 50000;
                daringleatherjerkin_hib.MaxCondition = 50000;
                daringleatherjerkin_hib.Durability = 50000;
                daringleatherjerkin_hib.MaxDurability = 50000;
                daringleatherjerkin_hib.PoisonCharges = 0;
                daringleatherjerkin_hib.PoisonMaxCharges = 0;
                daringleatherjerkin_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringleatherjerkin_hib.AddQuestObjectToDatabase();
            }
            daringleatherleggings_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringleatherleggings_hib");
            if (daringleatherleggings_hib == null)
            {
                daringleatherleggings_hib = new ItemTemplate();
                daringleatherleggings_hib.Name = "Daring Leather Leggings";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleatherleggings_hib.Name + ", creating it ...");
                daringleatherleggings_hib.Level = 5;
                daringleatherleggings_hib.Weight = 16;
                daringleatherleggings_hib.Model = 394;
                daringleatherleggings_hib.Object_Type = 33;
                daringleatherleggings_hib.Item_Type = 27;
                daringleatherleggings_hib.Id_nb = "daringleatherleggings_hib";
                daringleatherleggings_hib.Hand = 0;
                daringleatherleggings_hib.Price = 0;
                daringleatherleggings_hib.IsPickable = true;
                daringleatherleggings_hib.IsDropable = true;
                daringleatherleggings_hib.IsTradable = true;
                daringleatherleggings_hib.CanDropAsLoot = false;
                daringleatherleggings_hib.Color = 0;
                daringleatherleggings_hib.Bonus = 0; // default bonus
                daringleatherleggings_hib.ClearTemplateBonuses();
                daringleatherleggings_hib.SetTemplateBonuses(eProperty.Dexterity, 4);
                daringleatherleggings_hib.Effect = 0;
                daringleatherleggings_hib.DPS_AF = 10;
                daringleatherleggings_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringleatherleggings_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringleatherleggings_hib.SPD_ABS = 10;
                daringleatherleggings_hib.Realm = 0;
                daringleatherleggings_hib.MaxCount = 1;
                daringleatherleggings_hib.PackSize = 1;
                daringleatherleggings_hib.Extension = 0;
                daringleatherleggings_hib.Quality = 89;
                daringleatherleggings_hib.Condition = 50000;
                daringleatherleggings_hib.MaxCondition = 50000;
                daringleatherleggings_hib.Durability = 50000;
                daringleatherleggings_hib.MaxDurability = 50000;
                daringleatherleggings_hib.PoisonCharges = 0;
                daringleatherleggings_hib.PoisonMaxCharges = 0;
                daringleatherleggings_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringleatherleggings_hib.AddQuestObjectToDatabase();
            }
            daringleathersleeves_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringleathersleeves_hib");
            if (daringleathersleeves_hib == null)
            {
                daringleathersleeves_hib = new ItemTemplate();
                daringleathersleeves_hib.Name = "Daring Leather Sleeves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleathersleeves_hib.Name + ", creating it ...");
                daringleathersleeves_hib.Level = 5;
                daringleathersleeves_hib.Weight = 16;
                daringleathersleeves_hib.Model = 395;
                daringleathersleeves_hib.Object_Type = 33;
                daringleathersleeves_hib.Item_Type = 28;
                daringleathersleeves_hib.Id_nb = "daringleathersleeves_hib";
                daringleathersleeves_hib.Hand = 0;
                daringleathersleeves_hib.Price = 0;
                daringleathersleeves_hib.IsPickable = true;
                daringleathersleeves_hib.IsDropable = true;
                daringleathersleeves_hib.IsTradable = true;
                daringleathersleeves_hib.CanDropAsLoot = false;
                daringleathersleeves_hib.Color = 0;
                daringleathersleeves_hib.Bonus = 0; // default bonus
                daringleathersleeves_hib.ClearTemplateBonuses();
                daringleathersleeves_hib.SetTemplateBonuses(eProperty.Strength, 4);
                daringleathersleeves_hib.Effect = 0;
                daringleathersleeves_hib.DPS_AF = 10;
                daringleathersleeves_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringleathersleeves_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringleathersleeves_hib.SPD_ABS = 10;
                daringleathersleeves_hib.Realm = 0;
                daringleathersleeves_hib.MaxCount = 1;
                daringleathersleeves_hib.PackSize = 1;
                daringleathersleeves_hib.Extension = 0;
                daringleathersleeves_hib.Quality = 89;
                daringleathersleeves_hib.Condition = 50000;
                daringleathersleeves_hib.MaxCondition = 50000;
                daringleathersleeves_hib.Durability = 50000;
                daringleathersleeves_hib.MaxDurability = 50000;
                daringleathersleeves_hib.PoisonCharges = 0;
                daringleathersleeves_hib.PoisonMaxCharges = 0;
                daringleathersleeves_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringleathersleeves_hib.AddQuestObjectToDatabase();
            }
            daringstuddedboots_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringstuddedboots_hib");
            if (daringstuddedboots_hib == null)
            {
                daringstuddedboots_hib = new ItemTemplate();
                daringstuddedboots_hib.Name = "Daring Studded Boots";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedboots_hib.Name + ", creating it ...");
                daringstuddedboots_hib.Level = 5;
                daringstuddedboots_hib.Weight = 24;
                daringstuddedboots_hib.Model = 387;
                daringstuddedboots_hib.Object_Type = 34;
                daringstuddedboots_hib.Item_Type = 23;
                daringstuddedboots_hib.Id_nb = "daringstuddedboots_hib";
                daringstuddedboots_hib.Hand = 0;
                daringstuddedboots_hib.Price = 0;
                daringstuddedboots_hib.IsPickable = true;
                daringstuddedboots_hib.IsDropable = true;
                daringstuddedboots_hib.IsTradable = true;
                daringstuddedboots_hib.CanDropAsLoot = false;
                daringstuddedboots_hib.Color = 0;
                daringstuddedboots_hib.Bonus = 0; // default bonus
                daringstuddedboots_hib.ClearTemplateBonuses();
                daringstuddedboots_hib.SetTemplateBonuses(eProperty.Constitution, 4);
                daringstuddedboots_hib.Effect = 0;
                daringstuddedboots_hib.DPS_AF = 12;
                daringstuddedboots_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringstuddedboots_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringstuddedboots_hib.SPD_ABS = 19;
                daringstuddedboots_hib.Realm = 0;
                daringstuddedboots_hib.MaxCount = 1;
                daringstuddedboots_hib.PackSize = 1;
                daringstuddedboots_hib.Extension = 0;
                daringstuddedboots_hib.Quality = 89;
                daringstuddedboots_hib.Condition = 50000;
                daringstuddedboots_hib.MaxCondition = 50000;
                daringstuddedboots_hib.Durability = 50000;
                daringstuddedboots_hib.MaxDurability = 50000;
                daringstuddedboots_hib.PoisonCharges = 0;
                daringstuddedboots_hib.PoisonMaxCharges = 0;
                daringstuddedboots_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringstuddedboots_hib.AddQuestObjectToDatabase();
            }
            daringstuddedcap_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringstuddedcap_hib");
            if (daringstuddedcap_hib == null)
            {
                daringstuddedcap_hib = new ItemTemplate();
                daringstuddedcap_hib.Name = "Daring Studded Cap";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedcap_hib.Name + ", creating it ...");
                daringstuddedcap_hib.Level = 5;
                daringstuddedcap_hib.Weight = 24;
                daringstuddedcap_hib.Model = 824;
                daringstuddedcap_hib.Object_Type = 34;
                daringstuddedcap_hib.Item_Type = 21;
                daringstuddedcap_hib.Id_nb = "daringstuddedcap_hib";
                daringstuddedcap_hib.Hand = 0;
                daringstuddedcap_hib.Price = 0;
                daringstuddedcap_hib.IsPickable = true;
                daringstuddedcap_hib.IsDropable = true;
                daringstuddedcap_hib.IsTradable = true;
                daringstuddedcap_hib.CanDropAsLoot = false;
                daringstuddedcap_hib.Color = 0;
                daringstuddedcap_hib.Bonus = 0; // default bonus
                daringstuddedcap_hib.ClearTemplateBonuses();
                daringstuddedcap_hib.SetTemplateBonuses(eProperty.Constitution, 4);
                daringstuddedcap_hib.Effect = 0;
                daringstuddedcap_hib.DPS_AF = 12;
                daringstuddedcap_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringstuddedcap_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringstuddedcap_hib.SPD_ABS = 19;
                daringstuddedcap_hib.Realm = 0;
                daringstuddedcap_hib.MaxCount = 1;
                daringstuddedcap_hib.PackSize = 1;
                daringstuddedcap_hib.Extension = 0;
                daringstuddedcap_hib.Quality = 89;
                daringstuddedcap_hib.Condition = 50000;
                daringstuddedcap_hib.MaxCondition = 50000;
                daringstuddedcap_hib.Durability = 50000;
                daringstuddedcap_hib.MaxDurability = 50000;
                daringstuddedcap_hib.PoisonCharges = 0;
                daringstuddedcap_hib.PoisonMaxCharges = 0;
                daringstuddedcap_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringstuddedcap_hib.AddQuestObjectToDatabase();
            }
            daringstuddedgloves_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringstuddedgloves_hib");
            if (daringstuddedgloves_hib == null)
            {
                daringstuddedgloves_hib = new ItemTemplate();
                daringstuddedgloves_hib.Name = "Daring Studded Gloves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedgloves_hib.Name + ", creating it ...");
                daringstuddedgloves_hib.Level = 5;
                daringstuddedgloves_hib.Weight = 24;
                daringstuddedgloves_hib.Model = 386;
                daringstuddedgloves_hib.Object_Type = 34;
                daringstuddedgloves_hib.Item_Type = 22;
                daringstuddedgloves_hib.Id_nb = "daringstuddedgloves_hib";
                daringstuddedgloves_hib.Hand = 0;
                daringstuddedgloves_hib.Price = 0;
                daringstuddedgloves_hib.IsPickable = true;
                daringstuddedgloves_hib.IsDropable = true;
                daringstuddedgloves_hib.IsTradable = true;
                daringstuddedgloves_hib.CanDropAsLoot = false;
                daringstuddedgloves_hib.Color = 0;
                daringstuddedgloves_hib.Bonus = 0; // default bonus
                daringstuddedgloves_hib.ClearTemplateBonuses();
                daringstuddedgloves_hib.SetTemplateBonuses(eProperty.Quickness, 4);
                daringstuddedgloves_hib.Effect = 0;
                daringstuddedgloves_hib.DPS_AF = 12;
                daringstuddedgloves_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringstuddedgloves_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringstuddedgloves_hib.SPD_ABS = 19;
                daringstuddedgloves_hib.Realm = 0;
                daringstuddedgloves_hib.MaxCount = 1;
                daringstuddedgloves_hib.PackSize = 1;
                daringstuddedgloves_hib.Extension = 0;
                daringstuddedgloves_hib.Quality = 89;
                daringstuddedgloves_hib.Condition = 50000;
                daringstuddedgloves_hib.MaxCondition = 50000;
                daringstuddedgloves_hib.Durability = 50000;
                daringstuddedgloves_hib.MaxDurability = 50000;
                daringstuddedgloves_hib.PoisonCharges = 0;
                daringstuddedgloves_hib.PoisonMaxCharges = 0;
                daringstuddedgloves_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringstuddedgloves_hib.AddQuestObjectToDatabase();
            }
            daringstuddedjerkin_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringstuddedjerkin_hib");
            if (daringstuddedjerkin_hib == null)
            {
                daringstuddedjerkin_hib = new ItemTemplate();
                daringstuddedjerkin_hib.Name = "Daring Studded Jerkin";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedjerkin_hib.Name + ", creating it ...");
                daringstuddedjerkin_hib.Level = 5;
                daringstuddedjerkin_hib.Weight = 60;
                daringstuddedjerkin_hib.Model = 383;
                daringstuddedjerkin_hib.Object_Type = 34;
                daringstuddedjerkin_hib.Item_Type = 25;
                daringstuddedjerkin_hib.Id_nb = "daringstuddedjerkin_hib";
                daringstuddedjerkin_hib.Hand = 0;
                daringstuddedjerkin_hib.Price = 0;
                daringstuddedjerkin_hib.IsPickable = true;
                daringstuddedjerkin_hib.IsDropable = true;
                daringstuddedjerkin_hib.IsTradable = true;
                daringstuddedjerkin_hib.CanDropAsLoot = false;
                daringstuddedjerkin_hib.Color = 0;
                daringstuddedjerkin_hib.Bonus = 0; // default bonus
                daringstuddedjerkin_hib.ClearTemplateBonuses();
                daringstuddedjerkin_hib.SetTemplateBonuses(eProperty.MaxHealth, 12);
                daringstuddedjerkin_hib.Effect = 0;
                daringstuddedjerkin_hib.DPS_AF = 12;
                daringstuddedjerkin_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringstuddedjerkin_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringstuddedjerkin_hib.SPD_ABS = 19;
                daringstuddedjerkin_hib.Realm = 0;
                daringstuddedjerkin_hib.MaxCount = 1;
                daringstuddedjerkin_hib.PackSize = 1;
                daringstuddedjerkin_hib.Extension = 0;
                daringstuddedjerkin_hib.Quality = 89;
                daringstuddedjerkin_hib.Condition = 50000;
                daringstuddedjerkin_hib.MaxCondition = 50000;
                daringstuddedjerkin_hib.Durability = 50000;
                daringstuddedjerkin_hib.MaxDurability = 50000;
                daringstuddedjerkin_hib.PoisonCharges = 0;
                daringstuddedjerkin_hib.PoisonMaxCharges = 0;
                daringstuddedjerkin_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringstuddedjerkin_hib.AddQuestObjectToDatabase();
            }
            daringstuddedleggings_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringstuddedleggings_hib");
            if (daringstuddedleggings_hib == null)
            {
                daringstuddedleggings_hib = new ItemTemplate();
                daringstuddedleggings_hib.Name = "Daring Studded Leggings";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedleggings_hib.Name + ", creating it ...");
                daringstuddedleggings_hib.Level = 5;
                daringstuddedleggings_hib.Weight = 42;
                daringstuddedleggings_hib.Model = 384;
                daringstuddedleggings_hib.Object_Type = 34;
                daringstuddedleggings_hib.Item_Type = 27;
                daringstuddedleggings_hib.Id_nb = "daringstuddedleggings_hib";
                daringstuddedleggings_hib.Hand = 0;
                daringstuddedleggings_hib.Price = 0;
                daringstuddedleggings_hib.IsPickable = true;
                daringstuddedleggings_hib.IsDropable = true;
                daringstuddedleggings_hib.IsTradable = true;
                daringstuddedleggings_hib.CanDropAsLoot = false;
                daringstuddedleggings_hib.Color = 0;
                daringstuddedleggings_hib.Bonus = 0; // default bonus
                daringstuddedleggings_hib.ClearTemplateBonuses();
                daringstuddedleggings_hib.SetTemplateBonuses(eProperty.Quickness, 4);
                daringstuddedleggings_hib.Effect = 0;
                daringstuddedleggings_hib.DPS_AF = 12;
                daringstuddedleggings_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringstuddedleggings_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringstuddedleggings_hib.SPD_ABS = 19;
                daringstuddedleggings_hib.Realm = 0;
                daringstuddedleggings_hib.MaxCount = 1;
                daringstuddedleggings_hib.PackSize = 1;
                daringstuddedleggings_hib.Extension = 0;
                daringstuddedleggings_hib.Quality = 89;
                daringstuddedleggings_hib.Condition = 50000;
                daringstuddedleggings_hib.MaxCondition = 50000;
                daringstuddedleggings_hib.Durability = 50000;
                daringstuddedleggings_hib.MaxDurability = 50000;
                daringstuddedleggings_hib.PoisonCharges = 0;
                daringstuddedleggings_hib.PoisonMaxCharges = 0;
                daringstuddedleggings_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringstuddedleggings_hib.AddQuestObjectToDatabase();
            }
            daringstuddedsleeves_hib = GameServer.Database.FindObjectByKey<ItemTemplate>("daringstuddedsleeves_hib");
            if (daringstuddedsleeves_hib == null)
            {
                daringstuddedsleeves_hib = new ItemTemplate();
                daringstuddedsleeves_hib.Name = "Daring Studded Sleeves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedsleeves_hib.Name + ", creating it ...");
                daringstuddedsleeves_hib.Level = 5;
                daringstuddedsleeves_hib.Weight = 36;
                daringstuddedsleeves_hib.Model = 385;
                daringstuddedsleeves_hib.Object_Type = 34;
                daringstuddedsleeves_hib.Item_Type = 28;
                daringstuddedsleeves_hib.Id_nb = "daringstuddedsleeves_hib";
                daringstuddedsleeves_hib.Hand = 0;
                daringstuddedsleeves_hib.Price = 0;
                daringstuddedsleeves_hib.IsPickable = true;
                daringstuddedsleeves_hib.IsDropable = true;
                daringstuddedsleeves_hib.IsTradable = true;
                daringstuddedsleeves_hib.CanDropAsLoot = false;
                daringstuddedsleeves_hib.Color = 0;
                daringstuddedsleeves_hib.Bonus = 0; // default bonus
                daringstuddedsleeves_hib.ClearTemplateBonuses();
                daringstuddedsleeves_hib.SetTemplateBonuses(eProperty.Strength, 4);
                daringstuddedsleeves_hib.Effect = 0;
                daringstuddedsleeves_hib.DPS_AF = 12;
                daringstuddedsleeves_hib.SetTemplateUseSpells(new [] { 0, 0 }, new []{ 0, 0 });
                daringstuddedsleeves_hib.SetTemplateProcSpells(new [] { 0, 0 }, new byte[]{ 0, 0 });
                daringstuddedsleeves_hib.SPD_ABS = 19;
                daringstuddedsleeves_hib.Realm = 0;
                daringstuddedsleeves_hib.MaxCount = 1;
                daringstuddedsleeves_hib.PackSize = 1;
                daringstuddedsleeves_hib.Extension = 0;
                daringstuddedsleeves_hib.Quality = 89;
                daringstuddedsleeves_hib.Condition = 50000;
                daringstuddedsleeves_hib.MaxCondition = 50000;
                daringstuddedsleeves_hib.Durability = 50000;
                daringstuddedsleeves_hib.MaxDurability = 50000;
                daringstuddedsleeves_hib.PoisonCharges = 0;
                daringstuddedsleeves_hib.PoisonMaxCharges = 0;
                daringstuddedsleeves_hib.PoisonSpellID = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                
                daringstuddedsleeves_hib.AddQuestObjectToDatabase();
            }


            #endregion

            #region defineAreas
            Hib_Statue_Area = WorldMgr.GetRegion(Hib_Statue.RegionID).AddArea(new Area.Circle("", Hib_Statue.X, Hib_Statue.Y, Hib_Statue.Z, 500));
            Hib_Statue_Area.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterStatueArea));

            #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(Charles, GameLivingEvent.Interact, new DOLEventHandler(TalkToCharles));
            GameEventMgr.AddHandler(Charles, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToCharles));

            Charles.AddQuestToGive(typeof(childsplay));
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" (Hib) initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            Hib_Statue_Area.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterStatueArea));
            WorldMgr.GetRegion(Hib_Statue.RegionID).RemoveArea(Hib_Statue_Area);
            if (Charles == null)
                return;
            GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.RemoveHandler(Charles, GameLivingEvent.Interact, new DOLEventHandler(TalkToCharles));
            GameEventMgr.RemoveHandler(Charles, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToCharles));

            Charles.RemoveQuestToGive(typeof(childsplay));
        }

        protected static void TalkToCharles(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (Charles.CanGiveQuest(typeof(childsplay), player) <= 0)
                return;

            childsplay quest = player.IsDoingQuest(typeof(childsplay)) as childsplay;

            Charles.TurnTo(player);

            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    Charles.SayTo(player, LanguageMgr.GetTranslation(player.Client, "ChildsPlay.TalkToCharles.Text1", player.CharacterClass.BaseName));
                    return;
                }
                if (quest.Step == 2)
                {
                    Charles.SayTo(player, LanguageMgr.GetTranslation(player.Client, "ChildsPlay.TalkToCharles.Text2"));

                    //k109:  Until I can get the quest dialog from live, I reward based on class, feel free to edit.
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "ChildsPlay.TalkToCharles.Text3", questTitle), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Guardian"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedboots_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedcap_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedgloves_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedjerkin_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedleggings_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedsleeves_hib);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Magician"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedboots_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedcap_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedgloves_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedpants_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedsleeves_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedvest_hib);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Forester"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedboots_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedcap_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedgloves_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedpants_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedsleeves_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedvest_hib);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Stalker"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringleatherboots_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringleathercap_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringleathergloves_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherjerkin_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherleggings_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringleathersleeves_hib);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Naturalist"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringleatherboots_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringleathercap_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringleathergloves_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherjerkin_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherleggings_hib);
                        GiveItem(Charles, quest.m_questPlayer, daringleathersleeves_hib);
                        quest.FinishQuest();
                    }
                }
            }
            else if (e == GameLivingEvent.WhisperReceive)
            {
                WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
                if (quest == null)
                {
                    //k109:  This is the "old" way of doing quests, by clicking on keywords, but have to use this until I can get the new quest dialog window.
                    String lowerCase = wArgs.Text.ToLower();

                    if (lowerCase == LanguageMgr.GetTranslation(player.Client, "ChildsPlay.TalkToCharles.CaseText1"))
                    {
                        Charles.SayTo(player, LanguageMgr.GetTranslation(player.Client, "ChildsPlay.TalkToCharles.Text4"));
                    }

                    else if (lowerCase == LanguageMgr.GetTranslation(player.Client, "ChildsPlay.TalkToCharles.CaseText2"))
                    {
                        Charles.SayTo(player, LanguageMgr.GetTranslation(player.Client, "ChildsPlay.TalkToCharles.Text5"));
                        player.Out.SendQuestSubscribeCommand(Charles, QuestMgr.GetIDForQuestType(typeof(childsplay)), LanguageMgr.GetTranslation(player.Client, "ChildsPlay.TalkToCharles.Text6"));
                    }
                    else if (lowerCase == LanguageMgr.GetTranslation(player.Client, "ChildsPlay.TalkToCharles.CaseText3"))
                    {
                        player.Out.SendCustomDialog(LanguageMgr.GetTranslation(player.Client, "ChildsPlay.TalkToCharles.Text7"), new CustomDialogResponse(CheckPlayerAbortQuest));
                    }
                }
            }
        }
        /// <summary>
        /// This method checks if a player qualifies for this quest
        /// </summary>
        /// <returns>true if qualified, false if not</returns>
        public override bool CheckQuestQualification(GamePlayer player)
        {
            if (player.IsDoingQuest(typeof(childsplay)) != null)
                return true;

            if (player.Level < minimumLevel || player.Level > maximumLevel)
                return false;

            return true;
        }

        private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
        {
            childsplay quest = player.IsDoingQuest(typeof(childsplay)) as childsplay;

            if (quest == null)
                return;

            if (response == 0x00)
            {
                SendSystemMessage(player, LanguageMgr.GetTranslation(player.Client, "ChildsPlay.CheckPlayerAbortQuest.Text1"));
            }
            else
            {
                SendSystemMessage(player, LanguageMgr.GetTranslation(player.Client, "ChildsPlay.CheckPlayerAbortQuest.Text2", questTitle));
                quest.AbortQuest();
            }
        }

        protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
        {
            QuestEventArgs qargs = args as QuestEventArgs;
            if (qargs == null)
                return;

            if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(childsplay)))
                return;

            if (e == GamePlayerEvent.AcceptQuest)
                CheckPlayerAcceptQuest(qargs.Player, 0x01);
            else if (e == GamePlayerEvent.DeclineQuest)
                CheckPlayerAcceptQuest(qargs.Player, 0x00);
        }
        protected static void PlayerEnterStatueArea(DOLEvent e, object sender, EventArgs args)
        {
            AreaEventArgs aargs = args as AreaEventArgs;
            GamePlayer player = aargs.GameObject as GamePlayer;
            childsplay quest = player.IsDoingQuest(typeof(childsplay)) as childsplay;

            if (quest != null && quest.Step == 1)
            {
                player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, LanguageMgr.GetTranslation(player.Client, "ChildsPlay.PlayerEnterStatueArea.Text1"));
                quest.Step = 2;
            }
        }
        private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
        {
            if (Charles.CanGiveQuest(typeof(childsplay), player) <= 0)
                return;

            if (player.IsDoingQuest(typeof(childsplay)) != null)
                return;

            if (response == 0x00)
            {
                SendReply(player, LanguageMgr.GetTranslation(player.Client, "ChildsPlay.CheckPlayerAcceptQuest.Text1"));
            }
            else
            {
                if (!Charles.GiveQuest(typeof(childsplay), player, 1))
                    return;
                SendReply(player, LanguageMgr.GetTranslation(player.Client, "ChildsPlay.CheckPlayerAcceptQuest.Text2"));
            }
        }

        public override string Name
        {
            get { return questTitle; }
        }

        public override string Description
        {
            get
            {
                //k109: Update each time a kill is made.
                if (Step == 1)
                {
                    return LanguageMgr.GetTranslation(m_questPlayer.Client, "Hib.ChildsPlay.Description.Text1");
                }
                else if (Step == 2)
                {
                    return LanguageMgr.GetTranslation(m_questPlayer.Client, "Hib.ChildsPlay.Description.Text2");
                }

                return base.Description;
            }
        }
        public override void AbortQuest()
        {
            base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...
        }

        public override void FinishQuest()
        {
            base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

            //k109: xp and money Rewards...
			m_questPlayer.GainExperience(GameLiving.eXPSource.Quest, 2, true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, 67), LanguageMgr.GetTranslation(m_questPlayer.Client, "ChildsPlay.FinishQuest.Text1"));
            InventoryLogging.LogInventoryAction("(QUEST;" + Name + ")", m_questPlayer, eInventoryActionType.Quest, 67);
        }
    }
}
