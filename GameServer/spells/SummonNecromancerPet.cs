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
	[SpellHandler("NecromancerPet")]
	public class NecromancerPetSpellHandler : SummonSpellHandler
	{        
		public NecromancerPetSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{

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

			INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.LifeDrainReturn);
			if (template == null)
			{
				if (log.IsWarnEnabled)
					log.WarnFormat("NPC template {0} not found! Spell: {1}", Spell.LifeDrainReturn, Spell.ToString());
				MessageToCaster("NPC template " + Spell.LifeDrainReturn + " not found!", eChatType.CT_System);
				return;
			}

			GameSpellEffect effect = CreateSpellEffect(target, effectiveness);

			target.GetSpotFromHeading(64, out x, out y);
			ControlledNpc controlledBrain = new ControlledNpc(Caster);
			NecromancerPet pet = new NecromancerPet(template);
			pet.SetOwnBrain(controlledBrain);
			pet.X = x;
			pet.Y = y;
			pet.Z = target.Z;
			pet.CurrentRegion = target.CurrentRegion;
			pet.Heading = (ushort)((target.Heading + 2048) % 4096);
			pet.Realm = target.Realm;
			pet.CurrentSpeed = 0;
			if (Spell.Damage < 0) pet.Level = (byte)(target.Level * Spell.Damage * -0.01);
			else pet.Level = (byte)Spell.Damage;
			if (pet.Level > Spell.Value) pet.Level = (byte)Spell.Value;
			pet.AddToWorld();
			
			GameEventMgr.AddHandler(Caster, GamePlayerEvent.CommandNpcRelease, new DOLEventHandler(OnNpcReleaseCommand));
			GameEventMgr.AddHandler(pet, GameLivingEvent.WhisperReceive, new DOLEventHandler(OnWhisperReceive));
			Caster.SetControlledNpc(controlledBrain);
			effect.Start(pet);
			summoned = (GameNPC)pet;
		}
		/// <summary>
		/// Called when owner release NPC
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected override void OnNpcReleaseCommand(DOLEvent e, object sender, EventArgs arguments)
		{
			base.OnNpcReleaseCommand(e,sender,arguments);
			GamePlayer player = sender as GamePlayer;
			if (player == null) return;
			player.Shade(false);			
		}
	}
}
