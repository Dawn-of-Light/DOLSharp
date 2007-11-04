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

namespace DOL.GS.Spells
{
	[SpellHandler("SummonNecroPet")]
	public class SummonNecromancerPet : SummonSpellHandler
	{
		public SummonNecromancerPet(GameLiving caster, Spell spell, SpellLine line) 
			: base(caster, spell, line) { }

		private int m_summonConBonus;
		private int m_summonHitsBonus;

		/// <summary>
		/// Note bonus constitution and bonus hits from items, then 
		/// summon the pet.
		/// </summary>
		public override void CastSpell()
		{
			// First check current item bonuses for constitution and hits
            // (including cap increases) of the caster.

            int conCap = StatCalculator.GetItemBonusCap(Caster, eProperty.Constitution) 
                + StatCalculator.GetItemBonusCapIncrease(Caster, eProperty.Constitution);
            int hitsCap = MaxHealthCalculator.GetItemBonusCap(Caster) 
                + MaxHealthCalculator.GetItemBonusCapIncrease(Caster);

			m_summonConBonus = Math.Min(Caster.ItemBonus[(int)(eProperty.Constitution)], conCap);
			m_summonHitsBonus = Math.Min(Caster.ItemBonus[(int)(eProperty.MaxHealth)], hitsCap);

            // Now summon the pet.

			base.CastSpell();
		}

        /// <summary>
        /// Check if caster is already in shade form.
        /// </summary>
        /// <param name="selectedTarget"></param>
        /// <returns></returns>
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (FindStaticEffectOnTarget(Caster, typeof(ShadeEffect)) != null)
            {
                MessageToCaster("You are already a shade!", eChatType.CT_System);
                return false;
            }
            return base.CheckBeginCast(selectedTarget);
        }

		/// <summary>
		/// Necromancer RR5 ability: Call of Darkness
		/// When active, the necromancer can summon a pet with only a 3 second cast time. 
		/// The effect remains active for 15 minutes, or until a pet is summoned.
		/// </summary>
		/// <returns></returns>
		public override int CalculateCastingTime()
		{
			if (Caster.EffectList.GetOfType(typeof(CallOfDarknessEffect)) != null)
				return 3000;

			return base.CalculateCastingTime();
		}

        /// <summary>
        /// Called after normal spell cast is completed and effect has to be started.
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
            {
                if (player != m_caster)
                    player.Out.SendMessage(m_caster.GetName(0, true) + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
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
					(brain.Body as NecromancerPet).HailMaster();
				}
            }

            GameEventMgr.Notify(GameLivingEvent.CastFinished, m_caster, new CastSpellEventArgs(this));
        }

		/// <summary>
		/// Create the pet and transfer stats.
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

			summoned = new NecromancerPet(template, player, m_summonConBonus, m_summonHitsBonus);
			summoned.X = x;
			summoned.Y = y;
			summoned.Z = z;
			summoned.CurrentRegion = target.CurrentRegion;
			summoned.Heading = (ushort)((target.Heading + 2048) % 4096);
			summoned.Realm = target.Realm;
			summoned.CurrentSpeed = 0;

			// Pet level will be 88% of the level of the caster +1, except for
			// the minor zombie servant, which will cap out at level 2 (patch 1.87).

			if (Spell.Damage < 0)
			{
				double petLevel = target.Level * Spell.Damage * -0.01 + 1;
				summoned.Level = (byte)((summoned.Name == "minor zombie servant")
					? Math.Min(2, petLevel) : petLevel);
			}
			else 
				summoned.Level = (byte)Spell.Damage;

			summoned.AddToWorld();
			player.Shade(true);

			GameEventMgr.AddHandler(player, GamePlayerEvent.CommandNpcRelease, new DOLEventHandler(OnNpcReleaseCommand));
			GameEventMgr.AddHandler(summoned, GameLivingEvent.WhisperReceive, new DOLEventHandler(OnWhisperReceive));

			player.SetControlledNpc((IControlledBrain)summoned.Brain);
			effect.Start(summoned);

			// Cancel RR5 Call of Darkness if on caster.

			IGameEffect callOfDarkness = FindStaticEffectOnTarget(Caster, typeof(CallOfDarknessEffect));
			if (callOfDarkness != null)
				callOfDarkness.Cancel(false);
		}

		/// <summary>
		/// Delve info string.
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				ArrayList delve = new ArrayList();
				delve.Add("Function: shade summon");
				delve.Add("");
				delve.Add("Summons an undead pet to serve the caster. The caster is transformed into a shade, and acts through the pet.");
				delve.Add("");
				delve.Add(String.Format("Target: {0}", Spell.Target));
				delve.Add(String.Format("Power cost: {0}%", Math.Abs(Spell.Power)));
				delve.Add(String.Format("Casting time: {0}", (Spell.CastTime / 1000).ToString("0.0## sec")));
				return delve;
			}
		}
	}
}
