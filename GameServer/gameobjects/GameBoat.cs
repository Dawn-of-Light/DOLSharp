using DOL.AI.Brain;
using DOL.Database;

namespace DOL.GS
{
	public class GameBoat : GameMovingObject
	{
		/*
		 * TODO:
		 * 
		 * Boat Commands
		 * Command	Description
		 * /setdest	Commands the vehicle you're driving to go to your ground target. (Also works with Seige Equipment)
		 * /vboard	Board targetted boat
		 * /vboot	Boot someone from your boat
		 * /vdestination [x] [y]	Will sail your boat to a specified location (x, y) within your current zone
		 * /vfollow	Allows the controller of a movable vehicle to follow their currently targeted object
		 * /vforward	Move your boat forward
		 * /vinvite	Invite someone onto your boat
		 * /vset [public/private]	Sets your boat to either Public or Private
		 * 
		 * different speeds for different populations on boats (personal / toa will always move full speed)
		 * - For each person under the total capacity that is not on the boat, movement speed is decreased approximately 3%. If a boat has less than the minimum requirement, the boat cannot be moved.
		 * - Boats can only be built away from enemy controlled keeps,
		 * 
		 **/
		private byte m_type = 0;

		private RegionTimer m_removeTimer = null;

		public GameBoat(byte type)
			: base()
		{
			m_type = type;
		}

		public GameBoat()
			: base()
		{ }

		public override void LoadFromDatabase(DataObject obj)
		{
			switch ((obj as Mob).Model)
			{
				case 2648: m_type = 0; break;
				case 2646: m_type = 1; break;
				case 2647: m_type = 2; break;
			}

			base.LoadFromDatabase(obj);
		}

		public override bool AddToWorld()
		{
			Name = "Boat";
			Model = 2648;
			BlankBrain brain = new BlankBrain();
			SetOwnBrain(brain);
			MaxSpeedBase = 316;
			return base.AddToWorld();
		}

		public override ushort Model
		{
			get
			{
				switch (m_type)
				{
					case 0: return 2648;
					case 1: return 2646;
					case 2: return 2647;
					default: return 2648;
				}
			}
		}

		public override string Name
		{
			get
			{
				switch (m_type)
				{
					case 0: return "scout boat";
					case 1: return "galleon";
					case 2: return "warship";
					default: return "boat";
				}
			}
		}

		public override int MAX_PASSENGERS
		{
			get
			{
				switch (m_type)
				{
					case 0: return 8;
					case 1: return 16;
					case 2: return 32;
					default: return 2;
				}
			}
		}

		public override int REQUIRED_PASSENGERS
		{
			get
			{
				switch (m_type)
				{
					case 0: return 1;
					case 1: return 2;
					case 2: return 4;
					default: return 1;
				}
			}
		}

		public override int SLOT_OFFSET
		{
			get
			{
				return 1;
			}
		}

		public override ushort Type()
		{
			//changes for realm
			return 18;
		}

		public override bool RiderMount(GamePlayer rider, bool forced)
		{
			if (!base.RiderMount(rider, forced))
				return false;

			if (m_removeTimer != null && m_removeTimer.IsAlive)
				m_removeTimer.Stop();

			return true;
		}

		public override bool RiderDismount(bool forced, GamePlayer player)
		{
			if (!base.RiderDismount(forced, player))
				return false;

			if (CurrentRiders.Length == 0)
			{
				if (m_removeTimer == null)
					m_removeTimer = new RegionTimer(this, new RegionTimerCallback(RemoveCallback));
				else if (m_removeTimer.IsAlive)
					m_removeTimer.Stop();
				m_removeTimer.Start(15 * 60 * 1000);
			}

			return true;
		}

		protected int RemoveCallback(RegionTimer timer)
		{
			m_removeTimer.Stop();
			m_removeTimer = null;
			Delete();
			return 0;
		}
	}
}