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
				log.Info(String.Format("{0} (level {1})", ArtifactID, ArtifactLevel));
				for (ArtifactBonus.ID bonusID = ArtifactBonus.ID.Min; bonusID <= ArtifactBonus.ID.Max; ++bonusID)
					log.Info(String.Format("Bonus {0} Requirement Level {1}",
						(int)bonusID, m_levelRequirements[(int)bonusID]));
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
		/// <param name="artifactLevel"></param>
		public void OnLevelGained(int artifactLevel)
		{
			if (artifactLevel > ArtifactLevel && artifactLevel <= 10)
			{
				ArtifactLevel = artifactLevel;
				// TODO: Refresh bonuses so they affect the player.
			}
		}

		#region Bonuses

		/// <summary>
		/// Stat bonus 1.
		/// </summary>
		public override int Bonus1
		{
			get 
			{ 
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus1])
					? base.Bonus1
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 2.
		/// </summary>
		public override int Bonus2
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus2])
					? base.Bonus2
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 3.
		/// </summary>
		public override int Bonus3
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus3])
					? base.Bonus3
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 4.
		/// </summary>
		public override int Bonus4
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus4])
					? base.Bonus4
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 5.
		/// </summary>
		public override int Bonus5
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus5])
					? base.Bonus5
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 6.
		/// </summary>
		public override int Bonus6
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus6])
					? base.Bonus6
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 7.
		/// </summary>
		public override int Bonus7
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus7])
					? base.Bonus7
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 8.
		/// </summary>
		public override int Bonus8
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus8])
					? base.Bonus8
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 9.
		/// </summary>
		public override int Bonus9
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus9])
					? base.Bonus9
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 10.
		/// </summary>
		public override int Bonus10
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus10])
					? base.Bonus10
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 1 type.
		/// </summary>
		public override int Bonus1Type
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus1])
					? base.Bonus1Type
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 2 type.
		/// </summary>
		public override int Bonus2Type
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus2])
					? base.Bonus2Type
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 3 type.
		/// </summary>
		public override int Bonus3Type
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus3])
					? base.Bonus3Type
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 4 type.
		/// </summary>
		public override int Bonus4Type
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus4])
					? base.Bonus4Type
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 5 type.
		/// </summary>
		public override int Bonus5Type
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus5])
					? base.Bonus5Type
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 6 type.
		/// </summary>
		public override int Bonus6Type
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus6])
					? base.Bonus6Type
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 7 type.
		/// </summary>
		public override int Bonus7Type
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus7])
					? base.Bonus7Type
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 8 type.
		/// </summary>
		public override int Bonus8Type
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus8])
					? base.Bonus8Type
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 9 type.
		/// </summary>
		public override int Bonus9Type
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus9])
					? base.Bonus9Type
					: 0;
			}
		}

		/// <summary>
		/// Stat bonus 10 type.
		/// </summary>
		public override int Bonus10Type
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Bonus10])
					? base.Bonus10Type
					: 0;
			}
		}

		/// <summary>
		/// Spell ID.
		/// </summary>
		public override int SpellID
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Spell])
					? base.SpellID
					: 0;
			}
		}

		/// <summary>
		/// Spell ID 1.
		/// </summary>
		public override int SpellID1
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.Spell1])
					? base.SpellID1
					: 0;
			}
		}

		/// <summary>
		/// Proc spell ID.
		/// </summary>
		public override int ProcSpellID
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.ProcSpell])
					? base.ProcSpellID
					: 0;
			}
		}

		/// <summary>
		/// Proc spell ID 1.
		/// </summary>
		public override int ProcSpellID1
		{
			get
			{
				return (ArtifactLevel >= m_levelRequirements[(int)ArtifactBonus.ID.ProcSpell1])
					? base.ProcSpellID1
					: 0;
			}
		}

		#endregion

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

			// TODO: Delve needs a serious makeover.

			DelveMagicalBonus(delve, Bonus1, Bonus1Type, m_levelRequirements[(int)ArtifactBonus.ID.Bonus1]);
			DelveMagicalBonus(delve, Bonus2, Bonus2Type, m_levelRequirements[(int)ArtifactBonus.ID.Bonus2]);
			DelveMagicalBonus(delve, Bonus3, Bonus3Type, m_levelRequirements[(int)ArtifactBonus.ID.Bonus3]);
			DelveMagicalBonus(delve, Bonus4, Bonus4Type, m_levelRequirements[(int)ArtifactBonus.ID.Bonus4]);
			DelveMagicalBonus(delve, Bonus5, Bonus5Type, m_levelRequirements[(int)ArtifactBonus.ID.Bonus5]);
			DelveMagicalBonus(delve, Bonus6, Bonus6Type, m_levelRequirements[(int)ArtifactBonus.ID.Bonus6]);
			DelveMagicalBonus(delve, Bonus7, Bonus7Type, m_levelRequirements[(int)ArtifactBonus.ID.Bonus7]);
			DelveMagicalBonus(delve, Bonus8, Bonus8Type, m_levelRequirements[(int)ArtifactBonus.ID.Bonus8]);
			DelveMagicalBonus(delve, Bonus9, Bonus9Type, m_levelRequirements[(int)ArtifactBonus.ID.Bonus9]);
			DelveMagicalBonus(delve, Bonus10, Bonus10Type, m_levelRequirements[(int)ArtifactBonus.ID.Bonus10]);

			delve.Add("");
		}

		/// <summary>
		/// Artifact magical bonus delve information (includes level
		/// requirement notes).
		/// </summary>
		protected void DelveMagicalBonus(List<String> delve, int bonusAmount, int bonusType, 
			int levelRequirement)
		{
			if (ArtifactLevel >= levelRequirement)
			{
				DelveMagicalBonus(delve, bonusAmount, bonusType);
				DelveRequirement(delve, levelRequirement);
			}
		}

		/// <summary>
		/// Artifact magical bonus delve information.
		/// NOTE: This should actually be in the base class.
		/// </summary>
		protected override void DelveMagicalBonus(List<String> delve, int bonusAmount, int bonusType)
		{
			log.Info(String.Format("Delve Bonus: {0}, Type: {1}", bonusAmount, bonusType));
			if (bonusType != 0 &&
				bonusAmount != 0 &&
				!SkillBase.CheckPropertyType((eProperty)bonusType, ePropertyType.Focus))
			{
				delve.Add(String.Format("- {0}: {1}{2}",
					SkillBase.GetPropertyName((eProperty)bonusType),
					bonusAmount.ToString("+0;-0;0"),
					((bonusType == (int)eProperty.PowerPool) ||
					(bonusType >= (int)eProperty.Resist_First && bonusType <= (int)eProperty.Resist_Last))
						? ((bonusType == (int)eProperty.PowerPool) ? "% of power pool." : "%")
						: " pts"));
			}
		}

		/// <summary>
		/// Level requirement delve information.
		/// </summary>
		/// <param name="delve"></param>
		/// <param name="levelRequirement"></param>
		private void DelveRequirement(List<String> delve, int levelRequirement)
		{
			if (levelRequirement > 0)
				delve.Add(String.Format("(Item level required: {0})", levelRequirement));
		}

		/// <summary>
		/// Returns the level when this artifact will gain a new 
		/// ability again.
		/// </summary>
		/// <returns></returns>
		private int GainsNewAbilityAtLevel()
		{
			for (ArtifactBonus.ID bonusID = ArtifactBonus.ID.Min; bonusID <= ArtifactBonus.ID.Max; ++bonusID)
				if (m_levelRequirements[(int)bonusID] > ArtifactLevel)
					return m_levelRequirements[(int)bonusID];

			return 10;
		}

		#endregion
	}
}
