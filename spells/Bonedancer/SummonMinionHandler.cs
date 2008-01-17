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
	/// 0 = melee
	/// 1 = healer
	/// 2 = mage
	/// 3 = debuffer
	/// 4 = Buffer
	/// 5 = Range
	/// </summary>
	[SpellHandler("SummonMinion")]
	public class SummonMinionHandler : SummonSpellHandler
	{
		public SummonMinionHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }

		/// <summary>
		/// All checks before any casting begins
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <returns></returns>
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (Caster is GamePlayer && ((GamePlayer)Caster).ControlledNpc == null)
			{
				MessageToCaster("You must have a controlled leader monster to summon into a group!", eChatType.CT_SpellResisted);
				return false;
			}

			if (Caster is GamePlayer && (((GamePlayer)Caster).ControlledNpc.Body.ControlledNpcList == null || ((GamePlayer)Caster).ControlledNpc.Body.PetCounter >= ((GamePlayer)Caster).ControlledNpc.Body.ControlledNpcList.Length))
			{
				MessageToCaster("Your general already has as many followers as he can command!", eChatType.CT_SpellResisted);
				return false;
			}
			//return base.CheckBeginCast(selectedTarget);
			return true;
		}

		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if (Caster == null)
				return;

			GameNPC pet = Caster.ControlledNpc.Body;
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
			target.GetSpotFromHeading(64, out x, out y);

			BDSubPet summoned = new BDSubPet(template, pet, (BDSubPet.SubPetType)(byte)Spell.DamageType);
			summoned.X = x;
			summoned.Y = y;
			summoned.Z = target.Z;
			summoned.CurrentRegion = target.CurrentRegion;
			summoned.Heading = (ushort)((target.Heading + 2048) % 4096);
			summoned.Realm = target.Realm;
			summoned.CurrentSpeed = 0;

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

			summoned.AddToWorld();
			summoned.UpdateNPCEquipmentAppearance();

			GameEventMgr.AddHandler(summoned, GameLivingEvent.PetReleased, new DOLEventHandler(OnNpcReleaseCommand));
			//Add the minion to the list - use 0 because it doesn't matter where it gets added
			if (pet.AddControlledNpc(summoned.Brain as IControlledBrain))
				effect.Start(summoned);
		}

		/// <summary>
		/// Called when owner release NPC
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected override void OnNpcReleaseCommand(DOLEvent e, object sender, EventArgs arguments)
		{
			GameNPC pet = sender as GameNPC;
			if (pet == null)
				return;

			GameEventMgr.RemoveHandler(pet, GameLivingEvent.PetReleased, new DOLEventHandler(OnNpcReleaseCommand));

			GameSpellEffect effect = FindEffectOnTarget(pet, this);
			if (effect != null)
				effect.Cancel(false);
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if ((effect.Owner is BDPet) && ((effect.Owner as BDPet).Brain is IControlledBrain) && (((effect.Owner as BDPet).Brain as IControlledBrain).Owner is CommanderPet))
			{
				BDPet pet = effect.Owner as BDPet;
				CommanderPet commander = (pet.Brain as IControlledBrain).Owner as CommanderPet;
				commander.RemoveControlledNpc(pet.Brain as IControlledBrain);
			}
			return base.OnEffectExpires(effect, noMessages);
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();

				list.Add(string.Format("Target: {0}", Spell.Target));
				list.Add(string.Format("Power cost: {0}", Math.Abs(Spell.Power)));
				list.Add(String.Format("Casting time: {0}", (Spell.CastTime / 1000).ToString("0.0## sec")));
				return list;
			}
		}
	}
}
