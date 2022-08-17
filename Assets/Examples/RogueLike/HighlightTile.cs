namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class HighlightTile : DungeonObject
    {
        Vector3 previousMousePos;

        public static HighlightTile instance;

        Vector3 oldCameraPosition;

        override protected void Awake()
        {
            instance = this;
            base.Awake();
        }

        void Update()
        {
            //if (!Cursor.visible)
            //{
            //    glyphs.glyphs[0].gameObject.SetActive(false);
            //    return;
            //}
            //if (!glyphs.glyphs[0].gameObject.activeSelf)
            //{
            //    glyphs.glyphs[0].gameObject.SetActive(true);
            //}

            Vector3 mousePos = Mouse.current.position.ReadValue();
            if ((previousMousePos - mousePos).sqrMagnitude > 4 || oldCameraPosition != PlayerCamera.instance.transform.position)
            {
                previousMousePos = mousePos;

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

            newTileX = Map.instance.WrapX(newTileX);

            if (doSomething)
            {
                if (newTileY >= 0 && newTileY < Map.instance.height)
                {
                    tile?.RemoveObject(this);
                    Map.instance.tileObjects[newTileY][newTileX].AddObject(this);
                }
            }

            return doSomething;
        }
    }
}