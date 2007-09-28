using System;
using System.Collections;
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.AI.Brain
{
    public class TurretBrain : ControlledNpc, IControlledBrain
    {
        private GameLiving m_target;
        private Spell m_spell;

        public TurretBrain(GameLiving owner, Spell spell)
            : base(owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            m_spell = spell;
        }

        #region Accessors

        public override int ThinkInterval
        {
            get { return 500; }
        }

        public GameLiving Target
        {
            get { return m_target; }
            set { m_target = value; }
        }

        public Spell Spell
        {
            get { return m_spell; }
            set { m_spell = value; }
        }

        #endregion

        #region AI

        private IList FindTarget(bool friends)
        {
            ArrayList list = new ArrayList();

            foreach (GamePlayer p in Body.GetPlayersInRadius((ushort)Spell.Range))
            {
                if (!friends && GameServer.ServerRules.IsAllowedToAttack(Body, p, true) && !p.IsStealthed)
                    list.Add(p);
                else if (friends && GameServer.ServerRules.IsSameRealm(Body, p, true) && !p.IsStealthed)
                    list.Add(p);
            }

            foreach (GameNPC n in Body.GetNPCsInRadius((ushort)Spell.Range))
            {
                if (!friends && GameServer.ServerRules.IsAllowedToAttack(Body, n, true))
                    list.Add(n);
                else if (friends && GameServer.ServerRules.IsSameRealm(Body, n, true) && (n.Brain is IControlledBrain || n.Brain is GuardBrain))
                    list.Add(n);
            }
            return list;
        }

        public override bool Start()
        {
            if (!base.Start()) return false;
            return true;
        }

        public override bool Stop()
        {
            if (!base.Stop()) return false;
            return true;
        }
        #endregion

        #region Think
        public override void Think()
        {
            if (Body.IsCasting)
                return;

            if (Spell.Target.ToLower() == "realm")
            {
                if (Spell.Range == 0 && Spell.Radius > 0)
                {
                    Body.CastSpell(Spell, SkillBase.GetSpellLine("Reserved Spells"));
                    return;
                }
                else
                {
                    IList friendly = new ArrayList();
                    if (Target == null)
                        friendly = FindTarget(true);
                    else if (Target == Body)
                        friendly = FindTarget(true);
                    else if (!WorldMgr.CheckDistance(Body, Target, Spell.Range))
                        friendly = FindTarget(true);
                    else if (!Target.IsAlive)
                        friendly = FindTarget(true);
                    else if (Target is GameNPC && (Target as GameNPC).IsRespawning == true)
                        friendly = FindTarget(true);
                    else if (Target is GamePlayer && Target.IsStealthed)
                        friendly = FindTarget(true);

                    if (friendly.Count > 0 && Target == null)
                    {
                        //pick a random target...
                        int targetnum = Util.Random(0, friendly.Count - 1);

                        //Choose a random target.
                        Target = friendly[targetnum] as GameLiving;
                    }
                    if (Target != null)
                    {
                        if (!Target.IsAlive)
                            Target = null;
                        if (Target is GamePlayer && Target.IsStealthed)
                            Target = null;
                        //Console.WriteLine(Target.Name);
                        if (Target != null && WorldMgr.CheckDistance(Body, Target, Spell.Range) && !LivingHasEffect(Target, Spell))
                        {
                            if (!Target.IsAlive)
                                Target = null;

                            Body.TargetObject = Target;
                            Body.TurnTo(Target);
                            GamePlayer LOSChecker = null;
                            if (Target is GamePlayer)
                                LOSChecker = Target as GamePlayer;
                            else if (Target is GameNPC)
                            {
                                foreach (GamePlayer ply in this.Body.GetPlayersInRadius(300))
                                {
                                    if (ply != null)
                                    {
                                        LOSChecker = ply;
                                        break;
                                    }
                                }
                            }
                            if (LOSChecker == null)
                                return;
                            LOSChecker.Out.SendCheckLOS(LOSChecker, Body, new CheckLOSResponse(this.PetStartSpellAttackCheckLOS));
                        }
                        else
                            Target = null;
                    }
                    return;
                }
            }
            else if (Spell.Target.ToLower() == "enemy")
            {
                if (Spell.Range == 0 && Spell.Radius > 0)
                {
                    Body.CastSpell(Spell, SkillBase.GetSpellLine("Reserved Spells"));
                    return;
                }
                else
                {
                    IList enemies = new ArrayList();
                    if (Target == null)
                        enemies = FindTarget(false);
                    else if (Target.Health == 0)
                        FindTarget(false);
                    else if (!WorldMgr.CheckDistance(Body, Target, Spell.Range))
                        enemies = FindTarget(false);
                    else if (!Target.IsAlive)
                        enemies = FindTarget(false);
                    else if (Target is GamePlayer && Target.IsStealthed)
                        enemies = FindTarget(false);
                    if (enemies.Count > 0 && Target == null)
                    {
                        //pick a random target...
                        int targetnum = Util.Random(0, enemies.Count - 1);

                        //Choose a random target.
                        Target = enemies[targetnum] as GameLiving;
                    }
                    if (Target != null)
                    {
                        if (!Target.IsAlive)
                            Target = null;
                        if (Target is GamePlayer && Target.IsStealthed)
                            Target = null;
                        else if (Target != null && WorldMgr.CheckDistance(Body, Target, Spell.Range) && !LivingHasEffect(Target, Spell))
                        {
                            //Cast Spell
                            Body.TargetObject = Target;
                            Body.TurnTo(Target);

                            GamePlayer LOSChecker = null;
                            if (Target is GamePlayer)
                                LOSChecker = Target as GamePlayer;
                            else if (Target is GameNPC)
                            {
                                foreach (GamePlayer ply in this.Body.GetPlayersInRadius(300))
                                {
                                    if (ply != null)
                                    {
                                        LOSChecker = ply;
                                        break;
                                    }
                                }
                            }

                            if (LOSChecker == null)
                                return;

                            LOSChecker.Out.SendCheckLOS(LOSChecker, Body, new CheckLOSResponse(PetStartSpellAttackCheckLOS));
                        }
                        else
                            Target = null;
                    }
                    return;
                }
            }
        }

        public void PetStartSpellAttackCheckLOS(GamePlayer player, ushort response, ushort targetOID)
        {
            if ((response & 0x100) == 0x100)
                Body.CastSpell(Spell, SkillBase.GetSpellLine("Reserved Spells"));
        }
        #endregion
    }
}