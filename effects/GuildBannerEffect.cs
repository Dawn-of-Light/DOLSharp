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
using DOL.Language;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Base for all GuildBannerEffects. Use CreateEffectOfClass to get instances.
	/// </summary>
	public abstract class GuildBannerEffect : TimedEffect
	{
		//Reference:
		//http://support.darkageofcamelot.com/kb/article.php?id=786

		// - Spells on banners will emit >SHORT< duration PBAOE buff spells around the banner's carrier.
		// Using standard pulsing spell pulse of 6 seconds, duration of 8 seconds - Tolakram
		protected const int duration = 8000; 

		/// <summary>
		/// Starts the matching GuildBannerEffect with type (by carrierCharacterClass) and effectiveness
		/// </summary>
		/// <param name="carrier"></param>
		/// <param name="target"></param>
		public static GuildBannerEffect CreateEffectOfClass (GamePlayer carrier, GamePlayer target) {
			
			//calculate effectiveness to target
			double effectiveness = 0;
			if (carrier == target) effectiveness = 1;
			else if (carrier.Guild != null && target.Guild != null && carrier.Guild == target.Guild)
				effectiveness = 1;
			else if (carrier.Group != null && target.Group != null
				&& carrier.Group.IsInTheGroup(target))
				effectiveness = 0.5;

			#region Get new classdependend effect
			switch ((eCharacterClass)carrier.DBCharacter.Class) {
				case eCharacterClass.Wizard: 
				case eCharacterClass.Theurgist:
				case eCharacterClass.Sorcerer:
				case eCharacterClass.Cabalist:
				case eCharacterClass.Spiritmaster:
				case eCharacterClass.Bonedancer:
				case eCharacterClass.Runemaster:
				case eCharacterClass.Warlock:
                case eCharacterClass.Animist:
                case eCharacterClass.Eldritch:
                case eCharacterClass.Enchanter:
                case eCharacterClass.Mentalist:
					return new BannerOfWardingEffect(effectiveness);
				case eCharacterClass.Armsman:
				case eCharacterClass.Mercenary:
				case eCharacterClass.Reaver:
				case eCharacterClass.Paladin:
				case eCharacterClass.Warrior:
				case eCharacterClass.Berserker:
				case eCharacterClass.Savage:
                case eCharacterClass.Hero:
                case eCharacterClass.Champion:
                case eCharacterClass.Vampiir:
					return new BannerOfShieldingEffect(effectiveness);					
				case eCharacterClass.Necromancer:
				case eCharacterClass.Friar:
				case eCharacterClass.Infiltrator:
				case eCharacterClass.Scout:
				case eCharacterClass.Shadowblade:
				case eCharacterClass.Hunter:
				case eCharacterClass.Valkyrie:
				case eCharacterClass.Thane:
				case eCharacterClass.Ranger:
				case eCharacterClass.Nightshade:
				case eCharacterClass.Valewalker:
				case eCharacterClass.Warden:
					return new BannerOfFreedomEffect(effectiveness);					
				case eCharacterClass.Cleric:
				case eCharacterClass.Heretic:
				case eCharacterClass.Minstrel:
				case eCharacterClass.Healer:
				case eCharacterClass.Shaman:
				case eCharacterClass.Skald:
				case eCharacterClass.Druid:
				case eCharacterClass.Bard:
				case eCharacterClass.Bainshee:
					return new BannerOfBesiegingEffect(effectiveness);
				default: return null;
			#endregion
			}

		}

		#region Properties
		double m_effectiveness;
		/// <summary>
		/// Returns the effectiveness of this GuildBannerEffect to be compareable.
		/// </summary>
		public double Effectiveness
		{
			get { return m_effectiveness; }
		}
		#endregion

		#region Delve
		protected abstract string Description { get; }

		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>(4);
				list.Add(Description);
				list.AddRange(base.DelveInfo);

				return list;
			}
		}
		#endregion

		#region ctor
		public GuildBannerEffect(double effectiveness) : base(duration)
		{
			m_effectiveness = effectiveness;
		}
		#endregion
	}

	
	/// <summary>
	/// Banner of Warding Effect
	/// </summary>
	public class BannerOfWardingEffect : GuildBannerEffect
	{
		// - Spell Resist Banner - Banner of Warding: 10% bonus to all magic resistances. (Note: This stacks with other effects.)

		#region visual overrides
		public override string Name
		{
			get
			{
				return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.BannerOfWardingEffect.Name");
			}
		}

		protected override string Description
		{
			get { return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.BannerOfWardingEffect.Description"); }
		}

		//5949,Spell Resist Banner,54,0,0,0,0,0,0,0,0,0,0,0,13,0,332,,,
		public override ushort Icon
		{
			get
			{
				return 54;
			}
		}
		#endregion

		#region effect
		public override void Start(GameLiving m_owner)
		{
			int effValue = (int)(Effectiveness*10);
			base.Start(m_owner);
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Body] += effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Cold] += effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Energy] += effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Heat] += effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Matter] += effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Spirit] += effValue;

		}

		public override void Stop()
		{
			int effValue = (int)(Effectiveness * 10);
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Body] -= effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Cold] -= effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Energy] -= effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Heat] -= effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Matter] -= effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Spirit] -= effValue;
			base.Stop();
		}
		#endregion

		#region ctor
		public BannerOfWardingEffect(double effectiveness) : base(effectiveness) { }
		#endregion
	}

	/// <summary>
	/// Banner of Shielding
	/// </summary>
	public class BannerOfShieldingEffect : GuildBannerEffect
	{
		//- Melee Resist Banner - Banner of Shielding: 6% bonus to all melee resistances. (Note: This stacks with other effects.)


		#region visual overrides
		public override string Name
		{
			get
			{
				return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.BannerOfShieldingEffect.Name");
			}
		}

		protected override string Description
		{
			get { return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.BannerOfShieldingEffect.Description"); }
		}

		//5950,Melee Resist Banner,49,0,0,0,0,0,0,0,0,0,0,0,13,0,332,,,
		public override ushort Icon
		{
			get
			{
				return 49;
			}
		}
		#endregion

		#region effect
		public override void Start(GameLiving target)
		{
			base.Start(target);
			int effValue = (int)(Effectiveness * 6);			
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Crush] += effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Slash] += effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Thrust] += effValue;


		}

		public override void Stop()
		{
			int effValue = (int)(Effectiveness * 6);
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Crush] -= effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Slash] -= effValue;
			m_owner.BuffBonusCategory4[(int)eProperty.Resist_Thrust] -= effValue;
			base.Stop();
		}
		#endregion

		#region ctor
		public BannerOfShieldingEffect(double effectiveness) : base(effectiveness) { }
		#endregion
	}


	/// <summary>
	/// Banner of Freedom
	/// </summary>
	public class BannerOfFreedomEffect : GuildBannerEffect
	{
		//- Crowd Control Duration Banner - Banner of Freedom: -6% reduction to the time effect of all Crowd Control.

		#region visual overrides
		public override string Name
		{
			get
			{
				return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.BannerOfFreedomEffect.Name");
			}
		}

		protected override string Description
		{
			get { return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.BannerOfFreedomEffect.Description"); }
		}

		//5951,CC Duration Banner,2309,0,0,0,0,0,0,0,0,0,0,0,13,0,332,,,
		public override ushort Icon
		{
			get
			{
				return 2309;
			}
		}
		#endregion

		#region effect
		public override void Start(GameLiving target)
		{
			base.Start(target);
			int effValue = (int)(Effectiveness * 6);
			m_owner.BaseBuffBonusCategory[(int)eProperty.MesmerizeDuration] += effValue;
			m_owner.BaseBuffBonusCategory[(int)eProperty.SpeedDecreaseDuration] += effValue;
			m_owner.BaseBuffBonusCategory[(int)eProperty.StunDuration] += effValue;			
		}

		public override void Stop()
		{
			int effValue = (int)(Effectiveness * 6);
			m_owner.BaseBuffBonusCategory[(int)eProperty.MesmerizeDuration] -= effValue;
			m_owner.BaseBuffBonusCategory[(int)eProperty.SpeedDecreaseDuration] -= effValue;
			m_owner.BaseBuffBonusCategory[(int)eProperty.StunDuration] -= effValue;			
			base.Stop();
		}
		#endregion

		#region ctor
		public BannerOfFreedomEffect(double effectiveness) : base(effectiveness) { }
		#endregion
	}

	/// <summary>
	/// Banner of Freedom
	/// </summary>
	public class BannerOfBesiegingEffect : GuildBannerEffect
	{
		// - Haste - Banner of Besieging: 20% reduction in siege firing speed. (Note that this effect does NOT stack with Warlord.)
		#region visual overrides
		public override string Name
		{
			get
			{
				return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.BannerOfBesiegingEffect.Name");
			}
		}

		protected override string Description
		{
			get { return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.BannerOfBesiegingEffect.Description"); }
		}

		//5952,Siege Banner,1419,0,0,0,0,0,0,0,0,0,0,0,13,0,332,,,
		public override ushort Icon
		{
			get
			{
				return 1419;
			}
		}
		#endregion

		#region effect
		// done in GameSiegeWeapon.GetActionDelay
		#endregion

		#region ctor
		public BannerOfBesiegingEffect(double effectiveness) : base(effectiveness) { }
		#endregion
	}

}