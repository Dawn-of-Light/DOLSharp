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
	ePrivLevel.Admin,
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
									ram.X = client.Player.X;
									ram.Y = client.Player.Y;
									ram.Z = client.Player.Z;
									ram.CurrentRegion = client.Player.CurrentRegion;
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
									ram.X = client.Player.X;
									ram.Y = client.Player.Y;
									ram.Z = client.Player.Z;
									ram.CurrentRegion = client.Player.CurrentRegion;
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
									ram.X = client.Player.X;
									ram.Y = client.Player.Y;
									ram.Z = client.Player.Z;
									ram.CurrentRegion = client.Player.CurrentRegion;
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
									ram.X = client.Player.X;
									ram.Y = client.Player.Y;
									ram.Z = client.Player.Z;
									ram.CurrentRegion = client.Player.CurrentRegion;
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
									cat.X = client.Player.X;
									cat.Y = client.Player.Y;
									cat.Z = client.Player.Z;
									cat.CurrentRegion = client.Player.CurrentRegion;
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
									bal.X = client.Player.X;
									bal.Y = client.Player.Y;
									bal.Z = client.Player.Z;
									bal.CurrentRegion = client.Player.CurrentRegion;
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
									ram.X = client.Player.X;
									ram.Y = client.Player.Y;
									ram.Z = client.Player.Z;
									ram.CurrentRegion = client.Player.CurrentRegion;
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
									tre.X = client.Player.X;
									tre.Y = client.Player.Y;
									tre.Z = client.Player.Z;
									tre.CurrentRegion = client.Player.CurrentRegion;
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