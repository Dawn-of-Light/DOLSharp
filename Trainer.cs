using System.Collections;
using System.Collections.Generic;
using DOL.GS.Trainer;
using System;
using DOL.Events;

namespace DOL.GS.Scripts
{
	public class GenericTrainer : GameTrainer
	{
		private static GameTrainer[] ValidTrainers = null;

		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			ValidTrainers = new GameTrainer[]
			{
				//alb
				new ArmsmanTrainer(),
				new CabalistTrainer(),
				new ClericTrainer(),
				new FriarTrainer(),
				new InfiltratorTrainer(),
				new MercenaryTrainer(),
				new MinstrelTrainer(),
				new PaladinTrainer(),
				new ReaverTrainer(),
				new ScoutTrainer(),
				new SorcererTrainer(),
				new TheurgistTrainer(),
				new WizardTrainer(),
				//hib
				new AnimistTrainer(),
				new BardTrainer(),
				new BlademasterTrainer(),
				new ChampionTrainer(),
				new DruidTrainer(),
				new EldritchTrainer(),
				new EnchanterTrainer(),
				new HeroTrainer(),
				new MentalistTrainer(),
				new NightshadeTrainer(),
				new RangerTrainer(),
				new ValewalkerTrainer(),
				new WardenTrainer(),
				//mid
				new BerserkerTrainer(),
				new BonedancerTrainer(),
				new HealerTrainer(),
				new HunterTrainer(),
				new RunemasterTrainer(),
				new SavageTrainer(),
				new ShadowbladeTrainer(),
				new ShamanTrainer(),
				new SkaldTrainer(),
				new SpiritmasterTrainer(),
				new ThaneTrainer(),
				new WarriorTrainer()
			};
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			if (player.Level < 5 && player.Experience >= GameServer.ServerRules.GetExperienceForLevel(5))
			{
				return Interact(player);
			}

			//offer the player the classes he can become
			if (player.Level == 5 && player.CharacterClass.BaseName == player.CharacterClass.Name)
			{
				string message = "Hello " + player.Name + ", you can become: ";
				foreach (GameTrainer trainer in ValidTrainers)
				{
					string className = trainer.GetType().ToString();
					className = className.Replace("DOL.GS.Trainer.", "");
					className = className.Replace("Trainer", "");
					if (trainer.CanPromotePlayer(player))
						message += "[" + className + "],";
				}
				message += " just click on the class name to become that class, then right click me to get to level 50.";
				SayTo(player, message);
			}

			if (player.CharacterClass.Name != player.CharacterClass.BaseName 
				&& player.Experience >= GameServer.ServerRules.GetExperienceForLevel(50) 
				&& player.Level < 50)
			{
				return Interact(player);
			}

			//send trainer window
			if (player.CharacterClass.BaseName != player.CharacterClass.Name)
				player.Out.SendTrainerWindow();

			return true;
		}

		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text))
				return false;

			GamePlayer player = source as GamePlayer;
			if (player == null)
				return false;

			foreach (GameTrainer trainer in ValidTrainers)
			{
				string className = trainer.GetType().ToString();
				className = className.Replace("DOL.GS.Trainer.", "");
				className = className.Replace("Trainer", "");

				if (className == text && trainer.CanPromotePlayer(player))
					trainer.PromotePlayer(player);
			}

			return true;
		}

		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList();
			list.Add("You target [" + GetName(0, false) + "]");
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and trains members of all classes.");
			list.Add("[Right click to display a trainer window]");
			return list;
		}
	}
}