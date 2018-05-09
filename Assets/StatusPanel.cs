using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusPanel : MonoBehaviour
{
    Camera cam;
    public float panelHeight = 3;
    public TextMesh health;
    public TextMesh mana;
    public AnimationCurve highlightAnimation;
    Player player;

    int oldHealth;

    Coroutine highlightHealthProcess;

	void Start ()
    {
        cam = GetComponentInParent<Camera>();
        player = Player.instance;
	}
	
	void Update ()
    {
        if (player.identity && player.identity.health != oldHealth)
        {
            if (highlightHealthProcess != null) StopCoroutine(highlightHealthProcess);
            highlightHealthProcess = StartCoroutine(ChangeHealth());
            oldHealth = player.identity.health;
        }
        transform.localPosition = new Vector3(0, -cam.orthographicSize + panelHeight, transform.localPosition.z);
	}

    IEnumerator ChangeHealth()
    {
        health.text = player.identity.health.ToString();

        float t = 0;
        float duration = 1f;
        while (t < duration)
        {
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
            health.color = Color.Lerp(Color.red, Color.white, highlightAnimation.Evaluate(t/duration));
        }

        highlightHealthProcess = null;
    }
}
