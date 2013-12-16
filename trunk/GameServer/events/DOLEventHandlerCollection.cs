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
using System.Threading;

namespace DOL.Events
{
	/// <summary>
	/// The callback method for DOLEvents
	/// </summary>
	/// <remarks>Override the EventArgs class to give custom parameters</remarks>
	public delegate void DOLEventHandler(DOLEvent e, object sender, EventArgs arguments);

	/// <summary>
	/// This class represents a collection of event handlers. You can add and remove
	/// handlers from this list and fire events with parameters which will be routed
	/// through all handlers.
	/// </summary>
	/// <remarks>This class is lazy initialized, meaning as long as you don't add any
	/// handlers, the memory usage will be very low!</remarks>
	public sealed class DOLEventHandlerCollection
	{
		/// <summary>
		/// How long to wait for a lock acquisition before failing.
		/// </summary>
		private const int LOCK_TIMEOUT = 3000;

		/// <summary>
		/// Holds a mapping of any delegates bound to any DOLEvent
		/// </summary>
		private readonly Dictionary<DOLEvent, WeakMulticastDelegate> _events;

		public WeakMulticastDelegate GetEventDelegate(DOLEvent e)
		{
			if (_events.ContainsKey(e))
				return _events[e];

			return null;
		}

		public int Count
		{
			get { return _events.Count; }
		}

		/// <summary>
		/// Reader/writer lock for synchronizing access to the event handler map
		/// </summary>
		private readonly ReaderWriterLockSlim _lock;

		/// <summary>
		/// Constructs a new DOLEventHandler collection
		/// </summary>
		public DOLEventHandlerCollection()
		{
			_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
			_events = new Dictionary<DOLEvent, WeakMulticastDelegate>();
		}

		/// <summary>
		/// Adds an event handler to the list.
		/// </summary>
		/// <param name="e">The event from which we add a handler</param>
		/// <param name="del">The callback method</param>
		public void AddHandler(DOLEvent e, DOLEventHandler del)
		{
			if(_lock.TryEnterWriteLock(LOCK_TIMEOUT))
			{
				try
				{
					WeakMulticastDelegate deleg;

					if(!_events.TryGetValue(e, out deleg))
					{
						_events.Add(e, new WeakMulticastDelegate(del));
					}
					else
					{
						_events[e] = WeakMulticastDelegate.Combine(deleg, del);
					}
				}
				finally
				{
					_lock.ExitWriteLock();
				}
			}
		}

		/// <summary>
		/// Adds an event handler to the list, if it's already added do nothing
		/// </summary>
		/// <param name="e">The event from which we add a handler</param>
		/// <param name="del">The callback method</param>
		public void AddHandlerUnique(DOLEvent e, DOLEventHandler del)
		{
			if(_lock.TryEnterWriteLock(LOCK_TIMEOUT))
			{
				try
				{
					WeakMulticastDelegate deleg;

					if(!_events.TryGetValue(e, out deleg))
					{
						_events.Add(e, new WeakMulticastDelegate(del));
					}
					else
					{
						_events[e] = WeakMulticastDelegate.CombineUnique(deleg, del);
					}
				}
				finally
				{
					_lock.ExitWriteLock();
				}
			}
		}

		/// <summary>
		/// Removes an event handler from the list
		/// </summary>
		/// <param name="e">The event from which to remove the handler</param>
		/// <param name="del">The callback method to remove</param>
		public void RemoveHandler(DOLEvent e, DOLEventHandler del)
		{
			if(_lock.TryEnterWriteLock(LOCK_TIMEOUT))
			{
				try
				{
					WeakMulticastDelegate deleg;

					if(_events.TryGetValue(e, out deleg))
					{
						deleg = WeakMulticastDelegate.Remove(deleg, del);

						if(deleg == null)
						{
							_events.Remove(e);
						}
						else
						{
							_events[e] = deleg;
						}
					}
				}
				finally
				{
					_lock.ExitWriteLock();
				}
			}
		}

		/// <summary>
		/// Removes all callback handlers for a given event
		/// </summary>
		/// <param name="e">The event from which to remove all handlers</param>
		public void RemoveAllHandlers(DOLEvent e)
		{
			if(_lock.TryEnterWriteLock(LOCK_TIMEOUT))
			{
				try
				{
					_events.Remove(e);
				}
				finally
				{
					_lock.ExitWriteLock();
				}
			}
		}

		/// <summary>
		/// Removes all event handlers
		/// </summary>
		public void RemoveAllHandlers()
		{
			if(_lock.TryEnterWriteLock(LOCK_TIMEOUT))
			{
				try
				{
					_events.Clear();
				}
				finally
				{
					_lock.ExitWriteLock();
				}
			}
		}

		/// <summary>
		/// Notifies all registered event handlers of the occurance of an event!
		/// </summary>
		/// <param name="e">The event that occured</param>
		public void Notify(DOLEvent e)
		{
			Notify(e, null, null);
		}


		/// <summary>
		/// Notifies all registered event handlers of the occurance of an event!
		/// </summary>
		/// <param name="e">The event that occured</param>
		/// <param name="sender">The sender of this event</param>
		public void Notify(DOLEvent e, object sender)
		{
			Notify(e, sender, null);
		}

		/// <summary>
		/// Notifies all registered event handlers of the occurance of an event!
		/// </summary>
		/// <param name="e">The event that occured</param>
		/// <param name="args">The event arguments</param>
		public void Notify(DOLEvent e, EventArgs args)
		{
			Notify(e, null, args);
		}

		/// <summary>
		/// Notifies all registered event handlers of the occurance of an event!
		/// </summary>
		/// <param name="e">The event that occured</param>
		/// <param name="sender">The sender of this event</param>
		/// <param name="eArgs">The event arguments</param>
		/// <remarks>Overwrite the EventArgs class to set own arguments</remarks>
		public void Notify(DOLEvent e, object sender, EventArgs eArgs)
		{
			WeakMulticastDelegate eventDelegate = null;

			if(_lock.TryEnterReadLock(LOCK_TIMEOUT))
			{
				try
				{
					if(!_events.TryGetValue(e, out eventDelegate))
						return;
				}
				finally
				{
					_lock.ExitReadLock();
				}
			}

			eventDelegate.InvokeSafe(new[] { e, sender, eArgs });
		}
	}
}
