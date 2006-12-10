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

using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Various keep guard commands
	/// </summary>
	[CmdAttribute(
		"&keepguard", //command to handle
		(uint)ePrivLevel.GM, //minimum privelege level
		"Various keep guard commands!", //command description
		"'/keepguard create <lord/fighter/archer/healer/stealther/caster> (optional for archer and caster)static'",
		/*
		"'/keepguard fastcreate <type>' to create a guard for the keep with base template",
		"'/keepguard fastcreate ' to show all template available in fast create",
		"'/keepguard create ' to create a guard for the closed keep ",
		"'/keepguard create lord' to create the lord for the closed keep ",
		"'/keepguard keep <keepID>' to assign guard to keep",
		"'/keepguard keep ' to assign guard to the nearest keep",
		"'/keepguard equipment <equipmentid>' to put equipment on guard",
		"'/keepguard level <level>' to change base level of guard",
		"'/keepguard save' to save guard into DB",
		 */
		"Use '/mob' command if you want to change other param of guard")] //usage
	public class KeepGuardCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		/// <summary>
		/// The command handler itself
		/// </summary>
		/// <param name="client">The client using the command</param>
		/// <param name="args">The command arguments</param>
		/// <returns></returns>
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return 1;
			}

			switch (args[1])
			{
				case "create":
					{
						GameKeepGuard guard = null;
						switch (args[2])
						{
							case "lord":
								{
									guard = new GuardLord();
									break;
								}
							case "fighter":
								{
									guard = new GuardFighter();
									break;
								}
							case "archer":
								{
									if (args.Length > 3)
										guard = new GuardStaticArcher();
									else guard = new GuardArcher();
									break;
								}
							case "healer":
								{
									guard = new GuardHealer();
									break;
								}
							case "stealther":
								{
									guard = new GuardStealther();
									break;
								}
							case "caster":
								{
									if (args.Length > 3)
										guard = new GuardStaticCaster();
									else guard = new GuardCaster();
									break;
								}
							case "hastener":
								{
									guard = new FrontierHastener();
									break;
								}
						}
						if (guard == null)
						{
							DisplaySyntax(client);
							return 0;
						}

						GameKeepComponent component = client.Player.TargetObject as GameKeepComponent;
						if (component != null)
						{
							DBKeepPosition pos = PositionMgr.CreatePosition(guard.GetType(), component.Height, client.Player, Guid.NewGuid().ToString(), component);
							PositionMgr.AddPosition(pos);
							PositionMgr.FillPositions();
						}
						else
						{
							guard.CurrentRegion = client.Player.CurrentRegion;
							guard.X = client.Player.X;
							guard.Y = client.Player.Y;
							guard.Z = client.Player.Z;
							guard.Heading = client.Player.Heading;
							guard.Realm = (byte)guard.CurrentZone.GetRealm();
							guard.SaveIntoDatabase();
							foreach (AbstractArea area in guard.CurrentAreas)
							{
								if (area is KeepArea)
								{
									AbstractGameKeep keep = (area as KeepArea).Keep;
									guard.Component = new GameKeepComponent();
									guard.Component.Keep = keep;
									guard.Component.Keep.Guards.Add(guard.InternalID, guard);
									break;
								}
							}
							TemplateMgr.RefreshTemplate(guard);
							guard.AddToWorld();
						}

						DisplayMessage(client, "Guard added!", new object[] { });
						break;
					}
				case "addposition":
					{
						if (client.Player.TargetObject is GameKeepGuard == false)
						{
							DisplayError(client, "Target a guard first!", new object[] { });
							return 1;
						}
						if (args.Length != 3)
						{
							DisplaySyntax(client);
							return 1;
						}
						byte height = byte.Parse(args[2]);
						height = KeepMgr.GetHeightFromLevel(height);
						if (height > 3)
						{
							DisplayError(client, "Keep levels range from 0 to 10", new object[] { });
							return 1;
						}

						GameKeepGuard guard = client.Player.TargetObject as GameKeepGuard;
						if (PositionMgr.GetPosition(guard) != null)
						{
							DisplayError(client, "You already have a position assigned for height " + height + ", remove first!", new object[] { });
							return 1;
						}

						DBKeepPosition pos = PositionMgr.CreatePosition(guard.GetType(), height, client.Player, guard.TemplateID, guard.Component);

						PositionMgr.AddPosition(pos);
						PositionMgr.FillPositions();

						DisplayMessage(client, "Guard position added", new object[] { });
						break;
					}
				case "removeposition":
					{
						if (client.Player.TargetObject is GameKeepGuard == false)
						{
							DisplayError(client, "Target a Guard first", null);
							return 1;
						}
						if (args.Length != 3)
						{
							DisplaySyntax(client);
							return 1;
						}
						byte height = byte.Parse(args[2]);
						if (height > 3)
						{
							DisplayError(client, "Choose a height between 0 and 3", new object[] { });
							return 1;
						}

						GameKeepGuard guard = client.Player.TargetObject as GameKeepGuard;

						DBKeepPosition pos = guard.Position;

						PositionMgr.RemovePosition(pos);

						PositionMgr.FillPositions();

						DisplayMessage(client, "Guard position removed", new object[] { });
						break;
					}
			}

			return 1;
		}
	}
}