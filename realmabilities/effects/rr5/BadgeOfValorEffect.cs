using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.GS.PropertyCalc;
using DOL.Events;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Effect handler for Arms Length
    /// </summary>
    public class BadgeOfValorEffect : TimedEffect
    {
        //http://daocpedia.de/index.php/Abzeichen_des_Mutes    

        /// <summary>
        /// Default constructor for AmelioratingMelodiesEffect
        /// </summary>
		public BadgeOfValorEffect()
			: base(20000)
		{

		}

        /// <summary>
        /// Called when effect is to be started
        /// </summary>
        /// <param name="player">The player to start the effect for</param>
        /// <param name="duration">The effectduration in secounds</param>
        /// <param name="value">The percentage additional value for melee absorb</param>
        public override void Start(GameLiving living)
        {
			base.Start(living);
            GameEventMgr.AddHandler(m_owner, GamePlayerEvent.AttackFinished, new DOLEventHandler(AttackFinished));
        }



        /// <summary>
        /// Called when a player is inflicted in an combat action
        /// </summary>
        /// <param name="e">The event which was raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">EventArgs associated with the event</param>
        private void AttackFinished(DOLEvent e, object sender, EventArgs args)
        {
            AttackFinishedEventArgs afea = (AttackFinishedEventArgs)args;
            
            if (m_owner != afea.AttackData.Attacker || afea.AttackData.AttackType == AttackData.eAttackType.Spell)
                return;
            //only affect this onto players
            if (!(afea.AttackData.Target is GamePlayer))
                return;
            GamePlayer target = afea.AttackData.Target as GamePlayer;
            
            Database.InventoryItem armor = target.Inventory.GetItem((eInventorySlot)((int)afea.AttackData.ArmorHitLocation));
            
            if (armor == null || armor.SPD_ABS == 0)
                return;
            //cap at 50%
            int bonusPercent = Math.Min(armor.SPD_ABS,50);
                        

            //add 2times percentual of abs, one time will be substracted later
            afea.AttackData.Damage = (int)(armor.SPD_ABS*(bonusPercent*2 + 100)*0.01 );

        }

        /// <summary>
        /// Called when effect is to be cancelled
        /// </summary>
        /// <param name="playerCancel">Whether or not effect is player cancelled</param>
        public override void Stop()
        {
			base.Stop();
            GameEventMgr.RemoveHandler(m_owner, GamePlayerEvent.AttackFinished, new DOLEventHandler(AttackFinished));
        }


        /// <summary>
        /// Name of the effect
        /// </summary>
        public override string Name
        {
            get
            {
                return "Badge of Valor";
            }
        }

        /// <summary>
        /// Icon ID
        /// </summary>
        public override UInt16 Icon
        {
            get
            {
                return 3056;
            }
        }

        /// <summary>
        /// Delve information
        /// </summary>
        public override IList DelveInfo
        {
            get
            {
                IList delveInfoList = new ArrayList(10);
                delveInfoList.Add("Melee damage for the next 20 seconds will be INCREASED by the targets armor-based ABS instead of decreased.");
                delveInfoList.Add(" ");

                int seconds = (int) RemainingTime/1000;
                if (seconds > 0)
                {
                    delveInfoList.Add(" ");
                    delveInfoList.Add("- " + seconds + " seconds remaining.");
                }

                return delveInfoList;
            }
        }
    }
}