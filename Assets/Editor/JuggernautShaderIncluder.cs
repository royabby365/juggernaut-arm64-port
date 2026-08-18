// Ensures placeholder shaders are always included in the build
// Run this once from the menu, or let [InitializeOnLoad] do it automatically.
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class JuggernautShaderIncluder
{
    static JuggernautShaderIncluder()
    {
        // Only run once per project load — add our shader to always-included list
        var shader = Shader.Find("Hidden/JuggernautArena");
        if (shader == null) return;
        
        var gs = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
        if (gs.Length == 0) return;
        
        var so = new SerializedObject(gs[0]);
        var prop = so.FindProperty("m_AlwaysIncludedShaders");
        if (prop == null) return;
        
        for (int i = 0; i < prop.arraySize; i++)
            if (prop.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                return; // already included
                
        prop.InsertArrayElementAtIndex(prop.arraySize);
        prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = shader;
        so.ApplyModifiedProperties();
        Debug.Log($"[ShaderIncluder] Added {shader.name} to Always Included Shaders");
    }
}