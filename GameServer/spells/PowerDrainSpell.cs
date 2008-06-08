using System;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
 
	/// <summary>
	/// Style combat speed debuff effect spell handler
	/// </summary>
	[SpellHandler("PowerDrainSpell")]
	public class PowerDrainSpell : SpellHandler
	{
		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

        public override int CalculateNeededPower(GameLiving target)
        {
            return 0;
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {

            if (target == null) return;
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

            // drain calc
            int power = (int)this.Spell.Damage; ;
            int power_factor  = 1; //TODO: is any factor here ?

            if (target is GamePlayer)
            {
                if (target.Mana == 0)
                {
                    ((GamePlayer)this.Caster).Out.SendMessage("You did not receive any power from " + target.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    power = 0;
                }
                else
                {
                    if (target.Mana < power) power = target.Mana;
                    ((GamePlayer)target).Out.SendMessage(this.Caster.Name + " takes " + power + " power!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
                    target.Mana -= power;
                }
            }

            if (this.Caster.Mana >= this.Caster.MaxMana)
            {
                this.Caster.Mana = this.Caster.MaxMana;
                ((GamePlayer)this.Caster).Out.SendMessage("You can't receive more power!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }
            else if (power != 0)
            {
                power /= power_factor;
                this.Caster.Mana += power;
                ((GamePlayer)this.Caster).Out.SendMessage("You receive " + power + " power from " + target.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }

            return;
        }

		// constructor
        public PowerDrainSpell(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}