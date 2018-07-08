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

namespace DOL.GS.Relics
{
    /// <summary>
    /// Class representing a relic pad.
    /// </summary>
    /// <author>Aredhel</author>
    public class RelicPad : GameObject
    {
        /// <summary>
        /// The pillar this pad triggers.
        /// </summary>
        private readonly RelicPillar _relicPillar;

        public RelicPad(RelicPillar relicPillar)
        {
            _relicPillar = relicPillar;
        }

        /// <summary>
        /// Relic pad radius.
        /// </summary>
        public static int Radius => 200;

        private int _playersOnPad;

        /// <summary>
        /// The number of players currently on the pad.
        /// </summary>
        public int PlayersOnPad
        {
            get => _playersOnPad;
            set
            {
                if (value < 0)
                {
                    return;
                }

                _playersOnPad = value;

                if (_playersOnPad >= ServerProperties.Properties.RELIC_PLAYERS_REQUIRED_ON_PAD &&
                    _relicPillar.State == eDoorState.Closed)
                {
                    _relicPillar.Open();
                }
                else if (_relicPillar.State == eDoorState.Open && _playersOnPad <= 0)
                {
                    _relicPillar.Close();
                }
            }
        }

        /// <summary>
        /// Called when a players steps on the pad.
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerEnter(GamePlayer player)
        {
            PlayersOnPad++;
        }

        /// <summary>
        /// Called when a player steps off the pad.
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerLeave(GamePlayer player)
        {
            PlayersOnPad--;
        }

        /// <summary>
        /// Class to register players entering or leaving the pad.
        /// </summary>
        public class Surface : Area.Circle
        {
            private readonly RelicPad _relicPad;

            public Surface(RelicPad relicPad)
                : base(string.Empty, relicPad.X, relicPad.Y, relicPad.Z, RelicPad.Radius)
            {
                _relicPad = relicPad;
            }

            public override void OnPlayerEnter(GamePlayer player)
            {
                _relicPad.OnPlayerEnter(player);
            }

            public override void OnPlayerLeave(GamePlayer player)
            {
                _relicPad.OnPlayerLeave(player);
            }
        }
    }
}
