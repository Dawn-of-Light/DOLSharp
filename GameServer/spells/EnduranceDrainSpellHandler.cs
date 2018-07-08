using System.Collections.Generic;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    [SpellHandler("EnduranceDrain")]
    public class EnduranceDrainSpellHandler : SpellHandler
    {
        public EnduranceDrainSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null)
            {
                return;
            }

            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            int end = (int)Spell.Damage;
            target.ChangeEndurance(target,GameLiving.eEnduranceChangeType.Spell, -end);

            if (target is GamePlayer player)
            {
                player.Out.SendMessage($"{Caster.Name} steal you for {end} endurance!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
            }

            StealEndurance(target,end);
            target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
        }

        public virtual void StealEndurance(GameLiving target,int end)
        {
            if (!Caster.IsAlive)
            {
                return;
            }

            Caster.ChangeEndurance(target, GameLiving.eEnduranceChangeType.Spell, end);
            SendCasterMessage(target,end);
        }

        public virtual void SendCasterMessage(GameLiving target,int end)
        {
            MessageToCaster($"You steal {target.Name} for {end} endurance!", eChatType.CT_YouHit);
            if (end > 0)
            {
                MessageToCaster($"You steal {end} endurance point{(end == 1 ? "." : "s.")}", eChatType.CT_Spell);
            }
            else
            {
                MessageToCaster("You cannot absorb any more endurance.", eChatType.CT_SpellResisted);
            }
        }

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    $"Name: {Spell.Name}",
                    $"Description: {Spell.Description}",
                    $"Target: {Spell.Target}",
                    $"Casting time: {Spell.CastTime * 0.001:0.0## sec;-0.0## sec;'instant'}"
                };

                if (Spell.Duration >= ushort.MaxValue * 1000)
                {
                    list.Add("Duration: Permanent.");
                }
                else if (Spell.Duration > 60000)
                {
                    list.Add($"Duration: {Spell.Duration / 60000}:{Spell.Duration % 60000 / 1000:00} min");
                }
                else if (Spell.Duration != 0)
                {
                    list.Add($"Duration: {Spell.Duration / 1000:0' sec';'Permanent.';'Permanent.'}");
                }

                // Recast
                if (Spell.RecastDelay > 60000)
                {
                    list.Add($"Recast time: {(Spell.RecastDelay / 60000)}:{Spell.RecastDelay % 60000 / 1000:00} min");
                }
                else if (Spell.RecastDelay > 0)
                {
                    list.Add($"Recast time: {Spell.RecastDelay / 1000} sec");
                }

                // Range
                if (Spell.Range != 0)
                {
                    list.Add($"Range: {Spell.Range}");
                }

                // Radius
                if (Spell.Radius != 0)
                {
                    list.Add($"Radius: {Spell.Radius}");
                }

                // Cost
                if (Spell.Power != 0)
                {
                    list.Add($"Power cost: {Spell.Power:0;0'%'}");
                }

                // Effect
                list.Add($"Drain Endurance By: {Spell.Damage}%");

                if (Spell.Frequency != 0)
                {
                    list.Add($"Frequency: {Spell.Frequency * 0.001:0.0}");
                }

                return list;
            }
        }
    }
}























