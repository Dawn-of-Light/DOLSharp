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
using System.Collections.Specialized;
using System.Reflection;
using System.Threading;
using log4net;

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
	public class DOLEventHandlerCollection
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// How long to wait for a lock before failing!
		/// </summary>
		protected const int TIMEOUT = 3000;
		/// <summary>
		/// A reader writer lock used to lock event list
		/// </summary>
		protected readonly ReaderWriterLock m_lock = null;
		/// <summary>
		///We use a HybridDictionary here to hold all event delegates
		/// </summary>
		protected readonly HybridDictionary m_events = null;

		/// <summary>
		/// Constructs a new DOLEventHandler collection
		/// </summary>
		public DOLEventHandlerCollection()
		{
			m_lock = new ReaderWriterLock();
			m_events = new HybridDictionary();
		}
		/// <summary>
		/// Adds an event handler to the list
		/// </summary>
		/// <param name="e">The event from which we add a handler</param>
		/// <param name="del">The callback method</param>
		public void AddHandler(DOLEvent e, DOLEventHandler del)
		{			
			try
			{
				m_lock.AcquireWriterLock(TIMEOUT);
				try
				{
					WeakMulticastDelegate deleg = (WeakMulticastDelegate) m_events[e];
					if(deleg==null)
					{
						m_events[e] = new WeakMulticastDelegate(del);
					}
					else
					{
						m_events[e] = WeakMulticastDelegate.Combine(deleg,del);
					}
				}
				finally
				{
					m_lock.ReleaseWriterLock();
				}
			}
			catch (ApplicationException ex)
			{
				if (log.IsErrorEnabled)
					log.Error("Failed to add event handler!", ex);
			}
		}

        /// <summary>
        /// Adds an event handler to the list, if it's already added do nothing
        /// </summary>
        /// <param name="e">The event from which we add a handler</param>
        /// <param name="del">The callback method</param>
        public void AddHandlerUnique(DOLEvent e, DOLEventHandler del)
        {
            try
            {
                m_lock.AcquireWriterLock(TIMEOUT);
                try
                {
                    WeakMulticastDelegate deleg = (WeakMulticastDelegate)m_events[e];
                    if (deleg == null)
                    {
                        m_events[e] = new WeakMulticastDelegate(del);
                    }
                    else
                    {
                        m_events[e] = WeakMulticastDelegate.CombineUnique(deleg, del);
                    }
                }
                finally
                {
                    m_lock.ReleaseWriterLock();
                }
            }
            catch (ApplicationException ex)
            {
                if (log.IsErrorEnabled)
                    log.Error("Failed to add event handler!", ex);
            }
        }

		/// <summary>
		/// Removes an event handler from the list
		/// </summary>
		/// <param name="e">The event from which to remove the handler</param>
		/// <param name="del">The callback method to remove</param>
		public void RemoveHandler(DOLEvent e, DOLEventHandler del)
		{
			try
			{
				m_lock.AcquireWriterLock(TIMEOUT);
				try
				{
					WeakMulticastDelegate deleg = (WeakMulticastDelegate) m_events[e];
					if(deleg!=null) 
					{
						deleg = WeakMulticastDelegate.Remove(deleg,del);
						if(deleg==null)
							m_events.Remove(e);
						else
							m_events[e] = deleg;
					}
				}
				finally
				{
					m_lock.ReleaseWriterLock();
				}
			}
			catch (ApplicationException ex)
			{
				if (log.IsErrorEnabled)
					log.Error("Failed to remove event handler!", ex);
			}
		}

		/// <summary>
		/// Removes all callback handlers for a given event
		/// </summary>
		/// <param name="e">The event from which to remove all handlers</param>
		public void RemoveAllHandlers(DOLEvent e)
		{
			try
			{
				m_lock.AcquireWriterLock(TIMEOUT);
				try
				{
					m_events.Remove(e);
				}
				finally
				{
					m_lock.ReleaseWriterLock();
				}
			}
			catch (ApplicationException ex)
			{
				if (log.IsErrorEnabled)
					log.Error("Failed to remove event handlers!", ex);
			}
		}

		/// <summary>
		/// Removes all event handlers
		/// </summary>
		public void RemoveAllHandlers()
		{
			try
			{
				m_lock.AcquireWriterLock(TIMEOUT);
				try
				{
					m_events.Clear();
				}
				finally
				{
					m_lock.ReleaseWriterLock();
				}
			}
			catch (ApplicationException ex)
			{
				if (log.IsErrorEnabled)
					log.Error("Failed to remove all event handlers!", ex);
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
			try
			{
				m_lock.AcquireReaderLock(TIMEOUT);
				WeakMulticastDelegate eventDelegate;
				try
				{
					eventDelegate = (WeakMulticastDelegate) m_events[e];
				}
				finally
				{
					m_lock.ReleaseReaderLock();
				}
				if(eventDelegate==null) return;
				eventDelegate.InvokeSafe(new object[] {e, sender, eArgs});
			}
			catch (ApplicationException ex)
			{
				if (log.IsErrorEnabled)
					log.Error("Failed to notify event handler!", ex);
			}
		}
	}
}
