using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("Textos")]
    public TMP_Text idText;
    public TMP_Text nameText;
    public TMP_Text suitText;

    [Header("Imagen")]
    public Image cardImage;


    public void SetCard(
        int id,
        string value,
        string suit)
    {
        if (idText != null)
        {
            idText.text = "ID: " + id;
        }

        if (nameText != null)
        {
            nameText.text = value;
        }

        if (suitText != null)
        {
            suitText.text = suit;
        }
    }


    public void SetImage(Sprite sprite)
    {
        if (cardImage != null)
        {
            cardImage.sprite = sprite;
            cardImage.enabled = true;
        }
    }
}