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

namespace DOL.GS.Quests.Midgard
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
        private static ItemTemplate daringpaddedboots = null;
        private static ItemTemplate daringpaddedcap = null;
        private static ItemTemplate daringpaddedgloves = null;
        private static ItemTemplate daringpaddedpants = null;
        private static ItemTemplate daringpaddedsleeves = null;
        private static ItemTemplate daringpaddedvest = null;
        private static ItemTemplate daringleatherboots = null;
        private static ItemTemplate daringleathercap = null;
        private static ItemTemplate daringleathergloves = null;
        private static ItemTemplate daringleatherjerkin = null;
        private static ItemTemplate daringleatherleggings = null;
        private static ItemTemplate daringleathersleeves = null;
        private static ItemTemplate daringstuddedboots = null;
        private static ItemTemplate daringstuddedcap = null;
        private static ItemTemplate daringstuddedgloves = null;
        private static ItemTemplate daringstuddedjerkin = null;
        private static ItemTemplate daringstuddedleggings = null;
        private static ItemTemplate daringstuddedsleeves = null;
        //private static AbstractArea Statua = null;
        private static GameLocation Mid_Statue = new GameLocation("Childs Play (Mid)", 489, 27580, 40006, 14483);

        private static IArea Mid_Statue_Area = null;

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
                log.Info("Quest \"" + questTitle + "\" (Mid) initializing ...");

            #region defineNPCs
            GameNPC[] npcs;

            npcs = WorldMgr.GetNPCsByName("Charles", (eRealm)2);
            if (npcs.Length == 0)
            {
                Charles = new DOL.GS.GameNPC();
                Charles.Model = 142;
                Charles.Name = "Charles";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Charles.Name + ", creating ...");
                //Charles.GuildName = "Part of " + questTitle + " Quest";
                Charles.Realm = eRealm.Midgard;
                Charles.CurrentRegionID = 100;
                Charles.Size = 37;
                Charles.Level = 1;
                Charles.MaxSpeedBase = 191;
                Charles.Faction = FactionMgr.GetFactionByID(0);
                Charles.X = 803946;
                Charles.Y = 727221;
                Charles.Z = 4680;
                Charles.Heading = 1592;
                Charles.RespawnInterval = -1;
                Charles.BodyType = 0;


                StandardMobBrain brain = new StandardMobBrain();
                brain.AggroLevel = 0;
                brain.AggroRange = 500;
                Charles.SetOwnBrain(brain);

                if (SAVE_INTO_DATABASE)
                    Charles.SaveIntoDatabase();

                Charles.AddToWorld();

            }
            else
            {
                Charles = npcs[0];
            }


            #endregion

            #region defineItems

            daringpaddedboots = GameServer.Database.FindObjectByKey<ItemTemplate>("daringpaddedboots");
            if (daringpaddedboots == null)
            {
                daringpaddedboots = new ItemTemplate();
                daringpaddedboots.Name = "Daring Padded Boots";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedboots.Name + ", creating it ...");
                daringpaddedboots.Level = 5;
                daringpaddedboots.Weight = 8;
                daringpaddedboots.Model = 249;
                daringpaddedboots.Object_Type = 32;
                daringpaddedboots.Item_Type = 23;
                daringpaddedboots.Id_nb = "daringpaddedboots";
                daringpaddedboots.Hand = 0;
                daringpaddedboots.Price = 0;
                daringpaddedboots.IsPickable = true;
                daringpaddedboots.IsDropable = true;
                daringpaddedboots.IsTradable = true;
                daringpaddedboots.CanDropAsLoot = false;
                daringpaddedboots.Color = 0;
                daringpaddedboots.Bonus = 0; // default bonus				
                daringpaddedboots.Bonus1 = 4;
                daringpaddedboots.Bonus1Type = (int)3;
                daringpaddedboots.Bonus2 = 0;
                daringpaddedboots.Bonus2Type = (int)0;
                daringpaddedboots.Bonus3 = 0;
                daringpaddedboots.Bonus3Type = (int)0;
                daringpaddedboots.Bonus4 = 0;
                daringpaddedboots.Bonus4Type = (int)0;
                daringpaddedboots.Bonus5 = 0;
                daringpaddedboots.Bonus5Type = (int)0;
                daringpaddedboots.Bonus6 = 0;
                daringpaddedboots.Bonus6Type = (int)0;
                daringpaddedboots.Bonus7 = 0;
                daringpaddedboots.Bonus7Type = (int)0;
                daringpaddedboots.Bonus8 = 0;
                daringpaddedboots.Bonus8Type = (int)0;
                daringpaddedboots.Bonus9 = 0;
                daringpaddedboots.Bonus9Type = (int)0;
                daringpaddedboots.Bonus10 = 0;
                daringpaddedboots.Bonus10Type = (int)0;
                daringpaddedboots.ExtraBonus = 0;
                daringpaddedboots.ExtraBonusType = (int)0;
                daringpaddedboots.Effect = 0;
                daringpaddedboots.DPS_AF = 6;
                daringpaddedboots.Charges = 0;
                daringpaddedboots.MaxCharges = 0;
                daringpaddedboots.SpellID = 0;
                daringpaddedboots.ProcSpellID = 0;
                daringpaddedboots.SPD_ABS = 0;
                daringpaddedboots.Realm = 0;
                daringpaddedboots.MaxCount = 1;
                daringpaddedboots.PackSize = 1;
                daringpaddedboots.Extension = 0;
                daringpaddedboots.Quality = 89;
                daringpaddedboots.Condition = 50000;
                daringpaddedboots.MaxCondition = 50000;
                daringpaddedboots.Durability = 50000;
                daringpaddedboots.MaxDurability = 50000;
                daringpaddedboots.PoisonCharges = 0;
                daringpaddedboots.PoisonMaxCharges = 0;
                daringpaddedboots.PoisonSpellID = 0;
                daringpaddedboots.ProcSpellID1 = 0;
                daringpaddedboots.SpellID1 = 0;
                daringpaddedboots.MaxCharges1 = 0;
                daringpaddedboots.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringpaddedboots);
            }
            daringpaddedcap = GameServer.Database.FindObjectByKey<ItemTemplate>("daringpaddedcap");
            if (daringpaddedcap == null)
            {
                daringpaddedcap = new ItemTemplate();
                daringpaddedcap.Name = "Daring Padded Cap";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedcap.Name + ", creating it ...");
                daringpaddedcap.Level = 5;
                daringpaddedcap.Weight = 8;
                daringpaddedcap.Model = 825;
                daringpaddedcap.Object_Type = 32;
                daringpaddedcap.Item_Type = 21;
                daringpaddedcap.Id_nb = "daringpaddedcap";
                daringpaddedcap.Hand = 0;
                daringpaddedcap.Price = 0;
                daringpaddedcap.IsPickable = true;
                daringpaddedcap.IsDropable = true;
                daringpaddedcap.IsTradable = true;
                daringpaddedcap.CanDropAsLoot = false;
                daringpaddedcap.Color = 0;
                daringpaddedcap.Bonus = 0; // default bonus				
                daringpaddedcap.Bonus1 = 4;
                daringpaddedcap.Bonus1Type = (int)2;
                daringpaddedcap.Bonus2 = 0;
                daringpaddedcap.Bonus2Type = (int)0;
                daringpaddedcap.Bonus3 = 0;
                daringpaddedcap.Bonus3Type = (int)0;
                daringpaddedcap.Bonus4 = 0;
                daringpaddedcap.Bonus4Type = (int)0;
                daringpaddedcap.Bonus5 = 0;
                daringpaddedcap.Bonus5Type = (int)0;
                daringpaddedcap.Bonus6 = 0;
                daringpaddedcap.Bonus6Type = (int)0;
                daringpaddedcap.Bonus7 = 0;
                daringpaddedcap.Bonus7Type = (int)0;
                daringpaddedcap.Bonus8 = 0;
                daringpaddedcap.Bonus8Type = (int)0;
                daringpaddedcap.Bonus9 = 0;
                daringpaddedcap.Bonus9Type = (int)0;
                daringpaddedcap.Bonus10 = 0;
                daringpaddedcap.Bonus10Type = (int)0;
                daringpaddedcap.ExtraBonus = 0;
                daringpaddedcap.ExtraBonusType = (int)0;
                daringpaddedcap.Effect = 0;
                daringpaddedcap.DPS_AF = 6;
                daringpaddedcap.Charges = 0;
                daringpaddedcap.MaxCharges = 0;
                daringpaddedcap.SpellID = 0;
                daringpaddedcap.ProcSpellID = 0;
                daringpaddedcap.SPD_ABS = 0;
                daringpaddedcap.Realm = 0;
                daringpaddedcap.MaxCount = 1;
                daringpaddedcap.PackSize = 1;
                daringpaddedcap.Extension = 0;
                daringpaddedcap.Quality = 89;
                daringpaddedcap.Condition = 50000;
                daringpaddedcap.MaxCondition = 50000;
                daringpaddedcap.Durability = 50000;
                daringpaddedcap.MaxDurability = 50000;
                daringpaddedcap.PoisonCharges = 0;
                daringpaddedcap.PoisonMaxCharges = 0;
                daringpaddedcap.PoisonSpellID = 0;
                daringpaddedcap.ProcSpellID1 = 0;
                daringpaddedcap.SpellID1 = 0;
                daringpaddedcap.MaxCharges1 = 0;
                daringpaddedcap.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringpaddedcap);
            }
            daringpaddedgloves = GameServer.Database.FindObjectByKey<ItemTemplate>("daringpaddedgloves");
            if (daringpaddedgloves == null)
            {
                daringpaddedgloves = new ItemTemplate();
                daringpaddedgloves.Name = "Daring Padded Gloves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedgloves.Name + ", creating it ...");
                daringpaddedgloves.Level = 5;
                daringpaddedgloves.Weight = 8;
                daringpaddedgloves.Model = 248;
                daringpaddedgloves.Object_Type = 32;
                daringpaddedgloves.Item_Type = 22;
                daringpaddedgloves.Id_nb = "daringpaddedgloves";
                daringpaddedgloves.Hand = 0;
                daringpaddedgloves.Price = 0;
                daringpaddedgloves.IsPickable = true;
                daringpaddedgloves.IsDropable = true;
                daringpaddedgloves.IsTradable = true;
                daringpaddedgloves.CanDropAsLoot = false;
                daringpaddedgloves.Color = 0;
                daringpaddedgloves.Bonus = 0; // default bonus				
                daringpaddedgloves.Bonus1 = 4;
                daringpaddedgloves.Bonus1Type = (int)3;
                daringpaddedgloves.Bonus2 = 0;
                daringpaddedgloves.Bonus2Type = (int)0;
                daringpaddedgloves.Bonus3 = 0;
                daringpaddedgloves.Bonus3Type = (int)0;
                daringpaddedgloves.Bonus4 = 0;
                daringpaddedgloves.Bonus4Type = (int)0;
                daringpaddedgloves.Bonus5 = 0;
                daringpaddedgloves.Bonus5Type = (int)0;
                daringpaddedgloves.Bonus6 = 0;
                daringpaddedgloves.Bonus6Type = (int)0;
                daringpaddedgloves.Bonus7 = 0;
                daringpaddedgloves.Bonus7Type = (int)0;
                daringpaddedgloves.Bonus8 = 0;
                daringpaddedgloves.Bonus8Type = (int)0;
                daringpaddedgloves.Bonus9 = 0;
                daringpaddedgloves.Bonus9Type = (int)0;
                daringpaddedgloves.Bonus10 = 0;
                daringpaddedgloves.Bonus10Type = (int)0;
                daringpaddedgloves.ExtraBonus = 0;
                daringpaddedgloves.ExtraBonusType = (int)0;
                daringpaddedgloves.Effect = 0;
                daringpaddedgloves.DPS_AF = 6;
                daringpaddedgloves.Charges = 0;
                daringpaddedgloves.MaxCharges = 0;
                daringpaddedgloves.SpellID = 0;
                daringpaddedgloves.ProcSpellID = 0;
                daringpaddedgloves.SPD_ABS = 0;
                daringpaddedgloves.Realm = 0;
                daringpaddedgloves.MaxCount = 1;
                daringpaddedgloves.PackSize = 1;
                daringpaddedgloves.Extension = 0;
                daringpaddedgloves.Quality = 89;
                daringpaddedgloves.Condition = 50000;
                daringpaddedgloves.MaxCondition = 50000;
                daringpaddedgloves.Durability = 50000;
                daringpaddedgloves.MaxDurability = 50000;
                daringpaddedgloves.PoisonCharges = 0;
                daringpaddedgloves.PoisonMaxCharges = 0;
                daringpaddedgloves.PoisonSpellID = 0;
                daringpaddedgloves.ProcSpellID1 = 0;
                daringpaddedgloves.SpellID1 = 0;
                daringpaddedgloves.MaxCharges1 = 0;
                daringpaddedgloves.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringpaddedgloves);
            }
            daringpaddedpants = GameServer.Database.FindObjectByKey<ItemTemplate>("daringpaddedpants");
            if (daringpaddedpants == null)
            {
                daringpaddedpants = new ItemTemplate();
                daringpaddedpants.Name = "Daring Padded Pants";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedpants.Name + ", creating it ...");
                daringpaddedpants.Level = 5;
                daringpaddedpants.Weight = 14;
                daringpaddedpants.Model = 246;
                daringpaddedpants.Object_Type = 32;
                daringpaddedpants.Item_Type = 27;
                daringpaddedpants.Id_nb = "daringpaddedpants";
                daringpaddedpants.Hand = 0;
                daringpaddedpants.Price = 0;
                daringpaddedpants.IsPickable = true;
                daringpaddedpants.IsDropable = true;
                daringpaddedpants.IsTradable = true;
                daringpaddedpants.CanDropAsLoot = false;
                daringpaddedpants.Color = 0;
                daringpaddedpants.Bonus = 0; // default bonus				
                daringpaddedpants.Bonus1 = 4;
                daringpaddedpants.Bonus1Type = (int)2;
                daringpaddedpants.Bonus2 = 0;
                daringpaddedpants.Bonus2Type = (int)0;
                daringpaddedpants.Bonus3 = 0;
                daringpaddedpants.Bonus3Type = (int)0;
                daringpaddedpants.Bonus4 = 0;
                daringpaddedpants.Bonus4Type = (int)0;
                daringpaddedpants.Bonus5 = 0;
                daringpaddedpants.Bonus5Type = (int)0;
                daringpaddedpants.Bonus6 = 0;
                daringpaddedpants.Bonus6Type = (int)0;
                daringpaddedpants.Bonus7 = 0;
                daringpaddedpants.Bonus7Type = (int)0;
                daringpaddedpants.Bonus8 = 0;
                daringpaddedpants.Bonus8Type = (int)0;
                daringpaddedpants.Bonus9 = 0;
                daringpaddedpants.Bonus9Type = (int)0;
                daringpaddedpants.Bonus10 = 0;
                daringpaddedpants.Bonus10Type = (int)0;
                daringpaddedpants.ExtraBonus = 0;
                daringpaddedpants.ExtraBonusType = (int)0;
                daringpaddedpants.Effect = 0;
                daringpaddedpants.DPS_AF = 6;
                daringpaddedpants.Charges = 0;
                daringpaddedpants.MaxCharges = 0;
                daringpaddedpants.SpellID = 0;
                daringpaddedpants.ProcSpellID = 0;
                daringpaddedpants.SPD_ABS = 0;
                daringpaddedpants.Realm = 0;
                daringpaddedpants.MaxCount = 1;
                daringpaddedpants.PackSize = 1;
                daringpaddedpants.Extension = 0;
                daringpaddedpants.Quality = 89;
                daringpaddedpants.Condition = 50000;
                daringpaddedpants.MaxCondition = 50000;
                daringpaddedpants.Durability = 50000;
                daringpaddedpants.MaxDurability = 50000;
                daringpaddedpants.PoisonCharges = 0;
                daringpaddedpants.PoisonMaxCharges = 0;
                daringpaddedpants.PoisonSpellID = 0;
                daringpaddedpants.ProcSpellID1 = 0;
                daringpaddedpants.SpellID1 = 0;
                daringpaddedpants.MaxCharges1 = 0;
                daringpaddedpants.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringpaddedpants);
            }
            daringpaddedsleeves = GameServer.Database.FindObjectByKey<ItemTemplate>("daringpaddedsleeves");
            if (daringpaddedsleeves == null)
            {
                daringpaddedsleeves = new ItemTemplate();
                daringpaddedsleeves.Name = "Daring Padded Sleeves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedsleeves.Name + ", creating it ...");
                daringpaddedsleeves.Level = 5;
                daringpaddedsleeves.Weight = 12;
                daringpaddedsleeves.Model = 247;
                daringpaddedsleeves.Object_Type = 32;
                daringpaddedsleeves.Item_Type = 28;
                daringpaddedsleeves.Id_nb = "daringpaddedsleeves";
                daringpaddedsleeves.Hand = 0;
                daringpaddedsleeves.Price= 0;
                daringpaddedsleeves.IsPickable = true;
                daringpaddedsleeves.IsDropable = true;
                daringpaddedsleeves.IsTradable = true;
                daringpaddedsleeves.CanDropAsLoot = false;
                daringpaddedsleeves.Color = 0;
                daringpaddedsleeves.Bonus = 0; // default bonus				
                daringpaddedsleeves.Bonus1 = 4;
                daringpaddedsleeves.Bonus1Type = (int)6;
                daringpaddedsleeves.Bonus2 = 0;
                daringpaddedsleeves.Bonus2Type = (int)0;
                daringpaddedsleeves.Bonus3 = 0;
                daringpaddedsleeves.Bonus3Type = (int)0;
                daringpaddedsleeves.Bonus4 = 0;
                daringpaddedsleeves.Bonus4Type = (int)0;
                daringpaddedsleeves.Bonus5 = 0;
                daringpaddedsleeves.Bonus5Type = (int)0;
                daringpaddedsleeves.Bonus6 = 0;
                daringpaddedsleeves.Bonus6Type = (int)0;
                daringpaddedsleeves.Bonus7 = 0;
                daringpaddedsleeves.Bonus7Type = (int)0;
                daringpaddedsleeves.Bonus8 = 0;
                daringpaddedsleeves.Bonus8Type = (int)0;
                daringpaddedsleeves.Bonus9 = 0;
                daringpaddedsleeves.Bonus9Type = (int)0;
                daringpaddedsleeves.Bonus10 = 0;
                daringpaddedsleeves.Bonus10Type = (int)0;
                daringpaddedsleeves.ExtraBonus = 0;
                daringpaddedsleeves.ExtraBonusType = (int)0;
                daringpaddedsleeves.Effect = 0;
                daringpaddedsleeves.DPS_AF = 6;
                daringpaddedsleeves.Charges = 0;
                daringpaddedsleeves.MaxCharges = 0;
                daringpaddedsleeves.SpellID = 0;
                daringpaddedsleeves.ProcSpellID = 0;
                daringpaddedsleeves.SPD_ABS = 0;
                daringpaddedsleeves.Realm = 0;
                daringpaddedsleeves.MaxCount = 1;
                daringpaddedsleeves.PackSize = 1;
                daringpaddedsleeves.Extension = 0;
                daringpaddedsleeves.Quality = 89;
                daringpaddedsleeves.Condition = 50000;
                daringpaddedsleeves.MaxCondition = 50000;
                daringpaddedsleeves.Durability = 50000;
                daringpaddedsleeves.MaxDurability = 50000;
                daringpaddedsleeves.PoisonCharges = 0;
                daringpaddedsleeves.PoisonMaxCharges = 0;
                daringpaddedsleeves.PoisonSpellID = 0;
                daringpaddedsleeves.ProcSpellID1 = 0;
                daringpaddedsleeves.SpellID1 = 0;
                daringpaddedsleeves.MaxCharges1 = 0;
                daringpaddedsleeves.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringpaddedsleeves);
            }
            daringpaddedvest = GameServer.Database.FindObjectByKey<ItemTemplate>("daringpaddedvest");
            if (daringpaddedvest == null)
            {
                daringpaddedvest = new ItemTemplate();
                daringpaddedvest.Name = "Daring Padded Vest";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedvest.Name + ", creating it ...");
                daringpaddedvest.Level = 5;
                daringpaddedvest.Weight = 20;
                daringpaddedvest.Model = 245;
                daringpaddedvest.Object_Type = 32;
                daringpaddedvest.Item_Type = 25;
                daringpaddedvest.Id_nb = "daringpaddedvest";
                daringpaddedvest.Hand = 0;
                daringpaddedvest.Price = 0;
                daringpaddedvest.IsPickable = true;
                daringpaddedvest.IsDropable = true;
                daringpaddedvest.IsTradable = true;
                daringpaddedvest.CanDropAsLoot = false;
                daringpaddedvest.Color = 0;
                daringpaddedvest.Bonus = 0; // default bonus				
                daringpaddedvest.Bonus1 = 12;
                daringpaddedvest.Bonus1Type = (int)10;
                daringpaddedvest.Bonus2 = 0;
                daringpaddedvest.Bonus2Type = (int)0;
                daringpaddedvest.Bonus3 = 0;
                daringpaddedvest.Bonus3Type = (int)0;
                daringpaddedvest.Bonus4 = 0;
                daringpaddedvest.Bonus4Type = (int)0;
                daringpaddedvest.Bonus5 = 0;
                daringpaddedvest.Bonus5Type = (int)0;
                daringpaddedvest.Bonus6 = 0;
                daringpaddedvest.Bonus6Type = (int)0;
                daringpaddedvest.Bonus7 = 0;
                daringpaddedvest.Bonus7Type = (int)0;
                daringpaddedvest.Bonus8 = 0;
                daringpaddedvest.Bonus8Type = (int)0;
                daringpaddedvest.Bonus9 = 0;
                daringpaddedvest.Bonus9Type = (int)0;
                daringpaddedvest.Bonus10 = 0;
                daringpaddedvest.Bonus10Type = (int)0;
                daringpaddedvest.ExtraBonus = 0;
                daringpaddedvest.ExtraBonusType = (int)0;
                daringpaddedvest.Effect = 0;
                daringpaddedvest.DPS_AF = 6;
                daringpaddedvest.Charges = 0;
                daringpaddedvest.MaxCharges = 0;
                daringpaddedvest.SpellID = 0;
                daringpaddedvest.ProcSpellID = 0;
                daringpaddedvest.SPD_ABS = 0;
                daringpaddedvest.Realm = 0;
                daringpaddedvest.MaxCount = 1;
                daringpaddedvest.PackSize = 1;
                daringpaddedvest.Extension = 0;
                daringpaddedvest.Quality = 89;
                daringpaddedvest.Condition = 50000;
                daringpaddedvest.MaxCondition = 50000;
                daringpaddedvest.Durability = 50000;
                daringpaddedvest.MaxDurability = 50000;
                daringpaddedvest.PoisonCharges = 0;
                daringpaddedvest.PoisonMaxCharges = 0;
                daringpaddedvest.PoisonSpellID = 0;
                daringpaddedvest.ProcSpellID1 = 0;
                daringpaddedvest.SpellID1 = 0;
                daringpaddedvest.MaxCharges1 = 0;
                daringpaddedvest.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringpaddedvest);
            }
            daringleatherboots = GameServer.Database.FindObjectByKey<ItemTemplate>("daringleatherboots");
            if (daringleatherboots == null)
            {
                daringleatherboots = new ItemTemplate();
                daringleatherboots.Name = "Daring Leather Boots";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleatherboots.Name + ", creating it ...");
                daringleatherboots.Level = 5;
                daringleatherboots.Weight = 16;
                daringleatherboots.Model = 284;
                daringleatherboots.Object_Type = 33;
                daringleatherboots.Item_Type = 23;
                daringleatherboots.Id_nb = "daringleatherboots";
                daringleatherboots.Hand = 0;
                daringleatherboots.Price = 0;
                daringleatherboots.IsPickable = true;
                daringleatherboots.IsDropable = true;
                daringleatherboots.IsTradable = true;
                daringleatherboots.CanDropAsLoot = false;
                daringleatherboots.Color = 0;
                daringleatherboots.Bonus = 0; // default bonus				
                daringleatherboots.Bonus1 = 4;
                daringleatherboots.Bonus1Type = (int)3;
                daringleatherboots.Bonus2 = 0;
                daringleatherboots.Bonus2Type = (int)0;
                daringleatherboots.Bonus3 = 0;
                daringleatherboots.Bonus3Type = (int)0;
                daringleatherboots.Bonus4 = 0;
                daringleatherboots.Bonus4Type = (int)0;
                daringleatherboots.Bonus5 = 0;
                daringleatherboots.Bonus5Type = (int)0;
                daringleatherboots.Bonus6 = 0;
                daringleatherboots.Bonus6Type = (int)0;
                daringleatherboots.Bonus7 = 0;
                daringleatherboots.Bonus7Type = (int)0;
                daringleatherboots.Bonus8 = 0;
                daringleatherboots.Bonus8Type = (int)0;
                daringleatherboots.Bonus9 = 0;
                daringleatherboots.Bonus9Type = (int)0;
                daringleatherboots.Bonus10 = 0;
                daringleatherboots.Bonus10Type = (int)0;
                daringleatherboots.ExtraBonus = 0;
                daringleatherboots.ExtraBonusType = (int)0;
                daringleatherboots.Effect = 0;
                daringleatherboots.DPS_AF = 10;
                daringleatherboots.Charges = 0;
                daringleatherboots.MaxCharges = 0;
                daringleatherboots.SpellID = 0;
                daringleatherboots.ProcSpellID = 0;
                daringleatherboots.SPD_ABS = 10;
                daringleatherboots.Realm = 0;
                daringleatherboots.MaxCount = 1;
                daringleatherboots.PackSize = 1;
                daringleatherboots.Extension = 0;
                daringleatherboots.Quality = 89;
                daringleatherboots.Condition = 50000;
                daringleatherboots.MaxCondition = 50000;
                daringleatherboots.Durability = 50000;
                daringleatherboots.MaxDurability = 50000;
                daringleatherboots.PoisonCharges = 0;
                daringleatherboots.PoisonMaxCharges = 0;
                daringleatherboots.PoisonSpellID = 0;
                daringleatherboots.ProcSpellID1 = 0;
                daringleatherboots.SpellID1 = 0;
                daringleatherboots.MaxCharges1 = 0;
                daringleatherboots.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringleatherboots);
            }
            daringleathercap = GameServer.Database.FindObjectByKey<ItemTemplate>("daringleathercap");
            if (daringleathercap == null)
            {
                daringleathercap = new ItemTemplate();
                daringleathercap.Name = "Daring Leather Cap";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleathercap.Name + ", creating it ...");
                daringleathercap.Level = 5;
                daringleathercap.Weight = 16;
                daringleathercap.Model = 337;
                daringleathercap.Object_Type = 33;
                daringleathercap.Item_Type = 21;
                daringleathercap.Id_nb = "daringleathercap";
                daringleathercap.Hand = 0;
                daringleathercap.Price = 0;
                daringleathercap.IsPickable = true;
                daringleathercap.IsDropable = true;
                daringleathercap.IsTradable = true;
                daringleathercap.CanDropAsLoot = false;
                daringleathercap.Color = 0;
                daringleathercap.Bonus = 0; // default bonus				
                daringleathercap.Bonus1 = 4;
                daringleathercap.Bonus1Type = (int)3;
                daringleathercap.Bonus2 = 0;
                daringleathercap.Bonus2Type = (int)0;
                daringleathercap.Bonus3 = 0;
                daringleathercap.Bonus3Type = (int)0;
                daringleathercap.Bonus4 = 0;
                daringleathercap.Bonus4Type = (int)0;
                daringleathercap.Bonus5 = 0;
                daringleathercap.Bonus5Type = (int)0;
                daringleathercap.Bonus6 = 0;
                daringleathercap.Bonus6Type = (int)0;
                daringleathercap.Bonus7 = 0;
                daringleathercap.Bonus7Type = (int)0;
                daringleathercap.Bonus8 = 0;
                daringleathercap.Bonus8Type = (int)0;
                daringleathercap.Bonus9 = 0;
                daringleathercap.Bonus9Type = (int)0;
                daringleathercap.Bonus10 = 0;
                daringleathercap.Bonus10Type = (int)0;
                daringleathercap.ExtraBonus = 0;
                daringleathercap.ExtraBonusType = (int)0;
                daringleathercap.Effect = 0;
                daringleathercap.DPS_AF = 10;
                daringleathercap.Charges = 0;
                daringleathercap.MaxCharges = 0;
                daringleathercap.SpellID = 0;
                daringleathercap.ProcSpellID = 0;
                daringleathercap.SPD_ABS = 10;
                daringleathercap.Realm = 0;
                daringleathercap.MaxCount = 1;
                daringleathercap.PackSize = 1;
                daringleathercap.Extension = 0;
                daringleathercap.Quality = 89;
                daringleathercap.Condition = 50000;
                daringleathercap.MaxCondition = 50000;
                daringleathercap.Durability = 50000;
                daringleathercap.MaxDurability = 50000;
                daringleathercap.PoisonCharges = 0;
                daringleathercap.PoisonMaxCharges = 0;
                daringleathercap.PoisonSpellID = 0;
                daringleathercap.ProcSpellID1 = 0;
                daringleathercap.SpellID1 = 0;
                daringleathercap.MaxCharges1 = 0;
                daringleathercap.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringleathercap);
            }
            daringleathergloves = GameServer.Database.FindObjectByKey<ItemTemplate>("daringleathergloves");
            if (daringleathergloves == null)
            {
                daringleathergloves = new ItemTemplate();
                daringleathergloves.Name = "Daring Leather Gloves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleathergloves.Name + ", creating it ...");
                daringleathergloves.Level = 5;
                daringleathergloves.Weight = 16;
                daringleathergloves.Model = 283;
                daringleathergloves.Object_Type = 33;
                daringleathergloves.Item_Type = 22;
                daringleathergloves.Id_nb = "daringleathergloves";
                daringleathergloves.Hand = 0;
                daringleathergloves.Price = 0;
                daringleathergloves.IsPickable = true;
                daringleathergloves.IsDropable = true;
                daringleathergloves.IsTradable = true;
                daringleathergloves.CanDropAsLoot = false;
                daringleathergloves.Color = 0;
                daringleathergloves.Bonus = 0; // default bonus				
                daringleathergloves.Bonus1 = 4;
                daringleathergloves.Bonus1Type = (int)2;
                daringleathergloves.Bonus2 = 0;
                daringleathergloves.Bonus2Type = (int)0;
                daringleathergloves.Bonus3 = 0;
                daringleathergloves.Bonus3Type = (int)0;
                daringleathergloves.Bonus4 = 0;
                daringleathergloves.Bonus4Type = (int)0;
                daringleathergloves.Bonus5 = 0;
                daringleathergloves.Bonus5Type = (int)0;
                daringleathergloves.Bonus6 = 0;
                daringleathergloves.Bonus6Type = (int)0;
                daringleathergloves.Bonus7 = 0;
                daringleathergloves.Bonus7Type = (int)0;
                daringleathergloves.Bonus8 = 0;
                daringleathergloves.Bonus8Type = (int)0;
                daringleathergloves.Bonus9 = 0;
                daringleathergloves.Bonus9Type = (int)0;
                daringleathergloves.Bonus10 = 0;
                daringleathergloves.Bonus10Type = (int)0;
                daringleathergloves.ExtraBonus = 0;
                daringleathergloves.ExtraBonusType = (int)0;
                daringleathergloves.Effect = 0;
                daringleathergloves.DPS_AF = 10;
                daringleathergloves.Charges = 0;
                daringleathergloves.MaxCharges = 0;
                daringleathergloves.SpellID = 0;
                daringleathergloves.ProcSpellID = 0;
                daringleathergloves.SPD_ABS = 10;
                daringleathergloves.Realm = 0;
                daringleathergloves.MaxCount = 1;
                daringleathergloves.PackSize = 1;
                daringleathergloves.Extension = 0;
                daringleathergloves.Quality = 89;
                daringleathergloves.Condition = 50000;
                daringleathergloves.MaxCondition = 50000;
                daringleathergloves.Durability = 50000;
                daringleathergloves.MaxDurability = 50000;
                daringleathergloves.PoisonCharges = 0;
                daringleathergloves.PoisonMaxCharges = 0;
                daringleathergloves.PoisonSpellID = 0;
                daringleathergloves.ProcSpellID1 = 0;
                daringleathergloves.SpellID1 = 0;
                daringleathergloves.MaxCharges1 = 0;
                daringleathergloves.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringleathergloves);
            }
            daringleatherjerkin = GameServer.Database.FindObjectByKey<ItemTemplate>("daringleatherjerkin");
            if (daringleatherjerkin == null)
            {
                daringleatherjerkin = new ItemTemplate();
                daringleatherjerkin.Name = "Daring Leather Jerkin";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleatherjerkin.Name + ", creating it ...");
                daringleatherjerkin.Level = 5;
                daringleatherjerkin.Weight = 16;
                daringleatherjerkin.Model = 280;
                daringleatherjerkin.Object_Type = 33;
                daringleatherjerkin.Item_Type = 25;
                daringleatherjerkin.Id_nb = "daringleatherjerkin";
                daringleatherjerkin.Hand = 0;
                daringleatherjerkin.Price = 0;
                daringleatherjerkin.IsPickable = true;
                daringleatherjerkin.IsDropable = true;
                daringleatherjerkin.IsTradable = true;
                daringleatherjerkin.CanDropAsLoot = false;
                daringleatherjerkin.Color = 0;
                daringleatherjerkin.Bonus = 0; // default bonus				
                daringleatherjerkin.Bonus1 = 12;
                daringleatherjerkin.Bonus1Type = (int)10;
                daringleatherjerkin.Bonus2 = 0;
                daringleatherjerkin.Bonus2Type = (int)0;
                daringleatherjerkin.Bonus3 = 0;
                daringleatherjerkin.Bonus3Type = (int)0;
                daringleatherjerkin.Bonus4 = 0;
                daringleatherjerkin.Bonus4Type = (int)0;
                daringleatherjerkin.Bonus5 = 0;
                daringleatherjerkin.Bonus5Type = (int)0;
                daringleatherjerkin.Bonus6 = 0;
                daringleatherjerkin.Bonus6Type = (int)0;
                daringleatherjerkin.Bonus7 = 0;
                daringleatherjerkin.Bonus7Type = (int)0;
                daringleatherjerkin.Bonus8 = 0;
                daringleatherjerkin.Bonus8Type = (int)0;
                daringleatherjerkin.Bonus9 = 0;
                daringleatherjerkin.Bonus9Type = (int)0;
                daringleatherjerkin.Bonus10 = 0;
                daringleatherjerkin.Bonus10Type = (int)0;
                daringleatherjerkin.ExtraBonus = 0;
                daringleatherjerkin.ExtraBonusType = (int)0;
                daringleatherjerkin.Effect = 0;
                daringleatherjerkin.DPS_AF = 10;
                daringleatherjerkin.Charges = 0;
                daringleatherjerkin.MaxCharges = 0;
                daringleatherjerkin.SpellID = 0;
                daringleatherjerkin.ProcSpellID = 0;
                daringleatherjerkin.SPD_ABS = 10;
                daringleatherjerkin.Realm = 0;
                daringleatherjerkin.MaxCount = 1;
                daringleatherjerkin.PackSize = 1;
                daringleatherjerkin.Extension = 0;
                daringleatherjerkin.Quality = 89;
                daringleatherjerkin.Condition = 50000;
                daringleatherjerkin.MaxCondition = 50000;
                daringleatherjerkin.Durability = 50000;
                daringleatherjerkin.MaxDurability = 50000;
                daringleatherjerkin.PoisonCharges = 0;
                daringleatherjerkin.PoisonMaxCharges = 0;
                daringleatherjerkin.PoisonSpellID = 0;
                daringleatherjerkin.ProcSpellID1 = 0;
                daringleatherjerkin.SpellID1 = 0;
                daringleatherjerkin.MaxCharges1 = 0;
                daringleatherjerkin.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringleatherjerkin);
            }
            daringleatherleggings = GameServer.Database.FindObjectByKey<ItemTemplate>("daringleatherleggings");
            if (daringleatherleggings == null)
            {
                daringleatherleggings = new ItemTemplate();
                daringleatherleggings.Name = "Daring Leather Leggings";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleatherleggings.Name + ", creating it ...");
                daringleatherleggings.Level = 5;
                daringleatherleggings.Weight = 16;
                daringleatherleggings.Model = 281;
                daringleatherleggings.Object_Type = 33;
                daringleatherleggings.Item_Type = 27;
                daringleatherleggings.Id_nb = "daringleatherleggings";
                daringleatherleggings.Hand = 0;
                daringleatherleggings.Price = 0;
                daringleatherleggings.IsPickable = true;
                daringleatherleggings.IsDropable = true;
                daringleatherleggings.IsTradable = true;
                daringleatherleggings.CanDropAsLoot = false;
                daringleatherleggings.Color = 0;
                daringleatherleggings.Bonus = 0; // default bonus				
                daringleatherleggings.Bonus1 = 4;
                daringleatherleggings.Bonus1Type = (int)2;
                daringleatherleggings.Bonus2 = 0;
                daringleatherleggings.Bonus2Type = (int)0;
                daringleatherleggings.Bonus3 = 0;
                daringleatherleggings.Bonus3Type = (int)0;
                daringleatherleggings.Bonus4 = 0;
                daringleatherleggings.Bonus4Type = (int)0;
                daringleatherleggings.Bonus5 = 0;
                daringleatherleggings.Bonus5Type = (int)0;
                daringleatherleggings.Bonus6 = 0;
                daringleatherleggings.Bonus6Type = (int)0;
                daringleatherleggings.Bonus7 = 0;
                daringleatherleggings.Bonus7Type = (int)0;
                daringleatherleggings.Bonus8 = 0;
                daringleatherleggings.Bonus8Type = (int)0;
                daringleatherleggings.Bonus9 = 0;
                daringleatherleggings.Bonus9Type = (int)0;
                daringleatherleggings.Bonus10 = 0;
                daringleatherleggings.Bonus10Type = (int)0;
                daringleatherleggings.ExtraBonus = 0;
                daringleatherleggings.ExtraBonusType = (int)0;
                daringleatherleggings.Effect = 0;
                daringleatherleggings.DPS_AF = 10;
                daringleatherleggings.Charges = 0;
                daringleatherleggings.MaxCharges = 0;
                daringleatherleggings.SpellID = 0;
                daringleatherleggings.ProcSpellID = 0;
                daringleatherleggings.SPD_ABS = 10;
                daringleatherleggings.Realm = 0;
                daringleatherleggings.MaxCount = 1;
                daringleatherleggings.PackSize = 1;
                daringleatherleggings.Extension = 0;
                daringleatherleggings.Quality = 89;
                daringleatherleggings.Condition = 50000;
                daringleatherleggings.MaxCondition = 50000;
                daringleatherleggings.Durability = 50000;
                daringleatherleggings.MaxDurability = 50000;
                daringleatherleggings.PoisonCharges = 0;
                daringleatherleggings.PoisonMaxCharges = 0;
                daringleatherleggings.PoisonSpellID = 0;
                daringleatherleggings.ProcSpellID1 = 0;
                daringleatherleggings.SpellID1 = 0;
                daringleatherleggings.MaxCharges1 = 0;
                daringleatherleggings.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringleatherleggings);
            }
            daringleathersleeves = GameServer.Database.FindObjectByKey<ItemTemplate>("daringleathersleeves");
            if (daringleathersleeves == null)
            {
                daringleathersleeves = new ItemTemplate();
                daringleathersleeves.Name = "Daring Leather Sleeves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleathersleeves.Name + ", creating it ...");
                daringleathersleeves.Level = 5;
                daringleathersleeves.Weight = 16;
                daringleathersleeves.Model = 282;
                daringleathersleeves.Object_Type = 33;
                daringleathersleeves.Item_Type = 28;
                daringleathersleeves.Id_nb = "daringleathersleeves";
                daringleathersleeves.Hand = 0;
                daringleathersleeves.Price = 0;
                daringleathersleeves.IsPickable = true;
                daringleathersleeves.IsDropable = true;
                daringleathersleeves.IsTradable = true;
                daringleathersleeves.CanDropAsLoot = false;
                daringleathersleeves.Color = 0;
                daringleathersleeves.Bonus = 0; // default bonus				
                daringleathersleeves.Bonus1 = 4;
                daringleathersleeves.Bonus1Type = (int)1;
                daringleathersleeves.Bonus2 = 0;
                daringleathersleeves.Bonus2Type = (int)0;
                daringleathersleeves.Bonus3 = 0;
                daringleathersleeves.Bonus3Type = (int)0;
                daringleathersleeves.Bonus4 = 0;
                daringleathersleeves.Bonus4Type = (int)0;
                daringleathersleeves.Bonus5 = 0;
                daringleathersleeves.Bonus5Type = (int)0;
                daringleathersleeves.Bonus6 = 0;
                daringleathersleeves.Bonus6Type = (int)0;
                daringleathersleeves.Bonus7 = 0;
                daringleathersleeves.Bonus7Type = (int)0;
                daringleathersleeves.Bonus8 = 0;
                daringleathersleeves.Bonus8Type = (int)0;
                daringleathersleeves.Bonus9 = 0;
                daringleathersleeves.Bonus9Type = (int)0;
                daringleathersleeves.Bonus10 = 0;
                daringleathersleeves.Bonus10Type = (int)0;
                daringleathersleeves.ExtraBonus = 0;
                daringleathersleeves.ExtraBonusType = (int)0;
                daringleathersleeves.Effect = 0;
                daringleathersleeves.DPS_AF = 10;
                daringleathersleeves.Charges = 0;
                daringleathersleeves.MaxCharges = 0;
                daringleathersleeves.SpellID = 0;
                daringleathersleeves.ProcSpellID = 0;
                daringleathersleeves.SPD_ABS = 10;
                daringleathersleeves.Realm = 0;
                daringleathersleeves.MaxCount = 1;
                daringleathersleeves.PackSize = 1;
                daringleathersleeves.Extension = 0;
                daringleathersleeves.Quality = 89;
                daringleathersleeves.Condition = 50000;
                daringleathersleeves.MaxCondition = 50000;
                daringleathersleeves.Durability = 50000;
                daringleathersleeves.MaxDurability = 50000;
                daringleathersleeves.PoisonCharges = 0;
                daringleathersleeves.PoisonMaxCharges = 0;
                daringleathersleeves.PoisonSpellID = 0;
                daringleathersleeves.ProcSpellID1 = 0;
                daringleathersleeves.SpellID1 = 0;
                daringleathersleeves.MaxCharges1 = 0;
                daringleathersleeves.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringleathersleeves);
            }
            daringstuddedboots = GameServer.Database.FindObjectByKey<ItemTemplate>("daringstuddedboots");
            if (daringstuddedboots == null)
            {
                daringstuddedboots = new ItemTemplate();
                daringstuddedboots.Name = "Daring Studded Boots";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedboots.Name + ", creating it ...");
                daringstuddedboots.Level = 5;
                daringstuddedboots.Weight = 24;
                daringstuddedboots.Model = 234;
                daringstuddedboots.Object_Type = 34;
                daringstuddedboots.Item_Type = 23;
                daringstuddedboots.Id_nb = "daringstuddedboots";
                daringstuddedboots.Hand = 0;
                daringstuddedboots.Price = 0;
                daringstuddedboots.IsPickable = true;
                daringstuddedboots.IsDropable = true;
                daringstuddedboots.IsTradable = true;
                daringstuddedboots.CanDropAsLoot = false;
                daringstuddedboots.Color = 0;
                daringstuddedboots.Bonus = 0; // default bonus				
                daringstuddedboots.Bonus1 = 4;
                daringstuddedboots.Bonus1Type = (int)3;
                daringstuddedboots.Bonus2 = 0;
                daringstuddedboots.Bonus2Type = (int)0;
                daringstuddedboots.Bonus3 = 0;
                daringstuddedboots.Bonus3Type = (int)0;
                daringstuddedboots.Bonus4 = 0;
                daringstuddedboots.Bonus4Type = (int)0;
                daringstuddedboots.Bonus5 = 0;
                daringstuddedboots.Bonus5Type = (int)0;
                daringstuddedboots.Bonus6 = 0;
                daringstuddedboots.Bonus6Type = (int)0;
                daringstuddedboots.Bonus7 = 0;
                daringstuddedboots.Bonus7Type = (int)0;
                daringstuddedboots.Bonus8 = 0;
                daringstuddedboots.Bonus8Type = (int)0;
                daringstuddedboots.Bonus9 = 0;
                daringstuddedboots.Bonus9Type = (int)0;
                daringstuddedboots.Bonus10 = 0;
                daringstuddedboots.Bonus10Type = (int)0;
                daringstuddedboots.ExtraBonus = 0;
                daringstuddedboots.ExtraBonusType = (int)0;
                daringstuddedboots.Effect = 0;
                daringstuddedboots.DPS_AF = 12;
                daringstuddedboots.Charges = 0;
                daringstuddedboots.MaxCharges = 0;
                daringstuddedboots.SpellID = 0;
                daringstuddedboots.ProcSpellID = 0;
                daringstuddedboots.SPD_ABS = 19;
                daringstuddedboots.Realm = 0;
                daringstuddedboots.MaxCount = 1;
                daringstuddedboots.PackSize = 1;
                daringstuddedboots.Extension = 0;
                daringstuddedboots.Quality = 89;
                daringstuddedboots.Condition = 50000;
                daringstuddedboots.MaxCondition = 50000;
                daringstuddedboots.Durability = 50000;
                daringstuddedboots.MaxDurability = 50000;
                daringstuddedboots.PoisonCharges = 0;
                daringstuddedboots.PoisonMaxCharges = 0;
                daringstuddedboots.PoisonSpellID = 0;
                daringstuddedboots.ProcSpellID1 = 0;
                daringstuddedboots.SpellID1 = 0;
                daringstuddedboots.MaxCharges1 = 0;
                daringstuddedboots.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringstuddedboots);
            }
            daringstuddedcap = GameServer.Database.FindObjectByKey<ItemTemplate>("daringstuddedcap");
            if (daringstuddedcap == null)
            {
                daringstuddedcap = new ItemTemplate();
                daringstuddedcap.Name = "Daring Studded Cap";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedcap.Name + ", creating it ...");
                daringstuddedcap.Level = 5;
                daringstuddedcap.Weight = 24;
                daringstuddedcap.Model = 829;
                daringstuddedcap.Object_Type = 34;
                daringstuddedcap.Item_Type = 21;
                daringstuddedcap.Id_nb = "daringstuddedcap";
                daringstuddedcap.Hand = 0;
                daringstuddedcap.Price = 0;
                daringstuddedcap.IsPickable = true;
                daringstuddedcap.IsDropable = true;
                daringstuddedcap.IsTradable = true;
                daringstuddedcap.CanDropAsLoot = false;
                daringstuddedcap.Color = 0;
                daringstuddedcap.Bonus = 0; // default bonus				
                daringstuddedcap.Bonus1 = 4;
                daringstuddedcap.Bonus1Type = (int)3;
                daringstuddedcap.Bonus2 = 0;
                daringstuddedcap.Bonus2Type = (int)0;
                daringstuddedcap.Bonus3 = 0;
                daringstuddedcap.Bonus3Type = (int)0;
                daringstuddedcap.Bonus4 = 0;
                daringstuddedcap.Bonus4Type = (int)0;
                daringstuddedcap.Bonus5 = 0;
                daringstuddedcap.Bonus5Type = (int)0;
                daringstuddedcap.Bonus6 = 0;
                daringstuddedcap.Bonus6Type = (int)0;
                daringstuddedcap.Bonus7 = 0;
                daringstuddedcap.Bonus7Type = (int)0;
                daringstuddedcap.Bonus8 = 0;
                daringstuddedcap.Bonus8Type = (int)0;
                daringstuddedcap.Bonus9 = 0;
                daringstuddedcap.Bonus9Type = (int)0;
                daringstuddedcap.Bonus10 = 0;
                daringstuddedcap.Bonus10Type = (int)0;
                daringstuddedcap.ExtraBonus = 0;
                daringstuddedcap.ExtraBonusType = (int)0;
                daringstuddedcap.Effect = 0;
                daringstuddedcap.DPS_AF = 12;
                daringstuddedcap.Charges = 0;
                daringstuddedcap.MaxCharges = 0;
                daringstuddedcap.SpellID = 0;
                daringstuddedcap.ProcSpellID = 0;
                daringstuddedcap.SPD_ABS = 19;
                daringstuddedcap.Realm = 0;
                daringstuddedcap.MaxCount = 1;
                daringstuddedcap.PackSize = 1;
                daringstuddedcap.Extension = 0;
                daringstuddedcap.Quality = 89;
                daringstuddedcap.Condition = 50000;
                daringstuddedcap.MaxCondition = 50000;
                daringstuddedcap.Durability = 50000;
                daringstuddedcap.MaxDurability = 50000;
                daringstuddedcap.PoisonCharges = 0;
                daringstuddedcap.PoisonMaxCharges = 0;
                daringstuddedcap.PoisonSpellID = 0;
                daringstuddedcap.ProcSpellID1 = 0;
                daringstuddedcap.SpellID1 = 0;
                daringstuddedcap.MaxCharges1 = 0;
                daringstuddedcap.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringstuddedcap);
            }
            daringstuddedgloves = GameServer.Database.FindObjectByKey<ItemTemplate>("daringstuddedgloves");
            if (daringstuddedgloves == null)
            {
                daringstuddedgloves = new ItemTemplate();
                daringstuddedgloves.Name = "Daring Studded Gloves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedgloves.Name + ", creating it ...");
                daringstuddedgloves.Level = 5;
                daringstuddedgloves.Weight = 24;
                daringstuddedgloves.Model = 233;
                daringstuddedgloves.Object_Type = 34;
                daringstuddedgloves.Item_Type = 22;
                daringstuddedgloves.Id_nb = "daringstuddedgloves";
                daringstuddedgloves.Hand = 0;
                daringstuddedgloves.Price = 0;
                daringstuddedgloves.IsPickable = true;
                daringstuddedgloves.IsDropable = true;
                daringstuddedgloves.IsTradable = true;
                daringstuddedgloves.CanDropAsLoot = false;
                daringstuddedgloves.Color = 0;
                daringstuddedgloves.Bonus = 0; // default bonus				
                daringstuddedgloves.Bonus1 = 4;
                daringstuddedgloves.Bonus1Type = (int)4;
                daringstuddedgloves.Bonus2 = 0;
                daringstuddedgloves.Bonus2Type = (int)0;
                daringstuddedgloves.Bonus3 = 0;
                daringstuddedgloves.Bonus3Type = (int)0;
                daringstuddedgloves.Bonus4 = 0;
                daringstuddedgloves.Bonus4Type = (int)0;
                daringstuddedgloves.Bonus5 = 0;
                daringstuddedgloves.Bonus5Type = (int)0;
                daringstuddedgloves.Bonus6 = 0;
                daringstuddedgloves.Bonus6Type = (int)0;
                daringstuddedgloves.Bonus7 = 0;
                daringstuddedgloves.Bonus7Type = (int)0;
                daringstuddedgloves.Bonus8 = 0;
                daringstuddedgloves.Bonus8Type = (int)0;
                daringstuddedgloves.Bonus9 = 0;
                daringstuddedgloves.Bonus9Type = (int)0;
                daringstuddedgloves.Bonus10 = 0;
                daringstuddedgloves.Bonus10Type = (int)0;
                daringstuddedgloves.ExtraBonus = 0;
                daringstuddedgloves.ExtraBonusType = (int)0;
                daringstuddedgloves.Effect = 0;
                daringstuddedgloves.DPS_AF = 12;
                daringstuddedgloves.Charges = 0;
                daringstuddedgloves.MaxCharges = 0;
                daringstuddedgloves.SpellID = 0;
                daringstuddedgloves.ProcSpellID = 0;
                daringstuddedgloves.SPD_ABS = 19;
                daringstuddedgloves.Realm = 0;
                daringstuddedgloves.MaxCount = 1;
                daringstuddedgloves.PackSize = 1;
                daringstuddedgloves.Extension = 0;
                daringstuddedgloves.Quality = 89;
                daringstuddedgloves.Condition = 50000;
                daringstuddedgloves.MaxCondition = 50000;
                daringstuddedgloves.Durability = 50000;
                daringstuddedgloves.MaxDurability = 50000;
                daringstuddedgloves.PoisonCharges = 0;
                daringstuddedgloves.PoisonMaxCharges = 0;
                daringstuddedgloves.PoisonSpellID = 0;
                daringstuddedgloves.ProcSpellID1 = 0;
                daringstuddedgloves.SpellID1 = 0;
                daringstuddedgloves.MaxCharges1 = 0;
                daringstuddedgloves.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringstuddedgloves);
            }
            daringstuddedjerkin = GameServer.Database.FindObjectByKey<ItemTemplate>("daringstuddedjerkin");
            if (daringstuddedjerkin == null)
            {
                daringstuddedjerkin = new ItemTemplate();
                daringstuddedjerkin.Name = "Daring Studded Jerkin";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedjerkin.Name + ", creating it ...");
                daringstuddedjerkin.Level = 5;
                daringstuddedjerkin.Weight = 60;
                daringstuddedjerkin.Model = 230;
                daringstuddedjerkin.Object_Type = 34;
                daringstuddedjerkin.Item_Type = 25;
                daringstuddedjerkin.Id_nb = "daringstuddedjerkin";
                daringstuddedjerkin.Hand = 0;
                daringstuddedjerkin.Price = 0;
                daringstuddedjerkin.IsPickable = true;
                daringstuddedjerkin.IsDropable = true;
                daringstuddedjerkin.IsTradable = true;
                daringstuddedjerkin.CanDropAsLoot = false;
                daringstuddedjerkin.Color = 0;
                daringstuddedjerkin.Bonus = 0; // default bonus				
                daringstuddedjerkin.Bonus1 = 12;
                daringstuddedjerkin.Bonus1Type = (int)10;
                daringstuddedjerkin.Bonus2 = 0;
                daringstuddedjerkin.Bonus2Type = (int)0;
                daringstuddedjerkin.Bonus3 = 0;
                daringstuddedjerkin.Bonus3Type = (int)0;
                daringstuddedjerkin.Bonus4 = 0;
                daringstuddedjerkin.Bonus4Type = (int)0;
                daringstuddedjerkin.Bonus5 = 0;
                daringstuddedjerkin.Bonus5Type = (int)0;
                daringstuddedjerkin.Bonus6 = 0;
                daringstuddedjerkin.Bonus6Type = (int)0;
                daringstuddedjerkin.Bonus7 = 0;
                daringstuddedjerkin.Bonus7Type = (int)0;
                daringstuddedjerkin.Bonus8 = 0;
                daringstuddedjerkin.Bonus8Type = (int)0;
                daringstuddedjerkin.Bonus9 = 0;
                daringstuddedjerkin.Bonus9Type = (int)0;
                daringstuddedjerkin.Bonus10 = 0;
                daringstuddedjerkin.Bonus10Type = (int)0;
                daringstuddedjerkin.ExtraBonus = 0;
                daringstuddedjerkin.ExtraBonusType = (int)0;
                daringstuddedjerkin.Effect = 0;
                daringstuddedjerkin.DPS_AF = 12;
                daringstuddedjerkin.Charges = 0;
                daringstuddedjerkin.MaxCharges = 0;
                daringstuddedjerkin.SpellID = 0;
                daringstuddedjerkin.ProcSpellID = 0;
                daringstuddedjerkin.SPD_ABS = 19;
                daringstuddedjerkin.Realm = 0;
                daringstuddedjerkin.MaxCount = 1;
                daringstuddedjerkin.PackSize = 1;
                daringstuddedjerkin.Extension = 0;
                daringstuddedjerkin.Quality = 89;
                daringstuddedjerkin.Condition = 50000;
                daringstuddedjerkin.MaxCondition = 50000;
                daringstuddedjerkin.Durability = 50000;
                daringstuddedjerkin.MaxDurability = 50000;
                daringstuddedjerkin.PoisonCharges = 0;
                daringstuddedjerkin.PoisonMaxCharges = 0;
                daringstuddedjerkin.PoisonSpellID = 0;
                daringstuddedjerkin.ProcSpellID1 = 0;
                daringstuddedjerkin.SpellID1 = 0;
                daringstuddedjerkin.MaxCharges1 = 0;
                daringstuddedjerkin.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringstuddedjerkin);
            }
            daringstuddedleggings = GameServer.Database.FindObjectByKey<ItemTemplate>("daringstuddedleggings");
            if (daringstuddedleggings == null)
            {
                daringstuddedleggings = new ItemTemplate();
                daringstuddedleggings.Name = "Daring Studded Leggings";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedleggings.Name + ", creating it ...");
                daringstuddedleggings.Level = 5;
                daringstuddedleggings.Weight = 42;
                daringstuddedleggings.Model = 231;
                daringstuddedleggings.Object_Type = 34;
                daringstuddedleggings.Item_Type = 27;
                daringstuddedleggings.Id_nb = "daringstuddedleggings";
                daringstuddedleggings.Hand = 0;
                daringstuddedleggings.Price = 0;
                daringstuddedleggings.IsPickable = true;
                daringstuddedleggings.IsDropable = true;
                daringstuddedleggings.IsTradable = true;
                daringstuddedleggings.CanDropAsLoot = false;
                daringstuddedleggings.Color = 0;
                daringstuddedleggings.Bonus = 0; // default bonus				
                daringstuddedleggings.Bonus1 = 4;
                daringstuddedleggings.Bonus1Type = (int)4;
                daringstuddedleggings.Bonus2 = 0;
                daringstuddedleggings.Bonus2Type = (int)0;
                daringstuddedleggings.Bonus3 = 0;
                daringstuddedleggings.Bonus3Type = (int)0;
                daringstuddedleggings.Bonus4 = 0;
                daringstuddedleggings.Bonus4Type = (int)0;
                daringstuddedleggings.Bonus5 = 0;
                daringstuddedleggings.Bonus5Type = (int)0;
                daringstuddedleggings.Bonus6 = 0;
                daringstuddedleggings.Bonus6Type = (int)0;
                daringstuddedleggings.Bonus7 = 0;
                daringstuddedleggings.Bonus7Type = (int)0;
                daringstuddedleggings.Bonus8 = 0;
                daringstuddedleggings.Bonus8Type = (int)0;
                daringstuddedleggings.Bonus9 = 0;
                daringstuddedleggings.Bonus9Type = (int)0;
                daringstuddedleggings.Bonus10 = 0;
                daringstuddedleggings.Bonus10Type = (int)0;
                daringstuddedleggings.ExtraBonus = 0;
                daringstuddedleggings.ExtraBonusType = (int)0;
                daringstuddedleggings.Effect = 0;
                daringstuddedleggings.DPS_AF = 12;
                daringstuddedleggings.Charges = 0;
                daringstuddedleggings.MaxCharges = 0;
                daringstuddedleggings.SpellID = 0;
                daringstuddedleggings.ProcSpellID = 0;
                daringstuddedleggings.SPD_ABS = 19;
                daringstuddedleggings.Realm = 0;
                daringstuddedleggings.MaxCount = 1;
                daringstuddedleggings.PackSize = 1;
                daringstuddedleggings.Extension = 0;
                daringstuddedleggings.Quality = 89;
                daringstuddedleggings.Condition = 50000;
                daringstuddedleggings.MaxCondition = 50000;
                daringstuddedleggings.Durability = 50000;
                daringstuddedleggings.MaxDurability = 50000;
                daringstuddedleggings.PoisonCharges = 0;
                daringstuddedleggings.PoisonMaxCharges = 0;
                daringstuddedleggings.PoisonSpellID = 0;
                daringstuddedleggings.ProcSpellID1 = 0;
                daringstuddedleggings.SpellID1 = 0;
                daringstuddedleggings.MaxCharges1 = 0;
                daringstuddedleggings.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringstuddedleggings);
            }
            daringstuddedsleeves = GameServer.Database.FindObjectByKey<ItemTemplate>("daringstuddedsleeves");
            if (daringstuddedsleeves == null)
            {
                daringstuddedsleeves = new ItemTemplate();
                daringstuddedsleeves.Name = "Daring Studded Sleeves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedsleeves.Name + ", creating it ...");
                daringstuddedsleeves.Level = 5;
                daringstuddedsleeves.Weight = 36;
                daringstuddedsleeves.Model = 232;
                daringstuddedsleeves.Object_Type = 34;
                daringstuddedsleeves.Item_Type = 28;
                daringstuddedsleeves.Id_nb = "daringstuddedsleeves";
                daringstuddedsleeves.Hand = 0;
                daringstuddedsleeves.Price = 0;
                daringstuddedsleeves.IsPickable = true;
                daringstuddedsleeves.IsDropable = true;
                daringstuddedsleeves.IsTradable = true;
                daringstuddedsleeves.CanDropAsLoot = false;
                daringstuddedsleeves.Color = 0;
                daringstuddedsleeves.Bonus = 0; // default bonus				
                daringstuddedsleeves.Bonus1 = 4;
                daringstuddedsleeves.Bonus1Type = (int)1;
                daringstuddedsleeves.Bonus2 = 0;
                daringstuddedsleeves.Bonus2Type = (int)0;
                daringstuddedsleeves.Bonus3 = 0;
                daringstuddedsleeves.Bonus3Type = (int)0;
                daringstuddedsleeves.Bonus4 = 0;
                daringstuddedsleeves.Bonus4Type = (int)0;
                daringstuddedsleeves.Bonus5 = 0;
                daringstuddedsleeves.Bonus5Type = (int)0;
                daringstuddedsleeves.Bonus6 = 0;
                daringstuddedsleeves.Bonus6Type = (int)0;
                daringstuddedsleeves.Bonus7 = 0;
                daringstuddedsleeves.Bonus7Type = (int)0;
                daringstuddedsleeves.Bonus8 = 0;
                daringstuddedsleeves.Bonus8Type = (int)0;
                daringstuddedsleeves.Bonus9 = 0;
                daringstuddedsleeves.Bonus9Type = (int)0;
                daringstuddedsleeves.Bonus10 = 0;
                daringstuddedsleeves.Bonus10Type = (int)0;
                daringstuddedsleeves.ExtraBonus = 0;
                daringstuddedsleeves.ExtraBonusType = (int)0;
                daringstuddedsleeves.Effect = 0;
                daringstuddedsleeves.DPS_AF = 12;
                daringstuddedsleeves.Charges = 0;
                daringstuddedsleeves.MaxCharges = 0;
                daringstuddedsleeves.SpellID = 0;
                daringstuddedsleeves.ProcSpellID = 0;
                daringstuddedsleeves.SPD_ABS = 6;
                daringstuddedsleeves.Realm = 0;
                daringstuddedsleeves.MaxCount = 1;
                daringstuddedsleeves.PackSize = 1;
                daringstuddedsleeves.Extension = 0;
                daringstuddedsleeves.Quality = 89;
                daringstuddedsleeves.Condition = 50000;
                daringstuddedsleeves.MaxCondition = 50000;
                daringstuddedsleeves.Durability = 50000;
                daringstuddedsleeves.MaxDurability = 50000;
                daringstuddedsleeves.PoisonCharges = 0;
                daringstuddedsleeves.PoisonMaxCharges = 0;
                daringstuddedsleeves.PoisonSpellID = 0;
                daringstuddedsleeves.ProcSpellID1 = 0;
                daringstuddedsleeves.SpellID1 = 0;
                daringstuddedsleeves.MaxCharges1 = 0;
                daringstuddedsleeves.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddObject(daringstuddedsleeves);
            }


            #endregion

            #region defineAreas
            Mid_Statue_Area = WorldMgr.GetRegion(Mid_Statue.RegionID).AddArea(new Area.Circle("", Mid_Statue.X, Mid_Statue.Y, Mid_Statue.Z, 500));
            Mid_Statue_Area.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterStatueArea));

            #endregion

            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

            GameEventMgr.AddHandler(Charles, GameLivingEvent.Interact, new DOLEventHandler(TalkToCharles));
            GameEventMgr.AddHandler(Charles, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToCharles));

            Charles.AddQuestToGive(typeof(childsplay));
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questTitle + "\" initialized");
        }

        [ScriptUnloadedEvent]
        public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            if (!ServerProperties.Properties.LOAD_QUESTS)
                return;
            //Region StatuaRegion = WorldMgr.GetRegion(489);
            //if (StatuaRegion != null)
            //    StatuaRegion.RemoveArea(Statua);
            Mid_Statue_Area.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterStatueArea));
            WorldMgr.GetRegion(Mid_Statue.RegionID).RemoveArea(Mid_Statue_Area);
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

                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Viking"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedboots);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedcap);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedgloves);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedjerkin);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedleggings);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedsleeves);
                    }
                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Mystic"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedboots);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedcap);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedgloves);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedpants);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedsleeves);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedvest);
                    }
                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.MidgardRogue"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringleatherboots);
                        GiveItem(Charles, quest.m_questPlayer, daringleathercap);
                        GiveItem(Charles, quest.m_questPlayer, daringleathergloves);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherjerkin);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherleggings);
                        GiveItem(Charles, quest.m_questPlayer, daringleathersleeves);
                    }
                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Seer"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringleatherboots);
                        GiveItem(Charles, quest.m_questPlayer, daringleathercap);
                        GiveItem(Charles, quest.m_questPlayer, daringleathergloves);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherjerkin);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherleggings);
                        GiveItem(Charles, quest.m_questPlayer, daringleathersleeves);
                    }
                    quest.FinishQuest();
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
                    return LanguageMgr.GetTranslation(m_questPlayer.Client, "Mid.ChildsPlay.Description.Text1");
                }
                else if (Step == 2)
                {
                    return LanguageMgr.GetTranslation(m_questPlayer.Client, "Mid.ChildsPlay.Description.Text2");
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
        }
    }
}
