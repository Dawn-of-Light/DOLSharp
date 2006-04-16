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
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// spell crafdting skill
	/// </summary>
	public class SpellCrafting : AdvancedCraftingSkill
	{
		private static readonly int[ , ] itemMaxBonusLevel =  // taken from mythic Spellcraft calculator
		{
			{0,1,1,1,1,1,1},
			{1,1,1,1,1,2,2},
			{1,1,1,2,2,2,2},
			{1,1,2,2,2,3,3},
			{1,2,2,2,3,3,4},
			{1,2,2,3,3,4,4},
			{2,2,3,3,4,4,5},
			{2,3,3,4,4,5,5},
			{2,3,3,4,5,5,6},
			{2,3,4,4,5,6,7},
			{2,3,4,5,6,6,7},
			{3,4,4,5,6,7,8},
			{3,4,5,6,6,7,9},
			{3,4,5,6,7,8,9},
			{3,4,5,6,7,8,10},
			{3,5,6,7,8,9,10},
			{4,5,6,7,8,10,11},
			{4,5,6,8,9,10,12},
			{4,6,7,8,9,11,12},
			{4,6,7,8,10,11,13},
			{4,6,7,9,10,12,13},
			{5,6,8,9,11,12,14},
			{5,7,8,10,11,13,15},
			{5,7,9,10,12,13,15},
			{5,7,9,10,12,14,16},
			{5,8,9,11,12,14,16},
			{6,8,10,11,13,15,17},
			{6,8,10,12,13,15,18},
			{6,8,10,12,14,16,18},
			{6,9,11,12,14,16,19},
			{6,9,11,13,15,17,20},
			{7,9,11,13,15,17,20},
			{7,10,12,14,16,18,21},
			{7,10,12,14,16,19,21},
			{7,10,12,14,17,19,22},
			{7,10,13,15,17,20,23},
			{8,11,13,15,17,20,23},
			{8,11,13,16,18,21,24},
			{8,11,14,16,18,21,24},
			{8,11,14,16,19,22,25},
			{8,12,14,17,19,22,26},
			{9,12,15,17,20,23,26},
			{9,12,15,18,20,23,27},
			{9,13,15,18,21,24,27},
			{9,13,16,18,21,24,28},
			{9,13,16,19,22,25,29},
			{10,13,16,19,22,25,29},
			{10,14,17,20,23,26,30},
			{10,14,17,20,23,27,31},
			{10,14,17,20,23,27,31},
			{10,15,18,21,24,28,32},
		};

		/// <summary>
		/// Constructor
		/// </summary>
		public SpellCrafting()
		{
			Icon = 0x0D;
			Name = "Evocation";
			eSkill = eCraftingSkill.SpellCrafting;
		}

		#region Classic craft functions

		/// <summary>
		/// Check if  the player own all needed tools
		/// </summary>
		/// <param name="player">the crafting player</param>
		/// <param name="craftItemData">the object in construction</param>
		/// <returns>true if the player hold all needed tools</returns>
		public override bool CheckTool(GamePlayer player, CraftItemData craftItemData)
		{
			if(! base.CheckTool(player, craftItemData)) return false;

			bool spellcraftKitFound = false;
			foreach (GenericItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				if(!(item is CraftingTool)) continue;

				if(((CraftingTool)item).Type == eCraftingToolType.SpellcraftKit)
				{
					spellcraftKitFound = true;
					break;
				}
			}

			if(spellcraftKitFound == false)
			{
				player.Out.SendMessage("You do not have the tools to make the "+craftItemData.TemplateToCraft.Name+".",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				player.Out.SendMessage("You must find a spellcraft kit!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Select craft to gain point and increase it
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		public override void GainCraftingSkillPoints(GamePlayer player, CraftItemData item)
		{
			base.GainCraftingSkillPoints(player, item);

			if(player.CraftingPrimarySkill == eCraftingSkill.SpellCrafting)
			{
				if(player.GetCraftingSkillValue(eCraftingSkill.SpellCrafting)%100 == 99)
				{
					player.Out.SendMessage("You must see your trainer to raise your Spellcraft further!",eChatType.CT_Important,eChatLoc.CL_SystemWindow);
					return;
				}
			}
			else
			{
				int maxAchivableLevel;
				switch (player.CraftingPrimarySkill)
				{
					case eCraftingSkill.Alchemy:
					{
						maxAchivableLevel = (int)(player.GetCraftingSkillValue(eCraftingSkill.Alchemy) * 0.45);
						break;
					}

					default:
					{
						maxAchivableLevel = 0;
						break;
					}
				}

				if(player.GetCraftingSkillValue(eCraftingSkill.SpellCrafting) >= maxAchivableLevel)
				{
					return;
				}
			}

			if(Util.Chance( CalculateChanceToGainPoint(player, item)))
			{
				player.IncreaseCraftingSkill(eCraftingSkill.SpellCrafting, 1);
				player.Out.SendUpdateCraftingSkills();
			}
		}

		#endregion

		#region Requirement check

		/// <summary>
		/// This function is called when player accept the combine
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		public override bool IsAllowedToCombine(GamePlayer player, GenericItem item)
		{
			if(! base.IsAllowedToCombine(player, item)) return false;
					
			if(player.TradeWindow.TradeItems[0] is MagicalDust) // first ingredient to combine is Dust => echantement
			{
				if (item.Level < 15)
				{
					player.Out.SendMessage("This item can't be enchanted!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return false;	
				}

				lock (player.TradeWindow.Sync)
				{
					foreach(GenericItem materialToCombine in player.TradeWindow.TradeItems)
					{
						if(!(materialToCombine is MagicalDust))
						{
							player.Out.SendMessage(materialToCombine.Name+" can't be used to enchant a item!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							return false;
						}
					}
				}
			}
			else
			{
				EquipableItem itemToEnchant = (EquipableItem)item;
				if(itemToEnchant.MagicalBonus.Count >= 4)
				{
					player.Out.SendMessage("The "+item.Name+" is already imbued with 4 different types of magical properties!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return false;
				}

				lock (player.TradeWindow.Sync)
				{
					for(int i = 0; i < player.TradeWindow.ItemsCount; i++)
					{
						SpellCraftGem currentGem = player.TradeWindow.TradeItems[i] as SpellCraftGem;

						if(currentGem == null)
						{
							player.Out.SendMessage("This item can't be used to Spellcraft!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							return false;
						}
				
						foreach(ItemMagicalBonus bonus in itemToEnchant.MagicalBonus)
						{
							if(bonus.BonusType == currentGem.BonusType)
							{
								player.Out.SendMessage("You can't put the same bonus on an item multiple times!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
								return false;
							}
						}
					}
				}

				if(itemToEnchant.MagicalBonus.Count + player.TradeWindow.ItemsCount >= 4)
				{
					player.Out.SendMessage("The "+item.Name+" can only be imbued with 4 different types of magical properties!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return false;
				}

				int bonusLevel = GetTotalImbuePoints(player, itemToEnchant);
				if(bonusLevel > player.GetCraftingSkillValue(eCraftingSkill.SpellCrafting)/20)
				{
					player.Out.SendMessage("You have not enough skill to imbue that much!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return false;
				}

				if(bonusLevel - GetItemMaxImbuePoints(itemToEnchant) > 5)
				{
					player.Out.SendMessage("You can't overcharge your "+item.Name+" more than 5 levels!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Apply magical effect

		/// <summary>
		/// Apply all needed magical bonus to the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		protected override void ApplyMagicalEffect(GamePlayer player, EquipableItem item)
		{
			if(item == null || player.TradeWindow.TradeItems[0] == null) return ; // be sure at least one item in each side
			
			if(player.TradeWindow.TradeItems[0] is MagicalDust) // Echant item
			{
				ApplyMagicalDusts(player, item);
			}
			else if(player.TradeWindow.TradeItems[0] is SpellCraftGem) // Spellcraft item
			{
				ApplySpellcraftGems(player, item);
			}
		}

		/// <summary>
		/// Apply all magical dust
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		private void ApplyMagicalDusts(GamePlayer player, EquipableItem item)
		{
			int spellCrafterLevel = player.GetCraftingSkillValue(eCraftingSkill.SpellCrafting);
			
			int bonusCap;
			if (spellCrafterLevel < 300 || item.Level < 15 )		bonusCap = 0;
			else if (spellCrafterLevel < 400 || item.Level < 20	)	bonusCap = 5;
			else if (spellCrafterLevel < 500 || item.Level < 25 )	bonusCap = 10;
			else if (spellCrafterLevel < 600 || item.Level < 30 )	bonusCap = 15;
			else if (spellCrafterLevel < 700 || item.Level < 35 )	bonusCap = 20;
			else if (spellCrafterLevel < 800 || item.Level < 40 )	bonusCap = 25;
			else if (spellCrafterLevel < 900 || item.Level < 45 )	bonusCap = 30;
			else													bonusCap = 35;
			
			((GamePlayerInventory)player.Inventory).BeginChanges();

			int bonusMod = item.Bonus;
			lock (player.TradeWindow.Sync)
			{
				foreach (MagicalDust dust in  player.TradeWindow.TradeItems)
				{
					switch (dust.Name)
					{
						case "coppery imbued dust": bonusMod +=2;break;
						case "coppery enchanted dust": bonusMod +=4;break;
						case "coppery glowing dust": bonusMod +=6;break;
						case "coppery ensorcelled dust": bonusMod +=8;break;

						case "silvery imbued dust": bonusMod +=10;break;
						case "silvery enchanted dust": bonusMod +=12;break;
						case "silvery glowing dust": bonusMod +=14;break;
						case "silvery ensorcelled dust": bonusMod +=16;break;

						case "golden imbued dust": bonusMod +=18;break;
						case "golden enchanted dust": bonusMod +=20;break;
						case "golden glowing dust": bonusMod +=22;break;
						case "golden ensorcelled dust": bonusMod +=24;break;

						case "mithril imbued dust": bonusMod +=26;break;
						case "mithril enchanted dust": bonusMod +=28;break;
						case "mithril glowing dust": bonusMod +=30;break;
						case "mithril ensorcelled dust": bonusMod +=32;break;

						case "platinum imbued dust": bonusMod +=34;break;
						case "platinum enchanted dust": bonusMod +=36;break;
						case "platinum glowing dust": bonusMod +=38;break;
						case "platinum ensorcelled dust": bonusMod +=40;break;

					}
					player.Inventory.RemoveItem(dust);
				}
			}

			item.Bonus = (byte)Math.Min(bonusMod, bonusCap);

			((GamePlayerInventory)player.Inventory).CommitChanges();
		}

		/// <summary>
		/// Apply all spellcraft gems bonus
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		private void ApplySpellcraftGems(GamePlayer player, EquipableItem item)
		{
			int maxBonusLevel = GetItemMaxImbuePoints(item);
			int bonusLevel = GetTotalImbuePoints(player, item);

			int sucessChances = 100;
			if(bonusLevel > maxBonusLevel) sucessChances = CalculateChanceToOverchargeItem(player, item, maxBonusLevel, bonusLevel);
 
			GamePlayer tradePartner = player.TradeWindow.Partner;

			if (Util.Chance(sucessChances))
			{
				lock (player.TradeWindow.Sync)
				{
					((GamePlayerInventory)player.Inventory).BeginChanges();
					foreach(SpellCraftGem gem in (ArrayList)player.TradeWindow.TradeItems.Clone())
					{
						item.MagicalBonus.Add(new ItemMagicalBonus(gem.BonusType, gem.Bonus));
						player.Inventory.RemoveItem(gem);
					}
					((GamePlayerInventory)player.Inventory).CommitChanges();
				}
	
				if(tradePartner != null)
				{
					tradePartner.Out.SendMessage(player.Name+" imbued 1 item.",eChatType.CT_Important,eChatLoc.CL_SystemWindow);
				}
				player.Out.SendMessage(player.Name+" imbued 1 item.",eChatType.CT_Important,eChatLoc.CL_SystemWindow);
			}
			else
			{
				player.Out.SendMessage("The power of the enchantments explodes before your eyes!",eChatType.CT_Important,eChatLoc.CL_SystemWindow);
				
				lock (player.TradeWindow.Sync)
				{
					((GamePlayerInventory)player.Inventory).BeginChanges();
					if(tradePartner != null && item.Owner == tradePartner)
					{
						tradePartner.Inventory.RemoveItem(item);
					}
					else
					{
						player.Inventory.RemoveItem(item);
					}

					foreach(GenericItem gem in (ArrayList)player.TradeWindow.TradeItems.Clone())
					{
						player.Inventory.RemoveItem(gem);
					}
					((GamePlayerInventory)player.Inventory).CommitChanges();
				}
				
				player.Emote(eEmote.SpellGoBoom);
				player.Die(player); // On official you take damages
				player.Out.SendMessage("The Spellcraft failed!",eChatType.CT_Important,eChatLoc.CL_SystemWindow);
				
				if(tradePartner != null)
				{
					if (Util.Chance(40))
					{
						tradePartner.Emote(eEmote.SpellGoBoom);
						tradePartner.Die(player);
					}
					tradePartner.Out.SendMessage("The Spellcraft failed!",eChatType.CT_Important,eChatLoc.CL_SystemWindow);
				}
			}
		}

		#endregion

		# region Show informations to player

		/// <summary>
		/// Show to player all infos about the current spellcraft
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		public virtual void ShowSpellCraftingInfos(GamePlayer player, EquipableItem item)
		{
			if(!(player.TradeWindow.TradeItems[0] is SpellCraftGem)) return; // check if applying dust

			ArrayList spellcraftInfos = new ArrayList(10);

			int totalItemCharges = GetItemMaxImbuePoints(item);
			int totalGemmesCharges = GetTotalImbuePoints(player, item);

			spellcraftInfos.Add("Spellcrafting the "+ item.Name +" ("+ totalItemCharges +" total imbue points)");
			spellcraftInfos.Add("Bonus currently on the "+ item.Name +" :");
			foreach(ItemMagicalBonus bonus in item.MagicalBonus)
			{
				spellcraftInfos.Add("\t"+ SkillBase.GetPropertyName(bonus.BonusType) +": "+ bonus.Bonus +" "+ ((bonus.BonusType >= eProperty.Resist_First && bonus.BonusType <= eProperty.Resist_Last) ? "%" : " pts"));
			}
			
			spellcraftInfos.Add("Gem Bonuses:");
			lock (player.TradeWindow.Sync)
			{
				for(int i = 0; i < player.TradeWindow.ItemsCount; i++)
				{
					SpellCraftGem currentGem = (SpellCraftGem)player.TradeWindow.TradeItems[i];
					spellcraftInfos.Add("\t"+ currentGem.Name +" - "+ SkillBase.GetPropertyName(currentGem.BonusType) +": ("+ GetGemImbuePoints(currentGem.BonusType, currentGem.Bonus) +") "+ currentGem.Bonus +" "+ ((currentGem.BonusType >= eProperty.Resist_First && currentGem.BonusType <= eProperty.Resist_Last) ? "%" : "pts"));
				}
			}
			spellcraftInfos.Add("Used "+ totalGemmesCharges +" of "+ totalItemCharges +" imbue point capacity.");

			if(totalGemmesCharges > totalItemCharges)
			{
				spellcraftInfos.Add("\t"+ GetOverchargePenality(totalItemCharges, totalGemmesCharges) +" overcharge penality");
			}
			
			GamePlayer partner = player.TradeWindow.Partner;
			foreach(string line in spellcraftInfos)
			{
				player.Out.SendMessage(line, eChatType.CT_Important,eChatLoc.CL_SystemWindow);
				if(partner != null) partner.Out.SendMessage(line, eChatType.CT_Important,eChatLoc.CL_SystemWindow);
			}

			if(totalGemmesCharges > totalItemCharges)
			{
				player.Out.SendMessage("(This penality is further modified by item quality and your skill!)", eChatType.CT_System,eChatLoc.CL_SystemWindow);
			}
		}

		# endregion

		#region Calcul functions

		/// <summary>
		/// Get the sucess chance to overcharge the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="maxBonusLevel"></param>
		/// <param name="bonusLevel"></param>
		/// <returns></returns>
		protected int CalculateChanceToOverchargeItem(GamePlayer player, EquipableItem item, int maxBonusLevel, int bonusLevel)
		{
			int baseChance = GetOverchargePenality(maxBonusLevel, bonusLevel);

			int gemmeModifier = 0;
			lock (player.TradeWindow.Sync)
			{
				foreach (SpellCraftGem gemme in player.TradeWindow.TradeItems)
				{
					if (gemme.Quality == 96) gemmeModifier += 1;
					else if (gemme.Quality == 97) gemmeModifier += 3;
					else if (gemme.Quality == 98) gemmeModifier += 5;
					else if (gemme.Quality == 99) gemmeModifier += 8;
					else if (gemme.Quality == 100) gemmeModifier += 11;
				}
			}

			int itemQualityModifier=0;
			if (item.Quality == 96) itemQualityModifier += 6;
			else if (item.Quality == 97) itemQualityModifier += 8;
			else if (item.Quality == 98) itemQualityModifier += 10;
			else if (item.Quality == 99) itemQualityModifier += 18;
			else if (item.Quality == 100) itemQualityModifier += 26;
			
			int intSkill = player.GetCraftingSkillValue(eCraftingSkill.SpellCrafting);
			int intSkillMod;
			if (intSkill < 50) { intSkillMod = -50; }
			else if (intSkill < 100) { intSkillMod = -45; }
			else if (intSkill < 150) { intSkillMod = -40; }
			else if (intSkill < 200) { intSkillMod = -35; }
			else if (intSkill < 250) { intSkillMod = -30; }
			else if (intSkill < 300) { intSkillMod = -25; }
			else if (intSkill < 350) { intSkillMod = -20; }
			else if (intSkill < 400) { intSkillMod = -15; }
			else if (intSkill < 450) { intSkillMod = -10; }
			else if (intSkill < 500) { intSkillMod = -5; }
			else if (intSkill < 550) { intSkillMod = 0; }
			else if (intSkill < 600) { intSkillMod = 5; }
			else if (intSkill < 650) { intSkillMod = 10; }
			else if (intSkill < 700) { intSkillMod = 15; }
			else if (intSkill < 750) { intSkillMod = 20; }
			else if (intSkill < 800) { intSkillMod = 25; }
			else if (intSkill < 850) { intSkillMod = 30; }
			else if (intSkill < 900) { intSkillMod = 35; }
			else if (intSkill < 950) { intSkillMod = 40; }
			else if (intSkill < 1000) { intSkillMod = 45; }
			else { intSkillMod = 50; }

			int finalChances = intSkillMod + itemQualityModifier + gemmeModifier + baseChance;
			if (finalChances > 100) finalChances = 100;
			else if (finalChances < 0) finalChances = 0;

			return finalChances;
		}

		/// <summary>
		/// Get the maximum bonus level the item can hold
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		protected int GetItemMaxImbuePoints(EquipableItem item)
		{
			if(item.Quality < 94) return 0;
			if(item.Level > 51) return 32;
			if(item.Level < 1) return 0;
			return itemMaxBonusLevel[item.Level - 1, item.Quality - 94];
		}

		/// <summary>
		/// Get gems bonus level
		/// </summary>
		/// <param name="player">player spellcrafting</param>
		/// <param name="item">item to calculate imbues for</param>
		/// <returns></returns>
		protected int GetTotalImbuePoints(GamePlayer player, EquipableItem item)
		{
			int totalGemBonus = 0;
			int biggerGemCharge = 0;	
			
			foreach(ItemMagicalBonus bonus in item.MagicalBonus)
			{
				int currentBonusImbuePoint = GetGemImbuePoints(bonus.BonusType, bonus.Bonus);
				totalGemBonus += currentBonusImbuePoint;
				if(currentBonusImbuePoint > biggerGemCharge) biggerGemCharge = currentBonusImbuePoint;
			}

			lock (player.TradeWindow.Sync)
			{
				for(int i = 0 ; i < player.TradeWindow.TradeItems.Count ; i++)
				{
					SpellCraftGem currentGem = (SpellCraftGem)player.TradeWindow.TradeItems[i];
					int currentGemCharge = GetGemImbuePoints(currentGem.BonusType, currentGem.Bonus);
					if(currentGemCharge > biggerGemCharge) biggerGemCharge = currentGemCharge;
					
					totalGemBonus += currentGemCharge;	
				}
			}
			totalGemBonus += biggerGemCharge;
			totalGemBonus /= 2;

			return (int)Math.Floor((double)totalGemBonus);
		}

		/// <summary>
		/// Get how much the gem use like bonus point
		/// </summary>
		/// <param name="bonusType"></param>
		/// <param name="bonusValue"></param>
		/// <returns></returns>
		protected int GetGemImbuePoints(eProperty bonusType, Int16 bonusValue)
		{
			int gemBonus;
			if (bonusType <= eProperty.Stat_Last)			gemBonus = (int)(((bonusValue - 1) * 2 / 3) + 1); //stat
			else if (bonusType == eProperty.MaxMana)		gemBonus = (int)((bonusValue * 2) - 2); //mana
			else if (bonusType == eProperty.MaxHealth)		gemBonus = (int)(bonusValue / 4); //HP
			else if (bonusType <= eProperty.Resist_Last)	gemBonus = (int)((bonusValue * 2) - 2);//resist
			else if (bonusType <= eProperty.Skill_Last)	gemBonus = (int)((bonusValue - 1) * 5);//skill
			else												gemBonus = 1;// focus
			if(gemBonus < 1) gemBonus = 1;

			return gemBonus;
		}

		/// <summary>
		/// Get the % overcharge penality
		/// </summary>
		/// <param name="maxBonusLevel"></param>
		/// <param name="bonusLevel"></param>
		/// <returns></returns>
		protected int GetOverchargePenality(int maxBonusLevel, int bonusLevel)
		{
			int diff = bonusLevel - maxBonusLevel;
			if		( diff == 1 ) return -10;
			else if ( diff == 2 ) return -20;
			else if ( diff == 3 ) return -30;
			else if ( diff == 4 ) return -50;
			else				  return -70;
		}
		
		#endregion

	}
}
