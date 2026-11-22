using UnityEngine;

/// <summary>
/// Swaps the greybox props for Kenney's CC0 furniture when Play starts.
/// Colliders and the code lock stay on the greybox pieces.
/// </summary>
public sealed class KitchenSetDressing : MonoBehaviour
{
    void Start()
    {
        var fridge = Spawn("kitchenFridge", new Vector3(-2.55f, 0f, 0.25f), -90f, 1.85f, FitAxis.Height);
        if (fridge != null)
        {
            HideRenderers("Fridge", "FridgeSeam");
            StickNote(CombinedBounds(fridge));
        }

        bool counter = false;
        counter |= Spawn("kitchenCabinet", new Vector3(2.42f, 0f, -1.15f), 90f, 0.92f, FitAxis.Height) != null;
        counter |= Spawn("kitchenSink", new Vector3(2.42f, 0f, -0.15f), 90f, 0.95f, FitAxis.Height) != null;
        counter |= Spawn("kitchenStove", new Vector3(2.42f, 0f, 0.9f), 90f, 0.95f, FitAxis.Height) != null;
        Spawn("kitchenCabinetUpper", new Vector3(2.5f, 1.42f, -0.65f), 90f, 0.7f, FitAxis.Height);
        Spawn("kitchenCoffeeMachine", new Vector3(2.2f, 0.92f, -0.7f), 90f, 0.32f, FitAxis.Height);
        Spawn("kitchenMicrowave", new Vector3(2.2f, 0.92f, 0.35f), 90f, 0.28f, FitAxis.Height);
        if (counter)
            HideRenderers("Counter");

        if (Spawn("table", new Vector3(0.15f, 0f, -1.05f), 0f, 0.75f, FitAxis.Height) != null)
            HideRenderers("Table");
        Spawn("chair", new Vector3(-0.55f, 0f, -1.05f), 90f, 0.9f, FitAxis.Height);
        Spawn("chair", new Vector3(0.85f, 0f, -1.05f), -90f, 0.9f, FitAxis.Height);
        Spawn("rugRectangle", new Vector3(0.15f, 0.01f, -1.05f), 0f, 1.7f, FitAxis.Width);
        Spawn("plantSmall1", new Vector3(-2.15f, 0f, -1.85f), 20f, 0.45f, FitAxis.Height);

        PaintSurfaces();
    }

    enum FitAxis
    {
        Height,
        Width
    }

    GameObject Spawn(string resourceName, Vector3 feetCenter, float yaw, float target, FitAxis axis)
    {
        GameObject prefab = Resources.Load<GameObject>("Furniture/" + resourceName);
        if (prefab == null)
            return null;

        GameObject instance = Instantiate(prefab, transform);
        instance.name = resourceName;
        foreach (Collider collider in instance.GetComponentsInChildren<Collider>())
            collider.enabled = false;

        instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, yaw, 0f));
        instance.transform.localScale = Vector3.one;
        Bounds bounds = CombinedBounds(instance);
        float source = axis == FitAxis.Height ? bounds.size.y : Mathf.Max(bounds.size.x, bounds.size.z);
        if (source < 0.001f)
            return instance;

        float scale = target / source;
        instance.transform.localScale = Vector3.one * scale;
        bounds = CombinedBounds(instance);
        Vector3 delta = feetCenter - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        instance.transform.position += delta;
        FixMaterials(instance);
        return instance;
    }

    static void HideRenderers(params string[] names)
    {
        foreach (string name in names)
        {
            GameObject target = GameObject.Find(name);
            if (target == null)
                continue;
            MeshRenderer renderer = target.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.enabled = false;
        }
    }

    static void StickNote(Bounds fridge)
    {
        Vector3 towardRoom = new Vector3(-fridge.center.x, 0f, -fridge.center.z);
        if (towardRoom.sqrMagnitude < 0.01f)
            towardRoom = Vector3.right;
        towardRoom.Normalize();

        Vector3 face = fridge.center;
        float yaw;
        if (Mathf.Abs(towardRoom.x) > Mathf.Abs(towardRoom.z))
        {
            bool positive = towardRoom.x > 0f;
            face.x = positive ? fridge.max.x : fridge.min.x;
            yaw = positive ? 90f : -90f;
        }
        else
        {
            bool positive = towardRoom.z > 0f;
            face.z = positive ? fridge.max.z : fridge.min.z;
            yaw = positive ? 0f : 180f;
        }

        face.y = fridge.center.y + 0.15f;
        PlaceNote("NoteCard", face, towardRoom, yaw, 0.01f);
        PlaceNote("FridgeNote", face, towardRoom, yaw, 0.02f);
    }

    static void PlaceNote(string objectName, Vector3 face, Vector3 normal, float yaw, float lift)
    {
        GameObject note = GameObject.Find(objectName);
        if (note == null)
            return;
        note.transform.SetPositionAndRotation(face + normal * lift, Quaternion.Euler(0f, yaw, 0f));
    }

    static void PaintSurfaces()
    {
        Texture2D floor = Planks(new Color(0.45f, 0.3f, 0.18f), new Color(0.62f, 0.45f, 0.28f));
        Texture2D wall = Plaster(new Color(0.86f, 0.83f, 0.76f));
        Texture2D door = Planks(new Color(0.36f, 0.22f, 0.12f), new Color(0.52f, 0.34f, 0.2f));
        Paint("Floor", floor, new Vector2(6f, 5f));
        Paint("Stoop", floor, new Vector2(2f, 2f));
        Paint("WallSouth", wall, new Vector2(4f, 2f));
        Paint("WallWest", wall, new Vector2(3f, 2f));
        Paint("WallEast", wall, new Vector2(3f, 2f));
        Paint("WallNorthLeft", wall, new Vector2(2f, 2f));
        Paint("WallNorthRight", wall, new Vector2(2f, 2f));
        Paint("WallNorthHeader", wall, new Vector2(1f, 1f));
        Paint("DoorLeaf", door, new Vector2(1f, 2f));
    }

    static void Paint(string objectName, Texture2D texture, Vector2 scale)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
            return;
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
            return;
        Material material = renderer.material;
        if (material.HasProperty("_MainTex"))
            material.mainTexture = texture;
        if (material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", texture);
        material.mainTextureScale = scale;
    }

    static Texture2D Planks(Color dark, Color light)
    {
        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Repeat;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n = Hash(x, y);
                Color color = Color.Lerp(dark, light, n);
                if (y % 16 == 0)
                    color *= 0.72f;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    static Texture2D Plaster(Color baseColor)
    {
        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Repeat;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n = Hash(x * 3, y * 5);
                texture.SetPixel(x, y, baseColor * Mathf.Lerp(0.92f, 1.05f, n));
            }
        }

        texture.Apply();
        return texture;
    }

    static float Hash(int x, int y)
    {
        int n = x * 374761393 + y * 668265263;
        n = (n ^ (n >> 13)) * 1274126177;
        return ((n ^ (n >> 16)) & 255) / 255f;
    }

    static Bounds CombinedBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(root.transform.position, Vector3.one);
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    static void FixMaterials(GameObject root)
    {
        Shader shader = Shader.Find("Standard") ?? Shader.Find("Diffuse") ?? Shader.Find("Sprites/Default");
        if (shader == null)
            return;
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
        {
            Material[] materials = renderer.materials;
            bool changed = false;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material == null || material.shader == null || material.shader.name == "Hidden/InternalErrorShader")
                {
                    Color color = material != null && material.HasProperty("_Color") ? material.color : Color.white;
                    materials[i] = new Material(shader) { color = color };
                    changed = true;
                }
            }

            if (changed)
                renderer.materials = materials;
        }
    }
}
