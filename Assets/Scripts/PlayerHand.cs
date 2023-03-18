using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHand : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    protected GameObject cardPrefab;

    [Header("Components")]
    [SerializeField]
    private HorizontalLayoutGroup horizontalGroup;

    private float cardWidth = 0;
    private List<Card> cardObjects = new();
    private int firstEmptyIndex = 0, cardsInHand = 0;
    // Variable to know if the there is currently a valid move to be made
    private bool _hasValidMove = false;
    public bool HasValidMove { get { return _hasValidMove; } }


    // Start is called before the first frame update
    void Start()
    {
        EventManager.onGameRestart += Initialize;
    }

    // Called after all cards have been instantiated
    public void InitialHandSetup()
    {
        if (cardWidth == 0)
            // Initialize the card width
            cardWidth = cardObjects[0].GetComponent<RectTransform>().rect.width;

        // Sort the card gameobjects to line up with the saved array
        SortHand();
        // Fix up the spacing between cards so they're all visible
        UpdateSpacing();
    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    private void OnDisable()
    {
        EventManager.onGameRestart -= Initialize;
    }

    public int GetCurrentSize() { return cardsInHand; }

    // Add the card to the player's hand and sort the array
    public void AddCard(CardInfo _newCard, Player _owner)
    {
        // Bool to know if the card needs to be added to the array (either because a new card was instantiated or because it was removed to sort it)
        bool insertCardIntoArray = false;

        // Index where the card will be placed
        int indexToAdd = 0;

        // Loop through the existing hand until we find the spot where the card goes or we reach the end
        for (; indexToAdd < cardObjects.Count && cardObjects[indexToAdd].gameObject.activeInHierarchy; indexToAdd++)
        {
            // If the suit in the index is "higher" than the suit of the card, then exit the loop (Suits are sorted arbitrarily based on their order in the Suits enum)
            if ((int)cardObjects[indexToAdd].GetSuit() > (int)_newCard.suit)
                break;
            // If the suit is the same, but the number in the index is higher
            if ((int)cardObjects[indexToAdd].GetSuit() == (int)_newCard.suit && cardObjects[indexToAdd].GetFaceValue() > _newCard.value)
                break;
        }

        Card newCard;

        if (firstEmptyIndex >= cardObjects.Count)
        {
            // Spawn the card from the hand
            newCard = Instantiate(cardPrefab, transform).GetComponent<Card>();
    
            insertCardIntoArray = true;
        }
        else
        {
            // Grab the first hidden card
            newCard = cardObjects[firstEmptyIndex];

            // If the card is not added in the correct location
            if (firstEmptyIndex != indexToAdd)
            {
                // Remove the card from the array temporarily, to sort it
                cardObjects.RemoveAt(firstEmptyIndex);

                insertCardIntoArray = true;
            }

            // Make the card object visible
            newCard.gameObject.SetActive(true);
        }

        if (insertCardIntoArray)
        {
            // Add the card in the new location
            cardObjects.Insert(indexToAdd, newCard);
        }

        // Move the first empty slot 1 further
        firstEmptyIndex++;

        // Assign the values to the card
        newCard.Initialize(_newCard, CardLocation.OnHand, _owner);

        // If the card is the 5 of Diamonds, it will be the starting card, so make it playable and selectable
        if (_newCard.value == 5 && _newCard.suit == Suits.Diamonds)
        {
            newCard.SetPlayableState(true);
            newCard.SetSelectable(true);
            _hasValidMove = true;
        }

        // A card was added to the hand
        cardsInHand++;
    }

    // Returns false if the player has 0 cards in hand, true otherwise
    // TODO: when a card is selected (that can be selected), mark it and/or zoom on it and (display the option to) play (or activate) it
    // - A Setting will decide if the card is played automatically upon being pressed or if it asks for confirmation first (for regular gameplay)
    public bool PlayCard(Card _cardPlayed)
    {
        // Hide the card from the player's hand
        _cardPlayed.gameObject.SetActive(false);
        
        // A card was removed from the hand
        cardsInHand--;

        UpdateSpacing();

        // Return whether the player is still playing
        return GetCurrentSize() != 0;
    }

    // Checks if the player has a playable card and saves the answer
    public void CheckForValidMove()
    {
        // Set _hasValidMove to false by default
        _hasValidMove = false;

        // Loop through all the cards owned
        foreach (Card card in cardObjects)
        {
            // If no valid move has been found so far, and this card is playable
            if (!_hasValidMove && card.IsPlayable())
                // The player has a move they can make
                _hasValidMove = true;
        }
    }

    // Tells the cards that the turn has changed
    public void UpdateCardsDisplay(bool _isTurn)
    {
        // Loop through all the cards owned
        foreach (Card card in cardObjects)
        {
            // Tell the card whether it's the owner's turn
            card.TurnChanged(_isTurn);
        }
    }

    ///////////////////////////////////
    /// AI FUNCTIONS
    ///////////////////////////////////
    // Gets a random playable card
    public Card GetRandomPlayableCard()
    {
        // Create a list of the indices all possible cards to play
        List<int> playableCards = new();
        for (int i = 0; i < cardObjects.Count; i++)
        {
            if (cardObjects[i].IsPlayable())
                playableCards.Add(i);
        }

        // Select a random card to play
        return playableCards.Count == 0 ? null : cardObjects[playableCards[Random.Range(0, playableCards.Count)]];
    }

    /////////////////////////////////////////////////
    // Custom Event Functions
    /////////////////////////////////////////////////
    public void Initialize()
    {
        firstEmptyIndex = cardsInHand = 0;
    }

    ///////////////////////////////////
    /// HELPER FUNCTIONS
    ///////////////////////////////////
    // Called in the initialization to sort the Cards GameObjects visually
    private void SortHand()
    {
        for (int i = 0; i < cardObjects.Count && cardObjects[i].isActiveAndEnabled; i++)
        {
            // SetSiblingIndex changes where in the list of children the GameObject is in the hierarchy, so they show up in the right order
            cardObjects[i].transform.SetSiblingIndex(i);
        }
    }

    // Updates the spacing between the cards in the Horizontal Layout Group so they're always visible
    private void UpdateSpacing()
    {
        // Find out the difference in width of all the cards and the space to display them
        float widthDifference = cardsInHand * cardWidth - GetComponent<RectTransform>().rect.width;
        // Update the Horizontal Layout Group's spacing so all the cards are displayed
        // (If the widthDifference is positive, then all cards fit in the screen, so the spacing is set to 0)
        horizontalGroup.spacing = widthDifference > 0 ? -widthDifference / cardsInHand : 0;
    }
}
