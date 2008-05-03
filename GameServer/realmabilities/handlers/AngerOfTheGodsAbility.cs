using System.Reflection;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Database2;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities
{
	public class AngerOfTheGodsAbility : TimedRealmAbility
	{
        private DBSpell dbs;
        private Spell s;
        private SpellLine sl;
        private int damage = 0;
        private GamePlayer player;
        public AngerOfTheGodsAbility(DBAbility dba, int level) : base(dba, level) {}
        public virtual void NewSpell(int damage)
        {
            dbs = new DBSpell();
            dbs.Name = "Anger of the Gods";
            dbs.Icon = 7023;
            dbs.ClientEffect = 7023;
            dbs.Damage = damage;
            dbs.DamageType = (int)eDamageType.Matter;
            dbs.Target = "Group";
            dbs.Radius = 0;
            dbs.Type = "DamageAdd";
            dbs.Value = 0;
            dbs.Duration = 30;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.Power = 0;
            dbs.CastTime = 0;
			dbs.EffectGroup = 10;
			dbs.SpellGroup = 10;
            dbs.Range = 1000;
            s = new Spell(dbs, 1);
            sl = new SpellLine("RAs", "RealmAbilitys", "RealmAbilitys", true);
        }	

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			player = living as GamePlayer;
			switch (Level)
			{
				case 1: damage = 10; break;
				case 2: damage = 20; break;
				case 3: damage = 30; break;
				default: return;
			}
			NewSpell(damage);
			CastSpell(player);
			DisableSkill(living);
		}
        protected void CastSpell(GameLiving target)
        {
            if (!target.IsAlive) return;
            s = new Spell(dbs, 1);
            ISpellHandler dd = ScriptMgr.CreateSpellHandler(player, s, sl);
            dd.StartSpell(target);
        }	
        public override int GetReUseDelay(int level)
        {
            return 600;
        }
	}
}
