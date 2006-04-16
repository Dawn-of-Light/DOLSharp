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
using DOL;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Database;
using DOL.Tests;
using NHibernate.Expression;
using NUnit.Framework;

namespace DOL.GS.Quests.Tests
{
	/// <summary>
	/// A simple test case to see how item work.
	/// </summary>
	[TestFixture]
	public class ItemTemplateTest : DOLTestCase
	{
		public ItemTemplateTest()
		{			
		}		

		[Test] public void TestItemTemplate()
		{
			RingTemplate template = new RingTemplate();
			template.ItemTemplateID = "waist_template_test";
			template.Name = "MyFirstWaistTemplate";
			template.Level = 10;
			template.Weight = 20;
			template.Realm = eRealm.Hibernia;
			template.IsDropable = true;
			template.IsTradable = false;
			template.IsSaleable = false;

			template.Quality = 100;
			template.Durability = 100;
			template.Condition = 100;
			template.Bonus = 20;
			template.MaterialLevel = eMaterialLevel.Bronze;
			template.ProcEffectType = eMagicalEffectType.NoEffect;
			template.ProcSpellID = 0;
			template.ChargeSpellID = 0;
			template.ChargeEffectType = eMagicalEffectType.NoEffect;
			template.Charge = 0;
			template.MaxCharge = 0;

			template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.ArmorFactor , 20));
			Iesi.Collections.ISet set = template.MagicalBonus;
			template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.ArcheryRange , 10));
			template.AllowedClass.Add(eCharacterClass.Acolyte);
			template.AllowedClass.Add(eCharacterClass.Bainshee);
			
			GameServer.Database.AddNewObject(template);
			
			Ring myWaist = (Ring)template.CreateInstance();
			myWaist.QuestName = "";
			myWaist.CrafterName = "";
			myWaist.SlotPosition = 10;
			GameServer.Database.AddNewObject(myWaist);

			RingTemplate temp = (RingTemplate)GameServer.Database.SelectObject(typeof(RingTemplate),Expression.Eq("ItemTemplateID","waist_template_test"));
			if(temp != null)
			{
				System.Console.WriteLine("Templ Trouvé");
				if(temp.AllowedClass.Contains(eCharacterClass.Bainshee)) System.Console.WriteLine("Bainshee Trouve");
				foreach(ItemMagicalBonus bonus in temp.MagicalBonus)
				{
					System.Console.WriteLine("Bonus pro : "+bonus.BonusType+" => "+bonus.Bonus);
				}
			}

			Ring inst = (Ring)GameServer.Database.SelectObject(typeof(Ring),Expression.Eq("Name","MyFirstWaistTemplate"));
			if(inst != null)
			{
				System.Console.WriteLine("Inst Trouvé");
				if(inst.AllowedClass.Contains(eCharacterClass.Bainshee)) System.Console.WriteLine("Bainshee Trouve");
				foreach(ItemMagicalBonus bonus in inst.MagicalBonus)
				{
					System.Console.WriteLine("Bonus pro : "+bonus.BonusType+" => "+bonus.Bonus);
				}
			}
		}
	}
}