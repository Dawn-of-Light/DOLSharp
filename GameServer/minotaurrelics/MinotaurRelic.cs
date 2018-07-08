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
using System.Text;
using System.Threading;

using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;

namespace DOL.GS
{
    public class MinotaurRelic : GameStaticItem
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MinotaurRelic()
        {
            m_saveInDB = true;
        }

        public MinotaurRelic(DBMinotaurRelic obj)
            : this()
        {
            LoadFromDatabase(obj);
        }

        public RegionTimer Respawntimer { get; set; }

        private DBMinotaurRelic _dbRelic;
        private Timer _timer;
        private ISpellHandler _spellHandler;
        private GameSpellEffect _gameSpellEffect;
        public IList<GamePlayer> Playerlist = new List<GamePlayer>();

        /// <summary>
        /// gets or sets the current Owner of this Relic
        /// </summary>
        public GamePlayer Owner { get; set; }

        public int RelicId { get; set; }

        public string ProtectorClassType { get; set; }

        public bool SpawnLocked { get; set; }

        /// <summary>
        /// gets or sets the current XP of this Relic
        /// </summary>
        public double Xp { get; set; }

        /// <summary>
        /// Get the RelicType
        /// </summary>
        public Spell RelicSpell { get; set; }

        /// <summary>
        /// Get the RelicType
        /// </summary>
        public int RelicSpellId { get; set; }

        /// <summary>
        /// Get the RelicTarget
        /// </summary>
        public string RelicTarget { get; set; }

        public int SpawnX { get; set; }

        public int SpawnY { get; set; }

        public int SpawnZ { get; set; }

        public int SpawnHeading { get; set; }

        public int SpawnRegion { get; set; }

        public int Effect { get; set; }

        /// <summary>
        /// Loads the GameRelic from Database
        /// </summary>
        /// <param name="obj">The DBRelic-object for this relic</param>
        public override void LoadFromDatabase(DataObject obj)
        {
            _dbRelic = obj as DBMinotaurRelic;

            if (_dbRelic == null)
            {
                return;
            }

            InternalID = obj.ObjectId;
            RelicId = _dbRelic.RelicID;

            Heading = (ushort)_dbRelic.SpawnHeading;
            CurrentRegionID = (ushort)_dbRelic.SpawnRegion;
            X = _dbRelic.SpawnX;
            Y = _dbRelic.SpawnY;
            Z = _dbRelic.SpawnZ;

            SpawnHeading = _dbRelic.SpawnHeading;
            SpawnRegion = _dbRelic.SpawnRegion;
            Effect = _dbRelic.Effect;
            SpawnX = _dbRelic.SpawnX;
            SpawnY = _dbRelic.SpawnY;
            SpawnZ = _dbRelic.SpawnZ;

            RelicSpellId = _dbRelic.relicSpell;
            RelicSpell = SkillBase.GetSpellByID(_dbRelic.relicSpell);
            RelicTarget = _dbRelic.relicTarget;

            Name = _dbRelic.Name;
            Model = _dbRelic.Model;

            Xp = MinotaurRelicManager.MaxRelicExp;

            ProtectorClassType = _dbRelic.ProtectorClassType;
            SpawnLocked = _dbRelic.SpawnLocked;

            // set still empty fields
            Emblem = 0;
            Level = 99;
        }

        /// <summary>
        /// Saves the current MinotaurRelic to the database
        /// </summary>
        public override void SaveIntoDatabase()
        {
            _dbRelic.SpawnHeading = Heading;
            _dbRelic.SpawnRegion = CurrentRegionID;
            _dbRelic.SpawnX = X;
            _dbRelic.SpawnY = Y;
            _dbRelic.SpawnZ = Z;

            _dbRelic.Effect = Effect;

            _dbRelic.Name = Name;
            _dbRelic.Model = Model;
            _dbRelic.relicSpell = RelicSpellId;
            _dbRelic.ProtectorClassType = ProtectorClassType;
            _dbRelic.SpawnLocked = SpawnLocked;

            if (InternalID == null)
            {
                GameServer.Database.AddObject(_dbRelic);
                InternalID = _dbRelic.ObjectId;
            }
            else
            {
                GameServer.Database.SaveObject(_dbRelic);
            }
        }

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
            {
                return false;
            }

            foreach (GameNPC npc in GetNPCsInRadius(100))
            {
                if (npc.Model == 1583)
                {
                    player.Out.SendMessage($"You cannot pickup {GetName(0, false)}. It is locked!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }
            }

            if (!player.IsAlive)
            {
                player.Out.SendMessage($"You cannot pickup {GetName(0, false)}. You are dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (Owner != null)
            {
                player.Out.SendMessage("This Relic is owned by someone else!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.MinotaurRelic != null)
            {
                player.Out.SendMessage("You already have a Relic!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.Group != null)
            {
                foreach (GamePlayer pl in player.Group.GetPlayersInTheGroup())
                {
                    if (pl.MinotaurRelic != null)
                    {
                        player.Out.SendMessage("Someone in your group already have a Relic!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return false;
                    }
                }
            }

            if (player.IsStealthed)
            {
                player.Stealth(false);
            }

            PlayerTakesRelic(player);
            return true;
        }

        /// <summary>
        /// Called when a Players picks up a Relic
        /// </summary>
        /// <param name="player"></param>
        private void PlayerTakesRelic(GamePlayer player)
        {
            if (player == null)
            {
                return;
            }

            RemoveFromWorld();
            SetHandlers(player, true);
            player.MinotaurRelic = this;
            Owner = player;

            foreach (GamePlayer pl in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                pl.Out.SendMinotaurRelicWindow(player, Effect, true);
            }

            player.Out.SendMinotaurRelicBarUpdate(player, (int)Xp);

            _spellHandler = ScriptMgr.CreateSpellHandler(Owner, RelicSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
            if (_spellHandler != null)
            {
                _gameSpellEffect = new GameSpellEffect(_spellHandler, RelicSpell.Duration, 0);
            }

            _timer = new Timer(XpTimerCallBack, null, 3000, 0);

            ApplyRelicEffect();
        }

        private void ApplyRelicEffect()
        {
            if (RelicSpell == null || _spellHandler == null || _gameSpellEffect == null)
            {
                return;
            }

            IList<GamePlayer> newPlayerlist = new List<GamePlayer>();

            if (Owner != null)
            {
                switch (RelicTarget.ToLower())
                {
                    case "self":
                        newPlayerlist.Add(Owner);
                        break;
                    case "group":
                        if (Owner.Group == null)
                        {
                            newPlayerlist.Add(Owner);
                        }
                        else
                        {
                            foreach (GamePlayer plr in Owner.Group.GetPlayersInTheGroup())
                            {
                                if (plr != null && !newPlayerlist.Contains(plr) && Owner.IsWithinRadius(plr, WorldMgr.VISIBILITY_DISTANCE))
                                {
                                    newPlayerlist.Add(plr);
                                }
                            }
                        }

                        break;
                    case "realm":
                        foreach (GamePlayer plr in Owner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        {
                            if (plr != null && GameServer.ServerRules.IsAllowedToAttack(Owner, plr, true) == false && !newPlayerlist.Contains(plr))
                            {
                                newPlayerlist.Add(plr);
                            }
                        }

                        break;
                }
            }

            lock (Playerlist)
            {
                foreach (GamePlayer plr in Playerlist)
                {
                    if (plr == null)
                    {
                        continue;
                    }

                    if (!newPlayerlist.Contains(plr))
                    {
                        try
                        {
                            lock (plr.EffectList)
                            {
                                GameSpellEffect check = SpellHandler.FindEffectOnTarget(plr, _gameSpellEffect.Spell.SpellType);
                                check?.Cancel(false);
                            }
                        }
                        catch (Exception e)
                        {
                            if (Log.IsErrorEnabled)
                            {
                                Log.Error($"Minotaur Relics : Effect Cancel : {e}");
                            }
                        }
                    }
                }

                foreach (GamePlayer plr in newPlayerlist)
                {
                    if (plr == null)
                    {
                        continue;
                    }

                    try
                    {
                        lock (plr.EffectList)
                        {
                            GameSpellEffect check = SpellHandler.FindEffectOnTarget(plr, _gameSpellEffect.Spell.SpellType);
                            if (check == null)
                            {
                                ISpellHandler handler = ScriptMgr.CreateSpellHandler(plr, RelicSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
                                GameSpellEffect plreffect = null;
                                if (handler != null)
                                {
                                    plreffect = new GameSpellEffect(handler, RelicSpell.Duration, 0);
                                }

                                plreffect?.Start(plr);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (Log.IsErrorEnabled)
                        {
                            Log.Error($"Minotaur Relics : Effect Start : {e}");
                        }
                    }
                }

                Playerlist = newPlayerlist;
            }
        }

        private void StopRelicEffect()
        {
            if (RelicSpell == null || _spellHandler == null || _gameSpellEffect == null)
            {
                return;
            }

            lock (Playerlist)
            {
                foreach (GamePlayer plr in Playerlist)
                {
                    if (plr == null)
                    {
                        continue;
                    }

                    try
                    {
                        lock (plr.EffectList)
                        {
                            GameSpellEffect check = SpellHandler.FindEffectOnTarget(plr, _gameSpellEffect.Spell.SpellType);
                            check?.Cancel(false);
                        }
                    }
                    catch (Exception e)
                    {
                        if (Log.IsErrorEnabled)
                        {
                            Log.Error($"Minotaur Relics : Stop Relic Effect : {e}");
                        }
                    }
                }

                Playerlist.Clear();
            }
        }

        /// <summary>
        /// Is called whenever the CurrentCarrier is supposed to loose the relic.
        /// </summary>
        /// <param name="player">the player who loses the relic</param>
        /// <param name="stop">True when we should stop the XP timer</param>
        public virtual void PlayerLoosesRelic(GamePlayer player, bool stop)
        {
            StopRelicEffect();
            player.Out.SendMinotaurRelicWindow(player, 0, false);
            Update(player);
            SetHandlers(player, false);
            player.MinotaurRelic = null;
            Owner = null;
            if (stop && _timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            foreach (GamePlayer pl in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                pl.Out.SendMinotaurRelicWindow(player, 0, false);
            }

            AddToWorld();
        }

        /// <summary>
        /// Called when the Timer is reached
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private void XpTimerCallBack(object state)
        {
            ApplyRelicEffect();

            if (Xp - MinotaurRelicManager.XpLossPerTick < 0)
            {
                Xp = 0;
            }
            else
            {
                Xp -= MinotaurRelicManager.XpLossPerTick;
            }

            if (Owner != null)
            {
                Update(Owner);
                Owner.Out.SendMinotaurRelicBarUpdate(Owner, (int)Xp);
            }

            if (Xp == 0)
            {
                RelicDispose();
                return;
            }

            _timer?.Change(3000, Timeout.Infinite);
        }

        /// <summary>
        /// Called when the Relic has reached 0 XP and drops
        /// </summary>
        public virtual void RelicDispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            if (Owner != null)
            {
                PlayerLoosesRelic(Owner, true);
            }

            RemoveFromWorld();

            if (Respawntimer != null)
            {
                Respawntimer.Stop();
                Respawntimer = null;
            }

            Respawntimer = new RegionTimer(this, RespawnTimerCallback, Util.Random(MinotaurRelicManager.MinRespawnTimer, MinotaurRelicManager.MaxRespawnTimer));
        }

        /// <summary>
        /// Called when the Respawntimer is reached
        /// </summary>
        /// <param name="respawnTimer"></param>
        /// <returns></returns>
        protected override int RespawnTimerCallback(RegionTimer respawnTimer)
        {
            if (Respawntimer != null)
            {
                Respawntimer.Stop();
                Respawntimer = null;
            }

            if (ObjectState == eObjectState.Active)
            {
                return 0;
            }

            X = SpawnX;
            Y = SpawnY;
            Z = SpawnZ;
            Heading = (ushort)SpawnHeading;
            CurrentRegionID = (ushort)SpawnRegion;
            Xp = MinotaurRelicManager.MaxRelicExp;
            AddToWorld();
            return 0;
        }

        public virtual void ManualRespawn()
        {
            if (Respawntimer != null)
            {
                Respawntimer.Stop();
                Respawntimer = null;
            }

            if (ObjectState == eObjectState.Active)
            {
                return;
            }

            X = SpawnX;
            Y = SpawnY;
            Z = SpawnZ;
            Heading = (ushort)SpawnHeading;
            CurrentRegionID = (ushort)SpawnRegion;
            Xp = MinotaurRelicManager.MaxRelicExp;
            AddToWorld();
        }

        /// <summary>
        /// Updates the Relic on the Warmap and such
        /// </summary>
        /// <param name="living"></param>
        protected virtual void Update(GameLiving living)
        {
            if (living == null)
            {
                return;
            }

            CurrentRegionID = living.CurrentRegionID;
            X = living.X;
            Y = living.Y;
            Z = living.Z;
            Heading = living.Heading;
            foreach (GameClient clt in WorldMgr.GetClientsOfRegion(CurrentRegionID))
            {
                if (clt?.Player == null)
                {
                    continue;
                }

                if (Xp > 0)
                {
                    clt.Player.Out.SendMinotaurRelicMapUpdate((byte)RelicId, CurrentRegionID, X, Y, Z);
                }
                else
                {
                    clt.Player.Out.SendMinotaurRelicMapRemove((byte)RelicId);
                }
            }
        }

        /// <summary>
        /// Called to set the Events to the Carrier
        /// </summary>
        /// <param name="player"></param>
        /// <param name="start"></param>
        protected virtual void SetHandlers(GamePlayer player, bool start)
        {
            if (start)
            {
                GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, PlayerAbsence);
                GameEventMgr.AddHandler(player, GameLivingEvent.Dying, PlayerAbsence);
                GameEventMgr.AddHandler(player, GamePlayerEvent.Linkdeath, PlayerAbsence);
                GameEventMgr.AddHandler(player, GameLivingEvent.GainedRealmPoints, RealmPointGain);
                GameEventMgr.AddHandler(player, GamePlayerEvent.RegionChanged, PlayerAbsence);
                GameEventMgr.AddHandler(player, GameLivingEvent.RegionChanging, PlayerAbsence);
                GameEventMgr.AddHandler(player, GamePlayerEvent.StealthStateChanged, PlayerAbsence);
                GameEventMgr.AddHandler(player, GamePlayerEvent.AcceptGroup, PlayerAbsence);
            }
            else
            {
                GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, PlayerAbsence);
                GameEventMgr.RemoveHandler(player, GameLivingEvent.Dying, PlayerAbsence);
                GameEventMgr.RemoveHandler(player, GamePlayerEvent.Linkdeath, PlayerAbsence);
                GameEventMgr.RemoveHandler(player, GameLivingEvent.GainedRealmPoints, RealmPointGain);
                GameEventMgr.RemoveHandler(player, GamePlayerEvent.RegionChanged, PlayerAbsence);
                GameEventMgr.RemoveHandler(player, GameLivingEvent.RegionChanging, PlayerAbsence);
                GameEventMgr.RemoveHandler(player, GamePlayerEvent.StealthStateChanged, PlayerAbsence);
                GameEventMgr.RemoveHandler(player, GamePlayerEvent.AcceptGroup, PlayerAbsence);
            }
        }

        private void RealmPointGain(DOLEvent e, object sender, EventArgs args)
        {
            if (sender is GamePlayer player && args is GainedRealmPointsEventArgs arg)
            {
                if (player.MinotaurRelic == null)
                {
                    return;
                }

                if (player.MinotaurRelic.Xp < MinotaurRelicManager.MaxRelicExp)
                {
                    player.MinotaurRelic.Xp += (int)arg.RealmPoints / 6;
                }
            }
        }

        private void PlayerAbsence(DOLEvent e, object sender, EventArgs args)
        {
            if (!(sender is GamePlayer player))
            {
                return;
            }

            if (e == GamePlayerEvent.AcceptGroup)
            {
                if (player.Group == null)
                {
                    return;
                }

                foreach (GamePlayer pl in player.Group.GetPlayersInTheGroup())
                {
                    if (pl.MinotaurRelic != null)
                    {
                        player.Out.SendMessage("Someone in your group already has a Relic!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        PlayerLoosesRelic(player, false);
                        return;
                    }
                }

                if (RelicTarget.ToLower() != "self")
                {
                    return;
                }
            }

            PlayerLoosesRelic(player, false);
        }

        public override bool AddToWorld()
        {
            if (SpawnLocked)
            {
                if (X == SpawnX && Y == SpawnY)
                {
                    if (ProtectorClassType != string.Empty)
                    {
                        GameObject protector = (GameObject)GetType().Assembly.CreateInstance(ProtectorClassType, false);
                        if (protector != null)
                        {
                            // each individual protector will need to have all info needed to add to world
                            // location, name, model, level, etc. hard coded into its class type (script)
                            // aswell as adding the Locked effect on the relic where it spawns, and removing
                            // the effect when dead.
                            protector.AddToWorld();

                            return base.AddToWorld();
                        }

                        Log.Debug($"[Minotaur Relic] ClassType: {ProtectorClassType} was not found, Relic not loaded!");
                        return false;
                    }
                }
            }

            return base.AddToWorld();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("name: ").Append(Name).Append("\n")
                .Append("RelicID: ").Append(RelicId).Append("\n");

            if (Owner != null)
            {
                sb.Append($"Owner: {Owner.Name}");
            }
            else
            {
                sb.Append("Owner: No Owner");
            }

            return sb.ToString();
        }
    }
}