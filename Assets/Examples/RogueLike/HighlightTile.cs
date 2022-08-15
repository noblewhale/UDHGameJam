namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

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
            if ((previousMousePos - Input.mousePosition).sqrMagnitude > 4 || oldCameraPosition != Player.instance.transform.position)
            {
                previousMousePos = Input.mousePosition;

                Vector2 mousePosRelativeToMapRenderer = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)Camera.main.transform.position) - (Vector2)MapRenderer.instance.transform.localPosition;
                mousePosRelativeToMapRenderer /= MapRenderer.instance.transform.localScale;
                float angle = Vector2.Angle(mousePosRelativeToMapRenderer, Vector2.up);
                bool ignoreInput = false;
                ignoreInput |= angle > -40 && angle < 40;
                ignoreInput |= mousePosRelativeToMapRenderer.y > 0 && mousePosRelativeToMapRenderer.magnitude < .075f;
                if (!ignoreInput)
                {
                    Vector2 relativeWorldPos = PolarMapUtil.GetPositionRelativeToMap(Input.mousePosition);
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
            KeyCode key = command.key;

            int newTileX = tile.x;
            int newTileY = tile.y;

            bool doSomething = true;
            switch (key)
            {
                case KeyCode.UpArrow:
                case KeyCode.W:
                case KeyCode.Keypad8:
                    newTileY++;
                    break;
                case KeyCode.DownArrow:
                case KeyCode.S:
                case KeyCode.Keypad2:
                    newTileY--;
                    break;
                case KeyCode.RightArrow:
                case KeyCode.D:
                case KeyCode.Keypad6:
                    newTileX++;
                    break;
                case KeyCode.LeftArrow:
                case KeyCode.A:
                case KeyCode.Keypad4:
                    newTileX--;
                    break;
                case KeyCode.Keypad9:
                    newTileY++;
                    newTileX++;
                    break;
                case KeyCode.Keypad7:
                    newTileY++;
                    newTileX--;
                    break;
                case KeyCode.Keypad1:
                    newTileY--;
                    newTileX--;
                    break;
                case KeyCode.Keypad3:
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