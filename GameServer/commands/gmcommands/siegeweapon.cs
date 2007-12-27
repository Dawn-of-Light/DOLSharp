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
	"/siegeweapon create miniram/lightram/mediumram/heavyram")]
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
						}
						break;
					}
			}
		}
	}
}