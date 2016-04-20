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

using DOL.GS;
using DOL.Database;

using NUnit.Framework;

namespace DOL.Database.Tests
{
	/// <summary>
	/// Unit tests for the Database Custom Params
	/// </summary>
	[TestFixture]
	public class CustomParamsTest
	{
		public CustomParamsTest()
		{
		}
		
		[Test]
		public void AccountParamSaveLoadTest()
		{
			var TestAccount = new Account();
			TestAccount.Name = "NUnitTestAccount";
			TestAccount.Password = "##NUnitTestAccountPassword";
			TestAccount.Realm = 0;
			TestAccount.CreationDate = DateTime.Now;
			TestAccount.LastLogin = DateTime.Now;
			TestAccount.LastLoginIP = "";
			TestAccount.LastClientVersion = "1109";
			TestAccount.Language = "EN";
			TestAccount.PrivLevel = 1;
			TestAccount.CustomParams = new [] { new AccountXCustomParam(TestAccount.Name, "TestParam", Convert.ToString(true)) };
			
			bool inserted = GameServer.Database.SaveObject(TestAccount);
			
			Assert.IsTrue(inserted, "Test Account not inserted properly in Database !");
			
			var RetrieveAccount = GameServer.Database.FindObjectByKey<Account>(TestAccount.Name);
			
			Assert.AreEqual(TestAccount.CustomParams.Length,
			                RetrieveAccount.CustomParams.Length,
			                "Saved Account and Retrieved Account doesn't have the same amount of Custom Params");
		}
	}
}
