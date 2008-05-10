using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using DOL.Events;
using DOL.Database2;
using DOL.GS.Spells;
using DOL.GS.Effects;
using log4net;

namespace DOL.GS
{
    public sealed class MinotaurRelicManager
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// table of all relics, InternalID as key
        /// </summary>
        public static readonly Hashtable m_minotaurrelics = new Hashtable();

        /// <summary>
        /// Holds the maximum XP of Minotaur Relics
        /// </summary>
        public const double MAX_RELIC_EXP = 3750;
        /// <summary>
        /// Holds the minimum respawntime
        /// </summary>
        public const int MIN_RESPAWN_TIMER = 300000;
        /// <summary>
        /// Holds the maximum respawntime
        /// </summary>
        public const int MAX_RESPAWN_TIMER = 1800000;
        /// <summary>
        /// Holds the Value which is removed from the XP per tick
        /// </summary>
        public const double XP_LOSS_PER_TICK = 2.5;

        /// <summary>
        /// Inits the Minotaurrelics
        /// </summary>
        public static bool Init()
        {
            foreach (MinotaurRelic relic in m_minotaurrelics)
            {
                relic.SaveIntoDatabase();
                relic.RemoveFromWorld();
            }

            m_minotaurrelics.Clear();

            try
            {
                foreach (DBMinotaurRelic dbrelic in GameServer.Database.SelectObjects<DBMinotaurRelic>())
                {
                    if (WorldMgr.GetRegion((ushort)dbrelic.SpawnRegion) == null)
                    {
                        log.Warn("DBMinotaurRelic: Could not load " + dbrelic.ObjectId + ": Region missmatch.");
                        continue;
                    }

                    MinotaurRelic relic = new MinotaurRelic(dbrelic);

                    m_minotaurrelics.Add(relic.InternalID, relic);

                    relic.AddToWorld();
                }
                log.Info("Minotaur Relics properly loaded");
                return true;
            }
            catch (Exception e)
            {
                log.Error("Error loading Minotaur Relics", e);
                return false;
            }
        }

        #region Helpers
        /// <summary>
        /// Adds a Relic to the Hashtable
        /// </summary>
        /// <param name="relic">The Relic you want to add</param>
        public static bool AddRelic(MinotaurRelic relic)
        {
            if (m_minotaurrelics.ContainsValue(relic)) return false;

            lock (m_minotaurrelics.SyncRoot)
            {
                m_minotaurrelics.Add(relic.InternalID, relic);
            }

            return true;
        }

        public static int GetRelicCount()
        {
            return m_minotaurrelics.Count;
        }

        /// <summary>
        /// Returns the Relic with the given ID
        /// </summary>
        /// <param name="ID">The Internal ID of the Relic</param>
        public static MinotaurRelic GetRelic(string ID)
        {
            return m_minotaurrelics[ID] as MinotaurRelic;
        }

        public static MinotaurRelic GetRelic(int ID)
        {
            lock (m_minotaurrelics.SyncRoot)
            {
                foreach (MinotaurRelic relic in m_minotaurrelics.Values)
                {
                    if (relic.RelicID == ID)
                        return relic;
                }
            }
            return null;
        }
        #endregion

        /// <summary>
        /// Starts the Relic effect on the Player
        /// </summary>
        /// <param name="player">The Effect Owner</param>
        /// <param name="relic">The Relic which gives the effect</param>
        public static void StartPlayerRelicEffect(GamePlayer player, MinotaurRelic relic)
        {
            if (player == null || relic == null) return;

            ArrayList targetlist = GetTargets(player, relic.RelicTarget);

            if (targetlist.Count <= 0) return;

            foreach (GamePlayer target in targetlist)
            {
                if (target == null) continue;
                
                GameSpellEffect effect = GetRelicEffect(player, relic.RelicSpell, true);
                if (effect != null)
                {
                    GameSpellEffect check = SpellHandler.FindEffectOnTarget(target, effect.Spell.SpellType);
                    if (check == null)// && check.MinotaurEffect)
                        effect.Start(target);

                    if (effect.Spell.SubSpellID != 0)
                    {
                        GameSpellEffect subspell = GetRelicEffect(player, effect.Spell.SubSpellID, false);
                        if (subspell != null)
                        {
                            check = SpellHandler.FindEffectOnTarget(target, subspell.Spell.SpellType);
                            if (check == null)
                                subspell.Start(target);
                        }
                    }
                }
            }

            targetlist.Clear();
        }

        /// <summary>
        /// Returns the Targets of the Relic Spell
        /// </summary>
        /// <param name="player"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static ArrayList GetTargets(GamePlayer player, string target)
        {
            ArrayList targetlist = new ArrayList();

            switch (target)
            {
                case "group":
                    {
                        if (player.Group != null)
                        {
                            foreach (GamePlayer pl in player.Group.GetPlayersInTheGroup())
                            {
                                targetlist.Add(pl);
                            }
                        }
                        else
                            targetlist.Add(player);
                    }
                    break;
                case "self":
                    {
                        targetlist.Add(player);
                    }
                    break;
                case "realm":
                    {
                        foreach (GamePlayer pl in player.GetPlayersInRadius(5000))
                        {
                            if (player != null)
                                targetlist.Add(pl);
                        }
                    }
                    break;
            }

            return targetlist;
        }

        /// <summary>
        /// Returns the Relic Effect of the Relic
        /// </summary>
        /// <param name="player"></param>
        /// <param name="spellid"></param>
        /// <returns></returns>
        public static GameSpellEffect GetRelicEffect(GamePlayer player, int spellid, bool minoreli)
        {
            Spell spell = DBSpellToSpell(SkillBase.GetSpellByID(spellid));

            if (spell == null) return null;

            ISpellHandler handler = ScriptMgr.CreateSpellHandler(player, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));

            int duration = spell.Duration / 1000;
            return new GameSpellEffect(handler, duration, 0, minoreli);
        }

        #region Spellstuff
        /// <summary>
        /// Converts a Spell to a new Spell and sets a new Target
        /// </summary>
        /// <param name="spell"></param>
        /// <param name="relic"></param>
        /// <returns></returns>
        public static Spell DBSpellToSpell(Spell spell)
        {
            if (spell == null) return null;

            DBSpell dbspell = new DBSpell();

            dbspell.AllowBolt = spell.AllowBolt;
            dbspell.AmnesiaChance = spell.AmnesiaChance;
            dbspell.CastTime = spell.CastTime;
            dbspell.ClientEffect = spell.ClientEffect;
            dbspell.Concentration = spell.Concentration;
            dbspell.Damage = spell.Damage;
            dbspell.DamageType = (int)spell.DamageType;
            dbspell.Description = spell.Description;
            dbspell.Duration = spell.Duration;
            dbspell.EffectGroup = spell.EffectGroup;
            dbspell.Frequency = spell.Frequency;
            dbspell.HealthPenalty = spell.HealthPenalty;
            dbspell.Icon = spell.Icon;
            dbspell.InstrumentRequirement = spell.InstrumentRequirement;
            dbspell.IsPrimary = spell.IsPrimary;
            dbspell.IsSecondary = spell.IsSecondary;
            dbspell.LifeDrainReturn = spell.LifeDrainReturn;
            dbspell.Message1 = spell.Message1;
            dbspell.Message2 = spell.Message2;
            dbspell.Message3 = spell.Message3;
            dbspell.Message4 = spell.Message4;
            dbspell.MoveCast = spell.MoveCast;
            dbspell.Name = spell.Name;
            dbspell.Power = spell.Power;
            dbspell.Pulse = spell.Pulse;
            dbspell.PulsePower = spell.PulsePower;
            dbspell.Radius = spell.Radius;
            dbspell.Range = spell.Range;
            dbspell.RecastDelay = spell.RecastDelay;
            dbspell.ResurrectHealth = spell.ResurrectHealth;
            dbspell.ResurrectMana = spell.ResurrectMana;
            dbspell.SharedTimerGroup = spell.SharedTimerGroup;
            dbspell.SpellGroup = spell.Group;
            dbspell.SpellID = spell.ID;
            dbspell.SubSpellID = spell.SubSpellID;
            dbspell.Target = spell.Target;
            dbspell.Type = spell.SpellType;
            dbspell.Uninterruptible = spell.Uninterruptible;
            dbspell.Value = spell.Value;

            return new Spell(dbspell, 1, true);
        }
        #endregion
    }
}
