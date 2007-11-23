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
		private int m_xpToNextLevel;
		private int m_newAbilityAtLevel;
		private String m_earnsXP;

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
			m_artifactID = ArtifactMgr.GetArtifactIDFromItemID(template.Id_nb);
			m_artifactLevel = ArtifactMgr.GetItemLevel(this);
			m_xpToNextLevel = ArtifactMgr.GetExperienceGainedTowardsNextLevel(this);
			m_newAbilityAtLevel = ArtifactMgr.GetNewAbilityAtLevel(this);
			m_earnsXP = ArtifactMgr.GetEarnsXP(this);
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
				m_artifactID = ArtifactMgr.GetArtifactIDFromItemID(item.Id_nb);
				m_artifactLevel = ArtifactMgr.GetItemLevel(this);
				m_xpToNextLevel = ArtifactMgr.GetExperienceGainedTowardsNextLevel(this);
				m_newAbilityAtLevel = ArtifactMgr.GetNewAbilityAtLevel(this);
				m_earnsXP = ArtifactMgr.GetEarnsXP(this);
			}
		}

		/// <summary>
		/// The artifact ID.
		/// </summary>
		public String ArtifactID
		{
			get { return m_artifactID; }
		}

		/// <summary>
		/// The current level of the artifact.
		/// </summary>
		public int ArtifactLevel
		{
			get { return m_artifactLevel; }
		}

		/// <summary>
		/// The total experience this artifact has gained so far.
		/// </summary>
		public override long Experience
		{
			get { return base.Experience; }
			set
			{
				base.Experience = value;
				m_artifactLevel = ArtifactMgr.GetItemLevel(this);
				m_xpToNextLevel = ArtifactMgr.GetExperienceGainedTowardsNextLevel(this);
				m_newAbilityAtLevel = ArtifactMgr.GetNewAbilityAtLevel(this);
			}
		}

		/// <summary>
		/// Artifact delve information.
		/// </summary>
		public override List<String> Delve
		{
			get
			{
				List<String> delve = new List<String>();

				delve.Add(string.Format("Artifact (Current level: {0})", ArtifactLevel));

				if (ArtifactLevel < 10)
				{
					delve.Add(string.Format("- {0}% exp earned towards level {1}", m_xpToNextLevel,
						ArtifactLevel + 1));
					delve.Add(string.Format("- Artifact will gain new abilities at level {0}",
						m_newAbilityAtLevel));
					delve.Add(string.Format("(Earns exp: {0})", m_earnsXP));
				}

				delve.Add("");

				foreach (string line in base.Delve)
					delve.Add(line);

				return delve;
			}
		}
	}
}
