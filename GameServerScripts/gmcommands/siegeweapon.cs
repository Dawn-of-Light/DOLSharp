namespace DOL.GS.Scripts
{
	[CmdAttribute(
	"&siegeweapon",
	(uint)ePrivLevel.GM,
	"creates siege weapons",
	"/siegeweapon create miniram/lightram/mediumram/heavyram")]
	public class SiegeWeaponCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			try 
			{
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
			catch { DisplaySyntax(client); }
			return 1;
		}
	}
}