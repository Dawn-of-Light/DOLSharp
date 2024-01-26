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

namespace DOL.Integration.Managers
{
	/// <summary>
	/// Unit Tests for Invalid Names Manager.
	/// </summary>
	[TestFixture]
	public class TestInvalidNamesManager
	{
		public TestInvalidNamesManager()
		{
		}

		[Test]
		public void InvalidNamesManager_LoadContainsAndCheckOneString_Match()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "Bob" } );
			
			Assert.That(manager["bob"], Is.True, "'bob' should match 'Bob' Pattern with case insensitive...");
			Assert.That(manager["boba fett"], Is.True, "'boba fett' should match 'Bob' Pattern with case insensitive...");
			Assert.That(manager["FOOBobBAR"], Is.True, "'FOOBobBAR' should match 'Bob' Pattern with case insensitive...");
		}
		
		[Test]
		public void InvalidNamesManager_LoadContainsAndCheckOneString_NoMatch()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "Bob" } );
			
			Assert.That(manager["john"], Is.False, "'john' should not match 'Bob' Pattern...");
			Assert.That(manager["han solo"], Is.False, "'han solo' not should match 'Bob' Pattern...");
			Assert.That(manager["FOOBAR"], Is.False, "'FOOBAR' should not match 'Bob' Pattern...");
		}
		
		[Test]
		public void InvalidNamesManager_LoadContainsAndCheckTwoString_Match()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "Bob" } );
			
			Assert.That(manager["bo", "b"], Is.True, "'bo' 'b' should match 'Bob' Pattern with case insensitive...");
			Assert.That(manager["boba", "fett"], Is.True, "'boba' 'fett' should match 'Bob' Pattern with case insensitive...");
			Assert.That(manager["FOOB", "obBAR"], Is.True, "'FOOB' 'obBAR' should match 'Bob' Pattern with case insensitive...");
		}
		
		[Test]
		public void InvalidNamesManager_LoadContainsAndCheckTwoString_NoMatch()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "Bob" } );
			
			Assert.That(manager["jo", "hn"], Is.False, "'jo' 'hn' should not match 'Bob' Pattern...");
			Assert.That(manager["han", "solo"], Is.False, "'han' 'solo' not should match 'Bob' Pattern...");
			Assert.That(manager["FO", "OBAR"], Is.False, "'FO' 'OBAR' should not match 'Bob' Pattern...");
		}
		
		[Test]
		public void InvalidNamesManager_LoadRegexAndCheckOneString_Match()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "/B.*o.*b/" } );
			
			Assert.That(manager["bob"], Is.True, "'bob' should match '/B.*o.*b/' Pattern with case insensitive...");
			Assert.That(manager["boba fett"], Is.True, "'boba fett' should match '/B.*o.*b/' Pattern with case insensitive...");
			Assert.That(manager["FOOBobBAR"], Is.True, "'FOOBobBAR' should match '/B.*o.*b/' Pattern with case insensitive...");
			Assert.That(manager["F O O B o b B A R"], Is.True, "'F O O B o b B A R' should match '/B.*o.*b/' Pattern with case insensitive...");
		}
		
		[Test]
		public void InvalidNamesManager_LoadRegexAndCheckOneString_NoMatch()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "/B.*o.*b/" } );
			
			Assert.That(manager["john"], Is.False, "'john' should not match '/B.*o.*b/' Pattern...");
			Assert.That(manager["han solo"], Is.False, "'han solo' not should match '/B.*o.*b/' Pattern...");
			Assert.That(manager["FOOBAR"], Is.False, "'FOOBAR' should not match '/B.*o.*b/' Pattern...");
			Assert.That(manager["F O O B A R"], Is.False, "'F O O B A R' should not match '/B.*o.*b/' Pattern...");
		}
		
		[Test]
		public void InvalidNamesManager_LoadRegexAndCheckTwoString_Match()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "/B.*o.*b/" } );
			
			Assert.That(manager["bo", "b"], Is.True, "'bo' 'b' should match '/B.*o.*b/' Pattern with case insensitive...");
			Assert.That(manager["boba", "fett"], Is.True, "'boba' 'fett' should match '/B.*o.*b/' Pattern with case insensitive...");
			Assert.That(manager["FOOB", "obBAR"], Is.True, "'FOOB' 'obBAR' should match '/B.*o.*b/' Pattern with case insensitive...");
			Assert.That(manager["F O O", " B o b B A R"], Is.True, "'F O O' ' B o b B A R' should match '/B.*o.*b/' Pattern with case insensitive...");
		}
		
		[Test]
		public void InvalidNamesManager_LoadRegexAndCheckTwoString_NoMatch()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "/B.*o.*b/" } );
			
			Assert.That(manager["jo", "hn"], Is.False, "'jo' 'hn' should not match '/B.*o.*b/' Pattern...");
			Assert.That(manager["han", "solo"], Is.False, "'han' 'solo' not should match '/B.*o.*b/' Pattern...");
			Assert.That(manager["FOOB", "AR"], Is.False, "'FOOB' 'AR' should not match '/B.*o.*b/' Pattern...");
			Assert.That(manager["F O O", " B A R"], Is.False, "'F O O' ' B A R' should not match '/B.*o.*b/' Pattern...");
		}
		
		[Test]
		public void InvalidNamesManager_LoadMultiConstraintsCheckOneString_Match()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "/B.*o.*b/", "/B.*a.*r/", "FOO", "Luke" } );
			
			Assert.That(manager["bar"], Is.True, "'bar' should match '/B.*a.*r/' Pattern with case insensitive...");
			Assert.That(manager["boba fett"], Is.True, "'boba fett' should match '/B.*o.*b/' Pattern with case insensitive...");
			Assert.That(manager["FOOBobBAR"], Is.True, "'FOOBobBAR' should match 'FOO' Pattern with case insensitive...");
			Assert.That(manager["LukeSkywalker"], Is.True, "'LukeSkywalker' should match 'Luke' Pattern with case insensitive...");
		}
		
		[Test]
		public void InvalidNamesManager_LoadMultiConstraintsCheckOneString_NoMatch()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "/B.*o.*b/", "/B.*a.*r/", "FOO", "Luke" } );
			
			Assert.That(manager["john"], Is.False, "'john' should not match any Pattern...");
			Assert.That(manager["han solo"], Is.False, "'han solo' should not match any Pattern...");
			Assert.That(manager["FARBABBoo"], Is.False, "'FARBABBoo' should not match any Pattern...");
			Assert.That(manager["AnakinSkywalker"], Is.False, "'AnakinSkywalker' should not match any Pattern...");
		}
		
		[Test]
		public void InvalidNamesManager_LoadMultiConstraintsCheckTwoString_Match()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "/B.*o.*b/", "/B.*a.*r/", "FOO", "Luke" } );
			
			Assert.That(manager["ba", "r"], Is.True, "'ba' 'r' should match '/B.*a.*r/' Pattern with case insensitive...");
			Assert.That(manager["boba", "fett"], Is.True, "'boba' 'fett' should match '/B.*o.*b/' Pattern with case insensitive...");
			Assert.That(manager["FOOB", "obBAR"], Is.True, "'FOOB' 'obBAR' should match 'FOO' Pattern with case insensitive...");
			Assert.That(manager["LukeSky", " walker"], Is.True, "'LukeSky' ' walker' should match 'Luke' Pattern with case insensitive...");
		}
		
		[Test]
		public void InvalidNamesManager_LoadMultiConstraintsCheckTwoString_NoMatch()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "/B.*o.*b/", "/B.*a.*r/", "FOO", "Luke" } );
			
			Assert.That(manager["jo", "hn"], Is.False, "'jo' 'hn' should not match any Pattern...");
			Assert.That(manager["han", "solo"], Is.False, "'han' 'solo' should not match any Pattern...");
			Assert.That(manager["FARB", "ABBoo"], Is.False, "'FARB' 'ABBoo' should not match any Pattern...");
			Assert.That(manager["AnakinSky", " walker"], Is.False, "'AnakinSky' ' walker' should not match any Pattern...");
		}
		
		[Test]
		public void InvalidNamesManager_LoadCommentCheckOneString_NoMatch()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "BA #R", "#Luke", "bob" } );
			
			Assert.That(manager["Luke"], Is.False, "'Luke' should not match any Pattern...");
			Assert.That(manager["B AR"], Is.False, "'B AR' should not match any Pattern...");
			Assert.That(manager["han solo"], Is.False, "'han solo' should not match any Pattern...");
		}
		
		[Test]
		public void InvalidNamesManager_LoadCommentCheckOneString_Match()
		{
			var manager = new InvalidNamesManager(string.Empty);
			manager.LoadFromLines(new [] { "BA #R", "#Luke", "bob" } );
			
			Assert.That(manager["Bob"], Is.True, "'Bob' should match 'Bob' Pattern...");
			Assert.That(manager["BA F"], Is.True, "'BA F' should match 'BA' Pattern...");
			Assert.That(manager["bobaFett"], Is.True, "'boba' should match 'BA' Pattern...");
		}
	}
}
