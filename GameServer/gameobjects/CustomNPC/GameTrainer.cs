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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS
{
	/// <summary>
	/// The mother class for all class trainers
	/// </summary>
	public class GameTrainer : GameNPC
	{
		// List of disabled classes
		private static List<string> disabled_classes = null;
		
		// Values from live servers
		public enum eChampionTrainerType : int
		{
			Acolyte = 4,
			AlbionRogue = 2,
			Disciple = 7,
			Elementalist = 5,
			Fighter = 1,
			Forester = 12,
			Guardian = 1,
			Mage = 6,
			Magician = 11,
			MidgardRogue = 3,
			Mystic = 9,
			Naturalist = 10,
			Seer = 8,
			Stalker = 2,
			Viking = 1,
			None = 0,
		}
		
		// What kind of Champion trainer is this
		protected eChampionTrainerType m_championTrainerType = eChampionTrainerType.None;

		public virtual eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Unknown; }
		}
		/// <summary>
		/// Constructs a new GameTrainer
		/// </summary>
		public GameTrainer()
		{
		}
		/// <summary>
		/// Constructs a new GameTrainer that will also train Champion levels
		/// </summary>
		public GameTrainer(eChampionTrainerType championTrainerType)
		{
			m_championTrainerType = championTrainerType;
		}

		#region GetExamineMessages
		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			string TrainerClassName = "";
            switch (player.Client.Account.Language)
			{
                case "DE":
                    {
                        var translation = (DBLanguageNPC)LanguageMgr.GetTranslation(player.Client.Account.Language, this);

                        if (translation != null)
                        {
                            int index = -1;
                            if (translation.GuildName.Length > 0)
                                index = translation.GuildName.IndexOf("-Ausbilder");
                            if (index >= 0)
                                TrainerClassName = translation.GuildName.Substring(0, index);
                        }
                        else
                        {
                            TrainerClassName = GuildName;
                        }
                    }
                    break;
				default:
					{
						int index = -1;
						if (GuildName.Length > 0)
							index = GuildName.IndexOf(" Trainer");
						if (index >= 0)
							TrainerClassName = GuildName.Substring(0, index);
					}
					break;
			}

			IList list = new ArrayList();
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.GetExamineMessages.YouTarget", 
                                                GetName(0, false, player.Client.Account.Language, this)));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.GetExamineMessages.YouExamine",
                                                GetName(0, false, player.Client.Account.Language, this), GetPronoun(0, true, player.Client.Account.Language),
                                                GetAggroLevelString(player, false), TrainerClassName));
			list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.GetExamineMessages.RightClick"));
			return list;
		}
		#endregion

		public virtual bool CanTrain(GamePlayer player)
		{
			return player.CharacterClass.ID == (int)TrainedClass || TrainedClass == eCharacterClass.Unknown;
		}

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;

			// Turn to face player
			TurnTo(player, 10000);

			// Unknown class must be used for multitrainer
			if (CanTrain(player))
			{
				player.Out.SendTrainerWindow();
				
				player.GainExperience(GameLiving.eXPSource.Other, 0);//levelup

				if (player.FreeLevelState == 2)
				{
					player.LastFreeLevel = player.Level;
					//long xp = GameServer.ServerRules.GetExperienceForLevel(player.PlayerCharacter.LastFreeLevel + 3) - GameServer.ServerRules.GetExperienceForLevel(player.PlayerCharacter.LastFreeLevel + 2);
					long xp = player.GetExperienceNeededForLevel(player.LastFreeLevel + 1) - player.GetExperienceNeededForLevel(player.LastFreeLevel);
					//player.PlayerCharacter.LastFreeLevel = player.Level;
					player.GainExperience(GameLiving.eXPSource.Other, xp);
					player.LastFreeLeveled = DateTime.Now;
					player.Out.SendPlayerFreeLevelUpdate();
				}
			}

			if (CanTrainChampionLevels(player))
			{
				player.Out.SendChampionTrainerWindow((int)m_championTrainerType);
			}

			return true;
		}

		/// <summary>
		/// Can we offer this player training for Champion levels?
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public virtual bool CanTrainChampionLevels(GamePlayer player)
		{
			return player.Level >= player.MaxLevel && player.Champion && m_championTrainerType != eChampionTrainerType.None && m_championTrainerType != player.CharacterClass.ChampionTrainerType();
		}

		/// <summary>
		/// Talk to trainer
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text)) return false;
			GamePlayer player = source as GamePlayer;
			if (player == null) return false;
			
			//level respec for players
			if (CanTrain(player) && text == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.Interact.CaseRespecialize"))
			{
				if (player.Level == 5 && !player.IsLevelRespecUsed)
				{
					int specPoints = player.SkillSpecialtyPoints;

					player.RespecAll();

					// Assign full points returned
					if (player.SkillSpecialtyPoints > specPoints)
					{
						player.RemoveAllStyles(); // Kill styles
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.Interact.RegainPoints", (player.SkillSpecialtyPoints - specPoints)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					player.RefreshSpecDependantSkills(false);
					// Notify Player of points
					player.Out.SendUpdatePlayerSkills();
					player.Out.SendUpdatePoints();
					player.Out.SendUpdatePlayer();
					player.Out.SendTrainerWindow();
					player.SaveIntoDatabase();
				}

			}

			//Now we turn the npc into the direction of the person
			TurnTo(player, 10000);

			return true;
		}

		/// <summary>
		/// Offer respecialize to the player.
		/// </summary>
		/// <param name="player"></param>
		protected virtual void OfferRespecialize(GamePlayer player)
		{
			player.Out.SendMessage(String.Format(LanguageMgr.GetTranslation
			                                     (player.Client, "GameTrainer.Interact.Respecialize", this.Name, player.Name)),
			                       eChatType.CT_Say, eChatLoc.CL_PopupWindow);
		}

		/// <summary>
		/// Check Ability to use Item
		/// </summary>
		/// <param name="player"></param>
		protected virtual void CheckAbilityToUseItem(GamePlayer player)
		{
			// drop any equiped-non usable item, in inventory or on the ground if full
			lock (player.Inventory)
			{
				foreach (InventoryItem item in player.Inventory.EquippedItems)
				{
					if (!player.HasAbilityToUseItem(item.Template))
						if (player.Inventory.IsSlotsFree(item.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == true)
					{
						player.Inventory.MoveItem((eInventorySlot)item.SlotPosition, player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item.Count);
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.CheckAbilityToUseItem.Text1", item.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else
					{
						player.Inventory.MoveItem((eInventorySlot)item.SlotPosition, eInventorySlot.Ground, item.Count);
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.CheckAbilityToUseItem.Text1", item.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
			}
		}

		/// <summary>
		/// For Recieving Respec Stones.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			if (source == null || item == null) return false;

			GamePlayer player = source as GamePlayer;
			if (player != null)
			{
				switch (item.Id_nb)
				{
					case "respec_single":
						{
							player.Inventory.RemoveCountFromStack(item, 1);
							InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, item.Template);
							player.RespecAmountSingleSkill++;
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.ReceiveItem.RespecSingle"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							return true;
						}
					case "respec_full":
						{
							player.Inventory.RemoveCountFromStack(item, 1);
							InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, item.Template);
							player.RespecAmountAllSkill++;
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.ReceiveItem.RespecFull", item.Name), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							return true;
						}
					case "respec_realm":
						{
							player.Inventory.RemoveCountFromStack(item, 1);
							InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, item.Template);
							player.RespecAmountRealmSkill++;
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.ReceiveItem.RespecRealm"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							return true;
						}
				}
			}
			return base.ReceiveItem(source, item);
		}

		public void PromotePlayer(GamePlayer player)
		{
			if (TrainedClass != eCharacterClass.Unknown)
				PromotePlayer(player, (int)TrainedClass, "", null);
		}
		
		/// <summary>
		/// Check if Player can be Promoted
		/// </summary>
		/// <param name="player"></param>
		public virtual bool CanPromotePlayer(GamePlayer player)
		{
			var baseClass = ScriptMgr.FindCharacterBaseClass((int)TrainedClass);
			
			// Error or Base Trainer...
			if (baseClass == null || baseClass.ID == (int)TrainedClass)
				return false;
			
			if (player.Level < 5 || player.CharacterClass.ID != baseClass.ID)
				return false;
			
			if (!GlobalConstants.RACES_CLASSES_DICT.ContainsKey((eRace)player.Race) || !GlobalConstants.RACES_CLASSES_DICT[(eRace)player.Race].Contains(TrainedClass))
				return false;
			
			if (GlobalConstants.CLASS_GENDER_CONSTRAINTS_DICT.ContainsKey(TrainedClass) && GlobalConstants.CLASS_GENDER_CONSTRAINTS_DICT[TrainedClass] != player.Gender)
				return false;
			    
			return true;
		}

		/// <summary>
		/// Called to promote a player
		/// </summary>
		/// <param name="player">the player to promote</param>
		/// <param name="classid">the new classid</param>
		/// <param name="messageToPlayer">the message for the player</param>
		/// <param name="gifts">Array of inventory items as promotion gifts</param>
		/// <returns>true if successfull</returns>
		public bool PromotePlayer(GamePlayer player, int classid, string messageToPlayer, InventoryItem[] gifts)
		{

			if (player == null) return false;

			ICharacterClass oldClass = player.CharacterClass;

			// Player was promoted
			if (player.SetCharacterClass(classid))
			{
				player.RemoveAllStyles();
				player.RemoveAllAbilities();
				player.RemoveAllSpellLines();

				if (messageToPlayer != "")
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.PromotePlayer.Says", this.Name, messageToPlayer), eChatType.CT_System, eChatLoc.CL_PopupWindow);
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.PromotePlayer.Upgraded", player.CharacterClass.Name), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

				player.CharacterClass.OnLevelUp(player, player.Level);
				player.RefreshSpecDependantSkills(true);
				player.StartPowerRegeneration();
				player.Out.SendUpdatePlayerSkills();
				player.Out.SendUpdatePlayer();
				// drop any non usable item
				CheckAbilityToUseItem(player);

				// Initiate equipment
				if (gifts != null && gifts.Length > 0)
				{
					for (int i = 0; i < gifts.Length; i++)
					{
						player.ReceiveItem(this, gifts[i]);
					}
				}

				// after gifts
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.PromotePlayer.Accepted", player.CharacterClass.Profession), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

				Notify(GameTrainerEvent.PlayerPromoted, this, new PlayerPromotedEventArgs(player, oldClass));

				player.SaveIntoDatabase();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Add a gift to the player
		/// </summary>
		/// <param name="template">the template ID of the item</param>
		/// <param name="player">the player to give it to</param>
		/// <returns>true if succesful</returns>
		public virtual bool addGift(String template, GamePlayer player)
		{
			ItemTemplate temp = GameServer.Database.FindObjectByKey<ItemTemplate>(template);
			if (temp != null)
			{
				if (!player.Inventory.AddTemplate(GameInventoryItem.Create(temp), 1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.AddGift.NotEnoughSpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Other, temp);
			}
			return true;
		}

		/// <summary>
		/// If we can't train champion levels then dismiss this player
		/// </summary>
		/// <param name="player"></param>
		protected virtual void CheckChampionTraining(GamePlayer player)
		{
			if (CanTrainChampionLevels(player) == false)
			{
				SayTo(player, eChatLoc.CL_ChatWindow, LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.Train.SeekElsewhere"));
			}
		}

		/// <summary>
		/// Offer training to the player.
		/// </summary>
		/// <param name="player"></param>
		protected virtual void OfferTraining(GamePlayer player)
		{
			SayTo(player, eChatLoc.CL_ChatWindow, LanguageMgr.GetTranslation(player.Client.Account.Language, "GameTrainer.Train.WouldYouLikeTo"));
		}
		
		/// <summary>
		/// No trainer for disabled classes
		/// </summary>
		/// <returns></returns>
		public override bool AddToWorld()
		{
			if (!string.IsNullOrEmpty(ServerProperties.Properties.DISABLED_CLASSES))
			{
				if (disabled_classes == null)
				{
					// creation of disabled_classes list.
					disabled_classes = Util.SplitCSV(ServerProperties.Properties.DISABLED_CLASSES).ToList();
				}

				if (disabled_classes.Contains(TrainedClass.ToString()))
					return false;
			}
			return base.AddToWorld();
		}
	}
}