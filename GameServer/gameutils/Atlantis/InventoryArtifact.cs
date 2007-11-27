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
using System.Text;
using DOL.GS;
using log4net;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// An artifact inside an inventory.
	/// </summary>
	/// <author>Aredhel</author>
	public class InventoryArtifact : InventoryItem
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private String m_artifactID;
		private int m_artifactLevel;
		private int[] m_levelRequirements;

		/// <summary>
		/// This constructor shouldn't be called, so we prevent anyone
		/// from using it.
		/// </summary>
		private InventoryArtifact() { }

		/// <summary>
		/// Create a new inventory artifact from an item template.
		/// </summary>
		/// <param name="template"></param>
		public InventoryArtifact(ItemTemplate template)
			: base(template)
		{
			ArtifactID = ArtifactMgr.GetArtifactIDFromItemID(template.Id_nb);
			ArtifactLevel = 0;
			m_levelRequirements = ArtifactMgr.GetLevelRequirements(ArtifactID);

			for (ArtifactBonus.ID bonusID = ArtifactBonus.ID.Min; bonusID <= ArtifactBonus.ID.Max; ++bonusID)
			{
				// Clear all bonuses except the base (L0) bonuses.

				if (m_levelRequirements[(int)bonusID] > 0)
				{
					SetBonusType(bonusID, 0);
					SetBonusAmount(bonusID, 0);
				}
			}
		}

		/// <summary>
		/// Create a new inventory artifact from an existing inventory item.
		/// </summary>
		/// <param name="item"></param>
		public InventoryArtifact(InventoryItem item)
			: base(item)
		{
			if (item != null)
			{
				this.ObjectId = item.ObjectId;	// This is the key for the 'inventoryitem' table
				ArtifactID = ArtifactMgr.GetArtifactIDFromItemID(Id_nb);
				ArtifactLevel = ArtifactMgr.GetCurrentLevel(this);
				m_levelRequirements = ArtifactMgr.GetLevelRequirements(ArtifactID);
			}
		}

		/// <summary>
		/// The ID of this artifact.
		/// </summary>
		public String ArtifactID
		{
			get { return m_artifactID; }
			protected set { m_artifactID = value; }
		}

		/// <summary>
		/// The level of this artifact.
		/// </summary>
		public int ArtifactLevel
		{
			get { return m_artifactLevel; }
			protected set { m_artifactLevel = value; }
		}

		/// <summary>
		/// Called from ArtifactMgr when this artifact has gained a level.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="artifactLevel"></param>
		public void OnLevelGained(GamePlayer player, int artifactLevel)
		{
			if (artifactLevel > ArtifactLevel && artifactLevel <= 10)
			{
				ArtifactLevel = artifactLevel;
				if (AddAbilities(player, ArtifactLevel) && player != null)
					player.Out.SendMessage(String.Format("Your {0} has gained a new ability!", Name),
						eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Add all abilities for this level.
		/// </summary>
		/// <param name="artifactLevel">The level to add abilities for.</param>
		/// <param name="player"></param>
		/// <returns>True, if artifact gained 1 or more abilities, else false.</returns>
		private bool AddAbilities(GamePlayer player, int artifactLevel)
		{
			ItemTemplate template = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), 
				Id_nb);

			if (template == null)
			{
				log.Warn(String.Format("Item template missing for artifact '{0}'", Name));
				return false;
			}

			bool abilityGained = false;

			for (ArtifactBonus.ID bonusID = ArtifactBonus.ID.Min; bonusID <= ArtifactBonus.ID.Max; ++bonusID)
			{
				if (m_levelRequirements[(int)bonusID] == artifactLevel)
				{
					SetBonusType(bonusID, template.GetBonusType(bonusID));
					SetBonusAmount(bonusID, template.GetBonusAmount(bonusID));

					if (bonusID <= ArtifactBonus.ID.MaxStat)
						player.Notify(PlayerInventoryEvent.ItemBonusChanged, this,
							new ItemBonusChangedEventArgs(GetBonusType(bonusID), GetBonusAmount(bonusID)));

					abilityGained = true;
				}
			}

			return abilityGained;
		}

		#region Delve

		/// <summary>
		/// Artifact delve information.
		/// </summary>
		public override void Delve(List<String> delve)
		{
			if (ArtifactLevel < 10)
			{
				delve.Add(string.Format("Artifact (Current level: {0})", ArtifactLevel));
				delve.Add(string.Format("- {0}% exp earned towards level {1}",
					ArtifactMgr.GetXPGainedForLevel(this), ArtifactLevel + 1));
				delve.Add(string.Format("- Artifact will gain new abilities at level {0}",
					GainsNewAbilityAtLevel()));
				delve.Add(string.Format("(Earns exp: {0})",
					ArtifactMgr.GetEarnsXP(this)));
			}
			else
			{
				delve.Add("Artifact:");
				delve.Add("Current level: 10");
			}

			delve.Add("");
			delve.Add("Magical Bonuses:");

			for (ArtifactBonus.ID bonusID = ArtifactBonus.ID.MinStat; bonusID <= ArtifactBonus.ID.MaxStat; ++bonusID)
				DelveMagicalBonus(delve, GetBonusAmount(bonusID), GetBonusType(bonusID),
					m_levelRequirements[(int)bonusID]);

			delve.Add("");

			for (ArtifactBonus.ID bonusID = ArtifactBonus.ID.MinStat; bonusID <= ArtifactBonus.ID.MaxStat; ++bonusID)
				DelveBonus(delve, GetBonusAmount(bonusID), GetBonusType(bonusID),
					m_levelRequirements[(int)bonusID]);

			delve.Add("");
		}

		/// <summary>
		/// Artifact classic magical bonus delve information.
		/// </summary>
		/// <param name="delve"></param>
		/// <param name="bonusAmount"></param>
		/// <param name="bonusType"></param>
		/// <param name="levelRequirement"></param>
		protected virtual void DelveMagicalBonus(List<String> delve, int bonusAmount, int bonusType,
			int levelRequirement)
		{
			String levelTag = (levelRequirement > 0) 
				? String.Format("[L{0}]: ", levelRequirement) 
				: "";

			if (IsStatBonus(bonusType) || IsSkillBonus(bonusType))
				delve.Add(String.Format("- {0}{1}: {2} pts",
					levelTag,
					SkillBase.GetPropertyName((eProperty)bonusType),
					bonusAmount.ToString("+0;-0;0")));
			else if (IsResistBonus(bonusType))
				delve.Add(String.Format("- {0}{1}: {2}%",
					levelTag,
					SkillBase.GetPropertyName((eProperty)bonusType),
					bonusAmount.ToString("+0;-0;0")));
			else if (bonusType == (int)eProperty.PowerPool)
				delve.Add(String.Format("- {0}{1}: {2}% of power pool.",
					levelTag,
					SkillBase.GetPropertyName((eProperty)bonusType),
					bonusAmount.ToString("+0;-0;0")));
		}

		/// <summary>
		/// Artifact ToA magical bonus delve information.
		/// </summary>
		/// <param name="delve"></param>
		/// <param name="bonusAmount"></param>
		/// <param name="bonusType"></param>
		/// <param name="levelRequirement"></param>
		protected virtual void DelveBonus(List<String> delve, int bonusAmount, int bonusType,
			int levelRequirement)
		{
			if (!IsToABonus(bonusType))
				return;

			String levelTag = (levelRequirement > 0)
				? String.Format("[L{0}]: ", levelRequirement)
				: "";

			if (IsCapIncreaseBonus(bonusType) || bonusType == (int)eProperty.ArmorFactor)
				delve.Add(String.Format("{0}{1}: {2}",
					levelTag,
					SkillBase.GetPropertyName((eProperty)bonusType),
					bonusAmount));
			else
				delve.Add(String.Format("{0}{1}: {2}%",
					levelTag,
					SkillBase.GetPropertyName((eProperty)bonusType),
					bonusAmount));
		}

		/// <summary>
		/// Returns the level when this artifact will gain a new 
		/// ability.
		/// </summary>
		/// <returns></returns>
		private int GainsNewAbilityAtLevel()
		{
			for (ArtifactBonus.ID bonusID = ArtifactBonus.ID.Min; bonusID <= ArtifactBonus.ID.Max; ++bonusID)
				if (m_levelRequirements[(int)bonusID] > ArtifactLevel)
					return m_levelRequirements[(int)bonusID];

			return 10;
		}

		/// <summary>
		/// Check whether this type is a cap increase bonus.
		/// </summary>
		/// <param name="bonusType"></param>
		/// <returns></returns>
		protected virtual bool IsCapIncreaseBonus(int bonusType)
		{
			if ((bonusType >= (int)eProperty.StatCapBonus_First) && (bonusType <= (int)eProperty.StatCapBonus_Last))
				return true;
			return false;
		}

		/// <summary>
		/// Check whether this type is focus.
		/// </summary>
		/// <param name="bonusType"></param>
		/// <returns></returns>
		protected virtual bool IsFocusBonus(int bonusType)
		{
			if ((bonusType >= (int)eProperty.Focus_Darkness) && (bonusType <= (int)eProperty.Focus_Arboreal))
				return true;
			if ((bonusType >= (int)eProperty.Focus_EtherealShriek) && (bonusType <= (int)eProperty.Focus_Witchcraft))
				return true;
			if (bonusType == (int)eProperty.AllFocusLevels)
				return true;
			return false;
		}

		/// <summary>
		/// Check whether this type is resist.
		/// </summary>
		/// <param name="bonusType"></param>
		/// <returns></returns>
		protected virtual bool IsResistBonus(int bonusType)
		{
			if ((bonusType >= (int)eProperty.Resist_First) && (bonusType <= (int)eProperty.Resist_Last))
				return true;
			return false;
		}

		/// <summary>
		/// Check whether this type is a skill.
		/// </summary>
		/// <param name="bonusType"></param>
		/// <returns></returns>
		protected virtual bool IsSkillBonus(int bonusType)
		{
			if ((bonusType >= (int)eProperty.Skill_First) && (bonusType <= (int)eProperty.Skill_Last))
				return true;
			return false;
		}

		/// <summary>
		/// Check whether this type is a stat bonus.
		/// </summary>
		/// <param name="bonusType"></param>
		/// <returns></returns>
		protected virtual bool IsStatBonus(int bonusType)
		{
			if ((bonusType >= (int)eProperty.Stat_First) && (bonusType <= (int)eProperty.Stat_Last))
				return true;
			if ((bonusType == (int)eProperty.MaxHealth) || (bonusType == (int)eProperty.MaxMana))
				return true;
			if ((bonusType == (int)eProperty.Fatigue) || (bonusType == (int)eProperty.PowerPool))
				return true;
			if (bonusType == (int)eProperty.Acuity)
				return true;
			if ((bonusType == (int)eProperty.AllMeleeWeaponSkills) || (bonusType == (int)eProperty.AllMagicSkills))
				return true;
			if ((bonusType == (int)eProperty.AllDualWieldingSkills) || (bonusType == (int)eProperty.AllArcherySkills))
				return true;
			return false;
		}

		/// <summary>
		/// Check whether this type is a ToA bonus (it's all mixed
		/// up in GlobalConstants.cs, but let's try anyway).
		/// </summary>
		/// <param name="bonusType"></param>
		/// <returns></returns>
		protected virtual bool IsToABonus(int bonusType)
		{
			if (IsCapIncreaseBonus(bonusType))
				return true;
			if ((bonusType >= (int)eProperty.ToABonus_First) && (bonusType <= (int)eProperty.ToABonus_Last) &&
				(bonusType != (int)eProperty.PowerPool))
				return true;
			if ((bonusType >= (int)eProperty.MaxSpeed) && (bonusType <= (int)eProperty.MeleeSpeed))
				return true;
			if ((bonusType >= (int)eProperty.CriticalMeleeHitChance) &&
				(bonusType <= (int)eProperty.CriticalHealHitChance))
				return true;
			if ((bonusType >= (int)eProperty.EvadeChance) && (bonusType <= (int)eProperty.SpeedDecreaseDuration))
				return true;
			return false;
		}

		#endregion
	}
}
