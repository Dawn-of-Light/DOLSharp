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
using DOL.Database;
using DOL.Database.DataAccessInterfaces;
using log4net;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x54^168,"Handles account realm info and sending char overview")]
	public class CharacterOverviewRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			string accountName = packet.ReadString(24);

			//GameServer.Database.FillObjectRelations(client.Account);

			//reset realm if no characters
			if(client.Account.Realm != eRealm.None && (int)GameServer.Database.SelectObject("SELECT COUNT(*) FROM GamePlayer WHERE `AccountId` = '"+ client.Account.AccountId+"'") <= 0)
			{
				//DOLConsole.WriteLine("no chars, realm reset.");
				client.Account.Realm = eRealm.None;
			}

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
					client.Out.SendRealm(client.Account.Realm);
				}
			} 
			else 
			{
				eRealm chosenRealm;

				if(client.Account.Realm == eRealm.None || GameServer.ServerRules.IsAllowedCharsInAllRealms(client))
				{
					// allow player to choose the realm if not set already or if allowed by server rules
					if(accountName.EndsWith("-S"))      chosenRealm = eRealm.Albion;
					else if(accountName.EndsWith("-N")) chosenRealm = eRealm.Midgard;
					else if(accountName.EndsWith("-H")) chosenRealm = eRealm.Hibernia;
					else
					{
						if (log.IsErrorEnabled)
							log.Error("User has chosen unknown realm: "+accountName+"; account="+client.Account.AccountName);
						client.Out.SendRealm(eRealm.None);
						return 1;
					}

					if (client.Account.Realm == eRealm.None && !GameServer.ServerRules.IsAllowedCharsInAllRealms(client))
					{
						// save the choice
						client.Account.Realm = chosenRealm;
						client.Account.UpdateDatabase();
					}
				}
				else
				{
					// use saved realm ignoring what user has chosen if server rules do not allow to choose the realm
					chosenRealm = client.Account.Realm;
				}

				//DOLConsole.WriteLine("Sending overview! realm="+client.Account.Realm);
				client.ClientState=GameClient.eClientState.CharScreen;
				client.Player = null;
				client.Out.SendCharacterOverview(chosenRealm);
			}
			return 1;
		}
	}
}
