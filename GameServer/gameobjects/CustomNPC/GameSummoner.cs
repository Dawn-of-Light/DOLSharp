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
using DOL.AI;
using DOL.AI.Brain;

namespace DOL.GS
{
    /// <summary>
    /// An NPC which summons a pet based on PetTemplate when health is below SummonThreshold, 
    /// and releases it on death or when it heals to full.
    /// </summary>
    public abstract class GameSummoner : GameNPC
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Percent health remaining to summon pet
        /// </summary>
        abstract public int SummonThreshold { get; }

        /// <summary>
        /// Template to base summon on
        /// </summary>
        abstract public NpcTemplate PetTemplate { get; }

        /// <summary>
        /// Level to summon pet at; positive for absolute value, negative for percentage of summoner's level
        /// </summary>
        abstract public byte PetLevel { get; }

        virtual public int PetTetherRange { get { return 0; } }
        virtual public int PetMaxDistance { get { return 1500; } }
        virtual public byte PetSize {  get { return 0; } }
        virtual public int PetSummonDistance {  get { return 100; } }

        /// <summary>
        /// Health threshold to summon pet at
        /// </summary>
        private GameNPC m_pet = null;

        public GameSummoner() : base() { }
        public GameSummoner(ABrain defaultBrain) : base(defaultBrain) { }
        public GameSummoner(INpcTemplate template) : base(template) { }

        /// <summary>
        /// Gets/sets the object health
        /// </summary>
        public override int Health
        {
            get { return base.Health; }
            set
            {
                base.Health = value;

                if (Health <= 0 || Health >= MaxHealth)
                    ReleasePet();
                else if (HealthPercent <= SummonThreshold)
                    SummonPet();
            }
        }

        /// <summary>
        /// Summon the pet
        /// </summary>
        protected virtual void SummonPet()
        {
            if (PetTemplate != null && PetLevel != 0 && m_pet == null)
            {
                m_pet = new GameNPC(PetTemplate);
                if (m_pet != null)
                {
                    m_pet.CurrentRegion = CurrentRegion;

                    // Summon pet to the left or right of the summoner
                    ushort sideHeading = (ushort)(Heading + 900);
                    if (Util.Random(1) < 1)
                        sideHeading += 1800;
                    Point2D point = GetPointFromHeading(sideHeading, PetSummonDistance);
                    m_pet.X = point.X;
                    m_pet.Y = point.Y;
                    m_pet.Z = Z;

                    m_pet.Heading = Heading;
                    m_pet.Realm = eRealm.None;
                    m_pet.LoadedFromScript = true;
                    m_pet.MaxDistance = PetMaxDistance;
                    m_pet.TetherRange = PetTetherRange;
                    m_pet.RespawnInterval = -1;

                    if (PetSize > 0)
                        m_pet.Size = PetSize;

                    if (PetLevel > 0)
                        m_pet.Level = PetLevel;
                    else
                        m_pet.Level = (byte)(Level * PetLevel / -100);

                    m_pet.AutoSetStats();

                    if (m_pet.Brain is StandardMobBrain petBrain && Brain is StandardMobBrain brain && TargetObject is GameLiving living)
                    {
                        petBrain.CanBAF = false;
                        brain.AddAggroListTo(petBrain);
                    }

                    m_pet.AddToWorld();
                }
            }
        }

        /// <summary>
        /// Release the pet
        /// </summary>
        protected virtual void ReleasePet()
        {
            if (m_pet != null)
            {
                m_pet.RemoveFromWorld();
                m_pet.Delete();
                m_pet = null;
            }
        }
    }
}
