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
using System.Reflection;
using System.Text;
using log4net;

namespace DOL
{
	/// <summary>
	/// This class implements weakreference delegates which
	/// enable the target to be garbage collected
	/// </summary>
	public class WeakMulticastDelegate
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// A weak reference to the target for this delegate
		/// </summary>
		private WeakReference weakRef = null;
	
		/// <summary>
		/// The method info of the target
		/// </summary>
		private MethodInfo method = null;
	
		/// <summary>
		/// The previous weak multicast delegate in the list
		/// </summary>
		private WeakMulticastDelegate prev = null;

		/// <summary>
		/// Creates a new weak multicast delegate based on
		/// a normal delegate
		/// </summary>
		/// <param name="realDelegate">the normal delegate</param>
		public WeakMulticastDelegate(Delegate realDelegate)
		{
			//Is it a static?
			if (realDelegate.Target != null)
				this.weakRef = new WeakRef(realDelegate.Target);
			this.method = realDelegate.Method;
		}

		/// <summary>
		/// Combines a weak multicast delegate with a normal delegate
		/// </summary>
		/// <param name="weakDelegate">the weak multicast delegate</param>
		/// <param name="realDelegate">the normal delegate</param>
		/// <returns>the new combinded weak multicast delegate</returns>
		public static WeakMulticastDelegate Combine(WeakMulticastDelegate weakDelegate, Delegate realDelegate)
		{
			if (realDelegate == null)
				return null;
			return (weakDelegate == null) ? new WeakMulticastDelegate(realDelegate) : weakDelegate.Combine(realDelegate);
		}

		/// <summary>
		/// Combines a weak multicast delegate with a normal delegate
		/// and makes sure the normal delegate has not been added yet.
		/// </summary>
		/// <param name="weakDelegate">the weak multicast delegate</param>
		/// <param name="realDelegate">the normal delegate</param>
		/// <returns>the new combined weak multicast delegate</returns>
		public static WeakMulticastDelegate CombineUnique(WeakMulticastDelegate weakDelegate, Delegate realDelegate)
		{
			if (realDelegate == null)
				return null;
			return (weakDelegate == null) ? new WeakMulticastDelegate(realDelegate) : weakDelegate.CombineUnique(realDelegate);
		}

		/// <summary>
		/// Combines this weak multicast delegate with a normal delegate
		/// </summary>
		/// <param name="realDelegate">the normal delegate</param>
		/// <returns>this delegate</returns>
		private WeakMulticastDelegate Combine(Delegate realDelegate)
		{
			WeakMulticastDelegate head = new WeakMulticastDelegate(realDelegate);
			head.prev = this.prev;
			this.prev = head;
			return this;
		}

		/// <summary>
		/// Compares this weak multicast delegate with a normal delegate
		/// and returns wether their targets are equal
		/// </summary>
		/// <param name="realDelegate">the normal delegate</param>
		/// <returns>true if equal, false if not equal</returns>
		protected bool Equals(Delegate realDelegate)
		{
			if (weakRef == null)
			{
				if (realDelegate.Target == null && method == realDelegate.Method)
					return true;
				return false;
			}
			if (weakRef.Target == realDelegate.Target
				&& method == realDelegate.Method)
				return true;
			return false;
		}

		/// <summary>
		/// Combines this weak multicast delegate with a normal delegate
		/// Makes sure the delegate target has not been added yet
		/// </summary>
		/// <param name="realDelegate">the real delegate</param>
		/// <returns>the new weak multicast delegate</returns>
		private WeakMulticastDelegate CombineUnique(Delegate realDelegate)
		{
			bool found = Equals(realDelegate);
			if (!found && prev != null)
			{
				WeakMulticastDelegate curNode = prev;
				while (!found && curNode != null)
				{
					if (curNode.Equals(realDelegate))
						found = true;
					curNode = curNode.prev;
				}
			}
			return found ? this : Combine(realDelegate);
		}

		/// <summary>
		/// Combines a weak multicast delegate with a normal delegate
		/// </summary>
		/// <param name="d">the weak multicast delegate</param>
		/// <param name="realD">the real delegate</param>
		/// <returns>the new weak multicast delegate</returns>
		public static WeakMulticastDelegate operator +(WeakMulticastDelegate d, Delegate realD)
		{
			return WeakMulticastDelegate.Combine(d, realD);
		}

		/// <summary>
		/// Removes a normal delegate from a weak multicast delegate
		/// </summary>
		/// <param name="d">the weak multicast delegate</param>
		/// <param name="realD">the real delegate</param>
		/// <returns>the new weak multicast delegate</returns>
		public static WeakMulticastDelegate operator -(WeakMulticastDelegate d, Delegate realD)
		{
			return WeakMulticastDelegate.Remove(d, realD);
		}

		/// <summary>
		/// Removes a normal delegate from a weak multicast delegate
		/// </summary>
		/// <param name="weakDelegate">the weak multicast delegate</param>
		/// <param name="realDelegate">the normal delegate</param>
		/// <returns>the new weak multicast delegate</returns>
		public static WeakMulticastDelegate Remove(WeakMulticastDelegate weakDelegate, Delegate realDelegate)
		{
			if (realDelegate == null || weakDelegate == null)
				return null;
			return weakDelegate.Remove(realDelegate);
		}

		/// <summary>
		/// Removes a normal delegate from this weak multicast delegate
		/// </summary>
		/// <param name="realDelegate">the normal delegate</param>
		/// <returns>the new weak multicast delegate</returns>
		private WeakMulticastDelegate Remove(Delegate realDelegate)
		{
			if (Equals(realDelegate))
			{
				return this.prev;
			}
			WeakMulticastDelegate current = this.prev;
			WeakMulticastDelegate last = this;
			while (current != null)
			{
				if (current.Equals(realDelegate))
				{
					last.prev = current.prev;
					current.prev = null;
					break;
				}
				last = current;
				current = current.prev;
			}
			return this;
		}

		/// <summary>
		/// Invokes the delegate with the given arguments
		/// If one target throws an exception the other targets
		/// won't be handled anymore.
		/// </summary>
		/// <param name="args">the argument array to pass to the target</param>
		public void Invoke(object[] args)
		{
			WeakMulticastDelegate current = this;
			while (current != null)
			{
				int start = Environment.TickCount;

				if (current.weakRef == null)
				{
					current.method.Invoke(null, args);
				}
				else if (current.weakRef.IsAlive)
				{
					current.method.Invoke(current.weakRef.Target, args);
				}

				if (Environment.TickCount - start > 500)
				{
					if(log.IsWarnEnabled)
						log.Warn("Invoke took " + (Environment.TickCount - start) + "ms! " + current.ToString());
				}
				current = current.prev;
			}
		}

		/// <summary>
		/// Invokes the delegate with the given arguments
		/// If one target throws an exception the other targets
		/// won't be affected.
		/// </summary>
		/// <param name="args">the argument array to pass to the target</param>
		public void InvokeSafe(object[] args)
		{
			WeakMulticastDelegate current = this;
			while (current != null)
			{
				int start = Environment.TickCount;

				try
				{
					if (current.weakRef == null)
					{
							current.method.Invoke(null, args);
					}
					else if (current.weakRef.IsAlive)
					{
							current.method.Invoke(current.weakRef.Target, args);
					}
				}
				catch (Exception ex)
				{
					if(log.IsErrorEnabled)
						log.Error("InvokeSafe", ex);
				}

				if (Environment.TickCount - start > 500)
				{
					if(log.IsWarnEnabled)
						log.Warn("InvokeSafe took " + (Environment.TickCount - start) + "ms! " + current.ToString());
				}
				current = current.prev;
			}
		}

		/// <summary>
		/// Dumps the delegates in this multicast delegate to a string
		/// </summary>
		/// <returns>The string containing the formated dump</returns>
		public string Dump()
		{
			StringBuilder builder = new StringBuilder();
			WeakMulticastDelegate current = this;
			int count = 0;
			while (current != null)
			{
				count++;
				if (current.weakRef == null)
				{
					builder.Append("\t");
					builder.Append(count);
					builder.Append(") ");
					builder.Append(current.method.Name);
					builder.Append(System.Environment.NewLine);
				}
				else
				{
					if (current.weakRef.IsAlive)
					{
						builder.Append("\t");
						builder.Append(count);
						builder.Append(") ");
						builder.Append(current.weakRef.Target);
						builder.Append(".");
						builder.Append(current.method.Name);
						builder.Append(Environment.NewLine);
					}
					else
					{
						builder.Append("\t");
						builder.Append(count);
						builder.Append(") INVALID.");
						builder.Append(current.method.Name);
						builder.Append(Environment.NewLine);
					}
				}
				current = current.prev;
			}
			return builder.ToString();
		}

		/// <summary>
		/// Gets string representation of delegate
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			Type declaringType = null;
			if (method != null)
				declaringType = method.DeclaringType;

			object target = null;
			if (weakRef != null && weakRef.IsAlive)
				target = weakRef.Target;

			return new StringBuilder(64)
				.Append("method: ").Append(declaringType == null ? "(null)" : declaringType.FullName)
				.Append('.').Append(method == null ? "(null)" : method.Name)
				.Append(" target: ").Append(target == null ? "null" : target.ToString())
				.ToString();
		}
	}
}