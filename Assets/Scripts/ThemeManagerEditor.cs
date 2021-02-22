#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ThemeManager))]
public class ThemeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //ThemeManager themeManager = (ThemeManager)target;

        DrawDefaultInspector();

        GUILayout.Space(20);

        if (GUILayout.Button("Add Themables"))
        {
            AddThemables();
        }
    }

    private void AddThemables()
    {
        ThemeManager themeManager = (ThemeManager)target;

        themeManager.themablesList.Clear();
        Object[] objects = Resources.FindObjectsOfTypeAll(typeof(Themable));
        foreach (Themable themable in objects)
        {
            if (themable != null && !EditorUtility.IsPersistent(themable.transform.root.gameObject) && !(themable.hideFlags == HideFlags.NotEditable || themable.hideFlags == HideFlags.HideAndDontSave))
            {
                themeManager.themablesList.Add(themable);
            }
        }
        AssetDatabase.Refresh();
    }
}
#endif