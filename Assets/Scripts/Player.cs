using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField]
    protected PlayerHand playerHand;

    [Header("Scriptable Objects")]
    [SerializeField]
    protected InputStateVariable currInputState;
    [SerializeField]
    protected BoolVariable isGameOver;

    protected bool isTurn = false;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.onGameRestart += InitializePlayer;
    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    private void OnDisable()
    {
        EventManager.onGameRestart -= InitializePlayer;
    }

    public virtual PlayerType GetPlayerType()
    {
        return PlayerType.Player;
    }

    public void SetTurn(bool _isTurn)
    {
        if (isTurn != _isTurn)
        {
            isTurn = _isTurn;
            playerHand.UpdateCardsDisplay(isTurn);
        }
    }

    // Add the card to the player's hand
    public void AddCard(CardInfo _newCard)
    {
        playerHand.AddCard(_newCard, this);
    }

    // TODO: when a card is selected (that can be selected), mark it and/or zoom on it and (display the option to) play (or activate) it
    // - A Setting will decide if the card is played automatically upon being pressed or if it asks for confirmation first (for regular gameplay)
    public void PlayCard(Card _cardPlayed)
    {
        // If there are no cards left in the player's hand
        if (!playerHand.PlayCard(_cardPlayed))
        {
            // Let everyone know
            EventManager.PlayedLastCard(this);
        }
    }

    /////////////////////////////////////////////////
    // Custom Event Functions
    /////////////////////////////////////////////////
    public void InitializePlayer()
    {
        isGameOver.Value = false;
        isTurn = false;
        SetTurn(false);
    }
}
