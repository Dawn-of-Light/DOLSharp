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
using System.Text;

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
		private RelicPillar m_relicPillar;

		public RelicPad(RelicPillar relicPillar)
		{
			m_relicPillar = relicPillar;
		}

		/// <summary>
		/// Relic pad radius.
		/// </summary>
		static public int Radius
		{
			get { return 200; }
		}

		private int m_playersOnPad = 0;

		/// <summary>
		/// The number of players currently on the pad.
		/// </summary>
		public int PlayersOnPad
		{
			get { return m_playersOnPad; }
			set 
			{
				if (value < 0)
					return;

				m_playersOnPad = value;

				if (m_playersOnPad >= ServerProperties.Properties.RELIC_PLAYERS_REQUIRED_ON_PAD &&
					m_relicPillar.State == eDoorState.Closed)
					m_relicPillar.Open();
				else if (m_relicPillar.State == eDoorState.Open && m_playersOnPad <= 0)
					m_relicPillar.Close();
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
			private RelicPad m_relicPad;

			public Surface(RelicPad relicPad)
				: base("", relicPad.X, relicPad.Y, relicPad.Z, RelicPad.Radius)
			{
				m_relicPad = relicPad;
			}

			public override void OnPlayerEnter(GamePlayer player)
			{
				m_relicPad.OnPlayerEnter(player);
			}

			public override void OnPlayerLeave(GamePlayer player)
			{
				m_relicPad.OnPlayerLeave(player);
			}
		}
	}
}
