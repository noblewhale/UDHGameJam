using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public DungeonObject playerPrefab;

    [NonSerialized]
    public DungeonObject identity;

    public bool isControllingCamera = false;

    public Map map;

    public Camera mainCamera;

    public static Player instance;

    public PlayerBehaviour playerBehaviour;
    public PlayerInput playerInput;

    public virtual void Awake ()
    {
        instance = this;
        map = FindObjectOfType<Map>().GetComponent<Map>();
        map.OnMapLoaded += OnMapLoaded;
        mainCamera.GetComponent<EntryAnimation>().OnDoneAnimating += OnEntryAnimationFinished;
	}

    void OnPositionChange(int oldX, int oldY, int newX, int newY)
    {
        map.Reveal(newX, newY, identity.viewDistance);
    }

    public void OnEntryAnimationFinished()
    {
        isControllingCamera = true;
    }

    void OnMapLoaded()
    {
        if (identity == null)
        {
            identity = Instantiate(playerPrefab);
            //foreach (var behaviour in identity.GetComponents<TickableBehaviour>())
            //{
            //    behaviour.enabled = false;
            //}
            identity.onSetPosition += OnPositionChange;
            playerBehaviour = identity.GetComponent<Tickable>().AddBehaviour<PlayerBehaviour>();
        }

        Tile startTile = map.GetRandomTileThatAllowsSpawn();
        startTile.AddObject(identity);
        map.UpdateLighting();
        map.Reveal(identity.x, identity.y, identity.viewDistance);

        mainCamera.GetComponent<PlayerCamera>().SetRotation(startTile.x, startTile.y, float.Epsilon, float.MaxValue);
        if (map.dungeonLevel == 1)
        {
            mainCamera.GetComponent<EntryAnimation>().isAnimating = true;
        }
        else
        {
            mainCamera.GetComponent<PlayerCamera>().SetY(identity.transform.position.y, 1, float.MaxValue);
        }
    }
}
