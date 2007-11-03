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
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using DOL.GS.Spells;
using DOL.AI.Brain;
using DOL.Events;
using log4net;
using DOL.GS.PacketHandler;
using DOL.Database;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.Styles;

namespace DOL.GS
{
	class NecromancerPet : GameNPC
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Disable default constructor.
		/// </summary>
		private NecromancerPet()
		{
		}

        INpcTemplate m_petTemplate;

		/// <summary>
		/// Create necromancer pet from template. Con and hit bonuses from
		/// items the caster was wearing when the summon started, will be
		/// transferred to the pet.
		/// </summary>
		/// <param name="npcTemplate"></param>
		/// <param name="owner">Player who summoned this pet.</param>
		/// <param name="summonConBonus">Item constitution bonuses of the player.</param>
		/// <param name="summonHitsBonus">Hits bonuses of the player.</param>
		public NecromancerPet(INpcTemplate npcTemplate, GamePlayer owner, int summonConBonus, 
			int summonHitsBonus) : base(npcTemplate)
		{
            m_petTemplate = npcTemplate;

            // Transfer bonuses.
            
			m_summonConBonus = summonConBonus;
			m_summonHitsBonus = summonHitsBonus;

			// Set level dependent stats.

			Strength = (short) (20 + Level * 4);

            // Load equipment, if available.

            String templateID = "";

            switch (Name)
            {
                case "reanimated servant" : templateID = "reanimated_servant";
                    break;
                case "necroservant": templateID = "necroservant";
                    break;
                case "abomination": templateID = "abomination_fiery_sword";
                    break;
            }

			LoadEquipmentTemplate(templateID);

            // Create a brain for the pet.

			SetOwnBrain(new NecromancerPetBrain(owner));
		}

		/// <summary>
		/// Address the master.
		/// </summary>
		public void HailMaster()
		{
			GamePlayer owner = ((Brain as IControlledBrain).Owner) as GamePlayer;
			if (owner != null)
			{
				String message = String.Format("Hail, {0}. As my summoner, you may target me and say Arawn to learn more about the abilities I possess.",
					(owner.PlayerCharacter.Gender == 0) ? "Master" : "Mistress");
				WhisperOwner(message);
			}
		}

		/// <summary>
		/// Multiplier for melee and magic.
		/// </summary>
		public override double Effectiveness
		{
			get { return (Brain as NecromancerPetBrain).Owner.Effectiveness; }
		}

        /// <summary>
        /// Specialisation level including item bonuses and RR.
        /// </summary>
        /// <param name="keyName">The specialisation line.</param>
        /// <returns>The specialisation level.</returns>
        public override int GetModifiedSpecLevel(string keyName)
        {
            return (Brain as NecromancerPetBrain).Owner.GetModifiedSpecLevel(keyName);
        }

		/// <summary>
		/// Get melee speed in milliseconds.
		/// </summary>
		/// <param name="weapons"></param>
		/// <returns></returns>
		public override int AttackSpeed(params InventoryItem[] weapons)
		{
			double weaponSpeed = 0.0;

			if (weapons != null)
			{
				foreach (InventoryItem item in weapons)
					if (item != null)
						weaponSpeed += item.SPD_ABS;
				weaponSpeed = (weapons.Length > 0) ? weaponSpeed / weapons.Length : 34.0;
			}
			else
			{
				weaponSpeed = 34.0;
			}

			double speed = 100 * weaponSpeed * (1.0 - (GetModified(eProperty.Quickness) - 60) / 500.0);
			return (int)(speed * GetModified(eProperty.MeleeSpeed) * 0.01);
		}

        /// <summary>
        /// Weapon specialisation is up to level, if a weapon is equipped.
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public override int WeaponSpecLevel(InventoryItem weapon)
        {
            return (weapon != null) ? Level : base.WeaponSpecLevel(weapon);
        }

        /// <summary>
        /// Get weapon skill for the pet (for formula see Spydor's Web,
        /// http://daoc.nisrv.com/modules.php?name=Weapon_Skill_Calc).
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public override double GetWeaponSkill(InventoryItem weapon)
        {
            if (weapon == null)
                return base.GetWeaponSkill(weapon);

            // Let's say the pet is paladin-like.

            double factor = 1.9;
            double baseWS = 380;
            return ((GetWeaponStat(weapon) - 50) * factor + baseWS) * (1 + WeaponSpecLevel(weapon) / 100);
        }

        /// <summary>
        /// The type of damage the currently active weapon does.
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public override eDamageType AttackDamageType(InventoryItem weapon)
        {
			//if (weapon != null)
			//    return (eDamageType) (weapon.Type_Damage);

            return base.AttackDamageType(weapon);
        }

		private int m_summonConBonus;
		private int m_summonHitsBonus;

		/// <summary>
		/// Maximum health for a necro pet. Level*38 base plus Con and
		/// Hits bonuses on the summoning suit (3.1 hits per 1 Con).
		/// </summary>
		public override int MaxHealth
		{
			get
			{
				return (int) (Level * 38 + m_summonHitsBonus + 3.1 * m_summonConBonus);
			}
		}

		/// <summary>
		/// Called when spell has finished casting.
		/// </summary>
		/// <param name="handler"></param>
		public override void OnAfterSpellCastSequence(ISpellHandler handler)
		{
			base.OnAfterSpellCastSequence(handler);
			Brain.Notify(GameNPCEvent.CastFinished, this, new CastSpellEventArgs(handler));
		}

        /// <summary>
        /// Actions to be taken when the pet receives a whisper.
        /// </summary>
        /// <param name="source">Source of the whisper.</param>
        /// <param name="text">"Text that was whispered</param>
        /// <returns>True if whisper was handled, false otherwise.</returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			GamePlayer owner = ((Brain as IControlledBrain).Owner) as GamePlayer;
			if (source == null || source != owner) return false;
			String reply;

			switch (text)
			{
				case "arawn":
					{
						String taunt = "As one of the many cadaverous servants of Arawn, I am able to [taunt] your enemies so that they will focus on me instead of you.";
						String empower = "You may also [empower] me with just a word.";
						switch (Name)
						{
							case "minor zombie servant":
							case "lesser zombie servant":
							case "zombie servant":
							case "reanimated servant":
							case "necroservant": reply = taunt;
								break;
							case "greater necroservant": reply = taunt + " I can also inflict [poison] or [disease] on your enemies. " + empower;
								break;
							case "abomination": reply = "As one of the chosen warriors of Arawn, I have a mighty arsenal of [weapons] at your disposal. If you wish it, I am able to [taunt] your enemies so that they will focus on me instead of you. "
								+ empower;
								break;
							default: return false;
						}
					}
					break;
				case "disease": WhisperOwner("As you command.");
					return true;	// TODO
				case "empower": WhisperOwner("As you command.");
					Empower();
					return true;
				case "poison": WhisperOwner("As you command.");
					return true; // TODO
				case "taunt": return true;	// TODO
				case "weapons":
					{
						if (Name != "abomination")
							return false;

						reply = "What weapon do you command me to wield? A [fiery sword], [icy sword], [poisonous sword] or a [flaming mace], [frozen mace], [venomous mace]?";
						break;
					}
				case "fiery sword": 
				case "icy sword": 
				case "poisonous sword": 
				case "flaming mace": 
				case "frozen mace":
				case "venomous mace": 
					{
						if (Name != "abomination")
							return false;

						String templateID = String.Format("{0}_{1}", Name, text.Replace(" ", "_"));
						if (LoadEquipmentTemplate(templateID))
							WhisperOwner("As you command.");
						return true;
					}
				default: return false;
			}
			owner.Out.SendMessage(String.Format("The {0} says, \"{1}\"", Name, reply),
				eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			return true;
		}

        /// <summary>
        /// Load equipment for the pet.
        /// </summary>
        /// <param name="templateID">Equipment Template ID.</param>
        /// <returns>True on success, else false.</returns>
		private bool LoadEquipmentTemplate(String templateID)
		{
			if (templateID.Length > 0)
			{
				GameNpcInventoryTemplate inventoryTemplate = new GameNpcInventoryTemplate();
				if (inventoryTemplate.LoadFromDatabase(templateID))
				{
					Inventory = new GameNPCInventory(inventoryTemplate);
					InventoryItem item;
					if ((item = Inventory.GetItem(eInventorySlot.TwoHandWeapon)) != null)
					{
						item.DPS_AF = (int)(Level * 3.3);
						item.SPD_ABS = 50;
						switch (templateID)
						{
							case "abomination_fiery_sword": 
							case "abomination_flaming_mace":item.ProcSpellID = 32053;
								break;
							case "abomination_icy_sword": 
							case "abomination_frozen_mace":item.ProcSpellID = 32050;
								break;
							case "abomination_poisonous_sword":
								case "abomination_venomous_mace":
									break;
						}
						SwitchWeapon(eActiveWeaponSlot.TwoHanded);
					}
					else
					{
						if ((item = Inventory.GetItem(eInventorySlot.RightHandWeapon)) != null)
						{
							item.DPS_AF = (int)(Level * 3.3);
							item.SPD_ABS = 37;
						}
						SwitchWeapon(eActiveWeaponSlot.Standard);
					}
				}
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if (player == null) continue;
					player.Out.SendLivingEquipmentUpdate(this);
				}
				return true;
			}
			return false;
		}

        /// <summary>
        /// Pick a random style for now.
        /// </summary>
        /// <returns></returns>
        protected override Style GetStyleToUse()
        {
            if (m_petTemplate != null)
            {
                int styleCount = m_petTemplate.Styles.Count;
                if (styleCount > 0)
                    return (Style)(m_petTemplate.Styles[Util.Random(styleCount - 1)]);
            }

            return base.GetStyleToUse();
        }

		/// <summary>
		/// Pet-only insta spells.
		/// </summary>
		public String PetInstaSpellLine
		{
			get { return "Necro Pet Insta Spells"; }
		}

		/// <summary>
		/// Insta cast baseline buffs (STR+DEX) on the pet.
		/// </summary>
		private void Empower()
		{
            if (AttackState) return;

			SpellLine buffLine = SkillBase.GetSpellLine(PetInstaSpellLine);
			if (buffLine == null)
				return;

			IList buffList = SkillBase.GetSpellList(PetInstaSpellLine);
			if (buffList.Count == 0)
				return;

			// Find the best baseline buffs for this level.

			int maxLevel = Level;
			Spell strBuff = null, dexBuff = null;
			foreach (Spell spell in buffList)
			{
				if (spell.Level <= maxLevel)
				{
					switch (spell.SpellType)
					{
						case "StrengthBuff":
							{
								if (strBuff == null)
									strBuff = spell;
								else
									strBuff = (strBuff.Level < spell.Level) ? spell : strBuff;
							}
							break;
						case "DexterityBuff":
							{
								if (dexBuff == null)
									dexBuff = spell;
								else
									dexBuff = (dexBuff.Level < spell.Level) ? spell : dexBuff;
							}
							break;
					}
				}
			}

			// Insta buff.

			if (strBuff != null)
				CastSpell(strBuff, buffLine);
			if (dexBuff != null)
				CastSpell(dexBuff, buffLine);
		}

        /// <summary>
        /// Pet stayed out of range for too long, despawn it.
        /// </summary>
        public void CutTether()
        {
            GamePlayer owner = ((Brain as IControlledBrain).Owner) as GamePlayer;
            if (owner == null)
                return;
			Brain.Stop();
            owner.Notify(GameNPCEvent.PetLost);
            Die(null);
        }

		/// <summary>
		/// Send a whisper to the owner.
		/// </summary>
		/// <param name="message">Message.</param>
		private void WhisperOwner(String message)
		{
			GamePlayer owner = ((Brain as IControlledBrain).Owner) as GamePlayer;
			if (owner == null)
				return;

			owner.Out.SendMessage(String.Format("The {0} says, \"{1}\"", Name, message), 
				eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}
