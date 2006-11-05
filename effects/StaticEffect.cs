using System;
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Effects
{
	/// <summary>
	/// 
	/// </summary>
	public class StaticEffect : IGameEffect
	{
		private ushort m_id;
		protected GameLiving m_owner = null;

		/// <summary>
		/// create static effect
		/// </summary>
		public StaticEffect()
		{
		}

		/// <summary>
		/// Cancel effect
		/// </summary>
		/// <param name="playerCanceled"></param>
		public virtual void Cancel(bool playerCanceled) {
			if (playerCanceled && HasNegativeEffect) {
				if (Owner is GamePlayer)
					((GamePlayer)Owner).Out.SendMessage("You can't remove this effect!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			Stop();
		}

		/// <summary>
		/// Start the effect on target
		/// </summary>
		/// <param name="target">The effect target</param>
		public virtual void Start(GameLiving target)
		{
			lock (this)
			{
				m_owner = target;
				target.EffectList.Add(this);
			}
		}

		/// <summary>
		/// Stop the effect on owner
		/// </summary>
		public virtual void Stop()
		{
			lock (this)
			{
				if (m_owner != null)
				{
					m_owner.EffectList.Remove(this);
				}
			}
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public virtual string Name { get { return "Noname"; } }

		/// <summary>
		/// Remaining Time of the effect in milliseconds
		/// </summary>
		public virtual int RemainingTime {
			get {
				return 0; // unlimited
			}
		}

		public GameLiving Owner {
			get { return m_owner; }
		}

		public virtual ushort Icon { 
			get { return 0; } }

		/// <summary>
		/// unique id for identification in effect list
		/// </summary>
		public ushort InternalID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		public virtual bool HasNegativeEffect {
			get { return false; }
		}

		public virtual IList DelveInfo {
			get {
				return new ArrayList(0);
			}
		}
	}
}