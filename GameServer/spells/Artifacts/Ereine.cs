using System;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
    [SpellHandler("Ereine")]
    public class Ereine : AllStatsBuff
    {
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if (effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;
                GameEventMgr.AddHandler(player, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;
                Spell subspell = SkillBase.GetSpellByID(Spell.ResurrectMana);
                if (subspell != null)
                {
                    subspell.Level = Spell.Level;
                    ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(Caster, subspell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
                    spellhandler?.StartSpell(Caster);
                }

                GameEventMgr.RemoveHandler(player, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            }

            return base.OnEffectExpires(effect, noMessages);
        }

        private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GameLiving))
            {
                return;
            }

            AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
            AttackData ad = null;
            if (attackedByEnemy != null)
            {
                ad = attackedByEnemy.AttackData;
            }

            if ((int)ad.DamageType >= 1 && (int)ad.DamageType <= 3)
            {
                int absorb = (int)Math.Round(ad.Damage * Spell.Value / 100);
                int critical = (int)Math.Round(ad.CriticalDamage * Spell.Value / 100);
                ad.Damage -= absorb;
                ad.CriticalDamage -= critical;
                (ad.Attacker as GamePlayer)?.Out.SendMessage($"Your target\'s Ereine charge absorb {absorb + critical} damages", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

                (ad.Target as GamePlayer)?.Out.SendMessage($"Your Ereine charge absorb {absorb + critical} damages", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
            }
        }

        public Ereine(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    [SpellHandler("Ereine2")]
    public class Ereine2 : AllStatsDebuff
    {
        public override bool HasPositiveEffect => false;

        public override bool IsUnPurgeAble => true;

        public Ereine2(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    [SpellHandler("EreineProc")]
    public class EreineProc : SpellHandler
    {
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if (effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;
                GameEventMgr.AddHandler(player, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;
                GameEventMgr.RemoveHandler(player, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            }

            return base.OnEffectExpires(effect, noMessages);
        }

        private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GameLiving living))
            {
                return;
            }

            AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
            AttackData ad = null;
            if (attackedByEnemy != null)
            {
                ad = attackedByEnemy.AttackData;
            }

            if ((int)ad.DamageType >= 10 && (int)ad.DamageType <= 15)
            {
                (ad.Attacker as GamePlayer)?.Out.SendMessage($"Your target\' Ereine Proc absorb {ad.Damage + ad.CriticalDamage} damages", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

                (ad.Target as GamePlayer)?.Out.SendMessage($"Your Ereine Proc absorb {ad.Damage + ad.CriticalDamage} damages", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

                ad.Damage = 0; ad.CriticalDamage = 0;
                GameSpellEffect effect = FindEffectOnTarget(living, this);
                effect?.Cancel(false);
            }
        }

        public EreineProc(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}