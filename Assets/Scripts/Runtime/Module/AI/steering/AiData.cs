using System.Collections.Generic;
using UnityEngine;

public class AiData : MonoBehaviour
{
    [SerializeField]
    public CombatNumberBox CombatNumberBox;
    [HideInInspector]
    /// <summary>
    /// Ŀ������
    /// </summary>
    public List<Transform> targets = null;
    [HideInInspector]
    /// <summary>
    /// �ϰ�������
    /// </summary>
    public Collider2D[] obstacles = null;
    [HideInInspector]
    /// <summary>
    /// �Ƿ�õ��漴Ŀ��
    /// </summary>
    public bool reachedLastTarget = true;
    [HideInInspector]
    /// <summary>
    /// ��ǰӦ�õ����Ŀ���
    /// </summary>
    public Transform currentTarget;
    [HideInInspector]
    /// <summary>
    /// ����Ŀ��
    /// </summary>
    public Transform enemy;
    [HideInInspector]
    /// <summary>
    /// �Լ�������
    /// </summary>
    public Transform me;

    private void Awake()
    {
        CombatNumberBox = new CombatNumberBox();
        CombatNumberBox.Init();
    }
    private void Start()
    {
        me = gameObject.transform;
    }
    public int GetTargetsCount() => targets == null ? 0 : targets.Count;
}
