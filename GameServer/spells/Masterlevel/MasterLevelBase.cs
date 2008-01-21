using System;
using System.Text;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.ServerRules;
using DOL.Database2;

namespace DOL.GS.Spells
{
    #region MasterlevelBase
    /// <summary>
    /// Contains all common code for Banelord Spells
    /// </summary>
    public class MasterlevelHandling : SpellHandler
    {
        public override bool HasPositiveEffect
        {
            get { return false; }
        }

        public override bool IsUnPurgeAble
        {
            get { return true; }
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        /// <summary>
        /// Calculates the effect duration in milliseconds
        /// </summary>
        /// <param name="target">The effect target</param>
        /// <param name="effectiveness">The effect effectiveness</param>
        /// <returns>The effect duration in milliseconds</returns>
        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            return Spell.Duration;
        }

        #region Targets
        /// <summary>
        /// Select all targets for this spell
        /// </summary>
        /// <param name="castTarget"></param>
        /// <returns></returns>
        public override IList SelectTargets(GameObject castTarget)
        {
            ArrayList list = new ArrayList(8);
            GameLiving target = castTarget as GameLiving;

            switch (Spell.Target.ToLower())
            {
                //GTAoE
                case "area":
                    if (Spell.Radius > 0)
                    {
                        foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, (ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                    }
                    break;

                case "pet":
                    if (Caster is GamePlayer)
                    {
                        IControlledBrain npc = ((GamePlayer)Caster).ControlledNpc;
                        if (npc != null)
                            list.Add(npc.Body);
                    }
                    break;

                case "enemy":
                    if (Spell.Radius > 0)
                    {
                        target = Caster;
                        foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                    }
                    else
                    {
                        if (target != null && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                            list.Add(target);
                    }
                    break;

                case "realm":
                    if (Spell.Radius > 0)
                    {
                        if (target == null || Spell.Range == 0)
                            target = Caster;
                        foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsSameRealm(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                    }
                    else
                    {
                        if (target != null && GameServer.ServerRules.IsSameRealm(Caster, target, true))
                            list.Add(target);
                    }
                    break;

                case "self":
                    {
                        if (Spell.Radius > 0)
                        {
                            if (target == null || Spell.Range == 0)
                                target = Caster;
                            foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                            {
                                if (GameServer.ServerRules.IsSameRealm(Caster, player, true))
                                {
                                    list.Add(player);
                                }
                            }
                        }
                        else
                        {
                            list.Add(Caster);
                        }
                        break;
                    }
				case "group":
					{
						Group group = m_caster.Group;
						int spellRange = CalculateSpellRange();
						if (spellRange == 0)
							spellRange = m_spell.Radius;
						if (group == null)
						{
							list.Add(m_caster);
							IControlledBrain npc = m_caster.ControlledNpc;
							if (npc != null)
							{
								if (WorldMgr.CheckDistance(m_caster, npc.Body, spellRange))
									list.Add(npc.Body);
							}
						}
						else
						{
							lock (group)
							{
								foreach (GameLiving living in group)
								{
									// only players in range
									if (WorldMgr.CheckDistance(m_caster, living, spellRange))
										list.Add(living);

									IControlledBrain npc = living.ControlledNpc;
									if (npc != null)
									{
										if (WorldMgr.CheckDistance(living, npc.Body, spellRange))
											list.Add(npc.Body);
									}
								}
							}
						}
						break;
					}
            }
            return list;
        }
        #endregion

        /// <summary>
        /// Current depth of delve info
        /// </summary>
        public override IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add(Spell.Description);
                return list;
            }
        }

        public MasterlevelHandling(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
    }
    #endregion

    #region MasterlevelDebuff
    /// <summary>
    /// Contains all common code for Banelord Spells
    /// </summary>
    public abstract class MasterlevelDebuffHandling : SingleStatDebuff
    {
        // bonus category
        public override int BonusCategory1 { get { return 3; } }

        public override bool HasPositiveEffect
        {
            get { return false; }
        }

        public override bool IsUnPurgeAble
        {
            get { return true; }
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        /// <summary>
        /// Calculates the effect duration in milliseconds
        /// </summary>
        /// <param name="target">The effect target</param>
        /// <param name="effectiveness">The effect effectiveness</param>
        /// <returns>The effect duration in milliseconds</returns>
        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            return Spell.Duration;
        }

        #region Targets
        /// <summary>
        /// Select all targets for this spell
        /// </summary>
        /// <param name="castTarget"></param>
        /// <returns></returns>
        public override IList SelectTargets(GameObject castTarget)
        {
            ArrayList list = new ArrayList(8);
            GameLiving target = castTarget as GameLiving;

            switch (Spell.Target.ToLower())
            {
                //GTAoE
                case "area":
                    if (Spell.Radius > 0)
                    {
                        foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, (ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                        foreach (GameNPC npc in WorldMgr.GetNPCsCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, (ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
                            {
                                list.Add(npc);
                            }
                        }
                    }
                    break;

                case "corpse":
                    if (target != null && !target.IsAlive)
                        list.Add(target);
                    break;

                case "pet":
                    if (Caster is GamePlayer)
                    {
                        IControlledBrain npc = ((GamePlayer)Caster).ControlledNpc;
                        if (npc != null)
                            list.Add(npc.Body);
                    }
                    break;

                case "enemy":
                    if (Spell.Radius > 0)
                    {
                        target = Caster;
                        foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                    }
                    else
                    {
                        if (target != null && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                            list.Add(target);
                    }
                    break;

                case "realm":
                    if (Spell.Radius > 0)
                    {
                        if (target == null || Spell.Range == 0)
                            target = Caster;
                        foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsSameRealm(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                    }
                    else
                    {
                        if (target != null && GameServer.ServerRules.IsSameRealm(Caster, target, true))
                            list.Add(target);
                    }
                    break;

                case "self":
                    {
                        if (Spell.Radius > 0)
                        {
                            if (target == null || Spell.Range == 0)
                                target = Caster;
                            foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                            {
                                if (GameServer.ServerRules.IsSameRealm(Caster, player, true))
                                {
                                    list.Add(player);
                                }
                            }
                        }
                        else
                        {
                            list.Add(Caster);
                        }
                        break;
                    }
				case "group":
					{
						Group group = m_caster.Group;
						int spellRange = CalculateSpellRange();
						if (spellRange == 0)
							spellRange = m_spell.Radius;
						if (group == null)
						{
							list.Add(m_caster);
							IControlledBrain npc = m_caster.ControlledNpc;
							if (npc != null)
							{
								if (WorldMgr.CheckDistance(m_caster, npc.Body, spellRange))
									list.Add(npc.Body);
							}
						}
						else
						{
							lock (group)
							{
								foreach (GameLiving living in group)
								{
									// only players in range
									if (WorldMgr.CheckDistance(m_caster, living, spellRange))
										list.Add(living);

									IControlledBrain npc = living.ControlledNpc;
									if (npc != null)
									{
										if (WorldMgr.CheckDistance(m_caster, npc.Body, spellRange))
											list.Add(npc.Body);
									}
								}
							}
						}
						break;
					}
            }
            return list;
        }
        #endregion

        /// <summary>
        /// Current depth of delve info
        /// </summary>
        public override IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add(Spell.Description);
                return list;
            }
        }

        public MasterlevelDebuffHandling(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
    }
    #endregion

    #region MasterlevelBuff
    /// <summary>
    /// Contains all common code for Banelord Spells
    /// </summary>
    public abstract class MasterlevelBuffHandling : SingleStatBuff
    {
        // bonus category
        public override int BonusCategory1 { get { return 1; } }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        /// <summary>
        /// Calculates the effect duration in milliseconds
        /// </summary>
        /// <param name="target">The effect target</param>
        /// <param name="effectiveness">The effect effectiveness</param>
        /// <returns>The effect duration in milliseconds</returns>
        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            return Spell.Duration;
        }

        #region Targets
        /// <summary>
        /// Select all targets for this spell
        /// </summary>
        /// <param name="castTarget"></param>
        /// <returns></returns>
        public override IList SelectTargets(GameObject castTarget)
        {
            ArrayList list = new ArrayList(8);
            GameLiving target = castTarget as GameLiving;

            switch (Spell.Target.ToLower())
            {
                //GTAoE
                case "area":
                    if (Spell.Radius > 0)
                    {
                        foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, (ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                        foreach (GameNPC npc in WorldMgr.GetNPCsCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, (ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
                            {
                                list.Add(npc);
                            }
                        }
                    }
                    break;

                case "corpse":
                    if (target != null && !target.IsAlive)
                        list.Add(target);
                    break;

                case "pet":
                    if (Caster is GamePlayer)
                    {
                        IControlledBrain npc = ((GamePlayer)Caster).ControlledNpc;
                        if (npc != null)
                            list.Add(npc.Body);
                    }
                    break;

                case "enemy":
                    if (Spell.Radius > 0)
                    {
                        target = Caster;
                        foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                    }
                    else
                    {
                        if (target != null && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                            list.Add(target);
                    }
                    break;

                case "realm":
                    if (Spell.Radius > 0)
                    {
                        if (target == null || Spell.Range == 0)
                            target = Caster;
                        foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsSameRealm(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                    }
                    else
                    {
                        if (target != null && GameServer.ServerRules.IsSameRealm(Caster, target, true))
                            list.Add(target);
                    }
                    break;

                case "self":
                    {
                        if (Spell.Radius > 0)
                        {
                            if (target == null || Spell.Range == 0)
                                target = Caster;
                            foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                            {
                                if (GameServer.ServerRules.IsSameRealm(Caster, player, true))
                                {
                                    list.Add(player);
                                }
                            }
                        }
                        else
                        {
                            list.Add(Caster);
                        }
                        break;
                    }
                case "group":
                    {
						Group group = m_caster.Group;
						int spellRange = CalculateSpellRange();
						if (spellRange == 0)
							spellRange = m_spell.Radius;
						if (group == null)
						{
							list.Add(m_caster);
							IControlledBrain npc = m_caster.ControlledNpc;
							if (npc != null)
							{
								if (WorldMgr.CheckDistance(m_caster, npc.Body, spellRange))
									list.Add(npc.Body);
							}
						}
						else
						{
							lock (group)
							{
								foreach (GameLiving living in group)
								{
									// only players in range
									if (WorldMgr.CheckDistance(m_caster, living, spellRange))
										list.Add(living);

									IControlledBrain npc = living.ControlledNpc;
									if (npc != null)
									{
										if (WorldMgr.CheckDistance(living, npc.Body, spellRange))
											list.Add(npc.Body);
									}
								}
							}
						}
                        break;
                    }
            }
            return list;
        }
        #endregion

        /// <summary>
        /// Current depth of delve info
        /// </summary>
        public override IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add(Spell.Description);
                return list;
            }
        }

        public MasterlevelBuffHandling(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
    }
    #endregion

    #region MasterlevelDualBuff
    /// <summary>
    /// Contains all common code for Banelord Spells
    /// </summary>
    public abstract class MasterlevelDualBuffHandling : DualStatBuff
    {
        // bonus category
        public override int BonusCategory1 { get { return 1; } }
        public override int BonusCategory2 { get { return 2; } }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        /// <summary>
        /// Calculates the effect duration in milliseconds
        /// </summary>
        /// <param name="target">The effect target</param>
        /// <param name="effectiveness">The effect effectiveness</param>
        /// <returns>The effect duration in milliseconds</returns>
        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            return Spell.Duration;
        }

        #region Targets
        /// <summary>
        /// Select all targets for this spell
        /// </summary>
        /// <param name="castTarget"></param>
        /// <returns></returns>
        public override IList SelectTargets(GameObject castTarget)
        {
            ArrayList list = new ArrayList(8);
            GameLiving target = castTarget as GameLiving;

            switch (Spell.Target.ToLower())
            {
                //GTAoE
                case "area":
                    if (Spell.Radius > 0)
                    {
                        foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, (ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                        foreach (GameNPC npc in WorldMgr.GetNPCsCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, (ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
                            {
                                list.Add(npc);
                            }
                        }
                    }
                    break;

                case "corpse":
                    if (target != null && !target.IsAlive)
                        list.Add(target);
                    break;

                case "pet":
                    if (Caster is GamePlayer)
                    {
                        IControlledBrain npc = ((GamePlayer)Caster).ControlledNpc;
                        if (npc != null)
                            list.Add(npc.Body);
                    }
                    break;

                case "enemy":
                    if (Spell.Radius > 0)
                    {
                        target = Caster;
                        foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                    }
                    else
                    {
                        if (target != null && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                            list.Add(target);
                    }
                    break;

                case "realm":
                    if (Spell.Radius > 0)
                    {
                        if (target == null || Spell.Range == 0)
                            target = Caster;
                        foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsSameRealm(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                    }
                    else
                    {
                        if (target != null && GameServer.ServerRules.IsSameRealm(Caster, target, true))
                            list.Add(target);
                    }
                    break;

                case "self":
                    {
                        if (Spell.Radius > 0)
                        {
                            if (target == null || Spell.Range == 0)
                                target = Caster;
                            foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                            {
                                if (GameServer.ServerRules.IsSameRealm(Caster, player, true))
                                {
                                    list.Add(player);
                                }
                            }
                        }
                        else
                        {
                            list.Add(Caster);
                        }
                        break;
                    }
                case "group":
                    {
						Group group = m_caster.Group;
						int spellRange = CalculateSpellRange();
						if (spellRange == 0)
							spellRange = m_spell.Radius;
						if (group == null)
						{
							list.Add(m_caster);
							IControlledBrain npc = m_caster.ControlledNpc;
							if (npc != null)
							{
								if (WorldMgr.CheckDistance(m_caster, npc.Body, spellRange))
									list.Add(npc.Body);
							}
						}
						else
						{
							lock (group)
							{
								foreach (GameLiving living in group)
								{
									// only players in range
									if (WorldMgr.CheckDistance(m_caster, living, spellRange))
										list.Add(living);

									IControlledBrain npc = living.ControlledNpc;
									if (npc != null)
									{
										if (WorldMgr.CheckDistance(living, npc.Body, spellRange))
											list.Add(npc.Body);
									}
								}
							}
						}
                        break;
                    }
            }
            return list;
        }
        #endregion

        /// <summary>
        /// Current depth of delve info
        /// </summary>
        public override IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add(Spell.Description);
                return list;
            }
        }

        public MasterlevelDualBuffHandling(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
    }
    #endregion

    #region BanelordSnare
    /// <summary>
    /// Contains all common code for Banelord Spells
    /// </summary>
    public class BanelordSnare : UnbreakableSpeedDecreaseSpellHandler
    {
        public override bool HasPositiveEffect
        {
            get { return false; }
        }

        public override bool IsUnPurgeAble
        {
            get { return true; }
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameTimer timer = (GameTimer)effect.Owner.TempProperties.getObjectProperty(effect, null);
            effect.Owner.TempProperties.removeProperty(effect);
            timer.Stop();

            effect.Owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, effect);

            SendUpdates(effect.Owner);
            MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_SpellExpires, effect.Owner);

            return 0;
        }

        /// <summary>
        /// Calculates the effect duration in milliseconds
        /// </summary>
        /// <param name="target">The effect target</param>
        /// <param name="effectiveness">The effect effectiveness</param>
        /// <returns>The effect duration in milliseconds</returns>
        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            return Spell.Duration;
        }

        #region Targets
        /// <summary>
        /// Select all targets for this spell
        /// </summary>
        /// <param name="castTarget"></param>
        /// <returns></returns>
        public override IList SelectTargets(GameObject castTarget)
        {
            ArrayList list = new ArrayList(8);
            GameLiving target = castTarget as GameLiving;

            switch (Spell.Target.ToLower())
            {
                //GTAoE
                case "area":
                    if (Spell.Radius > 0)
                    {
                        foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, (ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                        foreach (GameNPC npc in WorldMgr.GetNPCsCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, (ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
                            {
                                list.Add(npc);
                            }
                        }
                    }
                    break;

                case "corpse":
                    if (target != null && !target.IsAlive)
                        list.Add(target);
                    break;

                case "pet":
                    if (Caster is GamePlayer)
                    {
                        IControlledBrain npc = ((GamePlayer)Caster).ControlledNpc;
                        if (npc != null)
                            list.Add(npc.Body);
                    }
                    break;

                case "enemy":
                    if (Spell.Radius > 0)
                    {
                        target = Caster;
                        foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                    }
                    else
                    {
                        if (target != null && GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                            list.Add(target);
                    }
                    break;

                case "realm":
                    if (Spell.Radius > 0)
                    {
                        if (target == null || Spell.Range == 0)
                            target = Caster;
                        foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                        {
                            if (GameServer.ServerRules.IsSameRealm(Caster, player, true))
                            {
                                list.Add(player);
                            }
                        }
                    }
                    else
                    {
                        if (target != null && GameServer.ServerRules.IsSameRealm(Caster, target, true))
                            list.Add(target);
                    }
                    break;

                case "self":
                    {
                        if (Spell.Radius > 0)
                        {
                            if (target == null || Spell.Range == 0)
                                target = Caster;
                            foreach (GamePlayer player in target.GetPlayersInRadius((ushort)Spell.Radius))
                            {
                                if (GameServer.ServerRules.IsSameRealm(Caster, player, true))
                                {
                                    list.Add(player);
                                }
                            }
                        }
                        else
                        {
                            list.Add(Caster);
                        }
                        break;
                    }
                case "group":
                    {
						Group group = m_caster.Group;
						int spellRange = CalculateSpellRange();
						if (spellRange == 0)
							spellRange = m_spell.Radius;
						if (group == null)
						{
							list.Add(m_caster);
							IControlledBrain npc = m_caster.ControlledNpc;
							if (npc != null)
							{
								if (WorldMgr.CheckDistance(m_caster, npc.Body, spellRange))
									list.Add(npc.Body);
							}
						}
						else
						{
							lock (group)
							{
								foreach (GameLiving living in group)
								{
									// only players in range
									if (WorldMgr.CheckDistance(m_caster, living, spellRange))
										list.Add(living);

									IControlledBrain npc = living.ControlledNpc;
									if (npc != null)
									{
										if (WorldMgr.CheckDistance(living, npc.Body, spellRange))
											list.Add(npc.Body);
									}
								}
							}
						}
                        break;
                    }
            }
            return list;
        }
        #endregion

        /// <summary>
        /// Current depth of delve info
        /// </summary>
        public override IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add(Spell.Description);
                return list;
            }
        }

        public BanelordSnare(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
    }
    #endregion

    #region Fontbase
    public class FontSpellHandler : DoTSpellHandler
    {
        protected GameFont font;
        protected DBSpell dbs;
        protected Spell s;
        protected SpellLine sl;
        protected ISpellHandler heal;
        protected bool ApplyOnNPC = false;
        protected bool ApplyOnCombat = false;
        protected bool Friendly = true;
        protected ushort sRadius = 350;

        public override bool IsOverwritable(GameSpellEffect compare)
        {
            return false;
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);
            font.AddToWorld();
            neweffect.Start(font);
        }

        public override void OnEffectPulse(GameSpellEffect effect)
        {
            if (font == null || font.ObjectState == GameObject.eObjectState.Deleted)
            {
                effect.Cancel(false);
                return;
            }

            if (heal == null || s == null) return;
            foreach (GamePlayer player in font.GetPlayersInRadius(sRadius))
            {
                if (!Friendly
                    && player.IsAlive
                    && GameServer.ServerRules.IsAllowedToAttack(Caster, player, true)
                    && (!player.InCombat
                    || ApplyOnCombat))
                        heal.StartSpell((GameLiving)player);
                else if (Friendly && player.IsAlive && (!player.InCombat || ApplyOnCombat))
                    heal.StartSpell((GameLiving)player);
            }
            if (!ApplyOnNPC) return;
            foreach (GameNPC npc in font.GetNPCsInRadius(sRadius))
            {
                if (!Friendly && npc.IsAlive && GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true) && (!npc.InCombat || ApplyOnCombat))
                    heal.StartSpell((GameLiving)npc);
                if (Friendly && npc.IsAlive && (!npc.InCombat || ApplyOnCombat))
                    heal.StartSpell((GameLiving)npc);
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (font != null) font.Delete();
            effect.Owner.EffectList.Remove(effect);
            return base.OnEffectExpires(effect, noMessages);
        }

        // constructor
        public FontSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }	
    #endregion

    #region Trapbase
    public class MineSpellHandler : DoTSpellHandler
    {
        protected GameMine mine;
        protected ISpellHandler trap;
        protected GameSpellEffect m_effect;
        protected DBSpell dbs;
        protected Spell s;
        protected SpellLine sl;
        protected bool Unstealth = true;
        protected bool DestroyOnEffect = true;
        protected ushort sRadius = 350;
        
        public override bool IsOverwritable(GameSpellEffect compare)
        {
            return false;
        }
        public override void OnEffectPulse(GameSpellEffect effect)
        {
            if (mine == null || mine.ObjectState == GameObject.eObjectState.Deleted)
            {
                effect.Cancel(false);
                return;
            }

            if (trap == null || s == null) return;
            bool wasstealthed = ((GamePlayer)Caster).IsStealthed;
            foreach (GamePlayer player in mine.GetPlayersInRadius(sRadius))
            {
                if (player.IsAlive && GameServer.ServerRules.IsAllowedToAttack(Caster, player, true))
                {
                    trap.StartSpell((GameLiving)player);
                    if (!Unstealth) 
                        ((GamePlayer)Caster).Stealth(wasstealthed);
                    if (DestroyOnEffect) 
                        OnEffectExpires(effect, true);
                    return;
                }
            }
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);
            mine.AddToWorld();
            neweffect.Start(mine);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            if (mine == null) return;
            base.OnEffectStart(effect);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (mine != null) mine.Delete();
            effect.Owner.EffectList.Remove(effect);
            return base.OnEffectExpires(effect, noMessages);
        }
        // constructor
        public MineSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    #region Stormbase
    public class StormSpellHandler : DoTSpellHandler
    {
        protected GameStorm storm;
        protected DBSpell dbs;
        protected Spell s;
        protected SpellLine sl;
        protected ISpellHandler tempest;
        protected ushort sRadius = 350;

        public override void OnEffectPulse(GameSpellEffect effect)
        {
            if (storm == null || storm.ObjectState == GameObject.eObjectState.Deleted)
            {
                effect.Cancel(false);
                return;
            }
            if (tempest == null || s == null)
            {
                return;
            }
            foreach (GamePlayer player in storm.GetPlayersInRadius(sRadius))
            {
                if (player.IsAlive) tempest.StartSpell(player);
            }
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);
            storm.AddToWorld();
            neweffect.Start(storm);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (storm != null) storm.Delete();
            effect.Owner.EffectList.Remove(effect);
            return base.OnEffectExpires(effect, noMessages);
        }

        // constructor
        public StormSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    #region SummonItemBase
    public class SummonItemSpellHandler : MasterlevelHandling
    {
        protected InventoryItem item;
        /// <summary>
        /// Execute create item spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= CalculateNeededPower(target);
            base.FinishSpellCast(target);
        }

        public override bool HasPositiveEffect
        {
            get { return true; }
        }

        /// <summary>
        /// execute non duration spell effect on target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            if (target == null || !target.IsAlive)
                return;

            if (target is GamePlayer && item != null)
            {
                GamePlayer targetPlayer = target as GamePlayer;
                if (targetPlayer.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
                {
                    targetPlayer.Out.SendMessage("Item created: " + item.GetName(0, false), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }

        }

        public SummonItemSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }	
    #endregion

    #region TargetModifier
    [SpellHandlerAttribute("TargetModifier")]
    public class TargetModifierSpellHandler : MasterlevelHandling
    {
        public override bool HasPositiveEffect
        {
            get { return true; }
        }
        public TargetModifierSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    #region Passive
    [SpellHandlerAttribute("PassiveSpell")]
    public class PassiveSpellHandler : MasterlevelHandling
    {
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            return false;
        }

        public PassiveSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion
}

namespace DOL.GS
{
    #region Decoy
    public class GameDecoy : GameNPC
    {
        public GameDecoy()
        {
            SetOwnBrain(new BlankBrain());
            this.MaxSpeedBase = 0;
        }
        public override void Die(GameObject killer)
        {
            DeleteFromDatabase();
            Delete();
        }
        private GamePlayer m_owner;
        public GamePlayer Owner
        {
            get { return m_owner; }
            set { m_owner = value; }
        }
        public override int MaxHealth
        {
            get { return 1; }
        }
    }
    #endregion

    #region Gamefont
    public class GameFont : GameMovingObject
    {
        public GameFont()
        {
            SetOwnBrain(new BlankBrain());
            this.Realm = 0;
            this.Level = 1;
            this.MaxSpeedBase = 0;
            this.Flags |= (uint)GameNPC.eFlags.DONTSHOWNAME;
            this.Health = this.MaxHealth;
            //this.Flags |= (uint) GameNPC.eFlags.PEACE;		
        }

        private GamePlayer m_owner;
        public GamePlayer Owner
        {
            get { return m_owner; }
            set { m_owner = value; }
        }
        public override int MaxHealth
        {
            get { return 1000; }
        }

        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
        {
            if (damageType == eDamageType.Slash || damageType == eDamageType.Crush || damageType == eDamageType.Thrust)
            {
                damageAmount /= 10;
                criticalAmount /= 10;
            }
            else
            {
                damageAmount /= 25;
                criticalAmount /= 25;
            }
            base.TakeDamage(source, damageType, damageAmount, criticalAmount);
        }

        public override void Die(GameObject killer)
        {
            DeleteFromDatabase();
            Delete();
        }
    }
    #endregion

    #region Gametrap
    public class GameMine : GameMovingObject
    {
        public GameMine()
        {
            this.Realm = 0;
            this.Level = 1;
            this.Health = this.MaxHealth;
            this.CurrentSpeed = 0;
        }

        private GamePlayer m_owner;
        public GamePlayer Owner
        {
            get { return m_owner; }
            set { m_owner = value; }
        }

        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
        {
            if (source is GamePlayer)
            {
                damageAmount = 0;
                criticalAmount = 0;
            }
            if (Health - damageAmount - criticalAmount <= 0)
                this.Delete();
            else
                Health = Health - damageAmount - criticalAmount;

        }
    }
    #endregion

    #region Gamestorm
    public class GameStorm : GameMovingObject
    {
        public GameStorm()
        {
            SetOwnBrain(new BlankBrain());
            this.Realm = 0;
            this.Level = 60;
            this.MaxSpeedBase = 191;
            this.Model = 3457;
            this.Name = "Storm";
            this.Flags |= (uint)GameNPC.eFlags.DONTSHOWNAME;
            //this.Flags |= (uint)GameNPC.eFlags.PEACE;
            //this.Flags |= (uint) GameNPC.eFlags.CANTTARGET;
            this.Movable = true;
        }

        private GamePlayer m_owner;
        public GamePlayer Owner
        {
            get { return m_owner; }
            set { m_owner = value; }
        }

        private bool m_movable;
        public bool Movable
        {
            get { return m_movable; }
            set { m_movable = value; }
        }

        public override int MaxHealth
        { 
            get { return 10000; } 
        }

        public override void Die(GameObject killer)
        {
            DeleteFromDatabase();
            Delete();
        }

        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
        {
        }
    }
    #endregion
}
