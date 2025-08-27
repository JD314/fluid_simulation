using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AirParticlesVisibilityTest))]
public class AirParticlesVisibilityTestEditor : Editor
{
    private bool referencesExpanded = true;
    private bool testsExpanded = true;
    private bool actionsExpanded = true;
    
    public override void OnInspectorGUI()
    {
        var testScript = (AirParticlesVisibilityTest)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pruebas de Visibilidad de Partículas de Aire", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Sección de Referencias
        DrawCollapsibleSection("🔗 Referencias", ref referencesExpanded, () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("simulation"), new GUIContent("Simulación"));
        });
        
        // Sección de Configuración de Pruebas
        DrawCollapsibleSection("⚙️ Configuración de Pruebas", ref testsExpanded, () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("testOnStart"), new GUIContent("Probar al Iniciar"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showDebugInfo"), new GUIContent("Mostrar Info de Debug"));
        });
        
        EditorGUILayout.Space();
        
        // Sección de Acciones
        DrawCollapsibleSection("🎮 Acciones de Prueba", ref actionsExpanded, () =>
        {
            EditorGUILayout.LabelField("Pruebas Principales", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Probar Visibilidad del Aire", GUILayout.Height(30)))
            {
                testScript.TestAirParticlesVisibility();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pruebas Rápidas", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Prueba Rápida de Alternancia", GUILayout.Height(25)))
            {
                testScript.QuickToggleTest();
            }
            if (GUILayout.Button("Probar Preset de Aire Invisible", GUILayout.Height(25)))
            {
                testScript.TestInvisiblePreset();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Información y Debug", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Mostrar Info de Debug", GUILayout.Height(25)))
            {
                testScript.ShowDebugInfo();
            }
        });
        
        EditorGUILayout.Space();
        
        // Información adicional
        EditorGUILayout.LabelField("Información", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Este script permite probar la funcionalidad de partículas de aire invisibles y la optimización de límites Y.\n\n" +
            "• Las pruebas se ejecutan en la consola de Unity\n" +
            "• Puedes usar el menú contextual para acceder a más opciones\n" +
            "• Asegúrate de tener una simulación activa antes de ejecutar las pruebas",
            MessageType.Info
        );
        
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
