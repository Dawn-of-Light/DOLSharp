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

using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell Helper Class that handle most static method to help spell handlers/effects.
	/// </summary>
	public static class SpellHelper
	{
		// warlock add
		
		/// <summary>
		/// Find effect by spell type / spell name
		/// </summary>
		/// <param name="target">Living to find effect on</param>
		/// <param name="spellType">Spell type to find</param>
		/// <param name="spellName">Spell name to find</param>
		/// <returns>first occurance of effect in target's effect list or null</returns>		
		public static GameSpellEffect FindEffectOnTarget(GameLiving target, string spellType, string spellName)
		{
			lock (target.EffectList)
			{
				foreach (IGameEffect fx in target.EffectList)
				{
					if (!(fx is GameSpellEffect))
						continue;
					
					GameSpellEffect effect = (GameSpellEffect)fx;
					
					// ignore immunity effects
					if (effect is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)effect).ImmunityState)
						continue;

					if (effect.SpellHandler.Spell != null && (effect.SpellHandler.Spell.SpellType == spellType) && (effect.SpellHandler.Spell.Name == spellName))
					{
						return effect;
					}
				}
			}
			return null;
		}
		
		/// <summary>
		/// Find effect by spell type
		/// </summary>
		/// <param name="target">Living to find effect on</param>
		/// <param name="spellType">Spell type to find</param>
		/// <returns>first occurance of effect in target's effect list or null</returns>
		public static GameSpellEffect FindEffectOnTarget(GameLiving target, string spellType)
		{
			if (target == null)
				return null;

			lock (target.EffectList)
			{
				foreach (IGameEffect fx in target.EffectList)
				{
					if (!(fx is GameSpellEffect))
						continue;
					
					GameSpellEffect effect = (GameSpellEffect)fx;
					// ignore immunity effects
					if (effect is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)effect).ImmunityState)
						continue;
					
					if (effect.SpellHandler.Spell != null && (effect.SpellHandler.Spell.SpellType == spellType))
					{
						return effect;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Find effect by spell handler
		/// </summary>
		/// <param name="target">Living to find effect on</param>
		/// <param name="spellHandler">Spell Handler to find (Exact Object Match)</param>
		/// <returns>first occurance of effect in target's effect list or null</returns>
		public static GameSpellEffect FindEffectOnTarget(GameLiving target, ISpellHandler spellHandler)
		{
			lock (target.EffectList)
			{
				foreach (IGameEffect effect in target.EffectList)
				{
					GameSpellEffect gsp = effect as GameSpellEffect;
					if (gsp == null)
						continue;
					
					if (gsp.SpellHandler != spellHandler)
						continue;
					
					// ignore immunity effects
					if (gsp is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)gsp).ImmunityState)
						continue;
					
					return gsp;
				}
			}
			return null;
		}

		/// <summary>
		/// Find effect by spell handler Object Type
		/// </summary>
		/// <param name="target">Living to find effect on</param>
		/// <param name="spellHandler">Spell Handler to find (Hierarchical Type Match)</param>
		/// <returns>first occurance of effect in target's effect list or null</returns>
		public static GameSpellEffect FindEffectOnTarget(GameLiving target, Type spellHandler)
		{
			lock (target.EffectList)
			{
				foreach (IGameEffect effect in target.EffectList)
				{
					GameSpellEffect gsp = effect as GameSpellEffect;
					if (gsp == null)
						continue;
					
					if (spellHandler.IsInstanceOfType(gsp.SpellHandler) == false)
						continue;
					
					// ignore immunity effects
					if (gsp is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)gsp).ImmunityState)
						continue;
					
					return gsp;
				}
			}
			return null;
		}

		/// <summary>
		/// Find effects by spellType
		/// </summary>
		/// <param name="target">Living to find effect on</param>
		/// <param name="spellHandler">Spell type to find</param>
		/// <returns>All occurances of effect in target's effect list or empty list</returns>
		public static List<GameSpellEffect> FindEffectsOnTarget(GameLiving target, string spellType)
		{
			List<GameSpellEffect> result = new List<GameSpellEffect>();
			
			if (target == null)
				return result;
			
			lock (target.EffectList)
			{
				foreach (IGameEffect effect in target.EffectList)
				{
					GameSpellEffect gsp = effect as GameSpellEffect;
					
					if (gsp == null)
						continue;
					
					 // ignore immunity effects
					 if (gsp is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)gsp).ImmunityState)
						continue;
					
					if (gsp.SpellHandler.Spell != null && (gsp.SpellHandler.Spell.SpellType == spellType))
						result.Add(gsp);
				}
			}
			
			return result;			
		}

		/// <summary>
		/// Find effects by spell handler Object Type
		/// </summary>
		/// <param name="target">Living to find effect on</param>
		/// <param name="spellHandler">Spell Handler to find (Hierarchical Type Match)</param>
		/// <returns>All occurances of effect in target's effect list or empty list</returns>
		public static List<GameSpellEffect> FindEffectsOnTarget(GameLiving target, Type spellHandler)
		{
			List<GameSpellEffect> result = new List<GameSpellEffect>();
			
			if (target == null)
				return result;
			
			lock (target.EffectList)
			{
				foreach (IGameEffect effect in target.EffectList)
				{
					GameSpellEffect gsp = effect as GameSpellEffect;
					
					if (gsp == null)
						continue;
					
					if (spellHandler.IsInstanceOfType(gsp.SpellHandler) == false)
						continue;
					
					// ignore immunity effects
					if (gsp is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)gsp).ImmunityState)
						continue;
					
					result.Add(gsp);
				}
			}
			
			return result;
		}
		
		/// <summary>
		/// Find Static Effect by Effect Type
		/// </summary>
		/// <param name="target">Living to find effect on</param>
		/// <param name="effectType">Effect Type to find</param>
		/// <returns>first occurance of effect in target's effect list or null</returns>
		public static IGameEffect FindStaticEffectOnTarget(GameLiving target, Type effectType)
		{
			if (target == null)
				return null;

			lock (target.EffectList)
			{
				foreach (IGameEffect effect in target.EffectList)
					if (effect.GetType() == effectType)
						return effect;
			}
			return null;
		}

		/// <summary>
		/// Find Immunity Effect by spellHandler Type
		/// </summary>
		/// <param name="target">Living to find effect on</param>
		/// <param name="effectType">Effect Type to find (Exact Type Match)</param>
		/// <returns>first occurance of effect in target's effect list or null</returns>
		public static GameSpellEffect FindImmunityEffectOnTarget(GameLiving target, Type spellHandler)
		{
			if (target == null)
				return null;

			lock (target.EffectList)
			{
				foreach (IGameEffect fx in target.EffectList)
				{
					if (!(fx is GameSpellEffect))
						continue;
					
					GameSpellEffect effect = (GameSpellEffect)fx;
					
					// ignore non-immunity effects
					if (effect is GameSpellAndImmunityEffect && !((GameSpellAndImmunityEffect)effect).ImmunityState)
						continue;
					
					if (effect.SpellHandler.GetType() == spellHandler)
						return effect;
				}
			}
			return null;
		}

		/// <summary>
		/// Find pulsing spell by spell handler
		/// </summary>
		/// <param name="living">Living to find effect on</param>
		/// <param name="handler">Spell Handler to find (Exact Object Match)</param>
		/// <returns>first occurance of spellhandler in targets' conc list or null</returns>
		public static PulsingSpellEffect FindPulsingSpellOnTarget(GameLiving living, ISpellHandler handler)
		{
			lock (living.ConcentrationEffects)
			{
				foreach (IConcentrationEffect concEffect in living.ConcentrationEffects)
				{
					PulsingSpellEffect pulsingSpell = concEffect as PulsingSpellEffect;
					if (pulsingSpell == null) 
						continue;
					if (pulsingSpell.SpellHandler == handler)
						return pulsingSpell;
				}
				return null;
			}
		}
	}
}
