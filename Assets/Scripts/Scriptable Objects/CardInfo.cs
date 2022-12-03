using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
// Holds the basic information of a card, without having to instantiate it
public class CardInfo
{
	public int value;
	public Suits suit;

	public CardInfo(int _value, Suits _suit)
	{
		value = _value;
		suit = _suit;
	}
}
