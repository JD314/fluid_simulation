using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class ObstacleLoader
{
    [System.Serializable]
    public struct ObstacleData
    {
        public float posX;
        public float posY;
        public float width;
        public float height;
        
        public ObstacleData(float x, float y, float w, float h)
        {
            posX = x;
            posY = y;
            width = w;
            height = h;
        }
    }
    
    public static List<ObstacleRectangle> LoadObstaclesFromCSV(string filePath)
    {
        List<ObstacleRectangle> obstacles = new List<ObstacleRectangle>();
        
        try
        {
            // Construir la ruta completa del archivo
            // Si la ruta no incluye la carpeta mazes_csv, la agregamos automáticamente
            if (!filePath.Contains("mazes_csv"))
            {
                filePath = Path.Combine("mazes_csv", filePath);
            }
            string fullPath = Path.Combine(Application.dataPath, filePath);
            
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"Archivo de obstáculos no encontrado: {fullPath}");
                return obstacles;
            }
            
            string[] lines = File.ReadAllLines(fullPath);
            
            // Saltar la primera línea (encabezados)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                
                string[] values = line.Split(',');
                if (values.Length >= 4)
                {
                    if (float.TryParse(values[0], out float posX) &&
                        float.TryParse(values[1], out float posY) &&
                        float.TryParse(values[2], out float width) &&
                        float.TryParse(values[3], out float height))
                    {
                        ObstacleRectangle obstacle = new ObstacleRectangle(
                            new Vector2(posX, posY),
                            new Vector2(width, height)
                        );
                        obstacles.Add(obstacle);
                        Debug.Log($"Obstáculo cargado: Pos({posX}, {posY}), Tamaño({width}, {height})");
                    }
                    else
                    {
                        Debug.LogWarning($"Error al parsear línea {i + 1}: {line}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Línea {i + 1} no tiene suficientes valores: {line}");
                }
            }
            
            Debug.Log($"Se cargaron {obstacles.Count} obstáculos desde {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al cargar obstáculos desde {filePath}: {e.Message}");
        }
        
        return obstacles;
    }
    
    public static void SaveObstaclesToCSV(List<ObstacleRectangle> obstacles, string filePath)
    {
        try
        {
            // Si la ruta no incluye la carpeta mazes_csv, la agregamos automáticamente
            if (!filePath.Contains("mazes_csv"))
            {
                filePath = Path.Combine("mazes_csv", filePath);
            }
            string fullPath = Path.Combine(Application.dataPath, filePath);
            List<string> lines = new List<string>();
            
            // Agregar encabezados
            lines.Add("pos_x,pos_y,width,height");
            
            // Agregar datos de obstáculos
            foreach (var obstacle in obstacles)
            {
                string line = $"{obstacle.position.x},{obstacle.position.y},{obstacle.size.x},{obstacle.size.y}";
                lines.Add(line);
            }
            
            File.WriteAllLines(fullPath, lines);
            Debug.Log($"Obstáculos guardados en {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar obstáculos en {filePath}: {e.Message}");
        }
    }
}
