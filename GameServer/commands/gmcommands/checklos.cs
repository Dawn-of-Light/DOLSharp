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
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using DOL.GS;
using DOL.GS.Geometry;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&checklos",
		ePrivLevel.GM,
		"Check line of sight with the target",
		"'/checklos'")]
	public class CheckLosCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public async void OnCommand(GameClient client, string[] args)
		{
			if (client.Player?.TargetObject == null && client.Player?.GroundTarget == null)
			{
				DisplayMessage(client, "You need a target to use this command.");
				return;
			}

			var text = new List<string>();
			if (client.Player.TargetObject != null)
			{
				var target = client.Player.TargetObject;
				text.Add($"Target: {target.Name} (OID: {target.ObjectID}, distance: {Vector3.Distance(target.ToVector3(), client.Player.ToVector3())})");
				text.Add($"Target in view (player's cache): {client.Player.TargetInView}");

				var stats = new RaycastStats();
				var sw = new Stopwatch();
				sw.Start();
				var serverResult = LosCheckMgr.GetCollisionDistance(client.Player, target, ref stats);
				sw.Stop();
				text.Add($"Target in view (server-side los): {serverResult} ({stats.nbNodeTests} node, {stats.nbFaceTests} face, {sw.Elapsed.TotalMilliseconds}ms)");
				var losResult = new TaskCompletionSource<ushort>();
				client.Out.SendCheckLOS(client.Player, target, (player, response, targetOID) => losResult.SetResult(response));
				var result = await losResult.Task;
				text.Add($"CheckLOS packet response: 0x{result:X4} (in view: {(result & 0x100) != 0})");
			}
			else
			{
				var ground = client.Player.GroundTarget;
				text.Add($"Ground target: {ground}");
				text.Add($"Target in view (player's cache): {client.Player.GroundTargetInView}");
				// TODO
			}

			client.Out.SendCustomTextWindow("Check LOS", text);
		}
	}
}