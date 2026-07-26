using System;
using System.Collections.Generic;
using GOAP;
using UnityEngine;

namespace GOAP.Demo
{
    public enum LumberjackStateKey
    {
        Location,
        WoodCount,
        AxeDurability,
        CycleCompleted
    }

    public enum LumberjackLocation
    {
        Start,
        PrimaryTree,
        BackupTree,
        PrimaryWorkbench,
        BackupWorkbench
    }

    public sealed class LumberjackMoveAction : GoapAction<LumberjackStateKey, int>
    {
        private readonly ulong actionMask;
        private readonly Transform actor;
        private readonly Transform destination;
        private readonly Func<bool> isDestinationAvailable;
        private readonly float moveSpeed;
        private readonly float arrivalDistance;
        private readonly Action<string> reportAction;

        public override ulong ActionMask => actionMask;

        public LumberjackMoveAction(
            string actionName,
            float actionCost,
            Transform actor,
            Transform destination,
            LumberjackLocation destinationLocation,
            Func<bool> isDestinationAvailable,
            float moveSpeed,
            float arrivalDistance,
            Action<string> reportAction,
            ulong actionMask)
        {
            name = actionName;
            cost = actionCost;
            this.actor = actor;
            this.destination = destination;
            this.isDestinationAvailable = isDestinationAvailable;
            this.moveSpeed = moveSpeed;
            this.arrivalDistance = arrivalDistance;
            this.reportAction = reportAction;
            this.actionMask = actionMask;

            effects.Add(LumberjackStateKey.Location, (int)destinationLocation);
        }

        protected override void Reset()
        {
            executed = false;
        }

        public override bool CheckProceduralPreCondition(
            Dictionary<LumberjackStateKey, int> state)
        {
            return actor != null &&
                   destination != null &&
                   (isDestinationAvailable == null || isDestinationAvailable());
        }

        public override void PlanEnter()
        {
            base.PlanEnter();
            reportAction?.Invoke($"开始执行：{name}");
        }

        public override void PlanExecute()
        {
            actor.position = Vector3.MoveTowards(
                actor.position,
                destination.position,
                moveSpeed * Time.deltaTime);

            if ((actor.position - destination.position).sqrMagnitude <=
                arrivalDistance * arrivalDistance)
            {
                actor.position = destination.position;
                executed = true;
                reportAction?.Invoke($"到达：{destination.name}");
            }
        }
    }

    public sealed class LumberjackChopAction : GoapAction<LumberjackStateKey, int>
    {
        private readonly ulong actionMask;
        private readonly Func<bool> isTreeAvailable;
        private readonly float duration;
        private readonly int woodAfter;
        private readonly int durabilityAfter;
        private readonly Action<string> reportAction;
        private float elapsedTime;

        public override ulong ActionMask => actionMask;

        public LumberjackChopAction(
            string actionName,
            float actionCost,
            LumberjackLocation treeLocation,
            int woodBefore,
            Func<bool> isTreeAvailable,
            float duration,
            Action<string> reportAction,
            ulong actionMask)
        {
            name = actionName;
            cost = actionCost;
            this.isTreeAvailable = isTreeAvailable;
            this.duration = duration;
            this.reportAction = reportAction;
            this.actionMask = actionMask;

            int durabilityBefore = 10 - woodBefore;
            woodAfter = woodBefore + 1;
            durabilityAfter = durabilityBefore - 1;

            preconditions.Add(LumberjackStateKey.Location, (int)treeLocation);
            preconditions.Add(LumberjackStateKey.WoodCount, woodBefore);
            preconditions.Add(LumberjackStateKey.AxeDurability, durabilityBefore);

            effects.Add(LumberjackStateKey.WoodCount, woodAfter);
            effects.Add(LumberjackStateKey.AxeDurability, durabilityAfter);
        }

        protected override void Reset()
        {
            executed = false;
            elapsedTime = 0f;
        }

        public override bool CheckProceduralPreCondition(
            Dictionary<LumberjackStateKey, int> state)
        {
            return isTreeAvailable == null || isTreeAvailable();
        }

        public override void PlanEnter()
        {
            base.PlanEnter();
            elapsedTime = 0f;
            reportAction?.Invoke($"开始执行：{name}");
        }

        public override void PlanExecute()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime < duration)
            {
                return;
            }

            executed = true;
            reportAction?.Invoke(
                $"砍树完成：木头 {woodAfter}/5，斧头耐久 {durabilityAfter}/10");
        }
    }

    public sealed class LumberjackCraftAxeAction : GoapAction<LumberjackStateKey, int>
    {
        private readonly ulong actionMask;
        private readonly Func<bool> isWorkbenchAvailable;
        private readonly float duration;
        private readonly Action<string> reportAction;
        private float elapsedTime;

        public override ulong ActionMask => actionMask;

        public LumberjackCraftAxeAction(
            string actionName,
            float actionCost,
            LumberjackLocation workbenchLocation,
            Func<bool> isWorkbenchAvailable,
            float duration,
            Action<string> reportAction,
            ulong actionMask)
        {
            name = actionName;
            cost = actionCost;
            this.isWorkbenchAvailable = isWorkbenchAvailable;
            this.duration = duration;
            this.reportAction = reportAction;
            this.actionMask = actionMask;

            preconditions.Add(LumberjackStateKey.Location, (int)workbenchLocation);
            preconditions.Add(LumberjackStateKey.WoodCount, 5);
            preconditions.Add(LumberjackStateKey.AxeDurability, 5);

            effects.Add(LumberjackStateKey.WoodCount, 0);
            effects.Add(LumberjackStateKey.AxeDurability, 10);
            effects.Add(LumberjackStateKey.CycleCompleted, 1);
        }

        protected override void Reset()
        {
            executed = false;
            elapsedTime = 0f;
        }

        public override bool CheckProceduralPreCondition(
            Dictionary<LumberjackStateKey, int> state)
        {
            return isWorkbenchAvailable == null || isWorkbenchAvailable();
        }

        public override void PlanEnter()
        {
            base.PlanEnter();
            elapsedTime = 0f;
            reportAction?.Invoke($"开始执行：{name}");
        }

        public override void PlanExecute()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime < duration)
            {
                return;
            }

            executed = true;
            reportAction?.Invoke("制作完成：消耗 5 个木头，新斧头耐久为 10");
        }
    }
}
