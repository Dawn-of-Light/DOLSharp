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
using DOL.Events;

namespace DOL.GS.Effects
{

    public class BoilingCauldronEffect : TimedEffect
    {
        // Parameters
        private const int CauldronModel = 2947;         // Model to use for cauldron
        private const int CauldronLevel = 50;           // Cauldron level
        private const string CauldronName = "Cauldron"; // Name of cauldron
        private const int SpellDamage = 650;            // Damage inflicted
        private const ushort SpellRadius = 350;         // Spell radius
        private const int Duration = 4500;

        // Objects
        private GamePlayer _effectOwner;             // Effect owner
        private GameStaticItem _cauldron;                    // The cauldron

        public BoilingCauldronEffect()
            : base(Duration)
        { }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GamePlayer player)
            {
                _effectOwner = player;
                foreach (GamePlayer p in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(_effectOwner, _effectOwner, 7086, 0, false, 1);
                }

                SummonCauldron()  ;
                GameEventMgr.AddHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            }
        }

        public override void Stop()
        {
            if (_cauldron != null) { _cauldron.Delete(); _cauldron = null; }
            if (_effectOwner != null)
            {
                GameEventMgr.RemoveHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            }

            base.Stop();
        }

        // Summon the cauldron
        private void SummonCauldron()
        {
            _cauldron = new GameStaticItem();
            _cauldron.CurrentRegion = _effectOwner.CurrentRegion;
            _cauldron.Heading = (ushort)((_effectOwner.Heading + 2048) % 4096);
            _cauldron.Level = CauldronLevel;
            _cauldron.Realm = _effectOwner.Realm;
            _cauldron.Name = CauldronName;
            _cauldron.Model = CauldronModel;
            _cauldron.X = _effectOwner.X;
            _cauldron.Y = _effectOwner.Y;
            _cauldron.Z = _effectOwner.Z;
            _cauldron.AddToWorld();

            new RegionTimer(_effectOwner, new RegionTimerCallback(CauldronCallBack), Duration - 1000);
        }

        private int CauldronCallBack(RegionTimer timer)
        {
            if (_cauldron != null && _effectOwner != null)
            {
                foreach (GamePlayer target in _cauldron.GetPlayersInRadius(SpellRadius))
                {
                    if (GameServer.ServerRules.IsAllowedToAttack(_effectOwner, target, true))
                    {
                        target.TakeDamage(_effectOwner, eDamageType.Heat, SpellDamage, 0);
                    }
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
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>();
                list.Add("Cauldron that boil in place for 5s before spilling and doing damage to all those nearby.");
                return list;
            }
        }
    }
}

