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
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        RegionTimer m_timer;
        GamePlayer m_player;
        GamePlayer m_killer;
        InventoryItem m_item;
        WorldInventoryItem gameItem;

        public GuildBanner(GamePlayer player)
		{
			m_player = player;
		}

        public GamePlayer Player
        {
            get { return m_player; }
        }

		public InventoryItem BannerItem
		{
			get { return m_item; }
		}

        public void Start()
        {
            if (m_player.Group != null)
            {
                if (m_player != null)
                {
					bool groupHasBanner = false;

                    foreach (GamePlayer playa in m_player.Group.GetPlayersInTheGroup())
                    {
                        if (playa.GuildBanner != null)
						{
							groupHasBanner = true;
							break;
						}
					}

                    if (groupHasBanner == false)
                    {
                        m_player.GuildBanner = this;
                        m_player.Stealth(false);
                        AddHandlers();

						m_item = m_player.Inventory.GetFirstItemByID(GuildBannerTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

						if (m_item == null)
						{
							InventoryItem item = new GuildBannerItem(GuildBannerTemplate);

							//if (item.Emblem != m_player.Guild.Emblem)
							{
								item.Template.Dirty = true;
								GameServer.Database.SaveObject(item.Template);
								GameServer.Database.UpdateInCache<ItemTemplate>(item.Template);
							}

							m_item = item;
							m_player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item);
						}

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
                        m_player.Out.SendMessage("Someone in your group already has a guild banner active!", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
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
                m_player.Out.SendMessage("You have left the group and your guild banner disappears!", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                m_player.GuildBanner = null;
                m_player.Inventory.RemoveItem(m_item);
                // m_player.Guild.GuildBanner = false; // I don't think guild loses banner just because player disbanded from group
                if (m_timer != null)
                {
                    m_timer.Stop();
                    m_timer = null;
                }
            }
        }

        public void Stop()
        {
            m_player.GuildBanner = null;
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

            return 6000; // Using standard pulsing spell pulse of 6 seconds, duration of 8 seconds - Tolakram
        }

        protected virtual void AddHandlers()
        {
            GameEventMgr.AddHandler(m_player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLoseBanner));
            GameEventMgr.AddHandler(m_player, GamePlayerEvent.StealthStateChanged, new DOLEventHandler(PlayerLoseBanner));
            GameEventMgr.AddHandler(m_player, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerLoseBanner));
            GameEventMgr.AddHandler(m_player, GamePlayerEvent.Dying, new DOLEventHandler(PlayerDied));
        }

		protected virtual void RemoveHandlers()
		{
			GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLoseBanner));
			GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.StealthStateChanged, new DOLEventHandler(PlayerLoseBanner));
			GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerLoseBanner));
			GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Dying, new DOLEventHandler(PlayerDied));
		}

		protected void PlayerLoseBanner(DOLEvent e, object sender, EventArgs args)
        {
			Stop();

			try 
			{
				// Remove item from inventory but do not delete it from DB
				m_player.Inventory.RemoveItem(m_item);
			} 
			catch
			{
				log.ErrorFormat("Failed to remove guild banner {0} from player {1}!", m_item.Name, m_player.Name);
			};

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
                m_player.GuildBanner = null;
                m_player.Inventory.RemoveItem(m_item);
				gameItem = WorldInventoryItem.CreateFromTemplate(m_item);

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
				if (m_player.Group != null)
				{
					foreach (GamePlayer player in m_player.Group.GetPlayersInTheGroup())
					{
						gameItem.AddOwner(player);
					}
				}

                gameItem.StartPickupTimer(10);
                m_player.Guild.GuildBanner = false;
                m_player.Guild.SendMessageToGuildMembers(m_player.Name + " has lost the guild banner!", eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                gameItem.Emblem = m_item.Emblem;
                gameItem.AddToWorld();

                Stop();
            }


        }

        protected ItemTemplate m_guildBannerTemplate;
        public ItemTemplate GuildBannerTemplate
        {
            get
            {
                if (m_guildBannerTemplate == null)
                {
					string guildIDNB = "GuildBanner_" + m_player.Guild.GuildID;
					// string guildIDNB = "GuildBanner_" + GameServer.Database.Escape(m_player.Guild.Name).Replace(";", "");

					// see if this guild already has banner created
					m_guildBannerTemplate = GameServer.Database.FindObjectByKey<ItemTemplate>(guildIDNB);

					if (m_guildBannerTemplate == null)
					{
						m_guildBannerTemplate = new ItemTemplate();
						m_guildBannerTemplate.CanDropAsLoot = false;
						m_guildBannerTemplate.Id_nb = guildIDNB;
						m_guildBannerTemplate.IsDropable = true;
						m_guildBannerTemplate.IsPickable = true;
						m_guildBannerTemplate.IsTradable = false;
						m_guildBannerTemplate.Item_Type = 41;
						m_guildBannerTemplate.Level = 1;
						m_guildBannerTemplate.MaxCharges = 1;
						m_guildBannerTemplate.MaxCount = 1;
						m_guildBannerTemplate.Emblem = m_player.Guild.Emblem;
						switch (m_player.Realm)
						{
							case eRealm.Albion:
								m_guildBannerTemplate.Model = 3223;
								break;
							case eRealm.Midgard:
								m_guildBannerTemplate.Model = 3224;
								break;
							case eRealm.Hibernia:
								m_guildBannerTemplate.Model = 3225;
								break;
						}
						m_guildBannerTemplate.Name = m_player.Guild.Name + "'s Banner";
						m_guildBannerTemplate.Object_Type = (int)eObjectType.HouseWallObject;
						m_guildBannerTemplate.Realm = 0;
						m_guildBannerTemplate.Quality = 100;
						m_guildBannerTemplate.ClassType = "DOL.GS.GuildBannerItem";
						m_guildBannerTemplate.PackageID = "GuildBanner";
						GameServer.Database.AddObject(m_guildBannerTemplate);
					}
                }

                return m_guildBannerTemplate;
            }
        }

    }
}



