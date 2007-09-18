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
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.Spells;
using DOL.GS.Scripts;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Minion Rescue
    /// </summary> 
    public class SearingPetEffect : TimedEffect
    {
        // Parameters
        private const ushort spellRadius = 350; 		// Radius of the RA
        private const int spellDamage = 25;			// pbaoe damage
        private const int spellFrequency = 3;		// pbaoe pulse frequency

        // Objects
        private Spell petSpell;				// The spell to cast
        private SpellLine petSpellLine;	 		// The spell line
        private ISpellHandler pbaoe;					// The Spell handler
        private GamePlayer EffectOwner;			// Owner of the effect
        private GameNPC pet;					// The pet
        private RegionTimer pulseTimer;				// Pulse timer
        private int currentTick = 0;		// Count ticks

        public SearingPetEffect(GamePlayer owner)
            : base(RealmAbilities.SearingPetAbility.DURATION)
        {
            EffectOwner = owner;

            // Build spell
            DBSpell tSpell = new DBSpell();
            tSpell.AutoSave = false;
            tSpell.Description = "Damage the target.";
            tSpell.Name = "PBAoE damage";
            tSpell.Target = "Enemy";
            tSpell.Radius = 0;
            tSpell.Range = WorldMgr.VISIBILITY_DISTANCE;
            tSpell.CastTime = 0;
            tSpell.Duration = 0;
            tSpell.Frequency = 0;
            tSpell.Pulse = 0;
            tSpell.Uninterruptible = true;
            tSpell.Type = "DirectDamage";
            tSpell.Damage = spellDamage;
            tSpell.DamageType = (int)eDamageType.Heat;
            tSpell.Value = 0;
            tSpell.Icon = 476;			// not official effect
            tSpell.ClientEffect = 476;	// not official effect
            petSpell = new Spell(tSpell, 1);
            petSpellLine = new SpellLine("RAs", "RealmAbilitys", "RealmAbilitys", true);
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GameNPC)
            {
                pet = target as GameNPC;
                pbaoe = ScriptMgr.CreateSpellHandler(EffectOwner, petSpell, petSpellLine);
                pulseTimer = new RegionTimer(EffectOwner, new RegionTimerCallback(PulseTimer), 1000);
                GameEventMgr.AddHandler(EffectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
            }
        }
        public override void Stop()
        {
            if (pulseTimer != null) { pulseTimer.Stop(); pulseTimer = null; }
            if (EffectOwner != null)
                GameEventMgr.RemoveHandler(EffectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

            base.Stop();
        }
        protected virtual int PulseTimer(RegionTimer timer)
        {
            if (EffectOwner == null || pet == null || pbaoe == null)
            {
                timer.Stop();
                timer = null;
                return 0;
            }
            if (currentTick % spellFrequency == 0)
            {
                foreach (GamePlayer target in pet.GetPlayersInRadius(spellRadius))
                {
                    pbaoe.StartSpell(target);
                }
                foreach (GameNPC npc in pet.GetNPCsInRadius(spellRadius))
                {
                    pbaoe.StartSpell(npc);
                }
            }
            currentTick++;
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
            GamePlayer player = sender as GamePlayer;
            if (player != null && player.ControlledNpc != null && player.ControlledNpc.Body != null)
            {
                GameNPC pet = player.ControlledNpc.Body as GameNPC;
                SearingPetEffect SearingPet = (SearingPetEffect)pet.EffectList.GetOfType(typeof(SearingPetEffect));
                if (SearingPet != null)
                    SearingPet.Cancel(false);
            }
        }

        public override string Name { get { return "Searing pet"; } }
        public override ushort Icon { get { return 7064; } }

        public override System.Collections.IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add("PBAoE Pet pulsing effect.");
                return list;
            }
        }
    }
}
