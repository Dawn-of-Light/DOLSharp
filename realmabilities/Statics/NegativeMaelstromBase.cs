using System;
using System.Collections;
using DOL.Database;
using DOL.GS;
using DOL.GS.Spells;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities.Statics
{
	public class NegativeMaelstromBase : GenericBase 
    {
		protected override string GetStaticName() {return "Negative Maelstrom";}
		protected override ushort GetStaticModel() {return 1293;}
		protected override ushort GetStaticEffect() {return 7027;}
		private DBSpell dbs;
		private Spell   s;
		private SpellLine sl;
		int damage;		
		public NegativeMaelstromBase(int damage) 
        {
			this.damage = damage;
			dbs = new DBSpell();
			dbs.Name = GetStaticName();
			dbs.Icon = GetStaticEffect();
			dbs.ClientEffect = GetStaticEffect();
			dbs.Damage = damage;
			dbs.DamageType = (int)eDamageType.Cold;
			dbs.Target = "Enemy";
			dbs.Radius = 0;
            dbs.Type = "DirectDamageNoVariance";
			dbs.Value =0;
			dbs.Duration = 0;
			dbs.Pulse = 0;
			dbs.PulsePower = 0;
			dbs.Power = 0;
			dbs.CastTime = 0;
			dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
			sl = new SpellLine("RAs","RealmAbilitys","RealmAbilitys",true);
		}
		protected override void CastSpell (GameLiving target)
        {
            if (!target.IsAlive) return;
			if (GameServer.ServerRules.IsAllowedToAttack(m_caster, target, true))
            {
				int dealDamage =damage;
				if (getCurrentPulse()<=6)
					dealDamage = (int)Math.Round(((double)getCurrentPulse()/6*damage));
				dbs.Damage = dealDamage;				
				s = new Spell(dbs,1);
				ISpellHandler dd = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
				dd.StartSpell(target);
			}
		}
	}
}
