using System;
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
    /* Maintains decks and cards for all players */
    public class CardMgr
    {
        /* A single card */
        private class Card
        {
            private uint m_id;
            private bool m_faceup;
            private string m_name;
            private GameClient m_dealer;       

            public bool Init(uint num, bool up, GameClient dealer)
            {
                if(num < 0 || num > 51) return false;
                m_id = num;
                m_faceup = up;
                m_dealer = dealer;
                m_name = GetCardName(num);
                return true;
            }

            public uint Id
            {
                get { return m_id; }
            }

            public bool Up
            {
                get { return m_faceup; }
                set { m_faceup = value; }
            }

            public string Name
            {
                get { return m_name; }
            }

            public GameClient Dealer
            {
                get { return m_dealer; }
            }

            private string GetCardName(uint num)
            {
                string res = null;
                switch(num%13)
                {
                    case 0: res = "Ace of "; break;
                    case 1: res = "Two of "; break;
                    case 2: res = "Three of "; break;
                    case 3: res = "Four of "; break;
                    case 4: res = "Five of "; break;
                    case 5: res = "Six of "; break;
                    case 6: res = "Seven of "; break;
                    case 7: res = "Eight of "; break;
                    case 8: res = "Nine of "; break;
                    case 9: res = "Ten of "; break;
                    case 10: res = "Jack of "; break;
                    case 11: res = "Queen of "; break;
                    case 12: res = "King of "; break;
                }
                switch (num/13)
                {
                    case 0: res += "Hearts"; return res;
                    case 1: res += "Diamonds"; return res;
                    case 2: res += "Clubs"; return res;
                    case 3: res += "Spades"; return res;
                }
                return "CARD ERROR OMG!";
            }
        };

        /* Maintains deck(s) of cards for a dealer */
        private class DealerDeck
        {
            private GameClient m_dealer;
            private uint m_numCards;
            private Queue m_Cards;

            public bool Init(GameClient Dealer, uint numDecks)
            {
                int i,j,tmp,swap;
                uint [] cards;
                Card  c;

                if(numDecks < 1) return false;

                /* Initialize Member Variables */
                m_dealer = Dealer;
                m_numCards = numDecks * 52;

                /* Initialize the array of 'Cards' and the card queue */
                try{ cards = new uint[52*numDecks]; m_Cards = new Queue((int)(52*numDecks));}
                catch(Exception) { return false; }

                /* Initialize card IDs for numDecks */
                for(i=0; i<numDecks; i++)
                for(j=0; j<52; j++)
                cards[i*52+j] = (uint)j;

                /* Decks have been initialized, shuffle them */
                j = (int)m_numCards;
                for(i=0; i<m_numCards; i++)
                {
                    swap = Util.Random(j - 1);
                    tmp = (int)cards[i];
                    cards[i] = cards[swap];
                    cards[swap] = (uint)tmp;
                }

                /* Place the cards in Queue */
                for(i=0; i<m_numCards; i++)
                {
                    c = new Card();
                    c.Init(cards[i], false, m_dealer);
                    m_Cards.Enqueue(c);
                }

                /* OK */
                return true;
            }

            public bool HasCard()
            {
                return m_Cards.Count > 0; 
            }    

            public Card GetCard()
            {
                Card card;
                try { card = (Card)m_Cards.Dequeue(); }
                catch(Exception) { return null; }
                return card;
            }

            public bool ReturnCard(Card card)
            {
                m_Cards.Enqueue(card);
                return true;
            }

            public GameClient Dealer
            {
                get { return m_dealer; }
            }
        };

        /* Maintains the hand of cards for a player */
        private class PlayerHand
        {    
            private GameClient m_owner;
            private ArrayList m_hand;

            public PlayerHand(GameClient Player)
            {
                m_owner = Player;
                m_hand = new ArrayList();
            }

            public void AddCard(Card c, bool up)
            {
                c.Up = up;
                m_hand.Add(c);
            }

            public void Show(GameClient Player)
            {
                foreach(Card c in m_hand)
                {
                    c.Up = true;
                }
            }

            public void Held(GameClient source)
            {
                if (m_hand.Count == 0)
                {
                    source.Out.SendMessage((source == m_owner ? "You have " : m_owner.Player.Name + " has ") + "no cards.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
                string cards = "";
                foreach (Card c in m_hand)
                    if(source == m_owner || c.Up) 
                        cards += c.Id + " - " + c.Name + "\n";
                source.Out.SendMessage((source == m_owner ? "You are holding " : m_owner.Player.Name + " is holding ") + m_hand.Count + (m_hand.Count > 1 ? " cards." : " card."), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                source.Out.SendMessage(cards, eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }

            public Card Discard(uint selection)
            {
                foreach (Card c in m_hand)
                {
                    if (c.Id == selection)
                    {
                        m_hand.Remove(c);
                        if (m_owner.Player.Group != null)
                        {
                            foreach (GamePlayer Groupee in m_owner.Player.Group.GetPlayersInTheGroup())
                            {
                                if(Groupee == m_owner.Player) m_owner.Out.SendMessage("You discard the " + c.Name + " from your hand.", eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                                else Groupee.Client.Out.SendMessage(m_owner.Player.Name + " discards " + (c.Up ? "the " + c.Name : "a card") + " from their hand.", eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                            }
                        }
                        else
                        {
                            m_owner.Out.SendMessage("You cannot play cards without a group!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            DiscardAll();
                            return null;
                        }
                        return c;
                    }
                }
                m_owner.Out.SendMessage("No card with ID " + selection + " exists in your hand!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return null;
            }

            public ArrayList DiscardAll()
            {
                ArrayList cards = (ArrayList)m_hand.Clone();
                m_hand.Clear();
                if(m_owner.Player.Group == null)
                    m_owner.Out.SendMessage("You discard all your cards.", eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                else
                    foreach(GamePlayer Groupee in m_owner.Player.Group.GetPlayersInTheGroup())
                        Groupee.Client.Out.SendMessage((Groupee.Client == m_owner ? "You discard all your cards." : m_owner.Player + " discards all their cards."), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                return cards;
            }
        };

        private static Hashtable m_dealerDecks = new Hashtable();
        private static Hashtable m_playerHands = new Hashtable();

        /* Returns the GameClient which is the designated dealer for Player's group */
        private static GameClient GroupDealer(GameClient player)
        {
            GameClient Dealer = null;
            if (player.Player.Group == null) return null;
            foreach(GamePlayer Groupee in player.Player.Group.GetPlayersInTheGroup())
                if(IsDealer(Groupee.Client)) Dealer = Groupee.Client;
            return Dealer;
        }

        /* True if Player is the dealer for his group */
        public static bool IsDealer(GameClient player)
        {
            return m_dealerDecks.ContainsKey(player.Player.PlayerCharacter.ObjectId);
        }

        /* True if Player already has a hand in progress */
        public static bool IsPlayer(GameClient player)
        {
            return m_playerHands.ContainsKey(player.Player.PlayerCharacter.ObjectId);
        }

        /* Removes dealer rights from the player */
        public static void QuitDealing(GameClient player)
        {
            if(IsDealer(player))
            {
                m_dealerDecks.Remove(player.Player.PlayerCharacter.ObjectId);
            }
        }

        /* Removes the player's hand from the manager, use when player leaves the server! */
        public static void QuitPlaying(GameClient player)
        {
            if (IsPlayer(player)) m_playerHands.Remove(player.Player.PlayerCharacter.ObjectId);
        }

        /* Makes the player the group dealer and prepares the decks */
        /* A new shuffle causes all group members to discard their hands */
        public static void Shuffle(GameClient player, uint numDecks)
        {
            if (player.Player.Group == null)
            {
                player.Out.SendMessage("You must have a group to play cards!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            GameClient Dealer = GroupDealer(player);
            DealerDeck newDecks;

            /* First clear out any previous decks for the group */
            if(Dealer != null)
            {
                QuitDealing(Dealer);
            }

            newDecks = new DealerDeck();
            if(!newDecks.Init(player, numDecks)) { return; }
            try
            {
                if (player.Player.Group == null) return;
                m_dealerDecks.Add(player.Player.PlayerCharacter.ObjectId, newDecks);
                foreach (GamePlayer Groupee in player.Player.Group.GetPlayersInTheGroup())
                {
                    DiscardAll(Groupee.Client);
                    if (Groupee == player.Player) player.Out.SendMessage("You shuffle " + numDecks + (numDecks > 1 ? " decks " : " deck ") + "of cards.", eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                    else Groupee.Client.Out.SendMessage(player.Player.Name + " shuffles " + numDecks + (numDecks > 1 ? " decks " : " deck ") + "of cards.", eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                }
            }
            catch(Exception)
            {
                return;
            }
        }

        /* Deals a card from Dealer to Player */
        public static void Deal(GameClient dealer, GameClient player, bool up)
        {
            PlayerHand hand;
            DealerDeck deck;
            Card c;

            if (!dealer.Player.Group.IsInTheGroup(player.Player))
            { dealer.Out.SendMessage(player.Player.Name + " must be in your group to play cards!", eChatType.CT_System, eChatLoc.CL_SystemWindow); return; }
            if(!IsDealer(dealer))
            { dealer.Out.SendMessage("You must use /shuffle to prepare cards before dealing!", eChatType.CT_System, eChatLoc.CL_SystemWindow); return; }
            if(!IsPlayer(player))
            {
                hand = new PlayerHand(player);
                m_playerHands.Add(player.Player.PlayerCharacter.ObjectId, hand);
            }
            else
            {
                hand = (PlayerHand)m_playerHands[player.Player.PlayerCharacter.ObjectId];
            }
            deck = (DealerDeck)m_dealerDecks[dealer.Player.PlayerCharacter.ObjectId];
            if(!deck.HasCard()) { return; }
            c = deck.GetCard();
            if (c == null)
            {
                dealer.Out.SendMessage("There are no cards left in the deck. Use /shuffle to prepare a new deck.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            hand.AddCard(c, up);
            foreach (GamePlayer Groupee in dealer.Player.Group.GetPlayersInTheGroup())
            {
                if (Groupee == dealer.Player)
                    dealer.Out.SendMessage("You deal " + (player == dealer ? "yourself" : player.Player.Name) + (up ? " the " + c.Name : " a card face down") + ".", eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                else if (Groupee == player.Player)
                    player.Out.SendMessage(dealer.Player.Name + " deals you the " + c.Name + ".", eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                else
                    Groupee.Client.Out.SendMessage(dealer.Player.Name + " deals " + (player == dealer ? "themself" : player.Player.Name) + (up ? " the " + c.Name : " a card face down") + ".", eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
            }
            return;
        }

        /* Returns a string of the cards held by Target as requested by Source */
        public static void Held(GameClient source, GameClient target)
        { 
            if(!IsPlayer(target)) 
            {
                source.Player.Out.SendMessage((source == target ? "You have" : target.Player.Name + " has") + " no cards.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            (m_playerHands[target.Player.PlayerCharacter.ObjectId] as PlayerHand).Held(source);
            return;
        }

        /* Show Player's hand to their group */
        public static void Show(GameClient player)
        {
            if (player.Player.Group == null) return;
            if (!IsPlayer(player)) { player.Out.SendMessage("You have no cards.", eChatType.CT_System, eChatLoc.CL_SystemWindow); return; }

            foreach (GamePlayer Groupee in player.Player.Group.GetPlayersInTheGroup())
            {
                if (Groupee == player.Player)
                    player.Out.SendMessage("You show your hand.", eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                else
                    Groupee.Client.Out.SendMessage(player.Player.Name + " shows their hand.", eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
            } 
        }

        /* Player discards selection, returning it back to the bottom of group's deck */
        public static void Discard(GameClient player, uint selection)
        {
            Card c = null;
            if (!IsPlayer(player))
            { player.Out.SendMessage("You have no cards to discard.", eChatType.CT_System, eChatLoc.CL_SystemWindow); return; }
            c = (m_playerHands[player.Player.PlayerCharacter.ObjectId] as PlayerHand).Discard(selection);
            if (c != null)
            {
                if(IsDealer(c.Dealer)) (m_dealerDecks[c.Dealer.Player.PlayerCharacter.ObjectId] as DealerDeck).ReturnCard(c);
            }
        }

        /* Players discards entire hand, returning to the bottom of group's deck */
        public static void DiscardAll(GameClient player)
        {
            GameClient Dealer = null;
            DealerDeck deck = null;
            if (!IsPlayer(player)) return;
            if ((Dealer = GroupDealer(player)) != null) deck = (DealerDeck)m_dealerDecks[Dealer.Player.PlayerCharacter.ObjectId];
            foreach (Card c in (m_playerHands[player.Player.PlayerCharacter.ObjectId] as PlayerHand).DiscardAll())
                if(deck != null && c.Dealer == Dealer) deck.ReturnCard(c);
            return;
        }
    }
}