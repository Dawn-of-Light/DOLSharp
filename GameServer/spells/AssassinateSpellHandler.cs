using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Styles;
using DOL.Language;
using System;

namespace DOL.GS.Spells
{
    [SpellHandler("Assassinate")]
    public class AssassinateHandler : SpellHandler
    {

        public override bool CastSpell(GameLiving target)
        {
            if (!base.CastSpell(target))
                return false;

            GamePlayer player = Caster as GamePlayer;
            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AssassinateAbility.Preparing"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            player.Out.SendTimerWindow("Assassinate", Spell.CastTime/1000);
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(EventManager));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(EventManager));
            GameEventMgr.AddHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(EventManager));
            GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(EventManager));
            return true;
        }

        public override void InterruptCasting()
        {
            base.InterruptCasting();
            ((GamePlayer)Caster).Out.SendCloseTimerWindow();
        }

        private void EventManager(DOLEvent e, object sender, EventArgs args)
        {
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(EventManager));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(EventManager));
            if (e == GameLivingEvent.Moving)
                MessageToCaster(LanguageMgr.GetTranslation(((GamePlayer)Caster).Client.Account.Language, "AssassinateAbility.Moving"), eChatType.CT_Important);
            if (e == GameLivingEvent.AttackedByEnemy)
                MessageToCaster(LanguageMgr.GetTranslation(((GamePlayer)Caster).Client.Account.Language, "AssassinateAbility.Attacked"), eChatType.CT_Important);
            InterruptCasting();
        }

        public override void FinishSpellCast(GameLiving target)
        {
            base.FinishSpellCast(target);

            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(EventManager));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(EventManager));

            GamePlayer player = Caster as GamePlayer;

            player.Out.SendCloseTimerWindow();

            // Hide the player
            player.Stealth(!Caster.IsStealthed);

            // teleport the player
            int xrange = 0;
            int yrange = 0;
            double angle = 0.00153248422;
            m_caster.MoveTo(target.CurrentRegionID, (int)(target.X - ((xrange + 10) * Math.Sin(angle * target.Heading))), (int)(target.Y + ((yrange + 10) * Math.Cos(angle * target.Heading))), target.Z, m_caster.Heading);

            // stay stealth for the next attack
            player.StayStealth = true;
        }

        public AssassinateHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}