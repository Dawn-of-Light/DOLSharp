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
namespace DOL.GS.Commands
{
	[CmdAttribute(
	"&siegeweapon",
	ePrivLevel.GM,
	"creates siege weapons",
	"/siegeweapon create miniram/lightram/mediumram/heavyram/catapult/ballista/cauldron/trebuchet")]
	public class SiegeWeaponCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 3)
			{
				DisplaySyntax(client);
				return;
			}

			switch (args[1].ToLower())
			{
				case "create":
					{
						switch (args[2].ToLower())
						{
							case "miniram":
								{
									GameSiegeRam ram = new GameSiegeRam();
									ram.Position = client.Player.Position;
									ram.Model = 2605;
									ram.Level = 0;
									ram.Name = "mini ram";
									ram.Realm = client.Player.Realm;
									ram.AddToWorld();
									break;
								}
							case "lightram":
								{
									GameSiegeRam ram = new GameSiegeRam();
									ram.Position = client.Player.Position;
									ram.Model = 2600;
									ram.Level = 1;
									ram.Name = "light ram";
									ram.Realm = client.Player.Realm;
									ram.AddToWorld();
									break;
								}
							case "mediumram":
								{
									GameSiegeRam ram = new GameSiegeRam();
									ram.Position = client.Player.Position;
									ram.Model = 2601;
									ram.Level = 2;
									ram.Name = "medium ram";
									ram.Realm = client.Player.Realm;
									ram.AddToWorld();
									break;
								}
							case "heavyram":
								{
									GameSiegeRam ram = new GameSiegeRam();
									ram.Position = client.Player.Position;
									ram.Model = 2602;
									ram.Level = 3;
									ram.Name = "heavy ram";
									ram.Realm = client.Player.Realm;
									ram.AddToWorld();
									break;
								}
								case "catapult":
								{
									GameSiegeCatapult cat = new GameSiegeCatapult();
									cat.Position = client.Player.Position;
									cat.Model = 0xA26;
									cat.Level = 3;
									cat.Name = "catapult";
									cat.Realm = client.Player.Realm;
									cat.AddToWorld();
									break;
								}
								case "ballista":
								{
									GameSiegeBallista bal = new GameSiegeBallista();
									bal.Position = client.Player.Position;
									bal.Model = 0x0A55;
									bal.Level = 3;
									bal.Name = "field ballista";
									bal.Realm = client.Player.Realm;
									bal.AddToWorld();
									break;
								}
								case "cauldron":
								{
									GameSiegeRam ram = new GameSiegeRam();
									ram.Position = client.Player.Position;
									ram.Model =  0xA2F;
									ram.Level = 3;
									ram.Name = "cauldron of boiling oil";
									ram.Realm = client.Player.Realm;
									ram.AddToWorld();
									break;
								}
								case "trebuchet":
								{
									GameSiegeTrebuchet tre = new GameSiegeTrebuchet();
									tre.Position = client.Player.Position;
									tre.Model = 0xA2E;
									tre.Level = 3;
									tre.Name = "trebuchet";
									tre.Realm = client.Player.Realm;
									tre.AddToWorld();
									break;
								}
						}
						break;
					}
			}
		}
	}
}