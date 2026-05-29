using System.Collections.Generic;
using UnityEngine;

public class BatterySpawner : MonoBehaviour
{
    public GameObject batteryPrefab;

    [Header("Spawn Settings")]
    public float spawnHeight = 0.3f;
    public float step = 2.0f; // Строгий шаг сетки

    void Start()
    {
        if (batteryPrefab == null) return;

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) return;

        // 1. Привязываемся к жесткой математической сетке 2х2 метра относительно игрока
        float startX = Mathf.Round(player.transform.position.x / step) * step;
        float startZ = Mathf.Round(player.transform.position.z / step) * step;

        Vector3 startNode = new Vector3(startX, spawnHeight, startZ);

        Queue<Vector3> queue = new Queue<Vector3>();
        HashSet<Vector3> visited = new HashSet<Vector3>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        int spawnedCount = 0;
        // 4 направления для ровной сетки
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        // Ограничение в 5000 шагов (чтобы хватило на весь лабиринт)
        while (queue.Count > 0 && spawnedCount < 5000)
        {
            Vector3 currentPos = queue.Dequeue();

            // 2. Проверяем, есть ли в этой точке СТЕНА лабиринта
            Vector3 checkPos = new Vector3(currentPos.x, 1.5f, currentPos.z);
            Collider[] colliders = Physics.OverlapSphere(checkPos, 0.4f);

            bool isWall = false;
            foreach (Collider col in colliders)
            {
                // ИЩЕМ СТРОГО ОБЪЕКТ СТЕН! (Это полностью игнорирует Игрока, пол и потолок)
                if (col.name.ToLower().Contains("walls"))
                {
                    isWall = true;
                    break;
                }
            }

            // 3. Если мы не в стене (это чистый проход) — ставим батарейку и шагаем во все 4 стороны
            if (!isWall)
            {
                Instantiate(batteryPrefab, currentPos, Quaternion.identity, transform);
                spawnedCount++;

                foreach (Vector3 dir in directions)
                {
                    Vector3 neighbor = currentPos + dir * step;

                    // Округляем координаты, чтобы сетка не искажалась
                    neighbor.x = Mathf.Round(neighbor.x * 10f) / 10f;
                    neighbor.z = Mathf.Round(neighbor.z * 10f) / 10f;

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        Debug.Log("Математическая заливка завершена! Создано идеальных рядов: " + spawnedCount + " батареек.");
    }
}