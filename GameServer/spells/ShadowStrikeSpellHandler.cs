using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Styles;
using DOL.Language;
using System;

namespace DOL.GS.Spells
{
    [SpellHandler("ShadowStrike")]
    public class ShadowStrikeSpellHandler : SpellHandler
    {

        public override void FinishSpellCast(GameLiving target)
        {
            base.FinishSpellCast(target);

            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(EventManager));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(EventManager));

            // Hide the player
            ((GamePlayer)Caster).Stealth(!Caster.IsStealthed);

            // teleport the player
            int xrange = 0;
            int yrange = 0;
            double angle = 0.00153248422;
            m_caster.MoveTo(target.CurrentRegionID, (int)(target.X - ((xrange + 10) * Math.Sin(angle * target.Heading))), (int)(target.Y + ((yrange + 10) * Math.Cos(angle * target.Heading))), target.Z, m_caster.Heading);

            // use style
            Style style = new Style(GameServer.Database.SelectObjects<DBStyle>("`StyleID` = @StyleID", new QueryParameter("@StyleID", 968))[0]);
            StyleProcessor.TryToUseStyle(Caster, style);

        }

        public override bool CastSpell(GameLiving target)
        {
            if (!base.CastSpell(target))
                return false;

            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(EventManager));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(EventManager));
            GameEventMgr.AddHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(EventManager));
            GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(EventManager));
            return true;
        }

        private void EventManager(DOLEvent e, object sender, EventArgs args)
        {
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(EventManager));
            GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(EventManager));

            if (e == GameLivingEvent.Moving)
                MessageToCaster(LanguageMgr.GetTranslation(((GamePlayer)Caster).Client.Account.Language, "ShadowStrikeAbility.Moving"), eChatType.CT_Important);
            if (e == GameLivingEvent.AttackedByEnemy)
                MessageToCaster(LanguageMgr.GetTranslation(((GamePlayer)Caster).Client.Account.Language, "ShadowStrikeAbility.Attacked"), eChatType.CT_Important);
            InterruptCasting();
        }

        public ShadowStrikeSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}