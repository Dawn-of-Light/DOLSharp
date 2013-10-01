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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using System.Linq;

using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.Effects;
using DOL.GS.SkillHandler;
using DOL.Events;

using DOL.Language;

using DOL.GS.PacketHandler;

using log4net;

namespace DOL.AI.Brain
{
	
	/// <summary>
	/// Statuses of Brain Thinking
	/// </summary>
	public enum eBrainStatus
	{
		idle,
		incoming,
		fighting,
		neardeath,
		dying,
		stopping,
	}
	
	/// <summary>
	/// Improved Brain is a drop in replacement for StandardMobBrain
	/// It's meant to support most in game behavior and be subclassed to make more specific NPC.
	/// Work in Progress
	/// </summary>
	public class ImprovedNpcBrain : APlayerVicinityBrain, IOldAggressiveBrain, IDOLEventHandler
	{
		
		#region static const SCORES
		
		/// <summary>
		/// Scores used to launch actions
		/// </summary>
		public const long MIN_BRAIN_ACTION_SCORE = long.MinValue;
		public const long MAX_BRAIN_ACTION_SCORE = long.MinValue;
		public const long AVG_BRAIN_ACTION_SCORE = 0;
		public const long HIGH_BRAIN_ACTION_SCORE = long.MaxValue >> 1;
		public const long LOW_BRAIN_ACTION_SCORE = long.MinValue >> 1;
	
		
		#endregion
		
		#region Static Class Params
		
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Max Distance to add Player to aggro list
		/// </summary>
		public const int MAX_AGGRO_DISTANCE = 3600;

		/// <summary>
		/// Max Distance to keep player in aggro list
		/// </summary>
		public const int MAX_AGGRO_LIST_DISTANCE = 6000;

		/// <summary>
		/// Max Distance for Aggroing Player Controlled NPC.
		/// Tolakram - Live test with caby pet - I was extremely close before auto aggro
		/// </summary>
		public const int MAX_PET_AGGRO_DISTANCE = 512;
		
		/// <summary>
		/// Max Aggro Level (100% chances Aggressive)
		/// </summary>		
		public const int MAX_AGGRO_LEVEL = 100;
		
		/// <summary>
		/// Min Aggro Level (Not Aggressive)
		/// </summary>	
		public const int MIN_AGGRO_LEVEL = 0;
		
		/// <summary>
		/// Thinking Interval in ms
		/// </summary>		
		public const int THINK_INTERVAL = 1500;
		
		/// <summary>
		/// Max Action Heap Size
		/// </summary>				
		public const int MAX_ACTION_HEAP_SIZE = 15;
		
		/// <summary>
		/// Max Action Queue Size
		/// </summary>				
		public const int MAX_ACTION_QUEUE_SIZE = 20;
		
		/// <summary>
		/// Max Aggressivness Level List Size
		/// </summary>				
		public const int MAX_AGGRO_AMOUNT_LIST_SIZE = 100;
		
		/// <summary>
		/// Min Aggressivness Level to store
		/// </summary>
		public const long MIN_AGGRO_AMOUNT = 1;
		
		#endregion
		
		#region Class Members
		
		private Queue<IBrainAction> m_actionQueue = new Queue<IBrainAction>();
		
		/// <summary>
		/// Action Queue that should be played
		/// </summary>				
		public Queue<IBrainAction> ActionQueue
		{
			get
			{
				return m_actionQueue;
			}
		}

		private bool m_isPlaying = false;
		
		/// <summary>
		/// IsPlaying Handler, to prevent think to add too much action
		/// </summary>				
		private void SetPlayingState(bool state)
		{
			lock((ActionQueue as ICollection).SyncRoot)
				m_isPlaying = state;
		}
		
		/// <summary>
		/// If true we are already playing actions
		/// </summary>				
		public bool GetPlayingState()
		{
			lock((ActionQueue as ICollection).SyncRoot)
				return m_isPlaying;
		}

		private bool m_isThinking = false;
		
		/// <summary>
		/// IsThinking Handler, to prevent thinking twice if first one took longer.
		/// </summary>						
		private void SetThinkingState(bool state)
		{
			lock((ActionQueue as ICollection).SyncRoot)
				m_isThinking = state;
		}
		
		/// <summary>
		/// if True the Brain is still thinking
		/// </summary>						
		public bool GetThinkingState()
		{
			lock((ActionQueue as ICollection).SyncRoot)
				return m_isThinking;			
		}

		private eBrainStatus m_brainStatus = eBrainStatus.idle;
		
		/// <summary>
		/// Brain Status Handler, changing turn of event depending of what we're doing.
		/// </summary>						
		public void SetBrainState(eBrainStatus state)
		{
			lock((ActionQueue as ICollection).SyncRoot)
				m_brainStatus = state;
		}
		
		/// <summary>
		/// Return current brain status to know what we are doing.
		/// </summary>						
		public eBrainStatus GetBrainState()
		{
			lock((ActionQueue as ICollection).SyncRoot)
				return m_brainStatus;			
		}		

		private GameObject m_currentTarget = null;
		
		/// <summary>
		/// Current Target that handle who we are attacking, not the targetted object in Game
		/// </summary>						
		public GameObject CurrentTarget
		{
			get
			{
				return m_currentTarget;
			}
			set 
			{
				m_currentTarget = value;
			}
		}

		private readonly Dictionary<IBrainAction, long> m_idleActions = new Dictionary<IBrainAction, long>();
		
		/// <summary>
		/// Actions list available for brain
		/// When has nothing to do
		/// </summary>
		public Dictionary<IBrainAction, long> IdleActions
		{
			get
			{
				return m_idleActions;
			}
		}
		
		private readonly Dictionary<IBrainAction, long> m_incomingActions = new Dictionary<IBrainAction, long>();

		/// <summary>
		/// Actions list available for brain
		/// When beginning an attack
		/// </summary>
		public Dictionary<IBrainAction, long> IncomingActions
		{
			get
			{
				return m_incomingActions;
			}
		}
		
		private readonly Dictionary<IBrainAction, long> m_fightingActions = new Dictionary<IBrainAction, long>();

		/// <summary>
		/// Actions list available for brain
		/// During Fight, standard actions.
		/// </summary>
		public Dictionary<IBrainAction, long> FightingActions
		{
			get
			{
				return m_fightingActions;
			}
		}
		
		private readonly Dictionary<IBrainAction, long> m_nearDeathActions = new Dictionary<IBrainAction, long>();

		/// <summary>
		/// Actions list available for brain
		/// When life goes low.
		/// </summary>
		public Dictionary<IBrainAction, long> NearDeathActions
		{
			get
			{
				return m_nearDeathActions;
			}
		}
		
		private readonly Dictionary<IBrainAction, long> m_dyingActions = new Dictionary<IBrainAction, long>();

		/// <summary>
		/// Actions list available for brain
		/// When is about to die or even dead already...
		/// </summary>
		public Dictionary<IBrainAction, long> DyingActions
		{
			get
			{
				return m_dyingActions;
			}
		}
		
		#endregion

		#region Constructor
		
		public ImprovedNpcBrain() 
			: base()
		{
			// Set base aggro values
			AggroLevel = 0;
			AggroRange = 0;
			
			// Add default Brain Action Handler
			IBrainAction aggro = new ScanAggroAction(this);
			IdleActions[aggro] = long.MinValue;
			IncomingActions[aggro] = long.MinValue;
			FightingActions[aggro] = long.MinValue;
			NearDeathActions[aggro] = long.MinValue;
			DyingActions[aggro] = long.MinValue;
		}
		
		#endregion
		
		#region Aggressive Interface Implementation
		
		/// <summary>
		/// Max Aggro range in that this npc searches for enemies
		/// Used for scanning only
		/// </summary>
		protected int m_aggroMaxRange;

		/// <summary>
		/// Aggressive Level of this npc
		/// Used for scanning only
		/// </summary>
		protected int m_aggroLevel;

		/// <summary>
		/// Aggressive Level in % 0..100, 0 means not Aggressive
		/// </summary>
		public virtual int AggroLevel
		{
			get { return Math.Min(Math.Max(MIN_AGGRO_LEVEL, m_aggroLevel), MAX_AGGRO_LEVEL); }
			set { m_aggroLevel = Math.Min(Math.Max(MIN_AGGRO_LEVEL, value), MAX_AGGRO_LEVEL); }
		}

		/// <summary>
		/// Max Range at which this brains aggroes.
		/// </summary>
		public virtual int AggroRange
		{
			get { return Math.Min(Math.Max(0, m_aggroMaxRange), MAX_AGGRO_DISTANCE); }
			set { m_aggroMaxRange = Math.Min(Math.Max(value, 0), MAX_AGGRO_DISTANCE); }
		}
		
		/// <summary>
		/// List of Target with aggressiveness Amount
		/// </summary>
		protected readonly Dictionary<GameLiving, long> m_aggressivenessAmountDict = new Dictionary<GameLiving, long>();
		
		/// <summary>
		/// Access aggressiveness Amounts Read Only.
		/// </summary>		
		public Dictionary<GameLiving, long> AggressivenessAmount 
		{
			get 
			{
				return m_aggressivenessAmountDict; 
			}
		}
		
		/// <summary>
		/// True if this mob already have aggro in his list.
		/// </summary>
		public bool HaveAggro
		{
			get {
				return AggressivenessAmount.Count > 1;
			}
		}
		
		/// <summary>
		/// Add living to the aggrolist
		/// aggroamount can be negative to lower amount of aggro
		/// </summary>
		/// <param name="living">Target for Aggressiveness</param>
		/// <param name="aggroamount">Amount of Aggressiveness</param>
		public virtual void AddToAggroList(GameLiving living, int aggroAmount)
		{
			lock((AggressivenessAmount as ICollection).SyncRoot)
			{			
				// If object is made unavailable
				if(!isGameObjectAvailable(living))
					return;
				
				// Confused NPC can't update their AggroList
				if(Body.IsConfused)
					return;
	
				// tolakram - duration spell effects will attempt to add to aggro after npc is dead
				if(!Body.IsAlive)
					return;
				
				// If living is Pet should add master aggro
				if(living is GameNPC && ((GameNPC)living).Brain != null && ((GameNPC)living).Brain is IControlledBrain) 
				{
					GameLiving master = ((IControlledBrain)((GameNPC)living).Brain).GetLivingOwner();
					
					if(isGameObjectAvailable(master))
					{
						// add to the master aggro
						if(!AggressivenessAmount.ContainsKey(master))
						{
							AggressivenessAmount[master] = aggroAmount >> 1;
						}
						else
						{						
							AggressivenessAmount[master] += aggroAmount >> 1;
						}
					}
				}
				else if(living is GamePlayer && aggroAmount > 0)
				{
					// Register all living group member, Pets should be added too
					GamePlayer player = (GamePlayer)living;
					
					if (player.Group != null)
					{
						foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
						{
							// Add Group Member and their Controlled Buddies
							if(!AggressivenessAmount.ContainsKey(p))
							{
								AggressivenessAmount[p] = MIN_AGGRO_AMOUNT;
							}
							
							if(p.ControlledBrain != null && isGameObjectAvailable(p.ControlledBrain.Body) && !AggressivenessAmount.ContainsKey(p.ControlledBrain.Body))
							{
								AggressivenessAmount[p.ControlledBrain.Body] = MIN_AGGRO_AMOUNT;
							}
						}
					}
					
					//Check Protect on GamePlayer
					foreach (ProtectEffect protect in player.EffectList.GetAllOfType<ProtectEffect>())
					{
						// if no aggro left no use to continue listing Protect Effect
						if (aggroAmount <= 0) 
							break;
						// Target is not under this protect effect
						if (protect.ProtectTarget != player) 
							continue;
						// Protecting player is incapacited (LD, dead, mezzed, stunned)
						if (protect.ProtectSource.IsIncapacitated) 
							continue;
						// Protecting Player is sitting
						if (protect.ProtectSource.IsSitting) 
							continue;
						// Protecting Player is not fighting...
						if (!protect.ProtectSource.InCombat) 
							continue;
						// Protecting Player is too far away
						if (!player.IsWithinRadius(protect.ProtectSource, ProtectAbilityHandler.PROTECT_DISTANCE))
							continue;
						
						// P I: prevents 10% of aggro amount
						// P II: prevents 20% of aggro amount
						// P III: prevents 30% of aggro amount
						// guessed percentages, should never be higher than or equal to 50%
						// Hard Capped to Protect Level IV
						int abilityLevel = Math.Max(protect.ProtectSource.GetAbilityLevel(Abilities.Protect), 4);
						int protectAmount = (int)((abilityLevel * 0.10) * aggroAmount);
	
						if (protectAmount > 0)
						{
							// Remove aggroAmount with protect ability to total aggroAmount
							aggroAmount -= protectAmount;
							
							// Tell Protect player his ability worked
							protect.ProtectSource.Out.SendMessage(LanguageMgr.GetTranslation(protect.ProtectSource.Client.Account.Language, "AI.Brain.StandardMobBrain.YouProtDist", player.GetName(0, false),
							                                                                 Body.GetName(0, false, protect.ProtectSource.Client.Account.Language, Body)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							// TODO : LanguageMgr
							// Tells Protected player that protect ability worked.
							protect.ProtectTarget.Out.SendMessage("You are protected by " + protect.ProtectSource.GetName(0, false) + " from " + Body.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							
							// Gives back aggro amount to the Protecting player.
							if (AggressivenessAmount.ContainsKey(protect.ProtectSource))
							{
								AggressivenessAmount[protect.ProtectSource] += protectAmount;
							}
							else 
							{
								AggressivenessAmount[protect.ProtectSource] = protectAmount;
							}
						}
					}
				}
			
				// Apply Remaining Aggro
				if (AggressivenessAmount.ContainsKey(living))
				{
					AggressivenessAmount[living] += aggroAmount;
				}
				// Else create Value
				else
				{
					AggressivenessAmount[living] = aggroAmount;
				}
			}
		}

		/// <summary>
		/// Get current amount of aggro on aggrotable
		/// </summary>
		/// <param name="living">Target to check Aggressiveness for</param>
		/// <returns>Aggressiveness amount or long.MinValue if not found</returns>
		public virtual long GetAggroAmountForLiving(GameLiving living)
		{
			if(!isGameObjectAvailable(living))
			   return long.MinValue;
			   
			lock (((ICollection)AggressivenessAmount).SyncRoot)
			{
				if (AggressivenessAmount.ContainsKey(living))
				{
					return AggressivenessAmount[living];
				}
				return long.MinValue;
			}
		}

		/// <summary>
		/// Remove one living from aggro list
		/// </summary>
		/// <param name="living">Target to remove</param>
		public virtual void RemoveFromAggroList(GameLiving living) 
		{
			if(!isGameObjectAvailable(living)) 
				return;
			
			lock (((ICollection)AggressivenessAmount).SyncRoot)
			{
				if (AggressivenessAmount.ContainsKey(living))
				{
					AggressivenessAmount.Remove(living);
				}
			}			
		}
				
		/// <summary>
		/// Remove all livings from the aggrolist
		/// </summary>
		public virtual void ClearAggroList() 
		{
			lock (((ICollection)AggressivenessAmount).SyncRoot)
			{
				AggressivenessAmount.Clear();
			}			
		}

		/// <summary>
		/// Makes a copy of current aggro list
		/// </summary>
		/// <returns>Cloned Dictionary</returns>
		public virtual Dictionary<GameLiving, long> CloneAggroList()
		{
			lock (((ICollection)AggressivenessAmount).SyncRoot)
			{
				return new Dictionary<GameLiving, long>(AggressivenessAmount);
			}
		}

		/// <summary>
		/// calculate the aggro of this npc against another living
		/// </summary>
		/// <param name="target">Living Target to check Aggro Level</param>
		/// <returns>0..100 = 0 not aggro, 100 most chances of Aggro.</returns>
		public virtual int CalculateAggroLevelToTarget(GameLiving target)
		{
			// if target is unavailable
			if(!isGameObjectAvailable(target))
				return MIN_AGGRO_LEVEL;
			
			// If not allowed to attack, no Aggressiveness
			if (GameServer.ServerRules.IsAllowedToAttack(Body, target, true) == false)
				return MIN_AGGRO_LEVEL;


			// related to the pet owner if applicable and not grey to target.
			if (target is GameNPC)
			{
				IControlledBrain brain = ((GameNPC)target).Brain as IControlledBrain;
				if (brain != null)
				{
					GameLiving thisLiving = (((GameNPC)target).Brain as IControlledBrain).GetLivingOwner();
					if (thisLiving != null)
					{
						if (thisLiving.IsObjectGreyCon(Body))
							return MIN_AGGRO_LEVEL;
					}
				}
			}
			
			// Only attack if green+ to target
			if (target.IsObjectGreyCon(Body)) 
				return MIN_AGGRO_LEVEL;

			
			int factionAggroLevel = MIN_AGGRO_LEVEL;
			// Check Faction to player
			if (Body.Faction != null && target is GamePlayer)
			{
				factionAggroLevel = Body.Faction.GetAggroToFaction((GamePlayer)target);
			}
			// Check Faction to NPC
			else if(Body.Faction != null && target is GameNPC)
			{
				if (((GameNPC)target).Faction != null)
				{
					// If NPC is controlled check master's
					if (((GameNPC)target).Brain is IControlledBrain)
					{
						GameLiving master = ((IControlledBrain)((GameNPC)target).Brain).GetLivingOwner();
						if(master != null && master is GamePlayer) 
						{
							factionAggroLevel = Body.Faction.GetAggroToFaction((GamePlayer)master);
						}
						else if(master != null && master is GameNPC && Body.Faction.EnemyFactions.Contains(((GameNPC)master).Faction))
						{
							factionAggroLevel = MAX_AGGRO_LEVEL;
						}
					}
					// Else it's just a NPC
					else
					{
						if(Body.Faction.EnemyFactions.Contains(((GameNPC)target).Faction))
							factionAggroLevel = MAX_AGGRO_LEVEL;
					}
				}
			}
			
			factionAggroLevel += AggroLevel;
			
			//we put this here to prevent aggroing non-factions npcs
			if(target.Realm == eRealm.None && target is GameNPC)
				return MIN_AGGRO_LEVEL;

			if (factionAggroLevel >= MAX_AGGRO_LEVEL) 
				return MAX_AGGRO_LEVEL;
			
			if(factionAggroLevel <= MIN_AGGRO_LEVEL)
				return MIN_AGGRO_LEVEL;
				
			return factionAggroLevel;
		}
		
		#endregion
		
		#region Entry Point
		
		public override void Think() {
			
			// if brain is playing and will finish in next interval, returns.
			// If brain hasn't finished thinking, returns. (could raise a warning)
			if((GetPlayingState() && GetPlayRemaingTime() > THINK_INTERVAL) || GetThinkingState())
			{
				return;					
			}
			
			SetThinkingState(true);
			
			// Prevent overriding max values
			DoCleanups();
			
			lock(((ICollection)ActionQueue).SyncRoot)
			{
				// What to do on brain status.
				EnQueueActions(GetBestScoresActions(GetBrainState()));
			}
			
			Play();
			
			SetThinkingState(false);
		}
		
		public override bool Start()
		{
			if(!base.Start())
				return false;

			if(PlayingTimer == null) {
				PlayingTimer = new Timer();
				PlayingTimer.AutoReset = false;
			}
				
			PlayingTimer.Elapsed += new ElapsedEventHandler(PlayTick);
			return true;
		}

		public override bool Stop()
		{
			if(!base.Stop())
				return false;
			
			// Tell we are stopping
			SetBrainState(eBrainStatus.stopping);
			
			// Stop Playing
			PlayingTimer.Stop();
			PlayingTimer.Close();
			
			// Break any pending action
			lock(((ICollection)ActionQueue))
			{
				foreach(IBrainAction act in ActionQueue)
					act.Break();
				
				ActionQueue.Clear();
			}
			
			// Clear States
			ClearAggroList();
			
			SetPlayingState(false);
			
			return true;
		}
		
		public IEnumerable<IBrainAction> GetBestScoresActions(eBrainStatus state) {
			switch(GetBrainState())
			{
				case eBrainStatus.idle:
					foreach(IBrainAction lookup in IdleActions.Keys) {
						IdleActions[lookup] = lookup.Analyze();
					}
				//Add Actions to Queue
				return (from actions in IdleActions where actions.Value > long.MinValue orderby actions.Value descending select actions.Key).Take(MAX_ACTION_QUEUE_SIZE);

					
				case eBrainStatus.incoming:
					foreach(IBrainAction lookup in IncomingActions.Keys) {
						IncomingActions[lookup] = lookup.Analyze();
					}
				//Add Actions to Queue
				return (from actions in IncomingActions where actions.Value > long.MinValue orderby actions.Value descending select actions.Key).Take(MAX_ACTION_QUEUE_SIZE);

				case eBrainStatus.fighting:
					foreach(IBrainAction lookup in FightingActions.Keys) {
						FightingActions[lookup] = lookup.Analyze();
					}
				//Add Actions to Queue
				return (from actions in FightingActions where actions.Value > long.MinValue orderby actions.Value descending select actions.Key).Take(MAX_ACTION_QUEUE_SIZE);
				
				case eBrainStatus.neardeath:
					foreach(IBrainAction lookup in NearDeathActions.Keys) {
						NearDeathActions[lookup] = lookup.Analyze();
					}
				//Add Actions to Queue
				return (from actions in NearDeathActions where actions.Value > long.MinValue orderby actions.Value descending select actions.Key).Take(MAX_ACTION_QUEUE_SIZE);
					
				case eBrainStatus.dying:
					foreach(IBrainAction lookup in DyingActions.Keys) {
						DyingActions[lookup] = lookup.Analyze();
					}
				//Add Actions to Queue
				return (from actions in DyingActions where actions.Value > long.MinValue orderby actions.Value descending select actions.Key).Take(MAX_ACTION_QUEUE_SIZE);
			}
			
			// Empty action list if no action taken
			return new List<IBrainAction>();
		}
		
		#endregion

		#region Clean Utils
		
		/// <summary>
		/// Start commons Cleanups
		/// </summary>
		private void DoCleanups() 
		{
			CleanupAggressiveness(MAX_AGGRO_AMOUNT_LIST_SIZE, MAX_AGGRO_LIST_DISTANCE);
			CleanupActionQueue(MAX_ACTION_QUEUE_SIZE);
		}
		
		/// <summary>
		/// Cleanup Aggressive Table if there is too much records
		/// </summary>
		public void CleanupAggressiveness(int maxSize, int maxDistance)
		{
			lock(((ICollection)AggressivenessAmount).SyncRoot) 
			{
				// Cleanup Aggro Amount List if too long.
				if(AggressivenessAmount.Count > maxSize)
				{
					// Sort Table
					IEnumerable<GameLiving> leastAggroAmount = (from livings in AggressivenessAmount orderby livings.Value select livings.Key).Take(AggressivenessAmount.Count-maxSize);
						
					foreach(GameLiving toRemove in leastAggroAmount)
					{
						AggressivenessAmount.Remove(toRemove);
					}
				}
				
				// Cleanup Aggro Amount List of living being too far
				foreach(GameLiving living in AggressivenessAmount.Keys)
					if(!Body.IsWithinRadius(living, maxDistance))
						AggressivenessAmount.Remove(living);
			}
		}

		/// <summary>
		/// Cleanup Action Queue if there is too much records.
		/// </summary>
		public void CleanupActionQueue(int maxSize) 
		{
			lock(((ICollection)ActionQueue).SyncRoot) 
			{
				if(ActionQueue.Count > maxSize)
				{
					int cnt;
					for(cnt = 0 ; cnt < maxSize - ActionQueue.Count ; cnt++) 
					{
						// drop some actions
						ActionQueue.Dequeue();
					}
				}
			}
		}
		
		#endregion
		
		#region Brain Utils
		/// <summary>
		/// Check if GameObject is not null and is Active
		/// </summary>
		public static bool isGameObjectAvailable(GameLiving obj)
		{
			if(obj == null || obj.ObjectState != GameObject.eObjectState.Active)
				return false;
			
			return true;
		}

		/// <summary>
		/// Get ActionQueue Playing remaining time
		/// </summary>		
		public ulong GetPlayRemaingTime() {
			if(GetPlayingState())
			{
				lock(((ICollection)ActionQueue).SyncRoot)
				{
					return CountQueueRemainingTime(ActionQueue);
				}
			}
			
			return 0;
		}

		/// <summary>
		/// Count total Duration of an ActionQueue
		/// </summary>
		public static ulong CountQueueRemainingTime(Queue<IBrainAction> actions)
		{
			if(actions == null)
				return 0;
			
			ulong remainingTime = 0;
			foreach(IBrainAction act in actions)
				remainingTime += act.Duration;
			
			return remainingTime;
		}
		
		#endregion
		
		#region Message Event Handlers
		/// <summary>
		/// Take action on Message Notified, and dispatch it to Brain Actions Handler
		/// </summary>
		public override void Notify(DOL.Events.DOLEvent e, object sender, EventArgs args)
		{
			base.Notify(e, sender, args);
			
			// Send messages to all actions Handler
			NotifyBrainActions(e, sender, args);
		}
		
		/// <summary>
		/// Dispatch message to Brain Actions Handler
		/// </summary>
		private void NotifyBrainActions(DOL.Events.DOLEvent e, object sender, EventArgs args)
		{
			// List all type of Actions and Notify them Only if they're able to handle event.
			lock(((ICollection)IdleActions).SyncRoot)
				foreach(IBrainAction act in IdleActions.Keys)
					if(act is IDOLEventHandler)
						((IDOLEventHandler)act).Notify(e, sender, args);

			lock(((ICollection)IdleActions).SyncRoot)
				foreach(IBrainAction act in IncomingActions.Keys)
					if(act is IDOLEventHandler)
						((IDOLEventHandler)act).Notify(e, sender, args);

			lock(((ICollection)IdleActions).SyncRoot)
				foreach(IBrainAction act in FightingActions.Keys)
					if(act is IDOLEventHandler)
						((IDOLEventHandler)act).Notify(e, sender, args);

			lock(((ICollection)IdleActions).SyncRoot)
				foreach(IBrainAction act in NearDeathActions.Keys)
					if(act is IDOLEventHandler)
						((IDOLEventHandler)act).Notify(e, sender, args);

			lock(((ICollection)IdleActions).SyncRoot)
				foreach(IBrainAction act in DyingActions.Keys)
					if(act is IDOLEventHandler)
						((IDOLEventHandler)act).Notify(e, sender, args);
			
		}
		#endregion
		
		#region Environnement Updaters (Initial State Changer)
		#endregion
		
		#region Action Playing

		/// <summary>
		/// Playing Timer to launch actions one after another.
		/// </summary>	
		private Timer m_playingTimer = new Timer();
		
		private Timer PlayingTimer
		{
			get
			{
				return m_playingTimer;
			}
			set
			{
				m_playingTimer = value;
			}
		}

		/// <summary>
		/// Play Actions Enqueued in ActionQueue until no more actions or Break !
		/// </summary>
		private void Play()
		{
			// Check if we are already playing
			if(GetPlayingState())
				return;
			
			SetPlayingState(true);
			
			// Launch our timer
			PlayingTimer.Interval = 0;
			PlayingTimer.Start();
		}

		/// <summary>
		/// Ticking Action Player
		/// </summary>		
		private static void PlayTick(object sender, ElapsedEventArgs args)
		{
			if(sender == null || !(sender is ImprovedNpcBrain))
				return;
			
			// get our brain object
			ImprovedNpcBrain brain = sender as ImprovedNpcBrain;
			
			lock(((ICollection)brain.ActionQueue).SyncRoot)
			{
				//Check if anything else to do
				if(brain.ActionQueue.Count < 1)
				{
					// We're Finished
					brain.PlayingTimer.Stop();
					brain.SetPlayingState(false);
					return;
				}
				
				// Get next Action
				IBrainAction act = brain.ActionQueue.Dequeue();
			
				// Play it
				if(!act.Breaking)
					act.Action();
				
				// Set next round time
				brain.PlayingTimer.Interval = act.Duration;
				brain.PlayingTimer.Start();
				
			}		
			
		}

		/// <summary>
		/// Enqueue selected Enumerable of IBrainAction to the ActionQueue
		/// </summary>		
		private void EnQueueActions(IEnumerable<IBrainAction> bestScoresAction)
		{
			if(bestScoresAction == null)
				return;
			
			ulong timeToFinish = 0;
			foreach(IBrainAction act in bestScoresAction) {
				// Dont enqueue too much actions
				if(timeToFinish > (THINK_INTERVAL << 1))
					break;
				
				// Enqueue and add Duration to flag.
				timeToFinish += act.Duration;
				ActionQueue.Enqueue(act);
			}
		}
		#endregion
	}
}
