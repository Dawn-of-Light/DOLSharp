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
using System.Collections;
using DOL.Events;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Effect handler for Fanatacism
    /// </summary>
    public class FanatacismEffect : TimedEffect
    {
 		private GamePlayer EffectOwner;
 		
        public FanatacismEffect()
            : base(RealmAbilities.FanatacismAbility.DURATION)
        { }    

         public override void Start(GameLiving target)
        {
        	base.Start(target);
            if (target is GamePlayer)
            {
                EffectOwner = target as GamePlayer;
                foreach (GamePlayer p in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(EffectOwner, p, 7088, 0, false, 1);
                }
                GameEventMgr.AddHandler(EffectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            	EffectOwner.BuffBonusCategory1[(int)eProperty.MagicAbsorbtion] += RealmAbilities.FanatacismAbility.VALUE;
            }
        }

        public override void Stop()
        {
            if (EffectOwner != null)
            {
            	EffectOwner.BuffBonusCategory1[(int)eProperty.MagicAbsorbtion] -= RealmAbilities.FanatacismAbility.VALUE;
                GameEventMgr.RemoveHandler(EffectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            }
            
            base.Stop();
        }

        /// <summary>
        /// Called when a player leaves the game
        /// </summary>
        /// <param name="e">The event which was raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">EventArgs associated with the event</param>
        protected void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
        {
  			Cancel(false);
        }

        public override string Name { get { return "Fanatacism"; } }
        public override ushort Icon { get { return 7088; } }

        // Delve Info
        public override IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add("Grants a reduction in all spell damage taken for 45 seconds.");
                return list;
            }
        }
    }
}
