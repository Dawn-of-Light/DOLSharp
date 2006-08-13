using DOL.GS;
using DOL.GS.Keeps;

namespace DOL.AI.Brain
{
	/// <summary>
	/// The Brain for the Area Lord
	/// </summary>
	public class LordBrain : KeepGuardBrain
	{
		/// <summary>
		/// The Brain Think Method
		/// </summary>
		public override void Think()
		{
			base.Think();
			CheckHealing();
		}

		/// <summary>
		/// Check if the lord can heal himself
		/// </summary>
		private void CheckHealing()
		{
			//if we are already casting a spell return
			if (Body.CurrentSpellHandler != null)
				return;

			//if the lord is in combat, there is only a chance to heal
			if (Body.InCombat)
			{
				if (Body.HealthPercent < 25 && Util.Chance(25))
					SpellMgr.LordCastHealSpell(guard);
			}
			//if the lord is not in combat, the lord can heal himself fully
			else
			{
				if (Body.HealthPercent != 100)
					SpellMgr.LordCastHealSpell(guard);
			}
		}
	}
}
