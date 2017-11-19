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

using DOL.GS;

using NUnit.Framework;

namespace DOL.Server.Tests
{
    /// <summary>
    /// Unit Test for Default Invalid Names Startup Behavior.
    /// Need Ressource InvalidNames.txt set to Default.
    /// </summary>
    [TestFixture]
	public class InvalidNamesStartupTest
	{
		public InvalidNamesStartupTest()
		{
		}
		
		[Test]
		public void InvalidNamesStartup_CheckDefaultConstraintOneString_Match()
		{
			Assert.IsTrue(GameServer.Instance.PlayerManager.InvalidNames["fuck"]);
		}
		
		[Test]
		public void InvalidNamesStartup_CheckDefaultConstraintOneString_NoMatch()
		{
			Assert.IsFalse(GameServer.Instance.PlayerManager.InvalidNames["unicorn"]);
		}
		
		[Test]
		public void InvalidNamesStartup_CheckDefaultConstraintTwoString_Match()
		{
			Assert.IsTrue(GameServer.Instance.PlayerManager.InvalidNames["fu", "ck"]);
		}
		
		[Test]
		public void InvalidNamesStartup_CheckDefaultConstraintTwoString_NoMatch()
		{
			Assert.IsFalse(GameServer.Instance.PlayerManager.InvalidNames["uni", "corn"]);
		}
	}
}
