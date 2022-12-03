using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{
	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (isTurn)
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
		// Create a list of the indices all possible cards to play
		List<int> playableCards = new List<int>();
		for (int i = 0; i < cardObjects.Count; i++)
		{
			if (cardObjects[i].IsPlayable())
				playableCards.Add(i);
		}

		// Select a random card to play
		Card selectedCard = cardObjects[playableCards[Random.Range(0, playableCards.Count)]];

		// Play the selected card
		PlayCard(selectedCard);

		// Tell everyone which card was played
		EventManager.CardPlayed(new CardInfo(selectedCard.GetFaceValue(), selectedCard.GetSuit()), this);

		// Tell everyone the turn has ended
		EventManager.EndTurn();
	}
}
