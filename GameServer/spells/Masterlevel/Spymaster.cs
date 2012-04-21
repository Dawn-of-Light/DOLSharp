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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS;
using DOL.Events;
using DOL.GS.Spells;
using DOL.Database;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
    //http://www.camelotherald.com/masterlevels/ma.php?ml=Spymaster
    #region Spymaster-1
    //AbstractServerRules OnPlayerKilled
    #endregion

    #region Spymaster-2
    [SpellHandlerAttribute("Decoy")]
    public class DecoySpellHandler : SpellHandler
    {
        private GameDecoy decoy;
        private GameSpellEffect m_effect;
        /// <summary>
        /// Execute Decoy summon spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }
        public override bool IsOverwritable(GameSpellEffect compare)
        {
            return false;
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);
            decoy.AddToWorld();
            neweffect.Start(decoy);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            m_effect = effect;
            if (effect.Owner == null || !effect.Owner.IsAlive)
                return;
            GameEventMgr.AddHandler(decoy, GameLivingEvent.Dying, new DOLEventHandler(DecoyDied));
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameEventMgr.RemoveHandler(decoy, GameLivingEvent.Dying, new DOLEventHandler(DecoyDied));
            if (decoy != null)
            {
                decoy.Health = 0;
                decoy.Delete();
            }
            return base.OnEffectExpires(effect, noMessages);
        }
        private void DecoyDied(DOLEvent e, object sender, EventArgs args)
        {
            GameNPC kDecoy = sender as GameNPC;
            if (kDecoy == null) return;
            if (e == GameLivingEvent.Dying)
            {
                MessageToCaster("Your Decoy has fallen!", eChatType.CT_SpellExpires);
                OnEffectExpires(m_effect, true);
                return;
            }
        }
        public DecoySpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            Random m_rnd = new Random();
            decoy = new GameDecoy();
            //Fill the object variables
            decoy.CurrentRegion = caster.CurrentRegion;
            decoy.Heading = (ushort)((caster.Heading + 2048) % 4096);
            decoy.Level = 50;
            decoy.Realm = caster.Realm;
            decoy.X = caster.X;
            decoy.Y = caster.Y;
            decoy.Z = caster.Z;
            string TemplateId = "";
            switch (caster.Realm)
            {
                case eRealm.Albion:
                    decoy.Name = "Avalonian Unicorn Knight";
                    decoy.Model = (ushort)m_rnd.Next(61, 68);
                    TemplateId = "e3ead77b-22a7-4b7d-a415-92a29295dcf7";
                    break;
                case eRealm.Midgard:
                    decoy.Name = "Kobold Elding Herra";
                    decoy.Model = (ushort)m_rnd.Next(169, 184);
                    TemplateId = "ee137bff-e83d-4423-8305-8defa2cbcd7a";
                    break;
                case eRealm.Hibernia:
                    decoy.Name = "Elf Gilded Spear";
                    decoy.Model = (ushort)m_rnd.Next(334, 349);
                    TemplateId = "a4c798a2-186a-4bda-99ff-ccef228cb745";
                    break;
            }
            GameNpcInventoryTemplate load = new GameNpcInventoryTemplate();
            if (load.LoadFromDatabase(TemplateId))
            {
                decoy.EquipmentTemplateID = TemplateId;
                decoy.Inventory = load;
                decoy.UpdateNPCEquipmentAppearance();
            }
            decoy.CurrentSpeed = 0;
            decoy.GuildName = "";
        }
    }
    #endregion

    #region Spymaster-3
    //Gameliving - StartWeaponMagicalEffect
    #endregion

    #region Spymaster-4
    [SpellHandlerAttribute("Sabotage")]
    public class SabotageSpellHandler : SpellHandler
    {
        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            if (target is GameFont)
            {
                GameFont targetFont = target as GameFont;
                targetFont.Delete();
                MessageToCaster("Selected ward has been saboted!", eChatType.CT_SpellResisted);
            }
        }

        public SabotageSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    //shared timer 1
    #region Spymaster-5
    [SpellHandlerAttribute("TangleSnare")]
    public class TangleSnareSpellHandler : MineSpellHandler
    {
        // constructor
        public TangleSnareSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            Unstealth = false;

            //Construct a new mine.
            mine = new GameMine();
            mine.Model = 2592;
            mine.Name = spell.Name;
            mine.Realm = caster.Realm;
            mine.X = caster.X;
            mine.Y = caster.Y;
            mine.Z = caster.Z;
            mine.CurrentRegionID = caster.CurrentRegionID;
            mine.Heading = caster.Heading;
            mine.Owner = (GamePlayer)caster;

            // Construct the mine spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7220;
            dbs.ClientEffect = 7220;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "SpeedDecrease";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            trap = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }
    #endregion

    //shared timer 1
    #region Spymaster-6
    [SpellHandlerAttribute("PoisonSpike")]
    public class PoisonSpikeSpellHandler : MineSpellHandler
    {
        // constructor
        public PoisonSpikeSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            Unstealth = false;

            //Construct a new font.
            mine = new GameMine();
            mine.Model = 2589;
            mine.Name = spell.Name;
            mine.Realm = caster.Realm;
            mine.X = caster.X;
            mine.Y = caster.Y;
            mine.Z = caster.Z;
            mine.CurrentRegionID = caster.CurrentRegionID;
            mine.Heading = caster.Heading;
            mine.Owner = (GamePlayer)caster;

            // Construct the mine spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7281;
            dbs.ClientEffect = 7281;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Enemy";
            dbs.Radius = 350;
            dbs.Type = "PoisonspikeDot";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            trap = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }
    #region Subspell
    [SpellHandlerAttribute("PoisonspikeDot")]
    public class Spymaster6DotHandler : DoTSpellHandler
    {
        // constructor
        public Spymaster6DotHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
        public override int CalculateSpellResistChance(GameLiving target) { return 0; }
        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
            return new GameSpellEffect(this, m_spell.Duration, m_spellLine.IsBaseLine ? 5000 : 4000, effectiveness);
        }
    }
    #endregion
    #endregion

    #region Spymaster-7
    [SpellHandlerAttribute("Loockout")]
    public class LoockoutSpellHandler : SpellHandler
    {
        private GameLiving m_target;

        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (!(selectedTarget is GamePlayer)) return false;
            if (!selectedTarget.IsSitting) { MessageToCaster("Target must be sitting!", eChatType.CT_System); return false; }
            return base.CheckBeginCast(selectedTarget);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            m_target = effect.Owner as GamePlayer;
            if (m_target == null) return;
            if (!m_target.IsAlive || m_target.ObjectState != GameLiving.eObjectState.Active || !m_target.IsSitting) return;
            Caster.BaseBuffBonusCategory[(int)eProperty.Skill_Stealth] += 100;
            GameEventMgr.AddHandler(m_target, GamePlayerEvent.Moving, new DOLEventHandler(PlayerAction));
            GameEventMgr.AddHandler(Caster, GamePlayerEvent.Moving, new DOLEventHandler(PlayerAction));
            new LoockoutOwner().Start(Caster);
            base.OnEffectStart(effect);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            Caster.BaseBuffBonusCategory[(int)eProperty.Skill_Stealth] -= 100;
            GameEventMgr.RemoveHandler(Caster, GamePlayerEvent.Moving, new DOLEventHandler(PlayerAction));
            GameEventMgr.RemoveHandler(m_target, GamePlayerEvent.Moving, new DOLEventHandler(PlayerAction));
            return base.OnEffectExpires(effect, noMessages);
        }

        private void PlayerAction(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = (GamePlayer)sender;
            if (player == null) return;
            MessageToLiving((GameLiving)player, "You are moving. Your concentration fades!", eChatType.CT_SpellResisted);
            GameSpellEffect effect = SpellHandler.FindEffectOnTarget(m_target, "Loockout");
            if (effect != null) effect.Cancel(false);
            IGameEffect effect2 = SpellHandler.FindStaticEffectOnTarget(Caster, typeof(LoockoutOwner));
            if (effect2 != null) effect2.Cancel(false);
            OnEffectExpires(effect, true);
        }
        public LoockoutSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    //shared timer 1
    #region Spymaster-8
    [SpellHandlerAttribute("SiegeWrecker")]
    public class SiegeWreckerSpellHandler : MineSpellHandler
    {
        public override void OnEffectPulse(GameSpellEffect effect)
        {
            if (mine == null || mine.ObjectState == GameObject.eObjectState.Deleted)
            {
                effect.Cancel(false);
                return;
            }

            if (trap == null) return;
            bool wasstealthed = ((GamePlayer)Caster).IsStealthed;
            foreach (GameNPC npc in mine.GetNPCsInRadius((ushort)s.Range))
            {
                if (npc is GameSiegeWeapon && npc.IsAlive && GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
                {
                    trap.StartSpell((GameLiving)npc);
                    if (!Unstealth) ((GamePlayer)Caster).Stealth(wasstealthed);
                    return;
                }
            }
        }
        // constructor
        public SiegeWreckerSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            Unstealth = false;

            //Construct a new mine.
            mine = new GameMine();
            mine.Model = 2591;
            mine.Name = spell.Name;
            mine.Realm = caster.Realm;
            mine.X = caster.X;
            mine.Y = caster.Y;
            mine.Z = caster.Z;
            mine.MaxSpeedBase = 0;
            mine.CurrentRegionID = caster.CurrentRegionID;
            mine.Heading = caster.Heading;
            mine.Owner = (GamePlayer)caster;

            // Construct the mine spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7301;
            dbs.ClientEffect = 7301;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "DirectDamage";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            trap = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }
    #endregion

    #region Spymaster-9
    [SpellHandlerAttribute("EssenceFlare")]
    public class EssenceFlareSpellHandler : SummonItemSpellHandler
    {
        public EssenceFlareSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>("Meschgift");
            if (template != null)
            {
                items.Add(GameInventoryItem.Create<ItemTemplate>(template));
                foreach (InventoryItem item in items)
                {
                    if (item.IsStackable)
                    {
                        item.Count = 1;
                        item.Weight = item.Count * item.Weight;
                    }
                }
            }
            }
        }
    #endregion

        #region Spymaster-10
        [SpellHandler("BlanketOfCamouflage")]
        public class GroupstealthHandler : MasterlevelHandling
        {
            public GroupstealthHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

            private GameSpellEffect m_effect;

            public override bool CheckBeginCast(GameLiving selectedTarget)
            {
                if (selectedTarget == Caster) return false;
                return base.CheckBeginCast(selectedTarget);
            }
            public override void OnEffectStart(GameSpellEffect effect)
            {
                base.OnEffectStart(effect);
                m_effect = effect;
                if (effect.Owner is GamePlayer)
                {
                    GamePlayer playerTarget = effect.Owner as GamePlayer;
                    playerTarget.Stealth(true);
                    if (effect.Owner != Caster)
                    {
                        //effect.Owner.BuffBonusCategory1[(int)eProperty.Skill_Stealth] += 80;
                        GameEventMgr.AddHandler(playerTarget, GamePlayerEvent.Moving, new DOLEventHandler(PlayerAction));
                        GameEventMgr.AddHandler(playerTarget, GamePlayerEvent.AttackFinished, new DOLEventHandler(PlayerAction));
                        GameEventMgr.AddHandler(playerTarget, GamePlayerEvent.CastStarting, new DOLEventHandler(PlayerAction));
                        GameEventMgr.AddHandler(playerTarget, GamePlayerEvent.Dying, new DOLEventHandler(PlayerAction));
                    }
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
                if (effect.Owner != Caster && effect.Owner is GamePlayer)
                {
                    //effect.Owner.BuffBonusCategory1[(int)eProperty.Skill_Stealth] -= 80;
                    GamePlayer playerTarget = effect.Owner as GamePlayer;
                    GameEventMgr.RemoveHandler(playerTarget, GamePlayerEvent.AttackFinished, new DOLEventHandler(PlayerAction));
                    GameEventMgr.RemoveHandler(playerTarget, GamePlayerEvent.CastStarting, new DOLEventHandler(PlayerAction));
                    GameEventMgr.RemoveHandler(playerTarget, GamePlayerEvent.Moving, new DOLEventHandler(PlayerAction));
                    GameEventMgr.RemoveHandler(playerTarget, GamePlayerEvent.Dying, new DOLEventHandler(PlayerAction));
                    playerTarget.Stealth(false);
                }
                return base.OnEffectExpires(effect, noMessages);
            }
            private void PlayerAction(DOLEvent e, object sender, EventArgs args)
            {
                GamePlayer player = (GamePlayer)sender;
                if (player == null) return;
                if (args is AttackFinishedEventArgs)
                {
                    MessageToLiving((GameLiving)player, "You are attacking. Your camouflage fades!", eChatType.CT_SpellResisted);
                    OnEffectExpires(m_effect, true);
                    return;
                }
                if (args is DyingEventArgs)
                {
                    OnEffectExpires(m_effect, false);
                    return;
                }
                if (args is CastingEventArgs)
                {
                    if ((args as CastingEventArgs).SpellHandler.Caster != Caster)
                        return;
                    MessageToLiving((GameLiving)player, "You are casting a spell. Your camouflage fades!", eChatType.CT_SpellResisted);
                    OnEffectExpires(m_effect, true);
                    return;
                }
                if (e == GamePlayerEvent.Moving)
                {
                    MessageToLiving((GameLiving)player, "You are moving. Your camouflage fades!", eChatType.CT_SpellResisted);
                    OnEffectExpires(m_effect, true);
                    return;
                }
            }
        }
        #endregion
    }
    //to show an Icon & informations to the caster
    namespace DOL.GS.Effects
    {
        public class LoockoutOwner : StaticEffect, IGameEffect
        {
            public LoockoutOwner() : base() { }
            public void Start(GamePlayer player) { base.Start(player); }
            public override void Stop() { base.Stop(); }
            public override ushort Icon { get { return 2616; } }
            public override string Name { get { return "Loockout"; } }
            public override IList<string> DelveInfo
            {
                get
                {
                    var delveInfoList = new List<string>();
                    delveInfoList.Add("Your stealth range is increased.");
                    return delveInfoList;
                }
            }
        }
    }

