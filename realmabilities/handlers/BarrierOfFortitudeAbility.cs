using System.Reflection;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.RealmAbilities 
{
    public class BarrierOfFortitudeAbility : TimedRealmAbility 
    {
        public BarrierOfFortitudeAbility(DBAbility dba, int level) : base(dba, level) { }
        
        public const string BofBaSb = "RA_DAMAGE_DECREASE";

        int m_range = 1500;
        int m_duration = 30;
        int m_reuseTimer = 600;
        int m_value = 0;

        public void Execute(GameLiving living)
        {
            GamePlayer player = living as GamePlayer;
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            if (player.TempProperties.getProperty(BofBaSb, false))
            {
                player.Out.SendMessage("You already an effect of that type!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }
            switch (Level) 
            {
                case 1: m_value = 10; break;
                case 2: m_value = 20; break;
                case 3: m_value = 40; break;
                default: return;
            }


            DisableSkill(living);
            ArrayList targets = new ArrayList();
			if (player.PlayerGroup == null)
				targets.Add(player);
			else
			{
				foreach (GamePlayer grpplayer in player.PlayerGroup.GetPlayersInTheGroup())
				{
					if (WorldMgr.CheckDistance(grpplayer, player, m_range) && grpplayer.IsAlive)
						targets.Add(grpplayer);
				}
			}
            bool success;
            foreach (GamePlayer target in targets) 
            {
                //send spelleffect
                if (!target.IsAlive) continue;
                success = !target.TempProperties.getProperty(BofBaSb, false);
                foreach (GamePlayer visPlayer in WorldMgr.GetPlayersCloseToObject(target, WorldMgr.VISIBILITY_DISTANCE))
                    visPlayer.Out.SendSpellEffectAnimation(player, target, 7016, 0, false, CastSuccess(success));
				if (success)
				{
					if (target != null)
						new BarrierOfFortitudeEffect().Start(target, m_duration, m_value);
				}
            }
            
        }
        private byte CastSuccess(bool success) 
        {
            if (success)
                return 1;
            else
                return 0;
        }
    }
}
