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
using DOL.GS;

namespace DOL.GS.Styles
{
	/// <summary>
	/// The style line containing all the styles able to execute
	/// </summary>
	public class StyleLine : Specialization
	{
		/// <summary>
		/// A list of styles contained in this style line
		/// </summary>
		Style[] m_styles = new Style[0];

		/// <summary>
		/// Creates a new style line
		/// </summary>
		/// <param name="keyName">The key name of this styleLine</param>
		/// <param name="name">The name of this styleLine, eg. "Blades"</param>
		/// <param name="styles">The styles for this StyleLine</param>
		public StyleLine (string keyName, string name, Style[] styles) : base(keyName, name, 0)
		{
			m_styles = styles;
		}

		/// <summary>
		/// Returns a list of Styles available at the current level of
		/// this StyleLine
		/// </summary>
		/// <returns>List of styles available at the current level of this styleline</returns>
		public IList GetStylesForLevel ()
		{
			ArrayList list = new ArrayList ();
			for (int i = 0; i < m_styles.Length; i++)
			{
				if (m_styles[i].Level <= Level)
				{
					list.Add (m_styles[i]);
				}
			}
			return list;
		}

		/// <summary>
		/// Gets or Sets all styles in this StyleLine
		/// </summary>
		public Style[] Styles
		{
			get { return m_styles; }
			set { m_styles = value; }
		}
	}
}
