using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GOAP.Tests
{
    [TestFixture]
    [Category("GOAP")]
    public class GoapRuntimeTests
    {
        private static int nextActionIndex;

        [SetUp]
        public void SetUp()
        {
            nextActionIndex = 0;
        }

        private static ulong NextActionMask()
        {
            if (nextActionIndex >= 64)
            {
                throw new InvalidOperationException(
                    "单个测试规划最多支持 64 个逻辑 Action。");
            }

            return 1UL << nextActionIndex++;
        }

        private sealed class TestAction : GoapAction<string, bool>
        {
            private readonly List<string> executionOrder;

            public override ulong ActionMask { get; }

            public bool ProceduralCondition { get; set; } = true;
            public bool CompleteOnExecute { get; set; } = true;
            public int EnterCount { get; private set; }
            public int ExecuteCount { get; private set; }
            public int ExitCount { get; private set; }

            public TestAction(
                string actionName,
                float actionCost,
                Dictionary<string, bool> actionPreconditions,
                Dictionary<string, bool> actionEffects,
                List<string> executionOrder = null,
                ulong actionMask = ulong.MaxValue)
            {
                name = actionName;
                cost = actionCost;
                this.executionOrder = executionOrder;
                ActionMask = actionMask == ulong.MaxValue
                    ? NextActionMask()
                    : actionMask;

                if (actionPreconditions != null)
                {
                    foreach (var condition in actionPreconditions)
                    {
                        preconditions.Add(condition.Key, condition.Value);
                    }
                }

                if (actionEffects != null)
                {
                    foreach (var effect in actionEffects)
                    {
                        effects.Add(effect.Key, effect.Value);
                    }
                }
            }

            protected override void Reset()
            {
                executed = false;
            }

            public override bool CheckProceduralPreCondition(Dictionary<string, bool> state)
            {
                return ProceduralCondition;
            }

            public override void PlanEnter()
            {
                EnterCount++;
                base.PlanEnter();
            }

            public override void PlanExecute()
            {
                ExecuteCount++;
                executionOrder?.Add(name);

                if (CompleteOnExecute)
                {
                    executed = true;
                }
            }

            public override void PlanExit()
            {
                ExitCount++;
                base.PlanExit();
            }
        }

        private sealed class IntEffectAction : GoapAction<string, int>
        {
            public override ulong ActionMask { get; }

            public IntEffectAction(
                string key,
                int value,
                ulong actionMask = ulong.MaxValue)
            {
                name = "IntEffect";
                ActionMask = actionMask == ulong.MaxValue
                    ? NextActionMask()
                    : actionMask;
                effects.Add(key, value);
            }

            protected override void Reset()
            {
                executed = false;
            }

            public override bool CheckProceduralPreCondition(Dictionary<string, int> state)
            {
                return true;
            }
        }

        private sealed class IntPlanningAction : GoapAction<string, int>
        {
            private readonly bool proceduralCondition;

            public override ulong ActionMask { get; }

            public IntPlanningAction(
                string actionName,
                float actionCost,
                Dictionary<string, int> actionPreconditions,
                Dictionary<string, int> actionEffects,
                bool proceduralCondition = true,
                ulong actionMask = ulong.MaxValue)
            {
                name = actionName;
                cost = actionCost;
                this.proceduralCondition = proceduralCondition;
                ActionMask = actionMask == ulong.MaxValue
                    ? NextActionMask()
                    : actionMask;

                CopyConditions(actionPreconditions, preconditions);
                CopyConditions(actionEffects, effects);
            }

            protected override void Reset()
            {
                executed = false;
            }

            public override bool CheckProceduralPreCondition(Dictionary<string, int> state)
            {
                return proceduralCondition;
            }

            private static void CopyConditions(
                Dictionary<string, int> source,
                Dictionary<string, int> destination)
            {
                if (source == null)
                {
                    return;
                }

                foreach (KeyValuePair<string, int> condition in source)
                {
                    destination.Add(condition.Key, condition.Value);
                }
            }
        }

        private readonly struct CollisionKey
        {
            public readonly int Id;

            public CollisionKey(int id)
            {
                Id = id;
            }

            public override bool Equals(object obj)
            {
                return obj is CollisionKey other && Id == other.Id;
            }

            public override int GetHashCode()
            {
                return 0;
            }
        }

        private sealed class CollisionAction : GoapAction<CollisionKey, bool>
        {
            public override ulong ActionMask { get; }

            public CollisionAction(
                ulong actionMask,
                Dictionary<CollisionKey, bool> actionPreconditions,
                Dictionary<CollisionKey, bool> actionEffects)
            {
                ActionMask = actionMask;

                if (actionPreconditions != null)
                {
                    foreach (KeyValuePair<CollisionKey, bool> condition in actionPreconditions)
                    {
                        preconditions.Add(condition.Key, condition.Value);
                    }
                }

                foreach (KeyValuePair<CollisionKey, bool> effect in actionEffects)
                {
                    effects.Add(effect.Key, effect.Value);
                }
            }

            protected override void Reset()
            {
                executed = false;
            }

            public override bool CheckProceduralPreCondition(
                Dictionary<CollisionKey, bool> state)
            {
                return true;
            }
        }

        [Test]
        public void Planner_WhenActionMaskIsZero_ThrowsArgumentException()
        {
            var planner = new GoapPlanner<string, bool>();
            var actions = new HashSet<GoapAction<string, bool>>
            {
                new TestAction("Invalid", 1f, null, State("Done"), actionMask: 0UL)
            };

            Assert.Throws<ArgumentException>(() =>
                planner.Plan(actions, new Dictionary<string, bool>(), State("Done")));
        }

        [Test]
        public void Planner_WhenActionMaskContainsMultipleBits_ThrowsArgumentException()
        {
            var planner = new GoapPlanner<string, bool>();
            var actions = new HashSet<GoapAction<string, bool>>
            {
                new TestAction("Invalid", 1f, null, State("Done"), actionMask: 3UL)
            };

            Assert.Throws<ArgumentException>(() =>
                planner.Plan(actions, new Dictionary<string, bool>(), State("Done")));
        }

        [Test]
        public void AddAction_WhenActionMaskIsDuplicated_ThrowsArgumentException()
        {
            var agent = new GoapAgent<string, bool>();
            agent.AddAction(new TestAction(
                "First",
                1f,
                null,
                State("FirstDone"),
                actionMask: 1UL));

            Assert.Throws<ArgumentException>(() => agent.AddAction(new TestAction(
                "Second",
                1f,
                null,
                State("SecondDone"),
                actionMask: 1UL)));
        }

        [Test]
        public void Planner_WhenActionMaskIsDuplicated_ThrowsArgumentException()
        {
            var planner = new GoapPlanner<string, bool>();
            var actions = new HashSet<GoapAction<string, bool>>
            {
                new TestAction("First", 1f, null, State("FirstDone"), actionMask: 1UL),
                new TestAction("Second", 1f, null, State("SecondDone"), actionMask: 1UL)
            };

            Assert.Throws<ArgumentException>(() => planner.Plan(
                actions,
                new Dictionary<string, bool>(),
                State("FirstDone")));
        }

        [Test]
        public void RemoveAction_ReleasesActionMaskForAnotherAction()
        {
            var agent = new GoapAgent<string, bool>();
            var first = new TestAction(
                "First",
                1f,
                null,
                State("FirstDone"),
                actionMask: 1UL);

            agent.AddAction(first);
            agent.RemoveAction(first);

            Assert.DoesNotThrow(() => agent.AddAction(new TestAction(
                "Second",
                1f,
                null,
                State("SecondDone"),
                actionMask: 1UL)));
        }

        [Test]
        public void Planner_ActionMaskUsingHighestBit_CreatesPlan()
        {
            var planner = new GoapPlanner<string, bool>();
            var actions = new HashSet<GoapAction<string, bool>>
            {
                new TestAction(
                    "HighestBit",
                    1f,
                    null,
                    State("Done"),
                    actionMask: 1UL << 63)
            };

            Queue<GoapAction<string, bool>> plan = planner.Plan(
                actions,
                new Dictionary<string, bool>(),
                State("Done"));

            Assert.That(plan, Is.Not.Null);
            Assert.That(plan.Count, Is.EqualTo(1));
        }

        [Test]
        public void Planner_WhenDifferentOrdersReachSameSearchState_DeduplicatesNode()
        {
            var planner = new GoapPlanner<string, bool>();
            var actions = new HashSet<GoapAction<string, bool>>
            {
                new TestAction("SetA", 1f, null, State("A")),
                new TestAction("SetB", 1f, null, State("B")),
                new TestAction("SetC", 1f, null, State("C"))
            };

            Queue<GoapAction<string, bool>> plan = planner.Plan(
                actions,
                new Dictionary<string, bool>(),
                Conditions(("A", true), ("B", true), ("C", true)));

            Assert.That(plan, Is.Not.Null);
            Assert.That(plan.Count, Is.EqualTo(3));
            Assert.That(planner.LastDeduplicatedNodeCount, Is.GreaterThan(0));
        }

        [Test]
        public void Planner_InPlaceRegression_RestoresBranchesAndResolvesHashCollisions()
        {
            var x = new CollisionKey(1);
            var y = new CollisionKey(2);
            var z = new CollisionKey(3);
            var p = new CollisionKey(4);

            var actionA = new CollisionAction(
                1UL << 0,
                CollisionConditions((z, true)),
                CollisionConditions((x, true), (p, true)));
            var actionB = new CollisionAction(
                1UL << 1,
                CollisionConditions((p, true)),
                CollisionConditions((y, true), (z, true)));
            var actionC = new CollisionAction(
                1UL << 2,
                null,
                CollisionConditions((z, true)));

            var planner = new GoapPlanner<CollisionKey, bool>();
            Queue<GoapAction<CollisionKey, bool>> plan = planner.Plan(
                new HashSet<GoapAction<CollisionKey, bool>>
                {
                    actionA,
                    actionB,
                    actionC
                },
                new Dictionary<CollisionKey, bool>(),
                CollisionConditions((x, true), (y, true)));

            Assert.That(plan, Is.Not.Null);
            Assert.That(plan.Count, Is.EqualTo(3));
            Assert.That(plan.Dequeue(), Is.SameAs(actionC));
            Assert.That(plan.Dequeue(), Is.SameAs(actionA));
            Assert.That(plan.Dequeue(), Is.SameAs(actionB));
            Assert.That(planner.LastHashCollisionStateCount, Is.GreaterThan(0));
        }

        [Test]
        public void TwoStepPlan_ExecutesInDependencyOrderAndReachesGoal()
        {
            var executionOrder = new List<string>();
            var agent = new GoapAgent<string, bool>();
            var collect = new TestAction(
                "CollectResource",
                1f,
                null,
                State("HasResource"),
                executionOrder);
            var craft = new TestAction(
                "CraftItem",
                1f,
                State("HasResource"),
                State("HasItem"),
                executionOrder);

            agent.AddAction(collect);
            agent.AddAction(craft);
            agent.AddGoal(new Goal<string, bool>(State("HasItem"), 10));

            Assert.That(agent.ForceBuildPlan(), Is.True);

            agent.RunPlan();
            agent.RunPlan();

            Assert.That(executionOrder, Is.EqualTo(new[] { "CollectResource", "CraftItem" }));
            Assert.That(agent.WorldState["HasResource"], Is.True);
            Assert.That(agent.WorldState["HasItem"], Is.True);
            Assert.That(collect.EnterCount, Is.EqualTo(1));
            Assert.That(collect.ExitCount, Is.EqualTo(1));
            Assert.That(craft.EnterCount, Is.EqualTo(1));
            Assert.That(craft.ExitCount, Is.EqualTo(1));
        }

        [Test]
        public void BuildPlan_WhenHighestPriorityGoalHasNoPlan_UsesNextGoal()
        {
            var agent = new GoapAgent<string, bool>();
            var reachableAction = new TestAction(
                "ReachFallbackGoal",
                1f,
                null,
                State("FallbackGoalReached"));

            agent.AddAction(reachableAction);
            agent.AddGoal(new Goal<string, bool>(State("FallbackGoalReached"), 10));
            agent.AddGoal(new Goal<string, bool>(State("UnreachableGoal"), 100));

            LogAssert.ignoreFailingMessages = true;
            try
            {
                Assert.That(agent.ForceBuildPlan(), Is.True);
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }

            agent.RunPlan();

            Assert.That(agent.WorldState["FallbackGoalReached"], Is.True);
            Assert.That(reachableAction.ExecuteCount, Is.EqualTo(1));
        }

        [Test]
        public void ForceBuildPlan_ExitsCurrentActionBeforeReplacingPlan()
        {
            var agent = new GoapAgent<string, bool>();
            var longRunningAction = new TestAction(
                "LongRunning",
                1f,
                null,
                State("GoalReached"))
            {
                CompleteOnExecute = false
            };

            agent.AddAction(longRunningAction);
            agent.AddGoal(new Goal<string, bool>(State("GoalReached"), 10));

            Assert.That(agent.ForceBuildPlan(), Is.True);
            agent.RunPlan();
            Assert.That(longRunningAction.Running, Is.True);

            Assert.That(agent.ForceBuildPlan(), Is.True);

            Assert.That(longRunningAction.ExitCount, Is.EqualTo(1));
            Assert.That(longRunningAction.Running, Is.False);
        }

        [Test]
        public void RunPlan_WhenCurrentActionBecomesInvalid_ReplansToFallbackAction()
        {
            var executionOrder = new List<string>();
            var agent = new GoapAgent<string, bool>();
            var primaryAction = new TestAction(
                "Primary",
                1f,
                null,
                State("GoalReached"),
                executionOrder)
            {
                CompleteOnExecute = false
            };
            var fallbackAction = new TestAction(
                "Fallback",
                10f,
                null,
                State("GoalReached"),
                executionOrder);

            agent.AddAction(primaryAction);
            agent.AddAction(fallbackAction);
            agent.AddGoal(new Goal<string, bool>(State("GoalReached"), 10));

            Assert.That(agent.ForceBuildPlan(), Is.True);
            agent.RunPlan();

            primaryAction.ProceduralCondition = false;
            agent.RunPlan();
            agent.RunPlan();

            Assert.That(primaryAction.ExitCount, Is.EqualTo(1));
            Assert.That(fallbackAction.ExecuteCount, Is.EqualTo(1));
            Assert.That(agent.WorldState["GoalReached"], Is.True);
            Assert.That(executionOrder, Is.EqualTo(new[] { "Primary", "Fallback" }));
        }

        [Test]
        public void MultiConditionPlan_CombinesIndependentActionsBeforeFinalAction()
        {
            var executionOrder = new List<string>();
            var agent = new GoapAgent<string, bool>();
            var collectWood = new TestAction(
                "CollectWood",
                2f,
                null,
                State("HasWood"),
                executionOrder);
            var getTool = new TestAction(
                "GetTool",
                3f,
                null,
                State("HasTool"),
                executionOrder);
            var craftItem = new TestAction(
                "CraftItem",
                1f,
                Conditions(("HasWood", true), ("HasTool", true)),
                State("HasItem"),
                executionOrder);

            agent.AddAction(collectWood);
            agent.AddAction(getTool);
            agent.AddAction(craftItem);
            agent.AddGoal(new Goal<string, bool>(State("HasItem"), 10));

            Assert.That(agent.ForceBuildPlan(), Is.True);

            agent.RunPlan();
            agent.RunPlan();
            agent.RunPlan();

            int woodIndex = executionOrder.IndexOf("CollectWood");
            int toolIndex = executionOrder.IndexOf("GetTool");
            int craftIndex = executionOrder.IndexOf("CraftItem");

            Assert.That(woodIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(toolIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(craftIndex, Is.GreaterThan(woodIndex));
            Assert.That(craftIndex, Is.GreaterThan(toolIndex));
            Assert.That(agent.WorldState["HasItem"], Is.True);
        }

        [Test]
        public void Planner_SelectsLowestTotalCostPlan()
        {
            var actions = new HashSet<GoapAction<string, bool>>
            {
                new TestAction("BuyWood", 10f, null, State("HasWood")),
                new TestAction("CollectWood", 2f, null, State("HasWood")),
                new TestAction("CraftItem", 1f, State("HasWood"), State("HasItem"))
            };
            var planner = new GoapPlanner<string, bool>();

            Queue<GoapAction<string, bool>> plan =
                planner.Plan(actions, new Dictionary<string, bool>(), State("HasItem"));

            Assert.That(plan, Is.Not.Null);
            Assert.That(plan.Count, Is.EqualTo(2));
            Assert.That(plan.Dequeue().name, Is.EqualTo("CollectWood"));
            Assert.That(plan.Dequeue().name, Is.EqualTo("CraftItem"));
        }

        [Test]
        public void Planner_RejectsActionWhoseEffectsConflictWithRequiredConditions()
        {
            var badPreparation = new TestAction(
                "UnsafePreparation",
                1f,
                null,
                Conditions(("Prepared", true), ("Safe", false)));
            var goodPreparation = new TestAction(
                "SafePreparation",
                5f,
                null,
                State("Prepared"));
            var finish = new TestAction(
                "Finish",
                1f,
                Conditions(("Prepared", true), ("Safe", true)),
                State("Done"));
            var actions = new HashSet<GoapAction<string, bool>>
            {
                badPreparation,
                goodPreparation,
                finish
            };
            var planner = new GoapPlanner<string, bool>();

            Queue<GoapAction<string, bool>> plan = planner.Plan(
                actions,
                State("Safe"),
                State("Done"));

            Assert.That(plan, Is.Not.Null);
            Assert.That(plan.Count, Is.EqualTo(2));
            Assert.That(plan.Dequeue().name, Is.EqualTo("SafePreparation"));
            Assert.That(plan.Dequeue().name, Is.EqualTo("Finish"));
        }

        [Test]
        public void BuildPlan_WhenHighestPriorityGoalIsAlreadySatisfied_UsesNextGoal()
        {
            var agent = new GoapAgent<string, bool>();
            agent.WorldState["AlreadyDone"] = true;

            var reachPendingGoal = new TestAction(
                "ReachPendingGoal",
                1f,
                null,
                State("PendingDone"));

            agent.AddAction(reachPendingGoal);
            agent.AddGoal(new Goal<string, bool>(State("PendingDone"), 10));
            agent.AddGoal(new Goal<string, bool>(State("AlreadyDone"), 100));

            Assert.That(agent.ForceBuildPlan(), Is.True);
            agent.RunPlan();

            Assert.That(reachPendingGoal.ExecuteCount, Is.EqualTo(1));
            Assert.That(agent.WorldState["PendingDone"], Is.True);
        }

        [Test]
        public void ApplyActionEffects_UpdatesWorldStateInPlaceWithExactValue()
        {
            var originalWorldState = new Dictionary<string, int>
            {
                ["Wood"] = 2
            };
            var agent = new GoapAgent<string, int>
            {
                WorldState = originalWorldState
            };
            var action = new IntEffectAction("Wood", 5);

            agent.ApplyActionEffects(action);

            Assert.That(agent.WorldState, Is.SameAs(originalWorldState));
            Assert.That(originalWorldState["Wood"], Is.EqualTo(5));
        }

        [Test]
        public void LumberjackPlan_WhenAllBranchesAvailable_SelectsLowerCostLocations()
        {
            HashSet<GoapAction<string, int>> actions = CreateLumberjackActions(
                primaryTreeAvailable: true,
                backupTreeAvailable: true,
                primaryWorkbenchAvailable: true,
                backupWorkbenchAvailable: true);
            var planner = new GoapPlanner<string, int>();

            Queue<GoapAction<string, int>> plan = planner.Plan(
                actions,
                LumberjackWorldState(),
                IntConditions(("Cycle", 1)));

            Assert.That(plan, Is.Not.Null);
            Assert.That(plan.Count, Is.EqualTo(8));
            Assert.That(plan.Dequeue().name, Is.EqualTo("MovePrimaryTree"));

            while (plan.Count > 1)
            {
                plan.Dequeue();
            }

            Assert.That(plan.Dequeue().name, Is.EqualTo("CraftPrimaryWorkbench"));
        }

        [Test]
        public void LumberjackPlan_WhenPrimaryLocationsUnavailable_SelectsBackupLocations()
        {
            HashSet<GoapAction<string, int>> actions = CreateLumberjackActions(
                primaryTreeAvailable: false,
                backupTreeAvailable: true,
                primaryWorkbenchAvailable: false,
                backupWorkbenchAvailable: true);
            var planner = new GoapPlanner<string, int>();

            Queue<GoapAction<string, int>> plan = planner.Plan(
                actions,
                LumberjackWorldState(),
                IntConditions(("Cycle", 1)));

            Assert.That(plan, Is.Not.Null);
            Assert.That(plan.Count, Is.EqualTo(8));
            Assert.That(plan.Dequeue().name, Is.EqualTo("MoveBackupTree"));

            while (plan.Count > 1)
            {
                plan.Dequeue();
            }

            Assert.That(plan.Dequeue().name, Is.EqualTo("CraftBackupWorkbench"));
        }

        [TestCase(10, 200)]
        [TestCase(25, 100)]
        [TestCase(50, 50)]
        [Category("Performance")]
        public void PlannerPerformance_LinearActionChain(int actionCount, int iterations)
        {
            HashSet<GoapAction<string, bool>> actions = CreateLinearActions(actionCount);
            var worldState = State("State_0");
            var goal = State($"State_{actionCount}");
            var planner = new GoapPlanner<string, bool>();

            for (int i = 0; i < 5; i++)
            {
                Assert.That(planner.Plan(actions, worldState, goal), Is.Not.Null);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            var stopwatch = Stopwatch.StartNew();
            Queue<GoapAction<string, bool>> lastPlan = null;

            for (int i = 0; i < iterations; i++)
            {
                lastPlan = planner.Plan(actions, worldState, goal);
            }

            stopwatch.Stop();
            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            double averageMilliseconds = stopwatch.Elapsed.TotalMilliseconds / iterations;
            long averageAllocatedBytes = allocatedBytes / iterations;

            Assert.That(lastPlan, Is.Not.Null);
            Assert.That(lastPlan.Count, Is.EqualTo(actionCount));
            TestContext.Progress.WriteLine(
                $"[GOAP Performance] Linear actions={actionCount}, iterations={iterations}, " +
                $"total={stopwatch.Elapsed.TotalMilliseconds:F3} ms, " +
                $"average={averageMilliseconds:F4} ms/plan, " +
                $"allocated={averageAllocatedBytes} bytes/plan");
        }

        [Test]
        [Category("Performance")]
        public void PlannerPerformance_BranchingActionGraph()
        {
            const int depth = 4;
            const int alternativesPerDepth = 2;
            const int iterations = 10;

            HashSet<GoapAction<string, bool>> actions =
                CreateBranchingActions(depth, alternativesPerDepth);
            var worldState = State("State_0");
            var goal = State($"State_{depth}");
            var planner = new GoapPlanner<string, bool>();

            Assert.That(planner.Plan(actions, worldState, goal), Is.Not.Null);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            var stopwatch = Stopwatch.StartNew();
            Queue<GoapAction<string, bool>> lastPlan = null;

            for (int i = 0; i < iterations; i++)
            {
                lastPlan = planner.Plan(actions, worldState, goal);
            }

            stopwatch.Stop();
            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

            Assert.That(lastPlan, Is.Not.Null);
            Assert.That(lastPlan.Count, Is.EqualTo(depth));
            TestContext.Progress.WriteLine(
                $"[GOAP Performance] Branching depth={depth}, alternatives={alternativesPerDepth}, " +
                $"actions={actions.Count}, iterations={iterations}, " +
                $"average={stopwatch.Elapsed.TotalMilliseconds / iterations:F4} ms/plan, " +
                $"allocated={allocatedBytes / iterations} bytes/plan");
        }

        private static Dictionary<string, bool> State(string key)
        {
            return new Dictionary<string, bool> { [key] = true };
        }

        private static Dictionary<string, bool> Conditions(
            params (string key, bool value)[] conditions)
        {
            var result = new Dictionary<string, bool>();
            foreach ((string key, bool value) in conditions)
            {
                result.Add(key, value);
            }

            return result;
        }

        private static Dictionary<string, int> IntConditions(
            params (string key, int value)[] conditions)
        {
            var result = new Dictionary<string, int>();
            foreach ((string key, int value) in conditions)
            {
                result.Add(key, value);
            }

            return result;
        }

        private static Dictionary<CollisionKey, bool> CollisionConditions(
            params (CollisionKey key, bool value)[] conditions)
        {
            var result = new Dictionary<CollisionKey, bool>();
            foreach ((CollisionKey key, bool value) in conditions)
            {
                result.Add(key, value);
            }

            return result;
        }

        private static Dictionary<string, int> LumberjackWorldState()
        {
            return IntConditions(
                ("Location", 0),
                ("Wood", 0),
                ("Durability", 10),
                ("Cycle", 0));
        }

        private static HashSet<GoapAction<string, int>> CreateLumberjackActions(
            bool primaryTreeAvailable,
            bool backupTreeAvailable,
            bool primaryWorkbenchAvailable,
            bool backupWorkbenchAvailable)
        {
            var actions = new HashSet<GoapAction<string, int>>();

            AddTreeRoute(actions, "PrimaryTree", 1, 1f, primaryTreeAvailable);
            AddTreeRoute(actions, "BackupTree", 2, 2f, backupTreeAvailable);
            AddWorkbenchRoute(
                actions,
                "PrimaryWorkbench",
                3,
                1f,
                primaryWorkbenchAvailable);
            AddWorkbenchRoute(
                actions,
                "BackupWorkbench",
                4,
                3f,
                backupWorkbenchAvailable);

            return actions;
        }

        private static void AddTreeRoute(
            HashSet<GoapAction<string, int>> actions,
            string routeName,
            int location,
            float routeCost,
            bool available)
        {
            actions.Add(new IntPlanningAction(
                $"Move{routeName}",
                routeCost,
                null,
                IntConditions(("Location", location)),
                available));

            for (int wood = 0; wood < 5; wood++)
            {
                actions.Add(new IntPlanningAction(
                    $"Chop{routeName}_{wood + 1}",
                    routeCost,
                    IntConditions(
                        ("Location", location),
                        ("Wood", wood),
                        ("Durability", 10 - wood)),
                    IntConditions(
                        ("Wood", wood + 1),
                        ("Durability", 9 - wood)),
                    available));
            }
        }

        private static void AddWorkbenchRoute(
            HashSet<GoapAction<string, int>> actions,
            string routeName,
            int location,
            float routeCost,
            bool available)
        {
            actions.Add(new IntPlanningAction(
                $"Move{routeName}",
                routeCost,
                null,
                IntConditions(("Location", location)),
                available));
            actions.Add(new IntPlanningAction(
                $"Craft{routeName}",
                routeCost,
                IntConditions(
                    ("Location", location),
                    ("Wood", 5),
                    ("Durability", 5)),
                IntConditions(
                    ("Wood", 0),
                    ("Durability", 10),
                    ("Cycle", 1)),
                available));
        }

        private static HashSet<GoapAction<string, bool>> CreateLinearActions(int actionCount)
        {
            var actions = new HashSet<GoapAction<string, bool>>();
            for (int i = 0; i < actionCount; i++)
            {
                actions.Add(new TestAction(
                    $"Action_{i}",
                    1f,
                    State($"State_{i}"),
                    State($"State_{i + 1}")));
            }

            return actions;
        }

        private static HashSet<GoapAction<string, bool>> CreateBranchingActions(
            int depth,
            int alternativesPerDepth)
        {
            var actions = new HashSet<GoapAction<string, bool>>();
            for (int level = 0; level < depth; level++)
            {
                for (int alternative = 0; alternative < alternativesPerDepth; alternative++)
                {
                    actions.Add(new TestAction(
                        $"Action_{level}_{alternative}",
                        1f + alternative,
                        State($"State_{level}"),
                        State($"State_{level + 1}")));
                }
            }

            return actions;
        }
    }
}
