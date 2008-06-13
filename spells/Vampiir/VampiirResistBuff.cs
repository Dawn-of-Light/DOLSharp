using System;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("VampiirMeleeResistance")]
	public class VampiirMeleeResistance : SpellHandler
	{
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			GameLiving living = effect.Owner as GameLiving;
            GamePlayer player = effect.Owner as GamePlayer;
            int value = (int)Spell.Value;
            living.BaseBuffBonusCategory[(int)eProperty.Resist_Slash] += value;
            living.BaseBuffBonusCategory[(int)eProperty.Resist_Crush] += value;
            living.BaseBuffBonusCategory[(int)eProperty.Resist_Thrust] += value;
            if (player != null)
            {
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                player.Out.SendCharResistsUpdate();
            }
			MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);
		}

		
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameLiving living = effect.Owner as GameLiving;
            GamePlayer player = effect.Owner as GamePlayer;
			int value = (int)Spell.Value;
            living.BaseBuffBonusCategory[(int)eProperty.Resist_Slash] -= value;
            living.BaseBuffBonusCategory[(int)eProperty.Resist_Crush] -= value;
            living.BaseBuffBonusCategory[(int)eProperty.Resist_Thrust] -= value;
            if (player != null)
            {
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                player.Out.SendCharResistsUpdate();
            }
			MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_Spell);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);
			return 0;
		}

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			int specLevel = 0;
			if (Caster is GamePlayer)
				specLevel = ((GamePlayer)Caster).GetModifiedSpecLevel(m_spellLine.Spec);
			effectiveness = 0.75 + (specLevel-1) * 0.5 / Spell.Level;
			effectiveness = Math.Max(0.75, effectiveness);
			effectiveness = Math.Min(1.25, effectiveness);
			base.ApplyEffectOnTarget(target, effectiveness);
        }

		public override IList DelveInfo 
		{
			get 
			{
				ArrayList list = new ArrayList();
				list.Add("Name: " + Spell.Name);
				list.Add("Description: " + Spell.Description);
				list.Add("Target: " + Spell.Target);
				list.Add("Casting time: " + (Spell.CastTime*0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				if (Spell.Duration >= ushort.MaxValue*1000)
					list.Add("Duration: Permanent.");
				else if (Spell.Duration > 60000)
					list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration/60000, (Spell.Duration%60000/1000).ToString("00")));
				else if (Spell.Duration != 0)
					list.Add("Duration: " + (Spell.Duration/1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if (Spell.RecastDelay > 60000)
					list.Add("Recast time: " + (Spell.RecastDelay/60000).ToString() + ":" + (Spell.RecastDelay%60000/1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
					list.Add("Recast time: " + (Spell.RecastDelay/1000).ToString() + " sec");
				if(Spell.Range != 0) list.Add("Range: " + Spell.Range);
				if(Spell.Radius != 0) list.Add("Radius: " + Spell.Radius);
				if(Spell.Power != 0) list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				list.Add("All Melee Resist Increased: 0" /*+ Spell.Value*/);
				if (Spell.Frequency != 0)
					list.Add("Frequency: " + (Spell.Frequency*0.001).ToString("0.0"));				
				return list;
			}
		}

		public VampiirMeleeResistance(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}

	[SpellHandlerAttribute("VampiirMagicResistance")]
	public class VampiirMagicResistance : SpellHandler
	{
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{

			base.OnEffectStart(effect);
			GamePlayer player = effect.Owner as GamePlayer;
            GameLiving living = effect.Owner as GameLiving;
			
			BedazzlingAuraEffect boad = (BedazzlingAuraEffect)player.EffectList.GetOfType(typeof(BedazzlingAuraEffect));
			if (boad != null) boad.Cancel(false);
			
            int value = (int)Spell.Value;
            living.AbilityBonus[(int)eProperty.Resist_Body] += value;
            living.AbilityBonus[(int)eProperty.Resist_Cold] += value;
            living.AbilityBonus[(int)eProperty.Resist_Energy] += value;
            living.AbilityBonus[(int)eProperty.Resist_Heat] += value;
            living.AbilityBonus[(int)eProperty.Resist_Matter] += value;
            living.AbilityBonus[(int)eProperty.Resist_Spirit] += value;
            if(player != null)
            {
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                player.Out.SendCharResistsUpdate();
            }
			MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
            GameLiving living = effect.Owner as GameLiving;
            int value = (int)Spell.Value;
            living.AbilityBonus[(int)eProperty.Resist_Body] -= value;
            living.AbilityBonus[(int)eProperty.Resist_Cold] -= value;
            living.AbilityBonus[(int)eProperty.Resist_Energy] -= value;
            living.AbilityBonus[(int)eProperty.Resist_Heat] -= value;
            living.AbilityBonus[(int)eProperty.Resist_Matter] -= value;
            living.AbilityBonus[(int)eProperty.Resist_Spirit] -= value;
            GamePlayer player = living as GamePlayer;
            if(player != null)
            {
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                player.Out.SendCharResistsUpdate();
            }
			MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_Spell);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);
			return 0;
		}

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			int specLevel = 0;
			if (Caster is GamePlayer)
				specLevel = ((GamePlayer)Caster).GetModifiedSpecLevel(m_spellLine.Spec);
			effectiveness = 0.75 + (specLevel-1) * 0.5 / Spell.Level;
			effectiveness = Math.Max(0.75, effectiveness);
			effectiveness = Math.Min(1.25, effectiveness);
			base.ApplyEffectOnTarget(target, effectiveness);
		}

		public override IList DelveInfo 
		{
			get 
			{
				ArrayList list = new ArrayList();
				list.Add("Name: " + Spell.Name);
				list.Add("Description: " + Spell.Description);
				list.Add("Target: " + Spell.Target);
				list.Add("Casting time: " + (Spell.CastTime*0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				if (Spell.Duration >= ushort.MaxValue*1000)
					list.Add("Duration: Permanent.");
				else if (Spell.Duration > 60000)
					list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration/60000, (Spell.Duration%60000/1000).ToString("00")));
				else if (Spell.Duration != 0)
					list.Add("Duration: " + (Spell.Duration/1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if (Spell.RecastDelay > 60000)
					list.Add("Recast time: " + (Spell.RecastDelay/60000).ToString() + ":" + (Spell.RecastDelay%60000/1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
					list.Add("Recast time: " + (Spell.RecastDelay/1000).ToString() + " sec");
				if(Spell.Range != 0) list.Add("Range: " + Spell.Range);
				if(Spell.Radius != 0) list.Add("Radius: " + Spell.Radius);
				if(Spell.Power != 0) list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				list.Add("All Magic Resist Increased: " + Spell.Value);
				if (Spell.Frequency != 0)
					list.Add("Frequency: " + (Spell.Frequency*0.001).ToString("0.0"));
				return list;
			}
		}

		public VampiirMagicResistance(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}