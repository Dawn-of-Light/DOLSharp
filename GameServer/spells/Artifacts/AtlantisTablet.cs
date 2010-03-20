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
                foreach (GameSpellEffect Effect in player.EffectList.GetAllOfType(typeof(GameSpellEffect)))
                {
                    if (Effect.SpellHandler.Spell.SpellType.Equals("ShadesOfMist") ||
                        Effect.SpellHandler.Spell.SpellType.Equals("TraitorsDaggerProc") ||
                        Effect.SpellHandler.Spell.SpellType.Equals("DreamMorph") ||
                        Effect.SpellHandler.Spell.SpellType.Equals("DreamGroupMorph") ||
                        Effect.SpellHandler.Spell.SpellType.Equals("MaddeningScalars") ||
                        Effect.SpellHandler.Spell.SpellType.Equals("AlvarusMorph"))
                    {
                        player.Out.SendMessage("You already have a activate morph!", DOL.GS.PacketHandler.eChatType.CT_SpellResisted, DOL.GS.PacketHandler.eChatLoc.CL_ChatWindow);
                        return;
                    }
                }
				if(player.CharacterClass.ID!=(byte)eCharacterClass.Necromancer && (ushort)Spell.LifeDrainReturn > 0) 
                    player.Model = (ushort)Spell.LifeDrainReturn;
				player.Out.SendUpdatePlayer();
			}
		}

		public override int OnEffectExpires(GameSpellEffect effect,bool noMessages)
		{
			if(effect.Owner is GamePlayer)
			{
				GamePlayer player=effect.Owner as GamePlayer; 				
				if(player.CharacterClass.ID!=(byte)eCharacterClass.Necromancer) player.Model = player.CreationModel;
				player.Out.SendUpdatePlayer();
			}	
			return base.OnEffectExpires(effect,noMessages);
		}

		public AtlantisTabletMorph(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}
