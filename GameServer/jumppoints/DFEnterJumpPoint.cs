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
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS.JumpPoints
{
	/// <summary>
	/// This jump point type allow all player to go to its destination
	/// </summary>
	public class DFEnterJumpPoint : ClassicJumpPoint
	{
		/// <summary>
		/// The realm who current can enter df
		/// </summary>
		public static eRealm DarknessFallOwner = eRealm.None;
	
		/// <summary>
		/// initialize the darkness fall entrance system
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			int albcount = KeepMgr.GetTowerCountByRealm(eRealm.Albion);
			int midcount = KeepMgr.GetTowerCountByRealm(eRealm.Midgard);
			int hibcount = KeepMgr.GetTowerCountByRealm(eRealm.Hibernia);

			if (albcount>midcount)
			{
				if (albcount > hibcount)
				{
					DarknessFallOwner = eRealm.Albion;
				}
				else
				{
					DarknessFallOwner = eRealm.Hibernia;
				}
			}
			else if (albcount>hibcount)
			{
				if (albcount > midcount)
				{
					DarknessFallOwner = eRealm.Albion;
				}
				else
				{
					DarknessFallOwner = eRealm.Midgard;
				}
			}
			else 
			{
				if (midcount > hibcount)
				{
					DarknessFallOwner = eRealm.Midgard;
				}
				else
				{
					DarknessFallOwner = eRealm.Hibernia;
				}
			}
			
			
			GameEventMgr.AddHandler(KeepEvent.KeepTaken,new DOLEventHandler(OnKeepTaken));
		}

		/// <summary>
		/// when  keep is taken it check if the realm which take gain the control of DF
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		public static void OnKeepTaken(DOLEvent e, object sender, EventArgs arguments)
		{
			KeepEventArgs args = arguments as KeepEventArgs;
			eRealm realm = (eRealm) args.Player.Realm ;
			if (realm != DarknessFallOwner )
			{
				int currentDFOwnerTowerCount = KeepMgr.GetTowerCountByRealm(DarknessFallOwner);
				int challengerOwnerTowerCount = KeepMgr.GetTowerCountByRealm(realm);
				if (currentDFOwnerTowerCount < challengerOwnerTowerCount)
					DarknessFallOwner = realm;
			}
		}

		#region Function

		/// <summary>
		/// Decides whether player can jump to the target point.
		/// All messages with reasons must be sent here.
		/// Can change destination too.
		/// </summary>
		/// <param name="player">the player who want to jump</param>
		/// <returns>the JumpPointTargetLocation if allowed, null if not</returns>
		public override JumpPointTargetLocation IsAllowedToJump(GamePlayer player)
		{
			if(base.IsAllowedToJump(player) == null) return null;

			if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_Normal
				&& DarknessFallOwner != (eRealm)player.Realm)
			{								 
				player.Out.SendMessage("Darkness fall is not owned by your realm.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return null;
			}
			return m_targetLocation;
		}
		
		#endregion
	}
}
