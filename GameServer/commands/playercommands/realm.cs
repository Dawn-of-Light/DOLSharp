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
using DOL.GS.PacketHandler;
using System.Collections;
using System.Collections.Generic;
using DOL.Language;
using DOL.GS.Keeps;
using DOL.GS.ServerRules;

namespace DOL.GS.Commands
{
	[CmdAttribute(
	   "&realm",
	   ePrivLevel.Player,
		 "Displays the current realm status.", "/realm")]
	public class RealmCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		/*          Realm status
		 *
		 * Albion Keeps:
		 * Caer Benowyc: OwnerRealm (Guild)
		 * Caer Berkstead: OwnerRealm (Guild)
		 * Caer Erasleigh: OwnerRealm (Guild)
		 * Caer Boldiam: OwnerRealm (Guild)
		 * Caer Sursbrooke: OwnerRealm (Guild)
		 * Caer Hurbury: OwnerRealm (Guild)
		 * Caer Renaris: OwnerRealm (Guild)
		 *
		 * Midgard Keeps:
		 * Bledmeer Faste: OwnerRealm (Guild)
		 * Notmoor Faste: OwnerRealm (Guild)
		 * Hlidskialf Faste: OwnerRealm (Guild)
		 * Blendrake Faste: OwnerRealm (Guild)
		 * Glenlock Faste: OwnerRealm (Guild)
		 * Fensalir Faste: OwnerRealm (Guild)
		 * Arvakr Faste: OwnerRealm (Guild)
		 *
		 * Hibernia Keeps:
		 * Dun Chrauchon: OwnerRealm (Guild)
		 * Dun Crimthainn: OwnerRealm (Guild)
		 * Dun Bolg: OwnerRealm (Guild)
		 * Dun na nGed: OwnerRealm (Guild)
		 * Dun da Behnn: OwnerRealm (Guild)
		 * Dun Scathaig: OwnerRealm (Guild)
		 * Dun Ailinne: OwnerRealm (Guild)
		 *
		 * Darkness Falls: DFOwnerRealm
		 *
		 * Type '/relic' to display the relic status.
		 */

		

		public void OnCommand(GameClient client, string[] args)
		{
			string albKeeps = "";
			string midKeeps = "";
			string hibKeeps = "";
			ICollection<AbstractGameKeep> keepList = KeepMgr.GetNFKeeps();
			foreach (AbstractGameKeep keep in keepList)
			{
				if (keep is GameKeep)
				{
					switch (keep.OriginalRealm)
					{
						case eRealm.Albion:
							albKeeps += KeepStringBuilder(keep);
							break;
						case eRealm.Hibernia:
							hibKeeps += KeepStringBuilder(keep);
							break;
						case eRealm.Midgard:
							midKeeps += KeepStringBuilder(keep);
							break;
					}
				}
			}
			ArrayList realmInfo = new ArrayList();
			realmInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Realm.AlbKeeps") + ":");
			realmInfo.Add(albKeeps);
			realmInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Realm.MidKeeps") + ":");
			realmInfo.Add(midKeeps);
			realmInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Realm.HibKeeps") + ":");
			realmInfo.Add(hibKeeps);
			realmInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Realm.DarknessFalls") + ": " + GlobalConstants.RealmToName(DFEnterJumpPoint.DarknessFallOwner));
			realmInfo.Add(" ");
			realmInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Realm.UseRelicCommand"));
			client.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(client, "Scripts.Players.Realm.Title"), realmInfo);
		}

		private string KeepStringBuilder(AbstractGameKeep keep)
		{
			string buffer = "";
			buffer += keep.Name + ": " + GlobalConstants.RealmToName(keep.Realm);
			if (keep.Guild != null)
			{
				buffer += " (" + keep.Guild.Name + ")";
			}
			buffer += "\n";
			return buffer;
		}


	}
}
