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
	/// Represents a weak-referenced multicast delegate.
	/// </summary>
	/// <remarks>This multicast delegate can be garbage collected.</remarks>
	public class WeakMulticastDelegate
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The method info of the target
		/// </summary>
		private readonly MethodInfo _method;

		/// <summary>
		/// Method info of this delegate
		/// </summary>
		public MethodInfo Method
		{
			get { return _method; }
		}

		/// <summary>
		/// A weak reference to the target for this delegate
		/// </summary>
		private readonly WeakReference _weakRef;

		/// <summary>
		/// A weak reference to the target for this delegate
		/// </summary>
		public WeakReference WeakRef
		{
			get { return _weakRef; }
		}


		/// <summary>
		/// The previous weak multicast delegate in the list
		/// </summary>
		private WeakMulticastDelegate _prev;

		/// <summary>
		/// The previous multicast delegate in this list
		/// </summary>
		public WeakMulticastDelegate Previous
		{
			get { return _prev; }
		}

		/// <summary>
		/// Creates a new weak multicast delegate based on
		/// a normal delegate
		/// </summary>
		/// <param name="realDelegate">the normal delegate</param>
		public WeakMulticastDelegate(Delegate realDelegate)
		{
			if (realDelegate == null)
				throw new ArgumentNullException("realDelegate");

			//Is it a static?
			if (realDelegate.Target != null)
				_weakRef = new WeakRef(realDelegate.Target);

			_method = realDelegate.Method;
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
			var head = new WeakMulticastDelegate(realDelegate);
			head._prev = _prev;
			_prev = head;

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
			if (realDelegate == null)
				return false;

			if (_weakRef == null)
			{
				if (realDelegate.Target == null && _method == realDelegate.Method)
					return true;

				return false;
			}

			if (_weakRef.Target == realDelegate.Target && _method == realDelegate.Method)
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

			if (!found && _prev != null)
			{
				WeakMulticastDelegate curNode = _prev;

				while (!found && curNode != null)
				{
					if (curNode.Equals(realDelegate))
						found = true;

					curNode = curNode._prev;
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
			return Combine(d, realD);
		}

		/// <summary>
		/// Removes a normal delegate from a weak multicast delegate
		/// </summary>
		/// <param name="d">the weak multicast delegate</param>
		/// <param name="realD">the real delegate</param>
		/// <returns>the new weak multicast delegate</returns>
		public static WeakMulticastDelegate operator -(WeakMulticastDelegate d, Delegate realD)
		{
			return Remove(d, realD);
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
				return _prev;
			}

			WeakMulticastDelegate current = _prev;
			WeakMulticastDelegate last = this;

			while (current != null)
			{
				if (current.Equals(realDelegate))
				{
					last._prev = current._prev;
					current._prev = null;
					break;
				}

				last = current;
				current = current._prev;
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

				if (current._weakRef == null)
				{
					current._method.Invoke(null, args);
				}
				else if (current._weakRef.IsAlive)
				{
					current._method.Invoke(current._weakRef.Target, args);
				}

				if (Environment.TickCount - start > 500)
				{
					if (Log.IsWarnEnabled)
						Log.Warn("Invoke took " + (Environment.TickCount - start) + "ms! " + current);
				}

				current = current._prev;
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
					if (current._weakRef == null)
					{
						current._method.Invoke(null, args);
					}
					else if (current._weakRef.IsAlive)
					{
						current._method.Invoke(current._weakRef.Target, args);
					}
				}
				catch (Exception ex)
				{
					if (Log.IsErrorEnabled)
						Log.Error("InvokeSafe", ex);
				}

				if (Environment.TickCount - start > 500)
				{
					if (Log.IsWarnEnabled)
						Log.Warn("InvokeSafe took " + (Environment.TickCount - start) + "ms! " + current);
				}

				current = current._prev;
			}
		}

		/// <summary>
		/// Dumps the delegates in this multicast delegate to a string
		/// </summary>
		/// <returns>The string containing the formated dump</returns>
		public string Dump()
		{
			var builder = new StringBuilder();

			WeakMulticastDelegate current = this;
			int count = 0;

			while (current != null)
			{
				count++;
				if (current._weakRef == null)
				{
					builder.Append("\t");
					builder.Append(count);
					builder.Append(") ");
					builder.Append(current._method.Name);
					builder.Append(Environment.NewLine);
				}
				else
				{
					if (current._weakRef.IsAlive)
					{
						builder.Append("\t");
						builder.Append(count);
						builder.Append(") ");
						builder.Append(current._weakRef.Target);
						builder.Append(".");
						builder.Append(current._method.Name);
						builder.Append(Environment.NewLine);
					}
					else
					{
						builder.Append("\t");
						builder.Append(count);
						builder.Append(") INVALID.");
						builder.Append(current._method.Name);
						builder.Append(Environment.NewLine);
					}
				}

				current = current._prev;
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
			if (_method != null)
			{
				declaringType = _method.DeclaringType;
			}

			object target = null;
			if (_weakRef != null && _weakRef.IsAlive)
			{
				target = _weakRef.Target;
			}

			return new StringBuilder(64)
				.Append("method: ").Append(declaringType == null ? "(null)" : declaringType.FullName)
				.Append('.').Append(_method == null ? "(null)" : _method.Name)
				.Append(" target: ").Append(target == null ? "null" : target.ToString())
				.ToString();
		}
	}
}