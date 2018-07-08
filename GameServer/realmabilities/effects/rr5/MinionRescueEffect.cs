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

using System.Collections.Generic;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Minion Rescue
    /// </summary>
    public class MinionRescueEffect : TimedEffect
    {
        private const int Duration = 6 * 1000;

        // Parameters
        private const int SpiritCount = 8;          // Max number of spirits to summon
        private const byte SpiritLevel = 50;            // Level of the spirit
        private const int SpiritModel = 908;        // Model to use for spirit
        private const int SpiritSpeed = 350;        // Max speed of the spirit
        private const string SpiritName = "Spirit"; // Name of spirit
        private const int SpellDuration = 4;        // Duration of stun in seconds

        // Objects
        private readonly GameNPC[] _spirits;              // Array containing spirits
        private readonly RegionTimer[] _spiritTimer;          // Array containing spirit timers
        private readonly Spell _spiritSpell;          // The spell to cast
        private readonly SpellLine _spiritSpellLine;      // The spell line
        private ISpellHandler _stun;                 // The spell handler
        private GamePlayer _effectOwner;         // Owner of the effect

        public MinionRescueEffect()
            : base(Duration)
        {
            // Init NPC & Timer array
            _spirits = new GameNPC[SpiritCount];
            _spiritTimer = new RegionTimer[SpiritCount];

            // Build spell
            DBSpell tSpell = new DBSpell
            {
                AllowAdd = false,
                Description = "Target is stunned and can't move or do any action during spell duration.",
                Name = "Rescue stun",
                Target = "Enemy",
                Radius = 0,
                Range = WorldMgr.VISIBILITY_DISTANCE,
                CastTime = 0,
                Duration = SpellDuration,
                Uninterruptible = true,
                Type = "Stun",
                ResurrectMana = 1,
                ResurrectHealth = 1,
                Damage = 0,
                DamageType = 0,
                Value = 0,
                Icon = 7049,
                ClientEffect = 7049
            };

            _spiritSpell = new Spell(tSpell, 1);
            _spiritSpellLine = new SpellLine("RAs", "RealmAbilitys", "RealmAbilitys", true);
        }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GamePlayer player)
            {
                _effectOwner = player;
                _stun = ScriptMgr.CreateSpellHandler(_effectOwner, _spiritSpell, _spiritSpellLine);

                int targetCount = 0;
                foreach (GamePlayer targetPlayer in _effectOwner.GetPlayersInRadius(RealmAbilities.MinionRescueAbility.SpellRadius))
                {
                    if (targetCount == SpiritCount)
                    {
                        return;
                    }

                    if (targetPlayer.IsAlive && GameServer.ServerRules.IsAllowedToAttack(_effectOwner, targetPlayer, true))
                    {
                        SummonSpirit(targetCount, targetPlayer);
                        targetCount++;
                    }
                }
            }
        }

        public override void Stop()
        {
            for (int index = 0; index < SpiritCount; index++)
            {
                if (_spiritTimer[index] != null) { _spiritTimer[index].Stop(); _spiritTimer[index] = null; }
                if (_spirits[index] != null) { _spirits[index].Delete(); _spirits[index] = null; }
            }

            base.Stop();
        }

        // Summon a spirit that will follow target
        private void SummonSpirit(int spiritId, GamePlayer targetPlayer)
        {
            _spirits[spiritId] = new GameNPC
            {
                CurrentRegion = _effectOwner.CurrentRegion,
                Heading = (ushort) ((_effectOwner.Heading + 2048) % 4096),
                Level = SpiritLevel,
                Realm = _effectOwner.Realm,
                Name = SpiritName,
                Model = SpiritModel,
                CurrentSpeed = 0,
                MaxSpeedBase = SpiritSpeed,
                GuildName = string.Empty,
                Size = 50,
                X = _effectOwner.X + Util.Random(20, 40) - Util.Random(20, 40),
                Y = _effectOwner.Y + Util.Random(20, 40) - Util.Random(20, 40),
                Z = _effectOwner.Z
            };

            _spirits[spiritId].Flags |= GameNPC.eFlags.DONTSHOWNAME;
            _spirits[spiritId].SetOwnBrain(new StandardMobBrain());
            _spirits[spiritId].AddToWorld();
            _spirits[spiritId].TargetObject = targetPlayer;
            _spirits[spiritId].Follow(targetPlayer, 0, RealmAbilities.MinionRescueAbility.SpellRadius + 100);
            _spiritTimer[spiritId] = new RegionTimer(_spirits[spiritId], new RegionTimerCallback(SpiritCallBack), 200);
        }

        // Check distance between spirit and target
        private int SpiritCallBack(RegionTimer timer)
        {
            if (!(timer.Owner is GameNPC))
            {
                timer.Stop();
                return 0;
            }

            GameNPC spirit = (GameNPC) timer.Owner;

            if (!(spirit.TargetObject is GamePlayer targetPlayer) || !targetPlayer.IsAlive)
            {
                spirit.StopFollowing();
                timer.Stop();
                return 0;
            }

            if (targetPlayer.IsWithinRadius(spirit, 100))
            {
                ApplySpiritEffect(spirit, targetPlayer);
                timer.Stop();
                return 0;
            }

            return 200;
        }

        // Stun target when spirit come in contact
        private void ApplySpiritEffect(GameLiving source, GameLiving target)
        {
            _stun?.StartSpell(target);

            source.Die(null);
            source.Delete();
        }

        public override string Name => "Minion Rescue";

        public override ushort Icon => 3048;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Summon pets that will follow and stun enemies."
                };

                return list;
            }
        }
    }
}
