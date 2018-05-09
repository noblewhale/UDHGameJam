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
    Player player;

    int oldHealth, oldGold;

    Coroutine highlightHealthProcess;
    Coroutine highlightGoldProcess;

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

        DungeonObject currentGold;
        bool hasGold = player.identity.inventory.TryGetValue("Gold", out currentGold);
        if (hasGold)
        {
            gold.text = currentGold.quantity.ToString();
            if (highlightGoldProcess != null) StopCoroutine(highlightGoldProcess);
            highlightGoldProcess = StartCoroutine(HighlightText(gold, Color.yellow));
            oldHealth = player.identity.health;
            oldGold = currentGold.quantity;
        }

        transform.localPosition = new Vector3(0, -cam.orthographicSize + panelHeight, transform.localPosition.z);
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
