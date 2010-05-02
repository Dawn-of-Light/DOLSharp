using System;
using System.Collections;
using System.Collections.Generic;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;
using DOL.Events;

namespace DOL.GS.Spells
{
    //http://www.camelotherald.com/masterlevels/ma.php?ml=Battlemaster
    #region Battlemaster-1
    [SpellHandlerAttribute("MLEndudrain")]
    public class MLEndudrain : MasterlevelHandling
    {
        public MLEndudrain(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }


        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }


        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null) return;
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;
            //spell damage should 25;
            int end = (int)(Spell.Damage);
            target.ChangeEndurance(target, GameLiving.eEnduranceChangeType.Spell, (-end));

            if (target is GamePlayer)
                ((GamePlayer)target).Out.SendMessage(" You lose " + end + " endurance!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
            (m_caster as GamePlayer).Out.SendMessage("" + target.Name + " loses " + end + " endurance!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);

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
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null) return;
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

            //spell damage shood be 50-100 (thats the amount power tapped on use) i recommend 90 i think thats it but cood be wrong
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
                effect.Owner.DisarmedTime = effect.Owner.CurrentRegion.Time + Spell.Duration;
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

            //effect.Owner.IsDisarmed = false;
            return 0;
        }

        public Grapple(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    //ml5 in database Target shood be Group if PvP..Realm if RvR..Value = spell proc'd (a.k the 80value dd proc)
    #region Battlemaster-5
    [SpellHandler("EssenceFlamesProc")]
    public class EssenceFlamesProcSpellHandler : OffensiveProcSpellHandler
    {
        /// <summary>
        /// Handler fired whenever effect target is attacked
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected override void EventHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;
            if (args == null || args.AttackData == null)
            {
                return;
            }
            AttackData ad = args.AttackData;
            if (ad.AttackResult != GameLiving.eAttackResult.HitUnstyled && ad.AttackResult != GameLiving.eAttackResult.HitStyle)
                return;

            int baseChance = Spell.Frequency / 100;

            if (ad.IsMeleeAttack)
            {
                if (sender is GamePlayer)
                {
                    GamePlayer player = (GamePlayer)sender;
                    InventoryItem leftWeapon = player.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                    // if we can use left weapon, we have currently a weapon in left hand and we still have endurance,
                    // we can assume that we are using the two weapons.
                    if (player.CanUseLefthandedWeapon && leftWeapon != null && leftWeapon.Object_Type != (int)eObjectType.Shield)
                    {
                        baseChance /= 2;
                    }
                }
            }

            if (baseChance < 1)
                baseChance = 1;

            if (Util.Chance(baseChance))
            {
                ISpellHandler handler = ScriptMgr.CreateSpellHandler((GameLiving)sender, m_procSpell, m_procSpellLine);
                if (handler != null)
                {
                    if (m_procSpell.Target.ToLower() == "enemy")
                        handler.StartSpell(ad.Target);
                    else if (m_procSpell.Target.ToLower() == "self")
                        handler.StartSpell(ad.Attacker);
                    else if (m_procSpell.Target.ToLower() == "group")
                    {
                        GamePlayer player = Caster as GamePlayer;
                        if (Caster is GamePlayer)
                        {
                            if (player.Group != null)
                            {
                                foreach (GameLiving groupPlayer in player.Group.GetMembersInTheGroup())
                                {
                                    if (player.IsWithinRadius(groupPlayer, m_procSpell.Range))
                                    {
                                        handler.StartSpell(groupPlayer);
                                    }
                                }
                            }
                            else
                                handler.StartSpell(player);
                        }
                    }
                    else
                    {
                        log.Warn("Skipping " + m_procSpell.Target + " proc " + m_procSpell.Name + " on " + ad.Target.Name + "; Realm = " + ad.Target.Realm);
                    }
                }
            }
        }

        // constructor
        public EssenceFlamesProcSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

	#region Battlemaster-6
	// LifeFlight
    [SpellHandler("ThrowWeapon")]
    public class ThrowWeaponSpellHandler : DirectDamageSpellHandler
 	{
        #region Disarm Weapon
        protected static Spell Disarm_Weapon;
        public static Spell Disarmed
        {
            get
            {
                if (Disarm_Weapon == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.Uninterruptible = true;
                    spell.Icon = 7293;
                    spell.ClientEffect = 7293;
                    spell.Description = "Disarms the caster.";
                    spell.Name = "Throw Weapon(Disarm)";
                    spell.Range = 0;
                    spell.Value = 0;
                    spell.Duration = 10;
                    spell.SpellID = 900100;
                    spell.Target = "Self";
                    spell.Type = "Disarm";
                    Disarm_Weapon = new Spell(spell, 50);
                    SkillBase.GetSpellList(GlobalSpellsLines.Combat_Styles_Effect).Add(Disarm_Weapon);
                }
                return Disarm_Weapon;
            }
        }
        #endregion
        public const string DISABLE = "ThrowWeapon.Shortened.Disable.Timer";
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			GamePlayer player = Caster as GamePlayer;
			if(player == null) 
                return false;

            if (player.IsDisarmed)
            {
                MessageToCaster("You are disarmed and can't use this spell!", eChatType.CT_YouHit);
                return false;
            }

			InventoryItem weapon = null;

            //assign the weapon the player is using, it can be a twohanded or a standard slot weapon
			if (player.ActiveWeaponSlot.ToString() == "TwoHanded") 
                weapon = player.Inventory.GetItem((eInventorySlot)12);
			if (player.ActiveWeaponSlot.ToString() == "Standard")
                weapon = player.Inventory.GetItem((eInventorySlot)10);
            
            //if the weapon is null, ie. they don't have an appropriate weapon active
			if(weapon == null) 
            { 
                MessageToCaster("Equip a weapon before using this spell!",eChatType.CT_SpellResisted); 
                return false; 
            }

            return base.CheckBeginCast(selectedTarget);
		}
		
        //Throw Weapon does not "resist"
		public override int CalculateSpellResistChance(GameLiving target) 
        { 
            return 0; 
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            return base.OnEffectExpires(effect, noMessages);
        }

        
        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null) return;

            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

            // calc damage
            AttackData ad = CalculateDamageToTarget(target, effectiveness);
            DamageTarget(ad, true);
            SendDamageMessages(ad);
            target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, ad.AttackType, Caster);
        }
        
        
        public override void DamageTarget(AttackData ad, bool showEffectAnimation)
        {
            InventoryItem weapon = null;
            weapon = ad.Weapon;

            if (showEffectAnimation && ad.Target != null)
            {
                byte resultByte = 0;
                int attackersWeapon = (weapon == null) ? 0 : weapon.Model;
                int defendersWeapon = 0;

                switch (ad.AttackResult)
                {
                    case GameLiving.eAttackResult.Missed: resultByte = 0; break;
                    case GameLiving.eAttackResult.Evaded: resultByte = 3; break;
                    case GameLiving.eAttackResult.Fumbled: resultByte = 4; break;
                    case GameLiving.eAttackResult.HitUnstyled: resultByte = 10; break;
                    case GameLiving.eAttackResult.HitStyle: resultByte = 11; break;
                    case GameLiving.eAttackResult.Parried:
                        resultByte = 1;
                        if (ad.Target != null && ad.Target.AttackWeapon != null)
                        {
                            defendersWeapon = ad.Target.AttackWeapon.Model;
                        }
                        break;
                    case GameLiving.eAttackResult.Blocked:
                        resultByte = 2;
                        if (ad.Target != null && ad.Target.Inventory != null)
                        {
                            InventoryItem lefthand = ad.Target.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                            if (lefthand != null && lefthand.Object_Type == (int)eObjectType.Shield)
                            {
                                defendersWeapon = lefthand.Model;
                            }
                        }
                        break;
                }

                foreach (GamePlayer player in ad.Target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (player == null) continue;
                    int animationId;
                    switch (ad.AnimationId)
                    {
                        case -1:
                            animationId = player.Out.OneDualWeaponHit;
                            break;
                        case -2:
                            animationId = player.Out.BothDualWeaponHit;
                            break;
                        default:
                            animationId = ad.AnimationId;
                            break;
                    }
                    //We don't need to send the animiation for the throwning, thats been done earlier.

                    //this is for the defender, which should show the appropriate animation
                    player.Out.SendCombatAnimation(null, ad.Target, (ushort)attackersWeapon, (ushort)defendersWeapon, animationId, 0, resultByte, ad.Target.HealthPercent);
                
                }
            }

            // send animation before dealing damage else dead livings show no animation
            ad.Target.OnAttackedByEnemy(ad);
            ad.Attacker.DealDamage(ad);
            if (ad.Damage == 0 && ad.Target is GameNPC)
            {
                IOldAggressiveBrain aggroBrain = ((GameNPC)ad.Target).Brain as IOldAggressiveBrain;
                if (aggroBrain != null)
                    aggroBrain.AddToAggroList(Caster, 1);
            }
            
        }

        public override void SendDamageMessages(AttackData ad)
        {
			GameObject target = ad.Target;
			InventoryItem weapon = ad.Weapon;
            GamePlayer player = Caster as GamePlayer;

			switch (ad.AttackResult)
			{
				case GameLiving.eAttackResult.TargetNotVisible: player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.NotInView", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                case GameLiving.eAttackResult.OutOfRange: player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.TooFarAway", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                case GameLiving.eAttackResult.TargetDead: player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.AlreadyDead", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                case GameLiving.eAttackResult.Blocked: player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.Blocked", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                case GameLiving.eAttackResult.Parried: player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.Parried", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                case GameLiving.eAttackResult.Evaded: player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.Evaded", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                case GameLiving.eAttackResult.NoTarget: player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.NeedTarget"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                case GameLiving.eAttackResult.NoValidTarget: player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.CantBeAttacked"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                case GameLiving.eAttackResult.Missed: player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.Miss"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                case GameLiving.eAttackResult.Fumbled: player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.Fumble"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
                case GameLiving.eAttackResult.HitStyle:
                case GameLiving.eAttackResult.HitUnstyled:
					string modmessage = "";
					if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier + ")";
					if (ad.Modifier < 0) modmessage = " (" + ad.Modifier + ")";

					string hitWeapon = "";

					switch (ServerProperties.Properties.SERV_LANGUAGE)
					{
						case "EN":
							if (weapon != null)
								hitWeapon = GlobalConstants.NameToShortName(weapon.Name);
							break;
						case "DE":
							if (weapon != null)
								hitWeapon = weapon.Name;
							break;
						default:
							if (weapon != null)
								hitWeapon = GlobalConstants.NameToShortName(weapon.Name);
							break;
					}

					if (hitWeapon.Length > 0)
						hitWeapon = " " + LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.WithYour") + " " + hitWeapon;

					string attackTypeMsg = LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.YouAttack");
 
					// intercept messages
					if (target != null && target != ad.Target)
					{
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.Intercepted", ad.Target.GetName(0, true), target.GetName(0, false)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.InterceptedHit", attackTypeMsg, target.GetName(0, false), hitWeapon, ad.Target.GetName(0, false), ad.Damage, modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					}
					else
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.InterceptHit", attackTypeMsg, ad.Target.GetName(0, false), hitWeapon, ad.Damage, modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

					// critical hit
					if (ad.CriticalDamage > 0)
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Attack.Critical", ad.Target.GetName(0, false), ad.CriticalDamage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					break;
			}
        }

        public override void FinishSpellCast(GameLiving target)
        {
            base.FinishSpellCast(target);

            //we need to make sure the spell is only disabled if the attack was a success
            int isDisabled = Caster.TempProperties.getProperty<int>(DISABLE);
            
            //if this value is greater than 0 then we know that their weapon did not damage the target
            //the skill's disable timer should be set to their attackspeed 
            if (isDisabled > 0)
            {
                Caster.DisableSkill(Spell, isDisabled);
                
                //remove the temp property
                Caster.TempProperties.removeProperty(DISABLE);
            }
            else
            {
                //they disarm them selves.
                Caster.CastSpell(Disarmed, (SkillBase.GetSpellLine(GlobalSpellsLines.Combat_Styles_Effect)));
            }
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GamePlayer player = target as GamePlayer;
          
            foreach (GamePlayer visPlayer in Caster.GetPlayersInRadius((ushort)WorldMgr.VISIBILITY_DISTANCE))
            {
                visPlayer.Out.SendCombatAnimation(Caster, target, 0x0000, 0x0000, (ushort)408, 0, 0x00, target.HealthPercent);
            }
            
            OnDirectEffect(target, effectiveness);

        }

        public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
        {
            GamePlayer player = Caster as GamePlayer;

            if (player == null)
                return null;

            InventoryItem weapon = null;

            if (player.ActiveWeaponSlot.ToString() == "TwoHanded")
                weapon = player.Inventory.GetItem((eInventorySlot)12);
            if (player.ActiveWeaponSlot.ToString() == "Standard")
                weapon = player.Inventory.GetItem((eInventorySlot)10);

            if (weapon == null)
                return null;

            //create the AttackData
            AttackData ad = new AttackData();
            ad.Attacker = player;
            ad.Target = target;
            ad.Damage = 0;
            ad.CriticalDamage = 0;
            ad.WeaponSpeed = player.AttackSpeed(weapon) / 100;
            ad.DamageType = player.AttackDamageType(weapon);
            ad.Weapon = weapon;
            ad.IsOffHand = weapon.Hand == 2;
            //we need to figure out which armor piece they are going to hit.
            //figure out the attacktype
            switch (weapon.Item_Type)
            {
                default:
                case Slot.RIGHTHAND:
                case Slot.LEFTHAND:
                    ad.AttackType = AttackData.eAttackType.MeleeOneHand;
                    break;
                case Slot.TWOHAND:
                    ad.AttackType = AttackData.eAttackType.MeleeTwoHand;
                    break;
            }
            //Throw Weapon is subject to all the conventional attack results, parry, evade, block, etc.
            ad.AttackResult = ad.Target.CalculateEnemyAttackResult(ad, weapon);

            if (ad.AttackResult == GameLiving.eAttackResult.HitUnstyled || ad.AttackResult == GameLiving.eAttackResult.HitStyle)
            {
                //we only need to calculate the damage if the attack was a success.
                double damage = player.AttackDamage(weapon) * effectiveness;

                if (target is GamePlayer)
                    ad.ArmorHitLocation = ((GamePlayer)target).CalculateArmorHitLocation(ad);

                InventoryItem armor = null;
                if (target.Inventory != null)
                    armor = target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

                //calculate the lowerboundary of the damage
                int lowerboundary = (player.WeaponSpecLevel(weapon) - 1) * 50 / (ad.Target.EffectiveLevel + 1) + 75;
                lowerboundary = Math.Max(lowerboundary, 75);
                lowerboundary = Math.Min(lowerboundary, 125);

                damage *= (player.GetWeaponSkill(weapon) + 90.68) / (ad.Target.GetArmorAF(ad.ArmorHitLocation) + 20 * 4.67);

                //If they have badge of Valor, we need to modify the damage
                if (ad.Attacker.EffectList.GetOfType(typeof(BadgeOfValorEffect)) != null)
                    damage *= 1.0 + Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
                else
                    damage *= 1.0 - Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));

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
                ad.CriticalDamage = player.GetMeleeCriticalDamage(ad, weapon);
            }
            else
            {
                //They failed, do they do not get disarmed, and the spell is not disabled for the full duration,
                //just the modified swing speed, this is in milliseconds
                int attackSpeed = player.AttackSpeed(weapon);
                player.TempProperties.setProperty(DISABLE, attackSpeed);
            }
            return ad;
        }
		public ThrowWeaponSpellHandler(GameLiving caster,Spell spell,SpellLine line) : base(caster,spell,line) {}
	}
	#endregion

    //essence debuff
    #region Battlemaster-7
    [SpellHandlerAttribute("EssenceSearHandler")]
    public class EssenceSearHandler : SpellHandler
    {
        public override int CalculateSpellResistChance(GameLiving target) { return 0; }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            GameLiving living = effect.Owner as GameLiving;
            //value should be 15 to reduce Essence resist
            living.DebuffCategory[(int)eProperty.Resist_Natural] += (int)m_spell.Value;

            if (effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;
                player.Out.SendCharStatsUpdate();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
                player.Out.SendUpdatePlayer();
            }
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameLiving living = effect.Owner as GameLiving;
            living.DebuffCategory[(int)eProperty.Resist_Natural] -= (int)m_spell.Value;

            if (effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                player.Out.SendUpdatePlayer();
            }
            return base.OnEffectExpires(effect, noMessages);
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, effectiveness);
            if (target.Realm == 0 || Caster.Realm == 0)
            {
                target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
                Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
            }
            else
            {
                target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
                Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
            }
            if (target is GameNPC)
            {
                IOldAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IOldAggressiveBrain;
                if (aggroBrain != null)
                    aggroBrain.AddToAggroList(Caster, (int)Spell.Value);
            }
        }
        public EssenceSearHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    #region Battlemaster-8
    [SpellHandlerAttribute("BodyguardHandler")]
    public class BodyguardHandler : SpellHandler
    {
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
        //    if (Caster.Group.MemberCount <= 2)
        //    {
        //        MessageToCaster("Your group is to small to use this spell.", eChatType.CT_Important);
        //        return false;
        //    }
              return base.CheckBeginCast(selectedTarget);
        
        }
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>();
                list.Add(Spell.Description);
                return list;
            }
        }
        public BodyguardHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    //for ML9 in the database u have to add  EssenceDampenHandler  in type (its a new method customly made) 
    #region Battlemaster-9
    [SpellHandlerAttribute("EssenceDampenHandler")]
    public class EssenceDampenHandler : SpellHandler
    {
        protected int DexDebuff = 0;
        protected int QuiDebuff = 0;
        public override int CalculateSpellResistChance(GameLiving target) { return 0; }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            double percentValue = (m_spell.Value) / 100;//15 / 100 = 0.15 a.k (15%) 100dex * 0.15 = 15dex debuff 
            DexDebuff = (int)((double)effect.Owner.GetModified(eProperty.Dexterity) * percentValue);
            QuiDebuff = (int)((double)effect.Owner.GetModified(eProperty.Quickness) * percentValue);
            GameLiving living = effect.Owner as GameLiving;
            living.DebuffCategory[(int)eProperty.Dexterity] += DexDebuff;
            living.DebuffCategory[(int)eProperty.Quickness] += QuiDebuff;

            if (effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                player.Out.SendUpdatePlayer();
            }
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameLiving living = effect.Owner as GameLiving;
            living.DebuffCategory[(int)eProperty.Dexterity] -= DexDebuff;
            living.DebuffCategory[(int)eProperty.Quickness] -= QuiDebuff;

            if (effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                player.Out.SendUpdatePlayer();
            }
            return base.OnEffectExpires(effect, noMessages);
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, effectiveness);
            if (target.Realm == 0 || Caster.Realm == 0)
            {
                target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
                Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
            }
            else
            {
                target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
                Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
            }
            if (target is GameNPC)
            {
                IOldAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IOldAggressiveBrain;
                if (aggroBrain != null)
                    aggroBrain.AddToAggroList(Caster, (int)Spell.Value);
            }
        }
        public EssenceDampenHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    //ml10 in database Type shood be RandomBuffShear

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