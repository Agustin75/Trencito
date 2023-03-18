using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField]
    // Where should the 5 cards be placed
    private Transform[] suitPosition;
    [SerializeField]
    // How far up and down from the 5 the cards should be placed
    private int cardStackOffset;

    [Header("Scriptable Objects")]
    [SerializeField]
    private InputStateVariable currInputState;

    // Stores all the cards currently on the board (To use for stuff like resetting the game)
    // TODO: Change this and PlayerHand to never instantiate cards? (instead, the cards are all there to begin with, just hidden)
    private List<Card> cardsOnBoard = new();
    private bool playingCard = false;

    // Feedback Stuff
    #region CardMovement
    private Card cardPlayed;
    private Vector3 finalPos;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        EventManager.onGameRestart += BoardReset;
    }

    // Update is called once per frame
    void Update()
    {
        if (playingCard)
        {
            PlayCardFeedback();
        }
    }

    private void OnDisable()
    {
        EventManager.onGameRestart -= BoardReset;
    }

    public void PlaceCard(Card _cardPlayed, Player _owner)
    {
        // Save the current card played
        cardPlayed = _cardPlayed;

        // Get the position where the card will start moving from (The player's hand location)
        Vector3 startPos = _owner.transform.position;

        // Get the position where the card will end up
        finalPos = suitPosition[(int)(cardPlayed.GetSuit())].position;
        if (cardPlayed.GetFaceValue() > 5)
            finalPos += new Vector3(0, -cardStackOffset, 0);
        else if (cardPlayed.GetFaceValue() < 5)
            finalPos += new Vector3(0, cardStackOffset, 0);

        // Change the card's parent
        cardPlayed.transform.parent = suitPosition[(int)cardPlayed.GetSuit()];

        // Set the position to startPos
        cardPlayed.transform.position = startPos;

        // Make the card uninteractable while it's on the board
        cardPlayed.SetSelectable(false);

        // Save the new card played
        cardsOnBoard.Add(cardPlayed);

        playingCard = true;
    }

    /////////////////////////////////////////////////
    // Custom Event Functions
    /////////////////////////////////////////////////
    public void BoardReset()
    {
        foreach (Card boardCard in cardsOnBoard)
        {
            Destroy(boardCard.gameObject);
        }
        cardsOnBoard.Clear();
    }

    //////////////////////////////
    /// HELPER FUNCTIONS
    //////////////////////////////
    // Moves the card from a player's hand to the board
    private void PlayCardFeedback()
    {
        cardPlayed.transform.position = Vector3.Lerp(cardPlayed.transform.position, finalPos, 10 * Time.deltaTime);
        if (Vector3.Distance(cardPlayed.transform.position, finalPos) <= 0.1f)
        {
            cardPlayed.transform.position = finalPos;
            playingCard = false;
            currInputState.Value = InputState.WaitingForPlayer;

            // Tell everyone the card has reached the board
            EventManager.CardFeedbackShown();
        }
    }
}
