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
using DOL.Language;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&range",
		ePrivLevel.Player,
		"Gives a range to a target",
		"/range")]
	public class RangeCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			GameLiving living = client.Player.TargetObject as GameLiving;
			if (client.Player.TargetObject == null)
				DisplayMessage(client, (LanguageMgr.GetTranslation(client, "Scripts.Players.Range.NeedTarget")));
			else if (living == null || (living != null && client.Account.PrivLevel > 1))
			{
				int range = client.Player.GetDistance( client.Player.TargetObject );
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Range.Result", range, (client.Player.TargetInView ? "" : LanguageMgr.GetTranslation(client, "Scripts.Players.Range.NotVisible"))));
			}
			else
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Range.InvalidObject"));
		}
	}
}
