using System;
using System.Collections.Generic;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;

namespace DOL.GS
{
    public class NecromancerClassBehavior : DefaultClassBehavior
    {
        public NecromancerClassBehavior(GamePlayer player) : base(player) { }

        public override void Init()
        {
            Player.Model = (ushort)Player.Client.Account.Characters[Player.Client.ActiveCharIndex].CreationModel;
        }

        public override bool StartAttack(GameObject attackTarget)
        {
            if (!Player.IsShade)
            {
                return true;
            }
            else
            {
                Player.Out.SendMessage("You cannot enter combat while in shade form!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return false;
            }
        }

        public override byte HealthPercentGroupWindow
        {
            get
            {
                if (Player.ControlledBrain == null)
                    return Player.HealthPercent;

                return Player.ControlledBrain.Body.HealthPercent;
            }
        }

        protected override ShadeEffect CreateShadeEffect()
        {
            return new NecromancerShadeEffect();
        }

        // Changes shade state of the player
        public override void Shade(bool makeShade)
        {
            bool wasShade = Player.IsShade;
            base.Shade(makeShade);

            if (wasShade == makeShade)
                return;

            if (makeShade)
            {
                // Necromancer has become a shade. Have any previous NPC 
                // attackers aggro on pet now, as they can't attack the 
                // necromancer any longer.

                if (Player.ControlledBrain != null && Player.ControlledBrain.Body != null)
                {
                    GameNPC pet = Player.ControlledBrain.Body;
                    List<GameObject> attackerList;
                    lock (Player.Attackers)
                        attackerList = new List<GameObject>(Player.Attackers);

                    foreach (GameObject obj in attackerList)
                    {
                        if (obj is GameNPC)
                        {
                            GameNPC npc = (GameNPC)obj;
                            if (npc.TargetObject == Player && npc.AttackState)
                            {
                                IOldAggressiveBrain brain = npc.Brain as IOldAggressiveBrain;
                                if (brain != null)
                                {
                                    npc.AddAttacker(pet);
                                    npc.StopAttack();
                                    brain.AddToAggroList(pet, (int)(brain.GetAggroAmountForLiving(Player) + 1));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Necromancer has lost shade form, release the pet if it
                // isn't dead already and update necromancer's current health.

                if (Player.ControlledBrain != null)
                    (Player.ControlledBrain as ControlledNpcBrain).Stop();
            }
        }

        public override bool RemoveFromWorld()
        {
            if (Player.IsShade)
                Player.Shade(false);

            return base.RemoveFromWorld();
        }

        /// <summary>
        /// Drop shade first, this in turn will release the pet.
        /// </summary>
        public override void Die(GameObject killer)
        {
            Player.Shade(false);

            base.Die(killer);
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            if (Player.ControlledBrain != null)
            {
                GameNPC pet = Player.ControlledBrain.Body;

                if (pet != null && sender == pet && e == GameLivingEvent.CastStarting && args is CastingEventArgs)
                {
                    ISpellHandler spellHandler = (args as CastingEventArgs).SpellHandler;

                    if (spellHandler != null)
                    {
                        int powerCost = spellHandler.PowerCost(Player);

                        if (powerCost > 0)
                            Player.ChangeMana(Player, GameLiving.eManaChangeType.Spell, -powerCost);
                    }

                    return;
                }
            }

            base.Notify(e, sender, args);
        }
    }
}