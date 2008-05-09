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
using System.Linq;
using DOL.Database2;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// SinglePermission is special permission of command for player
	/// </summary>
	public class SinglePermission
	{
		protected SinglePermission()
		{

		}

		public static bool HasPermission(GamePlayer player,string command)
		{
			if ((from s in DatabaseLayer.Instance.OfType<DBSinglePermission>()
                 where s.Command == command && s.PlayerID == player.PlayerCharacter.ID
                 select s).Count() == 0) // TODO: TUne
				return false;
			return true;
		}

		public static void setPermission(GamePlayer player,string command)
		{
			DBSinglePermission perm = new DBSinglePermission();
			perm.Command = command;
			perm.PlayerID = player.PlayerCharacter.ObjectId;
            perm.Save();
		}

		public static bool removePermission(GamePlayer player,string command)
		{
            DatabaseObject obj = (from s in DatabaseLayer.Instance.OfType<DBSinglePermission>()
                                  where s.Command == command && s.PlayerID == player.PlayerCharacter.ID
                                  select s).First();
			if (obj == null)
			{
				return false;
			}
			GameServer.Database.DeleteObject(obj);
			return true;
		}
	}
}