using UnityEngine;

/// <summary>
/// Colors greybox meshes at play time if a hand-authored material did not import.
/// </summary>
public sealed class GreyboxFallback : MonoBehaviour
{
    void Awake()
    {
        Shader shader = Shader.Find("Standard")
            ?? Shader.Find("Diffuse")
            ?? Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Unlit/Color")
            ?? Shader.Find("Sprites/Default");
        if (shader == null) return;

        MeshRenderer[] renderers = FindObjectsByType<MeshRenderer>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.GetComponent<TextMesh>() != null) continue;
            Material current = renderer.sharedMaterial;
            bool broken = current == null
                || current.shader == null
                || current.shader.name == "Hidden/InternalErrorShader";
            if (!broken) continue;

            Color color = ColorFor(renderer.gameObject.name);
            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.color = color;
            renderer.material = material;
        }
    }

    static Color ColorFor(string objectName)
    {
        switch (objectName)
        {
            case "Floor":
            case "Stoop":
                return new Color(0.45f, 0.43f, 0.40f);
            case "WallSouth":
            case "WallWest":
            case "WallEast":
            case "WallNorthLeft":
            case "WallNorthRight":
            case "WallNorthHeader":
                return new Color(0.82f, 0.80f, 0.75f);
            case "Fridge":
                return new Color(0.75f, 0.82f, 0.86f);
            case "FridgeSeam":
                return new Color(0.22f, 0.25f, 0.28f);
            case "NoteCard":
                return new Color(0.95f, 0.86f, 0.35f);
            case "Counter":
                return new Color(0.55f, 0.58f, 0.56f);
            case "Table":
                return new Color(0.45f, 0.32f, 0.22f);
            case "DoorLeaf":
                return new Color(0.40f, 0.28f, 0.18f);
            case "LockPlate":
                return new Color(0.12f, 0.13f, 0.15f);
            case "Handle":
                return new Color(0.72f, 0.72f, 0.74f);
            default:
                return new Color(0.70f, 0.70f, 0.70f);
        }
    }
}
