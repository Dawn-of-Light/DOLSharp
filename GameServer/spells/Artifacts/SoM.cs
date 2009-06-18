//Andraste v2.0 - Vico

using System;
using System.Text;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using System.Collections;
using DOL.Database;
using DOL.GS.Scripts;

namespace DOL.GS.Spells
{
    [SpellHandler("Som")]
    public class ShadeOfMist : DefensiveProcSpellHandler
    {
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if(effect.Owner is GamePlayer)
            {
	            GamePlayer player = effect.Owner as GamePlayer;
	   			player.Shade(true);
	   			player.Out.SendUpdatePlayer();
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
 			if(effect.Owner is GamePlayer)
            {
	            GamePlayer player = effect.Owner as GamePlayer; 				
				player.Shade(false);
				player.Out.SendUpdatePlayer();
    		}	          
            return base.OnEffectExpires(effect, noMessages);
        }
        /// <summary>
        /// Constructs a new UnbreakableSpeedDecreaseSpellHandler
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="spell"></param>
        /// <param name="line"></param>
        public ShadeOfMist(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
        }
    }
	
	[SpellHandlerAttribute("SomArmorAbsorbtionBuff")]
	public class SomArmorAbsorbtionBuff : SingleStatBuff
	{
		public override eProperty Property1 { get { return eProperty.ArmorAbsorbtion; } }

		/// <summary>
		/// send updates about the changes
		/// </summary>
		/// <param name="target"></param>
		protected override void SendUpdates(GameLiving target)
		{
		}

		// constructor
		public SomArmorAbsorbtionBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
