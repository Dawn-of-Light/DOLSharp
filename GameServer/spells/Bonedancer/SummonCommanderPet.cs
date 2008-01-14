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
using System.Text;
using DOL.GS.Effects;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.GS.PropertyCalc;
using System.Collections;
using DOL.Language;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell handler to summon a bonedancer pet.
	/// </summary>
	/// <author>IST</author>
	[SpellHandler("SummonCommander")]
	public class SummonCommanderPet : SummonSpellHandler
	{
		public SummonCommanderPet(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }

		/// <summary>
		/// Called after normal spell cast is completed and effect has to be started.
		/// </summary>
		public override void FinishSpellCast(GameLiving target)
		{
			foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (player != m_caster)
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameObject.Casting.CastsASpell", m_caster.GetName(0, true)), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			}

			// Now deduct mana for the spell.

			int powerCost = CalculateNeededPower(Caster);
			if (powerCost > 0)
				Caster.ChangeMana(Caster, DOL.GS.GameLiving.eManaChangeType.Spell, -powerCost);

			// Create the pet.

			StartSpell(target);

			GamePlayer playerCaster = Caster as GamePlayer;
			if (playerCaster != null)
			{
				IControlledBrain brain = playerCaster.ControlledNpc;
				if (brain != null)
				{
					MessageToCaster(String.Format("The {0} is now under your control.", brain.Body.Name),
						eChatType.CT_Spell);
					(brain.Body as CommanderPet).HailMaster();
				}
			}

			GameEventMgr.Notify(GameLivingEvent.CastFinished, m_caster, new CastSpellEventArgs(this));
		}

		/// <summary>
		/// Create the pet
		/// </summary>
		/// <param name="target">Target that gets the effect</param>
		/// <param name="effectiveness">Factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			GamePlayer player = Caster as GamePlayer;
			if (player == null) return;

			INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.LifeDrainReturn);
			if (template == null)
			{
				if (log.IsWarnEnabled)
					log.WarnFormat("NPC template {0} not found! Spell: {1}", Spell.LifeDrainReturn, Spell.ToString());
				MessageToCaster("NPC template " + Spell.LifeDrainReturn + " not found!", eChatType.CT_System);
				return;
			}

			GameSpellEffect effect = CreateSpellEffect(target, effectiveness);

			Caster.GetSpotFromHeading(64, out x, out y);
			z = Caster.Z;

			summoned = new CommanderPet(template, player);
			summoned.X = x;
			summoned.Y = y;
			summoned.Z = z;
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

			summoned.AddToWorld();

			GameEventMgr.AddHandler(summoned, GameLivingEvent.PetReleased, new DOLEventHandler(OnNpcReleaseCommand));

			player.SetControlledNpc((IControlledBrain)summoned.Brain);
			effect.Start(summoned);
		}

		protected override void OnNpcReleaseCommand(DOLEvent e, object sender, EventArgs arguments)
		{
			if (!(sender is CommanderPet))
				return;

			CommanderPet pet = sender as CommanderPet;

			if (pet.ControlledNpcList != null)
			{
				foreach (BDPetBrain cnpc in pet.ControlledNpcList)
				{
					if (cnpc != null)
						GameEventMgr.Notify(GameLivingEvent.PetReleased, cnpc.Body);
				}
			}
			base.OnNpcReleaseCommand(e, sender, arguments);
		}

		/// <summary>
		/// Delve info string.
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				ArrayList delve = new ArrayList();
				delve.Add("Function: summon");
				delve.Add("");
				delve.Add("Summons a pet to serve the caster.");
				delve.Add("");
				delve.Add(String.Format("Target: {0}", Spell.Target));
				delve.Add(String.Format("Power cost: {0}%", Math.Abs(Spell.Power)));
				delve.Add(String.Format("Casting time: {0}", (Spell.CastTime / 1000).ToString("0.0## sec")));
				return delve;
			}
		}
	}
}
