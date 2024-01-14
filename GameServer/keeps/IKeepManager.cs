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
using DOL.Database;
using DOL.GS.Geometry;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Keeps
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class KeepManagerAttribute : Attribute
	{
		/// <summary>
		/// An attribute to identify a keep manager
		/// If one is defined then this will be used as the GameServer KeepManager
		/// </summary>
		public KeepManagerAttribute()
		{
		}
	}

	/// <summary>
	/// Interface for a Keep Manager
	/// To make your own inherit from this interface and implement all methods.  You can also inherit from
	/// DefaultKeepManager and only override what you want to change.  In order to use your keep manager you must also add
	/// the KeepManagerAttribute above ->  [KeepManager]
	/// </summary>
	public interface IKeepManager
	{
		log4net.ILog Log { get; }
		Hashtable Keeps { get; }
		List<uint> FrontierRegionsList { get; }

		bool Load();
		bool IsNewKeepComponent(int skin);
		void RegisterKeep(int keepID, AbstractGameKeep keep);
		AbstractGameKeep GetKeepByID(int id);
        [Obsolete("This is going to be removed.")]
		IEnumerable GetKeepsCloseToSpot(ushort regionid, IPoint3D point3d, int radius);
        [Obsolete("This is going to be removed.")]
		ICollection<AbstractGameKeep> GetKeepsCloseToSpot(ushort regionid, int x, int y, int z, int radius);
        [Obsolete("Use .GetKeepCloseToSpot(Position, int) instead!")]
		AbstractGameKeep GetKeepCloseToSpot(ushort regionid, IPoint3D point3d, int radius);
        [Obsolete("Use .GetKeepCloseToSpot(Position, int) instead!")]
		AbstractGameKeep GetKeepCloseToSpot(ushort regionid, int x, int y, int z, int radius);
        AbstractGameKeep GetKeepCloseToSpot(Position position, int radius);
		ICollection<IGameKeep> GetKeepsByRealmMap(int map);
		AbstractGameKeep GetBGPK(GamePlayer player);
		ICollection<AbstractGameKeep> GetFrontierKeeps();
		ICollection<AbstractGameKeep> GetKeepsOfRegion(ushort region);
		int GetTowerCountByRealm(eRealm realm);
		Dictionary<eRealm, int> GetTowerCountAllRealm();
		Dictionary<eRealm, int> GetTowerCountFromZones(List<int> zones);
		int GetKeepCountByRealm(eRealm realm);
		ICollection<AbstractGameKeep> GetAllKeeps();
		bool IsEnemy(AbstractGameKeep keep, GamePlayer target, bool checkGroup);
		bool IsEnemy(AbstractGameKeep keep, GamePlayer target);
		bool IsEnemy(GameKeepGuard checker, GamePlayer target);
		bool IsEnemy(GameKeepGuard checker, GamePlayer target, bool checkGroup);
		bool IsEnemy(GameKeepDoor checker, GamePlayer target);
		bool IsEnemy(GameKeepComponent checker, GamePlayer target);
		byte GetHeightFromLevel(byte level);
		Position GetBorderKeepPosition(int keepid);
        [Obsolete("Use GetBorderKeepPosition(int) instead!")]
		void GetBorderKeepLocation(int keepid, out int x, out int y, out int z, out ushort heading);
		int GetRealmKeepBonusLevel(eRealm realm);
		int GetRealmTowerBonusLevel(eRealm realm);
		void UpdateBaseLevels();
		Battleground GetBattleground(ushort region);
		void ExitBattleground(GamePlayer player);
	}
}
