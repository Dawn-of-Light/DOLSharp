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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DOL.Database;
using DOL.Events;

namespace DOL.GS.Keeps
{
    // TODO : find all skin of keep door to load it from here
    /// <summary>
    /// A keepComponent
    /// </summary>
    public class GameKeepComponent : GameLiving, IComparable, IGameKeepComponent
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly ushort INVISIBLE_MODEL = 150;

        public enum eComponentSkin : byte
        {
            Gate = 0,
            WallInclined = 1,
            WallInclined2 = 2,
            WallAngle2 = 3,
            TowerAngle = 4,
            WallAngle = 5,
            WallAngleInternal = 6,
            TowerHalf = 7,
            WallHalfAngle = 8,
            Wall = 9,
            Keep = 10,
            Tower = 11,
            WallWithDoorLow = 12,
            WallWithDoorHigh = 13,
            BridgeHigh = 14,
            WallInclinedLow = 15,
            BridgeLow = 16,
            BridgeHightSolid = 17,
            BridgeHighWithHook = 18,
            GateFree = 19,
            BridgeHightWithHook2 = 20,

            NewSkinClimbingWall = 27,
            NewSkinTower = 31,
        }

        /// <summary>
        /// keep owner of component
        /// </summary>
        private AbstractGameKeep m_abstractKeep;
        /// <summary>
        /// keep owner of component
        /// </summary>
        public AbstractGameKeep AbstractKeep
        {
            get { return m_abstractKeep; }
            set { m_abstractKeep = value; }
        }

        public IGameKeep Keep
        {
            get { return (IGameKeep)m_abstractKeep; }
            set { m_abstractKeep = (AbstractGameKeep)value; }
        }

        /// <summary>
        /// id of keep component id keep
        /// </summary>
        private int m_id;
        /// <summary>
        /// id of keep component id keep
        /// </summary>
        public int ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// height of keep grow with level
        /// </summary>
        public int Height
        {
            get { return GameServer.KeepManager.GetHeightFromLevel(AbstractKeep.Level); }
        }

        /// <summary>
        /// skin of keep component (wall, tower, ...)
        /// </summary>
        private int m_skin;

        public int Skin
        {
            get { return m_skin; }
            set { m_skin = value; }
        }

        public bool Climbing
        {
            get
            {
                if (ServerProperties.Properties.ALLOW_TOWER_CLIMB)
                {
                    if (m_skin == (int)eComponentSkin.Wall || m_skin == (int)eComponentSkin.NewSkinClimbingWall || m_skin == (int)eComponentSkin.Tower || m_skin == (int)eComponentSkin.NewSkinTower && !AbstractKeep.IsPortalKeep)
                    {
                        return true;
                    }
                }
                else
                {
                    if (m_skin == (int)eComponentSkin.Wall || m_skin == (int)eComponentSkin.NewSkinClimbingWall && !AbstractKeep.IsPortalKeep)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// relative X to keep
        /// </summary>
        private int m_componentx;
        /// <summary>
        /// relative X to keep
        /// </summary>
        public int ComponentX
        {
            get { return m_componentx; }
            set { m_componentx = value; }
        }

        /// <summary>
        /// relative Y to keep
        /// </summary>
        private int m_componenty;
        /// <summary>
        /// relative Y to keep
        /// </summary>
        public int ComponentY
        {
            get { return m_componenty; }
            set { m_componenty = value; }
        }

        /// <summary>
        /// relative heading to keep ( 0, 1, 2, 3)
        /// </summary>
        private int m_componentHeading;
        /// <summary>
        /// relative heading to keep ( 0, 1, 2, 3)
        /// </summary>
        public int ComponentHeading
        {
            get { return m_componentHeading; }
            set { m_componentHeading = value; }
        }

        protected int m_oldMaxHealth;

        /// <summary>
        /// Level of component
        /// </summary>
        public override byte Level
        {
            get
            {
                // return (byte)(40 + Keep.Level);
                return (byte)(AbstractKeep.BaseLevel - 10 + (AbstractKeep.Level * 3));
            }
        }

        public override eRealm Realm
        {
            get
            {
                if (AbstractKeep != null)
                {
                    return AbstractKeep.Realm;
                }

                return eRealm.None;
            }
        }

        private Hashtable m_keepHookPoints;
        protected byte m_oldHealthPercent;
        protected bool m_isRaized;

        public Hashtable KeepHookPoints
        {
            get { return m_keepHookPoints; }
            set { m_keepHookPoints = value; }
        }

        public IDictionary<int, GameKeepHookPoint> HookPoints
        {
            get
            {
                Dictionary<int, GameKeepHookPoint> dict = new Dictionary<int, GameKeepHookPoint>();
                foreach (DictionaryEntry item in m_keepHookPoints)
                {
                    dict.Add((int)item.Key, (GameKeepHookPoint)item.Value);
                }

                return dict;
            }

            set
            {
                Hashtable newHashTable = new Hashtable();
                foreach (KeyValuePair<int, GameKeepHookPoint> item in value)
                {
                    newHashTable.Add(item.Key, item.Value);
                }

                KeepHookPoints = newHashTable;
            }
        }

        private Hashtable m_positions;

        public Hashtable Positions
        {
            get { return m_positions; }
        }

        protected string m_CreateInfo = string.Empty;

        public override int RealmPointsValue
        {
            get
            {
                return 0;

                // if (IsRaized)
                //  return 0;

                // if (Skin == (int)eComponentSkin.Tower)
                // {
                //  return RepairedHealth / 100;
                // }

                // foreach (GameKeepComponent component in this.Keep.KeepComponents)
                // {
                //  if (component.IsAlive == false && component != this)
                //  {
                //      return MaxHealth / 100;
                //  }
                // }
                // return MaxHealth / 10;
            }
        }

        public override long ExperienceValue
        {
            get
            {
                return 0;
            }
        }

        public override int AttackRange
        {
            get { return 1000; }
        }

        public override IList GetExamineMessages(GamePlayer player)
        {
            IList list = base.GetExamineMessages(player);

            if (player.Client.Account.PrivLevel > 1)
            {
                list.Add(Name + " with a Z of " + Z.ToString());
            }

            return list;
        }

        /// <summary>
        /// Procs don't normally fire on game keep components
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public override bool AllowWeaponMagicalEffect(AttackData ad, InventoryItem weapon, Spell weaponSpell)
        {
            if (weapon.Flags == 10) // Bruiser or any other item needs Itemtemplate "Flags" set to 10 to proc on keep components
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// do not regen
        /// </summary>
        public override void StartHealthRegeneration()
        {
            m_repairTimer = new RegionTimer(CurrentRegion.TimeManager);
            m_repairTimer.Callback = new RegionTimerCallback(RepairTimerCallback);
            m_repairTimer.Interval = repairInterval;
            m_repairTimer.Start(1);
        }

        public virtual void RemoveTimers()
        {
            if (m_repairTimer != null)
            {
                m_repairTimer.Stop();
                m_repairTimer = null;
            }
        }

        /// <summary>
        /// constructor of component
        /// </summary>
        public GameKeepComponent()
        {
            m_keepHookPoints = new Hashtable(41);
            m_positions = new Hashtable();
        }

        /// <summary>
        /// load component from db object
        /// </summary>
        public virtual void LoadFromDatabase(DBKeepComponent component, AbstractGameKeep keep)
        {
            Region myregion = WorldMgr.GetRegion((ushort)keep.Region);
            if (myregion == null)
            {
                return;
            }

            AbstractKeep = keep;

            // this.DBKeepComponent = component;
            base.LoadFromDatabase(component);

            // this x and y is for get object in radius
            double angle = keep.Heading * ((Math.PI * 2) / 360); // angle*2pi/360;
            X = (int)(keep.X + ((sbyte)component.X * 148 * Math.Cos(angle) + (sbyte)component.Y * 148 * Math.Sin(angle)));
            Y = (int)(keep.Y - ((sbyte)component.Y * 148 * Math.Cos(angle) - (sbyte)component.X * 148 * Math.Sin(angle)));
            Z = keep.Z;

            // and this one for packet sent
            ComponentX = component.X;
            ComponentY = component.Y;
            ComponentHeading = (ushort)component.Heading;

            // need check to be sure for heading
            angle = component.Heading * 90 + keep.Heading;
            if (angle > 360)
            {
                angle -= 360;
            }

            Heading = (ushort)(angle / 0.08789);
            Name = keep.Name;
            Model = INVISIBLE_MODEL;
            Skin = component.Skin;
            m_oldMaxHealth = MaxHealth;
            Health = MaxHealth;

            // this.Health = component.Health;
            m_oldHealthPercent = HealthPercent;
            CurrentRegion = myregion;
            ID = component.ID;
            SaveInDB = false;
            IsRaized = false;
            LoadPositions();
            AddToWorld();
            FillPositions();
            RepairedHealth = MaxHealth;
            m_CreateInfo = component.CreateInfo;
            StartHealthRegeneration();
        }

        public virtual void LoadPositions()
        {
            ushort region = CurrentRegionID;
            if (CurrentRegion is BaseInstance)
            {
                region = (CurrentRegion as BaseInstance).Skin;
            }

            Battleground bg = GameServer.KeepManager.GetBattleground(region);

            Positions.Clear();

            string query = "`ComponentSkin` = '" + Skin + "'";
            if (Skin != (int)eComponentSkin.Keep && Skin != (int)eComponentSkin.Tower && Skin != (int)eComponentSkin.Gate)
            {
                query = query + " AND `ComponentRotation` = '" + ComponentHeading + "'";
            }

            if (bg != null)
            {
                // Battlegrounds, ignore all but GameKeepDoor
                query = query + " AND `ClassType` = 'DOL.GS.Keeps.GameKeepDoor'";
            }

            var DBPositions = GameServer.Database.SelectObjects<DBKeepPosition>(query);

            foreach (DBKeepPosition position in DBPositions)
            {
                DBKeepPosition[] list = Positions[position.TemplateID] as DBKeepPosition[];
                if (list == null)
                {
                    list = new DBKeepPosition[4];
                    Positions[position.TemplateID] = list;
                }

                list[position.Height] = position;
            }
        }

        public virtual void FillPositions()
        {
            foreach (DBKeepPosition[] positionGroup in Positions.Values)
            {
                for (int i = Height; i >= 0; i--)
                {
                    DBKeepPosition position = positionGroup[i] as DBKeepPosition;
                    if (position != null)
                    {
                        bool create = false;
                        if (position.ClassType == "DOL.GS.Keeps.GameKeepBanner")
                        {
                            if (AbstractKeep.Banners[position.TemplateID] == null)
                            {
                                create = true;
                            }
                        }
                        else if (position.ClassType == "DOL.GS.Keeps.GameKeepDoor")
                        {
                            if (AbstractKeep.Doors[position.TemplateID] == null)
                            {
                                create = true;
                            }
                        }
                        else if (position.ClassType == "DOL.GS.Keeps.FrontierTeleportStone")
                        {
                            if (AbstractKeep.TeleportStone == null)
                            {
                                create = true;
                            }
                        }
                        else if (position.ClassType == "DOL.GS.Keeps.Patrol")
                        {
                            if (position.KeepType == (int)AbstractGameKeep.eKeepType.Any || position.KeepType == (int)AbstractKeep.KeepType)
                            {
                                if (AbstractKeep.Patrols[position.TemplateID] == null)
                                {
                                    Patrol p = new Patrol(this);
                                    p.SpawnPosition = position;
                                    p.PatrolID = position.TemplateID;
                                    p.InitialiseGuards();
                                }
                            }

                            continue;
                        }
                        else
                        {
                            if (AbstractKeep.Guards[position.TemplateID] == null)
                            {
                                create = true;
                            }
                        }

                        if (create)
                        {
                            // create the object
                            try
                            {
                                Assembly asm = Assembly.GetExecutingAssembly();
                                IKeepItem obj = (IKeepItem)asm.CreateInstance(position.ClassType, true);
                                if (obj != null)
                                {
                                    obj.LoadFromPosition(position, this);
                                }

                                if (ServerProperties.Properties.ENABLE_DEBUG)
                                {
                                    if (obj is GameLiving)
                                    {
                                        (obj as GameLiving).Name += " is living, component " + obj.Component.ID;
                                    }
                                    else if (obj is GameObject)
                                    {
                                        (obj as GameObject).Name += " is object, component " + obj.Component.ID;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("GameKeepComponent:FillPositions: " + position.ClassType, ex);
                            }
                        }
                        else
                        {
                            // move the object
                            if (position.ClassType == "DOL.GS.Keeps.GameKeepBanner")
                            {
                                IKeepItem banner = AbstractKeep.Banners[position.TemplateID] as IKeepItem;
                                if (banner.Position != position)
                                {
                                    banner.MoveToPosition(position);
                                }
                            }
                            else if (position.ClassType == "DOL.GS.Keeps.GameKeepDoor")
                            {
                                // doors dont move
                            }
                            else if (position.ClassType == "DOL.GS.Keeps.FrontierPortalStone")
                            {
                                // these dont move
                            }
                            else
                            {
                                IKeepItem guard = AbstractKeep.Guards[position.TemplateID] as IKeepItem;
                                guard.MoveToPosition(position);
                            }
                        }

                        break;
                    }
                }
            }

            foreach (GameKeepGuard guard in AbstractKeep.Guards.Values)
            {
                if (guard.PatrolGroup != null)
                {
                    continue;
                }

                if (guard.HookPoint != null)
                {
                    continue;
                }

                if (guard.Position == null)
                {
                    continue;
                }

                if (guard.Position.Height > guard.Component.Height)
                {
                    guard.RemoveFromWorld();
                }
                else
                {
                    if (guard.Position.Height <= guard.Component.Height &&
                        guard.ObjectState != GameObject.eObjectState.Active && !guard.IsRespawning)
                    {
                        guard.AddToWorld();
                    }
                }
            }

            foreach (GameKeepBanner banner in AbstractKeep.Banners.Values)
            {
                if (banner.Position == null)
                {
                    continue;
                }

                if (banner.Position.Height > banner.Component.Height)
                {
                    banner.RemoveFromWorld();
                }
                else
                {
                    if (banner.Position.Height <= banner.Component.Height &&
                        banner.ObjectState != GameObject.eObjectState.Active)
                    {
                        banner.AddToWorld();
                    }
                }
            }
        }

        /// <summary>
        /// save component in DB
        /// </summary>
        public override void SaveIntoDatabase()
        {
            DBKeepComponent obj = null;
            bool New = false;
            if (InternalID != null)
            {
                obj = GameServer.Database.FindObjectByKey<DBKeepComponent>(InternalID);
            }

            if (obj == null)
            {
                obj = new DBKeepComponent();
                New = true;
            }

            obj.KeepID = AbstractKeep.KeepID;
            obj.Heading = ComponentHeading;
            obj.Health = Health;
            obj.X = ComponentX;
            obj.Y = ComponentY;
            obj.ID = ID;
            obj.Skin = Skin;
            obj.CreateInfo = m_CreateInfo;

            if (New)
            {
                GameServer.Database.AddObject(obj);
                InternalID = obj.ObjectId;
                log.DebugFormat("Added new component {0} for keep ID {1}, skin {2}, health {3}", ID, AbstractKeep.KeepID, Skin, Health);
            }
            else
            {
                GameServer.Database.SaveObject(obj);
            }

            base.SaveIntoDatabase();
        }

        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
        {
            if (damageAmount > 0)
            {
                AbstractKeep.LastAttackedByEnemyTick = CurrentRegion.Time;
                base.TakeDamage(source, damageType, damageAmount, criticalAmount);

                // only on hp change
                if (m_oldHealthPercent != HealthPercent)
                {
                    m_oldHealthPercent = HealthPercent;
                    foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
                    {
                        client.Out.SendObjectUpdate(this);
                        client.Out.SendKeepComponentDetailUpdate(this); // I knwo this works, not sure if ObjectUpdate is needed - Tolakram
                    }
                }
            }
        }

        public override void ModifyAttack(AttackData attackData)
        {
            // Allow a GM to use commands to damage components, regardless of toughness setting
            if (attackData.DamageType == eDamageType.GM)
            {
                return;
            }

            int toughness = ServerProperties.Properties.SET_STRUCTURES_TOUGHNESS;
            int baseDamage = attackData.Damage;
            int styleDamage = attackData.StyleDamage;
            int criticalDamage = 0;

            GameLiving source = attackData.Attacker;

            if (source is GamePlayer)
            {
                baseDamage = (baseDamage - (baseDamage * 5 * AbstractKeep.Level / 100)) * toughness / 100;
                styleDamage = (styleDamage - (styleDamage * 5 * AbstractKeep.Level / 100)) * toughness / 100;
            }
            else if (source is GameNPC)
            {
                if (!ServerProperties.Properties.STRUCTURES_ALLOWPETATTACK)
                {
                    baseDamage = 0;
                    styleDamage = 0;
                    attackData.AttackResult = eAttackResult.NotAllowed_ServerRules;
                }
                else
                {
                    baseDamage = (baseDamage - (baseDamage * 5 * AbstractKeep.Level / 100)) * toughness / 100;
                    styleDamage = (styleDamage - (styleDamage * 5 * AbstractKeep.Level / 100)) * toughness / 100;

                    if (((GameNPC)source).Brain is DOL.AI.Brain.IControlledBrain)
                    {
                        GamePlayer player = ((DOL.AI.Brain.IControlledBrain)((GameNPC)source).Brain).Owner as GamePlayer;
                        if (player != null)
                        {
                            // special considerations for pet spam classes
                            if (player.CharacterClass.ID == (int)eCharacterClass.Theurgist || player.CharacterClass.ID == (int)eCharacterClass.Animist)
                            {
                                baseDamage = (int)(baseDamage * ServerProperties.Properties.PET_SPAM_DAMAGE_MULTIPLIER);
                                styleDamage = (int)(styleDamage * ServerProperties.Properties.PET_SPAM_DAMAGE_MULTIPLIER);
                            }
                            else
                            {
                                baseDamage = (int)(baseDamage * ServerProperties.Properties.PET_DAMAGE_MULTIPLIER);
                                styleDamage = (int)(styleDamage * ServerProperties.Properties.PET_DAMAGE_MULTIPLIER);
                            }
                        }
                    }
                }
            }

            attackData.Damage = baseDamage;
            attackData.StyleDamage = styleDamage;
            attackData.CriticalDamage = criticalDamage;
        }

        public override void Die(GameObject killer)
        {
            base.Die(killer);
            if (AbstractKeep is GameKeepTower)
            {
                if (IsRaized == false)
                {
                    Notify(KeepEvent.TowerRaized, AbstractKeep, new KeepEventArgs(AbstractKeep, killer.Realm));
                    PlayerMgr.BroadcastRaize(AbstractKeep, killer.Realm);
                    IsRaized = true;

                    foreach (GameKeepGuard guard in AbstractKeep.Guards.Values)
                    {
                        guard.MoveTo(guard.CurrentRegionID, guard.X, guard.Y, AbstractKeep.Z, guard.Heading);
                        guard.SpawnPoint.Z = AbstractKeep.Z;
                    }
                }
            }

            foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
            {
                client.Out.SendKeepComponentDetailUpdate(this);
            }
        }

        public override void Delete()
        {
            StopHealthRegeneration();
            RemoveTimers();
            KeepHookPoints.Clear();
            Positions.Clear();
            AbstractKeep = null;
            base.Delete();
            CurrentRegion = null;
        }

        /// <summary>
        /// Remove a component and delete it from the database
        /// </summary>
        public virtual void Remove()
        {
            Delete();
            DBKeepComponent obj = null;
            if (InternalID != null)
            {
                obj = GameServer.Database.FindObjectByKey<DBKeepComponent>(InternalID);
            }

            if (obj != null)
            {
                GameServer.Database.DeleteObject(obj);
            }

            log.Warn("Keep Component deleted from database: " + obj.ID);

            // todo find a packet to remove the keep
        }

        /// <summary>
        /// IComparable.CompareTo implementation.
        /// </summary>
        public int CompareTo(object obj)
        {
            if (obj is GameKeepComponent)
            {
                return ID - ((GameKeepComponent)obj).ID;
            }
            else
            {
                return 0;
            }
        }

        public virtual byte Status
        {
            get
            {
                if (AbstractKeep is GameKeepTower)
                {
                    if (m_isRaized)
                    {
                        if (HealthPercent >= 25)
                        {
                            IsRaized = false;
                        }
                        else
                        {
                            return 0x02;
                        }
                    }

                    if (HealthPercent < 35)
                    {
                        return 0x01;// broken
                    }
                }

                if (AbstractKeep is GameKeep)
                {
                    if (!IsAlive)
                    {
                        return 0x01;// broken
                    }
                }

                return 0x00;
            }
        }

        public virtual void UpdateLevel()
        {
            if ((IsRaized == false) && (MaxHealth != m_oldMaxHealth))
            {
                if (m_oldMaxHealth > 0)
                {
                    Health = (int)Math.Ceiling(((double)Health) * ((double)MaxHealth) / ((double)m_oldMaxHealth));
                }
                else
                {
                    Health = MaxHealth;
                }

                m_oldMaxHealth = MaxHealth;
            }
        }

        public virtual bool IsRaized
        {
            get { return m_isRaized; }

            set
            {
                RepairedHealth = 0;
                m_isRaized = value;
                if (value == true)
                {
                    if (AbstractKeep.Level > 1)
                    {
                        AbstractKeep.ChangeLevel(1);
                    }
                }
                else
                {
                    FillPositions();
                }
            }
        }

        public int RepairedHealth = 0;

        protected RegionTimer m_repairTimer;
        protected static int repairInterval = 30 * 60 * 1000;

        public virtual int RepairTimerCallback(RegionTimer timer)
        {
            if (HealthPercent == 100 || AbstractKeep.InCombat)
            {
                return repairInterval;
            }

            Repair((MaxHealth / 100) * 5);
            return repairInterval;
        }

        public virtual void Repair(int amount)
        {
            if (amount > 0)
            {
                byte oldStatus = Status;
                Health += amount;
                m_oldHealthPercent = HealthPercent;
                if (oldStatus != Status)
                {
                    foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
                    {
                        client.Out.SendKeepComponentDetailUpdate(this);
                    }
                }

                // if a tower is repaired reload the guards so they arent on the floor
                if (AbstractKeep is GameKeepTower && oldStatus == 0x02 && oldStatus != Status)
                {
                    foreach (GameKeepComponent component in AbstractKeep.KeepComponents)
                    {
                        component.FillPositions();
                    }
                }

                RepairedHealth = Health;
            }
        }

        public override string ToString()
        {
            if (AbstractKeep == null)
            {
                return "Keep is null!";
            }

            return new StringBuilder(base.ToString())
                .Append(" ComponentID=").Append(ID)
                .Append(" Skin=").Append(Skin)
                .Append(" Height=").Append(Height)
                .Append(" Heading=").Append(Heading)
                .Append(" nComponentX=").Append((sbyte)ComponentX)
                .Append(" ComponentY=").Append((sbyte)ComponentY)
                .Append(" ComponentHeading=").Append(ComponentHeading)
                .ToString();
        }
    }
}
