using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameControl
{
    public class ReDrawTileMap : MonoBehaviour
    {
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private TileBase tile;
        [SerializeField] private int distance;
        [SerializeField] [Range(0, 30)] private int width, height;
        private Transform _player;
        private float _cameraHalfWidth, _cameraHalfHeight;

        private Vector2 _oldPos, _curPos;

        private void Start()
        {
            _player = GameManager.Instance.player.transform;
            _oldPos = _curPos = _player.position;

            // Calculate the bounds of the visible area
            var cam = Camera.main;
            if (cam == null) return;
            _cameraHalfWidth = cam.orthographicSize * cam.aspect;
            _cameraHalfHeight = cam.orthographicSize;
        }

        private void FixedUpdate()
        {
            _curPos = _player.position;
            var dis = Vector2.Distance(_oldPos, _curPos);
            if (dis <= distance) return;
            InfinityMap(); 
            _oldPos = _curPos;
        }

        private void InfinityMap()
        {
            // Get the current position of the player
            // Vector3 playerPos = player.position;

            var visibleBounds =
                new Bounds(_curPos, new Vector3((_cameraHalfWidth + width) * 2, (_cameraHalfHeight + height) * 2));

            // Get the bounds of the Tilemap
            var tilemapBounds = tilemap.localBounds;

            // Calculate the min and max tile indices for the visible area
            var minTileIndex = tilemap.WorldToCell(visibleBounds.min);
            var maxTileIndex = tilemap.WorldToCell(visibleBounds.max);

            // Iterate over the tiles in the visible area
            for (var x = minTileIndex.x; x <= maxTileIndex.x; x++)
            {
                for (var y = minTileIndex.y; y <= maxTileIndex.y; y++)
                {
                    // Set the tile if it is not already set
                    if (!tilemap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                    }
                }
            }

            // Iterate over the tiles outside the visible area
            for (var x = (int)tilemapBounds.min.x; x <= tilemapBounds.max.x; x++)
            {
                for (var y = (int)tilemapBounds.min.y; y <= tilemapBounds.max.y; y++)
                {
                    // Remove the tile if it is outside the visible area
                    if (x < minTileIndex.x || x > maxTileIndex.x || y < minTileIndex.y || y > maxTileIndex.y)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), null);
                    }
                }
            }
        }
    }
}