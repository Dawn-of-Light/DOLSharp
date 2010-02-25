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
using System.Reflection;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Language;
using log4net;


namespace DOL.GS.Spells
{
	/// <summary>
	/// Charms target NPC for the spell duration.
	/// 
	/// Spell.Value is used for hard NPC level cap
	/// Spell.Damage is used for percent of caster level cap
	/// </summary>
	[SpellHandlerAttribute("Charm")]
	public class CharmSpellHandler : SpellHandler
	{
		/// <summary>
		/// Holds the charmed Npc for pulsing spells
		/// </summary>
		protected GameNPC m_charmedNpc;

		/// <summary>
		/// The property that stores the new npc brain
		/// </summary>
		protected ControlledNpcBrain m_controlledBrain;

		/// <summary>
		/// Tells pulsing spells to not add brain if it was not removed by expire effect
		/// </summary>
		protected bool m_isBrainSet;

		/// <summary>
		/// called after normal spell cast is completed and effect has to be started
		/// </summary>
		public override void FinishSpellCast(GameLiving target)
		{
			Caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		public override void StartSpell(GameLiving target)
		{
			if (m_charmedNpc == null)
				m_charmedNpc = target as GameNPC; // save target on first start
			else
				target = m_charmedNpc; // reuse for pulsing spells

			if (target == null) return;
			if (Util.Chance(CalculateSpellResistChance(target)))
			{
				OnSpellResisted(target);
			}
			else
			{
				ApplyEffectOnTarget(target, 1);
			}
		}

		/// <summary>
		/// Calculates chance of spell getting resisted
		/// </summary>
		/// <param name="target">the target of the spell</param>
		/// <returns>chance that spell will be resisted for specific target</returns>
		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

		/// <summary>
		/// All checks before any casting begins
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <returns></returns>
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (selectedTarget == null || (selectedTarget != null && !selectedTarget.IsAlive))
			{
				MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
				return false;
			}
			if (selectedTarget is GameNPC == false)
			{//proper message?
				MessageToCaster("This spell works only on NPC.", eChatType.CT_SpellResisted);
				return false;
			}

			//You should be able to chain pulsing charm on the same mob
			if(Spell.Pulse!=0 && Caster is GamePlayer && (((GamePlayer)Caster).ControlledNpcBrain!=null && ((GamePlayer)Caster).ControlledNpcBrain.Body==(GameNPC)selectedTarget))
			{
				((GamePlayer)Caster).CommandNpcRelease();
			}
			
			if (!base.CheckBeginCast(selectedTarget)) return false;

			if (Caster is GamePlayer && ((GamePlayer)Caster).ControlledNpcBrain != null)
			{
				MessageToCaster("You already have a charmed creature, release it first!", eChatType.CT_SpellResisted);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			// check only if brain wasn't changed at least once
			if (m_controlledBrain == null)
			{
				if (target.Name.ToLower() != target.Name)
				{
					MessageToCaster("You can't charm this creature!", eChatType.CT_SpellResisted);
					return;
				}

				if (target.Realm != 0 || target is GameNPC == false)
				{
					MessageToCaster("This spell does not charm this type of monster!", eChatType.CT_SpellResisted);
					return;
				}
                //ToDo: Proper check for bodytypes but for now allowing ability to charm all bodytypes if amnesiachance is 0 so can have Bodytypes implemented without breaking charming - Sand
                if (m_spell.AmnesiaChance != 0)
                {    
                    if ((target as GameNPC).BodyType != m_spell.AmnesiaChance)
				    {
			   	        MessageToCaster("This spell does not charm this type of monster!", eChatType.CT_SpellResisted);
				        return;
				    }
                return;
                }
				if (Caster is GamePlayer && ((GamePlayer)Caster).ControlledNpcBrain != null)
				{
					MessageToCaster("You already have a charmed creature, release it first!", eChatType.CT_SpellResisted);
					return;
				}
				IControlledBrain brain = ((GameNPC)target).Brain as IControlledBrain;
				if (brain != null && (brain.Owner as GamePlayer)!=Caster)
				{
					// TODO: proper message
                    MessageToCaster("Your target is not valid.", eChatType.CT_SpellResisted);
					return;
				}
			}

			if (target.Level > Spell.Value || target.Level > Caster.Level * Spell.Damage / 100)
			{
				MessageToCaster(target.GetName(0, true) + " is too strong for you to charm!", eChatType.CT_SpellResisted);
				return;
			}

			if (Caster is GamePlayer)
			{
				/*
				 * The Minstrel/Mentalist has an almost certain chance to charm/retain control of 
				 * a creature his level or lower, although there is a small random chance that it
				 * could fail. The higher the level of the charmed creature compared to the 
				 * Minstrel/Mentalist, the greater the chance the monster has of breaking the charm.
				 * Please note that your specialization level in the magic skill that contains the
				 * charm spell will modify your base chance of charming and retaining control.
				 * The higher your spec level, the greater your chance of controlling.
				 */
				int diffLevel = (int)(Caster.Level/1.5 + Caster.GetModifiedSpecLevel(m_spellLine.Spec)/3) - target.Level;
				int resistChance;
				if (diffLevel >= 0)
				{
                    resistChance = 10 - diffLevel * 3;
                    resistChance = Math.Max(resistChance, 1);
				}
				else
				{
                    resistChance = 10 + diffLevel * diffLevel * 3;
                    resistChance = Math.Min(resistChance, 99);
				}
				
				if (Util.Chance(resistChance))
				{
					MessageToCaster(target.GetName(0, true) + " resists the charm!", eChatType.CT_SpellResisted);
					return;
				}
			}

			base.ApplyEffectOnTarget(target, effectiveness);
		}

		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);

			GamePlayer player = Caster as GamePlayer;
			GameNPC npc = effect.Owner as GameNPC;
			if (player != null && npc != null)
			{
				if (m_controlledBrain == null)
					m_controlledBrain = new ControlledNpcBrain(player);

				if (!m_isBrainSet)
				{
					npc.AddBrain(m_controlledBrain);
					m_isBrainSet = true;

					GameEventMgr.AddHandler(npc, GameLivingEvent.PetReleased, new DOLEventHandler(ReleaseEventHandler));
				}

				if (player.ControlledNpcBrain != m_controlledBrain)
				{
					// sorc: "The slough serpent is enthralled!" ct_spell
					Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message1, npc.GetName(0, false)), eChatType.CT_Spell);
					MessageToCaster(npc.GetName(0, true) + " is now under your control.", eChatType.CT_Spell);

					player.SetControlledNpcBrain(m_controlledBrain);
				}

				npc.BroadcastUpdate();
				//if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
				{
					foreach (GamePlayer ply in npc.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						ply.Out.SendObjectGuildID(npc, player.Guild);
				}
			}
			else
			{
				// hmm
				if (log.IsWarnEnabled)
					log.Warn(string.Format("charm effect start: Caster={0} effect.Owner={1}",
						(Caster == null ? "(null)" : Caster.GetType().ToString()),
						(effect.Owner == null ? "(null)" : effect.Owner.GetType().ToString())));
			}
		}

		/// <summary>
		/// Handles release commands
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		private void ReleaseEventHandler(DOLEvent e, object sender, EventArgs arguments)
		{
			IControlledBrain npc = null;
			if (e == GameLivingEvent.PetReleased)
				npc = ((GameNPC)sender).Brain as IControlledBrain;
			else if (e == GameLivingEvent.Dying)
				npc = ((GameNPC)sender).Brain as IControlledBrain;

			if (npc == null) return;

			PulsingSpellEffect concEffect = FindPulsingSpellOnTarget(npc.Owner, this);
			if (concEffect != null)
				concEffect.Cancel(false);

			GameSpellEffect charm = FindEffectOnTarget(npc.Body, this);
			if (charm == null)
			{
				log.Warn("charm effect is already canceled");
				return;
			}

			charm.Cancel(false);
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
			base.OnEffectExpires(effect, noMessages);

			GamePlayer player = Caster as GamePlayer;
			GameNPC npc = effect.Owner as GameNPC;
			if (player != null && npc != null)
			{
				if (!noMessages) // no overwrite
				{
					GameEventMgr.RemoveHandler(npc, GameLivingEvent.PetReleased, new DOLEventHandler(ReleaseEventHandler));

					player.SetControlledNpcBrain(null);
					MessageToCaster("You lose control of " + npc.GetName(0, false) + "!", eChatType.CT_SpellExpires);

					npc.BroadcastUpdate();

					foreach (GamePlayer ply in npc.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						ply.Out.SendObjectGuildID(npc, null);
					}

					lock (npc.BrainSync)
					{
						npc.StopAttack();
						npc.RemoveBrain(m_controlledBrain);
						m_isBrainSet = false;

						IOldAggressiveBrain aggroBrain = npc.Brain as IOldAggressiveBrain;
						if (aggroBrain != null)
						{
							aggroBrain.ClearAggroList();
							if (Spell.Pulse != 0 && Caster.ObjectState == GameObject.eObjectState.Active && Caster.IsAlive)
							{
								aggroBrain.AddToAggroList(Caster, Caster.Level * 10);
								npc.StartAttack(Caster);
							}
							else
							{
								npc.WalkToSpawn();
							}
						}
					}

					// remove NPC with new brain from all attackers aggro list
					ArrayList attackers;
					lock (npc.Attackers.SyncRoot)
					{
						attackers = new ArrayList(npc.Attackers);
					}
					foreach (GameObject obj in attackers)
					{
						GameNPC npcAttacker = obj as GameNPC;
						if (npcAttacker == null) continue;
						IOldAggressiveBrain attackerBrain = npcAttacker.Brain as IOldAggressiveBrain;
						if (attackerBrain != null)
							attackerBrain.RemoveFromAggroList(npc);
					}

					m_controlledBrain.ClearAggroList();
					npc.StopFollowing();

					npc.TempProperties.setProperty(GameNPC.CHARMED_TICK_PROP, npc.CurrentRegion.Time);
				}
			}
			else
			{
				if (log.IsWarnEnabled)
					log.Warn(string.Format("charm effect expired: Caster={0} effect.Owner={1}",
						(Caster == null ? "(null)" : Caster.GetType().ToString()),
						(effect.Owner == null ? "(null)" : effect.Owner.GetType().ToString())));
			}

			return 0;
		}

		/// <summary>
		/// Determines wether this spell is better than given one
		/// </summary>
		/// <param name="oldeffect"></param>
		/// <param name="neweffect"></param>
		/// <returns>true if this spell is better version than compare spell</returns>
		public override bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
		{
			Spell oldspell = oldeffect.Spell;
			Spell newspell = neweffect.Spell;
			if (oldspell.SpellType != newspell.SpellType)
			{
				if (log.IsWarnEnabled)
					log.Warn("Spell effect compare with different types " + oldspell.SpellType + " <=> " + newspell.SpellType + "\n" + Environment.StackTrace);
				return false;
			}
			return neweffect.SpellHandler == this;
		}

		/// <summary>
		/// Send the Effect Animation
		/// </summary>
		/// <param name="target">The target object</param>
		/// <param name="boltDuration">The duration of a bolt</param>
		/// <param name="noSound">sound?</param>
		/// <param name="success">spell success?</param>
		public override void SendEffectAnimation(GameObject target, ushort boltDuration, bool noSound, byte success)
		{
			base.SendEffectAnimation(m_charmedNpc, boltDuration, noSound, success);
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();

                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "CharmSpellHandler.DelveInfo.Function", (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType)));
				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				if (Spell.InstrumentRequirement != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.InstrumentRequire", GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement)));
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Target", Spell.Target));
				if (Spell.Range != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Range", Spell.Range));
				if (Spell.Duration >= ushort.MaxValue * 1000)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + " Permanent.");
				else if (Spell.Duration > 60000)
                    list.Add(string.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + Spell.Duration / 60000 + ":" + (Spell.Duration % 60000 / 1000).ToString("00") + " min"));
				else if (Spell.Duration != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if (Spell.Frequency != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")));
				if (Spell.Power != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")));
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
				if (Spell.RecastDelay > 60000)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.RecastTime") + Spell.RecastDelay / 60000 + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.RecastTime") + (Spell.RecastDelay / 1000).ToString() + " sec");
				if (Spell.Concentration != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.ConcentrationCost", Spell.Concentration));
				if (Spell.Radius != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Radius", Spell.Radius));
				if (Spell.DamageType != eDamageType.Natural)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType)));

				return list;
			}
		}

		// Constructs new Charm spell handler
		public CharmSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
		}

		/*

		http://www.camelotherald.com/more/1775.shtml

		... Can you please explain what the max level pet a hunter can charm if they are fully Beastcraft specd? The community feels its no higher then 41, but the builder says max level 50.

		A: Sayeth the Oracle: ”It's 82% of the caster's level for the highest charm in beastcraft; or level 41 if the caster is 50. Spec doesn't determine the level of the pet - it's purely based on the spell.”



		http://vnboards.ign.com/message.asp?topic=87170081&start=87173224&search=charm

		More info in the sticky thread, but... 


		<copies and pastes her charm spell info> 

		What you can charm: 
		4 - humanoids 
		10 - humanoids, animals 
		17 - humanoids, animals, insects 
		25 - humanoids, animals, insects, magical 
		33 - humanoids, animals, insects, magical, undead 
		42 - anything charmable 

		Always use lowest charm to save power. 

		Safety level formula: 
		(level * .66) + (spec level * .33) 
		spec level includes: trainings, items, and realm rank 

		Mastery of Focus: 
		Mastery of Focus affects SPELL level. Notice that SPELL level is not included in the above formula. SPEC level is important. If you raise the lvl 4 charm up to lvl 20 it makes NO difference to what you can charm. 

		Current charm bugs: 
		- Porting has the chance to completely break your charm if there is a delay in porting. Pet will show up at portal location very very mad. 
		- Porting also causes your pet to completely disappear. Walk away and it should reappear. Maybe 

		NOT A BUG, working as intended 
		- Artifact chants (Cloudsong, Crown, etc.) will interfere and overwrite your charm.





		sorc

		<Begin Info: Coerce Will>
		Function: charm
 
		Attempts to bring the target under the caster's control.
 
		Target: Targetted
		Range: 1000
		Duration: Permanent.
		Power cost: 25%
		Casting time:      4.0 sec
		Damage: Energy
 
		<End Info>

		[06:23:57] You begin casting a Coerce Will spell!
		[06:24:01] The slough serpent attacks you and misses!
		[06:24:01] You cast a Coerce Will Spell!
		[06:24:01] The slough serpent is enthralled!
		[06:24:01] The slough serpent is now under your control.

		[14:30:55] The frost stallion dies!
		[14:30:55] This monster has been charmed recently and is worth no experience.




		pulsing, mentalist

		<Begin Info: Imaginary Enemy>
		Function: charm
 
		Attempts to bring the target under the caster's control.
 
		Target: Targetted
		Range: 2000
		Duration: 10 sec
		Frequency:      4.8 sec
		Casting time:      3.0 sec
		Damage: Heat
 
		<End Info>

		[16:11:59] You begin casting a Imaginary Enemy spell!
		[16:11:59] You are already casting a spell!  You prepare this spell as a follow up!
		[16:12:01] You are already casting a spell!  You prepare this spell as a follow up!
		[16:12:02] You cast a Imaginary Enemy Spell!
		[16:12:02] The villainous youth is now under your control.
		[16:12:02] You cancel your effect.

		[16:11:42] You can't attack yourself!
		[16:11:42] You lose control of the villainous youth!
		[16:11:42] You lose control of the villainous youth.




		minstrel

		[09:00:12] <Begin Info: Attracting Melodies>
		[09:00:12] Function: charm
		[09:00:12]  
		[09:00:12] Attempts to bring the target under the caster's control.
		[09:00:12]  
		[09:00:12] Target: Targetted
		[09:00:12] Range: 2000
		[09:00:12] Duration: 10 sec
		[09:00:12] Frequency:      5.0 sec
		[09:00:12] Casting time: instant
		[09:00:12] Recast time: 5 sec
		[09:00:12]  
		[09:00:12] <End Info>

		[09:05:56] You command the the worker ant to kill your target!
		[09:05:59] The worker ant attacks the worker ant and hits!
		[09:06:00] The worker ant attacks the worker ant and hits!
		[09:06:01] You lose control of the worker ant!
		[09:06:01] You release control of your controlled target.

		[09:06:50] The worker ant is now under your control.
		[09:06:51] The worker ant attacks you and misses!
		[09:06:55] The worker ant attacks the worker ant and hits!
		[09:06:55] The worker ant resists the charm!

		*/
	}
}
