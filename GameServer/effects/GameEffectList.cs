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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DOL.Database;
using DOL.AI.Brain;
using DOL.GS.Spells;

using log4net;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Holds and manages multiple effects on livings
	/// when iterating over this effect list lock the list!
	/// </summary>
	public class GameEffectList : IEnumerable<IGameEffect>
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Default Lock Object
		/// </summary>
		private readonly object m_lockObject = new object();
		
		/// <summary>
		/// Stores all effects
		/// </summary>
		protected List<IGameEffect> m_effects;

		/// <summary>
		/// The owner of this list
		/// </summary>
		protected readonly GameLiving m_owner;

		/// <summary>
		/// The current unique effect ID
		/// </summary>
		protected ushort m_runningID = 1;

		/// <summary>
		/// The count of started changes to the list
		/// </summary>
		protected volatile sbyte m_changesCount;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="owner"></param>
		public GameEffectList(GameLiving owner)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");
			m_owner = owner;
		}

		/// <summary>
		/// add a new effect to the effectlist, it does not start the effect
		/// </summary>
		/// <param name="effect">The effect to add to the list</param>
		/// <returns>true if the effect was added</returns>
		public virtual bool Add(IGameEffect effect)
		{
			// dead owners don't get effects
			if (!m_owner.IsAlive || m_owner.ObjectState != GameObject.eObjectState.Active)
				return false;

			lock (m_lockObject)
			{
				if (m_effects == null)
					m_effects = new List<IGameEffect>(5);
				
				effect.InternalID = m_runningID++;
				
				if (m_runningID == 0)
					m_runningID = 1;
				
				m_effects.Add(effect);
			}

			OnEffectsChanged(effect);

			return true;
		}

		/// <summary>
		/// remove effect
		/// </summary>
		/// <param name="effect">The effect to remove from the list</param>
		/// <returns>true if the effect was removed</returns>
		public virtual bool Remove(IGameEffect effect)
		{
			if (m_effects == null)
				return false;

			List<IGameEffect> changedEffects = new List<IGameEffect>();

			lock (m_lockObject) // Mannen 10:56 PM 10/30/2006 - Fixing every lock ('this')
			{
				int index = m_effects.IndexOf(effect);
				
				if (index < 0)
					return false;
				
				m_effects.RemoveAt(index);
				
				// Register remaining effects for change
				changedEffects.AddRange(m_effects.Skip(index));
			}

			BeginChanges();
			changedEffects.ForEach(OnEffectsChanged);
			CommitChanges();
			return true;
		}

		/// <summary>
		/// Cancels all effects
		/// </summary>
		public virtual void CancelAll()
		{
			IGameEffect[] fx;

			if (m_effects == null)
				return;
			
			lock (m_lockObject)
			{
				fx = m_effects.ToArray();
				m_effects.Clear();
			}
			
			BeginChanges();
			fx.ForEach(effect => effect.Cancel(false));
			CommitChanges();
		}

		/// <summary>
		/// Restore All Effect From PlayerXEffect Data Table
		/// </summary>
		public virtual void RestoreAllEffects()
		{
			GamePlayer player = m_owner as GamePlayer;
			
			if (player == null || player.DBCharacter == null || GameServer.Database == null)
				return;

			var effs = GameServer.Database.SelectObjects<PlayerXEffect>("ChardID = '" + GameServer.Database.Escape(player.ObjectId) + "'");
			if (effs == null)
				return;

			foreach (PlayerXEffect eff in effs)
			{
				if (eff.SpellLine == GlobalSpellsLines.Reserved_Spells)
					continue;

				bool good = true;
				Spell spell = SkillBase.GetSpellByID(eff.Var1);

				if (spell == null)
					good = false;

				SpellLine line = null;

				if (!Util.IsEmpty(eff.SpellLine))
				{
					line = SkillBase.GetSpellLine(eff.SpellLine, false);
					if (line == null)
					{
						good = false;
					}
				}
				else
				{
					good = false;
				}

				if (good)
				{
					ISpellHandler handler = ScriptMgr.CreateSpellHandler(player, spell, line);
					GameSpellEffect e;
					e = new GameSpellEffect(handler, eff.Duration, spell.Frequency);
					e.RestoredEffect = true;
					int[] vars = { eff.Var1, eff.Var2, eff.Var3, eff.Var4, eff.Var5, eff.Var6 };
					e.RestoreVars = vars;
					e.Start(player);
				}

				GameServer.Database.DeleteObject(eff);
			}
		}

		/// <summary>
		/// Save All Effect to PlayerXEffect Data Table
		/// </summary>
		public virtual void SaveAllEffects()
		{
			GamePlayer player = m_owner as GamePlayer;
			
			if (player == null || m_effects == null)
				return;

			lock (m_lockObject)
			{
				if (m_effects.Count < 1)
					return;

				foreach (IGameEffect eff in m_effects)
				{
					try
					{
						var gse = eff as GameSpellEffect;
						if (gse != null)
						{
							// No concentration Effect from Other Caster
							if (gse.Concentration > 0 && gse.SpellHandler.Caster != player)
								continue;
						}
						
						PlayerXEffect effx = eff.getSavedEffect();
						
						if (effx == null)
							continue;
	
						if (effx.SpellLine == GlobalSpellsLines.Reserved_Spells)
							continue;
	
						effx.ChardID = player.ObjectId;
						
						GameServer.Database.AddObject(effx);
					}
					catch (Exception e)
					{
						if (log.IsWarnEnabled)
							log.WarnFormat("Could not save effect ({0}) on player: {1}, {2}", eff, player, e);
					}
				}
			}
		}

		/// <summary>
		/// Called when an effect changed
		/// </summary>
		public virtual void OnEffectsChanged(IGameEffect changedEffect)
		{
			if (m_changesCount > 0)
				return;

			UpdateChangedEffects();
		}

		/// <summary>
		/// Begins multiple changes to the list that should not send updates
		/// </summary>
		public void BeginChanges()
		{
			m_changesCount++;
		}

		/// <summary>
		/// Updates all list changes to the owner since BeginChanges was called
		/// </summary>
		public virtual void CommitChanges()
		{
			if (--m_changesCount < 0)
			{
				if (log.IsWarnEnabled)
					log.WarnFormat("changes count is less than zero, forgot BeginChanges()?\n{0}", Environment.StackTrace);

				m_changesCount = 0;
			}

			if (m_changesCount == 0)
				UpdateChangedEffects();
		}

		/// <summary>
		/// Updates the changed effects.
		/// </summary>
		protected virtual void UpdateChangedEffects()
		{
			if (m_owner is GameNPC)
			{
				IControlledBrain npc = ((GameNPC)m_owner).Brain as IControlledBrain;
				if (npc != null)
					npc.UpdatePetWindow();
			}
		}

		/// <summary>
		/// Find the first occurence of an effect with given type
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns>effect or null</returns>
		public virtual T GetOfType<T>() where T : IGameEffect
		{
			if (m_effects == null)
				return default(T);

			lock (m_lockObject)
			{
				return (T)m_effects.FirstOrDefault(effect => effect.GetType().Equals(typeof(T)));
			}
		}

		/// <summary>
		/// Find effects of specific type
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns>resulting effectlist</returns>
		public virtual ICollection<T> GetAllOfType<T>() where T : IGameEffect
		{
			if (m_effects == null)
				return new T[0];

			lock (m_lockObject) // Mannen 10:56 PM 10/30/2006 - Fixing every lock ('this')
			{
				return m_effects.Where(effect => effect.GetType().Equals(typeof(T))).Cast<T>().ToArray();
			}
		}

		/// <summary>
		/// Count effects of a specific type
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns></returns>
		public int CountOfType<T>() where T : IGameEffect
		{
			if (m_effects == null)
				return 0;

			lock (m_lockObject) // Mannen 10:56 PM 10/30/2006 - Fixing every lock ('this')
			{
				return m_effects.Count(effect => effect.GetType().Equals(typeof(T)));
			}
		}

		/// <summary>
		/// Count effects of a specific type
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns></returns>
		public int CountOfType(params Type[] types)
		{
			if (m_effects == null)
				return 0;

			lock (m_lockObject) // Mannen 10:56 PM 10/30/2006 - Fixing every lock ('this')
			{
				return m_effects.Join(types, e => e.GetType(), t => t, (e, t) => e).Count();
			}
		}

		/// <summary>
		/// Find the first occurence of an effect with given type
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns>effect or null</returns>
		public virtual IGameEffect GetOfType(Type effectType)
		{
			if (m_effects == null)
				return null;
			
			lock (m_lockObject)
			{
				return m_effects.FirstOrDefault(effect => effect.GetType().Equals(effectType));
			}
		}

		/// <summary>
		/// Find effects of specific type
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns>resulting effectlist</returns>
		public virtual ICollection<IGameEffect> GetAllOfType(Type effectType)
		{
			if (m_effects == null)
				return new IGameEffect[0];

			lock (m_lockObject) // Mannen 10:56 PM 10/30/2006 - Fixing every lock ('this')
			{
				return m_effects.Where(effect => effect.GetType().Equals(effectType)).ToArray();
			}
		}

		/// <summary>
		/// Gets count of all stored effects
		/// </summary>
		public int Count
		{
			get
			{
				if (m_effects == null)
					return 0;
				
				lock (m_lockObject)
				{
					return m_effects.Count;
				}
			}
		}

		#region IEnumerable Member

		/// <summary>
		/// Returns an enumerator for the effects
		/// </summary>
		/// <returns></returns>
		public IEnumerator<IGameEffect> GetEnumerator()
		{
			if (m_effects == null)
				return new IGameEffect[0].AsEnumerable().GetEnumerator();
			
			lock (m_lockObject)
			{
				return m_effects.ToArray().AsEnumerable().GetEnumerator();
			}
		}

		/// <summary>
		/// Returns an enumerator for the effects
		/// </summary>
		/// <returns></returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion
	}
}
