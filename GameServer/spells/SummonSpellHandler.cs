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
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

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
	/// </summary>
	[SpellHandler("Summon")]
	public class SummonSpellHandler : SpellHandler
	{
		protected int x, y, z;
		public GameNPC summoned = null;

		public SummonSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

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
			//Theurgist Petcap
			if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ID == (int)eCharacterClass.Theurgist && ((GamePlayer)Caster).PetCounter >= 16)
			{
				MessageToCaster("You have to many controlled Creatures!", eChatType.CT_SpellResisted);
				return false;
			}
			#warning Summon shouldn't be used, it needs to be just a base class and all others inherit
			if (Caster is GamePlayer && ((GamePlayer)Caster).ControlledNpc != null && ((GamePlayer)Caster).CharacterClass.ID != (int)eCharacterClass.Bonedancer)
			{
				MessageToCaster("You already have a charmed creature, release it first!", eChatType.CT_SpellResisted);
				return false;
			}

			if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ID == (int)eCharacterClass.Animist && Caster.GroundTarget == null)
			{
				MessageToCaster("You have to set a Areatarget for this Spell.", eChatType.CT_SpellResisted);
				return false;
			}

			if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ID == (int)eCharacterClass.Animist && !Caster.GroundTargetInView)
			{
				MessageToCaster("Your Areatarget is not in view.", eChatType.CT_SpellResisted);
				return false;
			}

			if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ID == (int)eCharacterClass.Animist && !WorldMgr.CheckDistance(Caster, Caster.GroundTarget, CalculateSpellRange()))
			{
				MessageToCaster("You have to select a closer Areatarget.", eChatType.CT_SpellResisted);
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
			GamePlayer player = Caster as GamePlayer;
			INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.LifeDrainReturn);
			if (template == null)
			{
				if (log.IsWarnEnabled)
					log.WarnFormat("NPC template {0} not found! Spell: {1}", Spell.LifeDrainReturn, Spell.ToString());
				MessageToCaster("NPC template " + Spell.LifeDrainReturn + " not found!", eChatType.CT_System);
				return;
			}

			GameSpellEffect effect = CreateSpellEffect(target, effectiveness);

			if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ID == (int)eCharacterClass.Animist)
			{
				x = Caster.GroundTarget.X;
				y = Caster.GroundTarget.Y;
				z = Caster.GroundTarget.Z;
			}
			else
			{
				Caster.GetSpotFromHeading(64, out x, out y);
				z = Caster.Z;
			}
			if (Spell.Duration < ushort.MaxValue)
			{
				ControlledNpc controlledBrain = new ControlledNpc(Caster);

				summoned = new GameNPC(template);
				controlledBrain.WalkState = eWalkState.Stay;
				controlledBrain.IsMainPet = false;
				summoned.SetOwnBrain(controlledBrain);
				summoned.HealthMultiplicator = true;
				summoned.X = x;
				summoned.Y = y;
				summoned.Z = z;
				summoned.CurrentRegion = Caster.CurrentRegion;
				summoned.Heading = Caster.Heading;
				summoned.Realm = Caster.Realm;
				summoned.CurrentSpeed = 0;
				if (Spell.Damage < 0) summoned.Level = (byte)(Caster.Level * Spell.Damage * -0.01);
				else summoned.Level = (byte)Spell.Damage;
				if (summoned.Level > Spell.Value) summoned.Level = (byte)Spell.Value;
				summoned.AddToWorld();
				effect.Start(summoned);
				//Check for buffs
				controlledBrain.CheckSpells(StandardMobBrain.eCheckSpellType.Defensive);
				controlledBrain.Attack(target);
				//Initialize the Theurgist Petcap
				if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ID == (int)eCharacterClass.Theurgist)
					((GamePlayer)Caster).PetCounter++;
			}
			else
			{
				ControlledNpc controlledBrain = new ControlledNpc(Caster);

				summoned = new GameNPC(template);
				summoned.SetOwnBrain(controlledBrain);
				summoned.X = x;
				summoned.Y = y;
				summoned.Z = z;
				summoned.CurrentRegion = target.CurrentRegion;
				summoned.Heading = (ushort)((target.Heading + 2048) % 4096);
				summoned.Realm = target.Realm;
				summoned.CurrentSpeed = 0;
				if (Spell.Damage < 0) summoned.Level = (byte)(target.Level * Spell.Damage * -0.01);
				else summoned.Level = (byte)Spell.Damage;
				if (summoned.Level > Spell.Value) summoned.Level = (byte)Spell.Value;

				summoned.AddToWorld();
				if (Caster is GamePlayer)
					GameEventMgr.AddHandler(summoned, GameLivingEvent.PetReleased, new DOLEventHandler(OnNpcReleaseCommand));

				if (Caster is GamePlayer)
					player.SetControlledNpc(controlledBrain);
				effect.Start(summoned);
			}
		}

		/// <summary>
		/// When an applied effect expires.
		/// Duration spells only.
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		/// <returns>immunity duration in milliseconds</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			//Remove Theurgist Pets
			if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ID == (int)eCharacterClass.Theurgist)
				((GamePlayer)Caster).PetCounter--;
			effect.Owner.Health = 0; // to send proper remove packet
			effect.Owner.Delete();
			return 0;
		}

		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

		/// <summary>
		/// Called when owner release NPC
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected virtual void OnNpcReleaseCommand(DOLEvent e, object sender, EventArgs arguments)
		{
			if (!(sender is GameNPC) || !((sender as GameNPC).Brain is IControlledBrain) || !(((sender as GameNPC).Brain as IControlledBrain).Owner is GamePlayer))
				return;
			GameNPC pet = sender as GameNPC;
			IControlledBrain brain = pet.Brain as IControlledBrain;
			GamePlayer player = brain.Owner as GamePlayer;

			player.SetControlledNpc(null);
			GameEventMgr.RemoveHandler(pet, GameLivingEvent.PetReleased, new DOLEventHandler(OnNpcReleaseCommand));

			GameSpellEffect effect = FindEffectOnTarget(pet, this);
			if (effect != null)
				effect.Cancel(false);
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
