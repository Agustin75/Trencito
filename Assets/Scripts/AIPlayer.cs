using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{
    // Update is called once per frame
    void Update()
    {
        if (!isGameOver && isTurn && currInputState != InputState.ShowingFeedback)
        {
            TakeTurn();
        }
    }

    public override PlayerType GetPlayerType()
    {
        return PlayerType.AIPlayer;
    }

    public void TakeTurn()
    {
        // TODO: Show feedback when AI's turn is skipped
        if (playerHand.HasValidMove)
        {
            // Select a random card to play
            Card selectedCard = playerHand.GetRandomPlayableCard();

            // Play the selected card
            PlayCard(selectedCard);

            // Tell everyone which card was played
            EventManager.CardPlayed(new CardInfo(selectedCard.GetFaceValue(), selectedCard.GetSuit()), this);
        }
        else
        {
            // TODO: Change this to trigger after giving the "Skipped turn" feedback if no valid move was available
            // Need to tell everyone the turn has ended, as no card will be played
            EventManager.EndTurn();
        }
    }
}
