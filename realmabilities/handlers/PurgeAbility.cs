using System;
using System.Collections;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;
using System.Collections.Generic;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    /// Purge Ability, removes negative effects
    /// </summary>
    public class PurgeAbility : TimedRealmAbility
    {
        public PurgeAbility(DBAbility dba, int level) : base(dba, level) { }

        /// <summary>
        /// Action
        /// </summary>
        /// <param name="living"></param>
        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING)) return;

            if (Level < 2)
            {
                PurgeTimer timer = new PurgeTimer(living, this);
                timer.Interval = 1000;
                timer.Start(1);
                DisableSkill(living);
            }
            else
            {
                SendCastMessage(living);
                if (RemoveNegativeEffects(living, this))
                {
                    DisableSkill(living);
                }
            }
        }

        protected static bool RemoveNegativeEffects(GameLiving living, PurgeAbility purge)
        {
            bool removed = false;
            ArrayList effects = new ArrayList();


            GamePlayer player = (GamePlayer)living;

            if (player.CharacterClass.ID == (int)eCharacterClass.Necromancer)
            {
                NecromancerPet necroPet = (NecromancerPet)player.ControlledNpcBrain.Body;
                lock (necroPet.EffectList)
                {
                    foreach (IGameEffect effect in necroPet.EffectList)
                    {
                        GameSpellEffect gsp = (GameSpellEffect)effect;

                        if (gsp == null)
                            continue;
                        if (gsp is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)gsp).ImmunityState)
                            continue;
                        if (gsp.SpellHandler.HasPositiveEffect)
                            continue;

                        effects.Add(gsp);
                        removed = true;
                    }
                }
            }

            lock (living.EffectList)
            {
                foreach (IGameEffect effect in living.EffectList)
                {
                    GameSpellEffect gsp = effect as GameSpellEffect;
                    if (gsp == null)
                        continue;
                    if (gsp is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)gsp).ImmunityState)
                        continue; // ignore immunity effects
                    if (gsp.SpellHandler.HasPositiveEffect)//only enemy spells are affected
                        continue;
                    /*
                    if (gsp.SpellHandler is RvRResurrectionIllness)
                       continue;
                     */
                    //if (gsp.Spell.SpellType == "DesperateBowman")//Can't be purged
                    //continue;
                    effects.Add(gsp);
                    removed = true;
                }

                foreach (IGameEffect effect in effects)
                {
                    effect.Cancel(false);
                }
            }

            if (player != null)
            {
                foreach (GamePlayer rangePlayer in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (player.CharacterClass.ID == (int)eCharacterClass.Necromancer)
                    {
                        rangePlayer.Out.SendSpellEffectAnimation(player.ControlledNpcBrain.Body,
                            player.ControlledNpcBrain.Body, 7011, 0,
                            false, (byte)(removed ? 1 : 0));
                    }

                    rangePlayer.Out.SendSpellEffectAnimation(player, player, 7011, 0, false, (byte)(removed ? 1 : 0));
                }
                if (removed)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "PurgeAbility.RemoveNegativeEffects.FallFromYou"), eChatType.CT_Advise, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    player.DisableSkill(purge, 5);
                }
            }
            if (removed)
                player.Stealth(false);
            return removed;
        }

        protected class PurgeTimer : GameTimer
        {
            GameLiving m_caster;
            PurgeAbility m_purge;
            int counter;

            public PurgeTimer(GameLiving caster, PurgeAbility purge)
                : base(caster.CurrentRegion.TimeManager)
            {
                m_caster = caster;
                m_purge = purge;
                counter = 5;
            }
            protected override void OnTick()
            {
                if (!m_caster.IsAlive)
                {
                    Stop();
                    if (m_caster is GamePlayer)
                    {
                        ((GamePlayer)m_caster).DisableSkill(m_purge, 0);
                    }
                    return;
                }
                if (counter > 0)
                {
                    GamePlayer player = m_caster as GamePlayer;
                    if (player != null)
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "PurgeAbility.OnTick.PurgeActivate", counter), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    counter--;
                    return;
                }
                m_purge.SendCastMessage(m_caster);
                RemoveNegativeEffects(m_caster, m_purge);
                Stop();
            }
        }

        public override int GetReUseDelay(int level)
        {
            return (level < 3) ? 900 : 300;
        }

        public override void AddEffectsInfo(IList<string> list)
        {
            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PurgeAbility.AddEffectsInfo.Info1"));
            list.Add("");
            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PurgeAbility.AddEffectsInfo.Info2"));
            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "PurgeAbility.AddEffectsInfo.Info3"));
        }
    }
}