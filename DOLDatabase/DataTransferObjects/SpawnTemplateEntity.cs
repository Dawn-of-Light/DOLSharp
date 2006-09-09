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
	public class SpawnTemplateEntity
	{
		private int m_id;
		private int m_count;
		private int m_gameNPCTemplate;
		private int m_spawnGeneratorBase;
		private string m_spawnTemplateBaseType;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public int Count
		{
			get { return m_count; }
			set { m_count = value; }
		}
		public int GameNPCTemplate
		{
			get { return m_gameNPCTemplate; }
			set { m_gameNPCTemplate = value; }
		}
		public int SpawnGeneratorBase
		{
			get { return m_spawnGeneratorBase; }
			set { m_spawnGeneratorBase = value; }
		}
		public string SpawnTemplateBaseType
		{
			get { return m_spawnTemplateBaseType; }
			set { m_spawnTemplateBaseType = value; }
		}
	}
}
