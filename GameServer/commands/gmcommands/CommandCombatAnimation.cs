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

using DOL.Database;
using DOL.Language;

namespace DOL.GS.Commands
{
	/// <summary>
	/// CommandCombatAnimation is used to test Combat Animation Packets.
	/// </summary>
	[CmdAttribute(
		"&combatanimation",
		ePrivLevel.GM,
		"Used to Display a Combat Animation",
		"/combatanimation <animationid> [<weaponattackmodel>]")]
	public class CommandCombatAnimation : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			byte id = 0;
			try
			{
				id = Convert.ToByte(args[1]);
			}
			catch
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Cast.InvalidId"));
				return;
			}
			
			ushort attackWeapon = 0;
			ushort defenderShield = 0;
			byte healthPercent = 0;
			
			if (args.Length > 2)
			{
				try
				{
					attackWeapon = Convert.ToUInt16(args[2]);
				}
				catch
				{
					DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Cast.InvalidId"));
					return;
				}
			}
			else if (client.Player != null && client.Player.AttackWeapon != null)
			{
				attackWeapon = (ushort)client.Player.AttackWeapon.Model;
			}
			
			if (client.Player != null && client.Player.TargetObject != null && client.Player.TargetObject is GameLiving && ((GameLiving)client.Player.TargetObject).Inventory != null)
			{
				healthPercent = client.Player.TargetObject.HealthPercent;
				InventoryItem left = ((GameLiving)client.Player.TargetObject).Inventory.GetItem(eInventorySlot.LeftHandWeapon);
				if (left != null)
					defenderShield = (ushort)left.Model;
			}
			
			foreach (GamePlayer plr in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				GamePlayer player = plr;
				plr.Client.Out.SendCombatAnimation(client.Player, client.Player.TargetObject, attackWeapon, defenderShield, 0, 0, id, healthPercent);
			}
		}
	}
}
