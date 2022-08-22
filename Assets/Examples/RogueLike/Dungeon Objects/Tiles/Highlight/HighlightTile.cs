namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class HighlightTile : DungeonObject
    {
        Vector3 previousMousePos;

        public static HighlightTile instance;

        Vector3 oldCameraPosition;
        public bool isKeyboardControlled;
        public List<Tile> allowedTiles = null;

        override protected void Awake()
        {
            instance = this;
            base.Awake();
        }

        void Update()
        {
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

                Vector2 mousePosRelativeToMapRenderer = ((Vector2)Camera.main.ScreenToWorldPoint(mousePos) - (Vector2)Camera.main.transform.position) - (Vector2)MapRenderer.instance.transform.localPosition;
                mousePosRelativeToMapRenderer /= MapRenderer.instance.transform.localScale;
                float angle = Vector2.Angle(mousePosRelativeToMapRenderer, Vector2.up);
                bool ignoreInput = false;
                ignoreInput |= angle > -40 && angle < 40;
                ignoreInput |= mousePosRelativeToMapRenderer.y > 0 && mousePosRelativeToMapRenderer.magnitude < .075f;
                if (!ignoreInput)
                {
                    Vector2 relativeWorldPos = PolarMapUtil.GetPositionRelativeToMap(mousePos);
                    Vector2 unwarpedPos;
                    bool success = PolarMapUtil.UnwarpPosition(relativeWorldPos, out unwarpedPos);
                    if (success)
                    {
                        bool isInsideMap = PolarMapUtil.PositionToTile(unwarpedPos, out int tileX, out int tileY);
                        if (isInsideMap)
                        {
                            if (tile == null || tileY != tile.y || tileX != tile.x)
                            {
                                tile?.RemoveObject(this);
                                Map.instance.tileObjects[tileY][tileX].AddObject(this);
                            }
                        }
                    }
                    oldCameraPosition = PlayerCamera.instance.transform.position;
                }
            }
            if (allowedTiles != null && allowedTiles.Count > 0)
            {
                if (!allowedTiles.Contains(Map.instance.GetTile(x, y)))
                {
                    float minDistance = float.MaxValue;
                    Tile closestTile = null;
                    foreach (Tile allowedTile in allowedTiles)
                    {
                        Vector2Int difference = Map.instance.GetDifference(new Vector2Int(x, y), new Vector2Int(allowedTile.x, allowedTile.y));
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
                        Map.instance.tileObjects[closestTile.y][closestTile.x].AddObject(this);
                    }
                }
            }
        }

        public bool Move(Command command)
        {
            Key key = command.key;

            int newTileX = tile.x;
            int newTileY = tile.y;

            bool doSomething = true;
            switch (key)
            {
                case Key.UpArrow:
                case Key.W:
                case Key.Numpad8:
                    newTileY++;
                    break;
                case Key.DownArrow:
                case Key.S:
                case Key.Numpad2:
                    newTileY--;
                    break;
                case Key.RightArrow:
                case Key.D:
                case Key.Numpad6:
                    newTileX++;
                    break;
                case Key.LeftArrow:
                case Key.A:
                case Key.Numpad4:
                    newTileX--;
                    break;
                case Key.Numpad9:
                    newTileY++;
                    newTileX++;
                    break;
                case Key.Numpad7:
                    newTileY++;
                    newTileX--;
                    break;
                case Key.Numpad1:
                    newTileY--;
                    newTileX--;
                    break;
                case Key.Numpad3:
                    newTileY--;
                    newTileX++;
                    break;
                default:
                    doSomething = false;
                    break;
            }

            newTileX = Map.instance.GetXPositionOnMap(newTileX);

            if (doSomething)
            {
                if (allowedTiles != null && allowedTiles.Count > 0)
                {
                    if (!allowedTiles.Contains(Map.instance.GetTile(newTileX, newTileY)))
                    {
                        doSomething = false;
                    }
                }
            }

            if (doSomething)
            {
                if (newTileY >= 0 && newTileY < Map.instance.height)
                {
                    isKeyboardControlled = true;
                    tile?.RemoveObject(this);
                    Map.instance.tileObjects[newTileY][newTileX].AddObject(this);
                    oldCameraPosition = PlayerCamera.instance.transform.position;
                }
            }

            return doSomething;
        }
    }
}