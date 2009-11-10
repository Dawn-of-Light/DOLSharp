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
using System.Text;

using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
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
		public enum CLTrainerTypes : int
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
		}
		
		// Current trainer type, 0 is for normal trainers.
		public int TrainerType = 0;

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
		/// Constructs a new GameTrainer that will also train Champion level
		/// </summary>
		public GameTrainer(int CLTrainerType)
		{
			TrainerType	= CLTrainerType;
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
			switch (ServerProperties.Properties.SERV_LANGUAGE)
			{
				case "EN":
					{
						int index = -1;
						if (GuildName.Length > 0)
							index = GuildName.IndexOf(" Trainer");
						if (index >= 0)
							TrainerClassName = GuildName.Substring(0, index);
					}
					break;
				case "DE":
					TrainerClassName = GuildName;
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
			list.Add(LanguageMgr.GetTranslation(player.Client, "GameTrainer.GetExamineMessages.YouTarget", GetName(0, false)));
			list.Add(LanguageMgr.GetTranslation(player.Client, "GameTrainer.GetExamineMessages.YouExamine", GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false), TrainerClassName));
			list.Add(LanguageMgr.GetTranslation(player.Client, "GameTrainer.GetExamineMessages.RightClick"));
			return list;
		}
		#endregion

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;

			player.GainExperience(GameLiving.eXPSource.Other, 0);//levelup

			if (player.FreeLevelState == 2)
			{
				player.PlayerCharacter.LastFreeLevel = player.Level;
				//long xp = GameServer.ServerRules.GetExperienceForLevel(player.PlayerCharacter.LastFreeLevel + 3) - GameServer.ServerRules.GetExperienceForLevel(player.PlayerCharacter.LastFreeLevel + 2);
				long xp = GameServer.ServerRules.GetExperienceForLevel(player.PlayerCharacter.LastFreeLevel + 1) - GameServer.ServerRules.GetExperienceForLevel(player.PlayerCharacter.LastFreeLevel);
				//player.PlayerCharacter.LastFreeLevel = player.Level;
				player.GainExperience(GameLiving.eXPSource.Other, xp);
				player.PlayerCharacter.LastFreeLeveled = DateTime.Now;
				player.Out.SendPlayerFreeLevelUpdate();
			}

			// Turn to face player
			TurnTo(player, 10000);

			if (TrainerType > 0 && player.Level >= 50)
				player.Out.SendChampionTrainerWindow(TrainerType);

			return true;
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
			if (text == LanguageMgr.GetTranslation(player.Client, "GameTrainer.Interact.CaseRespecialize"))
			{
				if (player.Level == 5 && !player.IsLevelRespecUsed)
				{
					int specPoints = 0;

					specPoints = player.RespecAll();
					player.RespecAmountAllSkill++;

					// Assign full points returned
					if (specPoints > 0)
					{
						player.SkillSpecialtyPoints += specPoints;
						lock (player.GetStyleList().SyncRoot)
						{
							player.GetStyleList().Clear(); // Kill styles
						}
						player.UpdateSpellLineLevels(false);
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.Interact.RegainPoints", specPoints), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
					if (!player.HasAbilityToUseItem(item))
						if (player.Inventory.IsSlotsFree(item.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == true)
					{
						player.Inventory.MoveItem((eInventorySlot)item.SlotPosition, player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack), item.Count);
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.CheckAbilityToUseItem.Text1", item.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else
					{
						player.Inventory.MoveItem((eInventorySlot)item.SlotPosition, eInventorySlot.Ground, item.Count);
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.CheckAbilityToUseItem.Text1", item.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
							player.RespecAmountSingleSkill++;
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.ReceiveItem.RespecSingle"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							return true;
						}
					case "respec_full":
						{
							player.Inventory.RemoveCountFromStack(item, 1);
							player.RespecAmountAllSkill++;
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.ReceiveItem.RespecFull", item.Name), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							return true;
						}
					case "respec_realm":
						{
							player.Inventory.RemoveCountFromStack(item, 1);
							player.RespecAmountRealmSkill++;
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.ReceiveItem.RespecRealm"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
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

			IClassSpec oldClass = player.CharacterClass;

			// Player was promoted
			if (player.SetCharacterClass(classid))
			{
				player.RemoveAllStyles();

				if (messageToPlayer != "")
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.PromotePlayer.Says", this.Name, messageToPlayer), eChatType.CT_System, eChatLoc.CL_PopupWindow);
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.PromotePlayer.Upgraded", player.CharacterClass.Name), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

				player.CharacterClass.OnLevelUp(player);
				player.UpdateSpellLineLevels(true);
				player.RefreshSpecDependantSkills(true);
				player.StartPowerRegeneration();
				//player.Out.SendUpdatePlayerSpells();
				player.Out.SendUpdatePlayerSkills();
				player.Out.SendUpdatePlayer();

				// Initiate equipment
				if (gifts != null && gifts.Length > 0)
				{
					for (int i = 0; i < gifts.Length; i++)
					{
						player.ReceiveItem(this, gifts[i]);
					}
				}

				// after gifts
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.PromotePlayer.Accepted", player.CharacterClass.Profession), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

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
			ItemTemplate temp = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), template);
			if (!player.Inventory.AddTemplate(temp, 1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.AddGift.NotEnoughSpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Dismiss a player.
		/// </summary>
		/// <param name="player"></param>
		protected virtual void DismissPlayer(GamePlayer player)
		{
			SayTo(player, eChatLoc.CL_ChatWindow,
			      LanguageMgr.GetTranslation(player.Client, "GameTrainer.Train.SeekElsewhere"));
		}

		/// <summary>
		/// Offer training to the player.
		/// </summary>
		/// <param name="player"></param>
		protected virtual void OfferTraining(GamePlayer player)
		{
			SayTo(player, eChatLoc.CL_ChatWindow,
			      LanguageMgr.GetTranslation(player.Client, "GameTrainer.Train.WouldYouLikeTo"));
			player.Out.SendTrainerWindow();
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
					disabled_classes = new List<string>(ServerProperties.Properties.DISABLED_CLASSES.Split(';'));
				}

				if (disabled_classes.Contains(TrainedClass.ToString()))
					return false;
			}
			return base.AddToWorld();
		}
	}
}