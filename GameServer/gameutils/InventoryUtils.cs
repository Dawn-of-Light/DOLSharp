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

using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// Inventory Oriented Extension Class
	/// </summary>
	public static class InventoryUtils
	{
		#region bonuses
		/// <summary>
		/// Class for Holding Items Bonuses Property/Value data
		/// </summary>
		public class PropertyValue
		{
			public eProperty Eproperty
			{
				get	{ try { return (eProperty)Property; } catch { return eProperty.Undefined; } }
				set { Property = (int)value; }
			}
			public int Property { get; set; }
			public int Value { get; set; }
			
			public PropertyValue(int property, int value)
			{
				Property = property;
				Value = value;
			}
		}
		
		/// <summary>
		/// Helper to set Item Bonuses Types and Values
		/// </summary>
		/// <param name="item"></param>
		/// <param name="properties"></param>
		/// <param name="amounts"></param>
		public static void SetTemplateBonuses(this ItemTemplate item, eProperty[] properties, int[] amounts)
		{
			var length = Math.Min(properties.Length, amounts.Length);
			
			if (length > 0)
				item.SetBonus(Enumerable.Range(1, length).ToArray(), properties.Cast<int>().ToArray(), amounts);
		}
		
		/// <summary>
		/// Helper to set FIRST Item Bonus Type and Value
		/// </summary>
		/// <param name="item"></param>
		/// <param name="property"></param>
		/// <param name="amount"></param>
		public static void SetTemplateBonuses(this ItemTemplate item, eProperty property, int amount)
		{
			item.SetBonus(1, (int)property, amount);
		}
		
		/// <summary>
		/// Helper to set Last Item Bonus Type and Value
		/// </summary>
		/// <param name="item"></param>
		/// <param name="property"></param>
		/// <param name="amount"></param>
		public static void SetLastTemplateBonus(this ItemTemplate item, eProperty property, int amount)
		{
			item.SetBonus(item.Bonuses.Count, (int)property, amount);
		}
		
		/// <summary>
		/// Helper to set Given Index Item Bonus Type and Value
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		/// <param name="property"></param>
		/// <param name="amount"></param>
		public static void SetTemplateBonuses(this ItemTemplate item, int index, eProperty property, int amount)
		{
			item.SetBonus(index, (int)property, amount);
		}
		
		/// <summary>
		/// Helper to Clear Item Bonuses Type and Values
		/// </summary>
		/// <param name="item"></param>
		public static void ClearTemplateBonuses(this ItemTemplate item)
		{
			foreach (var index in item.Bonuses.Keys)
				item.SetBonus(index, (int)eProperty.Undefined, 0);
		}

		/// <summary>
		/// Helper to set Inventory Item Bonuses Types and Values
		/// </summary>
		/// <param name="item"></param>
		/// <param name="properties"></param>
		/// <param name="amounts"></param>
		public static void SetTemplateBonuses(this InventoryItem item, eProperty[] properties, int[] amounts)
		{
			item.Template.SetTemplateBonuses(properties, amounts);
		}
		
		/// <summary>
		/// Helper to set FIRST Inventory Item Bonus Type and Value
		/// </summary>
		/// <param name="item"></param>
		/// <param name="property"></param>
		/// <param name="amount"></param>
		public static void SetTemplateBonuses(this InventoryItem item, eProperty property, int amount)
		{
			item.Template.SetTemplateBonuses(property, amount);
		}
		
		/// <summary>
		/// Helper to set Last Inventory Item Bonus Type and Value
		/// </summary>
		/// <param name="item"></param>
		/// <param name="property"></param>
		/// <param name="amount"></param>
		public static void SetLastTemplateBonus(this InventoryItem item, eProperty property, int amount)
		{
			item.Template.SetLastTemplateBonus(property, amount);
		}

		/// <summary>
		/// Helper to set Given Index Inventory Item Bonus Type and Value
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		/// <param name="property"></param>
		/// <param name="amount"></param>
		public static void SetTemplateBonuses(this InventoryItem item, int index, eProperty property, int amount)
		{
			item.Template.SetTemplateBonuses(index, property, amount);
		}
		
		/// <summary>
		/// Helper to Clear Inventory Item Bonuses Type and Values
		/// </summary>
		/// <param name="item"></param>
		public static void ClearTemplateBonuses(this InventoryItem item)
		{
			item.Template.ClearTemplateBonuses();
		}
		
		/// <summary>
		/// Helper to iterate Inventory Item Bonuses and get Indexed Property/Value
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<int, PropertyValue>> GetIndexedTemplateBonuses(this InventoryItem item)
		{
			return item.Template.GetIndexedTemplateBonuses();
		}
		
		/// <summary>
		/// Helper to iterate Item Template Bonuses and get Indexed Property/Value
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<int, PropertyValue>> GetIndexedTemplateBonuses(this ItemTemplate item)
		{
			return item.Bonuses.Select(kv => new KeyValuePair<int, PropertyValue>(kv.Key, new PropertyValue(kv.Value.Item1, kv.Value.Item2)));
		}
		
		/// <summary>
		/// Helper to iterate Inventory Item Bonuses and get Property/Value
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IEnumerable<PropertyValue> GetTemplateBonuses(this InventoryItem item)
		{
			return item.Template.GetTemplateBonuses();
		}
		
		/// <summary>
		/// Helper to iterate Item Template Bonuses and get Property/Value
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IEnumerable<PropertyValue> GetTemplateBonuses(this ItemTemplate item)
		{
			return item.Bonuses.Select(kv => new PropertyValue(kv.Value.Item1, kv.Value.Item2));
		}
		
		/// <summary>
		/// Helper to get first Inventory Item Bonuses
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static PropertyValue GetTemplateFirstBonus(this InventoryItem item)
		{
			return item.Template.GetTemplateFirstBonus();
		}
		
		/// <summary>
		/// Helper to get first Item Template Bonuses
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static PropertyValue GetTemplateFirstBonus(this ItemTemplate item)
		{
			return item.GetTemplateBonuses().FirstOrDefault();
		}
		
		/// <summary>
		/// Helper to get Last Inventory Item Bonuses
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static PropertyValue GetTemplateLastBonus(this InventoryItem item)
		{
			return item.Template.GetTemplateLastBonus();
		}
		
		/// <summary>
		/// Helper to get Last Item Template Bonuses
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static PropertyValue GetTemplateLastBonus(this ItemTemplate item)
		{
			return item.GetTemplateBonuses().LastOrDefault();
		}
		
		/// <summary>
		/// Helper to iterate Inventory Item Bonuses and get Property/Value at given index
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static int GetTemplateBonusesValueAt(this InventoryItem item, int index)
		{
			return item.Template.GetTemplateBonusesValueAt(index);
		}
		
		/// <summary>
		/// Helper to iterate Item Template Bonuses and get Property/Value at given index
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static int GetTemplateBonusesValueAt(this ItemTemplate item, int index)
		{
			return item.Bonuses.FirstOrDefault(kv => kv.Key == index).Value.Item2;
		}
		#endregion
		
		#region spells
		/// <summary>
		/// Object Describing an Item Spell with /use 
		/// </summary>
		public class UseSpellValue
		{
			/// <summary>
			/// Does this Item Spell have a Charge Count ?
			/// </summary>
			bool HasCurrentChargeCount { get; set; }
			/// <summary>
			/// Item SpellID
			/// </summary>
			public int SpellID { get; set; }
			/// <summary>
			///  Item MaxCharges
			/// </summary>
			public int MaxCharges { get; set; }
			/// <summary>
			/// Item Current Charge Count
			/// </summary>
			public int Charges { get; set; }
			/// <summary>
			/// Does this item have any Charges to be Fired ?
			/// </summary>
			public bool HasCharges { get { return HasCurrentChargeCount && (MaxCharges == 0 || Charges > 1); } }
			/// <summary>
			/// Does this item is Fully Charged ?
			/// </summary>
			public bool isFullyCharged { get { return !HasCurrentChargeCount || MaxCharges == 0 || Charges == MaxCharges; } }
			
			/// <summary>
			/// Constructor without Charges Amount
			/// </summary>
			/// <param name="spellid"></param>
			/// <param name="maxCharges"></param>
			public UseSpellValue(int spellid, int maxCharges)
			{
				SpellID = spellid;
				MaxCharges = maxCharges;
				HasCurrentChargeCount = false;
			}
			
			/// <summary>
			/// Constructor with Charges Amount
			/// </summary>
			/// <param name="spellid"></param>
			/// <param name="maxCharges"></param>
			/// <param name="amount"></param>
			public UseSpellValue(int spellid, int maxCharges, int amount)
				:this(spellid, maxCharges)
			{
				Charges = amount;
				HasCurrentChargeCount = true;
			}
		}
		
		/// <summary>
		/// Check this Item reuse Timer against Given Hard Timer
		/// </summary>
		/// <param name="item"></param>
		/// <param name="useItemNextAvailable"></param>
		/// <returns>0 if ready to fire or Waiting Time</returns>
		public static int TemplateUseSpellCanUseAgainIn(this InventoryItem item, int useItemNextAvailable)
		{
			if (useItemNextAvailable > 0)
				return Math.Max(useItemNextAvailable, item.CanUseAgainIn);
			
			return item.CanUseAgainIn;
		}
		
		/// <summary>
		/// Set This Item Template First Use Spell
		/// </summary>
		/// <param name="item"></param>
		/// <param name="spellID"></param>
		/// <param name="maxCharge"></param>
		public static void SetTemplateUseSpells(this ItemTemplate item, int spellID, int maxCharge)
		{
			item.SetUseSpells(1, spellID, maxCharge);
		}
		
		/// <summary>
		/// Set This Inventory Item Template First Use Spell
		/// </summary>
		/// <param name="item"></param>
		/// <param name="spellID"></param>
		/// <param name="maxCharge"></param>
		public static void SetTemplateUseSpells(this InventoryItem item, int spellID, int maxCharge)
		{
			item.Template.SetTemplateUseSpells(spellID, maxCharge);
		}
		
		/// <summary>
		/// Set This Inventory Item Template First Use Spell with charges Amount
		/// </summary>
		/// <param name="item"></param>
		/// <param name="spellID"></param>
		/// <param name="maxCharge"></param>
		/// <param name="currentCharge"></param>
		public static void SetTemplateUseSpells(this InventoryItem item, int spellID, int maxCharge, int currentCharge)
		{
			item.SetTemplateUseSpells( new[]{ spellID }, new[]{ maxCharge }, new[]{ currentCharge });
		}
		
		/// <summary>
		/// Set This Item Template All Use Spell
		/// </summary>
		/// <param name="item"></param>
		/// <param name="spellIDs"></param>
		/// <param name="maxCharges"></param>
		public static void SetTemplateUseSpells(this ItemTemplate item, int[] spellIDs, int[] maxCharges)
		{
			var length = Math.Min(spellIDs.Length, maxCharges.Length);
			if (length > 0)
				item.SetUseSpells(Enumerable.Range(1, length).ToArray(), spellIDs, maxCharges);
		}
		
		/// <summary>
		/// Set This Inventory Item Template All Use Spell
		/// </summary>
		/// <param name="item"></param>
		/// <param name="spellIDs"></param>
		/// <param name="maxCharges"></param>
		public static void SetTemplateUseSpells(this InventoryItem item, int[] spellIDs, int[] maxCharges)
		{
			item.Template.SetTemplateUseSpells(spellIDs, maxCharges);
		}
		
		/// <summary>
		/// Set This Inventory Item Template All Use Spell with current charges amount
		/// </summary>
		/// <param name="item"></param>
		/// <param name="spellIDs"></param>
		/// <param name="maxCharges"></param>
		/// <param name="currentCharges"></param>
		public static void SetTemplateUseSpells(this InventoryItem item, int[] spellIDs, int[] maxCharges, int[] currentCharges)
		{
			var length = new [] { spellIDs.Length, maxCharges.Length, currentCharges.Length }.Min();
			
			if (length > 0)
			{
				var indexes = Enumerable.Range(1, length).ToArray();
				item.Template.SetUseSpells(indexes, spellIDs, maxCharges);
				item.SetChargesAmount(indexes, currentCharges);
			}
		}
		
		/// <summary>
		/// Reset Given Inventory Item Charges to MaxCharges
		/// </summary>
		/// <param name="item"></param>
		public static void SetTemplateUseSpellsMaxCharges(this InventoryItem item)
		{
			var useSpells = item.Template.UseSpells;
			item.SetChargesAmount(useSpells.Select(kv => kv.Key).ToArray(), useSpells.Select(kv => kv.Value.Item2).ToArray());
		}
		
		/// <summary>
		/// Reset Given Inventory Item Charges to MaxCharges
		/// </summary>
		/// <param name="item"></param>
		/// <param name="spell">Spell with ID To Match</param>
		/// <param name="increment">Amount to Add (Remove if Negative)</param>
		public static void ChangeTemplateUseSpellCharges(this InventoryItem item, Spell spell, int increment)
		{
			var index = item.GetIndexedTemplateUseSpells().FirstOrDefault(sp => sp.Value.SpellID == spell.ID && sp.Value.HasCharges);
			item.SetChargesAmount(index.Key, index.Value.Charges + increment);
		}
		
		/// <summary>
		/// Get All Item Template Use Spell with Index
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<int, UseSpellValue>> GetIndexedTemplateUseSpells(this ItemTemplate item)
		{
			return item.UseSpells.Select(kv => new KeyValuePair<int, UseSpellValue>(kv.Key, new UseSpellValue(kv.Value.Item1, kv.Value.Item2)));
		}
				
		/// <summary>
		/// Get All Inventory Item Template Use Spell with Index
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<int, UseSpellValue>> GetIndexedTemplateUseSpells(this InventoryItem item)
		{
			return item.UseSpells.Select(kv => new KeyValuePair<int, UseSpellValue>(kv.Key, new UseSpellValue(kv.Value.Item1, kv.Value.Item2, kv.Value.Item3)));
		}
		
		/// <summary>
		/// Get All Item Template Use Spell
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IEnumerable<UseSpellValue> GetTemplateUseSpells(this ItemTemplate item)
		{
			return item.UseSpells.Select(kv => new UseSpellValue(kv.Value.Item1, kv.Value.Item2));
		}
		
		/// <summary>
		/// Get All Item Template Use Spell
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static UseSpellValue GetTemplateFirstUseSpell(this ItemTemplate item)
		{
			return item.GetTemplateUseSpells().FirstOrDefault();
		}
		
		/// <summary>
		/// Get All Inventory Item Template Use Spell
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IEnumerable<UseSpellValue> GetTemplateUseSpells(this InventoryItem item)
		{
			return item.UseSpells.Select(kv => new UseSpellValue(kv.Value.Item1, kv.Value.Item2, kv.Value.Item3));
		}
		
		/// <summary>
		/// Get First Inventory Item Template Use Spell
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static UseSpellValue GetTemplateFirstUseSpell(this InventoryItem item)
		{
			return item.GetTemplateUseSpells().FirstOrDefault();
		}
		
		/// <summary>
		/// Item Proc Spell Object 
		/// </summary>
		public class SpellProcValue
		{
			/// <summary>
			/// Proc SpellID
			/// </summary>
			public int SpellID { get; set; }
			
			/// <summary>
			/// Proc Chance
			/// </summary>
			public byte ProcChance { get; set; }
			
			/// <summary>
			/// New Proc Spell Value with Spell Id and Chance
			/// </summary>
			/// <param name="spellid"></param>
			/// <param name="chance"></param>
			public SpellProcValue(int spellid, byte chance)
			{
				SpellID = spellid;
				ProcChance = chance;
			}
		}

		/// <summary>
		/// Get this Item Template All Proc Spells Indexed
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<int, SpellProcValue>> GetIndexedTemplateProcSpells(this ItemTemplate item)
		{
			return item.ProcSpells.Select(kv => new KeyValuePair<int, SpellProcValue>(kv.Key, new SpellProcValue(kv.Value.Item1, kv.Value.Item2)));
		}
				
		/// <summary>
		/// Get this Inventory Item Template All Proc Spells Indexed
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<int, SpellProcValue>> GetIndexedTemplateProcSpells(this InventoryItem item)
		{
			return item.Template.GetIndexedTemplateProcSpells();
		}
		
		/// <summary>
		/// Get this Item Template All Proc Spells
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IEnumerable<SpellProcValue> GetTemplateProcSpells(this ItemTemplate item)
		{
			return item.ProcSpells.Select(kv => new SpellProcValue(kv.Value.Item1, kv.Value.Item2));
		}
		
		/// <summary>
		/// Get this Item Template All Proc Spells Indexed
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static SpellProcValue GetTemplateFirstProcSpell(this ItemTemplate item)
		{
			return item.GetTemplateProcSpells().FirstOrDefault();
		}
		
		/// <summary>
		/// Get this Inventory Item Template All Proc Spells Indexed
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IEnumerable<SpellProcValue> GetTemplateProcSpells(this InventoryItem item)
		{
			return item.Template.GetTemplateProcSpells();
		}
		
		/// <summary>
		/// Get this Inventory Item Template All Proc Spells Indexed
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static SpellProcValue GetTemplateFirstProcSpell(this InventoryItem item)
		{
			return item.GetTemplateProcSpells().FirstOrDefault();
		}
		
		
		/// <summary>
		/// Set this Item Template First Proc Spell
		/// </summary>
		/// <param name="item"></param>
		/// <param name="spellID"></param>
		/// <param name="procChance"></param>
		public static void SetTemplateProcSpells(this ItemTemplate item, int spellID, byte procChance)
		{
			item.SetProcSpells(1, spellID, procChance);
		}
		
		/// <summary>
		/// Set this Inventory Item Template First Proc Spell
		/// </summary>
		/// <param name="item"></param>
		/// <param name="spellID"></param>
		/// <param name="procChance"></param>
		public static void SetTemplateProcSpells(this InventoryItem item, int spellID, byte procChance)
		{
			item.Template.SetTemplateProcSpells(spellID, procChance);
		}
		
		/// <summary>
		/// Set this Item Template All Proc Spells
		/// </summary>
		/// <param name="item"></param>
		/// <param name="spellIDs"></param>
		/// <param name="procChances"></param>
		public static void SetTemplateProcSpells(this ItemTemplate item, int[] spellIDs, byte[] procChances)
		{
			var length = Math.Min(spellIDs.Length, procChances.Length);
			if (length > 0)
				item.SetProcSpells(Enumerable.Range(1, length).ToArray(), spellIDs, procChances);
		}
		
		/// <summary>
		/// Set this Inventory Item Template All Proc Spells
		/// </summary>
		/// <param name="item"></param>
		/// <param name="spellIDs"></param>
		/// <param name="procChances"></param>
		public static void SetTemplateProcSpells(this InventoryItem item, int[] spellIDs, byte[] procChances)
		{
			item.Template.SetTemplateProcSpells(spellIDs, procChances);
		}
		#endregion

	}
	
}
