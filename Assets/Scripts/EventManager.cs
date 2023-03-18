using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    // Event called when a card is played
    public static event Action<CardInfo, Player> onCardPlayed;
    // Event called when a card is finished displaying its feedback (reach the board for played, effect particles for activated)
    public static event Action onCardFeedbackOver;
    // Event called when a player is done with their turn
    public static event Action onTurnEnd;
    // Event called when a player plays their last card
    public static event Action<Player> onLastCardPlayed;
    // Event called when all players chose "Play again"
    public static event Action onGameRestart;

    // A card was played
    public static void CardPlayed(CardInfo _card, Player _owner)
    {
        // Raise the onCardPlayed event
        onCardPlayed?.Invoke(_card, _owner);
    }

    public static void CardFeedbackShown() { onCardFeedbackOver?.Invoke(); }

    public static void EndTurn() { onTurnEnd?.Invoke(); }

    public static void PlayedLastCard(Player _player) { onLastCardPlayed?.Invoke(_player); }

    public static void ResetGame() { onGameRestart?.Invoke(); }
}
