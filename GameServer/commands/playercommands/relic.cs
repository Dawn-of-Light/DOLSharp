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
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using System.Collections;
using DOL.Language;
using DOL.GS.Keeps;
using DOL.GS.ServerRules;

namespace DOL.GS.Commands
{
	[CmdAttribute(
	"&relics",
	new string[] {"&relic"},
	ePrivLevel.Player,
	"Displays the current relic status.", "/relics")]
   public class RelicCommandHandler : AbstractCommandHandler, ICommandHandler
   {
        /*          Relic status
         *
         * Albion Relics:
         * Strength: OwnerRealm
         * Power: OwnerRealm
         *
         * Midgard Relics:
         * Strength: OwnerRealm
         * Power: OwnerRealm
         *
         * Hibernia Relics:
         * Strength: OwnerRealm
         * Power: OwnerRealm
         *
         * Use '/realm' for Realm Info.
         */

        public void OnCommand(GameClient client, string[] args)
        {
			if (IsSpammingCommand(client.Player, "relic"))
				return;

            string albStr = "", albPwr = "", midStr = "", midPwr = "", hibStr = "", hibPwr = "";
			var relicInfo = new List<string>();

            #region Reformat Relics  '[Type]: [OwnerRealm]'
            foreach (GameRelic relic in RelicMgr.getNFRelics())
            {
                switch (relic.OriginalRealm)
                {
                    case eRealm.Albion:
                        {
                            if (relic.RelicType == eRelicType.Strength)
								albStr = LanguageMgr.GetTranslation(client, "Scripts.Players.Relic.Strength") + ": " + GlobalConstants.RealmToName(relic.Realm);
                            if (relic.RelicType == eRelicType.Magic)
								albPwr = LanguageMgr.GetTranslation(client, "Scripts.Players.Relic.Power") + ": " + GlobalConstants.RealmToName(relic.Realm);
                            break;
                        }

                    case eRealm.Midgard:
                        {
                            if (relic.RelicType == eRelicType.Strength)
								midStr = LanguageMgr.GetTranslation(client, "Scripts.Players.Relic.Strength") + ": " + GlobalConstants.RealmToName(relic.Realm);
                            if (relic.RelicType == eRelicType.Magic)
								midPwr = LanguageMgr.GetTranslation(client, "Scripts.Players.Relic.Power") + ": " + GlobalConstants.RealmToName(relic.Realm);
                            break;
                        }

                    case eRealm.Hibernia:
                        {
                            if (relic.RelicType == eRelicType.Strength)
								hibStr = LanguageMgr.GetTranslation(client, "Scripts.Players.Relic.Strength") + ": " + GlobalConstants.RealmToName(relic.Realm);
                            if (relic.RelicType == eRelicType.Magic)
								hibPwr = LanguageMgr.GetTranslation(client, "Scripts.Players.Relic.Power") + ": " + GlobalConstants.RealmToName(relic.Realm);
                            break;
                        }
                }
            }
            #endregion

            relicInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Relic.AlbRelics")+ ":");
            relicInfo.Add(albStr);
            relicInfo.Add(albPwr);
            relicInfo.Add("");
            relicInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Relic.MidRelics") + ":");
            relicInfo.Add(midStr);
            relicInfo.Add(midPwr);
            relicInfo.Add("");
            relicInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Relic.HibRelics") + ":");
            relicInfo.Add(hibStr);
            relicInfo.Add(hibPwr);
            relicInfo.Add("");
            relicInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Relic.UseRealmCommand"));

            client.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(client, "Scripts.Players.Relic.Title"), relicInfo);
        }
   }
}
