using System;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("AtlantisTabletMorph")]
	public class AtlantisTabletMorph : OffensiveProcSpellHandler
	{   	
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			if(effect.Owner is GamePlayer)
			{
				GamePlayer player=effect.Owner as GamePlayer;
				if(player.CharacterClass.ID!=(byte)eCharacterClass.Necromancer) player.Model = (ushort)Spell.LifeDrainReturn;
				player.Out.SendUpdatePlayer();
			}
		}

		public override int OnEffectExpires(GameSpellEffect effect,bool noMessages)
		{
			if(effect.Owner is GamePlayer)
			{
				GamePlayer player=effect.Owner as GamePlayer; 				
				if(player.CharacterClass.ID!=(byte)eCharacterClass.Necromancer) player.Model=(ushort)player.Client.Account.Characters[player.Client.ActiveCharIndex].CreationModel;
				player.Out.SendUpdatePlayer();
			}	
			return base.OnEffectExpires(effect,noMessages);
		}

		public AtlantisTabletMorph(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}
