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
using System.Collections;
using System;
using System.Reflection;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using DOL.GS.Spells;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Summary description for a Weapon
	/// </summary> 
	public abstract class Weapon : VisibleEquipment
	{
		#region Declaraction
		/// <summary>
		/// The DPS of the weapon
		/// </summary>
		private byte m_damagePerSecond;

		/// <summary>
		/// The SPD of the weapon (in milli sec)
		/// </summary>
		private int m_speed;

		/// <summary>
		/// The damage type of the weapon
		/// </summary>
		protected eDamageType m_damageType;

		/// <summary>
		/// Witch hand(s) are needed to use this weapon
		/// </summary>
		protected eHandNeeded m_handNeeded;

		/// <summary>
		/// The glow effect id show with this weapon template
		/// </summary>
		private int m_glowEffect;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the DPS of this weapon
		/// </summary>
		public byte DamagePerSecond
		{
			get { return m_damagePerSecond; }
			set	{ m_damagePerSecond = value; }
		}

		/// <summary>
		/// Gets or sets the SPD of this weapon (in milli sec)
		/// </summary>
		public int Speed
		{
			get { return m_speed; }
			set	{ m_speed = value; }
		}

		/// <summary>
		/// Gets or sets the damage type of this weapon
		/// </summary>
		public virtual eDamageType DamageType
		{
			get { return m_damageType; }
			set	{ m_damageType = value; }
		}

		/// <summary>
		/// Gets or sets the glow effect to show with this weapon
		/// </summary>
		public int GlowEffect
		{
			get { return m_glowEffect; }
			set	{ m_glowEffect = value; }
		}

		/// <summary>
		/// Gets or sets which hand(s) are needed to use this weapon
		/// </summary>
		public virtual eHandNeeded HandNeeded
		{
			get { return m_handNeeded; }
			set	{ m_handNeeded = value; }
		}

		#endregion

		#region Methods
		/// <summary>
		/// Called when the item hit something or is hitted by something
		/// </summary>
		public override void OnItemHit(GameLivingBase target, bool isHitted)
		{
			base.OnItemHit(target, isHitted);

			if(!isHitted) // when the weapon hit start active effect and poison
			{
				if(ProcEffectType == eMagicalEffectType.ActiveEffect)
				{
					// random chance (4.0spd = 10%)
					if (!Util.ChanceDouble(Speed*0.0025))
						return;

					SpellLine procEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (procEffectLine != null)
					{
						IList spells = SkillBase.GetSpellList(procEffectLine.KeyName);
						if (spells != null)
						{
							foreach (Spell spell in spells)
							{
								if (spell.ID == ProcSpellID)
								{
									if(spell.Level <= Level)
									{
										ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(Owner, spell, procEffectLine);
										if (spellHandler != null)
										{
											spellHandler.StartSpell(target);
										}
									}
									break;
								}
							}
						}
					}
				}
				else if(ChargeEffectType == eMagicalEffectType.PoisonEffect)
				{
					SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
					if (poisonLine != null)
					{
						IList spells = SkillBase.GetSpellList(poisonLine.KeyName);
						if (spells != null)
						{
							foreach (Spell spell in spells)
							{
								if (spell.ID == ChargeSpellID)
								{
									if(spell.Level <= Level)
									{
										ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(Owner, spell, poisonLine);
										if (spellHandler != null)
										{
											spellHandler.StartSpell(target);
										}
									}
									break;
								}
							}
						}
					}
					Charge --;
					if(Charge <= 0)
					{
						ChargeEffectType = eMagicalEffectType.NoEffect;
					}
				}
			}
		}
		#endregion

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				ArrayList list = (ArrayList) base.DelveInfo;

				double itemDPS = DamagePerSecond/10.0;
				double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * Owner.Level);
				double itemSPD = Speed/1000.0;
				double effectiveDPS = itemDPS * Quality/100.0 * Condition/100;

				list.Add(" ");
				list.Add(" ");
				list.Add("Damage Modifiers:");
				list.Add("- " + itemDPS.ToString("0.0") + " Base DPS");
				list.Add("- " + clampedDPS.ToString("0.0") + " Clamped DPS");
				list.Add("- " + itemSPD.ToString("0.0") + " Weapon Speed");
				list.Add("- " + Quality + "% Quality");
				list.Add("- " + (byte)Condition + "% Condition");
				
				switch(DamageType)
				{
					case eDamageType.Crush:  list.Add("Damage Type: Crush"); break;
					case eDamageType.Slash:  list.Add("Damage Type: Slash"); break;
					case eDamageType.Thrust: list.Add("Damage Type: Thrust"); break;
					default: list.Add("Damage Type: Natural"); break;
				}

				list.Add(" ");
				list.Add("Effective Damage:");
				list.Add("- " + effectiveDPS.ToString("0.0") + " DPS");
				
				return list;
			}
		}
	}
}	