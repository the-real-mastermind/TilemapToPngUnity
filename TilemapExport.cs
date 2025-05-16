using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class TilemapExporter : MonoBehaviour
{
    [Header("Assign this as the object with the tilemap component")]
    public GameObject tilemapObject;

    [Header("Assign this as a new camera")]
    public Camera renderCamera; 

    [Header("This will be the filename of your exported image in /Assets/Exports")]
    public string fileName = "TilemapExport.png";

    [Header("Assign this to the pixels per unit of your spritesheets")]
    public int pixelsPerUnit = 16;

    [Header("Check this if you want the script to try and hide all other objects before exporting")]
    public bool autoHideOther = true;


    private void Start()
    {
        if (autoHideOther)
        {
            object[] obj = GameObject.FindSceneObjectsOfType(typeof(GameObject));
            foreach (object o in obj)
            {
                GameObject g = (GameObject)o;
                if (g.name != tilemapObject.name && !g.TryGetComponent<Camera>(out _))
                {
                    var spriteRenderer = g.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.enabled = false;
                    }

                    var tilemap = g.GetComponent<TilemapRenderer>();
                    if (tilemap != null)
                    {
                        tilemap.enabled = false;
                    }

                }
            }
        }
      
        ExportTilemapToPNG();
    }
    public void ExportTilemapToPNG()
    {
        Tilemap tilemap;
        if (tilemapObject)
        {
            tilemap = tilemapObject.GetComponent<Tilemap>();
        }
        else
        {
            Debug.LogError("Tilemap object is not assigned");
            return;
        }

        if (!tilemap)
        {
            Debug.LogError("No tilemap component was found");
            return;
        }

        if (!renderCamera)
        {
            Debug.LogError("Camera is not assigned");
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;
        Vector3Int size = bounds.size;

        int width = size.x * pixelsPerUnit;
        int height = size.y * pixelsPerUnit;

        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

        Vector3 center = tilemap.CellToWorld(bounds.position + size / 2);
        renderCamera.orthographic = true;
        renderCamera.orthographicSize = size.y / 2f;
        renderCamera.transform.position = new Vector3(center.x, center.y, -10);

        renderCamera.targetTexture = renderTexture;
        renderCamera.Render();

        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        renderCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        byte[] pngData = texture.EncodeToPNG();

        string folderPath = Path.Combine(Application.dataPath, "Exports");
        Directory.CreateDirectory(folderPath);
        if (!fileName.Contains(".png"))
        {
            fileName += ".png";
        }
        string filePath = Path.Combine(folderPath, fileName);
        File.WriteAllBytes(filePath, pngData);

        Debug.Log("Tilemap exported to: " + filePath);
    }
}
