using UnityEngine;

/// <summary>
/// 敌人AI使用示例：展示如何创建和使用不同的AI类型
/// </summary>
public class EnemyAISample : MonoBehaviour
{
    public GameObject enemyPrefab;

    private void Start()
    {
        // 示例：创建敌人并设置不同的AI类型
        CreateEnemyWithSimpleAI();
    }

    /// <summary>
    /// 创建使用简单AI的敌人
    /// </summary>
    private void CreateEnemyWithSimpleAI()
    {
        var enemyObj = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
        var enemy = enemyObj.GetComponent<Enemy>();

        if (enemy != null)
        {
            // 设置巡逻参数
            enemy.SetPatrolCenter(enemyObj.transform.position);
            enemy.SetPatrolRadius(10f);

            Debug.Log($"创建了敌人，AI类型：{enemy.CurrentAIType}");
        }
    }

    /// <summary>
    /// 创建使用激进AI的敌人（预留接口）
    /// </summary>
    private void CreateAggressiveEnemy()
    {
        var enemyObj = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
        var enemy = enemyObj.GetComponent<Enemy>();

        if (enemy != null)
        {
            // 切换到激进AI（以后实现）
            enemy.ChangeBrain("Aggressive");
            Debug.Log($"创建了激进敌人，AI类型：{enemy.CurrentAIType}");
        }
    }
}