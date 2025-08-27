using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Simulation2D))]
public class Simulation2DEditor : Editor
{
    private bool simulationSettingsExpanded = true;
    private bool particleTypesExpanded = true;
    private bool interactionSettingsExpanded = true;
    private bool optimizationSettingsExpanded = true;
    private bool mazeSettingsExpanded = true;
    private bool obstacleSettingsExpanded = true;
    private bool barrierSettingsExpanded = true;
    
    public override void OnInspectorGUI()
    {
        var simulation = (Simulation2D)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Simulación de Fluidos 2D", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Sección de Configuración de Simulación
        DrawCollapsibleSection("⚙️ Configuración de Simulación", ref simulationSettingsExpanded, () =>
        {
            DrawProperty("timeScale", "Escala de Tiempo");
            DrawProperty("fixedTimeStep", "Paso de Tiempo Fijo");
            DrawProperty("iterationsPerFrame", "Iteraciones por Frame");
            DrawProperty("smoothingRadius", "Radio de Suavizado");
            DrawProperty("collisionDamping", "Amortiguación de Colisión");
            DrawProperty("boundsSize", "Tamaño de Límites");
        });
        
        // Sección de Tipos de Partículas
        DrawCollapsibleSection("🔴 Tipos de Partículas", ref particleTypesExpanded, () =>
        {
            EditorGUILayout.LabelField("Configuración de Fluido", EditorStyles.boldLabel);
            DrawParticleTypeConfig("Fluido", serializedObject.FindProperty("fluidConfig"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Configuración de Aire", EditorStyles.boldLabel);
            DrawParticleTypeConfig("Aire", serializedObject.FindProperty("airConfig"));
        });
        
        // Sección de Configuración de Interacción
        DrawCollapsibleSection("🎯 Configuración de Interacción", ref interactionSettingsExpanded, () =>
        {
            DrawProperty("interactionRadius", "Radio de Interacción");
            DrawProperty("interactionStrength", "Fuerza de Interacción");
            DrawProperty("obstacleEdgeThreshold", "Umbral de Borde de Obstáculo");
            DrawProperty("obstacleOverlapOffset", "Desplazamiento de Superposición");
            DrawProperty("fluidAirInteractionStrength", "Fuerza de Interacción Fluido-Aire");
        });
        
        // Sección de Configuración de Optimización
        DrawCollapsibleSection("🚀 Configuración de Optimización", ref optimizationSettingsExpanded, () =>
        {
            DrawProperty("enableAirCleanup", "Habilitar Limpieza de Aire");
            DrawProperty("airCleanupFrequency", "Frecuencia de Limpieza");
            DrawProperty("maxBatchSize", "Tamaño Máximo de Lote");
        });
        
        // Sección de Configuración de Laberinto
        DrawCollapsibleSection("🏗️ Configuración de Laberinto", ref mazeSettingsExpanded, () =>
        {
            DrawProperty("displayObstacle", "Mostrar Obstáculos");
            DrawProperty("useMaze", "Usar Laberinto");
            
            // Selector de archivo CSV con botón
            EditorGUILayout.BeginHorizontal();
            var obstacleFilePathProp = serializedObject.FindProperty("obstacleFilePath");
            EditorGUILayout.PropertyField(obstacleFilePathProp, new GUIContent("Archivo de Obstáculos"));
            
            if (GUILayout.Button("📁 Seleccionar", GUILayout.Width(100)))
            {
                string initialPath = "Assets/mazes_csv/";
                if (!string.IsNullOrEmpty(obstacleFilePathProp.stringValue))
                {
                    initialPath = obstacleFilePathProp.stringValue;
                }
                
                string selectedPath = EditorUtility.OpenFilePanel("Seleccionar Archivo CSV", initialPath, "csv");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // Convertir a ruta relativa si es posible
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else if (selectedPath.StartsWith("Assets/"))
                    {
                        // Ya es una ruta relativa
                    }
                    else
                    {
                        // Si no es relativa, intentar hacerla relativa a mazes_csv
                        string fileName = System.IO.Path.GetFileName(selectedPath);
                        selectedPath = "mazes_csv/" + fileName;
                    }
                    
                    obstacleFilePathProp.stringValue = selectedPath;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Botones de acción para el laberinto
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Recargar Laberinto", GUILayout.Height(25)))
            {
                simulation.ReloadObstaclesFromFile();
            }
            if (GUILayout.Button("💾 Guardar Laberinto", GUILayout.Height(25)))
            {
                simulation.SaveObstaclesToFile();
            }
            EditorGUILayout.EndHorizontal();
            
            DrawProperty("obstacleOverlapOffset", "Desplazamiento de Superposición");
        });
        
        // Sección de Configuración de Obstáculos
        DrawCollapsibleSection("🧱 Configuración de Obstáculos", ref obstacleSettingsExpanded, () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("obstacles"), new GUIContent("Obstáculos"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnRegions"), new GUIContent("Regiones de Spawn"), true);
        });
        
        // Sección de Configuración de Barreras
        DrawCollapsibleSection("🚧 Configuración de Barreras", ref barrierSettingsExpanded, () =>
        {
            DrawProperty("enableHorizontalBarriers", "Habilitar Barreras Horizontales");
            DrawProperty("barrierStartY", "Y de Inicio de Barrera");
            DrawProperty("barrierSpacing", "Espaciado de Barreras");
            DrawProperty("barrierWidth", "Ancho de Barrera");
            DrawProperty("barrierHeight", "Alto de Barrera");
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
    
    private void DrawProperty(string propertyName, string displayName)
    {
        var prop = serializedObject.FindProperty(propertyName);
        if (prop != null)
        {
            EditorGUILayout.PropertyField(prop, new GUIContent(displayName));
        }
    }
    
    private void DrawParticleTypeConfig(string title, SerializedProperty configProperty)
    {
        if (configProperty == null) return;
        
        EditorGUI.indentLevel++;
        
        // Sección Física
        var physicsExpanded = configProperty.FindPropertyRelative("physicsExpanded");
        if (physicsExpanded != null)
        {
            var physicsHeaderExpanded = EditorGUILayout.Foldout(physicsExpanded.boolValue, "🔬 Física", true);
            if (physicsHeaderExpanded != physicsExpanded.boolValue)
            {
                physicsExpanded.boolValue = physicsHeaderExpanded;
            }
            
            if (physicsExpanded.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("gravity"), new GUIContent("Gravedad"));
                EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("targetDensity"), new GUIContent("Densidad Objetivo"));
                EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("pressureMultiplier"), new GUIContent("Multiplicador de Presión"));
                EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("nearPressureMultiplier"), new GUIContent("Multiplicador de Presión Cercana"));
                EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("viscosityStrength"), new GUIContent("Fuerza de Viscosidad"));
                EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("mass"), new GUIContent("Masa"));
                EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("compressibility"), new GUIContent("Compresibilidad"));
                EditorGUI.indentLevel--;
            }
        }
        
        // Sección Visual
        var visualExpanded = configProperty.FindPropertyRelative("visualExpanded");
        if (visualExpanded != null)
        {
            var visualHeaderExpanded = EditorGUILayout.Foldout(visualExpanded.boolValue, "👁️ Visual", true);
            if (visualHeaderExpanded != visualExpanded.boolValue)
            {
                visualExpanded.boolValue = visualHeaderExpanded;
            }
            
            if (visualExpanded.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("particleScale"), new GUIContent("Escala de Partícula"));
                EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("airParticlesInvisible"), new GUIContent("Partículas de Aire Invisibles"));
                EditorGUI.indentLevel--;
            }
        }
        
        // Sección Spawn
        var spawnExpanded = configProperty.FindPropertyRelative("spawnExpanded");
        if (spawnExpanded != null)
        {
            var spawnHeaderExpanded = EditorGUILayout.Foldout(spawnExpanded.boolValue, "🎯 Spawn", true);
            if (spawnHeaderExpanded != spawnExpanded.boolValue)
            {
                spawnExpanded.boolValue = spawnHeaderExpanded;
            }
            
            if (spawnExpanded.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("particleCount"), new GUIContent("Cantidad de Partículas"));
                EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("initialVelocity"), new GUIContent("Velocidad Inicial"));
                EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("jitterStrength"), new GUIContent("Fuerza de Jitter"));
                EditorGUI.indentLevel--;
            }
        }
        
        // Sección Optimización (solo para aire)
        if (title == "Aire")
        {
            var optimizationExpanded = configProperty.FindPropertyRelative("optimizationExpanded");
            if (optimizationExpanded != null)
            {
                var optimizationHeaderExpanded = EditorGUILayout.Foldout(optimizationExpanded.boolValue, "🚀 Optimización (Solo Aire)", true);
                if (optimizationHeaderExpanded != optimizationExpanded.boolValue)
                {
                    optimizationExpanded.boolValue = optimizationHeaderExpanded;
                }
                
                if (optimizationExpanded.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("yMin"), new GUIContent("Y Mínimo"));
                    EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("yMax"), new GUIContent("Y Máximo"));
                    EditorGUILayout.PropertyField(configProperty.FindPropertyRelative("showAirBounds"), new GUIContent("Mostrar Límites del Aire"));
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        EditorGUI.indentLevel--;
    }
}
