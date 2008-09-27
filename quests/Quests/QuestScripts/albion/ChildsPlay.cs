
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

namespace DOL.GS.Quests.Albion
{
    public class childsplay : BaseQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        ///
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected const string questTitle = "Child's Play";

        protected const int minimumLevel = 1;
        protected const int maximumLevel = 4;

        private static GameNPC Charles = null;
        private static ItemTemplate daringpaddedboots_alb = null;
        private static ItemTemplate daringpaddedcap_alb = null;
        private static ItemTemplate daringpaddedgloves_alb = null;
        private static ItemTemplate daringpaddedpants_alb = null;
        private static ItemTemplate daringpaddedsleeves_alb = null;
        private static ItemTemplate daringpaddedvest_alb = null;
        private static ItemTemplate daringleatherboots_alb = null;
        private static ItemTemplate daringleathercap_alb = null;
        private static ItemTemplate daringleathergloves_alb = null;
        private static ItemTemplate daringleatherjerkin_alb = null;
        private static ItemTemplate daringleatherleggings_alb = null;
        private static ItemTemplate daringleathersleeves_alb = null;
        private static ItemTemplate daringstuddedboots_alb = null;
        private static ItemTemplate daringstuddedcap_alb = null;
        private static ItemTemplate daringstuddedgloves_alb = null;
        private static ItemTemplate daringstuddedjerkin_alb = null;
        private static ItemTemplate daringstuddedleggings_alb = null;
        private static ItemTemplate daringstuddedsleeves_alb = null;
        private static GameLocation Albion_Statue = new GameLocation("Childs Play (Alb)", 489, 27580, 40006, 14483);

        private static IArea Albion_Statue_Area = null;

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
                log.Info("Quest \"" + questTitle + "\" (Alb) initializing ...");

            #region defineNPCs
            GameNPC[] npcs;

            npcs = WorldMgr.GetNPCsByName("Charles", (eRealm)1);
            if (npcs.Length == 0)
            {
                Charles = new DOL.GS.GameNPC();
                Charles.Model = 92;
                Charles.Name = "Charles";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + Charles.Name + ", creating ...");
                Charles.Realm = eRealm.Albion;
                Charles.CurrentRegionID = 1;
                Charles.Size = 37;
                Charles.Level = 1;
                Charles.MaxSpeedBase = 191;
                Charles.Faction = FactionMgr.GetFactionByID(0);
                Charles.X = 559883;
                Charles.Y = 511489;
                Charles.Z = 2382;
                Charles.Heading = 3515;
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

            daringpaddedboots_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringpaddedboots_alb");
            if (daringpaddedboots_alb == null)
            {
                daringpaddedboots_alb = new ItemTemplate();
                daringpaddedboots_alb.Name = "Daring Padded Boots";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedboots_alb.Name + ", creating it ...");
                daringpaddedboots_alb.Level = 5;
                daringpaddedboots_alb.Weight = 8;
                daringpaddedboots_alb.Model = 143;
                daringpaddedboots_alb.Object_Type = 32;
                daringpaddedboots_alb.Item_Type = 23;
                daringpaddedboots_alb.Id_nb = "daringpaddedboots_alb";
                daringpaddedboots_alb.Hand = 0;
                daringpaddedboots_alb.Platinum = 0;
                daringpaddedboots_alb.Gold = 0;
                daringpaddedboots_alb.Silver = 0;
                daringpaddedboots_alb.Copper = 0;
                daringpaddedboots_alb.IsPickable = true;
                daringpaddedboots_alb.IsDropable = true;
                daringpaddedboots_alb.IsTradable = true;
                daringpaddedboots_alb.CanDropAsLoot = false;
                daringpaddedboots_alb.Color = 0;
                daringpaddedboots_alb.Bonus = 0; // default bonus				
                daringpaddedboots_alb.Bonus1 = 4;
                daringpaddedboots_alb.Bonus1Type = (int)3;
                daringpaddedboots_alb.Bonus2 = 0;
                daringpaddedboots_alb.Bonus2Type = (int)0;
                daringpaddedboots_alb.Bonus3 = 0;
                daringpaddedboots_alb.Bonus3Type = (int)0;
                daringpaddedboots_alb.Bonus4 = 0;
                daringpaddedboots_alb.Bonus4Type = (int)0;
                daringpaddedboots_alb.Bonus5 = 0;
                daringpaddedboots_alb.Bonus5Type = (int)0;
                daringpaddedboots_alb.Bonus6 = 0;
                daringpaddedboots_alb.Bonus6Type = (int)0;
                daringpaddedboots_alb.Bonus7 = 0;
                daringpaddedboots_alb.Bonus7Type = (int)0;
                daringpaddedboots_alb.Bonus8 = 0;
                daringpaddedboots_alb.Bonus8Type = (int)0;
                daringpaddedboots_alb.Bonus9 = 0;
                daringpaddedboots_alb.Bonus9Type = (int)0;
                daringpaddedboots_alb.Bonus10 = 0;
                daringpaddedboots_alb.Bonus10Type = (int)0;
                daringpaddedboots_alb.ExtraBonus = 0;
                daringpaddedboots_alb.ExtraBonusType = (int)0;
                daringpaddedboots_alb.Effect = 0;
                daringpaddedboots_alb.DPS_AF = 6;
                daringpaddedboots_alb.Charges = 0;
                daringpaddedboots_alb.MaxCharges = 0;
                daringpaddedboots_alb.SpellID = 0;
                daringpaddedboots_alb.ProcSpellID = 0;
                daringpaddedboots_alb.SPD_ABS = 0;
                daringpaddedboots_alb.Realm = 0;
                daringpaddedboots_alb.MaxCount = 1;
                daringpaddedboots_alb.PackSize = 1;
                daringpaddedboots_alb.Extension = 0;
                daringpaddedboots_alb.Quality =89;
                daringpaddedboots_alb.Condition = 50000;
                daringpaddedboots_alb.MaxCondition = 50000;
                daringpaddedboots_alb.Durability = 50000;
                daringpaddedboots_alb.MaxDurability = 50000;
                daringpaddedboots_alb.PoisonCharges = 0;
                daringpaddedboots_alb.PoisonMaxCharges = 0;
                daringpaddedboots_alb.PoisonSpellID = 0;
                daringpaddedboots_alb.ProcSpellID1 = 0;
                daringpaddedboots_alb.SpellID1 = 0;
                daringpaddedboots_alb.MaxCharges1 = 0;
                daringpaddedboots_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringpaddedboots_alb);
            }
            daringpaddedcap_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringpaddedcap_alb");
            if (daringpaddedcap_alb == null)
            {
                daringpaddedcap_alb = new ItemTemplate();
                daringpaddedcap_alb.Name = "Daring Padded Cap";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedcap_alb.Name + ", creating it ...");
                daringpaddedcap_alb.Level = 5;
                daringpaddedcap_alb.Weight = 8;
                daringpaddedcap_alb.Model = 822;
                daringpaddedcap_alb.Object_Type = 32;
                daringpaddedcap_alb.Item_Type = 21;
                daringpaddedcap_alb.Id_nb = "daringpaddedcap_alb";
                daringpaddedcap_alb.Hand = 0;
                daringpaddedcap_alb.Platinum = 0;
                daringpaddedcap_alb.Gold = 0;
                daringpaddedcap_alb.Silver = 0;
                daringpaddedcap_alb.Copper = 0;
                daringpaddedcap_alb.IsPickable = true;
                daringpaddedcap_alb.IsDropable = true;
                daringpaddedcap_alb.IsTradable = true;
                daringpaddedcap_alb.CanDropAsLoot = false;
                daringpaddedcap_alb.Color = 0;
                daringpaddedcap_alb.Bonus = 0; // default bonus				
                daringpaddedcap_alb.Bonus1 = 4;
                daringpaddedcap_alb.Bonus1Type = (int)2;
                daringpaddedcap_alb.Bonus2 = 0;
                daringpaddedcap_alb.Bonus2Type = (int)0;
                daringpaddedcap_alb.Bonus3 = 0;
                daringpaddedcap_alb.Bonus3Type = (int)0;
                daringpaddedcap_alb.Bonus4 = 0;
                daringpaddedcap_alb.Bonus4Type = (int)0;
                daringpaddedcap_alb.Bonus5 = 0;
                daringpaddedcap_alb.Bonus5Type = (int)0;
                daringpaddedcap_alb.Bonus6 = 0;
                daringpaddedcap_alb.Bonus6Type = (int)0;
                daringpaddedcap_alb.Bonus7 = 0;
                daringpaddedcap_alb.Bonus7Type = (int)0;
                daringpaddedcap_alb.Bonus8 = 0;
                daringpaddedcap_alb.Bonus8Type = (int)0;
                daringpaddedcap_alb.Bonus9 = 0;
                daringpaddedcap_alb.Bonus9Type = (int)0;
                daringpaddedcap_alb.Bonus10 = 0;
                daringpaddedcap_alb.Bonus10Type = (int)0;
                daringpaddedcap_alb.ExtraBonus = 0;
                daringpaddedcap_alb.ExtraBonusType = (int)0;
                daringpaddedcap_alb.Effect = 0;
                daringpaddedcap_alb.DPS_AF = 6;
                daringpaddedcap_alb.Charges = 0;
                daringpaddedcap_alb.MaxCharges = 0;
                daringpaddedcap_alb.SpellID = 0;
                daringpaddedcap_alb.ProcSpellID = 0;
                daringpaddedcap_alb.SPD_ABS = 0;
                daringpaddedcap_alb.Realm = 0;
                daringpaddedcap_alb.MaxCount = 1;
                daringpaddedcap_alb.PackSize = 1;
                daringpaddedcap_alb.Extension = 0;
                daringpaddedcap_alb.Quality =89;
                daringpaddedcap_alb.Condition = 50000;
                daringpaddedcap_alb.MaxCondition = 50000;
                daringpaddedcap_alb.Durability = 50000;
                daringpaddedcap_alb.MaxDurability = 50000;
                daringpaddedcap_alb.PoisonCharges = 0;
                daringpaddedcap_alb.PoisonMaxCharges = 0;
                daringpaddedcap_alb.PoisonSpellID = 0;
                daringpaddedcap_alb.ProcSpellID1 = 0;
                daringpaddedcap_alb.SpellID1 = 0;
                daringpaddedcap_alb.MaxCharges1 = 0;
                daringpaddedcap_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringpaddedcap_alb);
            }
            daringpaddedgloves_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringpaddedgloves_alb");
            if (daringpaddedgloves_alb == null)
            {
                daringpaddedgloves_alb = new ItemTemplate();
                daringpaddedgloves_alb.Name = "Daring Padded Gloves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedgloves_alb.Name + ", creating it ...");
                daringpaddedgloves_alb.Level = 5;
                daringpaddedgloves_alb.Weight = 8;
                daringpaddedgloves_alb.Model = 142;
                daringpaddedgloves_alb.Object_Type = 32;
                daringpaddedgloves_alb.Item_Type = 22;
                daringpaddedgloves_alb.Id_nb = "daringpaddedgloves_alb";
                daringpaddedgloves_alb.Hand = 0;
                daringpaddedgloves_alb.Platinum = 0;
                daringpaddedgloves_alb.Gold = 0;
                daringpaddedgloves_alb.Silver = 0;
                daringpaddedgloves_alb.Copper = 0;
                daringpaddedgloves_alb.IsPickable = true;
                daringpaddedgloves_alb.IsDropable = true;
                daringpaddedgloves_alb.IsTradable = true;
                daringpaddedgloves_alb.CanDropAsLoot = false;
                daringpaddedgloves_alb.Color = 0;
                daringpaddedgloves_alb.Bonus = 0; // default bonus				
                daringpaddedgloves_alb.Bonus1 = 4;
                daringpaddedgloves_alb.Bonus1Type = (int)3;
                daringpaddedgloves_alb.Bonus2 = 0;
                daringpaddedgloves_alb.Bonus2Type = (int)0;
                daringpaddedgloves_alb.Bonus3 = 0;
                daringpaddedgloves_alb.Bonus3Type = (int)0;
                daringpaddedgloves_alb.Bonus4 = 0;
                daringpaddedgloves_alb.Bonus4Type = (int)0;
                daringpaddedgloves_alb.Bonus5 = 0;
                daringpaddedgloves_alb.Bonus5Type = (int)0;
                daringpaddedgloves_alb.Bonus6 = 0;
                daringpaddedgloves_alb.Bonus6Type = (int)0;
                daringpaddedgloves_alb.Bonus7 = 0;
                daringpaddedgloves_alb.Bonus7Type = (int)0;
                daringpaddedgloves_alb.Bonus8 = 0;
                daringpaddedgloves_alb.Bonus8Type = (int)0;
                daringpaddedgloves_alb.Bonus9 = 0;
                daringpaddedgloves_alb.Bonus9Type = (int)0;
                daringpaddedgloves_alb.Bonus10 = 0;
                daringpaddedgloves_alb.Bonus10Type = (int)0;
                daringpaddedgloves_alb.ExtraBonus = 0;
                daringpaddedgloves_alb.ExtraBonusType = (int)0;
                daringpaddedgloves_alb.Effect = 0;
                daringpaddedgloves_alb.DPS_AF = 6;
                daringpaddedgloves_alb.Charges = 0;
                daringpaddedgloves_alb.MaxCharges = 0;
                daringpaddedgloves_alb.SpellID = 0;
                daringpaddedgloves_alb.ProcSpellID = 0;
                daringpaddedgloves_alb.SPD_ABS = 0;
                daringpaddedgloves_alb.Realm = 0;
                daringpaddedgloves_alb.MaxCount = 1;
                daringpaddedgloves_alb.PackSize = 1;
                daringpaddedgloves_alb.Extension = 0;
                daringpaddedgloves_alb.Quality =89;
                daringpaddedgloves_alb.Condition = 50000;
                daringpaddedgloves_alb.MaxCondition = 50000;
                daringpaddedgloves_alb.Durability = 50000;
                daringpaddedgloves_alb.MaxDurability = 50000;
                daringpaddedgloves_alb.PoisonCharges = 0;
                daringpaddedgloves_alb.PoisonMaxCharges = 0;
                daringpaddedgloves_alb.PoisonSpellID = 0;
                daringpaddedgloves_alb.ProcSpellID1 = 0;
                daringpaddedgloves_alb.SpellID1 = 0;
                daringpaddedgloves_alb.MaxCharges1 = 0;
                daringpaddedgloves_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringpaddedgloves_alb);
            }
            daringpaddedpants_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringpaddedpants_alb");
            if (daringpaddedpants_alb == null)
            {
                daringpaddedpants_alb = new ItemTemplate();
                daringpaddedpants_alb.Name = "Daring Padded Pants";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedpants_alb.Name + ", creating it ...");
                daringpaddedpants_alb.Level = 5;
                daringpaddedpants_alb.Weight = 14;
                daringpaddedpants_alb.Model = 140;
                daringpaddedpants_alb.Object_Type = 32;
                daringpaddedpants_alb.Item_Type = 27;
                daringpaddedpants_alb.Id_nb = "daringpaddedpants_alb";
                daringpaddedpants_alb.Hand = 0;
                daringpaddedpants_alb.Platinum = 0;
                daringpaddedpants_alb.Gold = 0;
                daringpaddedpants_alb.Silver = 0;
                daringpaddedpants_alb.Copper = 0;
                daringpaddedpants_alb.IsPickable = true;
                daringpaddedpants_alb.IsDropable = true;
                daringpaddedpants_alb.IsTradable = true;
                daringpaddedpants_alb.CanDropAsLoot = false;
                daringpaddedpants_alb.Color = 0;
                daringpaddedpants_alb.Bonus = 0; // default bonus				
                daringpaddedpants_alb.Bonus1 = 4;
                daringpaddedpants_alb.Bonus1Type = (int)2;
                daringpaddedpants_alb.Bonus2 = 0;
                daringpaddedpants_alb.Bonus2Type = (int)0;
                daringpaddedpants_alb.Bonus3 = 0;
                daringpaddedpants_alb.Bonus3Type = (int)0;
                daringpaddedpants_alb.Bonus4 = 0;
                daringpaddedpants_alb.Bonus4Type = (int)0;
                daringpaddedpants_alb.Bonus5 = 0;
                daringpaddedpants_alb.Bonus5Type = (int)0;
                daringpaddedpants_alb.Bonus6 = 0;
                daringpaddedpants_alb.Bonus6Type = (int)0;
                daringpaddedpants_alb.Bonus7 = 0;
                daringpaddedpants_alb.Bonus7Type = (int)0;
                daringpaddedpants_alb.Bonus8 = 0;
                daringpaddedpants_alb.Bonus8Type = (int)0;
                daringpaddedpants_alb.Bonus9 = 0;
                daringpaddedpants_alb.Bonus9Type = (int)0;
                daringpaddedpants_alb.Bonus10 = 0;
                daringpaddedpants_alb.Bonus10Type = (int)0;
                daringpaddedpants_alb.ExtraBonus = 0;
                daringpaddedpants_alb.ExtraBonusType = (int)0;
                daringpaddedpants_alb.Effect = 0;
                daringpaddedpants_alb.DPS_AF = 6;
                daringpaddedpants_alb.Charges = 0;
                daringpaddedpants_alb.MaxCharges = 0;
                daringpaddedpants_alb.SpellID = 0;
                daringpaddedpants_alb.ProcSpellID = 0;
                daringpaddedpants_alb.SPD_ABS = 0;
                daringpaddedpants_alb.Realm = 0;
                daringpaddedpants_alb.MaxCount = 1;
                daringpaddedpants_alb.PackSize = 1;
                daringpaddedpants_alb.Extension = 0;
                daringpaddedpants_alb.Quality =89;
                daringpaddedpants_alb.Condition = 50000;
                daringpaddedpants_alb.MaxCondition = 50000;
                daringpaddedpants_alb.Durability = 50000;
                daringpaddedpants_alb.MaxDurability = 50000;
                daringpaddedpants_alb.PoisonCharges = 0;
                daringpaddedpants_alb.PoisonMaxCharges = 0;
                daringpaddedpants_alb.PoisonSpellID = 0;
                daringpaddedpants_alb.ProcSpellID1 = 0;
                daringpaddedpants_alb.SpellID1 = 0;
                daringpaddedpants_alb.MaxCharges1 = 0;
                daringpaddedpants_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringpaddedpants_alb);
            }
            daringpaddedsleeves_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringpaddedsleeves_alb");
            if (daringpaddedsleeves_alb == null)
            {
                daringpaddedsleeves_alb = new ItemTemplate();
                daringpaddedsleeves_alb.Name = "Daring Padded Sleeves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedsleeves_alb.Name + ", creating it ...");
                daringpaddedsleeves_alb.Level = 5;
                daringpaddedsleeves_alb.Weight = 12;
                daringpaddedsleeves_alb.Model = 141;
                daringpaddedsleeves_alb.Object_Type = 32;
                daringpaddedsleeves_alb.Item_Type = 28;
                daringpaddedsleeves_alb.Id_nb = "daringpaddedsleeves_alb";
                daringpaddedsleeves_alb.Hand = 0;
                daringpaddedsleeves_alb.Platinum = 0;
                daringpaddedsleeves_alb.Gold = 0;
                daringpaddedsleeves_alb.Silver = 0;
                daringpaddedsleeves_alb.Copper = 0;
                daringpaddedsleeves_alb.IsPickable = true;
                daringpaddedsleeves_alb.IsDropable = true;
                daringpaddedsleeves_alb.IsTradable = true;
                daringpaddedsleeves_alb.CanDropAsLoot = false;
                daringpaddedsleeves_alb.Color = 0;
                daringpaddedsleeves_alb.Bonus = 0; // default bonus				
                daringpaddedsleeves_alb.Bonus1 = 4;
                daringpaddedsleeves_alb.Bonus1Type = (int)6;
                daringpaddedsleeves_alb.Bonus2 = 0;
                daringpaddedsleeves_alb.Bonus2Type = (int)0;
                daringpaddedsleeves_alb.Bonus3 = 0;
                daringpaddedsleeves_alb.Bonus3Type = (int)0;
                daringpaddedsleeves_alb.Bonus4 = 0;
                daringpaddedsleeves_alb.Bonus4Type = (int)0;
                daringpaddedsleeves_alb.Bonus5 = 0;
                daringpaddedsleeves_alb.Bonus5Type = (int)0;
                daringpaddedsleeves_alb.Bonus6 = 0;
                daringpaddedsleeves_alb.Bonus6Type = (int)0;
                daringpaddedsleeves_alb.Bonus7 = 0;
                daringpaddedsleeves_alb.Bonus7Type = (int)0;
                daringpaddedsleeves_alb.Bonus8 = 0;
                daringpaddedsleeves_alb.Bonus8Type = (int)0;
                daringpaddedsleeves_alb.Bonus9 = 0;
                daringpaddedsleeves_alb.Bonus9Type = (int)0;
                daringpaddedsleeves_alb.Bonus10 = 0;
                daringpaddedsleeves_alb.Bonus10Type = (int)0;
                daringpaddedsleeves_alb.ExtraBonus = 0;
                daringpaddedsleeves_alb.ExtraBonusType = (int)0;
                daringpaddedsleeves_alb.Effect = 0;
                daringpaddedsleeves_alb.DPS_AF = 6;
                daringpaddedsleeves_alb.Charges = 0;
                daringpaddedsleeves_alb.MaxCharges = 0;
                daringpaddedsleeves_alb.SpellID = 0;
                daringpaddedsleeves_alb.ProcSpellID = 0;
                daringpaddedsleeves_alb.SPD_ABS = 0;
                daringpaddedsleeves_alb.Realm = 0;
                daringpaddedsleeves_alb.MaxCount = 1;
                daringpaddedsleeves_alb.PackSize = 1;
                daringpaddedsleeves_alb.Extension = 0;
                daringpaddedsleeves_alb.Quality =89;
                daringpaddedsleeves_alb.Condition = 50000;
                daringpaddedsleeves_alb.MaxCondition = 50000;
                daringpaddedsleeves_alb.Durability = 50000;
                daringpaddedsleeves_alb.MaxDurability = 50000;
                daringpaddedsleeves_alb.PoisonCharges = 0;
                daringpaddedsleeves_alb.PoisonMaxCharges = 0;
                daringpaddedsleeves_alb.PoisonSpellID = 0;
                daringpaddedsleeves_alb.ProcSpellID1 = 0;
                daringpaddedsleeves_alb.SpellID1 = 0;
                daringpaddedsleeves_alb.MaxCharges1 = 0;
                daringpaddedsleeves_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringpaddedsleeves_alb);
            }
            daringpaddedvest_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringpaddedvest_alb");
            if (daringpaddedvest_alb == null)
            {
                daringpaddedvest_alb = new ItemTemplate();
                daringpaddedvest_alb.Name = "Daring Padded Vest";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringpaddedvest_alb.Name + ", creating it ...");
                daringpaddedvest_alb.Level = 5;
                daringpaddedvest_alb.Weight = 20;
                daringpaddedvest_alb.Model = 139;
                daringpaddedvest_alb.Object_Type = 32;
                daringpaddedvest_alb.Item_Type = 25;
                daringpaddedvest_alb.Id_nb = "daringpaddedvest_alb";
                daringpaddedvest_alb.Hand = 0;
                daringpaddedvest_alb.Platinum = 0;
                daringpaddedvest_alb.Gold = 0;
                daringpaddedvest_alb.Silver = 0;
                daringpaddedvest_alb.Copper = 0;
                daringpaddedvest_alb.IsPickable = true;
                daringpaddedvest_alb.IsDropable = true;
                daringpaddedvest_alb.IsTradable = true;
                daringpaddedvest_alb.CanDropAsLoot = false;
                daringpaddedvest_alb.Color = 0;
                daringpaddedvest_alb.Bonus = 0; // default bonus				
                daringpaddedvest_alb.Bonus1 = 12;
                daringpaddedvest_alb.Bonus1Type = (int)10;
                daringpaddedvest_alb.Bonus2 = 0;
                daringpaddedvest_alb.Bonus2Type = (int)0;
                daringpaddedvest_alb.Bonus3 = 0;
                daringpaddedvest_alb.Bonus3Type = (int)0;
                daringpaddedvest_alb.Bonus4 = 0;
                daringpaddedvest_alb.Bonus4Type = (int)0;
                daringpaddedvest_alb.Bonus5 = 0;
                daringpaddedvest_alb.Bonus5Type = (int)0;
                daringpaddedvest_alb.Bonus6 = 0;
                daringpaddedvest_alb.Bonus6Type = (int)0;
                daringpaddedvest_alb.Bonus7 = 0;
                daringpaddedvest_alb.Bonus7Type = (int)0;
                daringpaddedvest_alb.Bonus8 = 0;
                daringpaddedvest_alb.Bonus8Type = (int)0;
                daringpaddedvest_alb.Bonus9 = 0;
                daringpaddedvest_alb.Bonus9Type = (int)0;
                daringpaddedvest_alb.Bonus10 = 0;
                daringpaddedvest_alb.Bonus10Type = (int)0;
                daringpaddedvest_alb.ExtraBonus = 0;
                daringpaddedvest_alb.ExtraBonusType = (int)0;
                daringpaddedvest_alb.Effect = 0;
                daringpaddedvest_alb.DPS_AF = 6;
                daringpaddedvest_alb.Charges = 0;
                daringpaddedvest_alb.MaxCharges = 0;
                daringpaddedvest_alb.SpellID = 0;
                daringpaddedvest_alb.ProcSpellID = 0;
                daringpaddedvest_alb.SPD_ABS = 0;
                daringpaddedvest_alb.Realm = 0;
                daringpaddedvest_alb.MaxCount = 1;
                daringpaddedvest_alb.PackSize = 1;
                daringpaddedvest_alb.Extension = 0;
                daringpaddedvest_alb.Quality =89;
                daringpaddedvest_alb.Condition = 50000;
                daringpaddedvest_alb.MaxCondition = 50000;
                daringpaddedvest_alb.Durability = 50000;
                daringpaddedvest_alb.MaxDurability = 50000;
                daringpaddedvest_alb.PoisonCharges = 0;
                daringpaddedvest_alb.PoisonMaxCharges = 0;
                daringpaddedvest_alb.PoisonSpellID = 0;
                daringpaddedvest_alb.ProcSpellID1 = 0;
                daringpaddedvest_alb.SpellID1 = 0;
                daringpaddedvest_alb.MaxCharges1 = 0;
                daringpaddedvest_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringpaddedvest_alb);
            }
            daringleatherboots_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringleatherboots_alb");
            if (daringleatherboots_alb == null)
            {
                daringleatherboots_alb = new ItemTemplate();
                daringleatherboots_alb.Name = "Daring Leather Boots";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleatherboots_alb.Name + ", creating it ...");
                daringleatherboots_alb.Level = 5;
                daringleatherboots_alb.Weight = 16;
                daringleatherboots_alb.Model = 133;
                daringleatherboots_alb.Object_Type = 33;
                daringleatherboots_alb.Item_Type = 23;
                daringleatherboots_alb.Id_nb = "daringleatherboots_alb";
                daringleatherboots_alb.Hand = 0;
                daringleatherboots_alb.Platinum = 0;
                daringleatherboots_alb.Gold = 0;
                daringleatherboots_alb.Silver = 0;
                daringleatherboots_alb.Copper = 0;
                daringleatherboots_alb.IsPickable = true;
                daringleatherboots_alb.IsDropable = true;
                daringleatherboots_alb.IsTradable = true;
                daringleatherboots_alb.CanDropAsLoot = false;
                daringleatherboots_alb.Color = 0;
                daringleatherboots_alb.Bonus = 0; // default bonus				
                daringleatherboots_alb.Bonus1 = 4;
                daringleatherboots_alb.Bonus1Type = (int)3;
                daringleatherboots_alb.Bonus2 = 0;
                daringleatherboots_alb.Bonus2Type = (int)0;
                daringleatherboots_alb.Bonus3 = 0;
                daringleatherboots_alb.Bonus3Type = (int)0;
                daringleatherboots_alb.Bonus4 = 0;
                daringleatherboots_alb.Bonus4Type = (int)0;
                daringleatherboots_alb.Bonus5 = 0;
                daringleatherboots_alb.Bonus5Type = (int)0;
                daringleatherboots_alb.Bonus6 = 0;
                daringleatherboots_alb.Bonus6Type = (int)0;
                daringleatherboots_alb.Bonus7 = 0;
                daringleatherboots_alb.Bonus7Type = (int)0;
                daringleatherboots_alb.Bonus8 = 0;
                daringleatherboots_alb.Bonus8Type = (int)0;
                daringleatherboots_alb.Bonus9 = 0;
                daringleatherboots_alb.Bonus9Type = (int)0;
                daringleatherboots_alb.Bonus10 = 0;
                daringleatherboots_alb.Bonus10Type = (int)0;
                daringleatherboots_alb.ExtraBonus = 0;
                daringleatherboots_alb.ExtraBonusType = (int)0;
                daringleatherboots_alb.Effect = 0;
                daringleatherboots_alb.DPS_AF = 10;
                daringleatherboots_alb.Charges = 0;
                daringleatherboots_alb.MaxCharges = 0;
                daringleatherboots_alb.SpellID = 0;
                daringleatherboots_alb.ProcSpellID = 0;
                daringleatherboots_alb.SPD_ABS = 10;
                daringleatherboots_alb.Realm = 0;
                daringleatherboots_alb.MaxCount = 1;
                daringleatherboots_alb.PackSize = 1;
                daringleatherboots_alb.Extension = 0;
                daringleatherboots_alb.Quality =89;
                daringleatherboots_alb.Condition = 50000;
                daringleatherboots_alb.MaxCondition = 50000;
                daringleatherboots_alb.Durability = 50000;
                daringleatherboots_alb.MaxDurability = 50000;
                daringleatherboots_alb.PoisonCharges = 0;
                daringleatherboots_alb.PoisonMaxCharges = 0;
                daringleatherboots_alb.PoisonSpellID = 0;
                daringleatherboots_alb.ProcSpellID1 = 0;
                daringleatherboots_alb.SpellID1 = 0;
                daringleatherboots_alb.MaxCharges1 = 0;
                daringleatherboots_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringleatherboots_alb);
            }
            daringleathercap_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringleathercap_alb");
            if (daringleathercap_alb == null)
            {
                daringleathercap_alb = new ItemTemplate();
                daringleathercap_alb.Name = "Daring Leather Cap";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleathercap_alb.Name + ", creating it ...");
                daringleathercap_alb.Level = 5;
                daringleathercap_alb.Weight = 16;
                daringleathercap_alb.Model = 62;
                daringleathercap_alb.Object_Type = 33;
                daringleathercap_alb.Item_Type = 21;
                daringleathercap_alb.Id_nb = "daringleathercap_alb";
                daringleathercap_alb.Hand = 0;
                daringleathercap_alb.Platinum = 0;
                daringleathercap_alb.Gold = 0;
                daringleathercap_alb.Silver = 0;
                daringleathercap_alb.Copper = 0;
                daringleathercap_alb.IsPickable = true;
                daringleathercap_alb.IsDropable = true;
                daringleathercap_alb.IsTradable = true;
                daringleathercap_alb.CanDropAsLoot = false;
                daringleathercap_alb.Color = 0;
                daringleathercap_alb.Bonus = 0; // default bonus				
                daringleathercap_alb.Bonus1 = 4;
                daringleathercap_alb.Bonus1Type = (int)3;
                daringleathercap_alb.Bonus2 = 0;
                daringleathercap_alb.Bonus2Type = (int)0;
                daringleathercap_alb.Bonus3 = 0;
                daringleathercap_alb.Bonus3Type = (int)0;
                daringleathercap_alb.Bonus4 = 0;
                daringleathercap_alb.Bonus4Type = (int)0;
                daringleathercap_alb.Bonus5 = 0;
                daringleathercap_alb.Bonus5Type = (int)0;
                daringleathercap_alb.Bonus6 = 0;
                daringleathercap_alb.Bonus6Type = (int)0;
                daringleathercap_alb.Bonus7 = 0;
                daringleathercap_alb.Bonus7Type = (int)0;
                daringleathercap_alb.Bonus8 = 0;
                daringleathercap_alb.Bonus8Type = (int)0;
                daringleathercap_alb.Bonus9 = 0;
                daringleathercap_alb.Bonus9Type = (int)0;
                daringleathercap_alb.Bonus10 = 0;
                daringleathercap_alb.Bonus10Type = (int)0;
                daringleathercap_alb.ExtraBonus = 0;
                daringleathercap_alb.ExtraBonusType = (int)0;
                daringleathercap_alb.Effect = 0;
                daringleathercap_alb.DPS_AF = 10;
                daringleathercap_alb.Charges = 0;
                daringleathercap_alb.MaxCharges = 0;
                daringleathercap_alb.SpellID = 0;
                daringleathercap_alb.ProcSpellID = 0;
                daringleathercap_alb.SPD_ABS = 10;
                daringleathercap_alb.Realm = 0;
                daringleathercap_alb.MaxCount = 1;
                daringleathercap_alb.PackSize = 1;
                daringleathercap_alb.Extension = 0;
                daringleathercap_alb.Quality =89;
                daringleathercap_alb.Condition = 50000;
                daringleathercap_alb.MaxCondition = 50000;
                daringleathercap_alb.Durability = 50000;
                daringleathercap_alb.MaxDurability = 50000;
                daringleathercap_alb.PoisonCharges = 0;
                daringleathercap_alb.PoisonMaxCharges = 0;
                daringleathercap_alb.PoisonSpellID = 0;
                daringleathercap_alb.ProcSpellID1 = 0;
                daringleathercap_alb.SpellID1 = 0;
                daringleathercap_alb.MaxCharges1 = 0;
                daringleathercap_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringleathercap_alb);
            }
            daringleathergloves_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringleathergloves_alb");
            if (daringleathergloves_alb == null)
            {
                daringleathergloves_alb = new ItemTemplate();
                daringleathergloves_alb.Name = "Daring Leather Gloves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleathergloves_alb.Name + ", creating it ...");
                daringleathergloves_alb.Level = 5;
                daringleathergloves_alb.Weight = 16;
                daringleathergloves_alb.Model = 34;
                daringleathergloves_alb.Object_Type = 33;
                daringleathergloves_alb.Item_Type = 22;
                daringleathergloves_alb.Id_nb = "daringleathergloves_alb";
                daringleathergloves_alb.Hand = 0;
                daringleathergloves_alb.Platinum = 0;
                daringleathergloves_alb.Gold = 0;
                daringleathergloves_alb.Silver = 0;
                daringleathergloves_alb.Copper = 0;
                daringleathergloves_alb.IsPickable = true;
                daringleathergloves_alb.IsDropable = true;
                daringleathergloves_alb.IsTradable = true;
                daringleathergloves_alb.CanDropAsLoot = false;
                daringleathergloves_alb.Color = 0;
                daringleathergloves_alb.Bonus = 0; // default bonus				
                daringleathergloves_alb.Bonus1 = 4;
                daringleathergloves_alb.Bonus1Type = (int)2;
                daringleathergloves_alb.Bonus2 = 0;
                daringleathergloves_alb.Bonus2Type = (int)0;
                daringleathergloves_alb.Bonus3 = 0;
                daringleathergloves_alb.Bonus3Type = (int)0;
                daringleathergloves_alb.Bonus4 = 0;
                daringleathergloves_alb.Bonus4Type = (int)0;
                daringleathergloves_alb.Bonus5 = 0;
                daringleathergloves_alb.Bonus5Type = (int)0;
                daringleathergloves_alb.Bonus6 = 0;
                daringleathergloves_alb.Bonus6Type = (int)0;
                daringleathergloves_alb.Bonus7 = 0;
                daringleathergloves_alb.Bonus7Type = (int)0;
                daringleathergloves_alb.Bonus8 = 0;
                daringleathergloves_alb.Bonus8Type = (int)0;
                daringleathergloves_alb.Bonus9 = 0;
                daringleathergloves_alb.Bonus9Type = (int)0;
                daringleathergloves_alb.Bonus10 = 0;
                daringleathergloves_alb.Bonus10Type = (int)0;
                daringleathergloves_alb.ExtraBonus = 0;
                daringleathergloves_alb.ExtraBonusType = (int)0;
                daringleathergloves_alb.Effect = 0;
                daringleathergloves_alb.DPS_AF = 10;
                daringleathergloves_alb.Charges = 0;
                daringleathergloves_alb.MaxCharges = 0;
                daringleathergloves_alb.SpellID = 0;
                daringleathergloves_alb.ProcSpellID = 0;
                daringleathergloves_alb.SPD_ABS = 10;
                daringleathergloves_alb.Realm = 0;
                daringleathergloves_alb.MaxCount = 1;
                daringleathergloves_alb.PackSize = 1;
                daringleathergloves_alb.Extension = 0;
                daringleathergloves_alb.Quality =89;
                daringleathergloves_alb.Condition = 50000;
                daringleathergloves_alb.MaxCondition = 50000;
                daringleathergloves_alb.Durability = 50000;
                daringleathergloves_alb.MaxDurability = 50000;
                daringleathergloves_alb.PoisonCharges = 0;
                daringleathergloves_alb.PoisonMaxCharges = 0;
                daringleathergloves_alb.PoisonSpellID = 0;
                daringleathergloves_alb.ProcSpellID1 = 0;
                daringleathergloves_alb.SpellID1 = 0;
                daringleathergloves_alb.MaxCharges1 = 0;
                daringleathergloves_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringleathergloves_alb);
            }
            daringleatherjerkin_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringleatherjerkin_alb");
            if (daringleatherjerkin_alb == null)
            {
                daringleatherjerkin_alb = new ItemTemplate();
                daringleatherjerkin_alb.Name = "Daring Leather Jerkin";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleatherjerkin_alb.Name + ", creating it ...");
                daringleatherjerkin_alb.Level = 5;
                daringleatherjerkin_alb.Weight = 16;
                daringleatherjerkin_alb.Model = 31;
                daringleatherjerkin_alb.Object_Type = 33;
                daringleatherjerkin_alb.Item_Type = 25;
                daringleatherjerkin_alb.Id_nb = "daringleatherjerkin_alb";
                daringleatherjerkin_alb.Hand = 0;
                daringleatherjerkin_alb.Platinum = 0;
                daringleatherjerkin_alb.Gold = 0;
                daringleatherjerkin_alb.Silver = 0;
                daringleatherjerkin_alb.Copper = 0;
                daringleatherjerkin_alb.IsPickable = true;
                daringleatherjerkin_alb.IsDropable = true;
                daringleatherjerkin_alb.IsTradable = true;
                daringleatherjerkin_alb.CanDropAsLoot = false;
                daringleatherjerkin_alb.Color = 0;
                daringleatherjerkin_alb.Bonus = 0; // default bonus				
                daringleatherjerkin_alb.Bonus1 = 12;
                daringleatherjerkin_alb.Bonus1Type = (int)10;
                daringleatherjerkin_alb.Bonus2 = 0;
                daringleatherjerkin_alb.Bonus2Type = (int)0;
                daringleatherjerkin_alb.Bonus3 = 0;
                daringleatherjerkin_alb.Bonus3Type = (int)0;
                daringleatherjerkin_alb.Bonus4 = 0;
                daringleatherjerkin_alb.Bonus4Type = (int)0;
                daringleatherjerkin_alb.Bonus5 = 0;
                daringleatherjerkin_alb.Bonus5Type = (int)0;
                daringleatherjerkin_alb.Bonus6 = 0;
                daringleatherjerkin_alb.Bonus6Type = (int)0;
                daringleatherjerkin_alb.Bonus7 = 0;
                daringleatherjerkin_alb.Bonus7Type = (int)0;
                daringleatherjerkin_alb.Bonus8 = 0;
                daringleatherjerkin_alb.Bonus8Type = (int)0;
                daringleatherjerkin_alb.Bonus9 = 0;
                daringleatherjerkin_alb.Bonus9Type = (int)0;
                daringleatherjerkin_alb.Bonus10 = 0;
                daringleatherjerkin_alb.Bonus10Type = (int)0;
                daringleatherjerkin_alb.ExtraBonus = 0;
                daringleatherjerkin_alb.ExtraBonusType = (int)0;
                daringleatherjerkin_alb.Effect = 0;
                daringleatherjerkin_alb.DPS_AF = 10;
                daringleatherjerkin_alb.Charges = 0;
                daringleatherjerkin_alb.MaxCharges = 0;
                daringleatherjerkin_alb.SpellID = 0;
                daringleatherjerkin_alb.ProcSpellID = 0;
                daringleatherjerkin_alb.SPD_ABS = 10;
                daringleatherjerkin_alb.Realm = 0;
                daringleatherjerkin_alb.MaxCount = 1;
                daringleatherjerkin_alb.PackSize = 1;
                daringleatherjerkin_alb.Extension = 0;
                daringleatherjerkin_alb.Quality =89;
                daringleatherjerkin_alb.Condition = 50000;
                daringleatherjerkin_alb.MaxCondition = 50000;
                daringleatherjerkin_alb.Durability = 50000;
                daringleatherjerkin_alb.MaxDurability = 50000;
                daringleatherjerkin_alb.PoisonCharges = 0;
                daringleatherjerkin_alb.PoisonMaxCharges = 0;
                daringleatherjerkin_alb.PoisonSpellID = 0;
                daringleatherjerkin_alb.ProcSpellID1 = 0;
                daringleatherjerkin_alb.SpellID1 = 0;
                daringleatherjerkin_alb.MaxCharges1 = 0;
                daringleatherjerkin_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringleatherjerkin_alb);
            }
            daringleatherleggings_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringleatherleggings_alb");
            if (daringleatherleggings_alb == null)
            {
                daringleatherleggings_alb = new ItemTemplate();
                daringleatherleggings_alb.Name = "Daring Leather Leggings";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleatherleggings_alb.Name + ", creating it ...");
                daringleatherleggings_alb.Level = 5;
                daringleatherleggings_alb.Weight = 16;
                daringleatherleggings_alb.Model = 32;
                daringleatherleggings_alb.Object_Type = 33;
                daringleatherleggings_alb.Item_Type = 27;
                daringleatherleggings_alb.Id_nb = "daringleatherleggings_alb";
                daringleatherleggings_alb.Hand = 0;
                daringleatherleggings_alb.Platinum = 0;
                daringleatherleggings_alb.Gold = 0;
                daringleatherleggings_alb.Silver = 0;
                daringleatherleggings_alb.Copper = 0;
                daringleatherleggings_alb.IsPickable = true;
                daringleatherleggings_alb.IsDropable = true;
                daringleatherleggings_alb.IsTradable = true;
                daringleatherleggings_alb.CanDropAsLoot = false;
                daringleatherleggings_alb.Color = 0;
                daringleatherleggings_alb.Bonus = 0; // default bonus				
                daringleatherleggings_alb.Bonus1 = 4;
                daringleatherleggings_alb.Bonus1Type = (int)2;
                daringleatherleggings_alb.Bonus2 = 0;
                daringleatherleggings_alb.Bonus2Type = (int)0;
                daringleatherleggings_alb.Bonus3 = 0;
                daringleatherleggings_alb.Bonus3Type = (int)0;
                daringleatherleggings_alb.Bonus4 = 0;
                daringleatherleggings_alb.Bonus4Type = (int)0;
                daringleatherleggings_alb.Bonus5 = 0;
                daringleatherleggings_alb.Bonus5Type = (int)0;
                daringleatherleggings_alb.Bonus6 = 0;
                daringleatherleggings_alb.Bonus6Type = (int)0;
                daringleatherleggings_alb.Bonus7 = 0;
                daringleatherleggings_alb.Bonus7Type = (int)0;
                daringleatherleggings_alb.Bonus8 = 0;
                daringleatherleggings_alb.Bonus8Type = (int)0;
                daringleatherleggings_alb.Bonus9 = 0;
                daringleatherleggings_alb.Bonus9Type = (int)0;
                daringleatherleggings_alb.Bonus10 = 0;
                daringleatherleggings_alb.Bonus10Type = (int)0;
                daringleatherleggings_alb.ExtraBonus = 0;
                daringleatherleggings_alb.ExtraBonusType = (int)0;
                daringleatherleggings_alb.Effect = 0;
                daringleatherleggings_alb.DPS_AF = 10;
                daringleatherleggings_alb.Charges = 0;
                daringleatherleggings_alb.MaxCharges = 0;
                daringleatherleggings_alb.SpellID = 0;
                daringleatherleggings_alb.ProcSpellID = 0;
                daringleatherleggings_alb.SPD_ABS = 10;
                daringleatherleggings_alb.Realm = 0;
                daringleatherleggings_alb.MaxCount = 1;
                daringleatherleggings_alb.PackSize = 1;
                daringleatherleggings_alb.Extension = 0;
                daringleatherleggings_alb.Quality =89;
                daringleatherleggings_alb.Condition = 50000;
                daringleatherleggings_alb.MaxCondition = 50000;
                daringleatherleggings_alb.Durability = 50000;
                daringleatherleggings_alb.MaxDurability = 50000;
                daringleatherleggings_alb.PoisonCharges = 0;
                daringleatherleggings_alb.PoisonMaxCharges = 0;
                daringleatherleggings_alb.PoisonSpellID = 0;
                daringleatherleggings_alb.ProcSpellID1 = 0;
                daringleatherleggings_alb.SpellID1 = 0;
                daringleatherleggings_alb.MaxCharges1 = 0;
                daringleatherleggings_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringleatherleggings_alb);
            }
            daringleathersleeves_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringleathersleeves_alb");
            if (daringleathersleeves_alb == null)
            {
                daringleathersleeves_alb = new ItemTemplate();
                daringleathersleeves_alb.Name = "Daring Leather Sleeves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringleathersleeves_alb.Name + ", creating it ...");
                daringleathersleeves_alb.Level = 5;
                daringleathersleeves_alb.Weight = 16;
                daringleathersleeves_alb.Model = 33;
                daringleathersleeves_alb.Object_Type = 33;
                daringleathersleeves_alb.Item_Type = 28;
                daringleathersleeves_alb.Id_nb = "daringleathersleeves_alb";
                daringleathersleeves_alb.Hand = 0;
                daringleathersleeves_alb.Platinum = 0;
                daringleathersleeves_alb.Gold = 0;
                daringleathersleeves_alb.Silver = 0;
                daringleathersleeves_alb.Copper = 0;
                daringleathersleeves_alb.IsPickable = true;
                daringleathersleeves_alb.IsDropable = true;
                daringleathersleeves_alb.IsTradable = true;
                daringleathersleeves_alb.CanDropAsLoot = false;
                daringleathersleeves_alb.Color = 0;
                daringleathersleeves_alb.Bonus = 0; // default bonus				
                daringleathersleeves_alb.Bonus1 = 4;
                daringleathersleeves_alb.Bonus1Type = (int)1;
                daringleathersleeves_alb.Bonus2 = 0;
                daringleathersleeves_alb.Bonus2Type = (int)0;
                daringleathersleeves_alb.Bonus3 = 0;
                daringleathersleeves_alb.Bonus3Type = (int)0;
                daringleathersleeves_alb.Bonus4 = 0;
                daringleathersleeves_alb.Bonus4Type = (int)0;
                daringleathersleeves_alb.Bonus5 = 0;
                daringleathersleeves_alb.Bonus5Type = (int)0;
                daringleathersleeves_alb.Bonus6 = 0;
                daringleathersleeves_alb.Bonus6Type = (int)0;
                daringleathersleeves_alb.Bonus7 = 0;
                daringleathersleeves_alb.Bonus7Type = (int)0;
                daringleathersleeves_alb.Bonus8 = 0;
                daringleathersleeves_alb.Bonus8Type = (int)0;
                daringleathersleeves_alb.Bonus9 = 0;
                daringleathersleeves_alb.Bonus9Type = (int)0;
                daringleathersleeves_alb.Bonus10 = 0;
                daringleathersleeves_alb.Bonus10Type = (int)0;
                daringleathersleeves_alb.ExtraBonus = 0;
                daringleathersleeves_alb.ExtraBonusType = (int)0;
                daringleathersleeves_alb.Effect = 0;
                daringleathersleeves_alb.DPS_AF = 10;
                daringleathersleeves_alb.Charges = 0;
                daringleathersleeves_alb.MaxCharges = 0;
                daringleathersleeves_alb.SpellID = 0;
                daringleathersleeves_alb.ProcSpellID = 0;
                daringleathersleeves_alb.SPD_ABS = 10;
                daringleathersleeves_alb.Realm = 0;
                daringleathersleeves_alb.MaxCount = 1;
                daringleathersleeves_alb.PackSize = 1;
                daringleathersleeves_alb.Extension = 0;
                daringleathersleeves_alb.Quality =89;
                daringleathersleeves_alb.Condition = 50000;
                daringleathersleeves_alb.MaxCondition = 50000;
                daringleathersleeves_alb.Durability = 50000;
                daringleathersleeves_alb.MaxDurability = 50000;
                daringleathersleeves_alb.PoisonCharges = 0;
                daringleathersleeves_alb.PoisonMaxCharges = 0;
                daringleathersleeves_alb.PoisonSpellID = 0;
                daringleathersleeves_alb.ProcSpellID1 = 0;
                daringleathersleeves_alb.SpellID1 = 0;
                daringleathersleeves_alb.MaxCharges1 = 0;
                daringleathersleeves_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringleathersleeves_alb);
            }
            daringstuddedboots_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringstuddedboots_alb");
            if (daringstuddedboots_alb == null)
            {
                daringstuddedboots_alb = new ItemTemplate();
                daringstuddedboots_alb.Name = "Daring Studded Boots";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedboots_alb.Name + ", creating it ...");
                daringstuddedboots_alb.Level = 5;
                daringstuddedboots_alb.Weight = 24;
                daringstuddedboots_alb.Model = 84;
                daringstuddedboots_alb.Object_Type = 34;
                daringstuddedboots_alb.Item_Type = 23;
                daringstuddedboots_alb.Id_nb = "daringstuddedboots_alb";
                daringstuddedboots_alb.Hand = 0;
                daringstuddedboots_alb.Platinum = 0;
                daringstuddedboots_alb.Gold = 0;
                daringstuddedboots_alb.Silver = 0;
                daringstuddedboots_alb.Copper = 0;
                daringstuddedboots_alb.IsPickable = true;
                daringstuddedboots_alb.IsDropable = true;
                daringstuddedboots_alb.IsTradable = true;
                daringstuddedboots_alb.CanDropAsLoot = false;
                daringstuddedboots_alb.Color = 0;
                daringstuddedboots_alb.Bonus = 0; // default bonus				
                daringstuddedboots_alb.Bonus1 = 4;
                daringstuddedboots_alb.Bonus1Type = (int)3;
                daringstuddedboots_alb.Bonus2 = 0;
                daringstuddedboots_alb.Bonus2Type = (int)0;
                daringstuddedboots_alb.Bonus3 = 0;
                daringstuddedboots_alb.Bonus3Type = (int)0;
                daringstuddedboots_alb.Bonus4 = 0;
                daringstuddedboots_alb.Bonus4Type = (int)0;
                daringstuddedboots_alb.Bonus5 = 0;
                daringstuddedboots_alb.Bonus5Type = (int)0;
                daringstuddedboots_alb.Bonus6 = 0;
                daringstuddedboots_alb.Bonus6Type = (int)0;
                daringstuddedboots_alb.Bonus7 = 0;
                daringstuddedboots_alb.Bonus7Type = (int)0;
                daringstuddedboots_alb.Bonus8 = 0;
                daringstuddedboots_alb.Bonus8Type = (int)0;
                daringstuddedboots_alb.Bonus9 = 0;
                daringstuddedboots_alb.Bonus9Type = (int)0;
                daringstuddedboots_alb.Bonus10 = 0;
                daringstuddedboots_alb.Bonus10Type = (int)0;
                daringstuddedboots_alb.ExtraBonus = 0;
                daringstuddedboots_alb.ExtraBonusType = (int)0;
                daringstuddedboots_alb.Effect = 0;
                daringstuddedboots_alb.DPS_AF = 12;
                daringstuddedboots_alb.Charges = 0;
                daringstuddedboots_alb.MaxCharges = 0;
                daringstuddedboots_alb.SpellID = 0;
                daringstuddedboots_alb.ProcSpellID = 0;
                daringstuddedboots_alb.SPD_ABS = 19;
                daringstuddedboots_alb.Realm = 0;
                daringstuddedboots_alb.MaxCount = 1;
                daringstuddedboots_alb.PackSize = 1;
                daringstuddedboots_alb.Extension = 0;
                daringstuddedboots_alb.Quality =89;
                daringstuddedboots_alb.Condition = 50000;
                daringstuddedboots_alb.MaxCondition = 50000;
                daringstuddedboots_alb.Durability = 50000;
                daringstuddedboots_alb.MaxDurability = 50000;
                daringstuddedboots_alb.PoisonCharges = 0;
                daringstuddedboots_alb.PoisonMaxCharges = 0;
                daringstuddedboots_alb.PoisonSpellID = 0;
                daringstuddedboots_alb.ProcSpellID1 = 0;
                daringstuddedboots_alb.SpellID1 = 0;
                daringstuddedboots_alb.MaxCharges1 = 0;
                daringstuddedboots_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringstuddedboots_alb);
            }
            daringstuddedcap_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringstuddedcap_alb");
            if (daringstuddedcap_alb == null)
            {
                daringstuddedcap_alb = new ItemTemplate();
                daringstuddedcap_alb.Name = "Daring Studded Cap";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedcap_alb.Name + ", creating it ...");
                daringstuddedcap_alb.Level = 5;
                daringstuddedcap_alb.Weight = 24;
                daringstuddedcap_alb.Model = 824;
                daringstuddedcap_alb.Object_Type = 34;
                daringstuddedcap_alb.Item_Type = 21;
                daringstuddedcap_alb.Id_nb = "daringstuddedcap_alb";
                daringstuddedcap_alb.Hand = 0;
                daringstuddedcap_alb.Platinum = 0;
                daringstuddedcap_alb.Gold = 0;
                daringstuddedcap_alb.Silver = 0;
                daringstuddedcap_alb.Copper = 0;
                daringstuddedcap_alb.IsPickable = true;
                daringstuddedcap_alb.IsDropable = true;
                daringstuddedcap_alb.IsTradable = true;
                daringstuddedcap_alb.CanDropAsLoot = false;
                daringstuddedcap_alb.Color = 0;
                daringstuddedcap_alb.Bonus = 0; // default bonus				
                daringstuddedcap_alb.Bonus1 = 4;
                daringstuddedcap_alb.Bonus1Type = (int)3;
                daringstuddedcap_alb.Bonus2 = 0;
                daringstuddedcap_alb.Bonus2Type = (int)0;
                daringstuddedcap_alb.Bonus3 = 0;
                daringstuddedcap_alb.Bonus3Type = (int)0;
                daringstuddedcap_alb.Bonus4 = 0;
                daringstuddedcap_alb.Bonus4Type = (int)0;
                daringstuddedcap_alb.Bonus5 = 0;
                daringstuddedcap_alb.Bonus5Type = (int)0;
                daringstuddedcap_alb.Bonus6 = 0;
                daringstuddedcap_alb.Bonus6Type = (int)0;
                daringstuddedcap_alb.Bonus7 = 0;
                daringstuddedcap_alb.Bonus7Type = (int)0;
                daringstuddedcap_alb.Bonus8 = 0;
                daringstuddedcap_alb.Bonus8Type = (int)0;
                daringstuddedcap_alb.Bonus9 = 0;
                daringstuddedcap_alb.Bonus9Type = (int)0;
                daringstuddedcap_alb.Bonus10 = 0;
                daringstuddedcap_alb.Bonus10Type = (int)0;
                daringstuddedcap_alb.ExtraBonus = 0;
                daringstuddedcap_alb.ExtraBonusType = (int)0;
                daringstuddedcap_alb.Effect = 0;
                daringstuddedcap_alb.DPS_AF = 12;
                daringstuddedcap_alb.Charges = 0;
                daringstuddedcap_alb.MaxCharges = 0;
                daringstuddedcap_alb.SpellID = 0;
                daringstuddedcap_alb.ProcSpellID = 0;
                daringstuddedcap_alb.SPD_ABS = 19;
                daringstuddedcap_alb.Realm = 0;
                daringstuddedcap_alb.MaxCount = 1;
                daringstuddedcap_alb.PackSize = 1;
                daringstuddedcap_alb.Extension = 0;
                daringstuddedcap_alb.Quality =89;
                daringstuddedcap_alb.Condition = 50000;
                daringstuddedcap_alb.MaxCondition = 50000;
                daringstuddedcap_alb.Durability = 50000;
                daringstuddedcap_alb.MaxDurability = 50000;
                daringstuddedcap_alb.PoisonCharges = 0;
                daringstuddedcap_alb.PoisonMaxCharges = 0;
                daringstuddedcap_alb.PoisonSpellID = 0;
                daringstuddedcap_alb.ProcSpellID1 = 0;
                daringstuddedcap_alb.SpellID1 = 0;
                daringstuddedcap_alb.MaxCharges1 = 0;
                daringstuddedcap_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringstuddedcap_alb);
            }
            daringstuddedgloves_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringstuddedgloves_alb");
            if (daringstuddedgloves_alb == null)
            {
                daringstuddedgloves_alb = new ItemTemplate();
                daringstuddedgloves_alb.Name = "Daring Studded Gloves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedgloves_alb.Name + ", creating it ...");
                daringstuddedgloves_alb.Level = 5;
                daringstuddedgloves_alb.Weight = 24;
                daringstuddedgloves_alb.Model = 85;
                daringstuddedgloves_alb.Object_Type = 34;
                daringstuddedgloves_alb.Item_Type = 22;
                daringstuddedgloves_alb.Id_nb = "daringstuddedgloves_alb";
                daringstuddedgloves_alb.Hand = 0;
                daringstuddedgloves_alb.Platinum = 0;
                daringstuddedgloves_alb.Gold = 0;
                daringstuddedgloves_alb.Silver = 0;
                daringstuddedgloves_alb.Copper = 0;
                daringstuddedgloves_alb.IsPickable = true;
                daringstuddedgloves_alb.IsDropable = true;
                daringstuddedgloves_alb.IsTradable = true;
                daringstuddedgloves_alb.CanDropAsLoot = false;
                daringstuddedgloves_alb.Color = 0;
                daringstuddedgloves_alb.Bonus = 0; // default bonus				
                daringstuddedgloves_alb.Bonus1 = 4;
                daringstuddedgloves_alb.Bonus1Type = (int)4;
                daringstuddedgloves_alb.Bonus2 = 0;
                daringstuddedgloves_alb.Bonus2Type = (int)0;
                daringstuddedgloves_alb.Bonus3 = 0;
                daringstuddedgloves_alb.Bonus3Type = (int)0;
                daringstuddedgloves_alb.Bonus4 = 0;
                daringstuddedgloves_alb.Bonus4Type = (int)0;
                daringstuddedgloves_alb.Bonus5 = 0;
                daringstuddedgloves_alb.Bonus5Type = (int)0;
                daringstuddedgloves_alb.Bonus6 = 0;
                daringstuddedgloves_alb.Bonus6Type = (int)0;
                daringstuddedgloves_alb.Bonus7 = 0;
                daringstuddedgloves_alb.Bonus7Type = (int)0;
                daringstuddedgloves_alb.Bonus8 = 0;
                daringstuddedgloves_alb.Bonus8Type = (int)0;
                daringstuddedgloves_alb.Bonus9 = 0;
                daringstuddedgloves_alb.Bonus9Type = (int)0;
                daringstuddedgloves_alb.Bonus10 = 0;
                daringstuddedgloves_alb.Bonus10Type = (int)0;
                daringstuddedgloves_alb.ExtraBonus = 0;
                daringstuddedgloves_alb.ExtraBonusType = (int)0;
                daringstuddedgloves_alb.Effect = 0;
                daringstuddedgloves_alb.DPS_AF = 12;
                daringstuddedgloves_alb.Charges = 0;
                daringstuddedgloves_alb.MaxCharges = 0;
                daringstuddedgloves_alb.SpellID = 0;
                daringstuddedgloves_alb.ProcSpellID = 0;
                daringstuddedgloves_alb.SPD_ABS = 19;
                daringstuddedgloves_alb.Realm = 0;
                daringstuddedgloves_alb.MaxCount = 1;
                daringstuddedgloves_alb.PackSize = 1;
                daringstuddedgloves_alb.Extension = 0;
                daringstuddedgloves_alb.Quality =89;
                daringstuddedgloves_alb.Condition = 50000;
                daringstuddedgloves_alb.MaxCondition = 50000;
                daringstuddedgloves_alb.Durability = 50000;
                daringstuddedgloves_alb.MaxDurability = 50000;
                daringstuddedgloves_alb.PoisonCharges = 0;
                daringstuddedgloves_alb.PoisonMaxCharges = 0;
                daringstuddedgloves_alb.PoisonSpellID = 0;
                daringstuddedgloves_alb.ProcSpellID1 = 0;
                daringstuddedgloves_alb.SpellID1 = 0;
                daringstuddedgloves_alb.MaxCharges1 = 0;
                daringstuddedgloves_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringstuddedgloves_alb);
            }
            daringstuddedjerkin_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringstuddedjerkin_alb");
            if (daringstuddedjerkin_alb == null)
            {
                daringstuddedjerkin_alb = new ItemTemplate();
                daringstuddedjerkin_alb.Name = "Daring Studded Jerkin";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedjerkin_alb.Name + ", creating it ...");
                daringstuddedjerkin_alb.Level = 5;
                daringstuddedjerkin_alb.Weight = 60;
                daringstuddedjerkin_alb.Model = 81;
                daringstuddedjerkin_alb.Object_Type = 34;
                daringstuddedjerkin_alb.Item_Type = 25;
                daringstuddedjerkin_alb.Id_nb = "daringstuddedjerkin_alb";
                daringstuddedjerkin_alb.Hand = 0;
                daringstuddedjerkin_alb.Platinum = 0;
                daringstuddedjerkin_alb.Gold = 0;
                daringstuddedjerkin_alb.Silver = 0;
                daringstuddedjerkin_alb.Copper = 0;
                daringstuddedjerkin_alb.IsPickable = true;
                daringstuddedjerkin_alb.IsDropable = true;
                daringstuddedjerkin_alb.IsTradable = true;
                daringstuddedjerkin_alb.CanDropAsLoot = false;
                daringstuddedjerkin_alb.Color = 0;
                daringstuddedjerkin_alb.Bonus = 0; // default bonus				
                daringstuddedjerkin_alb.Bonus1 = 12;
                daringstuddedjerkin_alb.Bonus1Type = (int)10;
                daringstuddedjerkin_alb.Bonus2 = 0;
                daringstuddedjerkin_alb.Bonus2Type = (int)0;
                daringstuddedjerkin_alb.Bonus3 = 0;
                daringstuddedjerkin_alb.Bonus3Type = (int)0;
                daringstuddedjerkin_alb.Bonus4 = 0;
                daringstuddedjerkin_alb.Bonus4Type = (int)0;
                daringstuddedjerkin_alb.Bonus5 = 0;
                daringstuddedjerkin_alb.Bonus5Type = (int)0;
                daringstuddedjerkin_alb.Bonus6 = 0;
                daringstuddedjerkin_alb.Bonus6Type = (int)0;
                daringstuddedjerkin_alb.Bonus7 = 0;
                daringstuddedjerkin_alb.Bonus7Type = (int)0;
                daringstuddedjerkin_alb.Bonus8 = 0;
                daringstuddedjerkin_alb.Bonus8Type = (int)0;
                daringstuddedjerkin_alb.Bonus9 = 0;
                daringstuddedjerkin_alb.Bonus9Type = (int)0;
                daringstuddedjerkin_alb.Bonus10 = 0;
                daringstuddedjerkin_alb.Bonus10Type = (int)0;
                daringstuddedjerkin_alb.ExtraBonus = 0;
                daringstuddedjerkin_alb.ExtraBonusType = (int)0;
                daringstuddedjerkin_alb.Effect = 0;
                daringstuddedjerkin_alb.DPS_AF = 12;
                daringstuddedjerkin_alb.Charges = 0;
                daringstuddedjerkin_alb.MaxCharges = 0;
                daringstuddedjerkin_alb.SpellID = 0;
                daringstuddedjerkin_alb.ProcSpellID = 0;
                daringstuddedjerkin_alb.SPD_ABS = 19;
                daringstuddedjerkin_alb.Realm = 0;
                daringstuddedjerkin_alb.MaxCount = 1;
                daringstuddedjerkin_alb.PackSize = 1;
                daringstuddedjerkin_alb.Extension = 0;
                daringstuddedjerkin_alb.Quality =89;
                daringstuddedjerkin_alb.Condition = 50000;
                daringstuddedjerkin_alb.MaxCondition = 50000;
                daringstuddedjerkin_alb.Durability = 50000;
                daringstuddedjerkin_alb.MaxDurability = 50000;
                daringstuddedjerkin_alb.PoisonCharges = 0;
                daringstuddedjerkin_alb.PoisonMaxCharges = 0;
                daringstuddedjerkin_alb.PoisonSpellID = 0;
                daringstuddedjerkin_alb.ProcSpellID1 = 0;
                daringstuddedjerkin_alb.SpellID1 = 0;
                daringstuddedjerkin_alb.MaxCharges1 = 0;
                daringstuddedjerkin_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringstuddedjerkin_alb);
            }
            daringstuddedleggings_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringstuddedleggings_alb");
            if (daringstuddedleggings_alb == null)
            {
                daringstuddedleggings_alb = new ItemTemplate();
                daringstuddedleggings_alb.Name = "Daring Studded Leggings";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedleggings_alb.Name + ", creating it ...");
                daringstuddedleggings_alb.Level = 5;
                daringstuddedleggings_alb.Weight = 42;
                daringstuddedleggings_alb.Model = 82;
                daringstuddedleggings_alb.Object_Type = 34;
                daringstuddedleggings_alb.Item_Type = 27;
                daringstuddedleggings_alb.Id_nb = "daringstuddedleggings_alb";
                daringstuddedleggings_alb.Hand = 0;
                daringstuddedleggings_alb.Platinum = 0;
                daringstuddedleggings_alb.Gold = 0;
                daringstuddedleggings_alb.Silver = 0;
                daringstuddedleggings_alb.Copper = 0;
                daringstuddedleggings_alb.IsPickable = true;
                daringstuddedleggings_alb.IsDropable = true;
                daringstuddedleggings_alb.IsTradable = true;
                daringstuddedleggings_alb.CanDropAsLoot = false;
                daringstuddedleggings_alb.Color = 0;
                daringstuddedleggings_alb.Bonus = 0; // default bonus				
                daringstuddedleggings_alb.Bonus1 = 4;
                daringstuddedleggings_alb.Bonus1Type = (int)4;
                daringstuddedleggings_alb.Bonus2 = 0;
                daringstuddedleggings_alb.Bonus2Type = (int)0;
                daringstuddedleggings_alb.Bonus3 = 0;
                daringstuddedleggings_alb.Bonus3Type = (int)0;
                daringstuddedleggings_alb.Bonus4 = 0;
                daringstuddedleggings_alb.Bonus4Type = (int)0;
                daringstuddedleggings_alb.Bonus5 = 0;
                daringstuddedleggings_alb.Bonus5Type = (int)0;
                daringstuddedleggings_alb.Bonus6 = 0;
                daringstuddedleggings_alb.Bonus6Type = (int)0;
                daringstuddedleggings_alb.Bonus7 = 0;
                daringstuddedleggings_alb.Bonus7Type = (int)0;
                daringstuddedleggings_alb.Bonus8 = 0;
                daringstuddedleggings_alb.Bonus8Type = (int)0;
                daringstuddedleggings_alb.Bonus9 = 0;
                daringstuddedleggings_alb.Bonus9Type = (int)0;
                daringstuddedleggings_alb.Bonus10 = 0;
                daringstuddedleggings_alb.Bonus10Type = (int)0;
                daringstuddedleggings_alb.ExtraBonus = 0;
                daringstuddedleggings_alb.ExtraBonusType = (int)0;
                daringstuddedleggings_alb.Effect = 0;
                daringstuddedleggings_alb.DPS_AF = 12;
                daringstuddedleggings_alb.Charges = 0;
                daringstuddedleggings_alb.MaxCharges = 0;
                daringstuddedleggings_alb.SpellID = 0;
                daringstuddedleggings_alb.ProcSpellID = 0;
                daringstuddedleggings_alb.SPD_ABS = 19;
                daringstuddedleggings_alb.Realm = 0;
                daringstuddedleggings_alb.MaxCount = 1;
                daringstuddedleggings_alb.PackSize = 1;
                daringstuddedleggings_alb.Extension = 0;
                daringstuddedleggings_alb.Quality =89;
                daringstuddedleggings_alb.Condition = 50000;
                daringstuddedleggings_alb.MaxCondition = 50000;
                daringstuddedleggings_alb.Durability = 50000;
                daringstuddedleggings_alb.MaxDurability = 50000;
                daringstuddedleggings_alb.PoisonCharges = 0;
                daringstuddedleggings_alb.PoisonMaxCharges = 0;
                daringstuddedleggings_alb.PoisonSpellID = 0;
                daringstuddedleggings_alb.ProcSpellID1 = 0;
                daringstuddedleggings_alb.SpellID1 = 0;
                daringstuddedleggings_alb.MaxCharges1 = 0;
                daringstuddedleggings_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringstuddedleggings_alb);
            }
            daringstuddedsleeves_alb = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "daringstuddedsleeves_alb");
            if (daringstuddedsleeves_alb == null)
            {
                daringstuddedsleeves_alb = new ItemTemplate();
                daringstuddedsleeves_alb.Name = "Daring Studded Sleeves";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + daringstuddedsleeves_alb.Name + ", creating it ...");
                daringstuddedsleeves_alb.Level = 5;
                daringstuddedsleeves_alb.Weight = 36;
                daringstuddedsleeves_alb.Model = 83;
                daringstuddedsleeves_alb.Object_Type = 34;
                daringstuddedsleeves_alb.Item_Type = 28;
                daringstuddedsleeves_alb.Id_nb = "daringstuddedsleeves_alb";
                daringstuddedsleeves_alb.Hand = 0;
                daringstuddedsleeves_alb.Platinum = 0;
                daringstuddedsleeves_alb.Gold = 0;
                daringstuddedsleeves_alb.Silver = 0;
                daringstuddedsleeves_alb.Copper = 0;
                daringstuddedsleeves_alb.IsPickable = true;
                daringstuddedsleeves_alb.IsDropable = true;
                daringstuddedsleeves_alb.IsTradable = true;
                daringstuddedsleeves_alb.CanDropAsLoot = false;
                daringstuddedsleeves_alb.Color = 0;
                daringstuddedsleeves_alb.Bonus = 0; // default bonus				
                daringstuddedsleeves_alb.Bonus1 = 4;
                daringstuddedsleeves_alb.Bonus1Type = (int)1;
                daringstuddedsleeves_alb.Bonus2 = 0;
                daringstuddedsleeves_alb.Bonus2Type = (int)0;
                daringstuddedsleeves_alb.Bonus3 = 0;
                daringstuddedsleeves_alb.Bonus3Type = (int)0;
                daringstuddedsleeves_alb.Bonus4 = 0;
                daringstuddedsleeves_alb.Bonus4Type = (int)0;
                daringstuddedsleeves_alb.Bonus5 = 0;
                daringstuddedsleeves_alb.Bonus5Type = (int)0;
                daringstuddedsleeves_alb.Bonus6 = 0;
                daringstuddedsleeves_alb.Bonus6Type = (int)0;
                daringstuddedsleeves_alb.Bonus7 = 0;
                daringstuddedsleeves_alb.Bonus7Type = (int)0;
                daringstuddedsleeves_alb.Bonus8 = 0;
                daringstuddedsleeves_alb.Bonus8Type = (int)0;
                daringstuddedsleeves_alb.Bonus9 = 0;
                daringstuddedsleeves_alb.Bonus9Type = (int)0;
                daringstuddedsleeves_alb.Bonus10 = 0;
                daringstuddedsleeves_alb.Bonus10Type = (int)0;
                daringstuddedsleeves_alb.ExtraBonus = 0;
                daringstuddedsleeves_alb.ExtraBonusType = (int)0;
                daringstuddedsleeves_alb.Effect = 0;
                daringstuddedsleeves_alb.DPS_AF = 12;
                daringstuddedsleeves_alb.Charges = 0;
                daringstuddedsleeves_alb.MaxCharges = 0;
                daringstuddedsleeves_alb.SpellID = 0;
                daringstuddedsleeves_alb.ProcSpellID = 0;
                daringstuddedsleeves_alb.SPD_ABS = 19;
                daringstuddedsleeves_alb.Realm = 0;
                daringstuddedsleeves_alb.MaxCount = 1;
                daringstuddedsleeves_alb.PackSize = 1;
                daringstuddedsleeves_alb.Extension = 0;
                daringstuddedsleeves_alb.Quality =89;
                daringstuddedsleeves_alb.Condition = 50000;
                daringstuddedsleeves_alb.MaxCondition = 50000;
                daringstuddedsleeves_alb.Durability = 50000;
                daringstuddedsleeves_alb.MaxDurability = 50000;
                daringstuddedsleeves_alb.PoisonCharges = 0;
                daringstuddedsleeves_alb.PoisonMaxCharges = 0;
                daringstuddedsleeves_alb.PoisonSpellID = 0;
                daringstuddedsleeves_alb.ProcSpellID1 = 0;
                daringstuddedsleeves_alb.SpellID1 = 0;
                daringstuddedsleeves_alb.MaxCharges1 = 0;
                daringstuddedsleeves_alb.Charges1 = 0;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(daringstuddedsleeves_alb);
            }


            #endregion

            #region defineAreas
            Albion_Statue_Area = WorldMgr.GetRegion(Albion_Statue.RegionID).AddArea(new Area.Circle("", Albion_Statue.X, Albion_Statue.Y, Albion_Statue.Z, 500));
            Albion_Statue_Area.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterStatueArea));

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
            Albion_Statue_Area.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterStatueArea));
            WorldMgr.GetRegion(Albion_Statue.RegionID).RemoveArea(Albion_Statue_Area);
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

                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Fighter"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedboots_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedcap_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedgloves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedjerkin_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedleggings_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringstuddedsleeves_alb);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Mage"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedboots_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedcap_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedgloves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedpants_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedsleeves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedvest_alb);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Disciple"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedboots_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedcap_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedgloves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedpants_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedsleeves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedvest_alb);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Elementalist"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedboots_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedcap_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedgloves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedpants_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedsleeves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringpaddedvest_alb);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Rogue"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringleatherboots_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleathercap_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleathergloves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherjerkin_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherleggings_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleathersleeves_alb);
                        quest.FinishQuest();
                    }
                    if (player.CharacterClass.BaseName == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PlayerClass.Name.Acolyte"))
                    {
                        GiveItem(Charles, quest.m_questPlayer, daringleatherboots_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleathercap_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleathergloves_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherjerkin_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleatherleggings_alb);
                        GiveItem(Charles, quest.m_questPlayer, daringleathersleeves_alb);
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
                    return LanguageMgr.GetTranslation(m_questPlayer.Client, "Alb.ChildsPlay.Description.Text1");
                }
                else if (Step == 2)
                {
                    return LanguageMgr.GetTranslation(m_questPlayer.Client, "Alb.ChildsPlay.Description.Text2");
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
            m_questPlayer.GainExperience(2, true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, 67), LanguageMgr.GetTranslation(m_questPlayer.Client, "ChildsPlay.FinishQuest.Text1"));
        }
    }
}
