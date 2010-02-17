using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Effects;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Adrenaline Rush
    /// </summary>
    public class RetributionOfTheFaithfulStunEffect : TimedEffect
    {
		public RetributionOfTheFaithfulStunEffect()
			: base(3000)
		{ }

        private GameLiving owner;

        public override void Start(GameLiving target)
		{
            base.Start(target);
            owner = target;
            foreach (GamePlayer p in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                p.Out.SendSpellEffectAnimation(target, target, 7042, 0, false, 1);
            }
            owner.IsStunned = true;
            owner.StopAttack();
            owner.StopCurrentSpellcast();
            owner.DisableTurning(true);
            GamePlayer player = owner as GamePlayer;
            if (player != null)
            {
                player.Out.SendUpdateMaxSpeed();
            }
            else if(owner.CurrentSpeed > owner.MaxSpeed) 
            {
                owner.CurrentSpeed = owner.MaxSpeed;
            }
        }



        public override string Name { get { return "Retribution Of The Faithful"; } }

        public override ushort Icon { get { return 3041; } }

        public override void Stop()
        {
            owner.IsStunned = false;
            owner.DisableTurning(false);
            GamePlayer player = owner as GamePlayer;
            if (player != null)
            {
                player.Out.SendUpdateMaxSpeed();
            }
            else
            {
                owner.CurrentSpeed = owner.MaxSpeed;
            }
            base.Stop();

        }

        public int SpellEffectiveness
        {
            get { return 100; }
        }

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>();
                list.Add("Stuns you for the brief duration of 3 seconds");
                return list;
            }
        }
    }


    public class RetributionOfTheFaithfulEffect : TimedEffect
    {


        public RetributionOfTheFaithfulEffect()
            : base(30000)
        {
            ;
        }

        private GameLiving owner;

        public override void Start(GameLiving target)
        {
            base.Start(target);
            owner = target;
            GamePlayer player = target as GamePlayer;
            if (player != null)
            {
                foreach (GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(player, player, 7042, 0, false, 1);
                }
            }

            GameEventMgr.AddHandler(target, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
        }

        private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            if (arguments == null) return;
            AttackedByEnemyEventArgs args = arguments as AttackedByEnemyEventArgs;
            if (args == null) return;
            if (args.AttackData == null) return;
            if (!args.AttackData.IsMeleeAttack) return;
			//FIXME: [WARN] this has been commented out, it should be handled somewhere
			if (args.AttackData.Attacker.EffectList.GetOfType(typeof(ChargeEffect)) != null || args.AttackData.Attacker.TempProperties.getProperty("Charging", false))
				return;
            if ( !owner.IsWithinRadius( args.AttackData.Attacker, 300 ) ) return;
            if (Util.Chance(50))
            {
                RetributionOfTheFaithfulStunEffect effect = new RetributionOfTheFaithfulStunEffect();
                effect.Start(args.AttackData.Attacker);
            }
        }

        public override string Name { get { return "Retribution Of The Faithful"; } }

        public override ushort Icon { get { return 3041; } }

        public override void Stop()
        {
            GameEventMgr.RemoveHandler(owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            base.Stop();
        }

        public int SpellEffectiveness
        {
            get { return 100; }
        }

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>();
                list.Add("30 second buff that has a 50% chance to proc a 3 second (duration undiminished by resists) stun on any melee attack on the cleric.");
                return list;
            }
        }
    }
}
