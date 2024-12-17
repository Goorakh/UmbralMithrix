using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace UmbralMithrix
{
    internal class ModConfig
    {
        public static ConfigEntry<bool> purpleArena;
        public static ConfigEntry<bool> skipPhase4;
        public static ConfigEntry<bool> addShockwave;
        public static ConfigEntry<bool> purpleMithrix;
        public static ConfigEntry<float> basehealth;
        public static ConfigEntry<float> levelhealth;
        public static ConfigEntry<float> basedamage;
        public static ConfigEntry<float> leveldamage;
        public static ConfigEntry<float> basearmor;
        public static ConfigEntry<float> baseattackspeed;
        public static ConfigEntry<float> basespeed;
        public static ConfigEntry<float> mass;
        public static ConfigEntry<float> turningspeed;
        public static ConfigEntry<float> jumpingpower;
        public static ConfigEntry<float> acceleration;
        public static ConfigEntry<float> aircontrol;
        public static ConfigEntry<int> PrimStocks;
        public static ConfigEntry<int> SecStocks;
        public static ConfigEntry<int> UtilStocks;
        public static ConfigEntry<float> PrimCD;
        public static ConfigEntry<float> SecCD;
        public static ConfigEntry<float> UtilCD;
        public static ConfigEntry<float> SpecialCD;
        public static ConfigEntry<float> CrushingLeap;
        public static ConfigEntry<int> SlamOrbProjectileCount;
        public static ConfigEntry<int> LunarShardAdd;
        public static ConfigEntry<int> UltimateWaves;
        public static ConfigEntry<int> UltimateCount;
        public static ConfigEntry<float> UltimateDuration;
        public static ConfigEntry<int> JumpWaveCount;
        public static ConfigEntry<float> ShardHoming;
        public static ConfigEntry<float> ShardRange;
        public static ConfigEntry<float> ShardCone;

        public static void InitConfig(ConfigFile config)
        {
            purpleMithrix = config.Bind("General", "Purple Mithrix", true, "Adds umbral effects to Mithrix (purple when spawning in).");
            purpleArena = config.Bind("General", "Purple Arena", false, "Adds swirling purple walls/ceiling to the arena. Applies at the start of the fight.");
            skipPhase4 = config.Bind("General", "Skip Phase 4", false, "Skips Phase 4. Applies at the start of the fight.");
            addShockwave = config.Bind("General", "Add shockwave to Hammer Swipe", false, "Adds a 1 shockwave when Mithrix swipes. Applies at the start of the fight.");

            basehealth = config.Bind("Stats", "Base Health", 1000f, "Vanilla: 1000");
            levelhealth = config.Bind("Stats", "Level Health", 325f, "Health gained per level. Vanilla: 300");
            basedamage = config.Bind("Stats", "Base Damage", 15f, "Vanilla: 16");
            leveldamage = config.Bind("Stats", "Level Damage", 3f, "Damage gained per level. Vanilla: 3.2");
            basearmor = config.Bind("Stats", "Base Armor", 30f, "Vanilla: 20");
            baseattackspeed = config.Bind("Stats", "Base Attack Speed", 1.25f, "Vanilla: 1");
            basespeed = config.Bind("Stats", "Base Move Speed", 15f, "Vanilla: 15");
            mass = config.Bind("Stats", "Mass", 5000f, "Recommended to increase if you increase his movement speed. Vanilla: 900");
            turningspeed = config.Bind("Stats", "Turn Speed", 300f, "Vanilla: 270");
            jumpingpower = config.Bind("Stats", "Moon Shoes", 50f, "How high Mithrix jumps. Vanilla: 25");
            acceleration = config.Bind("Stats", "Acceleration", 100f, "Vanilla: 45");
            aircontrol = config.Bind("Stats", "Air Control", 0.5f, "Vanilla: 0.25");

            PrimStocks = config.Bind("Skills", "Primary Stocks", 1, "Max Stocks for Mithrix's Weapon Slam. Vanilla: 1");
            SecStocks = config.Bind("Skills", "Secondary Stocks", 1, "Max Stocks for Mithrix's Dash Attack. Vanilla: 1");
            UtilStocks = config.Bind("Skills", "Util Stocks", 3, "Max Stocks for Mithrix's Dash. Vanilla: 2");
            PrimCD = config.Bind("Skills", "Primary Cooldown", 4f, "Cooldown for Mithrix's Weapon Slam. Vanilla: 4");
            SecCD = config.Bind("Skills", "Secondary Cooldown", 4.5f, "Cooldown for Mithrix's Dash Attack. Vanilla: 5");
            UtilCD = config.Bind("Skills", "Util Cooldown", 2.5f, "Cooldown for Mithrix's Dash. Vanilla: 3");
            SpecialCD = config.Bind("Skills", "Special Cooldown", 30f, "Cooldown for Mithrix's Jump Attack. Vanilla: 30");

            CrushingLeap = config.Bind("Skill Mods", "Crushing Leap", 3f, "How long Mithrix stays in the air during the crushing leap. Vanilla: 3");
            SlamOrbProjectileCount = config.Bind("Skill Mods", "Orb Projectile Count", 3, "Orbs fired by weapon slam in a circle. Vanilla: N/A");
            LunarShardAdd = config.Bind("Skill Mods", "Shard Add Count", 1, "Bonus shards added to each shot of lunar shards. Vanilla: N/A");
            UltimateWaves = config.Bind("Skill Mods", "P3 Ult Lines", 8, "Total lines in ultimate per burst. Vanilla: 4");
            UltimateCount = config.Bind("Skill Mods", "P3 Ult Bursts", 6, "Total times the ultimate fires. Vanilla: 4");
            UltimateDuration = config.Bind("Skill Mods", "P3 Ult Duration", 8f, "How long ultimate lasts. Vanilla: 8");
            JumpWaveCount = config.Bind("Skill Mods", "Jump Wave Count", 16, "Shockwave count when Mithrix lands after a jump. Vanilla: 12");
            ShardHoming = config.Bind("Skill Mods", "Shard Homing", 25f, "How strongly lunar shards home in to targets. Vanilla: 20");
            ShardRange = config.Bind("Skill Mods", "Shard Range", 100f, "Range (distance) in which shards look for targets. Vanilla: 80");
            ShardCone = config.Bind("Skill Mods", "Shard Cone", 120f, "Cone (Angle) in which shards look for targets. Vanilla: 90");

            ModSettingsManager.AddOption(new CheckBoxOption(purpleArena));
            ModSettingsManager.AddOption(new CheckBoxOption(skipPhase4));
            ModSettingsManager.AddOption(new CheckBoxOption(addShockwave));

            ModSettingsManager.AddOption(new StepSliderOption(basehealth, new StepSliderConfig()
            {
                min = 500f,
                max = 2500f,
                increment = 50f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(levelhealth, new StepSliderConfig()
            {
                min = 100f,
                max = 500f,
                increment = 25f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(basedamage, new StepSliderConfig()
            {
                min = 10f,
                max = 30f,
                increment = 1f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(leveldamage, new StepSliderConfig()
            {
                min = 1f,
                max = 6.4f,
                increment = 0.25f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(basearmor, new StepSliderConfig()
            {
                min = 5f,
                max = 50f,
                increment = 5f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(baseattackspeed, new StepSliderConfig()
            {
                min = 0.25f,
                max = 3f,
                increment = 0.25f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(basespeed, new StepSliderConfig()
            {
                min = 10f,
                max = 30f,
                increment = 1f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(mass, new StepSliderConfig()
            {
                min = 900f,
                max = 10000f,
                increment = 100f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(turningspeed, new StepSliderConfig()
            {
                min = 200f,
                max = 1000f,
                increment = 10f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(jumpingpower, new StepSliderConfig()
            {
                min = 25f,
                max = 100f,
                increment = 5f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(acceleration, new StepSliderConfig()
            {
                min = 45f,
                max = 500f,
                increment = 5f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(aircontrol, new StepSliderConfig()
            {
                min = 0.25f,
                max = 3f,
                increment = 0.25f
            }));

            ModSettingsManager.AddOption(new IntSliderOption(PrimStocks, new IntSliderConfig()
            {
                min = 1,
                max = 5
            }));

            ModSettingsManager.AddOption(new IntSliderOption(SecStocks, new IntSliderConfig()
            {
                min = 1,
                max = 5
            }));

            ModSettingsManager.AddOption(new IntSliderOption(UtilStocks, new IntSliderConfig()
            {
                min = 1,
                max = 5
            }));

            ModSettingsManager.AddOption(new StepSliderOption(PrimCD, new StepSliderConfig()
            {
                min = 1f,
                max = 5f,
                increment = 0.25f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(SecCD, new StepSliderConfig()
            {
                min = 1f,
                max = 5f,
                increment = 0.25f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(UtilCD, new StepSliderConfig()
            {
                min = 1f,
                max = 5f,
                increment = 0.25f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(SpecialCD, new StepSliderConfig()
            {
                min = 10f,
                max = 50f,
                increment = 1f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(CrushingLeap, new StepSliderConfig()
            {
                min = 0.1f,
                max = 6f,
                increment = 0.1f
            }));

            ModSettingsManager.AddOption(new IntSliderOption(SlamOrbProjectileCount, new IntSliderConfig()
            {
                min = 0,
                max = 16
            }));

            ModSettingsManager.AddOption(new IntSliderOption(LunarShardAdd, new IntSliderConfig()
            {
                min = 1,
                max = 5
            }));

            ModSettingsManager.AddOption(new IntSliderOption(UltimateWaves, new IntSliderConfig()
            {
                min = 4,
                max = 18
            }));

            ModSettingsManager.AddOption(new StepSliderOption(UltimateDuration, new StepSliderConfig()
            {
                min = 5f,
                max = 10f,
                increment = 0.25f
            }));

            ModSettingsManager.AddOption(new IntSliderOption(JumpWaveCount, new IntSliderConfig()
            {
                min = 12,
                max = 24
            }));

            ModSettingsManager.AddOption(new StepSliderOption(ShardHoming, new StepSliderConfig()
            {
                min = 10f,
                max = 60f,
                increment = 5f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(ShardRange, new StepSliderConfig()
            {
                min = 80f,
                max = 160f,
                increment = 10f
            }));

            ModSettingsManager.AddOption(new StepSliderOption(ShardCone, new StepSliderConfig()
            {
                min = 90f,
                max = 180f,
                increment = 10f
            }));

            ModSettingsManager.SetModDescription("Moon man with shadows");
        }
    }
}
