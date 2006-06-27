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

namespace DOL
{
	namespace GS
	{
		/// <summary>
		/// Marks a class as a command handler
		/// </summary>
		[AttributeUsage(AttributeTargets.Class,AllowMultiple = true)]
		public class CmdAttribute : Attribute
		{
			private	string		m_cmd;
			private string[]	m_cmdAliases;
			private uint		m_lvl;
			private	string		m_description;
			private string[]	m_usage;

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="cmd">Command to handle</param>
			/// <param name="alias">Other names the command goes by</param>
			/// <param name="lvl">Minimum required plvl for this command</param>
			/// <param name="desc">Description of the command</param>
			/// <param name="usage">How to use the command</param>
			public CmdAttribute(string cmd, string[] alias, uint lvl, string desc, params string[] usage)
			{
				m_cmd=cmd;
				m_cmdAliases = alias;
				m_lvl = lvl;
				m_description = desc;
				//m_usage = new string[1];
				m_usage = usage;
			}

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="cmd">Command to handle</param>
			/// <param name="lvl">Minimum required plvl for this command</param>
			/// <param name="desc">Description of the command</param>
			/// <param name="usage">How to use the command</param>
			public CmdAttribute(string cmd, uint lvl, string desc, params string[] usage) : this(cmd,null,lvl,desc,usage)
			{
			}

			/// <summary>
			/// Gets the command being handled
			/// </summary>
			public string Cmd
			{
				get 
				{
	  				return m_cmd;
				}
			}

			/// <summary>
			/// Gets aliases for the command being handled
			/// </summary>
			public string[] Aliases
			{
				get
				{
					return m_cmdAliases;
				}
			}

			/// <summary>
			/// Gets minimum required plvl for the command to be used
			/// </summary>
			public uint Level
			{
				get
				{
					return m_lvl;
				}
			}

			/// <summary>
			/// Gets the description of the command
			/// </summary>
			public string Description
			{
				get 
				{
					return m_description;
				}
			}

			/// <summary>
			/// Gets the command usage
			/// </summary>
			public string[] Usage
			{
				get
				{
					return m_usage;
				}
			}
		}
	}
}