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
using System.Reflection;
using System.Text;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.Quests;
using DOL.GS.RealmAbilities;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.Language;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// delve button shift+i = detail of spell object...
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.DetailRequest, "Handles detail display", eClientStatus.PlayerInGame)]
	public class DetailDisplayHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			if (client == null || client.Player == null) 
				return;

			ushort objectType = packet.ReadShort();

			uint extraID = 0;
			if (client.Version >= GameClient.eClientVersion.Version186)
			{
				extraID = packet.ReadInt();
			}

			ushort objectID = packet.ReadShort();

			string caption = "";
			var objectInfo = new List<string>();

			/*
			Type    Description         Id
			1       Inventory item      Slot (ie. 0xC for 2 handed weapon)
			2       Spell               spell level + spell line ID * 100 (starting from 0)
			3       ???
			4       Merchant item       Slot (divide by 30 to get page)
			5       Buff/effect         The buff id (each buff has a unique id)
			6       Style               style list index = ID-100-abilities count
			7       Trade window        position in trade window (starting form 0)
			8       Ability             100+position in players abilities list (?)
			9       Trainers skill      position in trainers window list
			10		Market Search		slot?
			19		Reward Quest
			 */

			ChatUtil.SendDebugMessage(client, string.Format("Delve objectType={0}, objectID={1}, extraID={2}", objectType, objectID, extraID));

			ItemTemplate item = null;
			InventoryItem invItem = null;

			var snapSkills = client.Player.GetAllUsableSkills();
			var snapLists = client.Player.GetAllUsableListSpells();
			// find the first non-specialization index.
			int indexAtSpecOid = Math.Max(0, snapSkills.FindIndex(it => (it.Item1 is Specialization) == false)) + (objectID - 100);
			
			switch (objectType)
			{
					#region Inventory Item
				case 1: //Display Infos on inventory item
				case 10: // market search
					{
						if (objectType == 1)
						{
							IGameInventoryObject invObject = client.Player.TargetObject as IGameInventoryObject;

							// first try any active inventory object
							if (invItem == null)
							{
								if (client.Player.ActiveInventoryObject != null)
								{
									invObject = client.Player.ActiveInventoryObject;

									if (invObject != null && invObject.GetClientInventory(client.Player) != null)
									{
										invObject.GetClientInventory(client.Player).TryGetValue(objectID, out invItem);
									}
								}
							}

							// finally try direct inventory access
							if (invItem == null)
							{
								invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
							}

							// Failed to get any inventory
							if (invItem == null)
								return;

						}
						else if (objectType == 10)
						{
							List<InventoryItem> list = client.Player.TempProperties.getProperty<object>(MarketExplorer.EXPLORER_ITEM_LIST, null) as List<InventoryItem>;
							if (list == null)
							{
								list = client.Player.TempProperties.getProperty<object>("TempSearchKey", null) as List<InventoryItem>;
								if (list == null)
									return;
							}

							if (objectID >= list.Count)
								return;

							invItem = list[objectID];

							if (invItem == null)
								return;
						}

						// Aredhel: Start of a more sophisticated item delve system.
						// The idea is to have every item inherit from an item base class,
						// this base class will provide a method
						//
						// public virtual void Delve(List<String>, GamePlayer player)
						//
						// which can be overridden in derived classes to provide additional
						// information. Same goes for spells, just add the spell delve
						// in the Delve() hierarchy. This will on one hand make this class
						// much more concise (1800 lines at the time of this writing) and
						// on the other hand the whole delve system much more flexible, for
						// example when adding new item types (artifacts, for example) you
						// provide *only* an overridden Delve() method, use the base
						// Delve() and you're done, spells, charges and everything else.

						// Let the player class create the appropriate item to delve
						caption = invItem.Name;

						if (client.Player.DelveItem<InventoryItem>(invItem, objectInfo))
							break;

						#region Old Delve

						if (invItem is InventoryArtifact)
						{
							List<String> delve = new List<String>();
							(invItem as InventoryArtifact).Delve(delve, client.Player);

							foreach (string line in delve)
								objectInfo.Add(line);

							break;
						}

						//**********************************
						//show crafter name
						//**********************************
						if (invItem.IsCrafted)
						{
							objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.CrafterName", invItem.Creator));
							objectInfo.Add(" ");
						}
						else if (invItem.Description != null && invItem.Description != "")
						{
							objectInfo.Add(invItem.Description);
							objectInfo.Add(" ");
						}

						if ((invItem.Object_Type >= (int)eObjectType.GenericWeapon) && (invItem.Object_Type <= (int)eObjectType._LastWeapon) ||
						    invItem.Object_Type == (int)eObjectType.Instrument)
						{
							WriteUsableClasses(objectInfo, invItem, client);
							WriteMagicalBonuses(objectInfo, invItem, client, false);
							WriteClassicWeaponInfos(objectInfo, invItem, client);
						}

						if (invItem.Object_Type >= (int)eObjectType.Cloth && invItem.Object_Type <= (int)eObjectType.Scale)
						{
							WriteUsableClasses(objectInfo, invItem, client);
							WriteMagicalBonuses(objectInfo, invItem, client, false);
							WriteClassicArmorInfos(objectInfo, invItem, client);
						}

						if (invItem.Object_Type == (int)eObjectType.Shield)
						{
							WriteUsableClasses(objectInfo, invItem, client);
							WriteMagicalBonuses(objectInfo, invItem, client, false);
							WriteClassicShieldInfos(objectInfo, invItem, client);
						}

						if (invItem.Object_Type == (int)eObjectType.Magical || invItem.Object_Type == (int)eObjectType.AlchemyTincture || invItem.Object_Type == (int)eObjectType.SpellcraftGem)
						{
							WriteMagicalBonuses(objectInfo, invItem, client, false);
						}

						//***********************************
						//shows info for Poison Potions
						//***********************************
						if (invItem.Object_Type == (int)eObjectType.Poison)
						{
							WritePoisonInfo(objectInfo, invItem, client);
						}

						if (invItem.Object_Type == (int)eObjectType.Magical && invItem.Item_Type == (int)eInventorySlot.FirstBackpack) // potion
						{
							WritePotionInfo(objectInfo, invItem, client);
						}
						else if (invItem.CanUseEvery > 0)
						{
							// Items with a reuse timer (aka cooldown).
							objectInfo.Add(" ");

							int minutes = invItem.CanUseEvery / 60;
							int seconds = invItem.CanUseEvery % 60;

							if (minutes == 0)
							{
								objectInfo.Add(String.Format("Can use item every: {0} sec", seconds));
							}
							else
							{
								objectInfo.Add(String.Format("Can use item every: {0}:{1:00} min", minutes, seconds));
							}

							// objectInfo.Add(String.Format("Can use item every: {0:00}:{1:00}", minutes, seconds));

							int cooldown = invItem.CanUseAgainIn;

							if (cooldown > 0)
							{
								minutes = cooldown / 60;
								seconds = cooldown % 60;

								if (minutes == 0)
								{
									objectInfo.Add(String.Format("Can use again in: {0} sec", seconds));
								}
								else
								{
									objectInfo.Add(String.Format("Can use again in: {0}:{1:00} min", minutes, seconds));
								}

								// objectInfo.Add(String.Format("Can use again in: {0:00}:{1:00}", minutes, seconds));
							}
						}

						if (!invItem.IsDropable || !invItem.IsPickable || invItem.IsIndestructible)
							objectInfo.Add(" ");

						if (!invItem.IsPickable)
							objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.CannotTraded"));

						if (!invItem.IsDropable)
							objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.CannotSold"));

						if (invItem.IsIndestructible)
							objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.CannotDestroyed"));


						if (invItem.BonusLevel > 0)
						{
							objectInfo.Add(" ");
							objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.BonusLevel", invItem.BonusLevel));

						}

						//Add admin info
						if (client.Account.PrivLevel > 1)
						{
							WriteTechnicalInfo(objectInfo, client, invItem);
						}

						break;

						#endregion Old Delve
					}
					#endregion
					#region Spell
				case 2: //spell List
					{
						int lineID = objectID / 100;
						int spellID = objectID % 100;

						Skill sk = null;
						SpellLine sl = null;
						
						// is spelline in index ?
						if (lineID < snapLists.Count)
						{
							int index = snapLists[lineID].Item2.FindIndex(s => s is Spell ? 
							                                                   s.Level == spellID 
							                                                   : (s is Styles.Style ? ((Styles.Style)s).SpecLevelRequirement == spellID
							                                                      : (s is Ability ? ((Ability)s).SpecLevelRequirement == spellID : false)));
							
							if (index > -1)
							{
								sk = snapLists[lineID].Item2[index];
							}
							
							sl = snapLists[lineID].Item1;
						}
						
						if (sk is Spell && sl != null)
						{
							caption = sk.Name;
							WriteSpellInfo(objectInfo, (Spell)sk, sl, client);
						}
						else if (sk is Styles.Style)
						{
							caption = sk.Name;
							WriteStyleInfo(objectInfo, (Style)sk, client);
						}
						else if (sk is Ability)
						{
							caption = sk.Name;
							Ability abil = (Ability)sk;
							
							if (abil.DelveInfo.Count > 0)
								objectInfo.AddRange(abil.DelveInfo);
							else
								objectInfo.Add("There is no special information.");
						}
		
						break;
					}
				case 3: //spell Hybrid
					{	
						
	
						SpellLine spellLine = null;
						Spell spell = null;
	
						// are we in list ?
						if (indexAtSpecOid < snapSkills.Count)
						{
							spell = (Spell)snapSkills[indexAtSpecOid].Item1;
							spellLine = (SpellLine)snapSkills[indexAtSpecOid].Item2;
						}
						
						if (spell == null || spellLine == null)
							return;
	
						caption = spell.Name;
						WriteSpellInfo(objectInfo, spell, spellLine, client);	
						break;
					}
					#endregion
					#region Merchant / RewardQuest
				case 4: //Display Infos on Merchant objects
				case 19: //Display Info quest reward
					{
						if (objectType == 4)
						{
							GameMerchant merchant = null;
							if (client.Player.TargetObject != null && client.Player.TargetObject is GameMerchant)
								merchant = (GameMerchant)client.Player.TargetObject;
							if (merchant == null)
								return;

							byte pagenumber = (byte)(objectID / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS);
							short slotnumber = (short)(objectID % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS);

							item = merchant.TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
						}
						else if (objectType == 19)
						{
							ushort questID = (ushort)((extraID << 12) | (ushort)(objectID >> 4));
							int index = objectID & 0x0F;

							GameLiving questGiver = null;
							if (client.Player.TargetObject != null && client.Player.TargetObject is GameLiving)
								questGiver = (GameLiving)client.Player.TargetObject;

							ChatUtil.SendDebugMessage(client, "Quest ID: " + questID);

							if (questID == 0)
								return; // questID == 0, wrong ID ?
							
							if (questID <= DataQuest.DATAQUEST_CLIENTOFFSET)
							{
								AbstractQuest q = client.Player.IsDoingQuest(QuestMgr.GetQuestTypeForID(questID));

								if (q == null)
								{
									// player not doing quest, most likely on offer screen
									if (questGiver != null)
									{
										try
										{
											q = (AbstractQuest)Activator.CreateInstance(QuestMgr.GetQuestTypeForID(questID), new object[] { client.Player, 1 });
										}
										catch
										{
											// we tried!
										}
									}

									if (q == null)
									{
										ChatUtil.SendDebugMessage(client, "Can't find or create quest!");
										return;
									}
								}

								if (!(q is RewardQuest))
									return; // this is not new quest

								List<ItemTemplate> rewards = null;
								if (index < 8)
									rewards = (q as RewardQuest).Rewards.BasicItems;
								else
								{
									rewards = (q as RewardQuest).Rewards.OptionalItems;
									index -= 8;
								}
								if (rewards != null && index >= 0 && index < rewards.Count)
								{
									item = rewards[index];
								}
							}
							else // Data quest support, check for RewardQuest type
							{
								DataQuest dq = null;

								foreach (DBDataQuest d in GameObject.DataQuestCache)
								{
									if (d.ID == questID - DataQuest.DATAQUEST_CLIENTOFFSET)
									{
										dq = new DataQuest(d);
										break;
									}
								}

								if (dq != null && dq.StartType == DataQuest.eStartType.RewardQuest)
								{
									List<ItemTemplate> rewards = null;
									if (index < 8)
										rewards = dq.FinalRewards;
									else
									{
										rewards = dq.OptionalRewards;
										index -= 8;
									}
									if (rewards != null && index >= 0 && index < rewards.Count)
									{
										item = rewards[index];
									}
								}
							}

						}


						if (item == null)
							return;

						caption = item.Name;

						if (client.Player.DelveItem<ItemTemplate>(item, objectInfo))
							break;

						#region Old Delve

						// fallback to old delve

						if (item.Item_Type == (int)eInventorySlot.Horse)
						{
							WriteHorseInfo(objectInfo, item, client, "");
						}

						if ((item.Item_Type == (int)eInventorySlot.HorseBarding || item.Item_Type == (int)eInventorySlot.HorseArmor) && item.Level > 0)
						{
							objectInfo.Add(" ");//empty line
							objectInfo.Add(" ");//empty line
							objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.ChampionLevel", item.Level));
						}

						if ((item.Object_Type >= (int)eObjectType.GenericWeapon) && (item.Object_Type <= (int)eObjectType.MaulerStaff) ||
						    item.Object_Type == (int)eObjectType.Instrument)
						{
							WriteUsableClasses(objectInfo, item, client);
							WriteMagicalBonuses(objectInfo, item, client, false);
							WriteClassicWeaponInfos(objectInfo, GameInventoryItem.Create(item), client);
						}

						if (item.Object_Type >= (int)eObjectType.Cloth && item.Object_Type <= (int)eObjectType.Scale)
						{
							WriteUsableClasses(objectInfo, item, client);
							WriteMagicalBonuses(objectInfo, item, client, false);
							WriteClassicArmorInfos(objectInfo, GameInventoryItem.Create(item), client);
						}

						if (item.Object_Type == (int)eObjectType.Shield)
						{
							WriteUsableClasses(objectInfo, item, client);
							WriteMagicalBonuses(objectInfo, item, client, false);
							WriteClassicShieldInfos(objectInfo, item, client);
						}

						if ((item.Item_Type != (int)eInventorySlot.Horse && item.Object_Type == (int)eObjectType.Magical) || item.Object_Type == (int)eObjectType.AlchemyTincture || item.Object_Type == (int)eObjectType.SpellcraftGem)
						{
							WriteMagicalBonuses(objectInfo, item, client, false);
						}

						if (item.Object_Type == (int)eObjectType.Poison)
						{
							WritePoisonInfo(objectInfo, item, client);
						}

						if (item.Object_Type == (int)eObjectType.Magical && item.Item_Type == 40) // potion
							WritePotionInfo(objectInfo, item, client);

						//Add admin info
						if (client.Account.PrivLevel > 1)
						{
							WriteTechnicalInfo(objectInfo, client, GameInventoryItem.Create(item), item.MaxDurability, item.MaxCondition);
						}
						break;

						#endregion Old Delve
					}
					#endregion
					#region Effect
				case 5: //icons on top (buffs/dots)
					{
						IGameEffect foundEffect = null;
						lock (client.Player.EffectList)
						{
							foreach (IGameEffect effect in client.Player.EffectList)
							{
								if (effect.InternalID == objectID)
								{
									foundEffect = effect;
									break;
								}
							}
						}

						if (foundEffect == null) break;

						caption = foundEffect.Name;
						objectInfo.AddRange(foundEffect.DelveInfo);

						if (client.Account.PrivLevel > 1 && foundEffect is GameSpellEffect)
						{
							if ((foundEffect as GameSpellEffect).Spell != null)
							{
								if (client.Account.PrivLevel > 1)
								{
									objectInfo.Add(" ");
									objectInfo.Add("----------Technical informations----------");
									objectInfo.Add("Line: " + ((foundEffect as GameSpellEffect).SpellHandler == null ? "unknown" : (foundEffect as GameSpellEffect).SpellHandler.SpellLine.Name));
									objectInfo.Add("SpellID: " + (foundEffect as GameSpellEffect).Spell.ID);
									objectInfo.Add("Type: " + (foundEffect as GameSpellEffect).Spell.SpellType);
									objectInfo.Add("ClientEffect: " + (foundEffect as GameSpellEffect).Spell.ClientEffect);
									objectInfo.Add("Icon: " + (foundEffect as GameSpellEffect).Spell.Icon);
									if ((foundEffect as GameSpellEffect).SpellHandler != null)
										objectInfo.Add("HasPositiveEffect: " + (foundEffect as GameSpellEffect).SpellHandler.HasPositiveEffect);
									objectInfo.Add("Disabled: " + (foundEffect as GameSpellEffect).IsDisabled);
								}
							}
						}


						break;
					}
					#endregion
					#region Style
				case 6: //style
					{
						Style style = null;
						// are we in list ?
						if (indexAtSpecOid < snapSkills.Count)
						{
							style = (Style)snapSkills[indexAtSpecOid].Item1;
						}
						
						if (style == null)
							return;

						caption = style.Name;

						WriteStyleInfo(objectInfo, style, client);
						break;
					}
					#endregion
					#region Trade Window
				case 7: //trade windows
					{
						ITradeWindow playerTradeWindow = client.Player.TradeWindow;
						if (playerTradeWindow == null)
							return;

						if (playerTradeWindow.PartnerTradeItems != null && objectID < playerTradeWindow.PartnerItemsCount)
							invItem = (InventoryItem)playerTradeWindow.PartnerTradeItems[objectID];

						if (invItem == null)
							return;

						// Let the player class create the appropriate item to delve
						caption = invItem.Name;

						if (client.Player.DelveItem<InventoryItem>(invItem, objectInfo))
							break;

						#region Old Delve
						// fallback to old delve

						if (invItem.Item_Type == (int)eInventorySlot.Horse)
						{
							WriteHorseInfo(objectInfo, invItem, client, "");
						}

						if ((invItem.Item_Type == (int)eInventorySlot.HorseBarding || invItem.Item_Type == (int)eInventorySlot.HorseArmor) && invItem.Level > 0)
						{
							objectInfo.Add(" ");//empty line
							objectInfo.Add(" ");//empty line
							objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.ChampionLevel", invItem.Level));
						}
						if ((invItem.Object_Type >= (int)eObjectType.GenericWeapon) && (invItem.Object_Type <= (int)eObjectType.MaulerStaff) ||
						    invItem.Object_Type == (int)eObjectType.Instrument)
						{
							WriteUsableClasses(objectInfo, invItem, client);
							WriteMagicalBonuses(objectInfo, invItem, client, false);
							WriteClassicWeaponInfos(objectInfo, invItem, client);
						}

						if (invItem.Object_Type >= (int)eObjectType.Cloth && invItem.Object_Type <= (int)eObjectType.Scale)
						{
							WriteUsableClasses(objectInfo, invItem, client);
							WriteMagicalBonuses(objectInfo, invItem, client, false);
							WriteClassicArmorInfos(objectInfo, invItem, client);
						}

						if (invItem.Object_Type == (int)eObjectType.Shield)
						{
							WriteUsableClasses(objectInfo, invItem, client);
							WriteMagicalBonuses(objectInfo, invItem, client, false);
							WriteClassicShieldInfos(objectInfo, invItem, client);
						}

						if ((invItem.Item_Type != (int)eInventorySlot.Horse && invItem.Object_Type == (int)eObjectType.Magical) || invItem.Object_Type == (int)eObjectType.AlchemyTincture || invItem.Object_Type == (int)eObjectType.SpellcraftGem)
						{
							WriteMagicalBonuses(objectInfo, invItem, client, false);
						}

						if (invItem.Object_Type == (int)eObjectType.Poison)
						{
							WritePoisonInfo(objectInfo, invItem, client);
						}

						if (invItem.Object_Type == (int)eObjectType.Magical && invItem.Item_Type == 40) // potion
							WritePotionInfo(objectInfo, invItem, client);

						//Add admin info
						if (client.Account.PrivLevel > 1)
						{
							WriteTechnicalInfo(objectInfo, client, invItem);
						}

						break;

						#endregion Old Delve
					}
					#endregion
					#region Ability
				case 8://abilities
					{
						Ability abil = null;
						// are we in list ?
						if (indexAtSpecOid < snapSkills.Count)
						{
							abil = (Ability)snapSkills[indexAtSpecOid].Item1;
						}
						
						if (abil == null)
							return;
						
						caption = abil.Name;
						
						if (abil.DelveInfo.Count > 0)
							objectInfo.AddRange(abil.DelveInfo);
						else
							objectInfo.Add("There is no special information.");
						break;
					}
					#endregion
					#region Trainer
				case 9: //trainer window "info" button
					{
						Specialization spec = client.Player.GetSpecList().Where(e => e.Trainable).ElementAtOrDefault(objectID);
						
						if (spec != null)
							caption = spec.Name;
						
						if (client.TrainerSkillCache != null && objectID < client.TrainerSkillCache.Count)
						{
							objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.LevName"));
							foreach (Skill sk in client.TrainerSkillCache[objectID].Item2.Select(e => e.Item3))
							{
								if (sk is Ability)
									objectInfo.Add(string.Format("{0}: {1}", ((Ability)sk).SpecLevelRequirement, sk.Name));
								else if (sk is Style)
									objectInfo.Add(string.Format("{0}: {1}", ((Style)sk).SpecLevelRequirement, sk.Name));
								else
									objectInfo.Add(string.Format("{0}: {1}", sk.Level, sk.Name));
							}
						}
						else
						{
							//delve on realm abilities [by Suncheck]
							if (objectID >= 50)
							{
								int clientclassID = client.Player.CharacterClass.ID;
								int sub = 50;
								List<RealmAbility> ra_list = SkillBase.GetClassRealmAbilities(clientclassID);
								Ability ra5abil = SkillBase.GetClassRR5Ability(clientclassID);
								RealmAbility ab = ra_list[objectID - sub];
								if (ra5abil != null) //check if player have rr
								{
									if (client.Player.RealmPoints < 513500) //player have not rr5 abilty
										sub--;
								}
								for (int i = 0; i <= (objectID - sub); i++) //get all ra's at full level
								{
									RealmAbility raabil = ra_list[i];
									RealmAbility playerra = (RealmAbility)client.Player.GetAbility(raabil.KeyName);
									if (playerra != null)
										if (playerra.Level >= playerra.MaxLevel)
											sub--;
								}
								ab = ra_list[objectID - sub];
								if (ab != null)
								{
									caption = ab.Name;
									objectInfo.AddRange(ab.DelveInfo);
									break;
								}
							}
							caption = "Specialization not found";
							objectInfo.Add("that specialization is not found, id=" + objectID);
							break;
						}
						break;
					}
					#endregion
					#region Group
				case 12: // Item info to Group Chat
					{
						invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (invItem == null) return;
						string str = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.Item", client.Player.Name, GetShortItemInfo(invItem, client));
						if (client.Player.Group == null)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.NoGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						client.Player.Group.SendMessageToGroupMembers(str, eChatType.CT_Group, eChatLoc.CL_ChatWindow);
						return;
					}
					#endregion
					#region Guild
				case 13: // Item info to Guild Chat
					{
						invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (invItem == null) return;
						string str = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.GuildItem", client.Player.Name, GetShortItemInfo(invItem, client));
						if (client.Player.Guild == null)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.DontBelongGuild"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						if (!client.Player.Guild.HasRank(client.Player, Guild.eRank.GcSpeak))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.NoPermissionToSpeak"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						foreach (GamePlayer ply in client.Player.Guild.GetListOfOnlineMembers())
						{
							if (!client.Player.Guild.HasRank(ply, Guild.eRank.GcHear)) continue;
							ply.Out.SendMessage(str, eChatType.CT_Guild, eChatLoc.CL_ChatWindow);
						}
						return;
					}
					#endregion
					#region ChatGroup
				case 15: // Item info to Chat group
					{
						invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (invItem == null) return;

						ChatGroup mychatgroup = (ChatGroup)client.Player.TempProperties.getProperty<object>(ChatGroup.CHATGROUP_PROPERTY, null);
						if (mychatgroup == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.MustBeInChatGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						if (mychatgroup.Listen == true && (((bool)mychatgroup.Members[client.Player]) == false))
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.OnlyModerator"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						string str = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.ChatItem", client.Player.Name, GetShortItemInfo(invItem, client));
						foreach (GamePlayer ply in mychatgroup.Members.Keys)
						{
							ply.Out.SendMessage(str, eChatType.CT_Chat, eChatLoc.CL_ChatWindow);
						}
						return;
					}
					#endregion
					#region Trainer Window
					//styles
				case 20:
					{
						Style style = null;
						if (client.TrainerSkillCache != null)
						{
							// Search Id
							Skill sk = client.TrainerSkillCache.SelectMany(el => el.Item2).Where(e => e.Item2 == objectID && e.Item3.SkillType == eSkillPage.Styles).FirstOrDefault().Item3;
							if (sk is Style)
								style = (Style)sk;
						}
						
						if (style == null) return;

						caption = style.Name;
						WriteStyleInfo(objectInfo, style, client);
						break;
					}
					//spells
				case 22:
					//songs
				case 21:
					// Ability
				case 23:
					{
						Skill sk = null;
						if (client.TrainerSkillCache != null)
						{
							// Search Id
							sk = client.TrainerSkillCache.SelectMany(el => el.Item2).Where(e => ((objectType == 23 && e.Item2 == objectID && e.Item1 == 5) || (e.Item2 == objectID && e.Item1 != 5))).OrderBy(e => e.Item1).FirstOrDefault().Item3;
						}
						
						if (sk == null) return;
						
						if (sk is Spell)
						{
							Spell spell = (Spell)sk;
							caption = spell.Name;

							WriteSpellInfo(objectInfo, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells), client);
						}
						else if (sk is Ability)
						{
							Ability abil = (Ability)sk;
							caption = abil.Name;
						
							if (abil.DelveInfo.Count > 0)
								objectInfo.AddRange(abil.DelveInfo);
							else
								objectInfo.Add("There is no special information.");
						}
						
						break;
					}
					#endregion
					#region Repair
				case 100://repair
					{
						invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (invItem != null)
						{
							client.Player.RepairItem(invItem);
						}
						else
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryStrange"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						return;
					}
					#endregion
					#region Self Craft
				case 101://selfcraft
					{
						invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (invItem != null)
						{
							client.Player.OpenSelfCraft(invItem);
						}
						else
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryStrange"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						return;
					}
					#endregion
					#region Salvage
				case 102://salvage
					{
						invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (invItem != null)
						{
							client.Player.SalvageItem(invItem);
						}
						else
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.VeryStrange"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						return;
					}
					#endregion
					#region BattleGroup
				case 103: // Item info to battle group
					{
						invItem = client.Player.Inventory.GetItem((eInventorySlot)objectID);
						if (invItem == null) return;

						BattleGroup mybattlegroup = (BattleGroup)client.Player.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
						if (mybattlegroup == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.MustBeInBattleGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						if (mybattlegroup.Listen == true && (((bool)mybattlegroup.Members[client.Player]) == false))
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.OnlyModerator"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						string str = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.HandlePacket.ChatItem", client.Player.Name, GetShortItemInfo(invItem, client));
						foreach (GamePlayer ply in mybattlegroup.Members.Keys)
						{
							ply.Out.SendMessage(str, eChatType.CT_Chat, eChatLoc.CL_ChatWindow);
						}
						return;
					}
					#endregion
				#region v1.110+
				case 24://SpellsNew
					if (client.CanSendTooltip(24, objectID))
				        client.Out.SendDelveInfo(DelveSpell(client, objectID));
			        			        
					break;
				case 25://StylesNew
					if (client.CanSendTooltip(25, objectID))
	                    client.Out.SendDelveInfo(DelveStyle(client, objectID));
                    break;
				case 26://SongsNew
                    if (client.CanSendTooltip(26, objectID))
						client.Out.SendDelveInfo(DelveSong(client, objectID));
						client.Out.SendDelveInfo(DelveSpell(client, objectID));
					break;
				case 27://RANew
					if (client.CanSendTooltip(27, objectID))
	                   client.Out.SendDelveInfo(DelveRealmAbility(client, objectID));
                    break;
				case 28://AbilityNew
                    if (client.CanSendTooltip(28, objectID))
				        client.Out.SendDelveInfo(DelveAbility(client, objectID));
			        break;
				#endregion
				#region ChampionAbilities delve from trainer window
				default:
					{
						// Try retieving champion lines
						int clSpecID = objectType - 150;
						int clLineIndex = objectID >> 8;
						int clSkillIndex = objectID & 0x00FF;

						// Get Player CL Spec
						var clspec = client.Player.GetSpecList().Where(sp => sp is LiveChampionsSpecialization).Cast<LiveChampionsSpecialization>().FirstOrDefault();
						
						// check if the tree can be used
						List<Tuple<MiniLineSpecialization, List<Tuple<Skill, byte>>>> tree = null;
						if (clspec != null)
						{
							tree = clspec.GetTrainerTreeDisplay(client.Player, clspec.RetrieveTypeForIndex(clSpecID));
						}
						
						// Try and handle all Champion lines, including custom lines
						if (tree != null)
						{
							try
							{
								Skill skill = tree[clLineIndex].Item2[clSkillIndex].Item1;
								if (skill != null)
								{
									if (skill is Spell)
									{
										SpellLine spellLine = clspec.GetSpellLinesForLiving(client.Player).FirstOrDefault();
										if (spellLine != null)
										{
											caption = skill.Name;
											WriteSpellInfo(objectInfo, (Spell)skill, spellLine, client);
										}
										else
										{
											objectInfo.Add("Champion spell line not found!");
										}
									}
									else if (skill is Style)
									{
										caption = skill.Name;
										WriteStyleInfo(objectInfo, (Style)skill, client);
									}
								}
							}
							catch
							{
							}
						}

						break;
					}
					#endregion
			}

			if (objectInfo.Count > 0)
			{
				client.Out.SendCustomTextWindow(caption, objectInfo);
			}
			else if ((objectType < 24 || objectType > 28) && objectType < 150) // only warn for non v1.110+ objects
			{
				log.WarnFormat("DetailDisplayHandler no info for objectID {0} of type {1}. Item: {2}, client: {3}", objectID, objectType, (item == null ? (invItem == null ? "null" : invItem.Id_nb) : item.Id_nb), client);
			}
		}

		public static void WriteStyleInfo(IList<string> objectInfo, Style style, GameClient client)
		{
			client.Player.DelveWeaponStyle(objectInfo, style);
		}

		/// <summary>
		/// Write a formatted description of a spell
		/// </summary>
		/// <param name="output"></param>
		/// <param name="spell"></param>
		/// <param name="spellLine"></param>
		/// <param name="client"></param>
		public void WriteSpellInfo(IList<string> output, Spell spell, SpellLine spellLine, GameClient client)
		{
			if (client == null || client.Player == null)
				return;

			// check to see if player class handles delve
			if (client.Player.DelveSpell(output, spell, spellLine))
				return;

			ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, spellLine);
			if (spellHandler == null)
			{
				output.Add(" ");
				output.Add("Spell type (" + spell.SpellType + ") is not implemented.");
			}
			else
			{
				output.AddRange(spellHandler.DelveInfo);
				//Subspells
				if (spell.SubSpellID > 0)
				{
					Spell s = SkillBase.GetSpellByID(spell.SubSpellID);
					output.Add(" ");

					ISpellHandler sh = ScriptMgr.CreateSpellHandler(client.Player, s, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
					output.AddRange(sh.DelveInfo);
				}
			}
			if (client.Account.PrivLevel > 1)
			{
				output.Add(" ");
				output.Add("--- Spell Technical Information ---");
				output.Add(" ");
				output.Add("Line: " + (spellHandler == null ? spellLine.KeyName : spellHandler.SpellLine.Name));
				output.Add("Type: " + spell.SpellType);
				output.Add(" ");
				output.Add("SpellID: " + spell.ID);
				output.Add("Icon: " + spell.Icon);
				output.Add("Type: " + spell.SpellType);
				output.Add("ClientEffect: " + spell.ClientEffect);
				output.Add("Target: " + spell.Target);
				output.Add("MoveCast: " + spell.MoveCast);
				output.Add("Uninterruptible: " + spell.Uninterruptible);
				output.Add("Value: " + spell.Value);
				output.Add("LifeDrainReturn: " + spell.LifeDrainReturn);
				if (spellHandler != null)
					output.Add("HasPositiveEffect: " + spellHandler.HasPositiveEffect);
				output.Add("SharedTimerGroup: " + spell.SharedTimerGroup);
				output.Add("EffectGroup: " + spell.EffectGroup);
				output.Add("SpellGroup (for hybrid grouping): " + spell.Group);
				output.Add("Spell AllowCoexisting: " + spell.AllowCoexisting);
			}
		}

        public void WriteTechnicalInfo(IList<string> output, GameClient client, InventoryItem item)
		{
            WriteTechnicalInfo(output, client, item, item.Durability, item.Condition);
		}

		public void WriteTechnicalInfo(IList<string> output, GameClient client, InventoryItem item, int dur, int con)
		{
			output.Add(" ");
			output.Add("--- Item Technical Information ---");
			output.Add(" ");
			output.Add("Item Template: " + item.Id_nb);
			output.Add("         Name: " + item.Name);
			output.Add("        Level: " + item.Level);
			output.Add("       Object: " + GlobalConstants.ObjectTypeToName(item.Object_Type) + " (" + item.Object_Type + ")");
			output.Add("         Type: " + GlobalConstants.SlotToName(item.Item_Type) + " (" + item.Item_Type + ")");
			output.Add("    Extension: " + item.Extension);
			output.Add("        Model: " + item.Model);
			output.Add("        Color: " + item.Color);
			output.Add("       Emblem: " + item.Emblem);
			output.Add("       Effect: " + item.Effect);
			output.Add("  Value/Price: " + Money.GetShortString(item.Price));
			output.Add("       Weight: " + (item.Weight / 10.0f) + "lbs");
			output.Add("      Quality: " + item.Quality + "%");
			output.Add("   Durability: " + dur + "/" + item.MaxDurability);
			output.Add("    Condition: " + con + "/" + item.MaxCondition);
			output.Add("        Realm: " + item.Realm);
			output.Add("  Is dropable: " + (item.IsDropable ? "yes" : "no"));
			output.Add("  Is pickable: " + (item.IsPickable ? "yes" : "no"));
			output.Add("  Is tradable: " + (item.IsTradable ? "yes" : "no"));
			output.Add(" Is alwaysDUR: " + (item.IsNotLosingDur ? "yes" : "no"));
			output.Add("Is Indestruct: " + (item.IsIndestructible ? "yes" : "no"));
			output.Add(" Is stackable: " + (item.IsStackable ? "yes" : "no"));
			output.Add("  ProcSpellID: " + item.ProcSpellID);
			output.Add(" ProcSpellID1: " + item.ProcSpellID1);
			output.Add("      SpellID: " + item.SpellID + " (" + item.Charges + "/" + item.MaxCharges + ")");
			output.Add("     SpellID1: " + item.SpellID1 + " (" + item.Charges1 + "/" + item.MaxCharges1 + ")");
			output.Add("PoisonSpellID: " + item.PoisonSpellID + " (" + item.PoisonCharges + "/" + item.PoisonMaxCharges + ") ");

			if (GlobalConstants.IsWeapon(item.Object_Type))
			{
				output.Add("         Hand: " + GlobalConstants.ItemHandToName(item.Hand) + " (" + item.Hand + ")");
				output.Add("Damage/Second: " + (item.DPS_AF / 10.0f));
				output.Add("        Speed: " + (item.SPD_ABS / 10.0f));
				output.Add("  Damage type: " + GlobalConstants.WeaponDamageTypeToName(item.Type_Damage) + " (" + item.Type_Damage + ")");
				output.Add("        Bonus: " + item.Bonus);
			}
			else if (GlobalConstants.IsArmor(item.Object_Type))
			{
				output.Add("  Armorfactor: " + item.DPS_AF);
				output.Add("   Absorption: " + item.SPD_ABS);
				output.Add("        Bonus: " + item.Bonus);
			}
			else if (item.Object_Type == (int)eObjectType.Shield)
			{
				output.Add("Damage/Second: " + (item.DPS_AF / 10.0f));
				output.Add("        Speed: " + (item.SPD_ABS / 10.0f));
				output.Add("  Shield type: " + GlobalConstants.ShieldTypeToName(item.Type_Damage) + " (" + item.Type_Damage + ")");
				output.Add("        Bonus: " + item.Bonus);
			}
			else if (item.Object_Type == (int)eObjectType.Arrow || item.Object_Type == (int)eObjectType.Bolt)
			{
				output.Add(" Ammunition #: " + item.DPS_AF);
				output.Add("       Damage: " + GlobalConstants.AmmunitionTypeToDamageName(item.SPD_ABS));
				output.Add("        Range: " + GlobalConstants.AmmunitionTypeToRangeName(item.SPD_ABS));
				output.Add("     Accuracy: " + GlobalConstants.AmmunitionTypeToAccuracyName(item.SPD_ABS));
				output.Add("        Bonus: " + item.Bonus);
			}
			else if (item.Object_Type == (int)eObjectType.Instrument)
			{
				output.Add("   Instrument: " + GlobalConstants.InstrumentTypeToName(item.DPS_AF));
			}

			output.Add(" ");
			output.Add("            Flags: " + item.Flags);
			output.Add("        PackageID: " + item.PackageID);
		}

		protected string GetShortItemInfo(InventoryItem item, GameClient client)
		{
			// TODO: correct info format if anyone is interested...
			/*
						[Guild] Player/Item:  "- [Kerubis' Scythe]: scythe No Sell. No Destroy."
						[Guild] Player/Item: "- [Adroit Runed Duelist Rapier]: rapier 14.1 DPS 3.6 speed 89% qual 100% con (Thrust). Bonuses:  3% Spirit, 19 Dexterity, 2 Thrust, 6% Thrust, Tradeable.".
						[Party] Player/Item: "- [Matterbender Belt]: belt 4% Energy, 8% Matter, 9 Constitution, 4% Slash, Tradeable."

						multiline item...
						[Guild] Player/Item: "- [shimmering Archmage Focus Briny Staff]: staff for Theurgist, Wizard, Sorcerer, Necromancer, Cabalist, Spiritmaster, Runemaster, Bonedancer, Theurgist, Wizard,
						Sorcerer, Necromancer, Cabalist, Spiritmaster, Runemaster, Bonedancer, 16.5".
						[Guild] Player/Item: "- DPS 3.0 speed 94% qual 100% con (Crush) (Two Handed). Bonuses:
						22 Hits, 5% Energy, 2 All Casting, 50 lvls ALL focus 7% buff bonus, (10/10 char
						ges) health regen Value: 8  Tradeable.".
			 */

			string str = "- [" + item.Name + "]: " + GlobalConstants.ObjectTypeToName(item.Object_Type);
			var objectInfo = new List<string>();

			if ((item.Object_Type >= (int)eObjectType.GenericWeapon) && (item.Object_Type <= (int)eObjectType.MaulerStaff))
			{
				WriteMagicalBonuses(objectInfo, item, client, true);
				WriteClassicWeaponInfos(objectInfo, item, client);
			}
			if (item.Object_Type >= (int)eObjectType.Cloth && item.Object_Type <= (int)eObjectType.Scale)
			{
				WriteMagicalBonuses(objectInfo, item, client, true);
				WriteClassicArmorInfos(objectInfo, item, client);
			}
			if (item.Object_Type == (int)eObjectType.Shield)
			{
				WriteMagicalBonuses(objectInfo, item, client, true);
				WriteClassicShieldInfos(objectInfo, item, client);
			}
			if (item.Object_Type == (int)eObjectType.Magical ||
			    item.Object_Type == (int)eObjectType.Instrument)
			{
				WriteMagicalBonuses(objectInfo, item, client, true);
			}
			if (item.IsCrafted)
			{
				objectInfo.Add(" ");//empty line
				objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.GetShortItemInfo.CrafterName", item.Creator));
			}
			if (item.Object_Type == (int)eObjectType.Poison)
			{
				WritePoisonInfo(objectInfo, item, client);
			}

			if (item.Object_Type == (int)eObjectType.Magical && item.Item_Type == 40) // potion
				WritePotionInfo(objectInfo, item, client);

			if (!item.IsDropable)
			{
				objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.GetShortItemInfo.NoDrop"));
				objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.GetShortItemInfo.NoSell"));
			}
			if (!item.IsPickable)
				objectInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.GetShortItemInfo.NoTrade"));

			foreach (string s in objectInfo)
				str += " " + s;
			return str;
		}

		/// <summary>
		/// Damage Modifiers:
		/// - X.X Base DPS
		/// - X.X Clamped DPS
		/// - XX Weapon Speed
		/// - XX% Quality
		/// - XX% Condition
		/// Damage Type: XXX
		///
		/// Effective Damage:
		/// - X.X DPS
		/// </summary>
		public void WriteClassicWeaponInfos(IList<string> output, InventoryItem item, GameClient client)
		{
			double itemDPS = item.DPS_AF / 10.0;
			double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * client.Player.Level);
			double itemSPD = item.SPD_ABS / 10.0;
			double effectiveDPS = clampedDPS * item.Quality / 100.0 * item.Condition / item.Template.MaxCondition;

			output.Add(" ");
			output.Add(" ");
			output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.DamageMod"));
			if (itemDPS != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.BaseDPS", itemDPS.ToString("0.0")));
				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.ClampDPS", clampedDPS.ToString("0.0")));
			}

			if (item.SPD_ABS >= 0)
			{
				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.SPD", itemSPD.ToString("0.0")));
			}

			if (item.Quality != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.Quality", item.Quality));
			}
			if (item.Condition != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.Condition", item.ConditionPercent));
			}

			output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.DamageType",
			                                      (item.Type_Damage == 0 ? "None" : GlobalConstants.WeaponDamageTypeToName(item.Type_Damage))));
			output.Add(" ");

			output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicWeaponInfos.EffDamage"));
			if (itemDPS != 0)
			{
				output.Add("- " + effectiveDPS.ToString("0.0") + " DPS");
			}
		}

		public void WriteUsableClasses(IList<string> output, InventoryItem item, GameClient client)
		{
			WriteUsableClasses(output, item.Template, client);
		}
		public void WriteUsableClasses(IList<string> output, ItemTemplate item, GameClient client)
		{
			if (Util.IsEmpty(item.AllowedClasses, true))
				return;

			output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteUsableClasses.UsableBy"));

			foreach (string allowed in item.AllowedClasses.SplitCSV(true))
			{
				int classID = -1;
				if (int.TryParse(allowed, out classID))
					output.Add("- " + ((eCharacterClass)classID).ToString());
				else log.Error(item.Id_nb + " has an invalid entry for allowed classes '" + allowed + "'");
			}
		}

		/// <summary>
		/// Damage Modifiers (when used with shield styles):
		/// - X.X Base DPS
		/// - X.X Clamped DPS
		/// - XX Shield Speed
		/// </summary>
		public void WriteClassicShieldInfos(IList<string> output, InventoryItem item, GameClient client)
		{
			WriteClassicShieldInfos(output, item.Template, client);
		}
		public void WriteClassicShieldInfos(IList<string> output, ItemTemplate item, GameClient client)
		{
			double itemDPS = item.DPS_AF / 10.0;
			double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * client.Player.Level);
			double itemSPD = item.SPD_ABS / 10.0;

			output.Add(" ");
			output.Add(" ");
			output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.DamageMod"));
			if (itemDPS != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.BaseDPS", itemDPS.ToString("0.0")));
				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.ClampDPS", clampedDPS.ToString("0.0")));
			}
			if (item.SPD_ABS >= 0)
			{
				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.SPD", itemSPD.ToString("0.0")));
			}

			output.Add(" ");

			switch (item.Type_Damage)
			{
					case 1: output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.Small")); break;
					case 2: output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.Medium")); break;
					case 3: output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicShieldInfos.Large")); break;
			}
		}

		/// <summary>
		/// Armor Modifiers:
		/// - X.X Base Factor
		/// - X.X Clamped Factor
		/// - XX% Absorption
		/// - XX% Quality
		/// - XX% Condition
		/// Damage Type: XXX
		///
		/// Effective Armor:
		/// - X.X Factor
		/// </summary>
		public void WriteClassicArmorInfos(IList<string> output, InventoryItem item, GameClient client)
		{
			output.Add(" ");
			output.Add(" ");
			output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.ArmorMod"));
			if (item.DPS_AF != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.BaseFactor", item.DPS_AF));
			}
			double AF = 0;
			if (item.DPS_AF != 0)
			{
				int afCap = client.Player.Level;
				if (item.Object_Type != (int)eObjectType.Cloth)
				{
					afCap *= 2;
				}

				AF = Math.Min(afCap, item.DPS_AF);

				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.ClampFact", (int)AF));
			}
			if (item.SPD_ABS >= 0)
			{
				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.Absorption", item.SPD_ABS));
			}
			if (item.Quality != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.Quality", item.Quality));
			}
			if (item.Condition != 0)
			{
				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.Condition", 100 /*item.ConditionPercent*/));
			}
			output.Add(" ");

			output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.EffArmor"));
			double EAF = 0;
			if (item.DPS_AF != 0)
			{
				EAF = AF * item.Quality / 100.0 * item.Condition / item.MaxCondition * (1 + item.SPD_ABS / 100.0);
				output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteClassicArmorInfos.Factor", (int)EAF));
			}

		}

		public void WriteMagicalBonuses(IList<string> output, ItemTemplate item, GameClient client, bool shortInfo)
		{
			WriteMagicalBonuses(output, GameInventoryItem.Create(item), client, shortInfo);
		}

		public void WriteMagicalBonuses(IList<string> output, InventoryItem item, GameClient client, bool shortInfo)
		{
			int oldCount = output.Count;

			WriteBonusLine(output, client, item.Bonus1Type, item.Bonus1);
            WriteBonusLine(output, client, item.Bonus2Type, item.Bonus2);
            WriteBonusLine(output, client, item.Bonus3Type, item.Bonus3);
            WriteBonusLine(output, client, item.Bonus4Type, item.Bonus4);
            WriteBonusLine(output, client, item.Bonus5Type, item.Bonus5);
            WriteBonusLine(output, client, item.Bonus6Type, item.Bonus6);
            WriteBonusLine(output, client, item.Bonus7Type, item.Bonus7);
            WriteBonusLine(output, client, item.Bonus8Type, item.Bonus8);
            WriteBonusLine(output, client, item.Bonus9Type, item.Bonus9);
            WriteBonusLine(output, client, item.Bonus10Type, item.Bonus10);
            WriteBonusLine(output, client, item.ExtraBonusType, item.ExtraBonus);

			if (output.Count > oldCount)
			{
				output.Add(" ");
				output.Insert(oldCount, LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MagicBonus"));
				output.Insert(oldCount, " ");
			}

			oldCount = output.Count;

			WriteFocusLine(output, item.Bonus1Type, item.Bonus1);
			WriteFocusLine(output, item.Bonus2Type, item.Bonus2);
			WriteFocusLine(output, item.Bonus3Type, item.Bonus3);
			WriteFocusLine(output, item.Bonus4Type, item.Bonus4);
			WriteFocusLine(output, item.Bonus5Type, item.Bonus5);
			WriteFocusLine(output, item.Bonus6Type, item.Bonus6);
			WriteFocusLine(output, item.Bonus7Type, item.Bonus7);
			WriteFocusLine(output, item.Bonus8Type, item.Bonus8);
			WriteFocusLine(output, item.Bonus9Type, item.Bonus9);
			WriteFocusLine(output, item.Bonus10Type, item.Bonus10);
			WriteFocusLine(output, item.ExtraBonusType, item.ExtraBonus);

			if (output.Count > oldCount)
			{
				output.Add(" ");
				output.Insert(oldCount, LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.FocusBonus"));
				output.Insert(oldCount, " ");
			}

			if (!shortInfo)
			{
				if (item.ProcSpellID != 0 || item.ProcSpellID1 != 0 || item.SpellID != 0 || item.SpellID1 != 0)
				{
					int requiredLevel = item.LevelRequirement > 0 ? item.LevelRequirement : Math.Min(50, item.Level);
					output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired2", requiredLevel));
					output.Add(" ");
				}

				if (item.Object_Type == (int)eObjectType.Magical && item.Item_Type == (int)eInventorySlot.FirstBackpack) // potion
				{
					// let WritePotion handle the rest of the display
					return;
				}


				#region Proc1
				if (item.ProcSpellID != 0)
				{
					string spellNote = "";
					output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
					if (GlobalConstants.IsWeapon(item.Object_Type))
					{
						spellNote = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.StrikeEnemy");
					}
					else if (GlobalConstants.IsArmor(item.Object_Type))
					{
						spellNote = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.StrikeArmor");
					}

					SpellLine line = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (line != null)
					{
						Spell procSpell = SkillBase.FindSpell(item.ProcSpellID, line);

						if (procSpell != null)
						{
							ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, procSpell, line);
							if (spellHandler != null)
							{
								output.AddRange(spellHandler.DelveInfo);
								output.Add(" ");
							}
							else
							{
								output.Add("-" + procSpell.Name + " (Spell Handler Not Implemented)");
							}

							output.Add(spellNote);
						}
						else
						{
							output.Add("- Spell Not Found: " + item.ProcSpellID);
						}
					}
					else
					{
						output.Add("- Item_Effects Spell Line Missing");
					}

					output.Add(" ");
				}
				#endregion
				#region Proc2
				if (item.ProcSpellID1 != 0)
				{
					string spellNote = "";
					output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
					if (GlobalConstants.IsWeapon(item.Object_Type))
					{
						spellNote = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.StrikeEnemy");
					}
					else if (GlobalConstants.IsArmor(item.Object_Type))
					{
						spellNote = LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.StrikeArmor");
					}

					SpellLine line = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (line != null)
					{
						Spell procSpell = SkillBase.FindSpell(item.ProcSpellID1, line);

						if (procSpell != null)
						{
							ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, procSpell, line);
							if (spellHandler != null)
							{
								output.AddRange(spellHandler.DelveInfo);
								output.Add(" ");
							}
							else
							{
								output.Add("-" + procSpell.Name + " (Spell Handler Not Implemented)");
							}

							output.Add(spellNote);
						}
						else
						{
							output.Add("- Spell Not Found: " + item.ProcSpellID1);
						}
					}
					else
					{
						output.Add("- Item_Effects Spell Line Missing");
					}

					output.Add(" ");
				}
				#endregion
				#region Charge1
				if (item.SpellID != 0)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						Spell spell = SkillBase.FindSpell(item.SpellID, chargeEffectsLine);
						if (spell != null)
						{
							ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, chargeEffectsLine);

							if (spellHandler != null)
							{
								if (item.MaxCharges > 0)
								{
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.Charges", item.Charges));
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", item.MaxCharges));
									output.Add(" ");
								}

								output.AddRange(spellHandler.DelveInfo);
								output.Add(" ");
								output.Add("- This spell is cast when the item is used.");
							}
							else
							{
								output.Add("- Item_Effects Spell Line Missing");
							}
						}
						else
						{
							output.Add("- Spell Not Found: " + item.SpellID);
						}
					}

					output.Add(" ");
				}
				#endregion
				#region Charge2
				if (item.SpellID1 != 0)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						Spell spell = SkillBase.FindSpell(item.SpellID1, chargeEffectsLine);
						if (spell != null)
						{
							ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, chargeEffectsLine);

							if (spellHandler != null)
							{
								if (item.MaxCharges > 0)
								{
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.Charges", item.Charges));
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", item.MaxCharges));
									output.Add(" ");
								}

								output.AddRange(spellHandler.DelveInfo);
								output.Add(" ");
								output.Add("- This spell is cast when the item is used.");
							}
							else
							{
								output.Add("- Item_Effects Spell Line Missing");
							}
						}
						else
						{
							output.Add("- Spell Not Found: " + item.SpellID1);
						}
					}

					output.Add(" ");
				}
				#endregion
				#region Poison
				if (item.PoisonSpellID != 0)
				{
					if (GlobalConstants.IsWeapon(item.Object_Type))// Poisoned Weapon
					{
						SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
						if (poisonLine != null)
						{
							List<Spell> spells = SkillBase.GetSpellList(poisonLine.KeyName);
							foreach (Spell spl in spells)
							{
								if (spl.ID == item.PoisonSpellID)
								{
									output.Add(" ");
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired"));
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.Level", spl.Level));
									output.Add(" ");
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.Charges", item.PoisonCharges));
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", item.PoisonMaxCharges));
									output.Add(" ");

									ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, poisonLine);
									if (spellHandler != null)
									{
										output.AddRange(spellHandler.DelveInfo);
										output.Add(" ");
									}
									else
									{
										output.Add("-" + spl.Name + "(Not implemented yet)");
									}
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.StrikeEnemy"));
									return;
								}
							}
						}
					}

					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						List<Spell> spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						foreach (Spell spl in spells)
						{
							if (spl.ID == item.SpellID)
							{
								output.Add(" ");
								output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.LevelRequired"));
								output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.Level", spl.Level));
								output.Add(" ");
								if (item.MaxCharges > 0)
								{
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.ChargedMagic"));
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.Charges", item.Charges));
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MaxCharges", item.MaxCharges));
								}
								else
								{
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
								}
								output.Add(" ");

								ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, chargeEffectsLine);
								if (spellHandler != null)
								{
									output.AddRange(spellHandler.DelveInfo);
									output.Add(" ");
								}
								else
								{
									output.Add("-" + spl.Name + "(Not implemented yet)");
								}
								output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.UsedItem"));
								output.Add(" ");
								if (spl.RecastDelay > 0)
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.UseItem1", Util.FormatTime(spl.RecastDelay / 1000)));
								else
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.UseItem2"));
								long lastChargedItemUseTick = client.Player.TempProperties.getProperty<long>(GamePlayer.LAST_CHARGED_ITEM_USE_TICK);
								long changeTime = client.Player.CurrentRegion.Time - lastChargedItemUseTick;
								long recastDelay = (spl.RecastDelay > 0) ? spl.RecastDelay : 60000 * 3;
								if (changeTime < recastDelay) //3 minutes reuse timer
									output.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.UseItem3", Util.FormatTime((recastDelay - changeTime) / 1000)));
								return;
							}
						}
					}
				}
				#endregion
			}
		}

        protected void WriteBonusLine(IList<string> list, GameClient client, int bonusCat, int bonusValue)
		{
			if (bonusCat != 0 && bonusValue != 0 && !SkillBase.CheckPropertyType((eProperty)bonusCat, ePropertyType.Focus))
			{
				if (IsPvEBonus((eProperty)bonusCat))
				{
					// Evade: {0}% (PvE Only)
					list.Add(string.Format(SkillBase.GetPropertyName((eProperty)bonusCat), bonusValue));
				}
				else
				{
					//- Axe: 5 pts
					//- Strength: 15 pts
					//- Constitution: 15 pts
					//- Hits: 40 pts
					//- Fatigue: 8 pts
					//- Heat: 7%
					//Bonus to casting speed: 2%
					//Bonus to armor factor (AF): 18
					//Power: 6 % of power pool.
					list.Add(string.Format(
						"- {0}: {1}{2}",
						SkillBase.GetPropertyName((eProperty)bonusCat),
						bonusValue.ToString("+0 ;-0 ;0 "), //Eden
						((bonusCat == (int)eProperty.PowerPool)
						 || (bonusCat >= (int)eProperty.Resist_First && bonusCat <= (int)eProperty.Resist_Last)
						 || (bonusCat >= (int)eProperty.ResCapBonus_First && bonusCat <= (int)eProperty.ResCapBonus_Last)
						 || bonusCat == (int)eProperty.Conversion
						 || bonusCat == (int)eProperty.ExtraHP
						 || bonusCat == (int)eProperty.RealmPoints
						 || bonusCat == (int)eProperty.StyleAbsorb
						 || bonusCat == (int)eProperty.ArcaneSyphon
						 || bonusCat == (int)eProperty.BountyPoints
						 || bonusCat == (int)eProperty.XpPoints)
                        ? ((bonusCat == (int)eProperty.PowerPool) ? LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteBonusLine.PowerPool") : "%")
						: LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteBonusLine.Points")
					));
				}
			}
		}

		protected bool IsPvEBonus(eProperty property)
		{
			switch (property)
			{
					//case eProperty.BlockChance:
					//case eProperty.ParryChance:
					//case eProperty.EvadeChance:
				case eProperty.DefensiveBonus:
				case eProperty.BladeturnReinforcement:
				case eProperty.NegativeReduction:
				case eProperty.PieceAblative:
				case eProperty.ReactionaryStyleDamage:
				case eProperty.SpellPowerCost:
				case eProperty.StyleCostReduction:
				case eProperty.ToHitBonus:
					return true;

				default:
					return false;
			}
		}

		protected void WriteFocusLine(IList<string> list, int focusCat, int focusLevel)
		{
			if (SkillBase.CheckPropertyType((eProperty)focusCat, ePropertyType.Focus))
			{
				//- Body Magic: 4 lvls
				list.Add(string.Format("- {0}: {1} lvls", SkillBase.GetPropertyName((eProperty)focusCat), focusLevel));
			}
		}

		protected void WriteHorseInfo(IList<string> list, InventoryItem item, GameClient client, string horseName)
		{
			WriteHorseInfo(list, item.Template, client, horseName);
		}
		
		protected void WriteHorseInfo(IList<string> list, ItemTemplate item, GameClient client, string horseName)
		{
			list.Add(" ");
			list.Add(" ");
			if (item.Level <= 35)
			{
				list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.BasicHorse"));
				list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Speed1"));
				list.Add(" ");
				list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.MountWindow"));
				list.Add(" ");
				list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Quickbar"));
				list.Add(" ");
				list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.RvRZzones"));
			}
			else
			{
				list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.AdvancedHorse"));
				list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Speed2"));
				list.Add(" ");
				list.Add(" ");
				list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Summon"));
				list.Add(" ");
				list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Name", ((horseName == null || horseName == "") ? "None" : horseName)));
				list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.NameMount"));
			}
			list.Add(" ");
			list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Armor", (item.DPS_AF == 0 ? "None" : item.DPS_AF.ToString())));
			list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Barding"));
			list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteHorseInfo.Food"));
		}

		protected void WritePoisonInfo(IList<string> list, ItemTemplate item, GameClient client)
		{
			WritePoisonInfo(list, GameInventoryItem.Create(item), client);
		}

		protected void WritePoisonInfo(IList<string> list, InventoryItem item, GameClient client)
		{
			if (item.PoisonSpellID != 0)
			{
				SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
				if (poisonLine != null)
				{
					List<Spell> spells = SkillBase.GetSpellList(poisonLine.KeyName);

					foreach (Spell spl in spells)
					{
						if (spl.ID == item.PoisonSpellID)
						{
							list.Add(" ");
							list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePoisonInfo.LevelRequired"));
							list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePoisonInfo.Level", spl.Level));
							list.Add(" ");
							list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePoisonInfo.ProcAbility"));
							list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePoisonInfo.Charges", item.PoisonCharges));
							list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePoisonInfo.MaxCharges", item.PoisonMaxCharges));
							list.Add(" ");

							ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spl, poisonLine);
							if (spellHandler != null)
							{
								list.AddRange(spellHandler.DelveInfo);
							}
							else
							{
								list.Add("-" + spl.Name + " (Not implemented yet)");
							}
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Nidel: Write potions infos. Spell's infos include
		/// </summary>
		/// <param name="list"></param>
		/// <param name="item"></param>
		/// <param name="client"></param>
		private static void WritePotionInfo(IList<string> list, ItemTemplate item, GameClient client)
		{
			WritePotionInfo(list, GameInventoryItem.Create(item), client);
		}

		private static void WritePotionInfo(IList<string> list, InventoryItem item, GameClient client)
		{
			if (item.SpellID != 0)
			{
				SpellLine potionLine = SkillBase.GetSpellLine(GlobalSpellsLines.Potions_Effects);
				if (potionLine != null)
				{
					List<Spell> spells = SkillBase.GetSpellList(potionLine.KeyName);

					foreach (Spell spl in spells)
					{
						if (spl.ID == item.SpellID)
						{
							list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.ChargedMagic"));
							list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.Charges", item.Charges));
							list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.MaxCharges", item.MaxCharges));
							list.Add(" ");
							WritePotionSpellsInfos(list, client, spl, potionLine);
							list.Add(" ");
							long nextPotionAvailTime = client.Player.TempProperties.getProperty<long>("LastPotionItemUsedTick_Type" + spl.SharedTimerGroup);
							// Satyr Update: Individual Reuse-Timers for Pots need a Time looking forward
							// into Future, set with value of "itemtemplate.CanUseEvery" and no longer back into past
							if (nextPotionAvailTime > client.Player.CurrentRegion.Time)
							{
								list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.UseItem3", Util.FormatTime((nextPotionAvailTime - client.Player.CurrentRegion.Time) / 1000)));
							}
							else
							{
								int minutes = item.CanUseEvery / 60;
								int seconds = item.CanUseEvery % 60;

								if (minutes == 0)
								{
									list.Add(String.Format("Can use item every: {0} sec", seconds));
								}
								else
								{
									list.Add(String.Format("Can use item every: {0}:{1:00} min", minutes, seconds));
								}
							}

							if (spl.CastTime > 0)
							{
								list.Add(" ");
								list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.NoUseInCombat"));
							}
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Nidel: Write spell's infos of potions and subspell's infos with recursive method.
		/// </summary>
		/// <param name="list"></param>
		/// <param name="client"></param>
		/// <param name="spl"></param>
		/// <param name="line"></param>
		private static void WritePotionSpellsInfos(IList<string> list, GameClient client, Spell spl, NamedSkill line)
		{
			if (spl != null)
			{
				list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WriteMagicalBonuses.MagicAbility"));
				list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.Type", spl.SpellType));
				list.Add(" ");
				list.Add(spl.Description);
				list.Add(" ");
				if (spl.Value != 0)
				{
					list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.Value", spl.Value));
				}
				list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.Target", spl.Target));
				if (spl.Range > 0)
				{
					list.Add(LanguageMgr.GetTranslation(client.Account.Language, "DetailDisplayHandler.WritePotionInfo.Range", spl.Range));
				}
				list.Add(" ");
				list.Add(" ");
				if (spl.SubSpellID > 0)
				{
					List<Spell> spells = SkillBase.GetSpellList(line.KeyName);
					foreach (Spell subSpell in spells)
					{
						if (subSpell.ID == spl.SubSpellID)
						{
							WritePotionSpellsInfos(list, client, subSpell, line);
							break;
						}
					}
				}
			}
		}

		 #region v1.110+
        /** General info @ v1.110:
         *  - Examples can be found at http://dl.dropbox.com/u/48908369/delve.txt
         *  - 'Expires' can be left out
         *  - No idea what 'Fingerprint' does
         **/

        public static string DelveAbility(GameClient clt, int id)
        { /* or skill */

        	Skill sk = clt.Player.GetAllUsableSkills().Where(e => e.Item1.InternalID == id).OrderBy(e => e.Item1 is Ability ? 0 : 1).Select(e => e.Item1).FirstOrDefault();
        	
        	if(sk == null)
        		sk = SkillBase.GetAbilityByInternalID(id);
        	
        	if(sk == null)
        		sk = SkillBase.GetSpecializationByInternalID(id);
        	
        	MiniDelveWriter dw = new MiniDelveWriter(sk is Ability ? "Ability" : "Skill");

        	dw.AddKeyValuePair("Index", unchecked((short)id));

            if (sk != null) 
            {
                dw.AddKeyValuePair("Name", sk.Name);
            }
            else
            {
            	dw.AddKeyValuePair("Name", "(not found)");
            }
            
            return dw.ToString();
        }


		/// <summary>
		/// Delve Info for Songs (V1.110+)
		/// </summary>
		/// <param name="clt">Client</param>
		/// <param name="id">SpellID</param>
		/// <returns></returns>
		public static string DelveSong(GameClient clt, int id)
		{
			MiniDelveWriter dw = new MiniDelveWriter("Song");
			dw.AddKeyValuePair("Index", unchecked((short)id));
			
			Spell spell = SkillBase.GetSpellByTooltipID((ushort)id);
		
			ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(clt.Player, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
			
			if (spellHandler != null)
			{
				dw.AddKeyValuePair("effect", spellHandler.Spell.InternalID);
				dw.AddKeyValuePair("Name", spellHandler.Spell.Name);
				return dw.ToString();
			}

			// not found
			dw.AddKeyValuePair("Name", "(not found)");
			return dw.ToString();
		}

		/// <summary>
		/// Delve Info for Spells (V1.110+)
		/// </summary>
		/// <param name="clt">Client</param>
		/// <param name="id">SpellID</param>
		/// <returns></returns>
        public static string DelveSpell(GameClient clt, int id)
        {
            MiniDelveWriter dw = new MiniDelveWriter("Spell");
			
            Spell spell = SkillBase.GetSpellByTooltipID((ushort)id);
        	// Spell object are mostly "DB" Object, we can't subclass this object easily, but Spellhandler create subclass using "SpellType"
        	// We better rely on the handler to delve it correctly ! using reserved spellline as we can't guess it ! player can delve other object effect !
			ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(clt.Player, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
			
			if (spellHandler != null)
			{
				spellHandler.TooltipDelve(ref dw, id);
				return dw.ToString();
			}

        	// not found
        	dw.AddKeyValuePair("Index", unchecked((short)id));
        	dw.AddKeyValuePair("Name", "(not found)");
            return dw.ToString();
        }

		public static string DelveStyle(GameClient clt, int id)
        {
			Tuple<Skill,Skill> sk = clt.Player.GetAllUsableSkills().Where(e => e.Item1.InternalID == id && e.Item1 is Style).FirstOrDefault();
        	
			Style style = null;
        	if(sk == null || sk.Item1 == null)
        	{
            	style = SkillBase.GetStyleByInternalID(id);
        	}
        	else if (sk.Item1 is Style)
        	{
        		style = (Style)sk.Item1;
        	}

            MiniDelveWriter dw = new MiniDelveWriter("Style");
            dw.AddKeyValuePair("Index",  unchecked((short)id));

            if (style != null)
            {
                // Not implemented:
                // (Style (FollowupStyle "Sapphire Slash")(LevelBonus "2")(OpeningDamage "16")(Skill "1")(Expires "1343375647"))
                // (Style (Fingerprint "1746652963")(FollowupStyle "Thigh Cut")(Hidden "1")OpeningDamage "55")(Skill "118")(SpecialNumber "1511")(SpecialType "1")(Expires "1342381240"))
				// Skill = GetSpecToInternalIndex
				// find opening style, and follow up !!
				
				IEnumerable<Style> styles = clt.Player.GetSpecList().SelectMany(e => e.PretendStylesForLiving(clt.Player, clt.Player.MaxLevel));
				
				// Is a followup
				if (style.OpeningRequirementType == Style.eOpening.Offensive && style.AttackResultRequirement == Style.eAttackResultRequirement.Style)
				{
					Style st = styles.Where(s => s.ID == style.OpeningRequirementValue).FirstOrDefault();
					if (st != null)
					{
						// opening style should be only one.
						dw.AddKeyValuePair("OpeningStyle", st.Name);
					}
				}
				
				// Has Followup ?
				foreach (Style stl in styles.Where(s => (s.OpeningRequirementType == Style.eOpening.Offensive && s.AttackResultRequirement == Style.eAttackResultRequirement.Style && s.OpeningRequirementValue == style.ID)))
				{
					// we found the style that needs this one for opening.
					dw.AppendKeyValuePair("FollowupStyle", stl.Name);
				}
				
				dw.AddKeyValuePair("Name", style.Name);
				dw.AddKeyValuePair("Icon", style.Icon);
				dw.AddKeyValuePair("Level", style.Level);
				dw.AddKeyValuePair("Fatigue", style.EnduranceCost);
				//.Value("SpecialType", (int)style.SpecialType, style.SpecialType != 0)
				//.Value("SpecialNumber", GetSpecialNumber(style), GetSpecialNumber(style)!=0)
				if (style.BonusToDefense != 0)
					dw.AddKeyValuePair("DefensiveMod", style.BonusToDefense);
				if (style.BonusToHit != 0)
					dw.AddKeyValuePair("AttackMod", style.BonusToHit);
				dw.AddKeyValuePair("OpeningType", (int)style.OpeningRequirementType);
				if (style.OpeningRequirementType == Style.eOpening.Positional)
					dw.AddKeyValuePair("OpeningNumber", style.OpeningRequirementValue);				
				//.Value("OpeningResult",GetOpeningResult(style,clt),GetOpeningResult(style,clt)>0)
				//.Value("OpeningStyle",GetOpeningStyle(style),(Style.eAttackResult)GetOpeningResult(style,clt) == Style.eAttackResult.Style)
				if (style.WeaponTypeRequirement > 0)
					dw.AddKeyValuePair("Weapon", style.GetRequiredWeaponName());
				if (style.StealthRequirement)
					dw.AddKeyValuePair("Hidden", "1");
				//.Value("TwoHandedIcon", 10, style.TwoHandAnimation > 0)
				//.Value("Skill",43)
				if (style.GrowthRate>0)
					dw.AddKeyValuePair("OpeningDamage",style.GrowthRate*100);
				//.Value("SpecialValue", GetSpecialValue(style),GetSpecialValue(style)!=0)
				//.Value("FollowupStyle",style.DelveFollowUpStyles,!string.IsNullOrEmpty(style.DelveFollowUpStyles))
            }
            else
            {
                dw.AddKeyValuePair("Name", "(not found)");
            }
            
            return dw.ToString();
        }

		#region style v1.110 methods
		/*
		public static int GetSpecialNumber(Style style)
		{
			if (style.SpecialType == Style.eSpecialType.Effect)
			{
				Spell spell = SkillBase.GetSpellById(style.SpecialValue);
				if (spell != null)
					return spell.ClientEffect;
			}
			return 0;
		}

		public static int GetSpecialValue(Style style)
		{
			switch(style.SpecialType)
			{
				case Style.eSpecialType.ExtendedRange:
					return 128; // Extended Range für Reaver style
				case Style.eSpecialType.Taunt:
					return style.SpecialValue;
			}
			return 0;
		}*/

		
		/*public static int GetOpeningResult(Style style,GameClient clt)
		{
			switch(StyleProcessor.ResolveAttackResult(style,clt.Player.PlayerCharacter.Class))
			{
				case GameLiving.eAttackResult.Any:
					return (int)Style.eAttackResult.Any;
				case GameLiving.eAttackResult.Missed:
					return (int) Style.eAttackResult.Miss;
				case GameLiving.eAttackResult.Parried:
					return (int)Style.eAttackResult.Parry;
				case GameLiving.eAttackResult.Evaded:
					return (int)Style.eAttackResult.Evade;
				case GameLiving.eAttackResult.Blocked:
					return (int)Style.eAttackResult.Block;
				case GameLiving.eAttackResult.Fumbled:
					return (int)Style.eAttackResult.Fumble;
				case GameLiving.eAttackResult.HitStyle:
					return (int)Style.eAttackResult.Style;
				case GameLiving.eAttackResult.HitUnstyled:
					return (int)Style.eAttackResult.Hit;
			}
			return 0;
		}*/
		/*
		public static string GetOpeningStyle(Style style)
		{
			if (style.OpeningRequirementValue > 0)
			{
				Style style2 = SkillBase.GetStyleByID(style.OpeningRequirementValue);
				if (style2!=null)
					return style2.Name;
				return "";
			}
			return "";
		}*/

		#endregion

		/// <summary>
		/// Delve the realm abilities for v1.110+ clients
		/// </summary>
		/// <param name="clt"></param>
		/// <param name="id"></param>
		/// <returns></returns>
        public static string DelveRealmAbility(GameClient clt, int id)
        {
			Skill ra = clt.Player.GetAllUsableSkills().Where(e => e.Item1.InternalID == id && e.Item1 is Ability).Select(e => e.Item1).FirstOrDefault();
			
			if (ra == null)
			{
				ra = SkillBase.GetAbilityByInternalID(id);
			}
			
			MiniDelveWriter dw = new MiniDelveWriter("RealmAbility");
			dw.AddKeyValuePair("Index",  unchecked((short)id));
			
            if (ra is RealmAbility)
            {
           		((RealmAbility)ra).AddDelve(ref dw);
            }
            else if (ra != null)
            {
                dw.AddKeyValuePair("Name", ra.Name);
            }
            else
            {
           		dw.AddKeyValuePair("Name", "(not found)");
            }
            
            return dw.ToString();
        }
        #endregion
    }
}