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
using DOL.Database;
using DOL.Events;
using DOL.GS.Spells;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Minion Rescue
    /// </summary>
    public class SearingPetEffect : TimedEffect
    {
        private const int Duration = 19 * 1000;

        // Parameters
        private const ushort SpellRadius = 350;         // Radius of the RA
        private const int SpellDamage = 25;         // pbaoe damage
        private const int SpellFrequency = 3;       // pbaoe pulse frequency

        // Objects
        private readonly Spell _petSpell;             // The spell to cast
        private readonly SpellLine _petSpellLine;         // The spell line
        private readonly GamePlayer _effectOwner;         // Owner of the effect
        private ISpellHandler _pbaoe;                    // The Spell handler
        private GameNPC _pet;                    // The pet
        private RegionTimer _pulseTimer;             // Pulse timer
        private int _currentTick;        // Count ticks

        public SearingPetEffect(GamePlayer owner)
            : base(Duration)
        {
            _effectOwner = owner;

            // Build spell
            DBSpell tSpell = new DBSpell
            {
                AllowAdd = false,
                Description = "Damage the target.",
                Name = "PBAoE damage",
                Target = "Enemy",
                Radius = 0,
                Range = WorldMgr.VISIBILITY_DISTANCE,
                CastTime = 0,
                Duration = 0,
                Frequency = 0,
                Pulse = 0,
                Uninterruptible = true,
                Type = "DirectDamage",
                Damage = SpellDamage,
                DamageType = (int) eDamageType.Heat,
                Value = 0,
                Icon = 476,
                ClientEffect = 476
            };

            _petSpell = new Spell(tSpell, 1);
            _petSpellLine = new SpellLine("RAs", "RealmAbilitys", "RealmAbilitys", true);
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GameNPC npc)
            {
                _pet = npc;
                _pbaoe = ScriptMgr.CreateSpellHandler(_effectOwner, _petSpell, _petSpellLine);
                _pulseTimer = new RegionTimer(_effectOwner, new RegionTimerCallback(PulseTimer), 1000);
                GameEventMgr.AddHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            }
        }

        public override void Stop()
        {
            if (_pulseTimer != null)
            {
                _pulseTimer.Stop(); _pulseTimer = null;
            }

            if (_effectOwner != null)
            {
                GameEventMgr.RemoveHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            }

            base.Stop();
        }

        protected virtual int PulseTimer(RegionTimer timer)
        {
            if (_effectOwner == null || _pet == null || _pbaoe == null)
            {
                timer.Stop();
                return 0;
            }

            if (_currentTick % SpellFrequency == 0)
            {
                foreach (GamePlayer target in _pet.GetPlayersInRadius(SpellRadius))
                {
                    _pbaoe.StartSpell(target);
                }

                foreach (GameNPC npc in _pet.GetNPCsInRadius(SpellRadius))
                {
                    _pbaoe.StartSpell(npc);
                }
            }

            _currentTick++;
            return 1000;
        }

        /// <summary>
        /// Called when a player leaves the game
        /// </summary>
        /// <param name="e">The event which was raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">EventArgs associated with the event</param>
        private static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
        {
            if (sender is GamePlayer player && player.ControlledBrain?.Body != null)
            {
                GameNPC pet = player.ControlledBrain.Body;
                SearingPetEffect searingPet = pet.EffectList.GetOfType<SearingPetEffect>();
                searingPet?.Cancel(false);
            }
        }

        public override string Name => "Searing pet";

        public override ushort Icon => 7064;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "PBAoE Pet pulsing effect."
                };

                return list;
            }
        }
    }
}
