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
using DOL.Database;
using DOL.Events;
using log4net;
using System.Reflection;
using System.Collections;


namespace DOL.GS
{
	public class Gjalpinulva : GameDragon
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Add Spawns

		/// <summary>
		/// Spawn adds that will despawn again after 30 seconds.
		/// For Gjalpinulva, these will be level 40-45 GameNPCs and 
		/// their numbers will depend on the number of players inside 
		/// the lair.
		/// </summary>
		/// <returns>Whether or not any adds were spawned.</returns>
		public override bool CheckAddSpawns()
		{
			base.CheckAddSpawns();	// In order to reset HealthPercentOld.

			int numAdds = Math.Max(1, PlayersInLair / 2);
			numAdds = 10; // TEST
			GameNPC spawn;
			for (int add = 1; add <= numAdds; ++add)
			{
				spawn = SpawnTimedAdd(610, Util.Random(40, 45), 30);		// scaled retriever lvl 40-45
				spawn.WalkTo(GetExitCoordinates(Util.Random(1, 4)), 200);	// Pick 1 out of 4 possible exits.
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
			// Get target coordinates about 3500 units away from dragon
			// spawn point, this is not correct, because the exits don't
			// lie exactly to the northwest etc., but it will have to do
			// for the time being.

			int dx = 2475, dy = 2475;
			switch (exitNo)
			{
				case 1: return new Point3D(SpawnX - dx, SpawnY - dy, SpawnZ);
				case 2: return new Point3D(SpawnX - dx, SpawnY + dy, SpawnZ);
				case 3: return new Point3D(SpawnX + dx, SpawnY + dy, SpawnZ);
				case 4: return new Point3D(SpawnX + dx, SpawnY - dy, SpawnZ);
				default: return new Point3D(SpawnX, SpawnY, SpawnZ);
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
					spell.ClientEffect = 161;
					spell.Description = "Glare";
					spell.Name = "Dragon Glare";
					spell.Range = 2500;
					spell.Radius = 700;
					spell.Damage = 2000;
					spell.RecastDelay = 10;
					spell.DamageType = (int)eDamageType.Cold;
					spell.SpellID = 6001;
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
					spell.ClientEffect = 4568;
					spell.Description = "Nuke";
					spell.Name = "Dragon Nuke";
					spell.Range = 700;
					spell.Radius = 700;
					spell.Damage = 2000;
					spell.DamageType = (int)eDamageType.Cold;
					spell.SpellID = 6012;
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
					spell.ClientEffect = 778;
					spell.Icon = 778;
					spell.Description = "Cold Resist Debuff";
					spell.Name = "Biting Cold";
					spell.Range = 700;
					spell.Radius = 700;
					spell.Value = 30;
					spell.Duration = 30;
					spell.Damage = 0;
					spell.DamageType = (int)eDamageType.Heat;
					spell.SpellID = 6013;
					spell.Target = "Enemy";
					spell.Type = "ColdResistDebuff";
					spell.Message1 = "You feel more vulnerable to cold!";
					spell.Message2 = "{0} seems vulnerable to cold!";
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
					spell.Icon = 13082;
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
					spell.Icon = 590;
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