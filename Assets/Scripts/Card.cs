using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image cardImage;

    [SerializeField]
    private AssetManager assetManager;

    [Header("Scriptable Objects")]
    [SerializeField]
    protected InputStateVariable currInputState;


    private CardInfo info;
    private CardLocation location;
    // Player that currently own the Card (Will be null if the card is in the board)
    private Player owner = null;
    private bool playable = false;
    private bool selectable = false;
    // Variable used to know when the Card should become selectable by the player
    private bool waitingForFeedback = false;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.onCardPlayed += NewCardPlayed;
        SetSelectable(selectable);
    }

    public void Initialize(CardInfo _info, CardLocation _location = CardLocation.Played, Player _owner = null)
    {
        info = _info;
        if (_owner != null && _owner.GetPlayerType() != PlayerType.Player)
        {
            cardImage.sprite = assetManager.GetCardBack();
        }
        else
        {
            cardImage.sprite = assetManager.GetCardSprite(info.value, info.suit);
        }

        ChangeCardLocation(_location, _owner);
        SetPlayableState(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (waitingForFeedback && currInputState != InputState.ShowingFeedback)
        {
            SetSelectable(true);
            waitingForFeedback = false;
        }
    }

    private void OnDisable()
    {
        EventManager.onCardPlayed -= NewCardPlayed;
    }

    public int GetFaceValue()
    {
        return info.value;
    }

    public Suits GetSuit()
    {
        return info.suit;
    }

    public Player GetOwner()
    {
        return owner;
    }

    public bool IsPlayable()
    {
        return playable;
    }

    // Change whether the card can currently be played from the hand
    public void SetSelectable(bool _selectable)
    {
        selectable = _selectable;
        cardImage.color = selectable ? Color.white : Color.gray;
    }

    // Change whether the card can currently be played from the hand
    public void SetPlayableState(bool _playable)
    {
        playable = _playable;
    }

    public void ChangeCardLocation(CardLocation _newLocation, Player _newOwner = null)
    {
        location = _newLocation;
        owner = _newOwner;
    }

    // Handles what making the card un/selectable when the turn changes
    public void TurnChanged(bool _ownerTurn)
    {
        // The card can't be played, no need to change its display
        if (!playable)
            return;

        SetSelectable(false);

        // If this Card is selectable by the player, it's waiting for the card played animation to finish to become selectable
        waitingForFeedback = _ownerTurn && owner.GetPlayerType() == PlayerType.Player;

        //// If it's currently the owner's turn, make the card selectable
        //SetSelectable(_ownerTurn && owner.GetPlayerType() != PlayerType.AIPlayer);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Don't take input if the card is on the board or it's not playable
        if (owner == null || !selectable)
            return;

        // Tell the owner this card was played
        owner.PlayCard(this);

        // Call the onCardPlayed event
        EventManager.CardPlayed(info, owner);

        // Tell everyone the turn has ended
        EventManager.EndTurn();
    }

    /////////////////////////////////////////////////
    // Custom Event Functions
    /////////////////////////////////////////////////
    public void NewCardPlayed(CardInfo _cardPlayedInfo, Player _owner)
    {
        if (owner == null || _cardPlayedInfo == info)
            return;

        if (_cardPlayedInfo.suit != info.suit)
        {
            if (_cardPlayedInfo.suit != Suits.Diamonds || _cardPlayedInfo.value != 5)
                return;
            if (info.value != 5)
                return;
        }
        else if (_cardPlayedInfo.value != info.value + 1 && _cardPlayedInfo.value != info.value - 1)
            return;

        SetPlayableState(true);
    }
}
