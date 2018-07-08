using DOL.GS.PacketHandler;
using DOL.AI.Brain;

namespace DOL.GS.Atlantis
{
    /// <summary>
    /// The base class that most or all ArtifactEncounter mob's should inherit from.
    /// </summary>
    public class TetheredEncounterMob : BasicEncounterMob
    {
        public override void SaveIntoDatabase()
        {
        }

        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
        {
            // Check if this encounter mob is tethered and if so, ignore any damage done both outside of or too far from it's tether range.
            if (TetherRange > 100)
            {
                // if controlled NPC - do checks for owner instead
                if (source is GameNPC)
                {
                    IControlledBrain controlled = ((GameNPC)source).Brain as IControlledBrain;
                    if (controlled != null)
                    {
                        source = controlled.GetPlayerOwner();
                    }
                }

                if (IsOutOfTetherRange)
                {
                    if (source is GamePlayer)
                    {
                        GamePlayer player = source as GamePlayer;
                        player.Out.SendMessage("The " + Name + " is too far from its encounter area, your damage fails to have an effect on it!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                        return;
                    }

                    return;
                }
                else
                {
                    if (IsWithinRadius(source, TetherRange))
                    {
                        base.TakeDamage(source, damageType, damageAmount, criticalAmount);
                        return;
                    }

                    if (source is GamePlayer)
                    {
                        GamePlayer player = source as GamePlayer;
                        player.Out.SendMessage("You are too far from the " + Name + ", your damage fails to effect it!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                    }

                    return;
                }
            }
            else
            {
                base.TakeDamage(source, damageType, damageAmount, criticalAmount);
            }
        }
    }
}
