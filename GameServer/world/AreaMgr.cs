using DOL.Database;
using System;
using log4net;
using System.Reflection;

namespace DOL.GS
{
	public class AreaMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static bool LoadAllAreas()
		{
			try
			{
				Assembly gasm = Assembly.GetExecutingAssembly();
				var dbAreas = GameServer.Database.SelectAllObjects<DBArea>();
				foreach (DBArea thisArea in dbAreas)
				{
					AbstractArea area = (AbstractArea)gasm.CreateInstance(thisArea.ClassType, false);
					if (area == null)
					{
						foreach (Assembly asm in ScriptMgr.Scripts)
						{
							try
							{
								area = (AbstractArea)asm.CreateInstance(thisArea.ClassType, false);

                                if (area != null)
                                {
                                    break;
                                }
							}
							catch (Exception e)
							{
								if (log.IsErrorEnabled)
								{
								    log.Error("LoadAllAreas", e);
								}
							}
						}

						if (area == null)
						{
							log.Debug($"area type {thisArea.ClassType} cannot be created, skipping");
							continue;
						}
					}
					area.LoadFromDatabase(thisArea);
					area.Sound = thisArea.Sound;
					area.CanBroadcast = thisArea.CanBroadcast;
					area.CheckLOS = thisArea.CheckLOS;
					Region region = WorldMgr.GetRegion(thisArea.Region);

                    if (region == null)
					{
					    continue;
					}

					region.AddArea(area);
					log.Info($"Area added: {thisArea.Description}");
				}
				return true;
			}
			catch (Exception ex)
			{
				log.Error("Loading all areas failed", ex);
				return false;
			}
		}
	}
}