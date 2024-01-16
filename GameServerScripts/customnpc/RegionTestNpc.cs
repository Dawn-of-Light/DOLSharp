using System;
using DOL.GS.Geometry;

namespace DOL.GS
{
	public class RegionTestNpc : GameNPC
	{
		public override bool WhisperReceive(GameLiving source, string text)
		{
			return SayReceive(source, text);
		}

		public override bool SayReceive(GameLiving source, string str)
		{
			if (!base.SayReceive(source, str))
				return false;

			string[] split = str.Split(' ');
			switch (split[0])
			{
				case "create":
					ushort id = 286;

					if (split.Length > 1)
					{
						try { id = ushort.Parse(split[1]); }
						catch { }
					}

					m_instance = (Instance)WorldMgr.CreateInstance(id, typeof(Instance));
					if (m_instance != null)
						Say("Success, instance created.");
					else
						Say("Instance creation found errors.");

					//Try and load a template from the db...
					if (split.Length > 2)
					{
						string load = split[2];
						Say("Trying to load instance '" + load + "' template from DB");
						m_instance.LoadFromDatabase(load);
					}
					
					break;
				case "test":
					if (m_instance == null)
						Say("Instance is currently null.");
					else
					{
                        var entrancePosition = Position.Create(regionID: m_instance.ID, x: 32361, y: 31744, z: 16003, heading: 1075);

						if (m_instance.EntrancePosition != Position.Nowhere)
						{
                            entrancePosition = m_instance.EntrancePosition;
						}

						// save current position so player can use /instance exit
						var savePosition = source.Position.With(Angle.Zero);
						source.TempProperties.setProperty(source.Name + "_exit", savePosition);

						Say("Instance ID " + m_instance.ID + ", Skin: " + m_instance.Skin + ", with " + m_instance.Zones.Count + " zones inside the region.");

						if (!source.MoveTo(entrancePosition))
						{
							Say("Source could not be moved to instance entrance; MoveTo returned false.  Now trying to move to current location inside the instance.");

							if (!source.MoveTo(source.Position))
							{
								Say("Sorry, that failed as well.");
							}
						}
					}
					break;
			}
			return true;
		}

		private Instance m_instance;
	}
}

namespace DOL.GS.Commands
{
	[CmdAttribute("&npcdebug", //command to handle
	              ePrivLevel.Admin, //minimum privelege level
	              "Writes information of all npcs in a region to the console", //command description
	              "/npcdebug")] //usage
	public class NpcDebugHandler : AbstractCommandHandler, ICommandHandler
	{
		private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void OnCommand(GameClient client, string[] args)
		{
			int index = 0;
			foreach (GameObject npc in client.Player.CurrentRegion.Objects)
			{
				if (npc == null)
					continue;

				log.Warn(index + ": Object name=" + npc.Name + ", {X/Y/Z} H =  { " + npc.Position.X + " / " + npc.Position.Y + " / " + npc.Position.Z + " }  " + npc.Orientation.InHeading + ".");
				log.Warn("Zone, zoneID " + npc.CurrentZone.ID + " (" + npc.CurrentZone.Description + "), zoneSkinID = " + npc.CurrentZone.ZoneSkinID + ".");
			}
		}
	}
}
