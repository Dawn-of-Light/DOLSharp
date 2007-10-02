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

            if (Caster is GamePlayer && ((GamePlayer)Caster).ControlledNpc != null)
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

                GameNPC summoned = new GameNPC(template);
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
				controlledBrain.CheckSpells(true);
				controlledBrain.Attack(target);
                //Initialize the Theurgist Petcap
                if (Caster is GamePlayer && ((GamePlayer)Caster).CharacterClass.ID == (int)eCharacterClass.Theurgist)
                    ((GamePlayer)Caster).PetCounter++;
            }
            else
            {
                ControlledNpc controlledBrain = new ControlledNpc(Caster);

                GameNPC summoned = new GameNPC(template);
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
                //TODO: add in here based on summoned.Level how many minions can be summoned
                if (Caster is GamePlayer && player.CharacterClass.ID == (int)eCharacterClass.Bonedancer)
                {
                    switch (summoned.Name.ToLower())
                    {
                        case "returned commander":
                        case "decayed commander":
                            summoned.InitControlledNpc(0);
                            break;
                        case "skeletal commander":
                            summoned.InitControlledNpc(1);
                            break;
                        case "bone commander":
                            summoned.InitControlledNpc(2);
                            break;
                        case "dread commander":
                        case "dread guardian":
                        case "dread lich":
                        case "dread archer":
                            summoned.InitControlledNpc(3);
                            break;
                    }
                    if (summoned.Inventory != null)
                    {
                        if (summoned.Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
                            summoned.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance);
                        else if (summoned.Inventory.GetItem(eInventorySlot.RightHandWeapon) != null)
                            summoned.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
                        else if (summoned.Inventory.GetItem(eInventorySlot.TwoHandWeapon) != null)
                            summoned.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
                    }
                }
                summoned.AddToWorld();
                if (Caster is GamePlayer)
                    GameEventMgr.AddHandler(player, GamePlayerEvent.CommandNpcRelease, new DOLEventHandler(OnNpcReleaseCommand));
                GameEventMgr.AddHandler(summoned, GameLivingEvent.WhisperReceive, new DOLEventHandler(OnWhisperReceive));

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
			GamePlayer player = sender as GamePlayer;
			if (player == null) return;
			IControlledBrain npc = player.ControlledNpc;
			if (npc == null) return;
			if (npc.Body == null) return;

			player.SetControlledNpc(null);
			GameEventMgr.RemoveHandler(player, GamePlayerEvent.CommandNpcRelease, new DOLEventHandler(OnNpcReleaseCommand));
			GameEventMgr.RemoveHandler(npc.Body, GameLivingEvent.WhisperReceive, new DOLEventHandler(OnWhisperReceive));

			GameSpellEffect effect = FindEffectOnTarget(npc.Body, this);
			if (effect != null)
				effect.Cancel(false);
		}

		/// <summary>
		/// Called when owner sends a whisper to the pet
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected virtual void OnWhisperReceive(DOLEvent e, object sender, EventArgs arguments)
		{
			GameNPC body = sender as GameNPC;
			if (body == null) return;
			if (!(arguments is WhisperReceiveEventArgs)) return;
			GamePlayer player = (GamePlayer)((WhisperReceiveEventArgs)arguments).Source;
			if (player == null) return;

			string[] strargs = ((WhisperReceiveEventArgs)arguments).Text.ToLower().Split(' ');

			if (player.CharacterClass.ID == (int)eCharacterClass.Bonedancer)
			{
				for (int i = 0; i < strargs.Length; i++)
				{
					String str = strargs[i];
					string text = null;
					switch (str.ToLower())
					{
						case "commander":
							if (body.Name == "dread guardian")
								text = "The dread guardian says, \"As one of Bogdar's chosen guardians, I have mastered the ability to [harm] his enemies and [empower] myself with extra defenses.  I also have various [combat] tactics at your disposal.\"";
							else if (body.Name == "dread lich")
								text = "The dread lich says, \"As one of Bogdar's mystics, I have mastered many dark [spells].  I also have the means to [empower] my spells to be more effective.  I also have various [combat] tactics at your disposal.\"";
							else if (body.Name == "dread archer")
								text = "The dread archer says, \"As one of Bogdar's favored archers, you can [empower] me to be more effective with my bow.  I also have various [combat] tactics at your disposal.\"";
							else if (body.Name == "dread commander" || body.Name == "decayed commander" || body.Name == "returned commander" || body.Name == "skeletal commander" || body.Name == "bone commander")
								text = "The " + body.Name + " says, \"As one of Bogdar's commanders, I have mastered many of the fossil [weapons] within his bone army.  Which weapon shall I wield for you?  I also have various [combat] tactics at your disposal.\"";
							break;
						case "spells":
							if (body.Name != "dread lich") return;
							text = "The dread lich says, \"My arsenal consists of [snare], [debilitating], and pure [damage] spells.  I cast debilitating spells when first summoned but you can command me to cast a particular type of spells if you so desire.\"";
							break;
						case "empower":
							if (body.Name == "dread guardian" || body.Name == "dread lich" || body.Name == "dread archer")
							{
								foreach (Spell spell in body.Spells)
								{
									if (spell.Name == "Empower")
									{
										body.CastSpell(spell, null);
										break;
									}
								}
							}
							break;
						case "combat":
							text = "The " + body.Name + " says, \"From the start I order the minions under my control to [assist] me with a target your choose for me to kill.  Issuing the command again will make them cease assisting.  I am also able to [taunt] your enemies so that they will focus on me instead of you.\"";
							break;
						case "snares":
							break;
						case "debilitating":
							break;
						case "damage":
							break;
						case "assist":
							//TODO: implement this - I have no idea how to do that...
							break;
						case "taunt":
							bool found = false;
							foreach (Spell spell in body.Spells)
							{
								//If the taunt spell's ID is changed - this needs to be changed
								if (spell.ID == 60127)
								{
									body.Spells.Remove(spell);
									player.Out.SendMessage("Your commander will no long taunt its enemies!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
									found = true;
									break;
								}
							}
							if (found) break;
							//TODO: change this so it isn't hardcoded
							Spell tauntspell = SkillBase.GetSpellByID(60127);
							player.Out.SendMessage("Your commander will start to taunt its enemies!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							if (tauntspell != null)
								body.Spells.Add(tauntspell);
							else
								Console.WriteLine("Couldn't find BD pet's taunt spell");
							break;
						case "weapons":
							if (body.Name != "dread commander" && body.Name != "decayed commander" && body.Name != "returned commander" && body.Name != "skeletal commander" && body.Name != "bone commander") break;
							text = "The " + body.Name + " says, \"I have mastered the [one handed sword], the [two handed sword], the [one handed hammer], the [two handed hammer], the [one handed axe] and the [two handed axe].\"";
							break;
						case "one":
							i++;
							if (i + 1 >= strargs.Length)
								return;
							GameNPCInventory inven = body.Inventory as GameNPCInventory;
							switch (strargs[++i])
							{
								case "sword":
									if (inven != null)
									{
										if (inven.GetItem(eInventorySlot.RightHandWeapon) != null)
										{
											if (inven.GetItem(eInventorySlot.RightHandWeapon).Model == 3463) break;
											inven.RemoveItem(inven.GetItem(eInventorySlot.RightHandWeapon));
										}
										Database.InventoryItem newItem = new Database.InventoryItem();
										newItem.Model = 3463;
										newItem.Object_Type = (int)eObjectType.Sword;
										newItem.Hand = (int)eInventorySlot.RightHandWeapon;
										inven.AddItem(eInventorySlot.RightHandWeapon, newItem);
										body.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
										body.MeleeDamageType = eDamageType.Thrust;
										body.UpdateNPCEquipmentAppearance();
									}
									break;
								case "hammer":
									if (inven != null)
									{
										if (inven.GetItem(eInventorySlot.RightHandWeapon) != null)
										{
											if (inven.GetItem(eInventorySlot.RightHandWeapon).Model == 3466) break;
											inven.RemoveItem(inven.GetItem(eInventorySlot.RightHandWeapon));
										}
										Database.InventoryItem newItem = new Database.InventoryItem();
										newItem.Model = 3466;
										newItem.Object_Type = (int)eObjectType.Hammer;
										newItem.Hand = (int)eInventorySlot.RightHandWeapon;
										body.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
										body.MeleeDamageType = eDamageType.Crush;
										body.UpdateNPCEquipmentAppearance();
									}
									break;
								case "axe":
									if (inven != null)
									{
										if (inven.GetItem(eInventorySlot.RightHandWeapon) != null)
										{
											if (inven.GetItem(eInventorySlot.RightHandWeapon).Model == 3469) break;
											inven.RemoveItem(inven.GetItem(eInventorySlot.RightHandWeapon));
										}
										Database.InventoryItem newItem = new Database.InventoryItem();
										newItem.Model = 3469;
										newItem.Object_Type = (int)eObjectType.Axe;
										newItem.Hand = (int)eInventorySlot.RightHandWeapon;
										inven.AddItem(eInventorySlot.RightHandWeapon, newItem);
										body.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
										body.MeleeDamageType = eDamageType.Slash;
										body.UpdateNPCEquipmentAppearance();
									}
									break;
							}
							break;
						case "two":
							i++;
							if (i + 1 >= strargs.Length)
								return;
							inven = body.Inventory as GameNPCInventory;
							switch (strargs[++i])
							{
								case "sword":
									if (inven != null)
									{
										if (inven.GetItem(eInventorySlot.TwoHandWeapon) != null)
										{
											if (inven.GetItem(eInventorySlot.TwoHandWeapon).Model == 3462) break;
											inven.RemoveItem(inven.GetItem(eInventorySlot.TwoHandWeapon));
										}
										Database.InventoryItem newItem = new Database.InventoryItem();
										newItem.Model = 3462;
										newItem.Object_Type = (int)eObjectType.TwoHandedWeapon;
										newItem.Hand = (int)eInventorySlot.TwoHandWeapon;
										inven.AddItem(eInventorySlot.TwoHandWeapon, newItem);
										body.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
										body.MeleeDamageType = eDamageType.Thrust;
										body.UpdateNPCEquipmentAppearance();
									}
									break;
								case "hammer":
									if (inven != null)
									{
										if (inven.GetItem(eInventorySlot.TwoHandWeapon) != null)
										{
											if (inven.GetItem(eInventorySlot.TwoHandWeapon).Model == 3465) break;
											inven.RemoveItem(inven.GetItem(eInventorySlot.TwoHandWeapon));
										}
										Database.InventoryItem newItem = new Database.InventoryItem();
										newItem.Model = 3465;
										newItem.Object_Type = (int)eObjectType.TwoHandedWeapon;
										newItem.Hand = (int)eInventorySlot.TwoHandWeapon;
										inven.AddItem(eInventorySlot.TwoHandWeapon, newItem);
										body.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
										body.MeleeDamageType = eDamageType.Crush;
										body.UpdateNPCEquipmentAppearance();
									}
									break;
								case "axe":
									if (inven != null)
									{
										if (inven.GetItem(eInventorySlot.TwoHandWeapon) != null)
										{
											if (inven.GetItem(eInventorySlot.TwoHandWeapon).Model == 3468) break;
											inven.RemoveItem(inven.GetItem(eInventorySlot.TwoHandWeapon));
										}
										Database.InventoryItem newItem = new Database.InventoryItem();
										newItem.Model = 3468;
										newItem.Object_Type = (int)eObjectType.TwoHandedWeapon;
										newItem.Hand = (int)eInventorySlot.TwoHandWeapon;
										inven.AddItem(eInventorySlot.TwoHandWeapon, newItem);
										body.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
										body.MeleeDamageType = eDamageType.Slash;
										body.UpdateNPCEquipmentAppearance();
									}
									break;
							}
							break;
						case "harm":
							if (body.Name != "dread guardian") return;
							text = "The dread guardian says, \"Bogdar has granted me with the ability to [drain] the life of our enemies or to [suppress] and hurt their spirit.  I cast spirit damaging spells when first summoned but you can command me to cast a particular type of spell if you so desire.\"";
							break;
						case "drain":
							break;
						case "suppress":
							break;
						default:
							return;
					}
					if (text == null) return;
					player.Out.SendMessage(text, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}
			}
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

		//		public override void StartSpell(GameLiving target)
		//		{
		//			#region Add the Mob to World and set Pet Property for Caster
		//
		//			petMob.AddToWorld();
		//			petMob.NewOwner(m_PetOwner);
		//			foreach (GamePlayer player in target.GetPlayersInRadius((ushort)WorldMgr.VISIBILITY_DISTANCE))
		//			{
		//				player.Out.SendSpellEffectAnimation(m_caster, target, m_spell.ID, 0, false, 1);
		//			}
		//			if (m_PetOwner == m_caster)
		//			{
		//				if (pet.MaxOwnedPets == 0) ((GameLiving)m_caster).TempProperties.setProperty("Pet", petMob);
		//				GameSummoned mobpet = (GameSummoned)m_caster.TempProperties.getObjectProperty("Pet", null);
		//				if (mobpet == null) return;
		//			}
		//
		//			#endregion
		//		}
	}
}
