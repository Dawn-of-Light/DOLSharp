using System;
using System.Collections;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
"&train",
1,
"trains multiple times")]
	public class MultipleTrainCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player.TargetObject is GameTrainer == false)
			{
				client.Out.SendMessage("You have to be at your trainer to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			if (args.Length != 3)
			{
				client.Out.SendMessage("Usage: e.g. /train Shields 40", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			string line = args[1];
			int target = int.Parse(args[2]);
			if (target == -1)
			{
				client.Out.SendMessage("Invalid amount!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			Specialization spec = client.Player.GetSpecialization(line);
			if (spec == null)
			{
				client.Out.SendMessage("Invalid line name, remember if there are 2 words in your line name enclose them with \"\"!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			int current = spec.Level;
			if (current >= client.Player.Level)
			{
				client.Out.SendMessage("You can't train in this specialization again this level!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			if (target <= current)
			{
				client.Out.SendMessage("You have already trained this amount!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			target = target - current;
			for (int i = 0; i < target; i++)
			{
				if (spec.Level >= client.Player.Level)
				{
					client.Out.SendMessage("You can't train in this specialization again this level!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					break;
				}
				if (client.Player.SkillSpecialtyPoints >= spec.Level + 1)
				{
					client.Player.SkillSpecialtyPoints -= (ushort)(spec.Level + 1);
					spec.Level++;
					client.Player.OnSkillTrained(spec);
					client.Out.SendUpdatePoints();
					client.Out.SendTrainerWindow();
				}
				else
				{
					client.Out.SendMessage("That specialization costs " + (spec.Level + 1) + " specialization points!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendMessage("You don't have that many specialization points left for this level.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					break;
				}
			}
			client.Out.SendMessage("Training complete!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return 1;
		}
	}
}