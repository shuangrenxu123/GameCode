using UnityEngine;

namespace Bayes
{
    public static class BayesMinimalExample
    {
        public static void RunChestTrapExample()
        {
            // 示例语义映射：Condition = Locked, Target = Trap
            var ctx = BayesSimpleContext.Create();

            // 4格联合计数：
            // Condition=true,  Target=true  -> 29
            // Condition=true,  Target=false -> 18
            // Condition=false, Target=true  -> 8
            // Condition=false, Target=false -> 45
            ctx.Set(
                conditionTrueTargetTrueCount: 29f,
                conditionTrueTargetFalseCount: 18f,
                conditionFalseTargetTrueCount: 8f,
                conditionFalseTargetFalseCount: 45f);

            float pTrapGivenLocked = ctx.GetTargetTrueProb(conditionValue: true);
            float pTrapGivenUnlocked = ctx.GetTargetTrueProb(conditionValue: false);

            Debug.Log($"[更新前] P(陷阱=真 | 上锁=真) = {pTrapGivenLocked:0.000} (约 0.617)");
            Debug.Log($"[更新前] P(陷阱=真 | 上锁=假) = {pTrapGivenUnlocked:0.000} (约 0.151)");

            float openProbLocked = 1f - pTrapGivenLocked;
            float openProbUnlocked = 1f - pTrapGivenUnlocked;
            bool openLockedNow = Random.value < openProbLocked;
            bool openUnlockedNow = Random.value < openProbUnlocked;

            Debug.Log($"[更新前] 上锁=真时的打开概率: {openProbLocked:0.000}, 本次采样是否打开={openLockedNow}");
            Debug.Log($"[更新前] 上锁=假时的打开概率: {openProbUnlocked:0.000}, 本次采样是否打开={openUnlockedNow}");

            // 增量观测：一个新箱子，上锁且有陷阱
            ctx.Update(conditionValue: true, targetValue: true);

            float pTrapGivenLockedAfter = ctx.GetTargetTrueProb(conditionValue: true);
            Debug.Log($"[更新1次后] P(陷阱=真 | 上锁=真) = {pTrapGivenLockedAfter:0.000} (约 0.625)");

            // 再来一个新箱子：不上锁且无陷阱
            ctx.Update(conditionValue: false, targetValue: false);

            float pTrapGivenUnlockedAfter = ctx.GetTargetTrueProb(conditionValue: false);
            Debug.Log($"[更新2次后] P(陷阱=真 | 上锁=假) = {pTrapGivenUnlockedAfter:0.000}");
        }

        public static void RunTwoLayerChestExample()
        {
            // 三维示例语义映射：
            // ConditionA = Trap（是否陷阱）
            // ConditionB = Treasure（是否有宝藏）
            // Target = Locked（是否上锁）
            var ctx = BayesSimpleContext3D.Create();

            // 8格联合计数顺序：
            // 1) A=T, B=T, Target=T
            // 2) A=T, B=T, Target=F
            // 3) A=T, B=F, Target=T
            // 4) A=T, B=F, Target=F
            // 5) A=F, B=T, Target=T
            // 6) A=F, B=T, Target=F
            // 7) A=F, B=F, Target=T
            // 8) A=F, B=F, Target=F
            //
            // 由旧案例数据映射得到：
            // TT: Locked=T/F = 28/12
            // TF: Locked=T/F = 14/6
            // FT: Locked=T/F = 3/7
            // FF: Locked=T/F = 9/21
            ctx.Set(
                conditionATrueConditionBTrueTargetTrueCount: 28f,
                conditionATrueConditionBTrueTargetFalseCount: 12f,
                conditionATrueConditionBFalseTargetTrueCount: 14f,
                conditionATrueConditionBFalseTargetFalseCount: 6f,
                conditionAFalseConditionBTrueTargetTrueCount: 3f,
                conditionAFalseConditionBTrueTargetFalseCount: 7f,
                conditionAFalseConditionBFalseTargetTrueCount: 9f,
                conditionAFalseConditionBFalseTargetFalseCount: 21f);

            float pLockedGivenTrapAndTreasure = ctx.GetTargetTrueProb(conditionAValue: true, conditionBValue: true);
            float pLockedGivenTrapAndNoTreasure = ctx.GetTargetTrueProb(conditionAValue: true, conditionBValue: false);
            float pLockedGivenSafeAndTreasure = ctx.GetTargetTrueProb(conditionAValue: false, conditionBValue: true);
            float pLockedGivenSafeAndNoTreasure = ctx.GetTargetTrueProb(conditionAValue: false, conditionBValue: false);

            Debug.Log($"[三维-更新前] P(上锁=真 | 陷阱=真, 宝藏=真) = {pLockedGivenTrapAndTreasure:0.000} (约 0.700)");
            Debug.Log($"[三维-更新前] P(上锁=真 | 陷阱=真, 宝藏=假) = {pLockedGivenTrapAndNoTreasure:0.000} (约 0.700)");
            Debug.Log($"[三维-更新前] P(上锁=真 | 陷阱=假, 宝藏=真) = {pLockedGivenSafeAndTreasure:0.000} (约 0.300)");
            Debug.Log($"[三维-更新前] P(上锁=真 | 陷阱=假, 宝藏=假) = {pLockedGivenSafeAndNoTreasure:0.000} (约 0.300)");

            // 增量观测：新增一个“陷阱=真、宝藏=真、上锁=真”的样本
            ctx.Update(conditionAValue: true, conditionBValue: true, targetValue: true);

            float pLockedGivenTrapAndTreasureAfter = ctx.GetTargetTrueProb(conditionAValue: true, conditionBValue: true);
            Debug.Log($"[三维-更新后] P(上锁=真 | 陷阱=真, 宝藏=真) = {pLockedGivenTrapAndTreasureAfter:0.000} (约 0.707)");
        }
    }
}
