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
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using System.Collections;
using log4net;

namespace DOL.GS
{
    /// <summary>
    /// Stable master that sells and takes horse route tickes
    /// </summary>
    [NPCGuildScript("Stable Master", eRealm.None)]
    public class GameStableMaster : GameMerchant
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs a new stable master
        /// </summary>
        public GameStableMaster()
        {
        }

        /// <summary>
        /// Called when the living is about to get an item from someone
        /// else
        /// </summary>
        /// <param name="source">Source from where to get the item</param>
        /// <param name="item">Item to get</param>
        /// <returns>true if the item was successfully received</returns>
        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            if (source == null || item == null) return false;

            if (source is GamePlayer)
            {
                GamePlayer player = (GamePlayer)source;

                if (item.Item_Type == 40 && isItemInMerchantList(item))
                {
                    PathPoint path = MovementMgr.LoadPath(item.Id_nb);

                    if ((path != null) && ((Math.Abs(path.X - this.X)) < 500) && ((Math.Abs(path.Y - this.Y)) < 500))
                    {
                        player.Inventory.RemoveCountFromStack(item, 1);

                        GameHorse mount;
                        
                        // item.Color of ticket is used for npctemplate. defaults to standard horse if item.color is 0
                        if (item.Color > 0)
                        {
                           	mount = new GameHorse(NpcTemplateMgr.GetTemplate(item.Color));
                        }
                        else
                        {
                        	#warning Graveen: Deprecated - please use npctemplate for voucher mounts
                        	mount = new GameHorse();
       						foreach (GameNPC npc in GetNPCsInRadius(400))
							if (npc.Name == LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameStableMaster.ReceiveItem.HorseName")
								|| npc.Name == "Dragon Fly" || npc.Name == "Ampheretere" || npc.Name == "Gryphon")
							{
								mount.Model = npc.Model;
								mount.Size = npc.Size;
								mount.Name = npc.Name;
								mount.Level = npc.Level;
                                if (npc.Name == "Gryphon" || npc.Name == "Dragon Fly" || npc.Name == "Ampheretere")
                                 mount.Flags = (uint)eFlags.FLYING;
								break;
							}
						}
  
                        switch ((eRace)player.Race)
                        {
                            case eRace.Briton: mount.Size = 50; //Briton
                                break;
                            case eRace.Avalonian: mount.Size = 55; //Avalonian
                                break;
                            case eRace.Highlander: mount.Size = 55; //Highlander
                                break;
                            case eRace.Saracen: mount.Size = 50; //Saracen
                                break;
                            case eRace.Norseman: mount.Size = 55; //Norseman
                                break;
                            case eRace.Troll: mount.Size = 67; //Troll
                                break;
                            case eRace.Dwarf: mount.Size = 42; //Dwarf
                                break;
                            case eRace.Kobold: mount.Size = 38; //Kobold
                                break;
                            case eRace.Celt: mount.Size = 50; //Celt
                                break;
                            case eRace.Firbolg: mount.Size = 50; //Firbolg
                                break;
                            case eRace.Elf: mount.Size = 55; //Elf
                                break;
                            case eRace.Lurikeen: mount.Size = 31; //Lurikeen
                                break;
                            case eRace.Inconnu: mount.Size = 45; //Inconnu
                                break;
                            case eRace.Valkyn: mount.Size = 52; //Valkyn
                                break;
                            case eRace.Sylvan: mount.Size = 55; //Sylvan
                                break;
                            case eRace.HalfOgre: mount.Size = 65; //HalfOgre
                                break;
                            case eRace.Frostalf: mount.Size = 48; //Frostalf
                                break;
                            case eRace.Shar: mount.Size = 48; //Shar
                                break;
                            case eRace.AlbionMinotaur: mount.Size = 65; //AlbionMinotaur
                                break;
                            case eRace.MidgardMinotaur: mount.Size = 65; //MidgardMinotaur
                                break;
                            case eRace.HiberniaMinotaur: mount.Size = 65; //HiberniaMinotaur
                                break;

                        }

                        mount.Realm = source.Realm;
                        mount.X = path.X;
                        mount.Y = path.Y;
                        mount.Z = path.Z;
                        mount.CurrentRegion = CurrentRegion;
                        mount.Heading = Point2D.GetHeadingToLocation(path, path.Next);
                        mount.AddToWorld();
                        mount.CurrentWayPoint = path;
                        GameEventMgr.AddHandler(mount, GameNPCEvent.PathMoveEnds, new DOLEventHandler(OnHorseAtPathEnd));
                        new MountHorseAction(player, mount).Start(400);
                        new HorseRideAction(mount).Start(4000);
                        return true;
                    }
                }
                else
                {
                    player.Out.SendMessage("Sorry, that ticket isn't for around here.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
            return false;
        }

        private bool isItemInMerchantList(InventoryItem item)
        {
            foreach (DictionaryEntry de in m_tradeItems.GetAllItems())
            {
                ItemTemplate compareItem = de.Value as ItemTemplate;
                if (compareItem != null)
                {
                    if (compareItem.Id_nb == item.Id_nb)
                    {
                        return true;
                    }
                }
            }
            return false;
        }    


        private void SendReply(GamePlayer target, string msg)
        {
            target.Out.SendMessage(
                msg,
                eChatType.CT_System, eChatLoc.CL_PopupWindow);
        }

        /// <summary>
        /// Handles 'horse route end' events
        /// </summary>
        /// <param name="e"></param>
        /// <param name="o"></param>
        /// <param name="args"></param>
        public void OnHorseAtPathEnd(DOLEvent e, object o, EventArgs args)
        {
            if (!(o is GameNPC)) return;
            GameNPC npc = (GameNPC)o;

            GameEventMgr.RemoveHandler(npc, GameNPCEvent.PathMoveEnds, new DOLEventHandler(OnHorseAtPathEnd));
            npc.StopMoving();
            npc.RemoveFromWorld();
        }

        /// <summary>
        /// Handles delayed player mount on horse
        /// </summary>
        protected class MountHorseAction : RegionAction
        {
            /// <summary>
            /// The target horse
            /// </summary>
            protected readonly GameNPC m_horse;

            /// <summary>
            /// Constructs a new MountHorseAction
            /// </summary>
            /// <param name="actionSource">The action source</param>
            /// <param name="horse">The target horse</param>
            public MountHorseAction(GamePlayer actionSource, GameNPC horse)
                : base(actionSource)
            {
                if (horse == null)
                    throw new ArgumentNullException("horse");
                m_horse = horse;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                GamePlayer player = (GamePlayer)m_actionSource;
                player.MountSteed(m_horse, true);
            }
        }

        /// <summary>
        /// Handles delayed horse ride actions
        /// </summary>
        protected class HorseRideAction : RegionAction
        {
            /// <summary>
            /// Constructs a new HorseStartAction
            /// </summary>
            /// <param name="actionSource"></param>
            public HorseRideAction(GameNPC actionSource)
                : base(actionSource)
            {
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                GameNPC horse = (GameNPC)m_actionSource;
                horse.MoveOnPath(horse.MaxSpeed);
            }
        }
    }
}