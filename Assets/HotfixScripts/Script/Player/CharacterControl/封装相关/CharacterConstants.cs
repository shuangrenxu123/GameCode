using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����������ƶ����м�����еĳ��������������
/// </summary>
public class CharacterConstants
{
    /// <summary>
    ///ʩ�ӵ��ӵش�������ƫ�ƣ�������棩��
    /// </summary>
    public const float GroundTriggerOffset = 0.05f;

    /// <summary>
    /// ��ֵ��ʾ��Ծ�ַ��ڽӴ�����ʱ���ֲ��ȶ���ʱ�䣨����Ϊ��λ��������ڷ�ֹ�ַ�������
    /// �����ֲ��ȶ���״̬��ͣ��̫���ˡ�
    /// </summary>
    public const float MaxUnstableGroundContactTime = 0.25f;

    /// <summary>
    /// �ϱ�Ե���±�Ե�������ԭ��֮��ľ��롣
    /// </summary>
    public const float EdgeRaysSeparation = 0.005f;

    /// <summary>
    /// ��Ե����㷨���������ߵ�Ͷ����롣
    /// </summary>
    public const float EdgeRaysCastDistance = 2f;

    /// <summary>
    ///��ײ�����ײ��״֮��Ŀռ䣨�������ѯʹ�ã���
    /// </summary>
    public const float SkinWidth = 0.005f;

    /// <summary>
    ///ʩ���ڽ��ҵײ������ϣ�����Сƫ���Ա��������Ӵ���
    /// </summary>
    public const float ColliderMinBottomOffset = 0.1f;

    /// <summary>
    ///����ߵ��Ϸ��ߺ����޷��ߣ����Ա�Ե�������֮�����С�Ƕȡ�
    /// </summary>
    public const float MinEdgeAngle = 0.5f;

    /// <summary>
    /// ����ߵ��Ϸ��ߺ����޷��ߣ����Ա߼������֮������Ƕȡ�
    /// </summary>
    public const float MaxEdgeAngle = 170f;

    /// <summary>
    /// �����Ծ���Ϸ��ߺ����޷��ߣ����Ա�Ե�������֮�����С�Ƕȡ�
    /// </summary>
    public const float MinStepAngle = 85f;

    /// <summary>
    /// �����Ծ���Ϸ��ߺ����޷��ߣ����Ա�Ե�������֮������Ƕȡ�
    /// </summary>
    public const float MaxStepAngle = 95f;

    /// <summary>
    /// ���ڵ���̽��Ļ������롣
    /// </summary>
    public const float GroundCheckDistance = 0.1f;

    /// <summary>
    ///��������ײ�ͻ����㷨������������
    /// </summary>
    public const int MaxSlideIterations = 3;

    /// <summary>
    /// ģ���ʹ�õ���ײ�ͻ����㷨�������õ�����������̬���洦����
    /// </summary>
    public const int MaxPostSimulationSlideIterations = 2;

    /// <summary>
    /// Ĭ�ϵ�����ֵ
    /// </summary>
    public const float DefaultGravity = 9.8f;

    /// <summary>
    /// ѡ��ͷ���Ӵ���ʱ���ǵ���С�Ƕ�ֵ���ýǶ����ڽӴ����ߺ͡����ϡ�ʸ��֮������ġ���Ч��Χ�ӡ�MinHeadContactAngle����180�ȡ�
    /// </summary>
    public const float HeadContactMinAngle = 100f;

    /// <summary>
    /// Tolerance value considered when choosing the "wall contact". The angle is measured between the contact normal and the "Up" vector.
    /// The valid range goes from 90 - "WallContactAngleTolerance" to 90 degrees.
    /// </summary>
    public const float WallContactAngleTolerance = 10f;

    /// <summary>
    /// ����Ԥ���ɫ�·�����ľ��롣
    /// </summary>
    public const float GroundPredictionDistance = 10f;

}