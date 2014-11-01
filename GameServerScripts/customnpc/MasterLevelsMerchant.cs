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
using System.Collections.Generic;
using System.Linq;

using DOL.GS;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// MasterLevelsMerchant is responsible for selling ML Leveling Item and receiving them.
	/// </summary>
	public class MasterLevelsMerchant : GameBountyMerchant
	{
		private static MerchantTradeItems m_offeredItems = new MerchantTradeItems("Master Level Credits");
		
		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{			
			int[] prices = new int[] { 0, 1500, 1500, 3750, 3000, 2000, 3250, 2500, 3500, 4000, 5000 };
			
			for (int i = 1 ; i < 11 ; i++)
			{
				ItemTemplate credit = GameServer.Database.FindObjectByKey<ItemTemplate>(string.Format("master_level_credit_bptoken_{0}", i));
				if (credit == null)
				{
					credit = new ItemTemplate();
					credit.AllowAdd = true;
					credit.Id_nb = string.Format("master_level_credit_bptoken_{0}", i);
					credit.Weight = 0;
					credit.Price = prices[i];
					credit.Condition = 50000;
					credit.MaxCondition = 50000;
					credit.Model = 0x01E7;
					credit.Name = string.Format("Master Level {0} Credit", i);
					GameServer.Database.AddObject(credit);
				}
				m_offeredItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, credit);
			}
			
			// Respec
			ItemTemplate resp = GameServer.Database.FindObjectByKey<ItemTemplate>("master_level_respec_bptoken");
			if (resp == null)
			{
				resp = new ItemTemplate();
				resp.AllowAdd = true;
				resp.Id_nb = "master_level_respec_bptoken";
				resp.Weight = 5;
				resp.Price = 5000;
				resp.Condition = 50000;
				resp.MaxCondition = 50000;
				resp.Level = 40;
				resp.Object_Type = 0x29;
				resp.Model = 0x075E;
				resp.Name = "Star of Destiny";
				GameServer.Database.AddObject(resp);
			}
			m_offeredItems.AddTradeItem(0, eMerchantWindowSlot.FirstEmptyInPage, resp);
		}
		
		public MasterLevelsMerchant()
		{
			m_tradeItems = m_offeredItems;
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			TurnTo(player, 10000);
			SendInteractMessage(player);
			return true;
		}
		
		public virtual void SendInteractMessage(GamePlayer player)
		{
			string text;
			if (player.Level > 39)
				text = string.Format("Hello {0}, you can buy Master Level Credits here to acquire Ancients knowledge...\n\nYou can review your available Paths by asking [details]\n\nHand me your Master Level Credits in order and I shall entrust you with our secrets !", player.Name);
			else
				text = string.Format("Hello {0}, you're too much inexperimented to take any benefits from my knowledge, please come back when you're over Level 40 !", player.Name);
			
			player.Out.SendMessage(text, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			
			if (player.MLGranted && player.MLLevel == 0)
			{
				try
				{
					var mls = SkillBase.GetSpecializationCareer(player.CharacterClass.ID).Where(sp => sp.Key is IMasterLevelsSpecialization).Select(sp => sp.Key.KeyName);
					// First Token
					text = string.Format("Which path will you choose to advance your knowledge of Atlantis ?\n\nWill you seek the path of [{0}] or would you rather enter the ways of [{1}] ?\n\nthis choice can't be undone, unless you can find a Star of Destiny !"
					                            , mls.ElementAtOrDefault(0), mls.ElementAtOrDefault(1));
					player.Out.SendMessage(text, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}
				catch
				{
				}
			}
		}
		
		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text))
				return false;
			if (source is GamePlayer == false)
				return true;
			
			GamePlayer player = (GamePlayer)source;
			
			if (player.Level < 40)
				return true;
			
			if (text.ToLower().Equals("details"))
			{
				List<Tuple<Specialization, List<Tuple<Skill, byte>>>> tree = new List<Tuple<Specialization, List<Tuple<Skill, byte>>>>();
				// send custom trainer window
				var specs = SkillBase.GetSpecializationCareer(player.CharacterClass.ID).Where(sp => sp.Key is IMasterLevelsSpecialization);
				int first = 0;
				foreach (var spec in specs)
				{
					if (first > 0)
					{
						//space
						tree.Add(new Tuple<Specialization, List<Tuple<Skill, byte>>>(spec.Key, new List<Tuple<Skill, byte>>()));
					}
					
					int count = 0;
					Tuple<Specialization, List<Tuple<Skill, byte>>> element = null;
					foreach (var sk in spec.Key.PretendLinesSpellsForLiving(player, 10).Values.FirstOrDefault().OrderBy(sk => 
					                                                                                                    {
					                                                                                                    	if (sk is Styles.Style)
					                                                                                                    		return ((Styles.Style)sk).SpecLevelRequirement;
					                                                                                                    	if (sk is Ability)
					                                                                                                    		return ((Ability)sk).SpecLevelRequirement;
					                                                                                                    	if (sk is Spell)
					                                                                                                    		return ((Spell)sk).Level;
					                                                                                                    	
					                                                                                                    	return 0;
					                                                                                                    }))
					{
						if ((count % 5) == 0)
						{
							element = new Tuple<Specialization, List<Tuple<Skill, byte>>>(spec.Key, new List<Tuple<Skill, byte>>());
							tree.Add(element); 
						}
						
						count++;
						element.Item2.Add(new Tuple<Skill, byte>(sk, 0));
					}
					first++;
				}
				
				player.Out.SendCustomTrainerWindow(0xFE, tree);
				return false;
			}
			
			if (player.MLGranted && player.MLLevel == 0)
			{
				var mls = SkillBase.GetSpecializationCareer(player.CharacterClass.ID).Where(sp => sp.Key is IMasterLevelsSpecialization).ToList();
				int index = mls.FindIndex(sp => sp.Key.KeyName.ToLower().Equals(text.ToLower()));
				if (index > -1)
				{
					string msg = string.Format("You will learn the Master Level Path of {0}, are you sure ?", mls[index].Key.KeyName);
					player.TempProperties.setProperty("ML_MERCHANT_PATH_CHOSEN_PROPERTY", index);
					player.Out.SendCustomDialog(msg, CallbackPathMasterLevel);
					return false;
				}
			}
			
			return true;
		}
		
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			if(!base.ReceiveItem(source, item))
			{
				// Try to take it
				if (!(source is GamePlayer))
					return false;
				
				GamePlayer player = (GamePlayer)source;
				
				if (player.Level < 40)
				{
					string text = "I can take any items from such inexperimented traveler, please come back when you're over Level 40 !";
					player.Out.SendMessage(text, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return false;
				}
				
				// Check the item whe received.
				if (item.Id_nb.StartsWith("master_level_respec_") && item.Id_nb.EndsWith("token"))
				{
					if (player.MLLevel > 0) 
					{
						string text;
						try
						{
							text = string.Format("This will change your Master Level Path to {0}, are you sure ?",
						                     SkillBase.GetSpecializationCareer(player.CharacterClass.ID).Where(sp => sp.Key is IMasterLevelsSpecialization).ElementAtOrDefault((player.MLLine + 1) % 2).Key.KeyName);
							player.TempProperties.setProperty("ML_MERCHANT_RESPEC_GIVEN_PROPERTY", item);
							player.Out.SendCustomDialog(text, CallbackRespecMasterLevel);
						}
						catch
						{
							return false;
						}
					}
					else
					{
						string text = "You must train in the secrets of the Ancients before thinking about changing Path !";
						player.Out.SendMessage(text, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					
					return true;
				}
				
				if (item.Id_nb.StartsWith("master_level_credit_") && item.Id_nb.EndsWith("token_1"))
				{
					if (player.MLLevel == 0 && !player.MLGranted)
					{
						if (player.Inventory.RemoveItem(item))
						{
							InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, item.Template, 1);
							player.MLGranted = true;
							var mls = SkillBase.GetSpecializationCareer(player.CharacterClass.ID).Where(sp => sp.Key is IMasterLevelsSpecialization).Select(sp => sp.Key.KeyName);
							// First Token
							string text = string.Format("Which path will you choose to advance your knowledge of Atlantis ?\n\nWill you seek the path of [{0}] or would you rather enter the ways of [{1}] ?\n\nthis choice can't be undone, unless you can find a Star of Destiny !"
							                            , mls.ElementAtOrDefault(0), mls.ElementAtOrDefault(1));
							player.Out.SendMessage(text, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
						else
						{
							return false;
						}
					}
					else
					{
						string text = string.Format("You need to give me the credit for Level {0} to continue learning your path !", player.MLLevel+1);
						player.Out.SendMessage(text, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					
					return true;
				}
				
				if (item.Id_nb.StartsWith("master_level_credit_"))
				{
					if (item.Id_nb.EndsWith(string.Format("token_{0}", player.MLLevel+1)))
					{
						// Following Tokens
						if (player.Inventory.RemoveItem(item))
						{
							InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, item.Template, 1);
							player.MLLevel++;
							player.RefreshSpecDependantSkills(true);
							player.Out.SendUpdatePlayerSkills();
							player.Out.SendUpdatePlayer();
							string text = string.Format("You have been granted knowledge of the Master Level {0} !", player.MLLevel);
							player.Out.SendMessage(text, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
						else
						{
							return false;
						}
					}
					else
					{
						if (player.MLLevel < GamePlayer.ML_MAX_LEVEL)
						{
							string text = string.Format("You need to give me the credit for Level {0} to continue learning your path !", player.MLLevel+1);
							player.Out.SendMessage(text, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						}
						else
						{
							string text = "You have acquire all the knowledge we could give you, try other Paths if you're looking for newer Skills !";
							player.Out.SendMessage(text, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							return false;
						}
					}
					
					return true;
				}
			}
			
			return false;
		}
		
		protected virtual void CallbackPathMasterLevel(GamePlayer player, byte response)
		{
			int index = player.TempProperties.getProperty<int>("ML_MERCHANT_PATH_CHOSEN_PROPERTY", -1);
			
			if (response != 0x00 && index != -1 && player.MLGranted && player.MLLevel == 0)
			{
				player.MLLine = (byte)index;
				player.MLLevel = 1;
				player.RefreshSpecDependantSkills(true);
				player.Out.SendUpdatePlayerSkills();
				player.Out.SendUpdatePlayer();
			}
		}
		
		protected virtual void CallbackRespecMasterLevel(GamePlayer player, byte response)
		{
			// test item
			InventoryItem item = player.TempProperties.getProperty<InventoryItem>("ML_MERCHANT_RESPEC_GIVEN_PROPERTY", null);
			
			if (response != 0x00 && item != null && player.Inventory.RemoveItem(item))
			{
				player.MLLine = (byte)((player.MLLine + 1) % 2);
				player.RefreshSpecDependantSkills(true);
				player.Out.SendUpdatePlayerSkills();
				player.Out.SendUpdatePlayer();
			}
		}

	}
}
