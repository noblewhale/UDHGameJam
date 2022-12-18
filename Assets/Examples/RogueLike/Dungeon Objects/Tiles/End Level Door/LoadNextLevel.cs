using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextLevel : MonoBehaviour
{
    public void LoadLevel(DungeonObject ob)
    {
        if (ob == Player.instance.identity)
        {
            SceneManager.LoadScene("Level 2");
        }
    }
}