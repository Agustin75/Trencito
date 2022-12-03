using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Deck")]
public class Deck : ScriptableObject
{
	[SerializeField]
	private List<CardInfo> startingDeck = new List<CardInfo>();

	private void OnEnable()
	{
		// DEBUG: When something changes in the deck's behavior, use this to repopulate the Deck. Remember to comment it out afterwards so it doesn't generate more cards.
		//for (int s = 0; s < sizeof(Suits); s++)
		//{
		//	for (int v = 1; v < 14; v++)
		//	{
		//		startingDeck.Add(new CardInfo(v, (Suits)s));
		//	}
		//}
	}

	public List<CardInfo> GetDeckCopy()
	{
		return new List<CardInfo>(startingDeck);
	}
}
