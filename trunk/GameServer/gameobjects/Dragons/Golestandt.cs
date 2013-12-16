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
	/// <summary>
	/// The Albion dragon.
	/// </summary>
	/// <author>Aredhel</author>    
	public class Golestandt : GameDragon
    {
		#region Add Spawns

		/// <summary>
		/// Spawn adds that will despawn again after 30 seconds.
		/// For Golestandt, these will be level 57-60 GameNPCs and 
		/// their numbers will depend on the number of players inside 
		/// the lair.
		/// </summary>
		/// <returns>Whether or not any adds were spawned.</returns>
		public override bool CheckAddSpawns()
		{
			base.CheckAddSpawns();	// In order to reset HealthPercentOld.

			int numAdds = Math.Max(1, PlayersInLair / 2);
			for (int add = 1; add <= numAdds; ++add)
			{
				SpawnTimedAdd(600, Util.Random(57, 60),	X + Util.Random(300, 600), Y + Util.Random(300, 600), 30, false);	// granite giant pounder lvl 57-60
			}
			return true;
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
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.ClientEffect = 5700;
                    spell.Description = "Glare";
                    spell.Name = "Dragon Glare";
                    spell.Range = 2500;
                    spell.Radius = 700;
                    spell.Damage = 2000* DragonDifficulty /100;
					spell.RecastDelay = 10;
                    spell.DamageType = (int)eDamageType.Heat;
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
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.Uninterruptible = true;
                    spell.ClientEffect = 2308;
                    spell.Description = "Nuke";
                    spell.Name = "Dragon Nuke";
                    spell.Range = 700;
                    spell.Radius = 700;
                    spell.Damage = 2000* DragonDifficulty /100;
                    spell.DamageType = (int)eDamageType.Heat;
                    spell.SpellID = 6002;
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
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.Uninterruptible = true;
                    spell.ClientEffect = 777;
                    spell.Icon = 777;
                    spell.Description = "Heat Resist Debuff";
                    spell.Name = "Melt Armor";
                    spell.Range = 700;
                    spell.Radius = 700;
                    spell.Value = 30* DragonDifficulty/100;
                    spell.Duration = 30;
                    spell.Damage = 0;
                    spell.DamageType = (int)eDamageType.Heat;
                    spell.SpellID = 6003;
                    spell.Target = "Enemy";
                    spell.Type = "HeatResistDebuff";
                    spell.Message1 = "You feel more vulnerable to heat!";
                    spell.Message2 = "{0} seems vulnerable to heat!";
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
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Uninterruptible = true;
					spell.ClientEffect = 13082;
					spell.Icon = 13082;
					spell.Description = "Fumble Chance Debuff";
					spell.Name = "Growing Trepidation";
					spell.Range = 700;
					spell.Radius = 700;
					spell.Value = 50;
					spell.Duration = 90* DragonDifficulty /100;
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
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Uninterruptible = true;
					spell.ClientEffect = 590;
					spell.Icon = 590;
					spell.Description = "Nearsight";
					spell.Name = "Dazzling Light";
					spell.Range = 700;
					spell.Radius = 700;
					spell.Value = 100;
					spell.Duration = 90* DragonDifficulty /100;
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
