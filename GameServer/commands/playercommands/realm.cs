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

        private string[] albKeeps = { "Caer Benowyc", "Caer Berkstead", "Caer Erasleigh", "Caer Boldiam",
                                      "Caer Sursbrooke", "Caer Hurbury", "Caer Renaris" };
        private string[] midKeeps = { "Bledmeer Faste", "Nottmoor Faste", "Hlidskialf Faste", "Blendrake Faste",
                                      "Glenlock Faste", "Fensalir Faste", "Arvakr Faste"};
        private string[] hibKeeps = { "Dun Crauchon", "Dun Crimthainn", "Dun Bolg", "Dun nGed",
                                      "Dun da Behnn", "Dun Scathaig", "Dun Ailinne"};

        public void OnCommand(GameClient client, string[] args)
        {
            ArrayList realmInfo = new ArrayList();
            string[] tmpAlbKeeps = new string[albKeeps.Length];
            albKeeps.CopyTo(tmpAlbKeeps, 0);
            string[] tmpMidKeeps = new string[midKeeps.Length];
            midKeeps.CopyTo(tmpMidKeeps, 0);
            string[] tmpHibKeeps = new string[hibKeeps.Length];
            hibKeeps.CopyTo(tmpHibKeeps, 0);

            foreach (AbstractGameKeep keep in KeepMgr.GetNFKeeps())
            {
                #region Reformat Albion Keeps '[KeepName]: [OwnerRealm] ([Guild])'
                for (int i = 0; i < tmpAlbKeeps.Length; i++)
                {
                    if (keep.Name == tmpAlbKeeps[i])
                    {
                        tmpAlbKeeps[i] += ": ";
                        if (keep.Realm != eRealm.None)
                            tmpAlbKeeps[i] += keep.Realm;
                        if (keep.Guild != null)
                            tmpAlbKeeps[i] += " (" + keep.Guild.Name + ")";
                    }
                }
                #endregion

                #region Reformat Midgard Keeps '[KeepName]: [OwnerRealm] ([Guild])'
                for (int i = 0; i < tmpMidKeeps.Length; i++)
                {
                    if (keep.Name == tmpMidKeeps[i])
                    {
                        tmpMidKeeps[i] += ": ";
                        if (keep.Realm != eRealm.None)
                            tmpMidKeeps[i] += keep.Realm;
                        if (keep.Guild != null)
                            tmpMidKeeps[i] += " (" + keep.Guild.Name + ")";
                    }
                   
                }
                #endregion

                #region Reformat Hibernia Keeps '[KeepName]: [OwnerRealm] ([Guild])'
                for (int i = 0; i < tmpHibKeeps.Length; i++)
                {
                    if (keep.Name == tmpHibKeeps[i])
                    {
                        tmpHibKeeps[i] += ": ";
                        if (keep.Realm != eRealm.None)
                            tmpHibKeeps[i] += keep.Realm;
                        if (keep.Guild != null)
                            tmpHibKeeps[i] += " (" + keep.Guild.Name + ")";
                    }
                }
                #endregion
            }

            realmInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Realm.AlbKeeps") + ":");
            realmInfo.Add(AddKeeps(tmpAlbKeeps));
            realmInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Realm.MidKeeps") + ":");
            realmInfo.Add(AddKeeps(tmpMidKeeps));
            realmInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Realm.HibKeeps") + ":");
            realmInfo.Add(AddKeeps(tmpHibKeeps));
            realmInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Realm.DarknessFalls") + ": " + DFEnterJumpPoint.DarknessFallOwner);
            realmInfo.Add(" ");
            realmInfo.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Realm.UseRelicCommand"));


            client.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(client, "Scripts.Players.Realm.Title"), realmInfo);
        }

        private string AddKeeps(string[] keeps)
        {
            string keepString = "";

            foreach (string keep in keeps)
            {
                keepString += keep + "\n";
            }

            return keepString;
        }
   }
}
