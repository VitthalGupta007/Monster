using System.Collections.Generic;
using VXMonster.Core;
using VXMonster.Core.Bossfight;

namespace VXMonster.Gameplay
{
    public static class StageEnemyProgression
    {
        public enum ThreatTier
        {
            Fodder = 0,
            Light = 1,
            Medium = 2,
            Heavy = 3,
            Elite = 4,
        }

        public readonly struct EnemyStatProfile
        {
            public EnemyStatProfile(EnemyType type, ThreatTier tier, int hp, int damage, float speed)
            {
                Type = type;
                Tier = tier;
                Hp = hp;
                Damage = damage;
                Speed = speed;
            }

            public EnemyType Type { get; }
            public ThreatTier Tier { get; }
            public int Hp { get; }
            public int Damage { get; }
            public float Speed { get; }

            public int ThreatScore => Hp + Damage * 10;
        }

        static readonly EnemyStatProfile[] Profiles =
        {
            new(EnemyType.Shade, ThreatTier.Fodder, 2, 1, 1f),
            new(EnemyType.Wasp, ThreatTier.Fodder, 8, 4, 0.5f),
            new(EnemyType.Bat, ThreatTier.Fodder, 8, 10, 0.8f),
            new(EnemyType.ShadeBat, ThreatTier.Fodder, 8, 10, 0.8f),
            new(EnemyType.Bug, ThreatTier.Fodder, 10, 5, 0.6f),
            new(EnemyType.PurpleJellyfish, ThreatTier.Fodder, 15, 4, 1.3f),
            new(EnemyType.Jellyfish, ThreatTier.Fodder, 20, 4, 0.2f),
            new(EnemyType.Slime, ThreatTier.Fodder, 20, 3, 0.2f),
            new(EnemyType.Pumpkin, ThreatTier.Light, 45, 5, 0.2f),
            new(EnemyType.Hand, ThreatTier.Medium, 80, 10, 1.1f),
            new(EnemyType.FireSlime, ThreatTier.Medium, 80, 15, 0.4f),
            new(EnemyType.StagBeetle, ThreatTier.Medium, 80, 10, 0.6f),
            new(EnemyType.Plant, ThreatTier.Medium, 90, 10, 0.2f),
            new(EnemyType.Eye, ThreatTier.Heavy, 120, 3, 0.3f),
            new(EnemyType.Vampire, ThreatTier.Heavy, 120, 10, 0.4f),
            new(EnemyType.ShadeVampire, ThreatTier.Heavy, 120, 10, 0.4f),
            new(EnemyType.ShadeJellyfish, ThreatTier.Elite, 1000, 15, 0.4f),
        };

        static readonly Dictionary<EnemyType, EnemyStatProfile> ProfileLookup = BuildLookup();

        public static IReadOnlyList<EnemyStatProfile> AllProfiles => Profiles;

        public static bool TryGetProfile(EnemyType type, out EnemyStatProfile profile)
        {
            return ProfileLookup.TryGetValue(type, out profile);
        }

        public static int GetThreatScore(EnemyType type)
        {
            return TryGetProfile(type, out var profile) ? profile.ThreatScore : 0;
        }

        public static ThreatTier GetTier(EnemyType type)
        {
            return TryGetProfile(type, out var profile) ? profile.Tier : ThreatTier.Fodder;
        }

        public static ThreatTier GetMaxTierForStage(int stageOneBased)
        {
            return stageOneBased switch
            {
                1 => ThreatTier.Light,
                2 => ThreatTier.Medium,
                3 => ThreatTier.Medium,
                4 => ThreatTier.Heavy,
                5 => ThreatTier.Heavy,
                6 => ThreatTier.Elite,
                _ => ThreatTier.Elite,
            };
        }

        public static int GetMinStageForEnemy(EnemyType type)
        {
            return type switch
            {
                EnemyType.ShadeJellyfish => 6,
                EnemyType.ShadeVampire or EnemyType.Shade or EnemyType.ShadeBat => 5,
                EnemyType.Vampire or EnemyType.Eye => 4,
                EnemyType.FireSlime or EnemyType.StagBeetle => 3,
                EnemyType.Plant or EnemyType.Hand => 2,
                _ => 1,
            };
        }

        public static StageMixDefinition GetMixForStage(int stageOneBased)
        {
            for (var i = 0; i < StageMixes.Length; i++)
            {
                if (StageMixes[i].Stage == stageOneBased) return StageMixes[i];
            }

            return StageMixes[0];
        }

        public static readonly StageMixDefinition[] StageMixes =
        {
            new(
                1,
                new[]
                {
                    EnemyType.Slime, EnemyType.Bug, EnemyType.Wasp, EnemyType.Bat,
                    EnemyType.Pumpkin, EnemyType.Jellyfish,
                },
                new[] { BossType.MegaSlime, BossType.Crab, BossType.Mask }),
            new(
                2,
                new[]
                {
                    EnemyType.Slime, EnemyType.Bug, EnemyType.Wasp, EnemyType.Bat,
                    EnemyType.Jellyfish, EnemyType.PurpleJellyfish, EnemyType.Pumpkin,
                    EnemyType.Plant, EnemyType.Hand, EnemyType.Slime, EnemyType.Bug,
                },
                new[] { BossType.Crab, BossType.Mask, BossType.MegaSlime }),
            new(
                3,
                new[]
                {
                    EnemyType.Jellyfish, EnemyType.Wasp, EnemyType.Bug, EnemyType.Bat,
                    EnemyType.Pumpkin, EnemyType.PurpleJellyfish, EnemyType.Hand,
                    EnemyType.Slime, EnemyType.Plant, EnemyType.StagBeetle, EnemyType.FireSlime,
                },
                new[] { BossType.Crab, BossType.MegaSlime, BossType.Mask }),
            new(
                4,
                new[]
                {
                    EnemyType.Plant, EnemyType.Hand, EnemyType.StagBeetle, EnemyType.FireSlime,
                    EnemyType.Vampire, EnemyType.Pumpkin, EnemyType.Bug, EnemyType.Wasp,
                    EnemyType.Bat, EnemyType.Slime, EnemyType.Eye,
                },
                new[] { BossType.MegaSlime, BossType.Mask, BossType.Bell }),
            new(
                5,
                new[]
                {
                    EnemyType.Hand, EnemyType.FireSlime, EnemyType.StagBeetle, EnemyType.Vampire,
                    EnemyType.Eye, EnemyType.Plant, EnemyType.ShadeVampire, EnemyType.ShadeBat,
                    EnemyType.StagBeetle, EnemyType.Vampire, EnemyType.FireSlime,
                },
                new[] { BossType.QueenWasp, BossType.Crab, BossType.MegaSlime }),
            new(
                6,
                new[]
                {
                    EnemyType.Vampire, EnemyType.ShadeVampire, EnemyType.FireSlime, EnemyType.StagBeetle,
                    EnemyType.Eye, EnemyType.Hand, EnemyType.Plant, EnemyType.ShadeBat,
                    EnemyType.Shade, EnemyType.PurpleJellyfish, EnemyType.ShadeJellyfish,
                },
                new[] { BossType.Void, BossType.Bell, BossType.Mask }),
        };

        public readonly struct StageMixDefinition
        {
            public StageMixDefinition(int stage, EnemyType[] enemies, BossType[] bosses)
            {
                Stage = stage;
                Enemies = enemies;
                Bosses = bosses;
            }

            public int Stage { get; }
            public EnemyType[] Enemies { get; }
            public BossType[] Bosses { get; }
        }

        public readonly struct StageThreatSummary
        {
            public StageThreatSummary(int stage, float averageThreat, int maxThreat, ThreatTier peakTier, int eliteCount)
            {
                Stage = stage;
                AverageThreat = averageThreat;
                MaxThreat = maxThreat;
                PeakTier = peakTier;
                EliteCount = eliteCount;
            }

            public int Stage { get; }
            public float AverageThreat { get; }
            public int MaxThreat { get; }
            public ThreatTier PeakTier { get; }
            public int EliteCount { get; }
        }

        public static StageThreatSummary SummarizeMix(IReadOnlyList<EnemyType> enemies, int stageOneBased)
        {
            if (enemies == null || enemies.Count == 0)
                return new StageThreatSummary(stageOneBased, 0f, 0, ThreatTier.Fodder, 0);

            var total = 0;
            var max = 0;
            var peak = ThreatTier.Fodder;
            var eliteCount = 0;

            for (var i = 0; i < enemies.Count; i++)
            {
                var type = enemies[i];
                var score = GetThreatScore(type);
                total += score;
                if (score > max) max = score;

                var tier = GetTier(type);
                if (tier > peak) peak = tier;
                if (type == EnemyType.ShadeJellyfish) eliteCount++;
            }

            return new StageThreatSummary(stageOneBased, total / (float)enemies.Count, max, peak, eliteCount);
        }

        public static int GetGemXp(DropType dropType)
        {
            return dropType switch
            {
                DropType.SmallGem => 1,
                DropType.MediumGem => 4,
                DropType.BigGem => 10,
                DropType.LargeGem => 80,
                _ => 0,
            };
        }

        public static float EstimateXpPerKill(IReadOnlyList<EnemyDropData> drops)
        {
            if (drops == null || drops.Count == 0) return 0f;

            var sum = 0f;
            for (var i = 0; i < drops.Count; i++)
            {
                var gemXp = GetGemXp(drops[i].DropType);
                if (gemXp > 0)
                    sum += drops[i].Chance / 100f * gemXp;
            }

            return sum;
        }

        public readonly struct GraduatedDropProfile
        {
            public GraduatedDropProfile(
                float smallGem, float mediumGem, float bigGem = 0f, float largeGem = 0f,
                float magnet = 0f, float bomb = 0f, float food = 1f, float coin = 1f, float chest = 0f)
            {
                SmallGem = smallGem;
                MediumGem = mediumGem;
                BigGem = bigGem;
                LargeGem = largeGem;
                Magnet = magnet;
                Bomb = bomb;
                Food = food;
                Coin = coin;
                Chest = chest;
            }

            public float SmallGem { get; }
            public float MediumGem { get; }
            public float BigGem { get; }
            public float LargeGem { get; }
            public float Magnet { get; }
            public float Bomb { get; }
            public float Food { get; }
            public float Coin { get; }
            public float Chest { get; }

            public float ExpectedGemXp =>
                SmallGem / 100f * 1f +
                MediumGem / 100f * 4f +
                BigGem / 100f * 10f +
                LargeGem / 100f * 80f;

            public (DropType type, float chance)[] ToEntries()
            {
                var entries = new System.Collections.Generic.List<(DropType, float)>(9);
                if (SmallGem > 0f) entries.Add((DropType.SmallGem, SmallGem));
                if (MediumGem > 0f) entries.Add((DropType.MediumGem, MediumGem));
                if (BigGem > 0f) entries.Add((DropType.BigGem, BigGem));
                if (LargeGem > 0f) entries.Add((DropType.LargeGem, LargeGem));
                if (Magnet > 0f) entries.Add((DropType.Magnet, Magnet));
                if (Bomb > 0f) entries.Add((DropType.Bomb, Bomb));
                if (Food > 0f) entries.Add((DropType.Food, Food));
                if (Coin > 0f) entries.Add((DropType.Coin, Coin));
                if (Chest > 0f) entries.Add((DropType.Chest, Chest));
                return entries.ToArray();
            }
        }

        public static GraduatedDropProfile GetGraduatedDropProfile(EnemyType type)
        {
            return type switch
            {
                EnemyType.Slime => FodderGenerousDrop,
                EnemyType.Shade => FodderMidDrop,
                EnemyType.Pumpkin => LightDrop,
                EnemyType.Eye => HeavyPlusDrop,
                EnemyType.ShadeJellyfish => EliteDrop,
                EnemyType.Vampire or EnemyType.ShadeVampire => HeavyDrop,
                EnemyType.Hand or EnemyType.FireSlime or EnemyType.StagBeetle or EnemyType.Plant => MediumDrop,
                _ => GetTier(type) switch
                {
                    ThreatTier.Light => LightDrop,
                    ThreatTier.Medium => MediumDrop,
                    ThreatTier.Heavy => HeavyDrop,
                    ThreatTier.Elite => EliteDrop,
                    _ => FodderDrop,
                },
            };
        }

        public static (float min, float max) GetExpectedXpBand(ThreatTier tier) => tier switch
        {
            ThreatTier.Fodder => (1f, 4.5f),
            ThreatTier.Light => (3f, 6f),
            ThreatTier.Medium => (8f, 14f),
            ThreatTier.Heavy => (17f, 32f),
            ThreatTier.Elite => (42f, 65f),
            _ => (1f, 4.5f),
        };

        static readonly GraduatedDropProfile FodderDrop =
            new(smallGem: 85f, mediumGem: 15f);

        static readonly GraduatedDropProfile FodderGenerousDrop =
            new(smallGem: 90f, mediumGem: 35f, bigGem: 10f);

        static readonly GraduatedDropProfile FodderMidDrop =
            new(smallGem: 75f, mediumGem: 40f, bigGem: 15f);

        static readonly GraduatedDropProfile LightDrop =
            new(smallGem: 85f, mediumGem: 45f, bigGem: 10f);

        static readonly GraduatedDropProfile MediumDrop =
            new(smallGem: 55f, mediumGem: 70f, bigGem: 32f, largeGem: 6f, magnet: 1f);

        static readonly GraduatedDropProfile HeavyDrop =
            new(smallGem: 38f, mediumGem: 72f, bigGem: 48f, largeGem: 16f, magnet: 1f, bomb: 1f);

        static readonly GraduatedDropProfile HeavyPlusDrop =
            new(smallGem: 35f, mediumGem: 68f, bigGem: 52f, largeGem: 22f, magnet: 1f, bomb: 1f);

        static readonly GraduatedDropProfile EliteDrop =
            new(smallGem: 0f, mediumGem: 35f, bigGem: 88f, largeGem: 50f, food: 3f, coin: 5f, chest: 80f);

        public static readonly (int stage, float multiplier)[] StageXpGainTargets =
        {
            (1, 1f),
            (2, 1f),
            (3, 1.05f),
            (4, 1.10f),
            (5, 1.15f),
            (6, 1.20f),
        };

        static Dictionary<EnemyType, EnemyStatProfile> BuildLookup()
        {
            var lookup = new Dictionary<EnemyType, EnemyStatProfile>();
            for (var i = 0; i < Profiles.Length; i++)
                lookup[Profiles[i].Type] = Profiles[i];
            return lookup;
        }
    }
}
