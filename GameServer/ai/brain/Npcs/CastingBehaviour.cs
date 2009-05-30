using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS;

namespace DOL.AI.Brain
{
    public class CastingBehaviour : IAttackBehaviour
    {
        public CastingBehaviour(GameNPC body)
        {
            Body = body;
        }

        protected GameNPC Body { get; set; }

        protected SpellLine SpellLine
        {
            get
            {
                return SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);
            }
        }

        #region IAttackBehaviour Members

        public bool Attack(GameObject target)
        {
            foreach (Spell spell in Body.HarmfulSpells)
            {
                if (target is GameLiving && GameLiving.HasEffect(target as GameLiving, spell))
                    continue;

                Body.TargetObject = target;
                Body.TurnTo(target);
                Body.CastSpell(spell, SpellLine);
                return true;
            }

            return false;
        }

        public void Retreat()
        {
        }

        #endregion
    }
}
