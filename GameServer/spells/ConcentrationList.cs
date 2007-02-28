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
using System.Reflection;
using DOL.GS.Effects;
using log4net;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Holds spells that use concentration points
	/// </summary>
	public class ConcentrationList : IEnumerable
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds the list owner
		/// </summary>
		private readonly GameLiving m_owner;
		/// <summary>
		/// Stores the used concentration points
		/// </summary>
		private short m_usedConcPoints;
		/// <summary>
		/// Stores the amount of concentration spells in the list
		/// </summary>
		private byte m_concSpellsCount;
		/// <summary>
		/// Stores the count of BeginChanges
		/// </summary>
		private sbyte m_changeCounter;
		/// <summary>
		/// Holds the list effects
		/// </summary>
		private ArrayList m_concSpells;

		/// <summary>
		/// Constructs a new ConcentrationList
		/// </summary>
		/// <param name="owner">The list owner</param>
		public ConcentrationList(GameLiving owner)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");
			m_owner = owner;
		}

		/// <summary>
		/// Adds a new effect to the list
		/// </summary>
		/// <param name="effect">The effect to add</param>
		public void Add(IConcentrationEffect effect)
		{
			BeginChanges();

			if (m_concSpells == null)
				m_concSpells = new ArrayList(20);

			lock (m_concSpells) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				if (m_concSpells.Contains(effect))
				{
					if (log.IsWarnEnabled)
						log.Warn("(" + m_owner.Name + ") effect already applied " + effect.Name);
				}
				else
				{
					// no conc spells are always last in the list
					if (m_concSpells.Count > 0)
					{
						IConcentrationEffect lastEffect = (IConcentrationEffect)m_concSpells[m_concSpells.Count - 1];
						if (lastEffect.Concentration > 0)
						{
							m_concSpells.Add(effect);
						}
						else
						{
							m_concSpells.Insert(m_concSpells.Count - 1, effect);
						}
					}
					else
					{
						m_concSpells.Add(effect);
					}

					if (effect.Concentration > 0)
					{
						m_usedConcPoints += effect.Concentration;
						m_concSpellsCount++;
					}
				}
			}
			CommitChanges();
		}

		/// <summary>
		/// Removes an effect from the list
		/// </summary>
		/// <param name="effect">The effect to remove</param>
		public void Remove(IConcentrationEffect effect)
		{
			if (m_concSpells == null)
				return;

			lock (m_concSpells) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				if (m_concSpells.Contains(effect))
				{
					BeginChanges();
					m_concSpells.Remove(effect);
					if (effect.Concentration > 0)
					{
						m_usedConcPoints -= effect.Concentration;
						m_concSpellsCount--;
					}
					CommitChanges();
				}
			}
		}

		/// <summary>
		/// Cancels all list effects
		/// </summary>
		public void CancelAll()
		{
			CancelAll(false);
		}

		/// <summary>
		/// Cancels all list effects
		/// </summary>
		public void CancelAll(bool leaveself)
		{
			if (log.IsDebugEnabled)
				log.Debug("(" + m_owner.Name + ") cancels all conc spells leaveself: " + leaveself);
			ArrayList spells = null;
			if (m_concSpells == null) return;
			spells = (ArrayList)m_concSpells.Clone();
			BeginChanges();
			if (spells != null)
			{
				foreach (IConcentrationEffect fx in spells)
				{
					if (!leaveself || leaveself && fx.OwnerName != m_owner.Name)
						fx.Cancel(false);
				}
			}
			CommitChanges();
		}

		/// <summary>
		/// Stops list updates for multiple changes
		/// </summary>
		public void BeginChanges()
		{
			m_changeCounter++;
		}

		/// <summary>
		/// Sends all changes since BeginChanges was called
		/// </summary>
		public void CommitChanges()
		{
			m_changeCounter--;
			if (m_changeCounter < 0)
			{
				if (log.IsErrorEnabled)
					log.Error("Change counter below 0 in conc spell list, forgotten to use BeginChanges?");
				m_changeCounter = 0;
			}

			if (m_changeCounter <= 0 && m_owner is GamePlayer)
			{
				((GamePlayer)m_owner).Out.SendConcentrationList();
			}
		}

		/// <summary>
		/// Find the first occurence of an effect with given type
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns>effect or null</returns>
		public IConcentrationEffect GetOfType(Type effectType)
		{
			if (m_concSpells == null)
				return null;
			lock (m_concSpells) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				foreach (IConcentrationEffect effect in m_concSpells)
				{
					if (effect.GetType().Equals(effectType))
						return effect;
				}
			}
			return null;
		}

		/// <summary>
		/// Find effects of specific type
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns>resulting effectlist</returns>
		public IList GetAllOfType(Type effectType)
		{
			ArrayList list = new ArrayList();

			if (m_concSpells == null) return list;
			lock (m_concSpells) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				foreach (IConcentrationEffect effect in m_concSpells)
				{
					if (effect.GetType().Equals(effectType))
						list.Add(effect);
				}
			}
			return list;
		}

		/// <summary>
		/// Gets the concentration effect by index
		/// </summary>
		public IConcentrationEffect this[int index]
		{
			get
			{
				if (m_concSpells == null) return null;
				return (IConcentrationEffect)m_concSpells[index];
			}
		}

		/// <summary>
		/// Gets the amount of used concentration
		/// </summary>
		public short UsedConcentration
		{
			get { return m_usedConcPoints; }
		}

		/// <summary>
		/// Gets the count of conc spells in the list
		/// </summary>
		public byte ConcSpellsCount
		{
			get { return m_concSpellsCount; }
		}

		/// <summary>
		/// Gets the list effects count
		/// </summary>
		public int Count
		{
			get { return m_concSpells == null ? 0 : m_concSpells.Count; }
		}

		#region IEnumerable Members

		/// <summary>
		/// Gets the list enumerator
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			if (m_concSpells == null)
				return new ArrayList(0).GetEnumerator();
			return m_concSpells.GetEnumerator();
		}

		#endregion
	}
}