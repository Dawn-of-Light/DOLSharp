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
using System.Collections;
using System.Reflection;
using DOL.GS.Database;
using log4net;

namespace DOL.GS.PacketHandler.v168
{
	/// <summary>
	/// handles Train clicks from Trainer Window
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP,0xD4^168,"Handles Player Train Requests")]
	public class PlayerTrainRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			uint x = packet.ReadInt();
			uint y = packet.ReadInt();
			int sessionId = packet.ReadShort();
			int unk1 = packet.ReadByte();
			int skillindex = packet.ReadByte();

			IList speclist = client.Player.GetSpecList();
			if (skillindex < speclist.Count) {
				Specialization spec = (Specialization)speclist[skillindex];
				if (spec.Level >= client.Player.Level) {
					client.Out.SendMessage("You can't train in this specialization again this level!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 1;
				}
				if (client.Player.SkillSpecialtyPoints >= spec.Level+1) {
					client.Player.SkillSpecialtyPoints -= (ushort)(spec.Level+1);
					spec.Level++;
					client.Player.OnSkillTrained(spec);
					client.Out.SendUpdatePoints();
					//client.Player.CharacterClass.OnLevelUp(client.Player);
					//client.Player.CharacterClass.OnSkillTrained(client.Player, spec);
					//client.Out.SendUpdatePlayerSkills();
					client.Out.SendTrainerWindow();
					return 1;
				} else {					
					client.Out.SendMessage("That specialization costs "+(spec.Level+1)+" specialization points!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					client.Out.SendMessage("You don't have that many specialization points left for this level.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 1;
				}
			} else {
				if (log.IsErrorEnabled)
					log.Error("Player <"+client.Player.Name+"> requested to train incorrect skill index");
			}	
			return 1;
		}
	}
}
