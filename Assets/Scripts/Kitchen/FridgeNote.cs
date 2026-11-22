using JacobsEscape.PuzzleCore;
using UnityEngine;

/// <summary>
/// Puts the puzzle's fridge copy on this TextMesh and fits it to the note.
/// </summary>
public sealed class FridgeNote : MonoBehaviour
{
    const float TargetWidth = 0.64f;

    void Awake()
    {
        TextMesh text = GetComponent<TextMesh>();
        if (text == null) return;
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        text.text = DoorLockPuzzle.FridgeNoteText;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = new Color(0.12f, 0.09f, 0.07f, 1f);
        text.richText = false;
        text.fontSize = 0;
        text.characterSize = 1f;
        Fit(text);
    }

    void Start()
    {
        TextMesh text = GetComponent<TextMesh>();
        if (text != null)
            Fit(text);
    }

    void Fit(TextMesh text)
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh mesh = filter != null ? filter.sharedMesh : null;
        if (mesh == null || mesh.bounds.size.x < 0.001f)
        {
            text.characterSize = 0.02f;
            transform.localScale = Vector3.one;
            return;
        }

        float scale = TargetWidth / mesh.bounds.size.x;
        transform.localScale = Vector3.one * scale;
    }
}
