using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using DOL.GS;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.Scripts;
using DOL.GS.Styles;
using log4net;
using DOL.Events;

namespace DOL.GS
{
    public class ArtifactManager
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// stores all ArtifactBonus entries for fast access
        /// </summary>
        public static Hashtable m_ArtifactBonusByID_Nb = new Hashtable();

        /// <summary>
        /// stores all ArtifactLevel entries for fast access
        /// </summary>
        public static Hashtable m_ArtifactLevelByID_Nb = new Hashtable();

        /// <summary>
        /// Performs database queries selecting all entries in ArtifactLevel, ArtifactBonus
        /// and ArtifactExperienceXItem and stores them in hashtables afterwards
        /// Also initiates a timer that saves changed ArtifactExperienceXItem entries every hour
        /// to the m_ArtifactExperienceByInventoryItem_ID table
        /// </summary>
        public static void LoadArtifacts()
        {
            DataObject[] dbo = GameServer.Database.SelectAllObjects(typeof(ArtifactLevel));
            for (int i = 0; i < dbo.Length; i++)
            {
                ArtifactLevel entry = dbo[i] as ArtifactLevel;
                m_ArtifactLevelByID_Nb[entry.Id_nb] = entry;
            }

            //dbo = GameServer.Database.SelectAllObjects(typeof(ArtifactBonus));
			//for (int i = 0; i < dbo.Length; i++)
			//{
			//    ArtifactBonus entry = dbo[i] as ArtifactBonus;
			//    ArrayList artibonus = m_ArtifactBonusByID_Nb[entry.Id_nb] as ArrayList;
			//    if (artibonus == null)
			//    {
			//        artibonus = new ArrayList();
			//    }
			//    artibonus.Add(entry);
			//    m_ArtifactBonusByID_Nb[entry.Id_nb] = artibonus;
			//}
        }

        /// <summary>
        /// returns the same inventoryitem with the bonuses the artifact would have at given level
        /// </summary>
        /// <param name="iitem"></param>
        /// <returns></returns>
        public static InventoryItem AssignArtifactBonuses(InventoryItem iitem)
        {
			//if (!IsArtifact(iitem))
			//{
			//    log.Warn("Error: given iitem is not an artifact objectid: " + iitem.ObjectId + " id_nb: " + iitem.Id_nb);
			//    return iitem;
			//}

			//ArrayList abonuslist = m_ArtifactBonusByID_Nb[iitem.Id_nb] as ArrayList;
			//ArtifactLevel alevel = m_ArtifactLevelByID_Nb[iitem.Id_nb] as ArtifactLevel;

			//if (abonuslist == null)
			//{
			//    log.Warn("Error: given iitem is an artifact but ArtifactBonus values arent populated objectid: " + iitem.ObjectId + " id_nb: " + iitem.Id_nb);
			//    return iitem;
			//}

			//if (alevel == null)
			//{
			//    log.Warn("Error: given iitem is an artifact but ArtifactBonus values arent populated objectid: " + iitem.ObjectId + " id_nb: " + iitem.Id_nb);
			//    return iitem;
			//}

			//int level = (int)(iitem.Experience / alevel.Experience);
			////log.Info("Info: Calculated artifact level: " + level + " objectid: " + iitem.ObjectId + " id_nb: " + iitem.Id_nb);

			//ArrayList applyingBonus = new ArrayList();

			//foreach (ArtifactBonus abonus in abonuslist)
			//{
			//    if (abonus.Level <= level)
			//        applyingBonus.Add(abonus);
			//}
			////log.Info("Info: Found " + applyingBonus.Count + " bonuses that apply to artifact at given level objectid: " + iitem.ObjectId + " id_nb: " + iitem.Id_nb);

			//abonuslist = null;

			//for (int i = 0; i < applyingBonus.Count; i++)
			//{
			//    ArtifactBonus abonus = applyingBonus[i] as ArtifactBonus;
			//    if (abonus == null)
			//    {
			//        log.Warn("Error: failed to cast arraylist[" + i + "] entry to artifactbonus objectid: " + iitem.ObjectId + " id_nb: " + iitem.Id_nb);
			//        continue;
			//    }

			//    //log.Info("Info: Added bonustype " + (eProperty)abonus.BonusType + " amount: " + abonus.BonusAmount + " objectid: " + iitem.ObjectId + " id_nb: " + iitem.Id_nb + " and index[i] = " + i);

			//    if (abonus.BonusType >= 0)
			//    {
			//        switch (i)
			//        {
			//            case 0:
			//                iitem.Bonus1 = abonus.BonusAmount;
			//                iitem.Bonus1Type = abonus.BonusType;
			//                break;
			//            case 1:
			//                iitem.Bonus2 = abonus.BonusAmount;
			//                iitem.Bonus2Type = abonus.BonusType;
			//                break;
			//            case 2:
			//                iitem.Bonus3 = abonus.BonusAmount;
			//                iitem.Bonus3Type = abonus.BonusType;
			//                break;
			//            case 3:
			//                iitem.Bonus4 = abonus.BonusAmount;
			//                iitem.Bonus4Type = abonus.BonusType;
			//                break;
			//            case 4:
			//                iitem.Bonus5 = abonus.BonusAmount;
			//                iitem.Bonus5Type = abonus.BonusType;
			//                break;
			//            case 5:
			//                iitem.Bonus6 = abonus.BonusAmount;
			//                iitem.Bonus6Type = abonus.BonusType;
			//                break;
			//            case 6:
			//                iitem.Bonus7 = abonus.BonusAmount;
			//                iitem.Bonus7Type = abonus.BonusType;
			//                break;
			//            case 7:
			//                iitem.Bonus8 = abonus.BonusAmount;
			//                iitem.Bonus8Type = abonus.BonusType;
			//                break;
			//            case 8:
			//                iitem.Bonus9 = abonus.BonusAmount;
			//                iitem.Bonus9Type = abonus.BonusType;
			//                break;
			//            case 9:
			//                iitem.Bonus10 = abonus.BonusAmount;
			//                iitem.Bonus10Type = abonus.BonusType;
			//                break;
			//            case 10:
			//                iitem.ExtraBonus = abonus.BonusAmount;
			//                iitem.ExtraBonusType = abonus.BonusType;
			//                break;
			//        }
			//    }
			//    else if (abonus.BonusType < 0)
			//    {
			//        switch (abonus.BonusType)
			//        {
			//            case -10:
			//                iitem.ProcSpellID = abonus.BonusAmount;
			//                break;
			//            case -20:
			//                iitem.ProcSpellID1 = abonus.BonusAmount;
			//                break;
			//            case -30:
			//                iitem.SpellID = abonus.BonusAmount;
			//                iitem.MaxCharges = -1;
			//                break;
			//            case -40:
			//                iitem.SpellID1 = abonus.BonusAmount;
			//                iitem.MaxCharges1 = -1;
			//                break;
			//        }

			//    }

            //}

            //log.Info("Info: Returning item objectid: " + iitem.ObjectId + " id_nb: " + iitem.Id_nb);
            return iitem;
        }

        /// <summary>
        /// removes bonuses of the artifact before it leveled and adds them all again after it leveld
        /// </summary>
        /// <param name="olditem">the arti before levelup</param>
        /// <param name="newitem">the arti after levelup</param>
        /// <param name="player">the player</param>
        public static void UpdateArtifact(InventoryItem olditem, InventoryItem newitem, GamePlayer player)
        {
            // Remove all old bonuses
            player.ItemBonus[olditem.Bonus1] = 0;
            player.ItemBonus[olditem.Bonus1Type] = 0;
            player.ItemBonus[olditem.Bonus2] = 0;
            player.ItemBonus[olditem.Bonus2Type] = 0;
            player.ItemBonus[olditem.Bonus3] = 0;
            player.ItemBonus[olditem.Bonus3Type] = 0;
            player.ItemBonus[olditem.Bonus4] = 0;
            player.ItemBonus[olditem.Bonus4Type] = 0;
            player.ItemBonus[olditem.Bonus5] = 0;
            player.ItemBonus[olditem.Bonus5Type] = 0;
            player.ItemBonus[olditem.Bonus6] = 0;
            player.ItemBonus[olditem.Bonus6Type] = 0;
            player.ItemBonus[olditem.Bonus7] = 0;
            player.ItemBonus[olditem.Bonus7Type] = 0;
            player.ItemBonus[olditem.Bonus8] = 0;
            player.ItemBonus[olditem.Bonus8Type] = 0;
            player.ItemBonus[olditem.Bonus9] = 0;
            player.ItemBonus[olditem.Bonus9Type] = 0;
            player.ItemBonus[olditem.Bonus10] = 0;
            player.ItemBonus[olditem.Bonus10Type] = 0;

            // Add new bonuses
            player.ItemBonus[newitem.Bonus1Type] = newitem.Bonus1;
            player.ItemBonus[newitem.Bonus2Type] = newitem.Bonus2;
            player.ItemBonus[newitem.Bonus3Type] = newitem.Bonus3;
            player.ItemBonus[newitem.Bonus4Type] = newitem.Bonus4;
            player.ItemBonus[newitem.Bonus5Type] = newitem.Bonus5;
            player.ItemBonus[newitem.Bonus6Type] = newitem.Bonus6;
            player.ItemBonus[newitem.Bonus7Type] = newitem.Bonus7;
            player.ItemBonus[newitem.Bonus8Type] = newitem.Bonus8;
            player.ItemBonus[newitem.Bonus9Type] = newitem.Bonus9;
            player.ItemBonus[newitem.Bonus10Type] = newitem.Bonus10;
            player.ItemBonus[newitem.ExtraBonusType] = newitem.ExtraBonus;

            if (player.ObjectState == DOL.GS.GameObject.eObjectState.Active)
            {
                // TODO: remove when properties system is finished
                player.Out.SendCharStatsUpdate();
                player.Out.SendCharResistsUpdate();
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendEncumberance();
                player.Out.SendUpdatePlayerSkills();
                player.UpdatePlayerStatus();

                if (player.IsAlive)
                {
                    if (player.Health < player.MaxHealth) player.StartHealthRegeneration();
                    else if (player.Health > player.MaxHealth) player.Health = player.MaxHealth;

                    if (player.Mana < player.MaxMana) player.StartPowerRegeneration();
                    else if (player.Mana > player.MaxMana) player.Mana = player.MaxMana;

                    if (player.Endurance < player.MaxEndurance) player.StartEnduranceRegeneration();
                    else if (player.Endurance > player.MaxEndurance) player.Endurance = player.MaxEndurance;
                }
            }
        }

        /// <summary>
        /// Checks if an ArtifactsBonus entry can be found for Id_nb of given inventoryitem
        /// </summary>
        /// <param name="iitem">the given Inventoryitem</param>
        /// <returns>true if an entry can be found false otherwise</returns>
        public static bool IsArtifact(InventoryItem iitem)
        {
            ArrayList artibonus = m_ArtifactBonusByID_Nb[iitem.Id_nb] as ArrayList;
            if (artibonus == null)
            {
                //log.Info("Info: item check turned out to be FALSE for objectid: " + iitem.ObjectId + " id_nb: " + iitem.Id_nb);
                return false;
            }
            else
            {
                //log.Info("Info: item check turned out to be TRUE for objectid: " + iitem.ObjectId + " id_nb: " + iitem.Id_nb);
                return true;
            }
        }

        /// <summary>
        /// Makes all Artifacts found on a player gain experience and saves the changes to the hashtable
        /// </summary>
        /// <param name="player">the player</param>
        /// <param name="experience">the amount of xp</param>
        public static void GainArtifactExperience(GamePlayer player, long experience)
        {
            //log.Info("Info: Gainartifactexperience was called player:" + player.Name + " experience: " + experience);
            ArrayList leveledArtis = new ArrayList();

            foreach (InventoryItem iitem in player.Inventory.AllItems)
            {
                // If item is NOT Artifact and NOT Equipped we do not process it.
                if (!(player.Inventory as GamePlayerInventory).IsEquippedSlot((eInventorySlot)iitem.SlotPosition))
                {
                    //log.Info("Info: Artifact not equipped");
                }
                else if (IsArtifact(iitem) && IsActivated(iitem))
                {
                    //log.Info("Info: checking item: " + iitem.ObjectId + " with id_nb: " + iitem.Id_nb + " while looping through player: " + player.Name + " artifacts.");

                    ArtifactLevel alevel = m_ArtifactLevelByID_Nb[iitem.Id_nb] as ArtifactLevel;
                    if (alevel == null)
                    {
                        //log.Error("ArtifactLevel with Id_nb " + iitem.Id_nb + " not found, returning.");
                        return;
                    }

                    long oldlevel = GetArtifactLevel(iitem);
                    if (oldlevel <= 1)
                        oldlevel = 1;

                    //Added by Dinberg: check to prevent them going over level 10.
                    if (oldlevel < 10)
                    {
                        if ((experience / 1000) < 1)
                        {
                            iitem.Experience += 1;
                        }
                        else
                        {
                            iitem.Experience += experience / 1000;
                        }
                        if (iitem.Experience > alevel.Experience)
                            iitem.Experience = alevel.Experience;
                        //log.Info("Info: gained experience: " + experience + " item got: " + (experience / 1000) + " item: " + iitem.ObjectId + " id_nb: " + iitem.Id_nb);

                        long currentLevel = GetArtifactLevel(iitem);
                        if (currentLevel <= 1)
                            currentLevel = 1;
                        //Dinberg: preventing 'hopping' to level 11 with a big enough dose to skip level 10.
                        if (currentLevel > 10) { iitem.Experience = 15000000; }

                        //long xptogo = alevel.Experience - (axitem.Experience - currentLevel * alevel.Experience);

                        if (oldlevel < currentLevel)
                        {
                            leveledArtis.Add(iitem);
                            //log.Info("Info: artifact leveled item: " + iitem.ObjectId + " id_nb: " + iitem.Id_nb);
                        }

                        player.Out.SendMessage("Your " + iitem.Name + " has gained experience.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    }
                }

                if (leveledArtis.Count > 0)
                {
                    for (int i = 0; i <= leveledArtis.Count - 1; i++)
                    {
                        InventoryItem olditem = leveledArtis[i] as InventoryItem;
                        InventoryItem newitem = AssignArtifactBonuses(olditem);
                        UpdateArtifact(olditem, newitem, player);
                    }
                    player.Out.SendMessage(leveledArtis.Count + " of you Artifacts have gained a level.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                }
            }
        }
        public static bool IsActivated(InventoryItem iitem)
        {
            if (iitem.IsActivated)
                return true;
            else
                return false;
        }
        public static bool ActivateArtifcat(InventoryItem iitem)
        {
            if (iitem == null || !IsArtifact(iitem))
            {
                iitem.IsActivated = false;
                return false;
            }

            iitem.IsActivated = true;
            return true;
        }


        public static long GetArtifactLevel(InventoryItem iitem)
        {
            if (iitem == null || !IsArtifact(iitem))
                return 0;

            ArtifactLevel alevel = m_ArtifactLevelByID_Nb[iitem.Id_nb] as ArtifactLevel;
            if (alevel == null)
            {
                log.Error("ArtifactLevel with Id_nb " + iitem.Id_nb + " not found, returning.");
                return 0;
            }
            long expToLevel = (alevel.Experience / 10);
            if (expToLevel <= 0)
            {
                expToLevel = 0;
            }
            long level = 0;
            if (expToLevel > iitem.Experience)
            {
                if (iitem.Experience > 5000)
                    level = 1;
                else
                    level = 0;
            }
            else
            {
                long templvl = (iitem.Experience / expToLevel);
                if (templvl <= 0)
                    level = 0;
                else
                    level = templvl;
            }
            return level;
        }                                   

        /// <summary>
        /// Events for less code changes
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        [ScriptLoadedEvent]
        public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
        {
            GameEventMgr.AddHandler(GamePlayerEvent.GainedExperience, new DOLEventHandler(PlayerGainedExperience));
        }

        [ScriptUnloadedEvent]
        public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            GameEventMgr.RemoveHandler(GamePlayerEvent.GainedExperience, new DOLEventHandler(PlayerGainedExperience));
        }

        public static void PlayerGainedExperience(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;
            GainedExperienceEventArgs gargs = args as GainedExperienceEventArgs;
            if (player == null || gargs == null || (player.Inventory as GamePlayerInventory).Artifacts.Count <= 0)
                return;

            GainArtifactExperience(player, gargs.ExpBase + gargs.ExpCampBonus + gargs.ExpGroupBonus + gargs.ExpOutpostBonus);
        }
    }
}
