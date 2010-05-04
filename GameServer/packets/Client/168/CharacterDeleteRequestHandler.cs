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
using DOL.Database;
using DOL.Events;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, 0x68 ^ 168, "Handles character delete requests")]
	public class CharacterDeleteRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			string charName = packet.ReadString(30);
			Character[] chars = client.Account.Characters;
			if (chars == null)
				return 0;
			for (int i = 0; i < chars.Length; i++)
				if (chars[i].Name.ToLower().Equals(charName.ToLower()))
				{
					if (client.ActiveCharIndex == i)
						client.ActiveCharIndex = -1;

					if (Log.IsInfoEnabled)
						Log.Info(String.Format("IP {1} is Deleting character {0} from account {2}!", charName, client.TcpEndpoint, client.Account.Name));
					//Fire the deletion event before removing the char
					GameEventMgr.Notify(DatabaseEvent.CharacterDeleted, null, new CharacterEventArgs(chars[i], client));
					//EventMgr.FireCharacterDeletion(chars[i]);

					// delete items
					try
					{
						var objs = GameServer.Database.SelectObjects<InventoryItem>("OwnerID = '" + GameServer.Database.Escape(chars[i].ObjectId) + "'");
						foreach (InventoryItem item in objs)
						{
							GameServer.Database.DeleteObject(item);
						}
						// 2008-01-29 Kakuri - Obsolete
						//GameServer.Database.WriteDatabaseTable( typeof( InventoryItem ) );
					}
					catch (Exception e)
					{
						if (Log.IsErrorEnabled)
							Log.Error("Error deleting char items, char OID="+chars[i].ObjectId, e);
					}

					// delete quests
					try
					{
						var objs = GameServer.Database.SelectObjects<DBQuest>("Character_ID = '" + GameServer.Database.Escape(chars[i].ObjectId) + "'");
						foreach (DBQuest quest in objs)
						{
							GameServer.Database.DeleteObject(quest);
						}
						// 2008-01-29 Kakuri - Obsolete
						//GameServer.Database.WriteDatabaseTable( typeof( DBQuest ) );
					}
					catch (Exception e)
					{
						if (Log.IsErrorEnabled)
							Log.Error("Error deleting char quests, char OID="+chars[i].ObjectId, e);
					}
					
					// delete ML steps
					try
					{
						var objs = GameServer.Database.SelectObjects<DBCharacterXMasterLevel>("Character_ID = '" + GameServer.Database.Escape(chars[i].ObjectId) + "'");
						foreach (DBCharacterXMasterLevel mlstep in objs)
						{
							GameServer.Database.DeleteObject(mlstep);
						}
						// 2008-01-29 Kakuri - Obsolete
						//GameServer.Database.WriteDatabaseTable( typeof( DBCharacterXMasterLevel ) );
					}
					catch (Exception e)
					{
						if (Log.IsErrorEnabled)
							Log.Error("Error deleting char ml steps, char OID="+chars[i].ObjectId, e);
					}					

					GameServer.Database.DeleteObject(chars[i]);
					// 2008-01-29 Kakuri - Obsolete
					//GameServer.Database.WriteDatabaseTable( typeof( Character ) );
					client.Account.Characters = null;
					GameServer.Database.FillObjectRelations(client.Account);
					client.Player = null;

					if (client.Account.Characters == null || client.Account.Characters.Length == 0)
					{
						if (Log.IsInfoEnabled)
							Log.Info(string.Format("Account {0} has no more chars. Realm reset!", client.Account.Name));
						//Client has no more characters, so the client can choose
						//the realm again!
						client.Account.Realm = 0;
						GameServer.Database.SaveObject(client.Account);
						// 2008-01-29 Kakuri - Obsolete
						//GameServer.Database.WriteDatabaseTable( typeof( Account ) );
					}

					// log deletion
					AuditMgr.AddAuditEntry(client, AuditType.Character, AuditSubtype.CharacterDelete, "", charName);

					break;
				}
			return 1;
		}
	}
}
