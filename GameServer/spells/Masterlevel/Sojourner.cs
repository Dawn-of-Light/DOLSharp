using System;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using DOL.Events;

namespace DOL.GS.Spells
{
    // http://www.camelotherald.com/masterlevels/ma.php?ml=Sojourner
    // no shared timer
    // Gameplayer - MaxEncumbrance

    // ML2 Unending Breath - already handled in another area

    // ML3 Reveal Crystalseed - already handled in another area

    // no shared timer
    [SpellHandler("UnmakeCrystalseed")]
    public class UnmakeCrystalseedSpellHandler : SpellHandler
    {
        /// <summary>
        /// Execute unmake crystal seed spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
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
            {
                return;
            }

            foreach (GameNPC item in target.GetNPCsInRadius((ushort)Spell.Radius))
            {
                (item as GameMine)?.Delete();
            }
        }

        public UnmakeCrystalseedSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // no shared timer
    [SpellHandler("AncientTransmuter")]
    public class AncientTransmuterSpellHandler : SpellHandler
    {
        private readonly GameMerchant merchant;
        /// <summary>
        /// Execute Acient Transmuter summon spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if (effect.Owner == null || !effect.Owner.IsAlive)
            {
                return;
            }

            merchant.AddToWorld();
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            merchant?.Delete();

            return base.OnEffectExpires(effect, noMessages);
        }

        public AncientTransmuterSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            if (caster is GamePlayer casterPlayer)
            {
                merchant = new GameMerchant
                {
                    X = casterPlayer.X + Util.Random(20, 40) - Util.Random(20, 40),
                    Y = casterPlayer.Y + Util.Random(20, 40) - Util.Random(20, 40),
                    Z = casterPlayer.Z,
                    CurrentRegion = casterPlayer.CurrentRegion,
                    Heading = (ushort) ((casterPlayer.Heading + 2048) % 4096),
                    Level = 1,
                    Realm = casterPlayer.Realm,
                    Name = "Ancient Transmuter",
                    Model = 993,
                    CurrentSpeed = 0,
                    MaxSpeedBase = 0,
                    GuildName = string.Empty,
                    Size = 50
                };

                merchant.Flags |= GameNPC.eFlags.PEACE;
                merchant.TradeItems = new MerchantTradeItems("ML_transmuteritems");
            }
        }
    }

    // no shared timer
    [SpellHandler("Port")]
    public class Port : MasterlevelHandling
    {
        // constructor
        public Port(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null)
            {
                return;
            }

            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            if (Caster is GamePlayer player)
            {
                if (!player.InCombat && !GameRelic.IsPlayerCarryingRelic(player))
                {
                    SendEffectAnimation(player, 0, false, 1);
                    player.MoveToBind();
                }
            }
        }
    }

    // no shared timer
    [SpellHandler("EssenceResist")]
    public class EssenceResistHandler : AbstractResistBuff
    {
        public override eBuffBonusCategory BonusCategory1 => eBuffBonusCategory.BaseBuff;

        public override eProperty Property1 => eProperty.Resist_Natural;

        public EssenceResistHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // no shared timer
    [SpellHandler("Zephyr")]
    public class FZSpellHandler : MasterlevelHandling
    {
        protected RegionTimer m_expireTimer;
        protected GameNPC m_npc;
        protected GamePlayer m_target;
        protected IPoint3D m_loc;

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target is GamePlayer player && player.IsAlive)
            {
                Zephyr(player);
            }
        }

        public override bool CheckBeginCast(GameLiving target)
        {
            if (target == null)
            {
                MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
                return false;
            }

            if (target is GameNPC)
            {
                return false;
            }

            if (!GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
            {
                return false;
            }

            return base.CheckBeginCast(target);
        }

        private void Zephyr(GamePlayer target)
        {
            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            GameNPC npc = new GameNPC();

            m_npc = npc;

            npc.Realm = Caster.Realm;
            npc.Heading = Caster.Heading;
            npc.Model = 1269;
            npc.Y = Caster.Y;
            npc.X = Caster.X;
            npc.Z = Caster.Z;
            npc.Name = "Forceful Zephyr";
            npc.MaxSpeedBase = 400;
            npc.Level = 55;
            npc.CurrentRegion = Caster.CurrentRegion;
            npc.Flags |= GameNPC.eFlags.PEACE;
            npc.Flags |= GameNPC.eFlags.DONTSHOWNAME;
            npc.Flags |= GameNPC.eFlags.CANTTARGET;
            BlankBrain brain = new BlankBrain();
            npc.SetOwnBrain(brain);
            npc.AddToWorld();
            npc.TempProperties.setProperty("target", target);
            GameEventMgr.AddHandler(npc, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(ArriveAtTarget));
            npc.Follow(target, 10, 1500);

            m_target = target;

            StartTimer();
        }

        protected virtual void StartTimer()
        {
            StopTimer();
            m_expireTimer = new RegionTimer(m_npc, new RegionTimerCallback(ExpiredCallback), 10000);
        }

        protected virtual int ExpiredCallback(RegionTimer callingTimer)
        {
            m_target.IsStunned = false;
            m_target.DismountSteed(true);
            m_target.DebuffCategory[(int)eProperty.SpellFumbleChance] -= 100;
            GameEventMgr.RemoveHandler(m_target, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            m_npc.StopMoving();
            m_npc.RemoveFromWorld();

            // sometimes player can't move after zephyr :
            m_target.Out.SendUpdateMaxSpeed();
            return 0;
        }

        protected virtual void StopTimer()
        {

            if (m_expireTimer != null)
            {
                m_expireTimer.Stop();
                m_expireTimer = null;
            }
        }

        private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GameLiving))
            {
                return;
            }

            AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
            AttackData ad = null;
            if (attackedByEnemy != null)
            {
                ad = attackedByEnemy.AttackData;
            }

            double absorbPercent = 100;
            int damageAbsorbed = (int)(0.01 * absorbPercent * (ad.Damage + ad.CriticalDamage));
            int spellAbsorbed = (int)(0.01 * absorbPercent * Spell.Damage);

            ad.Damage -= damageAbsorbed;
            ad.Damage -= spellAbsorbed;

            MessageToLiving(ad.Target, "You\'re in a Zephyr and can\'t be attacked!", eChatType.CT_Spell);
            MessageToLiving(ad.Attacker, "Your target is in a Zephyr and can\'t be attacked!", eChatType.CT_Spell);
        }

        private void ArriveAtTarget(DOLEvent e, object obj, EventArgs args)
        {
            if (!(obj is GameNPC npc))
            {
                return;
            }

            if (!(npc.TempProperties.getProperty<object>("target", null) is GamePlayer target) || !target.IsAlive)
            {
                return;
            }

            GameEventMgr.RemoveHandler(npc, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(ArriveAtTarget));

            if (!(target is GamePlayer player))
            {
                return;
            }

            if (!player.IsAlive)
            {
                return;
            }

            player.IsStunned = true;

            // player.IsSilenced = true;
            player.DebuffCategory[(int)eProperty.SpellFumbleChance] += 100;
            player.StopAttack();
            player.StopCurrentSpellcast();
            player.MountSteed(npc, true);
            GameEventMgr.AddHandler(player, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));

            player.Out.SendMessage("You are picked up by a forceful zephyr!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            npc.StopFollowing();

            if (Caster is GamePlayer)
            {
                // Calculate random target
                m_loc = GetTargetLoc();
                (Caster as GamePlayer)?.Out.SendCheckLOS(Caster as GamePlayer, m_npc, new CheckLOSResponse(ZephyrCheckLOS));
            }
        }

        public void ZephyrCheckLOS(GamePlayer player, ushort response, ushort targetOID)
        {
            if ((response & 0x100) == 0x100)
            {
                m_npc.WalkTo(m_loc.X, m_loc.Y, m_loc.Z, 100);
            }
        }

        public virtual IPoint3D GetTargetLoc()
        {
            double targetX = m_npc.X + Util.Random(-1500, 1500);
            double targetY = m_npc.Y + Util.Random(-1500, 1500);

            return new Point3D((int)targetX, (int)targetY, m_npc.Z);
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public FZSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // no shared timer
    [SpellHandler("Phaseshift")]
    public class PhaseshiftHandler : MasterlevelHandling
    {
        private int endurance;

        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            endurance = (Caster.MaxEndurance * 50) / 100;

            if (Caster.Endurance < endurance)
            {
                MessageToCaster("You need 50% endurance for this spell!!", eChatType.CT_System);
                return false;
            }

            return base.CheckBeginCast(selectedTarget);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            Caster.Endurance -= endurance;
        }

        private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GameLiving living))
            {
                return;
            }

            AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
            AttackData ad = null;
            if (attackedByEnemy != null)
            {
                ad = attackedByEnemy.AttackData;
            }

            if (ad.Attacker is GamePlayer player)
            {
                ad.Damage = 0;
                ad.CriticalDamage = 0;
                player.Out.SendMessage($"{living.Name} is Phaseshifted and can\'t be attacked!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            return base.OnEffectExpires(effect, noMessages);
        }

        public override bool HasPositiveEffect => false;

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        // constructor
        public PhaseshiftHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    // no shared timer
    [SpellHandler("Groupport")]
    public class Groupport : MasterlevelHandling
    {
        public Groupport(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (Caster is GamePlayer player && Caster.CurrentRegionID == 51 && player.BindRegion == 51)
            {
                if (Caster.CurrentRegionID == 51)
                {
                    MessageToCaster("You can't use this Ability here", eChatType.CT_SpellResisted);
                    return false;
                }

                MessageToCaster("Bind in another Region to use this Ability", eChatType.CT_SpellResisted);
                return false;
            }

            return base.CheckBeginCast(selectedTarget);
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null)
            {
                return;
            }

            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            GamePlayer player = Caster as GamePlayer;
            if ((player != null) && (player.Group != null))
            {
                if (player.Group.IsGroupInCombat())
                {
                    player.Out.SendMessage("You can't teleport a group that is in combat!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }

                foreach (GamePlayer pl in player.Group.GetPlayersInTheGroup())
                {
                    if (pl != null)
                    {
                        SendEffectAnimation(pl, 0, false, 1);
                        pl.MoveTo((ushort)player.BindRegion, player.BindXpos, player.BindYpos, player.BindZpos, (ushort)player.BindHeading);
                    }
                }
            }
            else
            {
                player.Out.SendMessage("You are not a part of a group!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }
    }
}
