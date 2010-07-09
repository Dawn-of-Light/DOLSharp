using System;
using System.Reflection;
using System.Collections;
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Database;
using log4net;
using DOL.GS.Effects;

namespace DOL.GS
{
    public class GuildBanner
    {
        RegionTimer m_timer;
        //RegionTimer m_dieTimer;
        GamePlayer m_player;
        GamePlayer m_killer;
        InventoryItem m_item;
        WorldInventoryItem gameItem;
        public GuildBanner(GamePlayer player) { m_player = player; }
        public GamePlayer Player
        {
            get { return m_player; }
        }
        public void Start()
        {
            if (m_player.Group != null)
            {
                if (m_player != null)
                {
                    foreach (GamePlayer playa in m_player.Group.GetPlayersInTheGroup())
                    {
                        if (!playa.IsCarryingGuildBanner)
                        {
                            m_player.IsCarryingGuildBanner = true;
                            m_player.Stealth(false);
                            AddHandlers();
							// TODO: This is not saved because itemtemplate is never added to db ... needs reworked - tolakram
                            InventoryItem item = GameInventoryItem.Create<ItemTemplate>(GuildBannerItem);
                            m_item = item;
                            m_player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item);
                            if (m_timer != null)
                            {
                                m_timer.Stop();
                                m_timer = null;
                            }
                            m_timer = new RegionTimer(m_player, new RegionTimerCallback(TimerTick));
                            m_timer.Start(1);

                        }
                        else
                        {
                            m_player.Out.SendMessage("Someone in your group already has a Guildbanner active!", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                            m_player.IsCarryingGuildBanner = false;
                            m_player.Inventory.RemoveItem(m_item);
                            m_player.Guild.GuildBanner = false;
                            if (m_timer != null)
                            {
                                m_timer.Stop();
                                m_timer = null;
                            }
                        }
                    }
                }
                else
                {
                    if (m_timer != null)
                    {
                        m_timer.Stop();
                        m_timer = null;
                    }
                }
            }
            else
            {
                m_player.Out.SendMessage("You have left the group and your Guildbanner disappears!", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                m_player.IsCarryingGuildBanner = false;
                m_player.Inventory.RemoveItem(m_item);
                m_player.Guild.GuildBanner = false;
                if (m_timer != null)
                {
                    m_timer.Stop();
                    m_timer = null;
                }
            }
        }
        public void Stop()
        {
            m_player.IsCarryingGuildBanner = false;
            RemoveHandlers();
            if (m_timer != null)
            {
                m_timer.Stop();
                m_timer = null;
            }
        }
        private int TimerTick(RegionTimer timer)
        {
            foreach (GamePlayer playa in m_player.GetPlayersInRadius(1500))
            {
                if (playa.Group != null && m_player.Group != null && m_player.Group.IsInTheGroup(playa))
                {
                    if (!(GameServer.ServerRules.IsAllowedToAttack(m_player, playa, false)))
                    {
                        GuildBannerEffect effect = GuildBannerEffect.CreateEffectOfClass(m_player, playa);
                        if (effect != null)
                        {
                            if (playa.EffectList.GetOfType(typeof(GuildBannerEffect)) == null)
                            {
                                effect.Start(playa);
                            }
                        }
                    }
                }
            }
            return 20000;
        }
        protected virtual void RemoveHandlers()
        {
            GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerEvent));
            GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.StealthStateChanged, new DOLEventHandler(PlayerEvent));
            GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerEvent));
            GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Dying, new DOLEventHandler(PlayerDied));
        }
        protected virtual void AddHandlers()
        {
            GameEventMgr.AddHandler(m_player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerEvent));
            GameEventMgr.AddHandler(m_player, GamePlayerEvent.StealthStateChanged, new DOLEventHandler(PlayerEvent));
            GameEventMgr.AddHandler(m_player, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerEvent));
            GameEventMgr.AddHandler(m_player, GamePlayerEvent.Dying, new DOLEventHandler(PlayerDied));
        }
        protected void PlayerEvent(DOLEvent e, object sender, EventArgs args)
        {
            try { m_player.Inventory.RemoveItem(m_item); } catch{};
            Stop();
        }
        protected void PlayerDied(DOLEvent e, object sender, EventArgs args)
        {
            DyingEventArgs arg = args as DyingEventArgs;
            if (arg == null) return;
            GameObject killer = arg.Killer as GameObject;
            if (killer is GamePlayer)
            {
                GamePlayer killa = killer as GamePlayer;
                m_killer = killa;
                m_player.IsCarryingGuildBanner = false;
                m_player.Inventory.RemoveItem(m_item);
                gameItem = new WorldInventoryItem(m_item);

                Point2D point = m_player.GetPointFromHeading( m_player.Heading, 30 );
                gameItem.X = point.X;
                gameItem.Y = point.Y;
                gameItem.Z = m_player.Z;
                gameItem.Heading = m_player.Heading;
                gameItem.CurrentRegionID = m_player.CurrentRegionID;
                if (m_killer.Group != null)
                {
                    foreach (GamePlayer player in m_killer.Group.GetPlayersInTheGroup())
                    {
                        gameItem.AddOwner(player);
                    }
                }
                else
                {
                    gameItem.AddOwner(m_killer);
                }
                gameItem.StartPickupTimer(10);
                m_player.Guild.GuildBanner = false;
                m_player.Guild.SendMessageToGuildMembers(m_player.Name + " has lost the guild banner!", eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                gameItem.Emblem = m_item.Emblem;
                gameItem.AddToWorld();

                Stop();

            }


        }

        protected ItemTemplate m_guildBanner;
        public ItemTemplate GuildBannerItem
        {
            get
            {
                if (m_guildBanner == null)
                {
                    m_guildBanner = new ItemTemplate();
                    m_guildBanner.CanDropAsLoot = false;
                    m_guildBanner.Id_nb = "GuildBanner_" + m_player.Guild.Name;
                    m_guildBanner.IsDropable = true;
                    m_guildBanner.IsPickable = true;
                    m_guildBanner.IsTradable = false;
                    m_guildBanner.Item_Type = 41;
                    m_guildBanner.Level = 1;
                    m_guildBanner.MaxCharges = 1;
                    m_guildBanner.MaxCount = 1;
                    m_guildBanner.Emblem = m_player.Guild.Emblem;
                    /*
                       3223 Guild Banner Albion 
                       3224 Guild Banner Midgard 
                       3225 Guild Banner Hibernia 
                    */
                    switch (m_player.Realm)
                    {
                        case eRealm.Albion:
                            m_guildBanner.Model = 3223;
                            break;
                        case eRealm.Midgard:
                            m_guildBanner.Model = 3224;
                            break;
                        case eRealm.Hibernia:
                            m_guildBanner.Model = 3225;
                            break;
                    }
                    m_guildBanner.Name = m_player.Guild.Name+"'s Banner";
                    m_guildBanner.Object_Type = (int)eObjectType.HouseWallObject;
                    m_guildBanner.Realm = 0;
                    m_guildBanner.Quality = 100;
                }
                return m_guildBanner;
            }
        }

    }
}



