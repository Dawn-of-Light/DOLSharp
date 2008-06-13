using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.Spells
{
    #region Battlemaster-1
    [SpellHandlerAttribute("MLEndudrain")]
    public class MLEndudrain : MasterlevelHandling
    {
        public MLEndudrain(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }


        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= CalculateNeededPower(target);
            base.FinishSpellCast(target);
        }


        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null) return;
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

            int end = (int)(Spell.Damage);
            target.ChangeEndurance(target, GameLiving.eEnduranceChangeType.Spell, (-end));

            if (target is GamePlayer)
            {
                ((GamePlayer)target).Out.SendMessage(m_caster.Name + " steal you for " + end + " endurance!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
            }

            target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
        }
    }
    #endregion

    #region Battlemaster-2
    [SpellHandlerAttribute("KeepDamageBuff")]
    public class KeepDamageBuff : MasterlevelBuffHandling
    {
        public override eProperty Property1 { get { return eProperty.KeepDamage; } }

        public KeepDamageBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    #region Battlemaster-3
    [SpellHandlerAttribute("MLManadrain")]
    public class MLManadrain : MasterlevelHandling
    {
        public MLManadrain(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= CalculateNeededPower(target);
            base.FinishSpellCast(target);
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null) return;
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

            int mana = (int)(Spell.Damage);
            target.ChangeMana(target, GameLiving.eManaChangeType.Spell, (-mana));

            target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
        }
    }
    #endregion

    #region Battlemaster-4
    [SpellHandlerAttribute("Grapple")]
    public class Grapple : MasterlevelHandling
    {
        private int check = 0;
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (selectedTarget is GameNPC == true)
            {
                MessageToCaster("This spell works only on realm enemys.", eChatType.CT_SpellResisted);
                return false;
            }
            return base.CheckBeginCast(selectedTarget);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            if (effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;
                if (player.EffectList.GetOfType(typeof(ChargeEffect)) == null && player != null)
                {
                    effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, effect, 0);
                    player.Client.Out.SendUpdateMaxSpeed();
                    check = 1;
                }
                effect.Owner.StopAttack();
                effect.Owner.StopCurrentSpellcast();
                effect.Owner.IsDisarmed = true;
            }
            base.OnEffectStart(effect);
        }

        protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
        {
            return Spell.Duration;
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override bool IsOverwritable(GameSpellEffect compare)
        {
            return false;
        }

        public override void FinishSpellCast(GameLiving target)
        {
            if (m_spell.SubSpellID > 0)
            {
                Spell spell = SkillBase.GetSpellByID(m_spell.SubSpellID);
                if (spell != null && spell.SubSpellID == 0)
                {
                    ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(m_caster, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
                    spellhandler.StartSpell(Caster);
                }
            }
            base.FinishSpellCast(target);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (effect.Owner == null) return 0;

            base.OnEffectExpires(effect, noMessages);

            GamePlayer player = effect.Owner as GamePlayer;

            if (check > 0 && player != null)
            {
                effect.Owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, effect);
                player.Client.Out.SendUpdateMaxSpeed();
            }

            effect.Owner.IsDisarmed = false;
            return 0;
        }

        public Grapple(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion
	
	#region Battlemaster-6
	//Eden-Darwin
	[SpellHandlerAttribute("ThrowWeapon")]
	public class ThrowWeaponSpellHandler : DoomHammerSpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			GamePlayer player=Caster as GamePlayer;
			if(player==null) return false;
			InventoryItem weapon=null;
			if(player.ActiveWeaponSlot.ToString()=="TwoHanded") weapon=player.Inventory.GetItem((eInventorySlot)12);
			if(player.ActiveWeaponSlot.ToString()=="Standard") weapon=player.Inventory.GetItem((eInventorySlot)10);
			if(weapon==null) { MessageToCaster("Equip a weapon before using this spell!",eChatType.CT_SpellResisted); return false; }
			return base.CheckBeginCast(selectedTarget);
		}
		
		public override int CalculateSpellResistChance(GameLiving target) { return 0; }
		
		public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
		{		
			GamePlayer player=Caster as GamePlayer;
			if(player==null) return null;
			InventoryItem weapon=null;
			if(player.ActiveWeaponSlot.ToString()=="TwoHanded") weapon=player.Inventory.GetItem((eInventorySlot)12);
			if(player.ActiveWeaponSlot.ToString()=="Standard") weapon=player.Inventory.GetItem((eInventorySlot)10);
			if(weapon==null) return null;
			AttackData ad = new AttackData();
			ad.Attacker = player;
			ad.Target = target;
			ad.Damage = 0;
			ad.CriticalDamage = 0;
			ad.WeaponSpeed = player.AttackSpeed(weapon) / 100;
			ad.DamageType = player.AttackDamageType(weapon);
			ad.Weapon = weapon;
			ad.IsOffHand = weapon.Hand == 2;
			switch (weapon.Item_Type)
			{
				default:
				case Slot.RIGHTHAND:
				case Slot.LEFTHAND: ad.AttackType = AttackData.eAttackType.MeleeOneHand; break;
				case Slot.TWOHAND: ad.AttackType = AttackData.eAttackType.MeleeTwoHand; break;
			}
			double damage = player.AttackDamage(weapon) * effectiveness;
			InventoryItem armor = null;
			if (ad.Target.Inventory != null) armor = ad.Target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);
			int lowerboundary = (player.WeaponSpecLevel(weapon) - 1) * 50 / (ad.Target.EffectiveLevel + 1) + 75;
			lowerboundary = Math.Max(lowerboundary, 75);
			lowerboundary = Math.Min(lowerboundary, 125);
			damage *= (player.GetWeaponSkill(weapon) + 90.68) / (ad.Target.GetArmorAF(ad.ArmorHitLocation) + 20 * 4.67);
			if (ad.Attacker.EffectList.GetOfType(typeof(BadgeOfValorEffect)) != null) damage *= 1.0 + Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
			else damage *= 1.0 - Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
			damage *= (lowerboundary + Util.Random(50)) * 0.01;
			ad.Modifier = (int)(damage * (ad.Target.GetResist(ad.DamageType) + SkillBase.GetArmorResist(armor, ad.DamageType)) * -0.01);
			damage += ad.Modifier;
			int resist = (int)(damage * ad.Target.GetDamageResist(target.GetResistTypeForDamage(ad.DamageType)) * -0.01);
			eProperty property = ad.Target.GetResistTypeForDamage(ad.DamageType);
            int secondaryResistModifier = ad.Target.SpecBuffBonusCategory[(int)property];
			int resistModifier = 0;
			resistModifier += (int)((ad.Damage + (double)resistModifier) * (double)secondaryResistModifier * -0.01);
			damage += resist;
			damage += resistModifier;
			ad.Modifier += resist;
			ad.Damage = (int)damage;
			ad.UncappedDamage = ad.Damage;
			ad.Damage = Math.Min(ad.Damage, (int)(player.UnstyledDamageCap(weapon) * effectiveness));
			ad.Damage = (int)((double)ad.Damage * ServerProperties.Properties.PVP_DAMAGE);
			if (ad.Damage == 0) ad.AttackResult = DOL.GS.GameLiving.eAttackResult.Missed;	
			ad.CriticalDamage = player.CalculateCriticalDamage(ad, weapon);
			return ad;			
		}
		
		public ThrowWeaponSpellHandler(GameLiving caster,Spell spell,SpellLine line) : base(caster,spell,line) {}
	}
	#endregion

    #region Battlemaster-8
    [SpellHandlerAttribute("BodyguardHandler")]
    public class BodyguardHandler : SpellHandler
    {
        public override IList DelveInfo
        {
            get
            {
                ArrayList list = new ArrayList();
                list.Add(Spell.Description);
                return list;
            }
        }
        public BodyguardHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    #region Stylhandler
    [SpellHandlerAttribute("StyleHandler")]
    public class StyleHandler : MasterlevelHandling
    {
        public StyleHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion
}

#region KeepDamageCalc

namespace DOL.GS.PropertyCalc
{
    /// <summary>
    /// The melee damage bonus percent calculator
    ///
    /// BuffBonusCategory1 is used for buffs
    /// BuffBonusCategory2 unused
    /// BuffBonusCategory3 is used for debuff
    /// BuffBonusCategory4 unused
    /// BuffBonusMultCategory1 unused
    /// </summary>
    [PropertyCalculator(eProperty.KeepDamage)]
    public class KeepDamagePercentCalculator : PropertyCalculator
    {
        public override int CalcValue(GameLiving living, eProperty property)
        {
            int percent = 100
                +living.BaseBuffBonusCategory[(int)property];

            return percent;
        }
    }
}

#endregion