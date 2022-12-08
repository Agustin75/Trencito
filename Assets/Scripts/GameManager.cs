using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject opponentPrefab;
    [SerializeField]
    protected GameObject cardPrefab;

    [SerializeField]
    private Deck defaultDeck;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    // Where should the 5 cards be placed
    private Transform[] suitPosition;
    [SerializeField]
    // How far up and down from the 5 the cards should be placed
    private int cardStackOffset;
    //[SerializeField]
    private List<Player> players = new List<Player>();
    private List<int> winners = new List<int>();

    // Array storing the outermost cards for each suit. It goes "Low Club", "Low Diamond", "Low Heart", "Low Spade", "High Club", "High Diamond", "High Heart", "High Spade"
    // If no card of that suit is in play, value will be -1 for both Low and High
    // If 5 is in play, value will be 5 for both Low and High
    private int[] outermostCards = { -1, -1, -1, -1, -1, -1, -1, -1 };
    private int turnPlayer = -1;
    private List<CardInfo> currentDeck;

    private GameState gameState;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.onCardPlayed += PlaceCard;
        EventManager.onTurnEnd += ChangeTurn;
        EventManager.onLastCardPlayed += PlayerFinished;
        players = new List<Player>();
        gameState = GameState.Playing;

        currentDeck = defaultDeck.GetDeckCopy();
        players.Add(Instantiate(playerPrefab, canvas.transform).GetComponent<Player>());
        players.Add(Instantiate(opponentPrefab, canvas.transform).GetComponent<Player>());

        // TODO: Temporary behavior. It deals the entire deck to 2 players
        while (currentDeck.Count > 0)
        {
            // Get a random spot on the deck
            int randomInd = Random.Range(0, currentDeck.Count);
            // Instead of shuffling the deck, draw a random card from the deck
            CardInfo drawnCard = currentDeck[randomInd];
            // Remove the card pulled from the deck
            currentDeck.RemoveAt(randomInd);
            // Add the card to the hand of each player
            players[currentDeck.Count % 2].AddCard(drawnCard);

            // Make the player with the 5 of Diamonds the first player
            if (drawnCard.value == 5 && drawnCard.suit == Suits.Diamonds)
            {
                turnPlayer = currentDeck.Count % 2;
            }
        }

        foreach (Player player in players)
        {
            player.SortHand();
        }

        // TODO: Deal cards to every player in the play (Mostly to test stuff out: start with 1, then 2 local, then 2 networked)
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDisable()
    {
        EventManager.onCardPlayed -= PlaceCard;
        EventManager.onTurnEnd -= ChangeTurn;
        EventManager.onLastCardPlayed -= PlayerFinished;
    }

    /////////////////////////////////////////////////
    // Custom Event Functions
    /////////////////////////////////////////////////
    public void PlaceCard(CardInfo _cardPlayed, Player _owner)
    {
        // If the card is a 5 or below
        if (_cardPlayed.value <= 5)
            // Update the low border for the card's suit
            outermostCards[(int)(_cardPlayed.suit)] = _cardPlayed.value;
        // If the card is a 5 or above
        if (_cardPlayed.value >= 5)
            // Update the high border for the card's suit
            outermostCards[(int)(_cardPlayed.suit) + sizeof(Suits)] = _cardPlayed.value;

        // Get the position where the card will start moving from (The player's hand location)
        Vector3 startPos = _owner.transform.position;

        // Get the position where the card will end up
        Vector3 finalPos = suitPosition[(int)(_cardPlayed.suit)].position;
        if (_cardPlayed.value > 5)
            finalPos += new Vector3(0, -cardStackOffset, 0);
        else if (_cardPlayed.value < 5)
            finalPos += new Vector3(0, cardStackOffset, 0);

        // Create the new CardObject to play
        Card cardPlayed = Instantiate(cardPrefab).GetComponent<Card>();
        
        // TODO: Set the position to startPos

        // Initialize the CardObject from the data passed in (Default of Played and no Owner)
        cardPlayed.Initialize(_cardPlayed);

        // TODO: Play animation of the card moving from the player's hand to the field

        // Manually placing the card on the field
        cardPlayed.transform.position = finalPos;

        // Make the card uninteractable while it's on the board
        cardPlayed.SetSelectable(false);

        // Change the card's transform parent to the appropiate position's transform
        cardPlayed.transform.parent = suitPosition[(int)_cardPlayed.suit];
    }

    public void ChangeTurn()
    {
        do
        {
            // Tell the current turn player his turn is over
            players[turnPlayer].SetTurn(false);

            // Move the turn to the next player
            turnPlayer++;
            // If the index goes past the players' size
            if (turnPlayer >= players.Count)
                // Reset back to the first player
                turnPlayer = 0;

            // Tell the new player it's their turn
            players[turnPlayer].SetTurn(true);

            // Continue as long as the game isn't over
            // TODO: Temporary code! Turn player will have to press a button to skip turn in final version, for player feedback
        } while (gameState == GameState.Playing && !players[turnPlayer].CanPlay());
    }

    // A player played their last card
    public void PlayerFinished(Player _player)
    {
        // Add the player to the list of players that finished
        winners.Add(players.IndexOf(_player));

        // If all players have played all their cards, the game is over
        if (winners.Count == players.Count)
        {
            // Change the game to over
            gameState = GameState.GameOver;

            // TODO: Handle Game Over behavior
        }
    }
}
