using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("REFERENCIAS")]
    public Image cardImage;
    public TMP_Text nameText;

    public void SetCard(int id, string characterName)
    {
        Debug.Log("NOMBRE RECIBIDO: " + characterName);

        if (nameText == null)
        {
            Debug.LogError("? nameText está vacío.");
            return;
        }

        Debug.Log(
            "Text encontrado: " +
            nameText.gameObject.name
        );

        nameText.text = characterName;

        // Para asegurarnos de que sea visible
        nameText.gameObject.SetActive(true);
    }

    public void SetImage(Sprite sprite)
    {
        if (cardImage == null)
        {
            Debug.LogError("? cardImage está vacío.");
            return;
        }

        cardImage.sprite = sprite;
        cardImage.enabled = true;
    }
}