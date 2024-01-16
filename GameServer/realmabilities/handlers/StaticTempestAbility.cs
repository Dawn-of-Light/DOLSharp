using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	public class StaticTempestAbility : TimedRealmAbility
	{
        public StaticTempestAbility(DBAbility dba, int level) : base(dba, level) { }

		private int m_stunDuration;
		private uint m_duration;
		private GamePlayer m_player;
        public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			GamePlayer caster = living as GamePlayer;
            if (caster.TargetObject == null)
            {
                caster.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (!(caster.TargetObject is GameLiving)
                || !GameServer.ServerRules.IsAllowedToAttack(caster, (GameLiving)caster.TargetObject, true))
            {
                caster.Out.SendMessage("You cannot attack " + caster.TargetObject.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            if (!caster.TargetInView)
            {
                caster.Out.SendMessage("You cannot see " + caster.TargetObject.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            if (!caster.IsWithinRadius( caster.TargetObject, 1500 ))
            {
                caster.Out.SendMessage("You target is too far away to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            this.m_player = caster;
            
            if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
	            switch (Level)
	            {
	                case 1: m_duration = 15; break;
	                case 2: m_duration = 20; break;
	                case 3: m_duration = 25; break;
	                case 4: m_duration = 30; break;
	                case 5: m_duration = 35; break;
	                default: return;
	            }
            }
            else
            {
	            switch (Level)
	            {
	                case 1: m_duration = 10; break;
	                case 2: m_duration = 15; break;
	                case 3: m_duration = 30; break;
	                default: return;
	            }
            }
            m_stunDuration = 3;
            foreach (GamePlayer i_player in caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
            {
				if (i_player == caster)
				{
					i_player.MessageToSelf("You cast " + this.Name + "!", eChatType.CT_Spell);
				}
				else
				{
					i_player.MessageFromArea(caster, caster.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
			}
            Statics.StaticTempestBase st = new Statics.StaticTempestBase(m_stunDuration);
            st.CreateStatic(caster, caster.TargetObject.Coordinate, m_duration, 5, 360);
            DisableSkill(living);
        }
        public override int GetReUseDelay(int level)
        {
            return 600;
        }
	}
}
