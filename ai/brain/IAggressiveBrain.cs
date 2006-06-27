using System.Collections;
using DOL.GS;

namespace DOL.AI.Brain
{
	public interface IAggressiveBrain
	{
		/// <summary>
		/// Aggressive Level in % 0..100, 0 means not Aggressive
		/// </summary>
		int AggroLevel { get; set; }

		/// <summary>
		/// Range in that this npc aggros
		/// </summary>
		int AggroRange { get; set; }

		/// <summary>
		/// Add living to the aggrolist
		/// aggroamount can be negative to lower amount of aggro		
		/// </summary>
		/// <param name="living"></param>
		/// <param name="aggroamount"></param>
		void AddToAggroList(GameLiving living, int aggroamount);

		/// <summary>
		/// Get current amount of aggro on aggrotable
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		long GetAggroAmountForLiving(GameLiving living);

		/// <summary>
		/// Remove one living from aggro list
		/// </summary>
		/// <param name="living"></param>
		void RemoveFromAggroList(GameLiving living);

		/// <summary>
		/// Remove all livings from the aggrolist
		/// </summary>
		void ClearAggroList();

		/// <summary>
		/// Makes a copy of current aggro list
		/// </summary>
		/// <returns></returns>
		Hashtable CloneAggroList();

		/// <summary>
		/// calculate the aggro of this npc against another living
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		int CalculateAggroLevelToTarget(GameLiving target);
	}
}