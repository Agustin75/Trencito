using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
	[Header("Prefabs")]
	[SerializeField]
	protected GameObject cardPrefab;

	protected List<Card> cardObjects = new List<Card>();
	protected bool isTurn = false;
	// Variable to know if the player has a valid move for the turn
	protected bool canPlay = false;

	// Start is called before the first frame update
	void Start()
    {

	}

    // Update is called once per frame
    void Update()
    {

	}

	public virtual PlayerType GetPlayerType()
	{
		return PlayerType.Player;
	}

	// Returns whether the player can make a move
	public bool CanPlay()
	{
		return canPlay;
	}

	public void SetTurn(bool _isTurn)
	{
		isTurn = _isTurn;
		// Set canPlay to false by default
		canPlay = false;

		// Loop through all the cards owned
		foreach (Card card in cardObjects)
		{
			// Tell the card whether it's the owner's turn
			card.TurnChanged(isTurn);

			// If it's the player's turn, no card has been playable so far, and this card is playable
			if (_isTurn && !canPlay && card.IsPlayable())
				// The player has a move they can make
				canPlay = true;
		}
	}

	// Called in the initialization to sort the Cards GameObjects visually
	public void SortHand()
	{
		for (int i = 0; i < cardObjects.Count; i++)
		{
			// SetSiblingIndex changes where in the list of children the GameObject is in the hierarchy, so they show up in the right order
			cardObjects[i].transform.SetSiblingIndex(i);
		}
	}

	// Add the card to the player's hand and sort the array
	public void AddCard(CardInfo _newCard)
	{
		// Index where the card will be placed
		int indexToAdd = 0;
		// Loop through the existing hand until we find the spot where the card goes or we reach the end
		for (; indexToAdd < cardObjects.Count; indexToAdd++)
		{
			// If the suit in the index is "higher" than the suit of the card, then exit the loop (Suits are sorted arbitrarily based on their order in the Suits enum)
			if ((int)cardObjects[indexToAdd].GetSuit() > (int)_newCard.suit)
				break;
			// If the suit is the same, but the number in the index is higher
			if ((int)cardObjects[indexToAdd].GetSuit() == (int)_newCard.suit && cardObjects[indexToAdd].GetFaceValue() > _newCard.value)
				break;
		}

		// Spawn the hard from the hand
		Card newCard = Instantiate(cardPrefab, transform).GetComponent<Card>();
		// Assign the values to the spawned card
		newCard.Initialize(_newCard, CardLocation.OnHand, this);

		// If the card is the 5 of Diamonds, it will be the starting card, so make it playable and selectable
		if (_newCard.value == 5 && _newCard.suit == Suits.Diamonds)
		{
			newCard.SetPlayableState(true);
			newCard.SetSelectable(true);
			// The 5 of Diamonds starts, so make it this player's turn
			SetTurn(true);
		}

		// Add the card to the cardObjects array
		cardObjects.Insert(indexToAdd, newCard);
	}

	// TODO: when a card is selected (that can be selected), mark it and/or zoom on it and (display the option to) play (or activate) it
	// - A Setting will decide if the card is played automatically upon being pressed or if it asks for confirmation first (for regular gameplay)
	public void PlayCard(Card _cardPlayed)
	{
		// Remove the card object from this player's hand
		cardObjects.Remove(_cardPlayed);

		// Remove the card from the hand (a new one will be instantiated for playing)
		Destroy(_cardPlayed.gameObject);

		// If there are no cards left in the player's hand
		if (cardObjects.Count == 0)
		{
			// Let everyone know
			EventManager.PlayedLastCard(this);
		}
	}
}
