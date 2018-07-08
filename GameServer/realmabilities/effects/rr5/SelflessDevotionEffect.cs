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

// Eden - 1.94 RR5 Paladin
using System.Collections.Generic;
using DOL.GS.PacketHandler;

namespace DOL.GS.Effects
{
    public class SelflessDevotionEffect : TimedEffect
    {
        public SelflessDevotionEffect() : base(15000)
        {
            _healpulse = 5;
        }

        private GamePlayer _owner;
        private RegionTimer _timer;
        private int _healpulse;
        private Dictionary<eProperty, int> _debuffs;

        public override void Start(GameLiving target)
        {
            base.Start(target);

            _owner = target as GamePlayer;
            if (_owner == null)
            {
                return;
            }

            foreach (GamePlayer p in _owner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                p.Out.SendSpellEffectAnimation(_owner, _owner, Icon, 0, false, 1);
            }

            _debuffs = new Dictionary<eProperty, int>(1 + eProperty.Stat_Last - eProperty.Stat_First);

            for (eProperty property = eProperty.Stat_First; property <= eProperty.Stat_Last; property++)
            {
                _debuffs.Add(property, (int)(_owner.GetModified(property) * 0.25));
                _owner.DebuffCategory[(int)property] += _debuffs[property];
            }

            _owner.Out.SendCharStatsUpdate();

            _timer = new RegionTimer(_owner, new RegionTimerCallback(HealPulse));
            _timer.Start(1);
        }

        public int HealPulse(RegionTimer timer)
        {
            if (_healpulse > 0)
            {
                _healpulse--;

                if (!(Owner is GamePlayer player))
                {
                    return 0;
                }

                if (player.Group == null)
                {
                    return 3000;
                }

                foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                {
                    if (p.Health < p.MaxHealth && player.IsWithinRadius(p, 750) && p.IsAlive)
                    {
                        if (player.IsStealthed)
                        {
                            player.Stealth(false);
                        }

                        int heal = 300;

                        if (p.Health + heal > p.MaxHealth)
                        {
                            heal = p.MaxHealth - p.Health;
                        }

                        p.ChangeHealth(player, GameLiving.eHealthChangeType.Regenerate, heal);

                        player.Out.SendMessage("You heal " + p.Name + " for " + heal.ToString() + " hit points.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                        p.Out.SendMessage(player.Name + " heals you for " + heal.ToString() + " hit points.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    }
                }

                return 3000;
            }

            return 0;
        }

        public override void Stop()
        {
            base.Stop();

            if (_owner != null)
            {
                for (eProperty property = eProperty.Stat_First; property <= eProperty.Stat_Last; property++)
                {
                    if (_debuffs.ContainsKey(property))
                    {
                        _owner.DebuffCategory[(int)property] -= _debuffs[property];
                    }
                }

                _owner.Out.SendCharStatsUpdate();
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        public override string Name => "Selfless Devotion";

        public override ushort Icon => 3038;

        public int SpellEffectiveness => 100;

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Decrease Paladin stats by 25%, and pulse a 300 points group heal with a 750 units range every 3 seconds for 15 seconds total."
                };

                return list;
            }
        }
    }
}