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
    private Animation cardPlayAnimation;
    [SerializeField]
    // Where should the 5 cards be placed
    private Transform[] suitPosition;
    [SerializeField]
    // How far up and down from the 5 the cards should be placed
    private int cardStackOffset;

    [Header("Scriptable Objects")]
    [SerializeField]
    protected InputStateVariable currInputState;


    //[SerializeField]
    private List<Player> players = new();
    private readonly List<PlayerHand> playerHands = new();
    private readonly List<int> winners = new();
    private FeedbackType currFeedbackType = FeedbackType.None;

    // Array storing the outermost cards for each suit. It goes "Low Club", "Low Diamond", "Low Heart", "Low Spade", "High Club", "High Diamond", "High Heart", "High Spade"
    // If no card of that suit is in play, value will be -1 for both Low and High
    // If 5 is in play, value will be 5 for both Low and High
    private int[] outermostCards = { -1, -1, -1, -1, -1, -1, -1, -1 };
    private int turnPlayer = -1;
    private List<CardInfo> currentDeck;

    private GameState gameState;

    // Feedback Stuff
    #region CardMovement
    private Card cardPlayed = null;
    private Vector3 startPos, finalPos;
    #endregion

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
        playerHands.Add(players[players.Count - 1].GetComponent<PlayerHand>());
        players.Add(Instantiate(opponentPrefab, canvas.transform).GetComponent<Player>());
        playerHands.Add(players[players.Count - 1].GetComponent<PlayerHand>());

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
                // The 5 of Diamonds starts, so make it this player's turn
                turnPlayer = currentDeck.Count % 2;
            }
        }

        // TODO: Will need to change this when Networking is implemented and the local player can be a number other than 0
        switch (turnPlayer)
        {
            case 0:
                currInputState.Value = InputState.WaitingForPlayer;
                break;
            default:
                currInputState.Value = InputState.WaitingForOpponent;
                break;
        }

        // Tell the turnPlayer its their turn
        players[turnPlayer].SetTurn(true);

        foreach (PlayerHand playerHand in playerHands)
        {
            playerHand.InitialSetup();
        }

        // TODO: Deal cards to every player in the play (Mostly to test stuff out: start with 1, then 2 local, then 2 networked)
    }

    // Update is called once per frame
    void Update()
    {
        if (currInputState == InputState.ShowingFeedback)
        {
            if (currFeedbackType == FeedbackType.CardPlayed)
            {
                PlayCardFeedback();
            }
        }
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
        startPos = _owner.transform.position;

        // Get the position where the card will end up
        finalPos = suitPosition[(int)(_cardPlayed.suit)].position;
        if (_cardPlayed.value > 5)
            finalPos += new Vector3(0, -cardStackOffset, 0);
        else if (_cardPlayed.value < 5)
            finalPos += new Vector3(0, cardStackOffset, 0);

        // Create the new CardObject to play (Put the corresponding suit position as a parent)
        cardPlayed = Instantiate(cardPrefab, suitPosition[(int)_cardPlayed.suit]).GetComponent<Card>();

        // Set the position to startPos
        cardPlayed.transform.position = startPos;

        // Initialize the CardObject from the data passed in (Default of Played and no Owner)
        cardPlayed.Initialize(_cardPlayed);

        // TODO: Play animation of the card moving from the player's hand to the field

        // Make the card uninteractable while it's on the board
        cardPlayed.SetSelectable(false);

        // TODO: When adding effects, will have to do a playerHand.CheckForValidMove(); here if the player uses a "play 2 cards" effect

        // Set the current feedback type
        currFeedbackType = FeedbackType.CardPlayed;

        // Game is giving feedback to the player
        currInputState.Value = InputState.ShowingFeedback;
    }

    public void ChangeTurn()
    {
        // Tell the current turn player his turn is over
        players[turnPlayer].SetTurn(false);

        // It's a new turn, have all players check if they currently have a valid move
        foreach (PlayerHand playerHand in playerHands)
        {
            playerHand.CheckForValidMove();
        }

        // Move to the next player in line, and keep doing it until a player with a card that can be played is found
        // TODO: Temporary, loop will be gone when the button for skipping turn is implemented
        do
        {
            // Move the turn to the next player
            turnPlayer++;
            // If the index goes past the players' size
            if (turnPlayer >= players.Count)
                // Reset back to the first player
                turnPlayer = 0;
            // Continue as long as the game isn't over
            // TODO: Temporary code! Turn player will have to press a button to skip turn in final version, for player feedback
        } while (gameState == GameState.Playing && !playerHands[turnPlayer].HasValidMove);

        // Tell the new player it's their turn
        players[turnPlayer].SetTurn(true);
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

            // Call the Game Over Event
            EventManager.GameOver();
        }
    }

    //////////////////////////////
    /// HELPER FUNCTIONS
    //////////////////////////////
    private void PlayCardFeedback()
    {
        cardPlayed.transform.position = Vector3.Lerp(cardPlayed.transform.position, finalPos, 10 * Time.deltaTime);
        if (Vector3.Distance(cardPlayed.transform.position, finalPos) <= 0.1f)
        {
            cardPlayed.transform.position = finalPos;
            currFeedbackType = FeedbackType.None;
            currInputState.Value = InputState.WaitingForPlayer;
        }
    }
}
