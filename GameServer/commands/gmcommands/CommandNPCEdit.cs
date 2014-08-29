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

using DOL.Database;
using DOL.Language;
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOL.GS.Commands
{
	// TODO finish mock up !
	/// <summary>
	/// CommandNPCEdit is used to bring a dialog window and handle NPC edition.
	/// </summary>
	[CmdAttribute(
		"&npcedit",
		ePrivLevel.GM,
		"Bring up a NPC Edit Window",
		"/npcedit [targeting a NPC]")]
	public class CommandNPCEdit : AbstractCommandHandler, ICommandHandler
	{
		protected const string NPCEDIT_CURRENT = "NPCEDIT_WINDOW_CURRENT_MENU"; 
		/// <summary>
		/// Handle Command, No args handling.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="args"></param>
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player != null && client.Player.TargetObject is GameNPC)
			{
				List<string> windowText = new List<string>();
				windowText.Add("Welcome to the NPC Edit Window !");
				windowText.Add("Try some action, [test] ");
				windowText.Add("Try some action, [other action] ");
				windowText.Add(this.GetHashCode().ToString());

				client.Out.SendMessage(string.Join("\n", windowText.ToArray()), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				
				AddHandler(client.Player);
			}
			else
			{
				DisplaySyntax(client);
			}
		}

		protected void ReceiveKeyword(DOLEvent e, object sender, EventArgs arguments)
		{
			if (e == GameLivingEvent.Whisper && arguments is WhisperEventArgs && sender is GamePlayer)
			{
				WhisperEventArgs whisperArgs = (WhisperEventArgs)arguments;
				GamePlayer player = (GamePlayer)sender;
				
				ChatUtil.SendDebugMessage(player, string.Format("Received Command to Target {0}, Text : {1}", whisperArgs.Target == null ? "null" : whisperArgs.Target.Name, whisperArgs.Text));
			}
		}
		
		protected void AddHandler(GamePlayer player)
		{
			GameEventMgr.AddHandlerUnique(player, GamePlayerEvent.ChangeTarget, new DOLEventHandler(RemoveHandler));
			GameEventMgr.AddHandlerUnique(player, GamePlayerEvent.Moving, new DOLEventHandler(RemoveHandler));
			GameEventMgr.AddHandlerUnique(player, GamePlayerEvent.RemoveFromWorld, new DOLEventHandler(RemoveHandler));
			GameEventMgr.AddHandlerUnique(player, GamePlayerEvent.RegionChanging, new DOLEventHandler(RemoveHandler));
			GameEventMgr.AddHandlerUnique(player, GamePlayerEvent.Delete, new DOLEventHandler(RemoveHandler));
			GameEventMgr.AddHandlerUnique(player, GamePlayerEvent.Dying, new DOLEventHandler(RemoveHandler));
			GameEventMgr.AddHandlerUnique(player, GamePlayerEvent.Linkdeath, new DOLEventHandler(RemoveHandler));
			GameEventMgr.AddHandlerUnique(player, GamePlayerEvent.Quit, new DOLEventHandler(RemoveHandler));
			
			GameEventMgr.AddHandlerUnique(player, GameLivingEvent.Whisper, new DOLEventHandler(ReceiveKeyword));
			
			player.TempProperties.setProperty(NPCEDIT_CURRENT, "home");
		}
		
		protected void RemoveHandler(DOLEvent e, object sender, EventArgs arguments)
		{
			GameEventMgr.RemoveHandler(sender, GameLivingEvent.Whisper, new DOLEventHandler(ReceiveKeyword));
			
			GameEventMgr.RemoveHandler(sender, GamePlayerEvent.ChangeTarget, new DOLEventHandler(RemoveHandler));
			GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Moving, new DOLEventHandler(RemoveHandler));
			GameEventMgr.RemoveHandler(sender, GamePlayerEvent.RemoveFromWorld, new DOLEventHandler(RemoveHandler));
			GameEventMgr.RemoveHandler(sender, GamePlayerEvent.RegionChanging, new DOLEventHandler(RemoveHandler));
			GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Delete, new DOLEventHandler(RemoveHandler));
			GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Dying, new DOLEventHandler(RemoveHandler));
			GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Linkdeath, new DOLEventHandler(RemoveHandler));
			GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Quit, new DOLEventHandler(RemoveHandler));
			
			if (sender is GamePlayer)
			{
				((GamePlayer)sender).TempProperties.removeProperty(NPCEDIT_CURRENT);
			}
		}
	}
}
