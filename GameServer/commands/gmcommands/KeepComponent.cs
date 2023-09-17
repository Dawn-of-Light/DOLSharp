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
using System.Linq;

using DOL.Database;
using DOL.GS.Geometry;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		 "&keepcomponent",
		 ePrivLevel.GM,
		 "GMCommands.KeepComponents.Description",
		 "GMCommands.KeepComponents.Usage.Create.TID",
		 "GMCommands.KeepComponents.Usage.Create.T",
		 "GMCommands.KeepComponents.Usage.Skin",
		 "/keepcomponent move - move to your position",
		 "/keepcomponent rotate [0 - 3]",
		 "/keepcomponent reload",
		 "'/keepcomponent save' to save the component in the DB",
		 "GMCommands.KeepComponents.Usage.Delete")]
	public class KeepComponentCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private readonly ushort INVISIBLE_MODEL = 150;

		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return;
			}

			AbstractGameKeep myKeep = GameServer.KeepManager.GetKeepCloseToSpot(client.Player.Position, WorldMgr.OBJ_UPDATE_DISTANCE);

			if (myKeep == null)
			{
				DisplayMessage(client, "You are not near a keep.");
			}

			switch (args[1])
			{
				#region Create
				case "create":
					{
						if (args.Length < 3)
						{
							int i = 0;
							foreach (string str in Enum.GetNames(typeof(GameKeepComponent.eComponentSkin)))
							{
								client.Out.SendMessage("#" + i + ": " + str, eChatType.CT_System, eChatLoc.CL_SystemWindow);
								i++;
							}
							DisplaySyntax(client);
							return;
						}

						if (myKeep == null)
						{
							DisplaySyntax(client);
							return;
						}

						int skin = 0;
						try
						{
							skin = Convert.ToInt32(args[2]);
						}
						catch
						{
							int i = 0;
							foreach (string str in Enum.GetNames(typeof(GameKeepComponent.eComponentSkin)))
							{
								client.Out.SendMessage("#" + i + ": " + str, eChatType.CT_System, eChatLoc.CL_SystemWindow);
								i++;
							}
							DisplaySyntax(client);
							return;
						}

						if (args.Length >= 4)
						{
							int keepid = 0;
							try
							{
								keepid = Convert.ToInt32(args[3]);
								myKeep = GameServer.KeepManager.GetKeepByID(keepid);
							}
							catch
							{
								DisplaySyntax(client);
								return;
							}
						}

						GameKeepComponent component = new GameKeepComponent();
						component.Position = client.Player.Position
                            .With(Angle.Degrees(component.ComponentHeading * 90) + myKeep.Orientation);
						component.ComponentHeading = (client.Player.Orientation - myKeep.Orientation).InHeading / 1024;
						component.Keep = myKeep;

                        var angle = myKeep.Orientation.InRadians;
                        component.ComponentX = CalcCX(client.Player, myKeep, angle);
                        component.ComponentY = CalcCY(client.Player, myKeep, angle);

						component.Name = myKeep.Name;
						component.Model = INVISIBLE_MODEL;
						component.Skin = skin;
						component.Level = (byte)myKeep.Level;
						component.Health = component.MaxHealth;
						component.ID = myKeep.KeepComponents.Count;
						component.Keep.KeepComponents.Add(component);
						component.SaveInDB = true;
						component.AddToWorld();
						component.SaveIntoDatabase();
						client.Out.SendKeepInfo(myKeep);
						client.Out.SendKeepComponentInfo(component);
						client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.KeepComponents.Create.KCCreated"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					} break;
				#endregion Create
                #region Move
                case "move":
                    {
                        var component = client.Player.TargetObject as GameKeepComponent;

                        component.Position = client.Player.Position
                            .With(Angle.Heading(component.ComponentHeading * 1024) + myKeep.Orientation);
                        component.ComponentHeading = (client.Player.Orientation - myKeep.Orientation).InDegrees / 90;
                        component.Keep = myKeep;

                        var angle = myKeep.Orientation.InRadians;
                        component.ComponentX = CalcCX(client.Player, myKeep, angle);
                        component.ComponentY = CalcCY(client.Player, myKeep, angle);

                        client.Out.SendKeepInfo(myKeep);
                        client.Out.SendKeepComponentInfo(component);
						client.Out.SendKeepComponentDetailUpdate(component);
						client.Out.SendMessage("Component moved.  Use /keepcomponent save to save, or reload to reload the original position.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    } break;
                #endregion
				#region Rotate
				case "rotate":
					{
						try
						{
							ushort amount = Convert.ToUInt16(args[2]);

							if (amount > 3)
								amount = 3;

							GameKeepComponent component = client.Player.TargetObject as GameKeepComponent;

							component.ComponentHeading = amount;
							component.Orientation = Angle.Heading(component.ComponentHeading * 1024) + myKeep.Orientation;

							client.Out.SendKeepInfo(myKeep);
							client.Out.SendKeepComponentInfo(component);
							client.Out.SendKeepComponentDetailUpdate(component);
							client.Out.SendMessage("Component rotated.  Use /keepcomponent save to save, or reload to reload the original position.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch
						{
							DisplayMessage(client, "/keepcomponent rotate [0 - 3]");
						}
					} break;
				#endregion
				#region Skin
                case "skin":
					{
						if (args.Length < 3)
						{
							int i = 0;
							foreach (string str in Enum.GetNames(typeof(GameKeepComponent.eComponentSkin)))
							{
								client.Out.SendMessage("#" + i + ": " + str, eChatType.CT_System, eChatLoc.CL_SystemWindow);
								i++;
							}
							DisplaySyntax(client);
							return;
						}

						int skin = 0;
						try
						{
							skin = Convert.ToInt32(args[2]);
						}
						catch
						{
							DisplaySyntax(client);
							return;
						}
						GameKeepComponent component = client.Player.TargetObject as GameKeepComponent;
						if (component == null)
						{
							DisplaySyntax(client);
							return;
						}
						component.Skin = skin;
                        foreach (GameClient cli in WorldMgr.GetClientsOfRegion(client.Player.CurrentRegionID))
                        {
                            cli.Out.SendKeepComponentInfo(component);
							cli.Out.SendKeepComponentDetailUpdate(component);
                        }
						//client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.KeepComponents.Skin.YChangeSkin"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Out.SendMessage("Component skin updated.  Use /keepcomponent save to save, or reload to reload the original skin.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					} break;
				#endregion Skin
				#region Delete
				case "delete":
					{
						GameKeepComponent component = client.Player.TargetObject as GameKeepComponent;
						if (component == null)
						{
							DisplaySyntax(client);
							return;
						}
						component.RemoveFromWorld();
						component.Delete();
						component.DeleteFromDatabase();
						client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.KeepComponents.Delete.YDeleteKC"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

					} break;
				#endregion Delete
				#region Save
				case "save":
					{
						GameKeepComponent component = client.Player.TargetObject as GameKeepComponent;
						if (component == null)
						{
							DisplaySyntax(client);
							return;
						}
						component.SaveIntoDatabase();
						client.Out.SendMessage(string.Format("Saved ComponentID: {0}, KeepID: {1}, Skin: {2}, Health: {3}%", 
															component.ID, 
															(component.Keep == null ? "0" : component.Keep.KeepID.ToString()), 
															component.Skin, 
															component.HealthPercent), eChatType.CT_System, eChatLoc.CL_SystemWindow);

					} break;
				#endregion Save
				#region Reload
				case "reload":
                    {

                        GameKeepComponent component = client.Player.TargetObject as GameKeepComponent;
                        if (component == null)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        var dbcomponent = DOLDB<DBKeepComponent>.SelectObject(DB.Column(nameof(DBKeepComponent.KeepID)).IsEqualTo(component.Keep.KeepID).And(DB.Column(nameof(DBKeepComponent.ID)).IsEqualTo(component.ID)));
                        component.ComponentX = dbcomponent.X;
                        component.ComponentY = dbcomponent.Y;
                        component.ComponentHeading = dbcomponent.Heading;
						component.Skin = dbcomponent.Skin;

						foreach (GameClient cli in WorldMgr.GetClientsOfRegion(client.Player.CurrentRegionID))
						{
							cli.Out.SendKeepComponentInfo(component);
							cli.Out.SendKeepComponentDetailUpdate(component);
						}

                        client.Out.SendMessage("Component Reloaded", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
					}
				#endregion Reload
				#region Default
				default:
					{
						DisplaySyntax(client);
						return;
					}
				#endregion Default
			}
		}

        public int CalcCX(GamePlayer player, AbstractGameKeep myKeep, double angle)
        {
            var keepPos = myKeep.Position;
            var playerPos = player.Position;
            if (Math.Abs(Math.Sin(angle)) < 0.0001) //for approximations, == 0 wont work.
            {
                return (playerPos.X - keepPos.X) / 148;
            }
            else
            {
                return (int)((148 * Math.Sin(angle) * keepPos.X - 148 * Math.Sin(angle) * playerPos.X + playerPos.Y - keepPos.Y)
                    / (148 * Math.Sin(angle) - 148 * 148 * 2 * Math.Sin(angle) * Math.Cos(angle)));
            }
        }

        public int CalcCY(GamePlayer player, AbstractGameKeep myKeep, double angle)
        {
            var keepPos = myKeep.Position;
            var playerPos = player.Position;
            if (Math.Abs(Math.Sin(angle)) < 0.0001)
            {
                return (keepPos.Y - playerPos.Y) / 148;
            }
            else
            {
                int cx = (int)((148 * Math.Sin(angle) * keepPos.X - 148 * Math.Sin(angle) * playerPos.X + playerPos.Y - keepPos.Y)
                            / (148 * Math.Sin(angle) - 148 * 148 * 2 * Math.Sin(angle) * Math.Cos(angle)));
                return (int)((keepPos.Y - playerPos.Y + 148 * Math.Sin(angle) * cx) / (148 * Math.Cos(angle)));
            }
        }
	}
}