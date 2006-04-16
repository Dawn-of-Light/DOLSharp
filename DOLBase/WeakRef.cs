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
	/// <summary>
	/// This class is a weakreference wrapper
	/// because mono gc crashes with null targets
	/// </summary>
	public class WeakRef : WeakReference
	{
		private class NullValue {};
		private static readonly NullValue NULL = new NullValue();

		/// <summary>
		/// Creates a new weak reference wrapper for MONO because
		/// MONO gc crashes with null targets
		/// </summary>
		/// <param name="target">The target of this weak reference</param>
		public WeakRef(object target) : base((target==null) ? NULL : target) {
		}

		/// <summary>
		/// Creates a new weak reference wrapper for MONO because
		/// MONO gc crashes with null targets
		/// </summary>
		/// <param name="target">The target of this weak reference</param>
		/// <param name="trackResurrection">Track the resurrection of the target</param>
		public WeakRef(object target, bool trackResurrection) : base((target==null) ? NULL : target, trackResurrection) 
		{
		}

		/// <summary>
		/// Gets or sets the target of this weak reference
		/// </summary>
		public override object Target
		{
			get
			{
				object o = base.Target;
				return ((o==NULL) ? null : o);
			}
			set
			{
				base.Target = (value==null) ? NULL : value;
			}
		}
	}
}