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
 */
using System.Reflection;
using DOL.Database;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x54^168,"Handles account realm info and sending char overview")]
	public class CharacterOverviewRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			client.ClientState = GameClient.eClientState.CharScreen;
			if (client.Player != null)
			{
				try
				{
					// find the cached character and force it to update with the latest saved character
					DOLCharacters cachedCharacter = null;
					foreach (DOLCharacters accountChar in client.Account.Characters)
					{
						if (accountChar.ObjectId == client.Player.InternalID)
						{
							cachedCharacter = accountChar;
							break;
						}
					}

					if (cachedCharacter != null)
					{
						cachedCharacter = client.Player.DBCharacter;
					}
				}
				catch (System.Exception ex)
				{
					log.ErrorFormat("Error attempting to update cached player. {0}", ex.Message);
				}
			}

			client.Player = null;

			//reset realm if no characters
			if((client.Account.Characters == null || client.Account.Characters.Length <= 0) && client.Account.Realm != (int)eRealm.None)
			{
				client.Account.Realm = (int)eRealm.None;
			}

			string accountName = packet.ReadString(24);

			if(accountName.EndsWith("-X")) 
			{
				if(GameServer.ServerRules.IsAllowedCharsInAllRealms(client))
				{
					client.Out.SendRealm(eRealm.None);
				}
				else
				{
					//Requests to know what realm an account is
					//assigned to... if Realm::NONE is sent, the
					//Realm selection screen is shown
					switch(client.Account.Realm)
					{
						case 1: client.Out.SendRealm(eRealm.Albion); break;
						case 2: client.Out.SendRealm(eRealm.Midgard); break;
						case 3: client.Out.SendRealm(eRealm.Hibernia); break;
						default: client.Out.SendRealm(eRealm.None); break;
					}
				}
			} 
			else 
			{
				eRealm chosenRealm;

				if(client.Account.Realm == (int)eRealm.None || GameServer.ServerRules.IsAllowedCharsInAllRealms(client))
				{
					// allow player to choose the realm if not set already or if allowed by server rules
					if(accountName.EndsWith("-S"))      chosenRealm = eRealm.Albion;
					else if(accountName.EndsWith("-N")) chosenRealm = eRealm.Midgard;
					else if(accountName.EndsWith("-H")) chosenRealm = eRealm.Hibernia;
					else
					{
						if (log.IsErrorEnabled)
							log.Error("User has chosen unknown realm: "+accountName+"; account="+client.Account.Name);
						client.Out.SendRealm(eRealm.None);
						return;
					}

					if (client.Account.Realm == (int)eRealm.None && !GameServer.ServerRules.IsAllowedCharsInAllRealms(client))
					{
						// save the choice
						client.Account.Realm = (int)chosenRealm;
						GameServer.Database.SaveObject(client.Account);
						// 2008-01-29 Kakuri - Obsolete
						//GameServer.Database.WriteDatabaseTable( typeof( Account ) );
					}
				}
				else
				{
					// use saved realm ignoring what user has chosen if server rules do not allow to choose the realm
					chosenRealm = (eRealm)client.Account.Realm;
				}

				client.Out.SendCharacterOverview(chosenRealm);
			}
		}
	}
}
