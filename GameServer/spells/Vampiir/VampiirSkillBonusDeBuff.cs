using System;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("VampiirSkillBonusDeBuff")]
	public class VampiirSkillBonusDeBuff : SpellHandler
	{
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}


        public override void OnEffectStart(GameSpellEffect effect)
        {

            base.OnEffectStart(effect);
            if (effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;

                for (int i = (int)eProperty.Skill_First; i <= (int)eProperty.Skill_Last; i++)
                {
                    if (player.GetModifiedSpecLevel(SkillBase.GetPropertyName((eProperty)(i))) != 0)
                    {
                        player.BuffBonusCategory1[i] = -player.GetModifiedSpecLevel(SkillBase.GetPropertyName((eProperty)(i)));
                    }
                    //					DOLConsole.WriteWarning("Spec " + SkillBase.GetPropertyName((eProperty)(i)) + " " + player.GetModifiedSpecLevel(SkillBase.GetPropertyName((eProperty)(i))));
                }
                player.PropertiesChanged();
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);

            }

        }

		
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if (effect.Owner is GamePlayer)
			{
				GamePlayer player = effect.Owner as GamePlayer;
				for (int i=(int)eProperty.Skill_First; i<=(int)eProperty.Skill_Last; i++) 
				{
					player.BuffBonusCategory1[i] = 0;
				}
				player.PropertiesChanged();
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_Spell);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);
			
			}
			return 0;
		}


		public override IList DelveInfo 
		{
			get 
			{
				ArrayList list = new ArrayList();
				//Name
				list.Add("Name: " + Spell.Name);
				//Description
				list.Add("Description: " + Spell.Description);
				//Target
				list.Add("Target: " + Spell.Target);
				//Cast
				list.Add("Casting time: " + (Spell.CastTime*0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				//Duration
				if (Spell.Duration >= ushort.MaxValue*1000)
					list.Add("Duration: Permanent.");
				else if (Spell.Duration > 60000)
					list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration/60000, (Spell.Duration%60000/1000).ToString("00")));
				else if (Spell.Duration != 0)
					list.Add("Duration: " + (Spell.Duration/1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				//Recast
				if (Spell.RecastDelay > 60000)
					list.Add("Recast time: " + (Spell.RecastDelay/60000).ToString() + ":" + (Spell.RecastDelay%60000/1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
					list.Add("Recast time: " + (Spell.RecastDelay/1000).ToString() + " sec");
				//Range
				if(Spell.Range != 0) list.Add("Range: " + Spell.Range);
				//Radius
				if(Spell.Radius != 0) list.Add("Radius: " + Spell.Radius);
				//Cost
				if(Spell.Power != 0) list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				//Effect

				if (Spell.Frequency != 0)
					list.Add("Frequency: " + (Spell.Frequency*0.001).ToString("0.0"));
								
				return list;
			}
		}
		
		
		public VampiirSkillBonusDeBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}