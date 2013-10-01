/* DAWN OF LIGHT - The first free open source DAoC server emulator
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
*/
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using DOL.Events;
using DOL.Language;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Spells;
using DOL.GS.Effects;

using log4net;

namespace DOL.GS
{
        public class GameMythirian : GameInventoryItem
        {
                private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

                private GameMythirian() { }

                public GameMythirian(ItemTemplate template)
                        : base(template)
                {
                }

                public GameMythirian(ItemUnique template)
                        : base(template)
                {
                }

                public GameMythirian(InventoryItem item)
                        : base(item)
                {
                }

                public override bool CanEquip(GamePlayer player)
                {
                        if (base.CanEquip(player))
                        {
                                if (Type_Damage <= player.ChampionLevel)
                                {
                                        return true;
                                }
                                player.Out.SendMessage("You do not meet the Champion Level requirement to equip this item.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        return false;
                }
                
                #region Overrides

                public override void OnEquipped(GamePlayer player)
                {
                    if (this.Name.ToLower().Contains("ektaktos"))
                    {
                        player.CanBreathUnderWater = true;
                        player.Out.SendMessage("You find yourself able to breathe water like air!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    base.OnEquipped(player);
                }

                public override void OnUnEquipped(GamePlayer player)
                {
                    if (this.Name.ToLower().Contains("ektaktos") && SpellHandler.FindEffectOnTarget(player, "WaterBreathing") == null)
                    {
                        player.CanBreathUnderWater = false;
                        player.Out.SendMessage("With a gulp and a gasp you realize that you are unable to breathe underwater any longer!", eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                    }
                    base.OnUnEquipped(player);
                }
                
                public static int GetStatOverCapBonuses(GameLiving living, eProperty property)
                {
                	int overCap = 0;
                	                	
                	// Check for Mythirian               	
                	if(living == null || !(living is GamePlayer) || living.Inventory == null || living.Inventory.GetItem(eInventorySlot.Mythical) == null || !(living.Inventory.GetItem(eInventorySlot.Mythical) is GameMythirian))
		            {
                		return overCap;
		            }
                	
                	GameMythirian mythitem = ((GameMythirian)living.Inventory.GetItem(eInventorySlot.Mythical));

		            
                	// not a stat
                	if(mythitem == null || property < eProperty.Stat_First || property > eProperty.Stat_Last)
                		return overCap;
                		                	
					int statCapProperty = (int)(eProperty.StatCapBonus_First - eProperty.Stat_First + property);
                	
               		// find a stat cap increase
	            	if(mythitem.Bonus1 > 0 && mythitem.Bonus1Type == statCapProperty)
	            	{
	            			overCap += mythitem.Bonus1;
	            	}

	            	if(mythitem.Bonus2 > 0 && mythitem.Bonus2Type == statCapProperty)
	            	{
	            			overCap += mythitem.Bonus2;
	            	}

	            	if(mythitem.Bonus3 > 0 && mythitem.Bonus3Type == statCapProperty)
	            	{
	            			overCap += mythitem.Bonus3;
	            	}

	            	if(mythitem.Bonus4 > 0 && mythitem.Bonus4Type == statCapProperty)
	            	{
	            			overCap += mythitem.Bonus4;
	            	}

	            	if(mythitem.Bonus5 > 0 && mythitem.Bonus5Type == statCapProperty)
	            	{
	            			overCap += mythitem.Bonus5;
	            	}

	            	if(mythitem.Bonus6 > 0 && mythitem.Bonus6Type == statCapProperty)
	            	{
	            			overCap += mythitem.Bonus6;
	            	}

	            	if(mythitem.Bonus7 > 0 && mythitem.Bonus7Type == statCapProperty)
	            	{
	            			overCap += mythitem.Bonus7;
	            	}

	            	if(mythitem.Bonus8 > 0 && mythitem.Bonus8Type == statCapProperty)
	            	{
	            			overCap += mythitem.Bonus8;
	            	}

	            	if(mythitem.Bonus9 > 0 && mythitem.Bonus9Type == statCapProperty)
	            	{
	            			overCap += mythitem.Bonus9;
	            	}

	            	if(mythitem.Bonus10 > 0 && mythitem.Bonus10Type == statCapProperty)
	            	{
	            			overCap += mythitem.Bonus10;
	            	}

	            	if(mythitem.ExtraBonus > 0 && mythitem.ExtraBonusType == statCapProperty)
	            	{
	            			overCap += mythitem.ExtraBonus;
	            	}
	            	
	            	return overCap;
                }
                
                public static int GetMythicalOverCapBonuses(GameLiving living, eProperty property)
                {
                	int overCap = 0;
                	
                	// Check for Mythirian
               		if(living == null || !(living is GamePlayer) || living.Inventory == null || living.Inventory.GetItem(eInventorySlot.Mythical) == null || !(living.Inventory.GetItem(eInventorySlot.Mythical) is GameMythirian))
		            {
                		return overCap;
		            }
                	
                	GameMythirian mythitem = ((GameMythirian)living.Inventory.GetItem(eInventorySlot.Mythical));
                	
					// Properties that can overcap !

					int mythicalProperty = (int)property;
					if(mythitem == null)
						return overCap;
						
                	switch(property)
                	{
                		case eProperty.SpellDamage :
                		case eProperty.SpellRange :
                		case eProperty.SpellDuration :
                		case eProperty.HealingEffectiveness :
                		case eProperty.PowerPoolCapBonus :
                		case eProperty.ArcaneSyphon :
                		case eProperty.MeleeDamage :
                		break;
                		
                		default:
                		return overCap;
                	}
                	               		
                	// find a stat cap increase
	            	if(mythitem.Bonus1 > 0 && mythitem.Bonus1Type == mythicalProperty)
	            	{
	            			overCap += mythitem.Bonus1;
	            	}

	            	if(mythitem.Bonus2 > 0 && mythitem.Bonus2Type == mythicalProperty)
	            	{
	            			overCap += mythitem.Bonus2;
	            	}

	            	if(mythitem.Bonus3 > 0 && mythitem.Bonus3Type == mythicalProperty)
	            	{
	            			overCap += mythitem.Bonus3;
	            	}

	            	if(mythitem.Bonus4 > 0 && mythitem.Bonus4Type == mythicalProperty)
	            	{
	            			overCap += mythitem.Bonus4;
	            	}

	            	if(mythitem.Bonus5 > 0 && mythitem.Bonus5Type == mythicalProperty)
	            	{
	            			overCap += mythitem.Bonus5;
	            	}

	            	if(mythitem.Bonus6 > 0 && mythitem.Bonus6Type == mythicalProperty)
	            	{
	            			overCap += mythitem.Bonus6;
	            	}

	            	if(mythitem.Bonus7 > 0 && mythitem.Bonus7Type == mythicalProperty)
	            	{
	            			overCap += mythitem.Bonus7;
	            	}

	            	if(mythitem.Bonus8 > 0 && mythitem.Bonus8Type == mythicalProperty)
	            	{
	            			overCap += mythitem.Bonus8;
	            	}

	            	if(mythitem.Bonus9 > 0 && mythitem.Bonus9Type == mythicalProperty)
	            	{
	            			overCap += mythitem.Bonus9;
	            	}

	            	if(mythitem.Bonus10 > 0 && mythitem.Bonus10Type == mythicalProperty)
	            	{
	            			overCap += mythitem.Bonus10;
	            	}

	            	if(mythitem.ExtraBonus > 0 && mythitem.ExtraBonusType == mythicalProperty)
	            	{
	            			overCap += mythitem.ExtraBonus;
	            	}
	            	
	            	return overCap;
                }
                #endregion

        }
}
 