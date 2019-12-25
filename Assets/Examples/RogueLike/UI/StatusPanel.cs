using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusPanel : MonoBehaviour
{
    Camera cam;
    public float panelHeight = 3;
    public TextMesh health;
    public TextMesh mana;
    public TextMesh gold;
    public AnimationCurve highlightAnimation;
    public InventoryGUI inventory;
    Player player;

    int oldHealth, oldGold;
    int visualGold;

    Coroutine highlightHealthProcess;
    Coroutine highlightGoldProcess;
    Coroutine incrementGoldProcess;

    void Start ()
    {
        cam = GetComponentInParent<Camera>();
        player = Player.instance;
	}
	
	void Update ()
    {
        if (player.identity && player.identity.health != oldHealth)
        {
            health.text = player.identity.health.ToString();
            if (highlightHealthProcess != null) StopCoroutine(highlightHealthProcess);
            highlightHealthProcess = StartCoroutine(HighlightText(health, Color.red));
            oldHealth = player.identity.health;
        }
        
        if (player.identity.gold != oldGold)
        {
            if (incrementGoldProcess == null)
            {
                incrementGoldProcess = StartCoroutine(IncrementGold());
            }
            if (highlightGoldProcess != null) StopCoroutine(highlightGoldProcess);
            highlightGoldProcess = StartCoroutine(HighlightText(gold, Color.yellow));
            oldGold = player.identity.gold;
        }

        transform.localPosition = new Vector3(0, -cam.orthographicSize + panelHeight, transform.localPosition.z);
	}

    IEnumerator IncrementGold()
    {
        while(visualGold < player.identity.gold)
        {
            yield return new WaitForSeconds(.1f);
            visualGold++;

            gold.text = visualGold.ToString();
        }
        incrementGoldProcess = null;
    }

    IEnumerator HighlightText(TextMesh textMesh, Color color)
    {
        float t = 0;
        float duration = 1f;
        while (t < duration)
        {
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
            textMesh.color = Color.Lerp(color, Color.white, highlightAnimation.Evaluate(t/duration));
        }
    }
}
