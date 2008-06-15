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
using DOL.Database;
using DOL.Events;
using DOL.GS.Spells;

namespace DOL.GS.Effects
{

    public class BoilingCauldronEffect : TimedEffect
    {
        // Parameters
        private const int cauldronModel = 2947; 		// Model to use for cauldron
        private const int cauldronLevel = 50;			// Cauldron level		
        private const string cauldronName = "Cauldron";	// Name of cauldron
        private const int spellDamage = 450;			// Damage inflicted
        private const ushort spellRadius = 350;			// Spell radius

        // Objects
        private GamePlayer EffectOwner;				// Effect owner
        private GameStaticItem Cauldron;					// The cauldron

        public BoilingCauldronEffect()
            : base(RealmAbilities.BoilingCauldronAbility.DURATION)
        { }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GamePlayer)
            {
                EffectOwner = target as GamePlayer;
                foreach (GamePlayer p in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(EffectOwner, EffectOwner, 7086, 0, false, 1);
                }
                SummonCauldron();
                GameEventMgr.AddHandler(EffectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            }
        }
        public override void Stop()
        {
            if (Cauldron != null) { Cauldron.Delete(); Cauldron = null; }
            if (EffectOwner != null)
                GameEventMgr.RemoveHandler(EffectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

            base.Stop();
        }

        // Summon the cauldron
        private void SummonCauldron()
        {
            Cauldron = new GameStaticItem();
            Cauldron.CurrentRegion = EffectOwner.CurrentRegion;
            Cauldron.Heading = (ushort)((EffectOwner.Heading + 2048) % 4096);
            Cauldron.Level = cauldronLevel;
            Cauldron.Realm = EffectOwner.Realm;
            Cauldron.Name = cauldronName;
            Cauldron.Model = cauldronModel;
            Cauldron.X = EffectOwner.X;
            Cauldron.Y = EffectOwner.Y;
            Cauldron.Z = EffectOwner.Z;
            Cauldron.AddToWorld();

            new RegionTimer(EffectOwner, new RegionTimerCallback(CauldronCallBack), RealmAbilities.BoilingCauldronAbility.DURATION - 1000);
        }

        private int CauldronCallBack(RegionTimer timer)
        {
            if (Cauldron != null && EffectOwner != null)
            {
                foreach (GamePlayer target in Cauldron.GetPlayersInRadius(spellRadius))
                {
                    if (GameServer.ServerRules.IsAllowedToAttack(EffectOwner, target, true))
                        target.TakeDamage(EffectOwner, eDamageType.Heat, spellDamage, 0);
                }
            }
            timer.Stop();
            timer = null;
            return 0;
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

        public override string Name { get { return "Boiling Cauldron"; } }
        public override ushort Icon { get { return 3085; } }

        // Delve Info
        public override IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add("Cauldron that boil in place for 5s before spilling and doing damage to all those nearby.");
                return list;
            }
        }
    }
}

