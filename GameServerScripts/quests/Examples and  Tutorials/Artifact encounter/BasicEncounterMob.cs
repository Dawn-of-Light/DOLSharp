using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using log4net;
using System.Reflection;
using DOL.GS.Atlantis;
using DOL.Database;
using DOL.Language;
using DOL.GS.Spells;

namespace DOL.GS.Atlantis
{
    /// <summary>
    /// The base class that most or all ArtifactEncounter mob's should inherit from.
    /// </summary>
    public class BasicEncounterMob : GameNPC
    {
    	public override void SaveIntoDatabase() {}

        public virtual void CastSpellnoLOSchecks(Spell spell, SpellLine line)
        {
            if ((m_runningSpellHandler != null && spell.CastTime > 0))
            {
                Notify(GameLivingEvent.CastFailed, this, new CastFailedEventArgs(null, CastFailedEventArgs.Reasons.AlreadyCasting));
                return;
            }
            ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(this, spell, line);
            if (spellhandler != null)
            {
                m_runningSpellHandler = spellhandler;
                spellhandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
                spellhandler.CastSpell();
            }
            else
            {
                return;
            }
        }
    }
}

