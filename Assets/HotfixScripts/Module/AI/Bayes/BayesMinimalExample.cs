using UnityEngine;

namespace Bayes
{
    public static class BayesMinimalExample
    {
        public static void RunChestTrapExample()
        {
            // 使用简化封装：二值变量 + 计数建模
            var ctx = BayesSimpleContext.Create();
            ctx.DefineBoolVar("Trap");
            ctx.DefineBoolVar("Locked");

            // 初始经验统计（100 个箱子）
            float trapCount = 37f;
            float safeCount = 63f;
            float lockedWhenTrap = 29f;
            float unlockedWhenTrap = 8f;
            float lockedWhenSafe = 18f;
            float unlockedWhenSafe = 45f;

            // 初始化参数（一次调用设置 P(Trap) 和 P(Locked|Trap)）
            ctx.SetByCount(
                varName: "Locked",
                parentVar: "Trap",
                parentTrueCount: trapCount,
                parentFalseCount: safeCount,
                childTrueWhenParentTrue: lockedWhenTrap,
                childFalseWhenParentTrue: unlockedWhenTrap,
                childTrueWhenParentFalse: lockedWhenSafe,
                childFalseWhenParentFalse: unlockedWhenSafe);

            // 看到上锁箱子，求 P(Trap=T | Locked=T)
            float pTrapGivenLocked = ctx.PosteriorTrue("Trap", BoolEvidence.Of("Locked", true));

            // 看到不上锁箱子，求 P(Trap=T | Locked=F)
            float pTrapGivenUnlocked = ctx.PosteriorTrue("Trap", BoolEvidence.Of("Locked", false));

            Debug.Log($"[Before Update] P(Trap=T | Locked=T) = {pTrapGivenLocked:0.000} (约 0.61)");
            Debug.Log($"[Before Update] P(Trap=T | Locked=F) = {pTrapGivenUnlocked:0.000} (约 0.15)");

            // 一个简单决策示例：把“打开概率”映射为 1 - 陷阱概率，再做一次轮盘赌采样
            float openProbLocked = 1f - pTrapGivenLocked;
            float openProbUnlocked = 1f - pTrapGivenUnlocked;
            bool openLockedNow = Random.value < openProbLocked;
            bool openUnlockedNow = Random.value < openProbUnlocked;

            Debug.Log($"[Before Update] OpenProb when Locked=T: {openProbLocked:0.000}, sampledOpen={openLockedNow}");
            Debug.Log($"[Before Update] OpenProb when Locked=F: {openProbUnlocked:0.000}, sampledOpen={openUnlockedNow}");

            // ===== 更新逻辑：NPC 获得新增经验后重估参数 =====
            // 新增 40 个样本：其中陷阱 18（上锁 16，不上锁 2），非陷阱 22（上锁 6，不上锁 16）
            trapCount += 18f;
            safeCount += 22f;
            lockedWhenTrap += 16f;
            unlockedWhenTrap += 2f;
            lockedWhenSafe += 6f;
            unlockedWhenSafe += 16f;

            // 用累计数据重新估计概率（一次调用完成更新）
            ctx.SetByCount(
                varName: "Locked",
                parentVar: "Trap",
                parentTrueCount: trapCount,
                parentFalseCount: safeCount,
                childTrueWhenParentTrue: lockedWhenTrap,
                childFalseWhenParentTrue: unlockedWhenTrap,
                childTrueWhenParentFalse: lockedWhenSafe,
                childFalseWhenParentFalse: unlockedWhenSafe);

            float pTrapGivenLockedAfter = ctx.PosteriorTrue("Trap", BoolEvidence.Of("Locked", true));
            float pTrapGivenUnlockedAfter = ctx.PosteriorTrue("Trap", BoolEvidence.Of("Locked", false));

            Debug.Log($"[After Update]  P(Trap=T | Locked=T) = {pTrapGivenLockedAfter:0.000}");
            Debug.Log($"[After Update]  P(Trap=T | Locked=F) = {pTrapGivenUnlockedAfter:0.000}");

            float openProbLockedAfter = 1f - pTrapGivenLockedAfter;
            float openProbUnlockedAfter = 1f - pTrapGivenUnlockedAfter;
            bool openLockedAfter = Random.value < openProbLockedAfter;
            bool openUnlockedAfter = Random.value < openProbUnlockedAfter;

            Debug.Log($"[After Update]  OpenProb when Locked=T: {openProbLockedAfter:0.000}, sampledOpen={openLockedAfter}");
            Debug.Log($"[After Update]  OpenProb when Locked=F: {openProbUnlockedAfter:0.000}, sampledOpen={openUnlockedAfter}");
        }

        public static void RunTwoLayerChestExample()
        {
            // 二层网络：Treasure -> Trap，且 Treasure/Trap 共同影响 Locked
            // 结构：Treasure -> Trap -> Locked，同时 Treasure -> Locked
            var ctx = BayesSimpleContext.Create();
            ctx.DefineBoolVar("Treasure");
            ctx.DefineBoolVar("Trap");
            ctx.DefineBoolVar("Locked");

            // P(Treasure)
            ctx.SetByCount("Treasure", trueCount: 50f, falseCount: 50f);

            // P(Trap | Treasure)
            // Treasure=T: Trap=T/F = 40/10
            // Treasure=F: Trap=T/F = 20/30
            ctx.SetByCount(
                varName: "Trap",
                parentVar: "Treasure",
                childTrueWhenParentTrue: 40f,
                childFalseWhenParentTrue: 10f,
                childTrueWhenParentFalse: 20f,
                childFalseWhenParentFalse: 30f);

            // P(Locked | Trap, Treasure)
            // 组合顺序按 (Trap, Treasure): TT, TF, FT, FF
            // 已知数据：
            // Treasure=T, Trap=T: Locked=T/F = 28/12  -> TT
            // Treasure=T, Trap=F: Locked=T/F = 3/7   -> FT
            // 下面两项为补全示例（与 P(L)=0.54 对齐）：
            // Treasure=F, Trap=T: Locked=T/F = 14/6  -> TF
            // Treasure=F, Trap=F: Locked=T/F = 9/21  -> FF
            ctx.SetByCount(
                varName: "Locked",
                parentA: "Trap",
                parentB: "Treasure",
                childTrueWhenTT: 28f,
                childFalseWhenTT: 12f,
                childTrueWhenTF: 14f,
                childFalseWhenTF: 6f,
                childTrueWhenFT: 3f,
                childFalseWhenFT: 7f,
                childTrueWhenFF: 9f,
                childFalseWhenFF: 21f);

            // 证据：看到一个上锁宝箱，求“是否陷阱/是否有宝藏”的后验
            float pTrapGivenLocked = ctx.PosteriorTrue("Trap", BoolEvidence.Of("Locked", true));
            float pTreasureGivenLocked = ctx.PosteriorTrue("Treasure", BoolEvidence.Of("Locked", true));

            Debug.Log($"[2-Layer] P(Trap=T | Locked=T) = {pTrapGivenLocked:0.000} (约 0.78)");
            Debug.Log($"[2-Layer] P(Treasure=T | Locked=T) = {pTreasureGivenLocked:0.000}");
        }
    }
}

