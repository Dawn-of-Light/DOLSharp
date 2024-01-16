using System;
using System.Reflection;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Wrapper for the currently active pathing mgr
	/// </summary>
	public static class PathingMgr
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// PathingMgr that does nothing
		/// </summary>
		public static readonly IPathingMgr NullPathingMgr = new NullPathingMgr();

		/// <summary>
		/// Calculates data locally and performs calls here
		/// </summary>
		public static readonly LocalPathingMgr LocalPathingMgr = new LocalPathingMgr();

		public static bool Init()
		{
			log.Info("Starting PathingMgr");
			if (LocalPathingMgr.Init())
				SetPathingMgr(LocalPathingMgr);
			else
				SetPathingMgr(NullPathingMgr);
			return true;
		}

		/// <summary>
		/// Changes the active pathing mgr
		/// </summary>
		/// <param name="mgr"></param>
		public static void SetPathingMgr(IPathingMgr mgr)
		{
			log.Info($"Setting PathingMgr to {mgr}");
			Instance = (mgr == null) ? NullPathingMgr : mgr;
		}

		public static void Stop()
		{
			log.Info("Stopping PathingMgr");
			LocalPathingMgr.Stop();
		}

		/// <summary>
		/// Currently used instance
		/// </summary>
		public static IPathingMgr Instance { get; private set; } = NullPathingMgr;
	}
}
