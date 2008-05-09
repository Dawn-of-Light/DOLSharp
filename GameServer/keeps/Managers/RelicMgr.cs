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
using System.Reflection;

using DOL.Database2;
using DOL.Events;
using DOL.GS.PacketHandler;

using log4net;

namespace DOL.GS
{
	/// <summary>
	/// RelicManager
	/// The manager that keeps track of the relics.
	/// </summary>
	public sealed class RelicMgr
	{
		/// <summary>
		/// table of all relics, id as key
		/// </summary>
		private static readonly Hashtable m_relics = new Hashtable();


		/// <summary>
		/// list of all relicPads
		/// </summary>
		private static readonly ArrayList m_relicPads = new ArrayList();


		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// load all relics from DB
		/// </summary>
		/// <returns></returns>
		public static bool Init()
		{
			lock (m_relics.SyncRoot)
			{
				//at first remove all relics
				foreach (GameRelic rel in m_relics.Values)
				{
					rel.SaveIntoDatabase();
					rel.RemoveFromWorld();
				}

				//then clear the hashtable
				m_relics.Clear();

				//then we remove all relics from the pads
				foreach (GameRelicPad pad in m_relicPads)
				{
					pad.RemoveRelic();
				}
				foreach (DBRelic datarelic in GameServer.Database.SelectObjects(typeof(DBRelic)))
				{
					if (datarelic.relicType < 0 || datarelic.relicType > 1
						|| datarelic.OriginalRealm < 1 || datarelic.OriginalRealm > 3)
					{
						log.Warn("DBRelic: Could not load " + datarelic.RelicID + ": Realm or Type missmatch.");
						continue;
					}

					if (WorldMgr.GetRegion((ushort)datarelic.Region) == null)
					{
						log.Warn("DBRelic: Could not load " + datarelic.RelicID + ": Region missmatch.");
						continue;
					}
					GameRelic relic = new GameRelic(datarelic);
					m_relics.Add(datarelic.RelicID, relic);

					relic.AddToWorld();
					GameRelicPad pad = GetPadAtRelicLocation(relic);
					if (pad != null && relic.RelicType == pad.PadType)
					{
						relic.RelicPadTakesOver(pad);
						log.Debug("DBRelic: " + relic.Name + " has been loaded and added to pad " + pad.Name + ".");
					}


				}

			}
			log.Debug(m_relicPads.Count + " relicpads" + ((m_relicPads.Count > 1) ? "s were" : " was") + " loaded.");
			log.Debug(m_relics.Count + " relic" + ((m_relics.Count > 1) ? "s were" : " was") + " loaded.");
			return true;
		}


		/// <summary>
		/// This is called when the GameRelicPads are added to world
		/// </summary>
		/// <param name="pad"></param>
		public static void AddRelicPad(GameRelicPad pad)
		{
			lock (m_relicPads.SyncRoot)
			{
				if (!m_relicPads.Contains(pad))
					m_relicPads.Add(pad);
			}
		}

		/// <summary>
		/// This is called on during the loading. It looks for relicpads and where it could be stored.
		/// </summary>
		/// <returns>null if no GameRelicPad was found at the relic's position.</returns>
		private static GameRelicPad GetPadAtRelicLocation(GameRelic relic)
		{

			lock (m_relicPads.SyncRoot)
			{
				foreach (GameRelicPad pad in m_relicPads)
				{
					if (WorldMgr.CheckDistance(pad, relic, 200))
						//if (pad.X == relic.X && pad.Y == relic.Y && pad.Z == relic.Z && pad.CurrentRegionID == relic.CurrentRegionID)
						return pad;
				}
				return null;
			}

		}


		/// <summary>
		/// get relic by ID
		/// </summary>
		/// <param name="id">id of relic</param>
		/// <returns> Relic object with relicid = id</returns>
		public static GameRelic getRelic(int id)
		{
			return m_relics[id] as GameRelic;
		}





		#region Helpers

		public static IList getNFRelics()
		{
			ArrayList myRelics = new ArrayList();
			foreach (GameRelic relic in m_relics.Values)
			{
				myRelics.Add(relic);
			}
			return myRelics;
		}

		/// <summary>
		/// Returns an enumeration with all mounted Relics of an realm
		/// </summary>
		/// <param name="Realm"></param>
		/// <returns></returns>
		public static IEnumerable getRelics(eRealm Realm)
		{
			ArrayList realmRelics = new ArrayList();
			lock (m_relics)
			{
				foreach (GameRelic relic in m_relics.Values)
				{
					if (relic.Realm == Realm && relic.IsMounted)
						realmRelics.Add(relic);
				}
			}
			return realmRelics;
		}


		/// <summary>
		/// Returns an enumeration with all mounted Relics of an realm by a specified RelicType
		/// </summary>
		/// <param name="Realm"></param>
		/// <param name="RelicType"></param>
		/// <returns></returns>
		public static IEnumerable getRelics(eRealm Realm, eRelicType RelicType)
		{
			ArrayList realmTypeRelics = new ArrayList();
			foreach (GameRelic relic in getRelics(Realm))
			{
				if (relic.RelicType == RelicType)
					realmTypeRelics.Add(relic);
			}
			return realmTypeRelics;
		}



		/// <summary>
		/// get relic count by realm
		/// </summary>
		/// <param name="realm"></param>
		/// <returns></returns>
		public static int GetRelicCount(eRealm realm)
		{
			int index = 0;
			lock (m_relics.SyncRoot)
			{
				foreach (GameRelic relic in m_relics.Values)
				{
					if ((relic.Realm == realm) && (relic is GameRelic))
						index++;
				}
			}
			return index;
		}

		/// <summary>
        /// get relic count by realm and relictype
		/// </summary>
		/// <param name="realm"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static int GetRelicCount(eRealm realm, eRelicType type)
		{
			int index = 0;
			lock (m_relics.SyncRoot)
			{
				foreach (GameRelic relic in m_relics.Values)
				{
					if ((relic.Realm == realm) && (relic.RelicType == type) && (relic is GameRelic))
						index++;
				}
			}
			return index;

		}


		/// <summary>
		/// Gets the bonus modifier for a realm/relictype.
		/// </summary>
		/// <param name="realm"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static double GetRelicBonusModifier(eRealm realm, eRelicType type)
		{
			double value = 0.0;
			//only playerrealms can get bonus
			foreach (GameRelic rel in getRelics(realm, type))
			{
				if (rel.Realm != rel.OriginalRealm)
					value += 0.1;
			}
			return value;
		}

		/// <summary>
		/// Returns if a player is allowed to pick up a mounted relic (depends if they own their own relic of the same type)
		/// </summary>
		/// <param name="player"></param>
		/// <param name="relic"></param>
		/// <returns></returns>
		public static bool CanPickupRelicFromShrine(GamePlayer player, GameRelic relic)
		{
			//debug: if (player == null || relic == null) return false;
			//their own relics can always be picked up.
			if (player.Realm == relic.OriginalRealm)
				return true;
			IEnumerable list = getRelics(player.Realm, relic.RelicType);
			foreach (GameRelic curRelic in list)
			{
				if (curRelic.Realm == curRelic.OriginalRealm)
					return true;
			}

			return false;
		}


		/// <summary>
		/// Gets a copy of the current relics table, keyvalue is the relicId
		/// </summary>
		/// <returns></returns>
		public static Hashtable GetAllRelics()
		{
			lock (m_relics.SyncRoot)
			{
				return (Hashtable)m_relics.Clone();
			}
		}
		#endregion


		[ScriptLoadedEvent]
		private static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			Init();
		}
	}
}
