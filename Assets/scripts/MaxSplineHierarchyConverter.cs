using UnityEngine;

public class MaxSplineGlobalSpawner : MonoBehaviour
{
    [Header("Настройки объектов")]
    public GameObject batteryPrefab;       // Префаб батарейки
    public float stepBetweenItems = 2.0f;  // Расстояние между батарейками в ряду

    [Header("Защита на перекрестках")]
    public bool preventOverlap = true;      // Проверяем наложения
    public float overlapRadius = 1.0f;      // Если ближе 1м есть батарейка — скипаем
    public LayerMask batteryLayer;          // Слой батареек

    [ContextMenu("Заполнить ВЕСЬ лабиринт за один клик")]
    public void SpawnEverything()
    {
        if (batteryPrefab == null)
        {
            Debug.LogError("Слушай, ты забыл закинуть Префаб батарейки в инспектор!");
            return;
        }

        int totalSpawned = 0;

        // 1. Ищем все дочерние объекты ПЕРВОГО уровня (это сами объекты-линии: Line001, Line002 и т.д.)
        foreach (Transform lineTransform in transform)
        {
            // Собираем ВСЕ вершины (Transforms) внутри этой конкретной линии
            Transform[] points = lineTransform.GetComponentsInChildren<Transform>();

            // Если в линии меньше двух точек — её нельзя растянуть, идем к следующей
            if (points.Length < 2) continue;

            // 2. Строим ряды строго между точками внутри ОДНОЙ линии
            // Начинаем с 0, но аккуратно перебираем пары точек
            for (int i = 0; i < points.Length - 1; i++)
            {
                // Защита: не строим линию от объекта к самому себе, если Макс криво экспортировал корень
                if (points[i] == lineTransform || points[i + 1] == lineTransform) continue;

                totalSpawned += BuildRowBetweenTwoPoints(points[i].position, points[i + 1].position);
            }
        }

        Debug.Log($"Готово! Весь лабиринт заполнен. Создано батареек: {totalSpawned}");
    }

    int BuildRowBetweenTwoPoints(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        if (distance < stepBetweenItems) return 0;

        int count = Mathf.FloorToInt(distance / stepBetweenItems);
        Vector3 direction = (end - start).normalized;
        int spawnedCount = 0;

        for (int i = 0; i <= count; i++)
        {
            Vector3 spawnPos = start + (direction * (i * stepBetweenItems));

            if (preventOverlap)
            {
                // Проверяем сферу на перекрестках
                if (Physics.CheckSphere(spawnPos, overlapRadius, batteryLayer))
                {
                    continue;
                }
            }

            GameObject item = Instantiate(batteryPrefab, spawnPos, Quaternion.identity);

            // Складываем все батарейки внутрь спавнера, чтобы иерархия была чистой
            item.transform.parent = this.transform;
            spawnedCount++;
        }

        return spawnedCount;
    }
}