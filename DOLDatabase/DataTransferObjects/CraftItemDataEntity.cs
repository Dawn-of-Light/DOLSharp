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

namespace DOL.Database.DataTransferObjects
{
	[Serializable]
	public class CraftItemDataEntity
	{
		private int m_id;
		private int m_craftingLevel;
		private int m_craftingSkill;
		private string m_templateToCraft;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public int CraftingLevel
		{
			get { return m_craftingLevel; }
			set { m_craftingLevel = value; }
		}
		public int CraftingSkill
		{
			get { return m_craftingSkill; }
			set { m_craftingSkill = value; }
		}
		public string TemplateToCraft
		{
			get { return m_templateToCraft; }
			set { m_templateToCraft = value; }
		}
	}
}
