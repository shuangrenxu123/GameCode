using Fight.Number;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace Game.Tests
{
    public sealed class CombatPropertySetPerformanceTests
    {
        private const int WarmupCount = 5;
        private const int MeasurementCount = 20;
        private int _resultSink;

        [Test]
        public void SetBaseValue_重复修改后派生属性结果正确()
        {
            CombatPropertySet properties = CreateDerivedPropertySet();
            for (int index = 0; index < 1000; index++)
            {
                properties.SetBaseValue(PropertyType.Attack, index % 2 == 0 ? 101 : 100);
            }

            Assert.That(properties.GetFinalValue(PropertyType.DamageReductionRate), Is.EqualTo(30));
        }

        [Test, Performance]
        [Category("Performance")]
        public void GetFinalValue_连续读取一万次()
        {
            var properties = new CombatPropertySet();
            properties.RegisterProperty(PropertyType.Attack, 100);

            Measure.Method(() =>
                {
                    int total = 0;
                    for (int index = 0; index < 10000; index++)
                    {
                        total += properties.GetFinalValue(PropertyType.Attack);
                    }

                    _resultSink = total;
                })
                .SampleGroup("属性最终值读取_10000次")
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .GC()
                .Run();

            Assert.That(_resultSink, Is.EqualTo(1000000));
        }

        [Test, Performance]
        [Category("Performance")]
        public void ResourceValue_生命值增减一万次()
        {
            var properties = new CombatPropertySet();
            properties.RegisterProperty(PropertyType.MaxHp, 1000, 1, 999999);
            ResourceValue hp = properties.RegisterPropertyBoundResource(ResourceType.Hp, PropertyType.MaxHp);

            Measure.Method(() =>
                {
                    for (int index = 0; index < 5000; index++)
                    {
                        hp.Minus(1);
                        hp.Add(1);
                    }

                    _resultSink = hp.Value;
                })
                .SampleGroup("生命值增减_10000次")
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .GC()
                .Run();

            Assert.That(_resultSink, Is.EqualTo(1000));
        }

        [Test, Performance]
        [Category("Performance")]
        public void Modifier_添加与移除一千次()
        {
            var properties = new CombatPropertySet();
            properties.RegisterProperty(PropertyType.Attack, 100);
            var source = new ModifierSource(ModifierSourceType.Buff, 1001);

            Measure.Method(() =>
                {
                    for (int index = 0; index < 1000; index++)
                    {
                        ModifierHandle handle = properties.AddModifier(
                            PropertyType.Attack,
                            10,
                            ModifierType.Add,
                            source);
                        properties.RemoveModifier(PropertyType.Attack, handle);
                    }

                    _resultSink = properties.GetFinalValue(PropertyType.Attack);
                })
                .SampleGroup("Modifier增删_1000次")
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .GC()
                .Run();

            Assert.That(_resultSink, Is.EqualTo(100));
        }

        [Test, Performance]
        [Category("Performance")]
        public void DerivedProperty_三层派生链更新一千次()
        {
            CombatPropertySet properties = CreateDerivedPropertySet();
            int nextAttack = 100;

            Measure.Method(() =>
                {
                    for (int index = 0; index < 1000; index++)
                    {
                        nextAttack = nextAttack == 100 ? 101 : 100;
                        properties.SetBaseValue(PropertyType.Attack, nextAttack);
                    }

                    _resultSink = properties.GetFinalValue(PropertyType.DamageReductionRate);
                })
                .SampleGroup("三层派生链更新_1000次")
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .GC()
                .Run();

            Assert.That(_resultSink, Is.EqualTo(30));
        }

        [Test, Performance]
        [Category("Performance")]
        public void Batch_对比即时重算与批处理()
        {
            CombatPropertySet immediateProperties = CreateDerivedPropertySet();
            CombatPropertySet batchedProperties = CreateDerivedPropertySet();
            int immediateAttack = 100;
            int batchedAttack = 100;

            Measure.Method(() =>
                {
                    for (int index = 0; index < 1000; index++)
                    {
                        immediateAttack = immediateAttack == 100 ? 101 : 100;
                        immediateProperties.SetBaseValue(PropertyType.Attack, immediateAttack);
                    }

                    _resultSink = immediateProperties.GetFinalValue(PropertyType.DamageReductionRate);
                })
                .SampleGroup("即时重算_1000次")
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .GC()
                .Run();

            Measure.Method(() =>
                {
                    batchedProperties.BeginBatch();
                    for (int index = 0; index < 1000; index++)
                    {
                        batchedAttack = batchedAttack == 100 ? 101 : 100;
                        batchedProperties.SetBaseValue(PropertyType.Attack, batchedAttack);
                    }

                    batchedProperties.EndBatch();
                    _resultSink = batchedProperties.GetFinalValue(PropertyType.DamageReductionRate);
                })
                .SampleGroup("批处理重算_1000次")
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .GC()
                .Run();

            Assert.That(_resultSink, Is.EqualTo(30));
        }

        private static CombatPropertySet CreateDerivedPropertySet()
        {
            var properties = new CombatPropertySet();
            properties.RegisterProperty(PropertyType.Attack, 100);
            properties.RegisterDerivedProperty(
                PropertyType.Defense,
                new[] { PropertyType.Attack },
                context => context.GetFinalValue(PropertyType.Attack) / 2);
            properties.RegisterDerivedProperty(
                PropertyType.AllRound,
                new[] { PropertyType.Defense },
                context => context.GetFinalValue(PropertyType.Defense) + 10);
            properties.RegisterDerivedProperty(
                PropertyType.DamageReductionRate,
                new[] { PropertyType.AllRound },
                context => context.GetFinalValue(PropertyType.AllRound) / 2,
                0,
                100);
            return properties;
        }
    }
}
