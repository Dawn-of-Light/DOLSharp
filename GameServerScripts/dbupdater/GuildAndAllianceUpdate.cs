/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Linq;

using DOL.Database;

using log4net;

namespace DOL.GS.DatabaseUpdate
{
	/// <summary>
	/// Update Guild and Alliance Database then perform Clean Up
	/// </summary>
	[DatabaseUpdate]
	public class GuildAndAllianceUpdate : IDatabaseUpdater
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void Update()
		{
			if (log.IsInfoEnabled)
				log.Info("Start Searching for records that need update...");
			
			// Change the Leader Relation if Missing
			var alliances = DOLDB<DBAlliance>.SelectObjects(DB.Column("LeaderGuildID").IsEqualTo(string.Empty).Or(DB.Column("LeaderGuildID").IsNull()));
			
			if (alliances.Any())
			{
				
				var leadingGuilds = DOLDB<DBGuild>.MultipleSelectObjects(alliances.Select(al => DB.Column("AllianceID").IsEqualTo(al.ObjectId).And(DB.Column("GuildName").IsEqualTo(al.AllianceName))));
				
				var alliancesWithLeader = leadingGuilds.Select((gd, i) => {
				                                               	var al = alliances[i];
				                                               	DBGuild lead = null;
				                                               	try 
				                                               	{
				                                               		lead = gd.SingleOrDefault(gld => al.AllianceName.Equals(gld.GuildName));
				                                               	}
				                                               	catch (Exception e)
				                                               	{
				                                               		if (log.IsErrorEnabled)
				                                               			log.ErrorFormat("Wrong records while trying to retrieve Guild Leader (AllianceID: {0}, AllianceName: {1})\n{2}", al.ObjectId, al.AllianceName, e);
				                                               	}
				                                               	return new { Alliance = al, Leader = lead };
				                                               }).ToArray();
				
				if (log.IsInfoEnabled)
					log.InfoFormat("Fixing Alliances without Leader : {0} records found.", alliancesWithLeader.Length);
				
				foreach (var pair in alliancesWithLeader)
				{
					if (pair.Leader != null)
						pair.Alliance.LeaderGuildID = pair.Leader.GuildID;
					else if (log.IsWarnEnabled)
						log.WarnFormat("Alliance (ID:{0}, Name:{1}) can't resolve its Leading Guild !", pair.Alliance.ObjectId, pair.Alliance.AllianceName);
				}
				
				var saved = GameServer.Database.SaveObject(alliancesWithLeader.Select(pair => pair.Alliance));
				
				if (saved && log.IsInfoEnabled)
					log.InfoFormat("Finished saving Alliances without Leader successfully!");
				if (!saved && log.IsErrorEnabled)
					log.ErrorFormat("Could not save all Alliances without Leader, check logs or records...");
			}
			
			if (log.IsInfoEnabled)
				log.Info("End of Database Update...");
		}
	}
}
