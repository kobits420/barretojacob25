using JacobsEscape.PuzzleCore;
using UnityEngine;

/// <summary>
/// Keypad for a <see cref="DoorLockPuzzle"/>. It displays state and forwards
/// digits; it does not decide whether a code is correct.
/// </summary>
public sealed class CodeLockPanel : MonoBehaviour
{
    public bool IsOpen { get; private set; }

    DoorLockPuzzle puzzle;
    string hint = "";
    string status = "";
    string banner = "";
    float bannerUntil;

    public void SetHint(string value)
    {
        hint = value ?? "";
    }

    public void ShowBanner(string message, float seconds)
    {
        banner = message ?? "";
        bannerUntil = Time.time + seconds;
    }

    public void Open(DoorLockPuzzle target)
    {
        if (target == null || target.IsUnlocked) return;
        puzzle = target;
        status = "";
        IsOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        IsOpen = false;
        puzzle = null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!IsOpen || puzzle == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
        {
            status = "";
            puzzle.Backspace();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Submit();

        for (int digit = 0; digit <= 9; digit++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + digit) || Input.GetKeyDown(KeyCode.Keypad0 + digit))
            {
                status = "";
                puzzle.AppendDigit((char)('0' + digit));
            }
        }
    }

    void OnGUI()
    {
        if (IsOpen)
        {
            DrawPanel();
            return;
        }

        DrawCrosshair();
        if (Time.time < bannerUntil && banner.Length > 0)
            DrawBottom(banner);
        else if (hint.Length > 0)
            DrawBottom(hint);
        else
            DrawBottom("WASD move    Mouse look    Esc frees the cursor");
    }

    void Submit()
    {
        if (puzzle == null) return;
        DoorLockSubmitResult result = puzzle.Submit();
        if (result == DoorLockSubmitResult.Unlocked)
        {
            ShowBanner("Door unlocked", 2.5f);
            Close();
            return;
        }

        if (result == DoorLockSubmitResult.Rejected)
            status = "Wrong code";
    }

    void DrawPanel()
    {
        float width = 340f;
        float height = 460f;
        Rect area = new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f,
            width,
            height);

        GUILayout.BeginArea(area, GUI.skin.box);
        GUILayout.Space(8f);
        GUILayout.Label("Code lock", TitleStyle());
        string shown = puzzle != null && puzzle.Entry.Length > 0 ? puzzle.Entry : "—";
        GUILayout.Label(shown, EntryStyle());
        GUILayout.Label(status, StatusStyle());
        GUILayout.Space(6f);

        DrawRow("1", "2", "3");
        DrawRow("4", "5", "6");
        DrawRow("7", "8", "9");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear", GUILayout.Height(46f)))
        {
            puzzle.Clear();
            status = "";
        }
        if (GUILayout.Button("0", GUILayout.Height(46f)))
            puzzle.AppendDigit('0');
        if (GUILayout.Button("Enter", GUILayout.Height(46f)))
            Submit();
        GUILayout.EndHorizontal();
        GUILayout.Space(6f);
        GUILayout.Label("Esc closes the lock", HintStyle());
        GUILayout.EndArea();
    }

    void DrawRow(string a, string b, string c)
    {
        GUILayout.BeginHorizontal();
        DigitButton(a);
        DigitButton(b);
        DigitButton(c);
        GUILayout.EndHorizontal();
    }

    void DigitButton(string label)
    {
        if (GUILayout.Button(label, GUILayout.Height(46f)) && label.Length == 1)
        {
            status = "";
            puzzle.AppendDigit(label[0]);
        }
    }

    void DrawCrosshair()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        float x = Screen.width * 0.5f;
        float y = Screen.height * 0.5f;
        GUI.Label(new Rect(x - 8f, y - 12f, 20f, 24f), "+", CrosshairStyle());
    }

    void DrawBottom(string message)
    {
        float width = 640f;
        Rect rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 72f, width, 36f);
        GUI.Label(rect, message, BottomStyle());
    }

    static GUIStyle TitleStyle()
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
    }

    static GUIStyle EntryStyle()
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            alignment = TextAnchor.MiddleCenter
        };
    }

    static GUIStyle StatusStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = new Color(0.75f, 0.28f, 0.22f);
        return style;
    }

    static GUIStyle HintStyle()
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter
        };
    }

    static GUIStyle BottomStyle()
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };
    }

    static GUIStyle CrosshairStyle()
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };
    }
}
