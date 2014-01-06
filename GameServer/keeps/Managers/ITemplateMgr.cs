using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOL.GS.Keeps
{
	public interface ITemplateMgr
	{
		void RefreshTemplate(GameKeepGuard guard);
		void SetGuardLevel(GameKeepGuard guard);
		void SetGuardBrain(GameKeepGuard guard);
		void SetGuardSpeed(GameKeepGuard guard);
	}
}
