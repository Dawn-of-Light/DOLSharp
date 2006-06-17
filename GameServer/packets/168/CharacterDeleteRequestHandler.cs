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
using System.Reflection;
using System.Collections;
using DOL.Database;
using DOL.GS.Database;
using DOL.Events;
using NHibernate.Expression;
using log4net;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, 0x68 ^ 168, "Handles character delete requests")]
	public class CharacterDeleteRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			string charName = packet.ReadString(30);

			GamePlayer charac = client.Account.GetCharacter(charName);
			if (charac == null)
				return 0;
			
			if (log.IsInfoEnabled)
				log.Info(String.Format("Deleting character {0}!", charName));
			//Fire the deletion event before removing the char
			GameEventMgr.Notify(DatabaseEvent.CharacterDeleted, null, new CharacterEventArgs(charac));
			
			// delete items
		/*	try // items should be automatically deleted by nhibernate
			{
				foreach (GenericItem item in charac.InventoryItems.Values)
				{
					GameServer.Database.DeleteObject(item);
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Error deleting char items, char OID="+charac.PlayerName, e);
			}*/

			// delete quests
			/*try
			{
				IList objs = GameServer.Database.SelectObjects(typeof(DBQuest), Expression.Eq("PersistantGameObjectID", charac.PersistantGameObjectID));
				foreach (DBQuest quest in objs)
				{
					GameServer.Database.DeleteObject(quest);
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Error deleting char quests, char OID="+charac.Name, e);
			}*/

			GameServer.Database.DeleteObject(charac);
			client.Account.RemoveCharacter(charac);
			client.Player = null;

			if (client.Account.CharactersCount == 0)
			{
				if (log.IsInfoEnabled)
					log.Info(string.Format("Account {0} has no more chars. Realm resetted!", client.Account.AccountName));
				//Client has no more characters, so the client can choose
				//the realm again!
				client.Account.Realm = eRealm.None;
				client.Account.UpdateDatabase();
			}
					
			return 1;
		}
	}
}