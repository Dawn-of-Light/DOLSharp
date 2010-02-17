//Andraste v2.0 - Vico

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Collections;
using DOL;
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using log4net;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("RealmLore")]
	public class RealmLore : SpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
        {
			if(!base.CheckBeginCast(selectedTarget)) 
				return false;

			if(selectedTarget==null) 
				return false;

			if (selectedTarget is GameNPC)
			{
				MessageToCaster("This spell works only on players.", eChatType.CT_SpellResisted); return false;
			}

			if(selectedTarget as GamePlayer==null) 
				return false;

			if(!m_caster.IsWithinRadius(selectedTarget, Spell.Range))
			{
				MessageToCaster("Your target is too far away.", eChatType.CT_SpellResisted); return false;
			}

            return true;
        }
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			GamePlayer player = target as GamePlayer;
			if(player == null) 
				return;

			var text = new List<string>();
			text.Add("Class: "+player.CharacterClass.Name);
			text.Add("Realmpoints: "+player.RealmPoints+" = "+string.Format("{0:#L#} {1}",player.RealmLevel+10,player.RealmTitle));
			text.Add("----------------------------------------------------");
			text.Add("Str: "+player.Strength+" Dex: "+player.Dexterity+" Con: "+player.Constitution);
			text.Add("Qui: "+player.Quickness+" Emp: "+player.Empathy+" Cha: "+player.Charisma);
			text.Add("Pie: "+player.Piety+" Int: "+player.Intelligence+" HP: "+player.MaxHealth);
			text.Add("----------------------------------------------------");
			IList specs = player.GetSpecList();
			foreach (object obj in specs)
				if (obj is Specialization)
					text.Add(((Specialization)obj).Name + ": " + ((Specialization)obj).Level.ToString());
			text.Add("----------------------------------------------------");
			IList abilities = player.GetAllAbilities();
			foreach(Ability ab in abilities)
				if(ab is RealmAbility && ab is RR5RealmAbility == false)
					text.Add(((RealmAbility)ab).Name);

			(m_caster as GamePlayer).Out.SendCustomTextWindow("Realm Lore [ "+player.Name+" ]",text);
			(m_caster as GamePlayer).Out.SendMessage("Realm Lore [ "+player.Name+" ]\n"+text,eChatType.CT_System,eChatLoc.CL_SystemWindow);
		}
		public RealmLore(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
    }	
}