namespace Noble.DungeonCrawler
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    public class ContinueText : MonoBehaviour
    {
        TMPro.TMP_Text textComponent;
        Button button;

        void Start()
        {
            textComponent = GetComponent<TMPro.TMP_Text>();
            button = GetComponentInParent<Button>();
            StartCoroutine(AnimateText());
        }

        IEnumerator AnimateText()
        {
            while (true)
            {
                textComponent.text = "Continue ";
                yield return new WaitForSeconds(.4f);
                textComponent.text = "Continue -";
                yield return new WaitForSeconds(.4f);
                textComponent.text = "Continue --";
                yield return new WaitForSeconds(.4f);
                textComponent.text = "Continue ---";
                yield return new WaitForSeconds(.4f);
                textComponent.text = "Continue --->";
                yield return new WaitForSeconds(.4f);
            }
        }
    }
}