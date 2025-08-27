using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AirParticleConfigExample))]
public class AirParticleConfigExampleEditor : Editor
{
    private bool presetsExpanded = true;
    private bool actionsExpanded = true;
    
    public override void OnInspectorGUI()
    {
        var configExample = (AirParticleConfigExample)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Configurador de Partículas de Aire", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Referencias
        EditorGUILayout.LabelField("Referencias", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("simulation"), new GUIContent("Simulación"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("applyConfigOnStart"), new GUIContent("Aplicar Configuración al Iniciar"));
        
        EditorGUILayout.Space();
        
        // Sección de Presets
        DrawCollapsibleSection("📋 Presets de Aire", ref presetsExpanded, () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("airPresets"), new GUIContent("Presets Disponibles"), true);
        });
        
        EditorGUILayout.Space();
        
        // Sección de Acciones
        DrawCollapsibleSection("🎮 Acciones", ref actionsExpanded, () =>
        {
            EditorGUILayout.LabelField("Presets Rápidos", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Aire Ligero", GUILayout.Height(25)))
            {
                configExample.ApplyLightAir();
            }
            if (GUILayout.Button("Aire Medio", GUILayout.Height(25)))
            {
                configExample.ApplyMediumAir();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Aire Denso", GUILayout.Height(25)))
            {
                configExample.ApplyDenseAir();
            }
            if (GUILayout.Button("Aire Invisible", GUILayout.Height(25)))
            {
                configExample.ApplyInvisibleAir();
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Aire Optimizado", GUILayout.Height(25)))
            {
                configExample.ApplyOptimizedAir();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Configuraciones de Interacción", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Interacción Fuerte", GUILayout.Height(25)))
            {
                configExample.SetStrongInteraction();
            }
            if (GUILayout.Button("Interacción Media", GUILayout.Height(25)))
            {
                configExample.SetMediumInteraction();
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Interacción Débil", GUILayout.Height(25)))
            {
                configExample.SetWeakInteraction();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Utilidades", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Alternar Visibilidad", GUILayout.Height(25)))
            {
                configExample.ToggleAirVisibility();
            }
            if (GUILayout.Button("Mostrar Configuración", GUILayout.Height(25)))
            {
                configExample.ShowCurrentConfig();
            }
            EditorGUILayout.EndHorizontal();
        });
        
        EditorGUILayout.Space();
        
        // Aplicar cambios
        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
    
    private void DrawCollapsibleSection(string title, ref bool expanded, System.Action drawContent)
    {
        // Header con flecha
        var headerRect = GUILayoutUtility.GetRect(EditorGUILayout.GetControlRect().width, EditorGUIUtility.singleLineHeight);
        var arrowRect = headerRect;
        arrowRect.width = 20;
        
        // Dibujar flecha
        var arrowContent = expanded ? "▼" : "▶";
        var arrowStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12, fontStyle = FontStyle.Bold };
        EditorGUI.LabelField(arrowRect, arrowContent, arrowStyle);
        
        // Título clickeable
        var titleRect = headerRect;
        titleRect.x += 20;
        titleRect.width -= 20;
        var newExpanded = EditorGUI.ToggleLeft(titleRect, title, expanded, EditorStyles.boldLabel);
        
        if (newExpanded != expanded)
        {
            expanded = newExpanded;
        }
        
        // Contenido colapsable
        if (expanded)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("box");
            drawContent();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
    }
}
