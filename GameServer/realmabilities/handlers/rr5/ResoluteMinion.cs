//Andraste v2.0 - Vico

using System;
using System.Reflection;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

//using log4net;

namespace DOL.GS.RealmAbilities
{
	public class ResoluteMinionAbility : RR5RealmAbility
    {
		public const int DURATION = 60000;
		public ResoluteMinionAbility(DBAbility dba, int level) : base(dba, level) { }
        public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			GamePlayer player = living as GamePlayer;
			if (player == null) return;
			if (player.ControlledNpcBrain == null) return;
			if (player.ControlledNpcBrain.Body == null) return;
			player.ControlledNpcBrain.Body.AddAbility(SkillBase.GetAbility(Abilities.CCImmunity));
			new ResoluteMinionEffect().Start(player.ControlledNpcBrain.Body);
			foreach (GamePlayer visPlayer in player.GetPlayersInRadius((ushort)WorldMgr.VISIBILITY_DISTANCE))
				visPlayer.Out.SendSpellEffectAnimation(player, player.ControlledNpcBrain.Body, 7047, 0, false, 0x01);
			DisableSkill(living);
        }
		public override int GetReUseDelay(int level) { return 300; }
    }
}

namespace DOL.GS.Effects
{
	public class ResoluteMinionEffect : TimedEffect
	{
		public ResoluteMinionEffect() : base(RealmAbilities.ResoluteMinionAbility.DURATION) { }
		private GameNPC m_pet;
		public void Start(GameNPC controllednpc) { base.Start(controllednpc); m_pet = controllednpc; }
		public override void Stop()
		{
			if (m_pet != null)
			{
				if (m_pet.EffectList.GetOfType(typeof(ResoluteMinionEffect)) != null) m_pet.EffectList.Remove(this);
				if (m_pet.HasAbility(Abilities.CCImmunity)) m_pet.RemoveAbility("CCImmunity");
			}
			base.Stop();
		}
		public override ushort Icon { get { return 7047; } }
	}
}
