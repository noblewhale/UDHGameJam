namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.Rendering.PostProcessing;

    public class HighlightTile : DungeonObject
    {
        Vector3 previousMousePos;

        public static HighlightTile instance;

        Vector3 oldCameraPosition;
        public bool isKeyboardControlled;
        public List<Tile> allowedTiles = null;

        public PostProcessVolume circleWarpVolume;
        CircleWarp circleWarp;

        override protected void Awake()
        {
            base.Awake();
            instance = this;
        }

        override protected void Start()
        {
            base.Start();
            circleWarpVolume.profile.TryGetSettings(out circleWarp);
        }

        virtual public void OnDestroy()
        {
            instance = null;
        }

        void Update()
        {
            if (!Map.instance.isDoneGeneratingMap) return;

            Vector3 mousePos = Mouse.current.position.ReadValue();
            if ((previousMousePos - mousePos).sqrMagnitude > 8)
            {
                isKeyboardControlled = false;
                previousMousePos = mousePos;
            }

            if (!Cursor.visible && !isKeyboardControlled)
            {
                glyphs.glyphs[0].gameObject.SetActive(false);
                return;
            }
            if (!glyphs.glyphs[0].gameObject.activeSelf)
            {
                glyphs.glyphs[0].gameObject.SetActive(true);
            }

            if (!isKeyboardControlled || (oldCameraPosition != PlayerCamera.instance.transform.position && Cursor.visible))
            {
                isKeyboardControlled = false;

                bool success = circleWarp.UnwarpPosition(mousePos, out Vector2 unwarpedPos);
                unwarpedPos *= Camera.main.GetSize();
                unwarpedPos += (Vector2)Camera.main.transform.position - Camera.main.GetSize() / 2;
                if (success)
                {
                    Tile tileUnderMouse = Map.instance.GetTileFromWorldPosition(unwarpedPos);

                    if (tile == null || tileUnderMouse != tile)
                    {
                        tile?.RemoveObject(this);
                        tileUnderMouse.AddObject(this, false, 2);
                    }
                }
                oldCameraPosition = PlayerCamera.instance.transform.position;
            }
            if (allowedTiles != null && allowedTiles.Count > 0)
            {
                if (!allowedTiles.Contains(tile))
                {
                    float minDistance = float.MaxValue;
                    Tile closestTile = null;
                    foreach (Tile allowedTile in allowedTiles)
                    {
                        Vector2Int difference = Map.instance.GetDifference(tilePosition, allowedTile.tilePosition);
                        float distance = difference.magnitude;
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestTile = allowedTile;
                        }
                    }
                    if (tile == null || closestTile != tile)
                    {
                        tile?.RemoveObject(this);
                        closestTile.AddObject(this, false, 2);
                    }
                }
            }
        }

        public bool Move(Command command)
        {
            Key key = command.key;

            Vector2Int newTilePos = tile.tilePosition;

            bool doSomething = true;
            switch (key)
            {
                case Key.UpArrow:
                case Key.W:
                case Key.Numpad8:
                    newTilePos.y++;
                    break;
                case Key.DownArrow:
                case Key.S:
                case Key.Numpad2:
                    newTilePos.y--;
                    break;
                case Key.RightArrow:
                case Key.D:
                case Key.Numpad6:
                    newTilePos.x++;
                    break;
                case Key.LeftArrow:
                case Key.A:
                case Key.Numpad4:
                    newTilePos.x--;
                    break;
                case Key.Numpad9:
                    newTilePos.y++;
                    newTilePos.x++;
                    break;
                case Key.Numpad7:
                    newTilePos.y++;
                    newTilePos.x--;
                    break;
                case Key.Numpad1:
                    newTilePos.y--;
                    newTilePos.x--;
                    break;
                case Key.Numpad3:
                    newTilePos.y--;
                    newTilePos.x++;
                    break;
                default:
                    doSomething = false;
                    break;
            }

            newTilePos.x = Map.instance.GetXTilePositionOnMap(newTilePos.x);

            if (doSomething)
            {
                if (allowedTiles != null && allowedTiles.Count > 0)
                {
                    if (!allowedTiles.Contains(Map.instance.GetTile(newTilePos)))
                    {
                        doSomething = false;
                    }
                }
            }

            if (doSomething)
            {
                if (newTilePos.y >= 0 && newTilePos.y < Map.instance.height)
                {
                    isKeyboardControlled = true;
                    tile?.RemoveObject(this);
                    Map.instance.GetTile(newTilePos).AddObject(this, false, 2);
                    oldCameraPosition = PlayerCamera.instance.transform.position;
                }
            }

            return doSomething;
        }
    }
}