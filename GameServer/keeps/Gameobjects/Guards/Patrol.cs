using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DOL.GS;
using DOL.GS.Movement;
using DOL.Events;
using DOL.Database;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// Class for a Guard Patrol
	/// </summary>
	public class Patrol
	{
		public const int PATROL_SPEED = 250;
		public Patrol(GameKeepComponent component)
		{
			m_component = component;
		}

		private DBKeepPosition m_spawnPosition = null;
		/// <summary>
		/// The Position object the guards are assigned to
		/// </summary>
		public DBKeepPosition SpawnPosition
		{
			get { return m_spawnPosition; }
			set { m_spawnPosition = value; }
		}

		private GameKeepComponent m_component = null;
		/// <summary>
		/// The Component object the guards are assigned to
		/// </summary>
		public GameKeepComponent Component
		{
			get { return m_component; }
		}
		/// <summary>
		/// The Patrol ID, consider this a template ID
		/// </summary>
		public string PatrolID = "";
		/// <summary>
		/// The Guard Types that make up the Patrol
		/// </summary>
		public static Type[] GuardTypes = new Type[] { typeof(GuardFighter), typeof(GuardArcher), typeof(GuardHealer), typeof(GuardFighter), typeof(GuardArcher) };

		/// <summary>
		/// An ArrayList of all the Guards
		/// </summary>
		public List<GameKeepGuard> PatrolGuards = new List<GameKeepGuard>();
		/// <summary>
		/// The Patrol Path
		/// </summary>
		public PathPoint PatrolPath = null;

		/// <summary>
		/// Method to Initialise the Guards
		/// 
		/// Here we create the instances of the guard
		/// We add to the local array of guards
		/// We assign a Guard ID which is the Patrol ID
		/// We assign the guards component
		/// </summary>
		public void InitialiseGuards()
		{
			Component.Keep.Patrols[PatrolID] = this;
			//need this here becuase it's checked in add to world
			PatrolPath = PositionMgr.LoadPatrolPath(PatrolID, Component);

			Assembly asm = Assembly.GetAssembly(typeof(GameServer));

			//one guard for now
			for (int i = 0; i < 1; i++)
			{
				GameKeepGuard guard = (GameKeepGuard)asm.CreateInstance(GuardTypes[i].FullName, true);
				PatrolGuards.Add(guard);
				guard.TemplateID = PatrolID;
				guard.Component = Component;
				guard.PatrolGroup = this;
				TemplateMgr.RefreshTemplate(guard);
				Component.Keep.Guards.Add(DOL.Database.UniqueID.IdGenerator.generateId(), guard);
			}

			foreach (GameKeepGuard guard in PatrolGuards)
			{
				PositionMgr.LoadGuardPosition(SpawnPosition, guard);
				guard.AddToWorld();
			}
			ChangePatrolLevel();
			StartPatrol();
		}

		/// <summary>
		/// Method to Change a Patrol's Level
		/// 
		/// This method handles the add and removing of guards
		/// </summary>
		public void ChangePatrolLevel()
		{
			//#warning Etaew: deactivated for now
			//return;
			byte level = (byte)(Component.Keep.Level / 2);
			for (int i = 0; i < PatrolGuards.Count; i++)
			{
				GameKeepGuard guard = PatrolGuards[i] as GameKeepGuard;
				if (i <= level)
					guard.AddToWorld();
				else
				{
					PositionMgr.LoadGuardPosition(SpawnPosition, guard);
					guard.RemoveFromWorld();
				}
			}
		}

		/// <summary>
		/// Method to start a Patrol patroling
		/// It sets a patrol leader
		/// And starts moving on Patrol
		/// </summary>
		public void StartPatrol()
		{
			if (PatrolPath == null)
				PatrolPath = PositionMgr.LoadPatrolPath(PatrolID, Component);

			foreach (GameKeepGuard guard in PatrolGuards)
			{
				if (guard.CurrentWayPoint == null)
					guard.CurrentWayPoint = PatrolPath;

				guard.MoveOnPath(PATROL_SPEED);
			}
		}

		public void GetMovementOffset(GameKeepGuard guard, out int x, out int y)
		{
			int modifier = 50;
			x = 0; y = 0;
			int index = PatrolGuards.IndexOf(guard);
			switch (index)
			{
				case 0: x = -modifier; y = modifier; break;
				case 1: x = 0; y = modifier; break;
				case 2: x = -modifier; y = 0; break;
				case 3: x = 0; y = 0; break;
				case 4: x = -modifier; y = -modifier; break;
				case 5: x = 0; y = -modifier; break;
			}
		}
	}
}
