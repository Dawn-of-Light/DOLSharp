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

namespace DOL.GS
{
	/// <summary>
	/// callback handler for a spec that is activated by clicking on an associated icon
	/// </summary>
	public interface ISpecActionHandler {
		void Execute(Specialization ab, GamePlayer player);
	}

	/// <summary>
	/// Specialization can be in some way an ability too and can have icons then
	/// its level depends from skill points that were spent to it through trainers
	/// </summary>
	public class Specialization : NamedSkill {

		public Specialization(string keyname, string displayname, ushort icon) : 
			base(keyname, displayname, icon, 1) { }

		/// <summary>
		/// icon id (>=0x190) or 0 if spec is not clickable
		/// </summary>
		public virtual ushort Icon {
			get { return base.ID; }
		}

		/// <summary>
		/// type of skill
		/// </summary>
		public override eSkillPage SkillType {
			get {
				return eSkillPage.Specialization;
			}
		}
	}
}
