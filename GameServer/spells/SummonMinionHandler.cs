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
using System.Reflection;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using log4net;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Pet summon spell handler
	/// 
	/// Spell.LifeDrainReturn is used for pet ID.
	///
	/// Spell.Value is used for hard pet level cap
	/// Spell.Damage is used to set pet level:
	/// less than zero is considered as a percent (0 .. 100+) of target level;
	/// higher than zero is considered as level value.
	/// Resulting value is limited by the Byte field type.
	/// Spell.DamageType is used to determine which type of pet is being cast:
	/// 1 = healer
	/// </summary>
	[SpellHandler("SummonMinion")]
	public class SummonMinionHandler : SpellHandler
	{
		public SummonMinionHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{

		}

		/// <summary>
		/// called after normal spell cast is completed and effect has to be started
		/// </summary>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// All checks before any casting begins
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <returns></returns>
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (Caster is GamePlayer && ((GamePlayer)Caster).ControlledNpc == null)
			{
				MessageToCaster("You must summon your Commander!", eChatType.CT_SpellResisted);
				return false;
			}

			if (Caster is GamePlayer && (((GamePlayer)Caster).ControlledNpc.Body.ControlledNpcList == null || ((GamePlayer)Caster).ControlledNpc.Body.PetCounter >= ((GamePlayer)Caster).ControlledNpc.Body.ControlledNpcList.Length))
			{
				MessageToCaster("You already have the max number of pets!", eChatType.CT_SpellResisted);
				return false;
			}
			return base.CheckBeginCast(selectedTarget);
		}

		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if (Caster == null)
			{
				return;
			}

			GameNPC pet = ((GameLiving)Caster).ControlledNpc.Body;
			//Lets let NPC's able to cast minions.  Here we make sure that the Caster is a GameNPC
			//and that m_controlledNpc is initialized (since we aren't thread safe).
			if (pet == null)
			{
				if (Caster is GameNPC)
				{
					pet = (GameNPC)Caster;
					//We'll give default NPCs 2 minions!
					if (pet.ControlledNpcList == null)
						pet.InitControlledNpc(2);
				}
				else
					return;
			}

			INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.LifeDrainReturn);
			if (template == null)
			{
				if (log.IsWarnEnabled)
					log.WarnFormat("NPC template {0} not found! Spell: {1}", Spell.LifeDrainReturn, Spell.ToString());
				MessageToCaster("NPC template " + Spell.LifeDrainReturn + " not found!", eChatType.CT_System);
				return;
			}

			int x, y;
			GameSpellEffect effect = CreateSpellEffect(target, effectiveness);
			if (Spell.Duration >= 65535)
			{
				target.GetSpotFromHeading(64, out x, out y);

				ControlledNpc controlledBrain = new ControlledNpc(pet);

				GameNPC summoned = new GameNPC(template);
				summoned.SetOwnBrain(controlledBrain);
				summoned.X = x;
				summoned.Y = y;
				summoned.Z = target.Z;
				summoned.CurrentRegion = target.CurrentRegion;
				summoned.Heading = (ushort)((target.Heading + 2048) % 4096);
				summoned.Realm = target.Realm;
				summoned.CurrentSpeed = 0;
				//We summoned a minion, make it one
				((IControlledBrain)summoned.Brain).IsMinion = true;

				if (Spell.Damage < 0)
					summoned.Level = (byte)(target.Level * Spell.Damage * -0.01);
				else
					summoned.Level = (byte)Spell.Damage;
				if (summoned.Level > Spell.Value)
					summoned.Level = (byte)Spell.Value;

				//edit for BD
				//Patch 1.87: subpets have been increased by one level to make them blue
				//to a level 50
				if (summoned.Level == 37 && pet.Level >= 41)
					summoned.Level = 41;

				switch (Spell.DamageType)
				{
					//Melee/range
					case (eDamageType)((byte)0):
						summoned.CanFight = true;

						break;
					//Healer
					case (eDamageType)((byte)1):
						summoned.CanFight = false;

						break;
					//Mage
					case (eDamageType)((byte)2):
						summoned.CanFight = true;

						break;
					//Debuffer/buffer
					case (eDamageType)((byte)3):
						summoned.CanFight = true;

						break;
				}

				summoned.AddToWorld();
				summoned.UpdateNPCEquipmentAppearance();

				GameEventMgr.AddHandler(summoned, GameLivingEvent.Dying, new DOLEventHandler(OnNpcReleaseCommand));
				//Add the minion to the list - use 0 because it doesn't matter where it gets added
				if (pet.AddControlledNpc(controlledBrain))
					effect.Start(summoned);
			}
		}

		/// <summary>
		/// Called when owner release NPC
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected virtual void OnNpcReleaseCommand(DOLEvent e, object sender, EventArgs arguments)
		{
			GameNPC pet = sender as GameNPC;
			if (pet == null) return;
			IControlledBrain npc = pet.Brain as IControlledBrain;

			GameEventMgr.RemoveHandler(pet, GameLivingEvent.Dying, new DOLEventHandler(OnNpcReleaseCommand));

			((GameNPC)npc.Owner).RemoveControlledNpc(npc);

			GameSpellEffect effect = FindEffectOnTarget(npc.Body, this);
			if (effect != null)
				effect.Cancel(false);
		}

		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();

				list.Add("Function: " + (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				if (Spell.InstrumentRequirement != 0)
					list.Add("Instrument require: " + GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement));
				list.Add("Target: " + Spell.Target);
				if (Spell.Range != 0)
					list.Add("Range: " + Spell.Range);
				if (Spell.Duration >= ushort.MaxValue * 1000)
					list.Add("Duration: Permanent.");
				else if (Spell.Duration > 60000)
					list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration / 60000, (Spell.Duration % 60000 / 1000).ToString("00")));
				else if (Spell.Duration != 0)
					list.Add("Duration: " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if (Spell.Frequency != 0)
					list.Add("Frequency: " + (Spell.Frequency * 0.001).ToString("0.0"));
				if (Spell.Power != 0)
					list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				list.Add("Casting time: " + (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				if (Spell.RecastDelay > 60000)
					list.Add("Recast time: " + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
					list.Add("Recast time: " + (Spell.RecastDelay / 1000).ToString() + " sec");
				if (Spell.Concentration != 0)
					list.Add("Concentration cost: " + Spell.Concentration);
				if (Spell.Radius != 0)
					list.Add("Radius: " + Spell.Radius);
				if (Spell.DamageType != eDamageType.Natural)
					list.Add("Damage: " + GlobalConstants.DamageTypeToName(Spell.DamageType));

				return list;
			}
		}
	}
}