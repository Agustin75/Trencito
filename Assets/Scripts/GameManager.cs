using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField]
	private GameObject gameOverPanel, skipButton;

	[Header("Prefabs")]
	[SerializeField]
	private GameObject playerPrefab;
	[SerializeField]
	private GameObject opponentPrefab;
	[SerializeField]
	private GameObject cardPrefab;

	[SerializeField]
	private Deck defaultDeck;
	[SerializeField]
	private Canvas canvas;
	[SerializeField]
	private GameBoard board;

	[Header("Scriptable Objects")]
	[SerializeField]
	private InputStateVariable currInputState;
    [SerializeField]
    private BoolVariable isGameOver;


    private List<Player> players = new();
	private readonly List<PlayerHand> playerHands = new();
	private readonly List<int> winners = new();

	// Array storing the outermost cards for each suit. It goes "Low Club", "Low Diamond", "Low Heart", "Low Spade", "High Club", "High Diamond", "High Heart", "High Spade"
	// If no card of that suit is in play, value will be -1 for both Low and High
	// If 5 is in play, value will be 5 for both Low and High
	private int[] outermostCards = { -1, -1, -1, -1, -1, -1, -1, -1 };
	private int turnPlayer = -1;
	private List<CardInfo> currentDeck;

	//private GameState gameState;


	// Start is called before the first frame update
	void Start()
	{
		EventManager.onCardPlayed += PlaceCard;
		EventManager.onTurnEnd += ChangeTurn;
		EventManager.onLastCardPlayed += PlayerFinished;
		EventManager.onCardFeedbackOver += CardFeedbackShown;
        //gameState = GameState.Playing;

        players = new List<Player>
        {
            Instantiate(playerPrefab, canvas.transform).GetComponent<Player>()
        };
		playerHands.Add(players[^1].GetComponent<PlayerHand>());
		players.Add(Instantiate(opponentPrefab, canvas.transform).GetComponent<Player>());
		playerHands.Add(players[^1].GetComponent<PlayerHand>());

        SetUpGame();

		StartGame();
	}

	// Update is called once per frame
	//void Update()
	//{

	//}

	private void OnDisable()
	{
		EventManager.onCardPlayed -= PlaceCard;
		EventManager.onTurnEnd -= ChangeTurn;
		EventManager.onLastCardPlayed -= PlayerFinished;
		EventManager.onCardFeedbackOver -= CardFeedbackShown;
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

		// Create the new CardObject to play
		// TODO: Change this to use an existing card instead of creating a new one every time?
		Card cardPlayed = Instantiate(cardPrefab).GetComponent<Card>();

		// Initialize the CardObject from the data passed in (Default of Played and no Owner)
		cardPlayed.Initialize(_cardPlayed);

		// Make the card uninteractable while it's on the board
		cardPlayed.SetSelectable(false);

		// TODO: When adding effects, will have to do a playerHand.CheckForValidMove(); here if the player uses a "play 2 cards" effect

		// Game is giving feedback to the player
		currInputState.Value = InputState.ShowingFeedback;

		board.PlaceCard(cardPlayed, _owner);

        cardPlayed.ChangeCardLocation(CardLocation.Played);
    }

    public void CardFeedbackShown()
	{
		// The card has reached the board, change the turn
		// TODO: It's done in a different function to account for the future behavior of using effects that give the player more actions per turn.
		EventManager.EndTurn();
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

        // Move the turn to the next player
        turnPlayer++;
        // If the index goes past the players' size
        if (turnPlayer >= players.Count)
            // Reset back to the first player
            turnPlayer = 0;

        // Tell the new player it's their turn
        players[turnPlayer].SetTurn(true);

		// If the current player already discarded all their hand
		if (winners.Contains(turnPlayer))
		{
			// Check to make sure there is still someone left playing the game so it doesn't continue infinitely
            if (!isGameOver)
            {
                // Skip to the next play
                // TODO: Might need to add feedback?
                ChangeTurn();
            }
        }
		// Activate the skip button if:
		// - The turn player is a person (TODO: Will need to change logic for more than 1 person)
		// - They don't have a card they can play
		else if (turnPlayer == 0 && !playerHands[turnPlayer].HasValidMove)
		{
			skipButton.SetActive(true);
		}
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
			isGameOver.Value = true;
			//gameState = GameState.GameOver;

			// Show the Game Over screen
			gameOverPanel.SetActive(true);
		}
	}

	public void RestartGame()
	{
		// Hide the Game Over panel
		gameOverPanel.SetActive(false);

		// Tell everyone else that the game needs to be restarted
		EventManager.ResetGame();

		// Initialize the field of play
		SetUpGame();

		// Start the game proper
		StartGame();
	}

	//////////////////////////////
	/// HELPER FUNCTIONS
	//////////////////////////////
	// Initializes all variables needed for the game
	private void SetUpGame()
	{
		//gameState = GameState.Playing;
		isGameOver.Value = false;
        outermostCards = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
		
		winners.Clear();

		currentDeck = defaultDeck.GetDeckCopy();

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

        foreach (PlayerHand playerHand in playerHands)
        {
            playerHand.InitialHandSetup();
        }
    }

	// Starts the game proper once everything has been set up
	//	- Made as a different function to wait for everyone to initialize properly on restart
	private void StartGame()
	{
        // TODO: Will need to change this when Networking is implemented and the local player can be a number other than 0
        currInputState.Value = turnPlayer switch
        {
            0 => InputState.WaitingForPlayer,
            _ => InputState.WaitingForOpponent,
        };

        // Tell the turnPlayer its their turn
        players[turnPlayer].SetTurn(true);
	}
}
