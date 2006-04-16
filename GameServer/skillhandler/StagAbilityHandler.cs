using System;
using System.Collections;
using System.Reflection;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using log4net;

namespace DOL.GS.SkillHandler
{
	/// <summary>
	/// Handler for Stag Ability clicks
	/// </summary>
    [SkillHandler(Abilities.Stag)]
    public class StagAbilityHandler : IAbilityActionHandler
    {
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The ability reuse time in milliseconds
		/// </summary>
		protected const int REUSE_TIMER = 60000 * 30; // 30 minutes

		/// <summary>
		/// The ability effect duration in milliseconds
		/// </summary>
		public const int DURATION = 30 * 1000; // 30 seconds

		public void Execute(Ability ab, GamePlayer player)
		{
			if (player == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not retrieve player in StagAbilityHandler.");
				return;
			}

			//Cancel old stag effects on player
			StagEffect stag = (StagEffect) player.EffectList.GetOfType(typeof(StagEffect));
			if (stag!=null)
			{
				player.Out.SendMessage("That ability is already active, wait until it expires.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			player.DisableSkill(ab, REUSE_TIMER);

			new StagEffect().Start(player, ab);
		}                       
    }
}
