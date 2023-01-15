using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Asset Manager")]
public class AssetManager : ScriptableObject
{
    [SerializeField]
    private List<Sprite> defaultCardSprites = new List<Sprite>();
    [SerializeField]
    private Sprite defaultCardBack;

    public Sprite GetCardSprite(int _value, Suits _suit, Theme _theme = Theme.Default)
    {
        int cardIndex = (_value - 1) * sizeof(Suits);
        switch (_suit)
        {
            case Suits.Clubs:
                cardIndex += 1;
                break;
            case Suits.Hearts:
                cardIndex += 2;
                break;
            case Suits.Spades:
                cardIndex += 3;
                break;
        }

        switch (_theme)
        {
            default:
                return defaultCardSprites[cardIndex];
        }
    }

    public Sprite GetCardBack(Theme _theme = Theme.Default)
    {
        switch (_theme)
        {
            default:
                return defaultCardBack;
        }
    }
}
