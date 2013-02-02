using System.Reflection;
using DOL.GS;
using DOL.GS.Quests;
using log4net;

namespace DOL.GS
{
	public class TaskMaster : GameNPC
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			//we need to disable them for players for now
			if (player.Client.Account.PrivLevel == 1)
			{
				SayTo(player, "I'm sorry, Task Dungeons are currently disabled!");
				return true;
			}

			if (player.Mission == null)
				SayTo(player, "I'm sure you're already aware that the guards protecting our towns often pay bounties to young adventurers willing to help them deal with threats in the area. We've decided to expand upon this idea and begin what we call the Taskmaster program. Voulenteers such as myself have been authorized to reward those willing to confront the dangers lurking within our dungeons. If you would like to assist I can give you such an [assignment] right now, and you will be rewarded as soon as you complete it.");
			else SayTo(player, "You already have a task that requires competion.");

			return true;
		}

		public override bool WhisperReceive(GameLiving source, string str)
		{
			if (!base.WhisperReceive(source, str))
				return false;

			GamePlayer player = source as GamePlayer;
			if (player == null)
				return false;

			if (player.Mission != null)
				return false;

            switch (str.ToLower())
            {
                case "assignment":
                    {
                        SayTo(player, "Based on your prowess and preference in engaging the enemy, I have assignments located in the [labyrinthine dungeons] for close quarter melee and tasks awaiting in [long corridors] for those who prefer ranged attacks. Select which you would prefer and I shall assign a task for you to complete or if you wish I can go into more detail about the Taskmaster [program]");
                        break;
                    }
                case "program":
                    {
                        SayTo(player, "Unlike the tasks which you can receive from guards by using /whisper task when speaking to one, the taskmaster program is available to adventurers across a wide range of experience. You'll find taskmasters in many of our towns, ready to offer you the chance to aid the realm by confronting some of the monsters which inhabit a nearby dungeon. With the recent emergance of the new threat from beneath the Earth, as well as the ongoing war with the enemy realms, we need all the help that we can get.");
                        break;
                    }
                case "long corridors":
                case "labyrinthine dungeons":
                    {
                        if (player.Mission != null)
                            break;
                        //Can't if we already have one!

                        //Can't if group has one!
                        if (player.Group != null && player.Group.Mission != null)
                            break;

                        log.Info("INFO: TaskMaster Dungeons activated");
                        TaskDungeonMission mission;
                        if (player.Group != null)
                        	mission = new TaskDungeonMission(player.Group, TaskDungeonMission.eDungeonType.Ranged);
                        else
                        {
                            mission = new TaskDungeonMission(player, TaskDungeonMission.eDungeonType.Melee);
                            player.Mission = mission;
                        }
                        /*
                         * Very well Gwirenn, it's good to see adventurers willing to help out the realm in such times.  Dralkden the Thirster has taken over the caves to the south and needs to be disposed of.  Good luck!
                         * Very well Gwirenn, it's good to see adventurers willing to help out the realm in such times. Clear the caves to the south of creatures. Good luck!
                         */
                        string msg = "Very well " + player.Name + ", it's good to see adventurers willing to help out the realm in such times.";
                        switch (mission.TDMissionType)
                        {
                            case TaskDungeonMission.eTDMissionType.Clear:
                                msg += " Clear " + mission.TaskRegion.Description + " of creatures. Good luck!";
                                break;
                            case TaskDungeonMission.eTDMissionType.Boss:
                                msg += " " + mission.BossName + " has taken over " + mission.TaskRegion.Description + " and needs to be disposed of. Good luck!";
                                break;
                            case TaskDungeonMission.eTDMissionType.Specific:
                                msg += " Please remove " + mission.Total + " " + mission.TargetName + " from " + mission.TaskRegion.Description + "! The entrance is nearby.";
                                break;
                        }
                        SayTo(player, msg);
                        break;
                    }
            }
			return true;
		}
	}
}
