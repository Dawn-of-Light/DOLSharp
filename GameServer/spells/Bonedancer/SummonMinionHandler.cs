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
using DOL.Database;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;

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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SummonMinionHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line) { }

        /// <summary>
        /// All checks before any casting begins
        /// </summary>
        /// <param name="selectedTarget"></param>
        /// <returns></returns>
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (Caster is GamePlayer player && player.ControlledBrain == null)
            {
                MessageToCaster(LanguageMgr.GetTranslation(player.Client, "SummonMinionHandler.CheckBeginCast.Text1"), eChatType.CT_SpellResisted);
                return false;
            }

            if (Caster is GamePlayer gamePlayer && (gamePlayer.ControlledBrain.Body.ControlledNpcList == null || gamePlayer.ControlledBrain.Body.PetCount >= gamePlayer.ControlledBrain.Body.ControlledNpcList.Length))
            {
                MessageToCaster(LanguageMgr.GetTranslation(gamePlayer.Client, "SummonMinionHandler.CheckBeginCast.Text2"), eChatType.CT_SpellResisted);

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
            if (Caster?.ControlledBrain == null)
            {
                return;
            }

            GameNPC temppet = Caster.ControlledBrain.Body;

            // Lets let NPC's able to cast minions.  Here we make sure that the Caster is a GameNPC
            // and that m_controlledNpc is initialized (since we aren't thread safe).
            if (temppet == null)
            {
                if (Caster is GameNPC npc)
                {
                    temppet = npc;

                    // We'll give default NPCs 2 minions!
                    if (temppet.ControlledNpcList == null)
                    {
                        temppet.InitControlledBrainArray(2);
                    }
                }
                else
                {
                    return;
                }
            }

            base.ApplyEffectOnTarget(target, effectiveness);

            if (m_pet.Brain is BDArcherBrain)
            {
                if (!(GameServer.Database.FindObjectByKey<ItemTemplate>("BD_Archer_Distance_bow") is ItemTemplate temp))
                {
                    log.Error("Unable to find Bonedancer Archer's Bow");
                }
                else
                {
                    if (m_pet.Inventory == null)
                    {
                        m_pet.Inventory = new GameNPCInventory(new GameNpcInventoryTemplate());
                    }
                    else
                    {
                        m_pet.Inventory.RemoveItem(m_pet.Inventory.GetItem(eInventorySlot.DistanceWeapon));
                    }

                    m_pet.Inventory.AddItem(eInventorySlot.DistanceWeapon, GameInventoryItem.Create(temp));
                }

                m_pet.BroadcastLivingEquipmentUpdate();
            }
        }

        /// <summary>
        /// Called when owner release NPC
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected override void OnNpcReleaseCommand(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GameNPC pet))
            {
                return;
            }

            GameEventMgr.RemoveHandler(pet, GameLivingEvent.PetReleased, new DOLEventHandler(OnNpcReleaseCommand));

            GameSpellEffect effect = FindEffectOnTarget(pet, this);
            effect?.Cancel(false);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (((effect.Owner as BDPet)?.Brain as IControlledBrain)?.Owner is CommanderPet)
            {
                BDPet pet = (BDPet) effect.Owner;
                CommanderPet commander = (pet.Brain as IControlledBrain)?.Owner as CommanderPet;
                commander.RemoveControlledNpc(pet.Brain as IControlledBrain);
            }

            return base.OnEffectExpires(effect, noMessages);
        }

        protected override IControlledBrain GetPetBrain(GameLiving owner)
        {
            IControlledBrain controlledBrain;
            BDSubPet.SubPetType type = (BDSubPet.SubPetType)(byte)Spell.DamageType;
            owner = owner.ControlledBrain.Body;

            switch (type)
            {
                // Melee
                case BDSubPet.SubPetType.Melee:
                    controlledBrain = new BDMeleeBrain(owner);
                    break;

                // Healer
                case BDSubPet.SubPetType.Healer:
                    controlledBrain = new BDHealerBrain(owner);
                    break;

                // Mage
                case BDSubPet.SubPetType.Caster:
                    controlledBrain = new BDCasterBrain(owner);
                    break;

                // Debuffer
                case BDSubPet.SubPetType.Debuffer:
                    controlledBrain = new BDDebufferBrain(owner);
                    break;

                // Buffer
                case BDSubPet.SubPetType.Buffer:
                    controlledBrain = new BDBufferBrain(owner);
                    break;

                // Range
                case BDSubPet.SubPetType.Archer:
                    controlledBrain = new BDArcherBrain(owner);
                    break;

                // Other
                default:
                    controlledBrain = new ControlledNpcBrain(owner);
                    break;
            }

            return controlledBrain;
        }

        protected override GamePet GetGamePet(INpcTemplate template)
        {
            return new BDSubPet(template);
        }

        protected override void SetBrainToOwner(IControlledBrain brain)
        {
            Caster.ControlledBrain.Body.AddControlledNpc(brain);
        }

        protected override byte GetPetLevel()
        {
            byte level = base.GetPetLevel();

            // edit for BD
            // Patch 1.87: subpets have been increased by one level to make them blue
            // to a level 50
            if (level == 37 && ((IControlledBrain) m_pet.Brain).Owner.Level >= 41)
            {
                level = 41;
            }

            return level;
        }

        /// <summary>
        /// Delve Info
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var delve = new List<string>();
                delve.Add(string.Format(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "SummonMinionHandler.DelveInfo.Text1", Spell.Target)));
                delve.Add(string.Format(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "SummonMinionHandler.DelveInfo.Text2", Math.Abs(Spell.Power))));
                delve.Add(string.Format(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "SummonMinionHandler.DelveInfo.Text3", (Spell.CastTime / 1000).ToString("0.0## " + LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "Effects.DelveInfo.Seconds")))));
                return delve;
            }
        }
    }
}
