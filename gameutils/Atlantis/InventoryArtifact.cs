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
using DOL.Language;

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
					Template.SetBonusType(bonusID, 0);
					Template.SetBonusAmount(bonusID, 0);
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
				ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(Id_nb);

				if (template == null)
				{
					log.ErrorFormat("Artifact: Error loading artifact for owner {0} holding item {1} with an id_nb of {2}", item.OwnerID, item.Name, item.Id_nb);
					return;
				}

				this.Template = template.Clone() as ItemTemplate; // Must make a clone since we modify based on XP
				this.ObjectId = item.ObjectId;	// This is the key for the 'inventoryitem' table
				this.OwnerID = item.OwnerID;
				CanUseEvery = ArtifactMgr.GetReuseTimer(this);
				ArtifactID = ArtifactMgr.GetArtifactIDFromItemID(Id_nb);
				ArtifactLevel = ArtifactMgr.GetCurrentLevel(this);
				m_levelRequirements = ArtifactMgr.GetLevelRequirements(ArtifactID);
				UpdateAbilities(template);
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
			set { m_artifactLevel = value; }
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
					player.Out.SendMessage(String.Format("Your {0} has gained a new ability!", Name), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Repair cost for this artifact in the current state.
		/// </summary>
		public override long RepairCost
		{
			get
			{
				return (MaxCondition - Condition) / 100;
			}
		}

		/// <summary>
		/// Verify that this artifact has all the correct abilities
		/// </summary>
		public void UpdateAbilities(ItemTemplate template)
		{
			if (template == null)
				return;

			for (ArtifactBonus.ID bonusID = ArtifactBonus.ID.Min; bonusID <= ArtifactBonus.ID.Max; ++bonusID)
			{
				if (m_levelRequirements[(int)bonusID] <= ArtifactLevel)
				{
					Template.SetBonusType(bonusID, template.GetBonusType(bonusID));
					Template.SetBonusAmount(bonusID, template.GetBonusAmount(bonusID));
				}
				else
				{
					Template.SetBonusType(bonusID, 0);
					Template.SetBonusAmount(bonusID, 0);
				}
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
			ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(Id_nb);

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
					Template.SetBonusType(bonusID, template.GetBonusType(bonusID));
					Template.SetBonusAmount(bonusID, template.GetBonusAmount(bonusID));

					if (bonusID <= ArtifactBonus.ID.MaxStat)
						player.Notify(PlayerInventoryEvent.ItemBonusChanged, this, new ItemBonusChangedEventArgs(Template.GetBonusType(bonusID), Template.GetBonusAmount(bonusID)));

					abilityGained = true;
				}
			}

			return abilityGained;
		}

		#region Delve

		/// <summary>
		/// Artifact delve information.
		/// </summary>
		public virtual void Delve(List<String> delve, GamePlayer player)
		{
			if (player == null)
				return;

			// Artifact specific information.

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

			// Item bonuses.

			delve.Add("");
			delve.Add("Magical Bonuses:");

			for (ArtifactBonus.ID bonusID = ArtifactBonus.ID.MinStat; bonusID <= ArtifactBonus.ID.MaxStat; ++bonusID)
				DelveMagicalBonus(delve, Template.GetBonusAmount(bonusID), Template.GetBonusType(bonusID), m_levelRequirements[(int)bonusID]);

			for (ArtifactBonus.ID bonusID = ArtifactBonus.ID.MinStat; bonusID <= ArtifactBonus.ID.MaxStat; ++bonusID)
				DelveFocusBonus(delve, Template.GetBonusAmount(bonusID), Template.GetBonusType(bonusID), m_levelRequirements[(int)bonusID]);

			delve.Add("");

			for (ArtifactBonus.ID bonusID = ArtifactBonus.ID.MinStat; bonusID <= ArtifactBonus.ID.MaxStat; ++bonusID)
				DelveBonus(delve, Template.GetBonusAmount(bonusID), Template.GetBonusType(bonusID), m_levelRequirements[(int)bonusID]);

			// Spells & Procs

			DelveMagicalAbility(delve, ArtifactBonus.ID.Spell, m_levelRequirements[(int)ArtifactBonus.ID.Spell]);
			DelveMagicalAbility(delve, ArtifactBonus.ID.ProcSpell, m_levelRequirements[(int)ArtifactBonus.ID.ProcSpell]);
			DelveMagicalAbility(delve, ArtifactBonus.ID.Spell1, m_levelRequirements[(int)ArtifactBonus.ID.Spell1]);
			DelveMagicalAbility(delve, ArtifactBonus.ID.ProcSpell1, m_levelRequirements[(int)ArtifactBonus.ID.ProcSpell1]);

			delve.Add("");

			// Weapon & Armor Stats.

			if ((Object_Type >= (int)eObjectType.GenericWeapon) &&
			    (Object_Type <= (int)eObjectType.MaulerStaff) ||
			    (Object_Type == (int)eObjectType.Instrument))
				DelveWeaponStats(delve, player);
			else if (Object_Type >= (int)eObjectType.Cloth &&
			         Object_Type <= (int)eObjectType.Scale)
				DelveArmorStats(delve, player);

			// Reuse Timer

			int reuseTimer = ArtifactMgr.GetReuseTimer(this);
			
			if (reuseTimer > 0)
			{
				TimeSpan reuseTimeSpan = new TimeSpan(0, 0, reuseTimer);
				delve.Add("");
				delve.Add(String.Format("Can use item every {0} min",
				                        reuseTimeSpan.ToString().Substring(3)));
			}

			if (player.Client.Account.PrivLevel > 1)
				WriteTechnicalInfo(delve);
			
		}

		/// <summary>
		/// Artifact classic magical bonus delve information.
		/// </summary>
		/// <param name="delve"></param>
		/// <param name="bonusAmount"></param>
		/// <param name="bonusType"></param>
		/// <param name="levelRequirement"></param>
		protected virtual void DelveMagicalBonus(List<String> delve, int bonusAmount, int bonusType, int levelRequirement)
		{
			String levelTag = (levelRequirement > 0)
				? String.Format("[L{0}]: ", levelRequirement)
				: "";

			if (IsStatBonus(bonusType) || IsSkillBonus(bonusType))
			{
				delve.Add(String.Format("- {0}{1}: {2} pts",
				                        levelTag,
				                        SkillBase.GetPropertyName((eProperty)bonusType),
				                        bonusAmount.ToString("+0;-0;0")));
			}
			else if (IsResistBonus(bonusType))
			{
				delve.Add(String.Format("- {0}{1}: {2}%",
				                        levelTag,
				                        SkillBase.GetPropertyName((eProperty)bonusType),
				                        bonusAmount.ToString("+0;-0;0")));
			}
			else if (bonusType == (int)eProperty.PowerPool)
			{
				delve.Add(String.Format("- {0}{1}: {2}% of power pool.",
				                        levelTag,
				                        SkillBase.GetPropertyName((eProperty)bonusType),
				                        bonusAmount.ToString("+0;-0;0")));
			}
		}


		protected virtual void DelveFocusBonus(List<String> delve, int bonusAmount, int bonusType, int levelRequirement)
		{
			String levelTag = (levelRequirement > 0)
				? String.Format("[L{0}]: ", levelRequirement)
				: "";

			bool addedFocusDescription = false;

			if (IsFocusBonus(bonusType))
			{
				if (!addedFocusDescription)
				{
					delve.Add("");
					delve.Add("Focus Bonuses:");
					addedFocusDescription = true;
				}

				delve.Add(String.Format("- {0}{1}: {2} lvls",
				                        levelTag,
				                        SkillBase.GetPropertyName((eProperty)bonusType),
				                        bonusAmount));
			}
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
		/// Artifact Magical Ability delve information (spells, procs).
		/// </summary>
		/// <param name="delve"></param>
		/// <param name="bonusID"></param>
		/// <param name="levelRequirement"></param>
		public virtual void DelveMagicalAbility(List<String> delve, ArtifactBonus.ID bonusID, int levelRequirement)
		{
			String levelTag = (levelRequirement > 0)
				? String.Format("[L{0}]: ", levelRequirement)
				: "";

			bool isProc = false;
			bool isSecondary = false;
			int spellID = 0;

			switch (bonusID)
			{
				case ArtifactBonus.ID.Spell:
					spellID = SpellID;
					isProc = false;
					isSecondary = false;
					break;
				case ArtifactBonus.ID.Spell1:
					spellID = SpellID1;
					isProc = false;
					isSecondary = true;
					break;
				case ArtifactBonus.ID.ProcSpell:
					spellID = ProcSpellID;
					isProc = true;
					isSecondary = false;
					break;
				case ArtifactBonus.ID.ProcSpell1:
					spellID = ProcSpellID1;
					isProc = true;
					isSecondary = true;
					break;
			}

			if (spellID == 0)
				return;

			delve.Add("");
			delve.Add(String.Format("{0}{1}Magical Ability:", levelTag,
			                        (isSecondary) ? "Secondary " : ""));

			SpellLine spellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
			if (spellLine != null)
			{
				List<Spell> spells = SkillBase.GetSpellList(spellLine.KeyName);
				foreach (Spell spell in spells)
					if (spell.ID == spellID)
						spell.Delve(delve);
			}

			if (isProc)
				delve.Add(String.Format("- Spell has a chance of casting when this {0} enemy.",
				                        (GlobalConstants.IsWeapon(Object_Type))
				                        ? "weapon strikes an" : "armor is hit by"));
			else
				delve.Add("- This spell is cast when the item is used.");
		}

		/// <summary>
		/// Artifact weapon stats delve info.
		/// </summary>
		/// <param name="delve"></param>
		/// <param name="player"></param>
		public virtual void DelveWeaponStats(List<String> delve, GamePlayer player)
		{
			double itemDPS = DPS_AF / 10.0;
			double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * player.Level);
			double itemSPD = SPD_ABS / 10.0;
			double effectiveDPS = itemDPS * Quality / 100.0 * Condition / MaxCondition;

			delve.Add("Damage Modifiers:");

			if (itemDPS != 0)
			{
				delve.Add(LanguageMgr.GetTranslation(player.Client,
				                                     "DetailDisplayHandler.WriteClassicWeaponInfos.BaseDPS", itemDPS.ToString("0.0")));
				delve.Add(LanguageMgr.GetTranslation(player.Client,
				                                     "DetailDisplayHandler.WriteClassicWeaponInfos.ClampDPS", clampedDPS.ToString("0.0")));
			}

			if (SPD_ABS >= 0)
				delve.Add(LanguageMgr.GetTranslation(player.Client,
				                                     "DetailDisplayHandler.WriteClassicWeaponInfos.SPD", itemSPD.ToString("0.0")));

			if (Quality != 0)
				delve.Add(LanguageMgr.GetTranslation(player.Client,
				                                     "DetailDisplayHandler.WriteClassicWeaponInfos.Quality", Quality));

			if (Condition != 0)
				delve.Add(LanguageMgr.GetTranslation(player.Client,
				                                     "DetailDisplayHandler.WriteClassicWeaponInfos.Condition", ConditionPercent));

			delve.Add(LanguageMgr.GetTranslation(player.Client,
			                                     "DetailDisplayHandler.WriteClassicWeaponInfos.DamageType",
			                                     (Type_Damage == 0 ? "None" : GlobalConstants.WeaponDamageTypeToName(Type_Damage))));
			delve.Add(" ");

			delve.Add(LanguageMgr.GetTranslation(player.Client,
			                                     "DetailDisplayHandler.WriteClassicWeaponInfos.EffDamage"));
			
			if (itemDPS != 0)
				delve.Add("- " + effectiveDPS.ToString("0.0") + " DPS");
		}

		/// <summary>
		/// Artifact armor stats delve info.
		/// </summary>
		/// <param name="delve"></param>
		/// <param name="player"></param>
		public virtual void DelveArmorStats(List<String> delve, GamePlayer player)
		{
			delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicArmorInfos.ArmorMod"));

			double af = 0;
			int afCap = player.Level;
			double effectiveAF = 0;

			if (DPS_AF != 0)
			{
				delve.Add(LanguageMgr.GetTranslation(player.Client,
				                                     "DetailDisplayHandler.WriteClassicArmorInfos.BaseFactor", DPS_AF));

				if (Object_Type != (int)eObjectType.Cloth)
					afCap *= 2;

				af = Math.Min(afCap, DPS_AF);

				delve.Add(LanguageMgr.GetTranslation(player.Client,
				                                     "DetailDisplayHandler.WriteClassicArmorInfos.ClampFact", (int)af));
			}

			if (SPD_ABS >= 0)
				delve.Add(LanguageMgr.GetTranslation(player.Client,
				                                     "DetailDisplayHandler.WriteClassicArmorInfos.Absorption", SPD_ABS));

			if (Quality != 0)
				delve.Add(LanguageMgr.GetTranslation(player.Client,
				                                     "DetailDisplayHandler.WriteClassicArmorInfos.Quality", Quality));

			if (Condition != 0)
				delve.Add(LanguageMgr.GetTranslation(player.Client,
				                                     "DetailDisplayHandler.WriteClassicArmorInfos.Condition", ConditionPercent));

			delve.Add(" ");
			delve.Add(LanguageMgr.GetTranslation(player.Client,
			                                     "DetailDisplayHandler.WriteClassicArmorInfos.EffArmor"));

			if (DPS_AF != 0)
			{
				effectiveAF = af * Quality / 100.0 * Condition / MaxCondition * (1 + SPD_ABS / 100.0);
				delve.Add(LanguageMgr.GetTranslation(player.Client,
				                                     "DetailDisplayHandler.WriteClassicArmorInfos.Factor", (int)effectiveAF));
			}
		}

		/// <summary>
		/// Write item technical info
		/// </summary>
		/// <param name="output"></param>
		/// <param name="item"></param>
		public void WriteTechnicalInfo(List<String> delve)
		{
			delve.Add(" ");
			delve.Add("--- Artifact/Item technical information ---");
			delve.Add(" ");
			delve.Add("Item Template: " + Id_nb);
			delve.Add("         Name: " + Name);
			delve.Add("   Experience: " + Experience);
			delve.Add("       Object: " + GlobalConstants.ObjectTypeToName(Object_Type) + " (" + Object_Type + ")");
			delve.Add("         Type: " + GlobalConstants.SlotToName(Item_Type) + " (" + Item_Type + ")");
			delve.Add("    Extension: " + Extension);
			delve.Add("        Model: " + Model);
			delve.Add("        Color: " + Color);
			delve.Add("       Emblem: " + Emblem);
			delve.Add("       Effect: " + Effect);
			delve.Add("  Value/Price: " + Money.GetShortString(Price));
			delve.Add("       Weight: " + (Weight / 10.0f) + "lbs");
			delve.Add("      Quality: " + Quality + "%");
			delve.Add("   Durability: " + Durability + "/" + MaxDurability + "(max)");
			delve.Add("    Condition: " + Condition + "/" + MaxCondition + "(max)");
			delve.Add("        Realm: " + Realm);
			delve.Add("  Is dropable: " + (IsDropable ? "yes" : "no"));
			delve.Add("  Is pickable: " + (IsPickable ? "yes" : "no"));
			delve.Add(" Is stackable: " + (IsStackable ? "yes" : "no"));
			delve.Add(" Is tradeable: " + (IsTradable ? "yes" : "no"));
			delve.Add("  ProcSpellID: " + ProcSpellID);
			delve.Add(" ProcSpellID1: " + ProcSpellID1);
			delve.Add("      SpellID: " + SpellID + " (" + Charges + "/" + MaxCharges + ")");
			delve.Add("     SpellID1: " + SpellID1 + " (" + Charges1 + "/" + MaxCharges1 + ")");
			delve.Add("PoisonSpellID: " + PoisonSpellID + " (" + PoisonCharges + "/" + PoisonMaxCharges + ") ");

			if (GlobalConstants.IsWeapon(Object_Type))
			{
				delve.Add("         Hand: " + GlobalConstants.ItemHandToName(Hand) + " (" + Hand + ")");
				delve.Add("Damage/Second: " + (DPS_AF / 10.0f));
				delve.Add("        Speed: " + (SPD_ABS / 10.0f));
				delve.Add("  Damage type: " + GlobalConstants.WeaponDamageTypeToName(Type_Damage) + " (" + Type_Damage + ")");
				delve.Add("        Bonus: " + Bonus);
			}
			else if (GlobalConstants.IsArmor(Object_Type))
			{
				delve.Add("  Armorfactor: " + DPS_AF);
				delve.Add("   Absorption: " + SPD_ABS);
				delve.Add("        Bonus: " + Bonus);
			}
			else if (Object_Type == (int)eObjectType.Shield)
			{
				delve.Add("Damage/Second: " + (DPS_AF / 10.0f));
				delve.Add("        Speed: " + (SPD_ABS / 10.0f));
				delve.Add("  Shield type: " + GlobalConstants.ShieldTypeToName(Type_Damage) + " (" + Type_Damage + ")");
				delve.Add("        Bonus: " + Bonus);
			}
			else if (Object_Type == (int)eObjectType.Arrow || Object_Type == (int)eObjectType.Bolt)
			{
				delve.Add(" Ammunition #: " + DPS_AF);
				delve.Add("       Damage: " + GlobalConstants.AmmunitionTypeToDamageName(SPD_ABS));
				delve.Add("        Range: " + GlobalConstants.AmmunitionTypeToRangeName(SPD_ABS));
				delve.Add("     Accuracy: " + GlobalConstants.AmmunitionTypeToAccuracyName(SPD_ABS));
				delve.Add("        Bonus: " + Bonus);
			}
			else if (Object_Type == (int)eObjectType.Instrument)
			{
				delve.Add("   Instrument: " + GlobalConstants.InstrumentTypeToName(DPS_AF));
			}
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
			    (bonusType != (int)eProperty.PowerPool) && (bonusType != (int)eProperty.Fatigue))
				return true;
			if ((bonusType >= (int)eProperty.MaxSpeed) && (bonusType <= (int)eProperty.MeleeSpeed))
				return true;
			if ((bonusType >= (int)eProperty.CriticalMeleeHitChance) &&
			    (bonusType <= (int)eProperty.CriticalHealHitChance))
				return true;
			if ((bonusType >= (int)eProperty.EvadeChance) && (bonusType <= (int)eProperty.SpeedDecreaseDuration))
				return true;
			if ((bonusType >= (int)eProperty.BountyPoints) && (bonusType <= (int)eProperty.ArcaneSyphon))
				return true;
			return false;
		}

		#endregion
	}
}
