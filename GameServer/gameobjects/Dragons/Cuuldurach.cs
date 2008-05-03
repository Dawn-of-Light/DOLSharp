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
using System.Text;
using DOL.Database2;
using DOL.Events;
using log4net;
using System.Reflection;
using System.Collections;
using DOL.AI.Brain;


namespace DOL.GS
{
	/// <summary>
	/// The Hibernia dragon.
	/// </summary>
	/// <author>Aredhel</author>
	public class Cuuldurach : GameDragon
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Add Spawns

		private ArrayList m_messengerList = new ArrayList();

		/// <summary>
		/// Spawn dogs that will despawn again after 60 seconds; there is
		/// a 25% chance that a retriever will spawn.
		/// </summary>
		/// <returns>Whether or not any retrievers were spawned.</returns>
		public override bool CheckAddSpawns()
		{
			base.CheckAddSpawns();	// In order to reset HealthPercentOld.

			GameNPC glimmerSpawn;
			bool isMessenger;
			m_messengerList.Clear();

			// Spawn glimmer mobs, in most cases (75% chance) these mobs will be around
			// level 60 and con red to purple, in some cases (25%) they will be
			// messengers, who will try to get out of the lair and, if successful,
			// cause Cuuldurach to spawn a couple of deep purple adds.

			for (int glimmer = 1; glimmer <= 10; ++glimmer)
			{
				isMessenger = Util.Chance(25);
				glimmerSpawn = SpawnTimedAdd((isMessenger) ? 620 : 621+Util.Random(2),
					(isMessenger) ? Util.Random(47, 53) : Util.Random(57, 63),
					X + Util.Random(300, 600), Y + Util.Random(300, 600), 60);

				// We got a messenger, tell it who its master is and which exit
				// to run to.

				if (isMessenger)
				{
					if (glimmerSpawn.Brain is RetrieverMobBrain)
					{
						(glimmerSpawn.Brain as RetrieverMobBrain).Master = this;
						m_messengerList.Add(glimmerSpawn);
						glimmerSpawn.WalkTo(GetExitCoordinates(Util.Random(1, 4)), 200);	// Pick 1 out of 4 possible exits.
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Get coordinates for the given exit (1 = NW, 2 = SW, 
		/// 3 = SE, 4 = NE).
		/// </summary>
		/// <returns>Coordinates.</returns>
		private Point3D GetExitCoordinates(int exitNo)
		{
			// Get target coordinates (hardcoded). Yeah I know, this is
			// ugly, but to get this right NPC pathing is a must; as it
			// is at the moment, there is no way of knowing where the exits 
			// are (from the PoV of an NPC).

			switch (exitNo)
			{
				case 1: return new Point3D(407292, 704008, 0);
				case 2: return new Point3D(406158, 707745, 0);
				case 3: return new Point3D(410302, 708563, 0);
				case 4: return new Point3D(411117, 704696, 0);
				default: return new Point3D(SpawnX, SpawnY, SpawnZ);
			}
		}

		/// <summary>
		/// Invoked when retriever type mob has reached its target location.
		/// </summary>
		/// <param name="sender">The retriever mob.</param>
		public override void OnRetrieverArrived(GameNPC sender)
		{
			base.OnRetrieverArrived(sender);
			if (sender == null || sender == this) return;

			// Spawn nasty adds.

			if (m_messengerList.Contains(sender))
				SpawnGlimmers(Util.Random(7, 10), sender.X, sender.Y);
		}

		/// <summary>
		/// Spawn a couple of nasty level 62-68 glimmer mobs around the spot the
		/// retriever has reported back from, then make these spawns aggro the
		/// raid inside the lair.
		/// </summary>
		/// <param name="numAdds"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		private void SpawnGlimmers(int numAdds, int x, int y)
		{
			GameNPC glimmer;
			for (int add = 0; add < numAdds; ++add)
			{
				glimmer = SpawnTimedAdd(624+Util.Random(2), Util.Random(62, 68),
					x + Util.Random(250), y + Util.Random(250), 120);
				if (glimmer.Brain is StandardMobBrain && this.Brain is DragonBrain)
					(Brain as DragonBrain).AddAggroListTo(glimmer.Brain as StandardMobBrain);
			}
		}
		 
		#endregion

		#region Glare

		/// <summary>
		/// The Glare spell.
		/// </summary>
		protected override Spell Glare
		{
			get
			{
				if (m_glareSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 0;
					spell.ClientEffect = 5702;
					spell.Description = "Glare";
					spell.Name = "Dragon Glare";
					spell.Range = 2500;
					spell.Radius = 700;
					spell.Damage = 2000;
					spell.RecastDelay = 10;
					spell.DamageType = (int)eDamageType.Spirit;
					spell.SpellID = 6021;
					spell.Target = "Enemy";
					spell.Type = "DirectDamage";
					m_glareSpell = new Spell(spell, 70);
					SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells).Add(m_glareSpell);
				}
				return m_glareSpell;
			}
		}

		#endregion

		#region Breath

		/// <summary>
		/// The Breath spell.
		/// </summary>
		protected override Spell Breath
		{
			get
			{
				if (m_breathSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 0;
					spell.Uninterruptible = true;
					spell.ClientEffect = 4468;
					spell.Description = "Nuke";
					spell.Name = "Dragon Nuke";
					spell.Range = 700;
					spell.Radius = 700;
					spell.Damage = 2000;
					spell.DamageType = (int)eDamageType.Spirit;
					spell.SpellID = 6022;
					spell.Target = "Enemy";
					spell.Type = "DirectDamage";
					m_breathSpell = new Spell(spell, 70);
					SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells).Add(m_breathSpell);
				}
				return m_breathSpell;
			}
		}

		/// <summary>
		/// The resist debuff spell.
		/// </summary>
		protected override Spell ResistDebuff
		{
			get
			{
				if (m_resistDebuffSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 0;
					spell.Uninterruptible = true;
					spell.ClientEffect = 4576;
					spell.Icon = 4576;
					spell.Description = "Spirit Resist Debuff";
					spell.Name = "Feeble Will";
					spell.Range = 700;
					spell.Radius = 700;
					spell.Value = 30;
					spell.Duration = 30;
					spell.Damage = 0;
					spell.DamageType = (int)eDamageType.Heat;
					spell.SpellID = 6023;
					spell.Target = "Enemy";
					spell.Type = "SpiritResistDebuff";
					spell.Message1 = "You feel more vulnerable to spirit magic!";
					spell.Message2 = "{0} seems vulnerable to spirit magic!";
					m_resistDebuffSpell = new Spell(spell, 70);
					SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells).Add(m_resistDebuffSpell);
				}
				return m_resistDebuffSpell;
			}
		}

		#endregion

		#region Melee Debuff

		/// <summary>
		/// The melee debuff spell.
		/// </summary>
		protected override Spell MeleeDebuff
		{
			get
			{
				if (m_meleeDebuffSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 0;
					spell.Uninterruptible = true;
					spell.ClientEffect = 13082;
					spell.Description = "Fumble Chance Debuff";
					spell.Name = "Growing Trepidation";
					spell.Range = 700;
					spell.Radius = 700;
					spell.Value = 50;
					spell.Duration = 90;
					spell.Damage = 0;
					spell.DamageType = (int)eDamageType.Heat;
					spell.SpellID = 6003;
					spell.Target = "Enemy";
					spell.Type = "FumbleChanceDebuff";
					m_meleeDebuffSpell = new Spell(spell, 70);
					SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells).Add(m_meleeDebuffSpell);
				}
				return m_meleeDebuffSpell;
			}
		}

		#endregion

		#region Ranged Debuff

		/// <summary>
		/// The ranged debuff spell.
		/// </summary>
		protected override Spell RangedDebuff
		{
			get
			{
				if (m_rangedDebuffSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 0;
					spell.Uninterruptible = true;
					spell.ClientEffect = 590;
					spell.Description = "Nearsight";
					spell.Name = "Dazzling Light";
					spell.Range = 700;
					spell.Radius = 700;
					spell.Value = 100;
					spell.Duration = 90;
					spell.Damage = 0;
					spell.DamageType = (int)eDamageType.Heat;
					spell.SpellID = 6003;
					spell.Target = "Enemy";
					spell.Type = "Nearsight";
					spell.Message1 = "You are blinded!";
					spell.Message2 = "{0} is blinded!";
					m_rangedDebuffSpell = new Spell(spell, 70);
					SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells).Add(m_rangedDebuffSpell);
				}
				return m_rangedDebuffSpell;
			}
		}

		#endregion
	}
}