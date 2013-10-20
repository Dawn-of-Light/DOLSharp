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
using System.Threading;
using DOL.Database;
using DOL.AI.Brain;
using DOL.GS.Spells;

using log4net;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Holds &amp; manages multiple effects on livings
	/// when iterating over this effect list lock the list!
	/// </summary>
	public class GameEffectList : IEnumerable<IGameEffect>
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Stores all effects
		/// </summary>
		protected List<IGameEffect> m_effects = new List<IGameEffect>();

		public object SyncRoot
		{
			get
			{
				return ((ICollection)m_effects).SyncRoot;
			}
		}
		
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
			if (!m_owner.IsAlive || m_owner.ObjectState != GameObject.eObjectState.Active)
				return false;	// dead owners don't get effects

			if (m_effects == null)
				m_effects = new List<IGameEffect>(5);
			
			lock (SyncRoot)
			{
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
			List<IGameEffect> changedEffects = new List<IGameEffect>();

			if (m_effects == null)
				return false;

			lock (SyncRoot)
			{
				int index = m_effects.IndexOf(effect);
				if (index < 0)
					return false;
				m_effects.RemoveAt(index);
				for (int i = index; i < m_effects.Count; i++)
				{
					changedEffects.Add(m_effects[i]);
				}
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
			IList<IGameEffect> fx;

			if (m_effects == null)
				return;
			lock (SyncRoot)
			{
				fx = new List<IGameEffect>(m_effects);
				m_effects.Clear();
			}
			BeginChanges();
			fx.ForEach(effect => effect.Cancel(false));
			CommitChanges();
		}

		public virtual void RestoreAllEffects()
		{
			GamePlayer player = m_owner as GamePlayer;
			if (player == null || player.DBCharacter == null || GameServer.Database == null)
				return;

			var effs = GameServer.Database.SelectObjects<PlayerXEffect>("ChardID = '" + GameServer.Database.Escape(player.DBCharacter.ObjectId) + "'");
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

		public virtual void SaveAllEffects()
		{
			GamePlayer player = m_owner as GamePlayer;
			if (player == null)
				return;
			if (m_effects == null)
				return;
			lock (SyncRoot)
			{
				if (m_effects.Count < 1)
					return;

				foreach (IGameEffect eff in m_effects)
				{
					if (eff is GameSpellEffect)
					{
						GameSpellEffect gse = eff as GameSpellEffect;
						if (gse.Concentration > 0 && gse.SpellHandler.Caster != player)
							continue;
					}
					PlayerXEffect effx = eff.getSavedEffect();
					if (effx == null)
						continue;

					if (effx.SpellLine == GlobalSpellsLines.Reserved_Spells)
						continue;

					effx.ChardID = player.DBCharacter.ObjectId;
					GameServer.Database.AddObject(effx);
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
					log.Warn("changes count is less than zero, forgot BeginChanges()?\n" + Environment.StackTrace);

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

			lock (SyncRoot)
			{
				foreach (IGameEffect effect in m_effects)
					if (effect.GetType().Equals(typeof(T)))
						return (T)effect;
			}
			return default(T);
		}

		/// <summary>
		/// Find effects of specific type
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns>resulting effectlist</returns>
		public virtual IList<T> GetAllOfType<T>() where T : IGameEffect
		{
			if (m_effects == null)
				return new List<T>();

			lock (SyncRoot)
			{
				return m_effects.Where(effect => effect.GetType().Equals(typeof (T))).Cast<T>().ToList();
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

			lock (SyncRoot)
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

			lock (SyncRoot)
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
			if (m_effects == null) return null;
			lock (SyncRoot)
			{
				foreach (IGameEffect effect in m_effects)
					if (effect.GetType().Equals(effectType)) return effect;
			}
			return null;
		}

		/// <summary>
		/// Find effects of specific type
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns>resulting effectlist</returns>
		public virtual IList GetAllOfType(Type effectType)
		{
			List<IGameEffect> list = new List<IGameEffect>();

			if (m_effects == null) return list;

			lock (SyncRoot)
			{
				foreach (IGameEffect effect in m_effects)
					if (effect.GetType().Equals(effectType)) list.Add(effect);
			}
			return list;
		}

		/// <summary>
		/// Gets count of all stored effects
		/// </summary>
		public int Count
		{
			get { return m_effects == null ? 0 : m_effects.Count; }
		}

		#region IEnumerable Member

		/// <summary>
		/// Thread safe iterator
		/// </summary>
		public struct GameEffectEnumerator : IEnumerator<IGameEffect>
		{
			private readonly GameEffectList _list;
			private readonly IEnumerator<IGameEffect> _enumerator;

			public GameEffectEnumerator(GameEffectList list)
			{
				_list = list;
				_enumerator = _list.m_effects.GetEnumerator();
				//Monitor.Enter(_list.m_effects);
			}

			public void Dispose()
			{
				try
				{
					_enumerator.Dispose();
				}
				finally 
				{
					//Monitor.Exit(_list.m_effects);
				}
			}

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Reset()
			{
				_enumerator.Reset();
			}

			public IGameEffect Current { get { return _enumerator.Current; } }

			object IEnumerator.Current { get { return _enumerator.Current; } }
		}

		/// <summary>
		/// Returns an enumerator for the effects
		/// </summary>
		/// <returns></returns>
		public IEnumerator<IGameEffect> GetEnumerator()
		{
			if (m_effects == null)
				return new List<IGameEffect>().GetEnumerator();
			return new GameEffectEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion
	}
}
