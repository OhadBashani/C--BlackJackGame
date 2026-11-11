using System;
using System.Collections.Generic;

namespace BlackjackGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Blackjack!");
            bool playAgain = true;

            while (playAgain)
            {
                PlayBlackjack();

                Console.WriteLine("\nDo you want to play again? (y/n)");
                string response = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
                playAgain = response == "y";
            }

            Console.WriteLine("Thanks for playing!");
        }

        static void PlayBlackjack()
        {
            Deck deck = new Deck();
            deck.Shuffle();

            Hand dealerHand = new Hand();
            List<Hand> playerHands = new List<Hand> { new Hand() };
            List<int> bets = new List<int> { 10 }; // הימור בסיס ליד
            List<bool> splitAceHands = new List<bool> { false };
            bool hasSplit = false;
            bool playerTookInsurance = false;

            // 1. סדר חלוקה תקין: שחקן -> דילר -> שחקן -> דילר
            playerHands[0].AddCard(deck.Deal());
            dealerHand.AddCard(deck.Deal());
            playerHands[0].AddCard(deck.Deal());
            dealerHand.AddCard(deck.Deal());

            Console.WriteLine();
            Console.WriteLine($"Dealer's hand: {dealerHand.Cards[0]} [Hidden]");
            Console.WriteLine($"Dealer's visible total: {dealerHand.Cards[0].Value}");

            // 2. בדיקת ביטוח (Insurance) אם קלף הדילר הגלוי הוא אס
            if (dealerHand.Cards[0].Rank == "Ace")
            {
                Console.Write("\nDealer shows an Ace. Do you want insurance? (y/n): ");
                string insResponse = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
                if (insResponse == "y")
                {
                    playerTookInsurance = true;
                    Console.WriteLine("Insurance bought!");
                }
            }

            // תור השחקן
            for (int handIndex = 0; handIndex < playerHands.Count; handIndex++)
            {
                Hand currentHand = playerHands[handIndex];

                if (splitAceHands[handIndex])
                {
                    Console.WriteLine($"\nHand {handIndex + 1}: Split Aces receive one card each and must stand.");
                    DisplayPlayerHand(currentHand, handIndex + 1);
                    continue;
                }

                if (currentHand.GetTotalValue() == 21)
                {
                    Console.WriteLine($"\nHand {handIndex + 1} has 21 and automatically stands.");
                    DisplayPlayerHand(currentHand, handIndex + 1);
                    continue;
                }

                while (true)
                {
                    Console.WriteLine();
                    DisplayPlayerHand(currentHand, handIndex + 1);

                    bool canSplit = !hasSplit &&
                                    currentHand.Cards.Count == 2 &&
                                    currentHand.Cards[0].Value == currentHand.Cards[1].Value;

                    bool canDouble = currentHand.Cards.Count == 2;

                    // בניית תפריט דינמי לפי האפשרויות החוקיות
                    List<string> options = new List<string> { "(h)it", "(s)tand" };
                    if (canSplit) options.Add("(p)split");
                    if (canDouble) options.Add("(d)ouble");

                    Console.Write($"Choose {string.Join(", ", options)}: ");
                    string action = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

                    if (action == "h")
                    {
                        currentHand.AddCard(deck.Deal());
                        DisplayPlayerHand(currentHand, handIndex + 1);

                        if (currentHand.GetTotalValue() > 21)
                        {
                            Console.WriteLine($"Hand {handIndex + 1} busted!");
                            break;
                        }

                        if (currentHand.GetTotalValue() == 21)
                        {
                            Console.WriteLine("You have 21. Standing automatically.");
                            break;
                        }
                    }
                    else if (action == "s")
                    {
                        Console.WriteLine($"You stand on {currentHand.GetTotalValue()}.");
                        break;
                    }
                    else if (action == "d" && canDouble)
                    {
                        // 3. הכפלת הימור (Double Down)
                        bets[handIndex] *= 2;
                        Console.WriteLine($"You doubled down! Current bet for this hand: {bets[handIndex]}");
                        currentHand.AddCard(deck.Deal());
                        DisplayPlayerHand(currentHand, handIndex + 1);

                        if (currentHand.GetTotalValue() > 21)
                        {
                            Console.WriteLine($"Hand {handIndex + 1} busted after Double Down!");
                        }
                        else
                        {
                            Console.WriteLine($"You stand on {currentHand.GetTotalValue()}.");
                        }
                        break;
                    }
                    else if (action == "p" && canSplit)
                    {
                        Card firstCard = currentHand.Cards[0];
                        Card secondCard = currentHand.Cards[1];
                        bool splittingAces = firstCard.Rank == "Ace" && secondCard.Rank == "Ace";

                        Hand firstHand = new Hand();
                        Hand secondHand = new Hand();
                        firstHand.AddCard(firstCard);
                        secondHand.AddCard(secondCard);

                        firstHand.AddCard(deck.Deal());
                        secondHand.AddCard(deck.Deal());

                        playerHands[handIndex] = firstHand;
                        playerHands.Insert(handIndex + 1, secondHand);
                        splitAceHands[handIndex] = splittingAces;
                        splitAceHands.Insert(handIndex + 1, splittingAces);
                        bets.Insert(handIndex + 1, bets[handIndex]);
                        hasSplit = true;

                        Console.WriteLine("\nYou split your hand!");
                        DisplayPlayerHand(firstHand, handIndex + 1);
                        DisplayPlayerHand(secondHand, handIndex + 2);

                        if (splittingAces)
                        {
                            Console.WriteLine("Split Aces receive one card each and must stand.");
                            break;
                        }

                        currentHand = firstHand;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please try again.");
                    }
                }
            }

            // תור הדילר
            bool anyHandStillInPlay = playerHands.Exists(hand => hand.GetTotalValue() <= 21);

            if (anyHandStillInPlay)
            {
                while (dealerHand.GetTotalValue() < 17)
                {
                    dealerHand.AddCard(deck.Deal());
                }
            }

            Console.WriteLine("\nDealer's final hand:");
            Console.WriteLine(dealerHand);
            Console.WriteLine($"Dealer total: {dealerHand.GetTotalValue()}");

            // בדיקת תוצאת הביטוח
            bool dealerHasBlackjack = dealerHand.Cards.Count == 2 && dealerHand.GetTotalValue() == 21;

            if (playerTookInsurance)
            {
                Console.WriteLine("\n===== INSURANCE RESULT =====");
                if (dealerHasBlackjack)
                {
                    Console.WriteLine("Dealer has Blackjack! Insurance bet WON!");
                }
                else
                {
                    Console.WriteLine("Dealer does not have Blackjack. Insurance bet LOST.");
                }
            }

            // סיכום התוצאות
            Console.WriteLine("\n===== ROUND RESULTS =====");
            for (int i = 0; i < playerHands.Count; i++)
            {
                int playerTotal = playerHands[i].GetTotalValue();
                int dealerTotal = dealerHand.GetTotalValue();

                Console.WriteLine($"\nHand {i + 1}: {playerHands[i]} (Bet: {bets[i]})");
                Console.WriteLine($"Your total: {playerTotal}");

                if (playerTotal > 21)
                {
                    Console.WriteLine("Bust! Dealer wins this hand.");
                }
                else if (dealerTotal > 21 || playerTotal > dealerTotal)
                {
                    Console.WriteLine("You win this hand!");
                }
                else if (playerTotal < dealerTotal)
                {
                    Console.WriteLine("Dealer wins this hand.");
                }
                else
                {
                    Console.WriteLine("Push! It's a tie.");
                }
            }
        }

        static void DisplayPlayerHand(Hand hand, int handNumber)
        {
            Console.WriteLine($"Hand {handNumber}: {hand}");
            Console.WriteLine($"Total: {hand.GetTotalValue()}");
        }
    }

    class Card
    {
        public string Suit { get; }
        public string Rank { get; }
        public int Value { get; }

        public Card(string suit, string rank, int value)
        {
            Suit = suit;
            Rank = rank;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }

    class Deck
    {
        private readonly List<Card> cards;
        private readonly Random random = new Random();

        public Deck()
        {
            cards = new List<Card>();
            string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
            string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

            foreach (string suit in suits)
            {
                for (int i = 0; i < ranks.Length; i++)
                {
                    int value = i + 2;

                    if (ranks[i] == "Jack" || ranks[i] == "Queen" || ranks[i] == "King")
                    {
                        value = 10;
                    }
                    else if (ranks[i] == "Ace")
                    {
                        value = 11;
                    }

                    cards.Add(new Card(suit, ranks[i], value));
                }
            }
        }

        public void Shuffle()
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                Card temp = cards[i];
                cards[i] = cards[j];
                cards[j] = temp;
            }
        }

        public Card Deal()
        {
            if (cards.Count == 0)
            {
                throw new InvalidOperationException("No cards left in the deck.");
            }

            Card card = cards[0];
            cards.RemoveAt(0);
            return card;
        }
    }

    class Hand
    {
        public List<Card> Cards { get; }

        public Hand()
        {
            Cards = new List<Card>();
        }

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public int GetTotalValue()
        {
            int total = 0;
            int aceCount = 0;

            foreach (Card card in Cards)
            {
                total += card.Value;

                if (card.Rank == "Ace")
                {
                    aceCount++;
                }
            }

            while (total > 21 && aceCount > 0)
            {
                total -= 10;
                aceCount--;
            }

            return total;
        }

        public override string ToString()
        {
            return string.Join(", ", Cards);
        }
    }
}
