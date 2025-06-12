using BepInEx;
using EntityStates;
using EntityStates.BrotherMonster;
using EntityStates.BrotherMonster.Weapon;
using R2API;
using Rewired.ComponentControls.Effects;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace UmbralMithrix
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    public class UmbralMithrix : BaseUnityPlugin
    {
        public const string PluginGUID = "com." + PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Nuxlar";
        public const string PluginName = "UmbralMithrix";
        public const string PluginVersion = "2.5.3";

        public static bool practiceModeEnabled;
        public static bool hasfired;
        public static bool spawnedClone = false;
        public static bool finishedItemSteal = false;
        public static bool p2ThresholdReached = false;
        public static bool p3ThresholdReached = false;
        public static List<GameObject> timeCrystals = new List<GameObject>();

        public static Dictionary<int, Vector3> p23PizzaPoints = new Dictionary<int, Vector3>()
        {
            {
                0,
                new Vector3(13.5f, 489.7f, -107f)
            },
            {
                1,
                new Vector3(-189f, 489.7f, 107f)
            },
            {
                2,
                new Vector3(16.7f, 489.7f, 101f)
            },
            {
                3,
                new Vector3(-196f, 489.7f, -101f)
            }
        };

        public static Dictionary<int, Vector3> p4PizzaPoints = new Dictionary<int, Vector3>()
        {
            {
                0,
                new Vector3(-175f, 489.7f, -0.08f)
            },
            {
                1,
                new Vector3(-0.08f, 489.7f, 0.08f)
            },
            {
                2,
                new Vector3(-91f, 489.7f, -89f)
            },
            {
                3,
                new Vector3(-89f, 489.7f, 89f)
            }
        };

        public static ItemDef UmbralItem;

        public static GameObject umbralSlamImpact = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion();
        public static GameObject umbralSlamProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSunderWave, Energized.prefab").WaitForCompletion();
        public static GameObject umbralSlamPillar = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherFirePillar.prefab").WaitForCompletion();
        public static GameObject umbralSlamHitEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/OmniImpactVFXHuntress.prefab").WaitForCompletion();
        public static NetworkSoundEventDef umbralSlamHitSound = Addressables.LoadAssetAsync<NetworkSoundEventDef>("RoR2/Base/Croco/nseAcridBiteHit.asset").WaitForCompletion();

        public static GameObject umbralLeapWave = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSunderWave.prefab").WaitForCompletion();

        public static GameObject umbralSwingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSwing1, Kickup.prefab").WaitForCompletion();

        public static GameObject umbralUltMuzzleFlash = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/ItemStealEndMuzzleflash.prefab").WaitForCompletion();

        public static GameObject leapIndicatorPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Vagrant/VagrantNovaAreaIndicator.prefab").WaitForCompletion(), "UmbralLeapIndicator");
        public static GameObject leapIndicator;
        public static SpawnCard timeCrystalCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/WeeklyRun/bscTimeCrystal.asset").WaitForCompletion();
        public static GameObject mithrixMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherMaster.prefab").WaitForCompletion();
        public static GameObject lunarMissile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLunar/LunarMissileProjectile.prefab").WaitForCompletion(), "UmbralLunarMissile", false);
        public static GameObject mithrixHurtP3Master = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherHurtMaster.prefab").WaitForCompletion(), "BrotherHurtMasterP3");
        public static GameObject mithrix = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherBody.prefab").WaitForCompletion();
        public static GameObject mithrixHurtP3 = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherHurtBody.prefab").WaitForCompletion(), "BrotherHurtBodyP3");
        public static GameObject mithrixHurt = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherHurtBody.prefab").WaitForCompletion();
        public static SpawnCard mithrixCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/Brother/cscBrother.asset").WaitForCompletion();
        public static SpawnCard mithrixHurtCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/Brother/cscBrotherHurt.asset").WaitForCompletion();
        public static GameObject mithrixGlass = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/BrotherGlass/BrotherGlassBody.prefab").WaitForCompletion();
        public static SpawnCard mithrixGlassCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Junk/BrotherGlass/cscBrotherGlass.asset").WaitForCompletion();
        public static GameObject firePillar = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherFirePillar.prefab").WaitForCompletion();
        public static GameObject firePillarGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherFirePillarGhost.prefab").WaitForCompletion();
        public static GameObject leftP4Line = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherUltLineProjectileRotateLeft.prefab").WaitForCompletion(), "P4UltLineLeft");
        public static GameObject rightP4Line = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherUltLineProjectileRotateRight.prefab").WaitForCompletion(), "P4UltLineRight");
        public static GameObject leftUltLine = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherUltLineProjectileRotateLeft.prefab").WaitForCompletion();
        public static GameObject rightUltLine = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherUltLineProjectileRotateRight.prefab").WaitForCompletion();
        public static GameObject staticUltLine = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherUltLineProjectileRotateLeft.prefab").WaitForCompletion(), "StaticUltLine");
        public static Material preBossMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Brother/matBrotherPreBossSphere.mat").WaitForCompletion();
        public static Material arenaWallMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon/matMoonArenaWall.mat").WaitForCompletion();
        public static Material stealAuraMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Brother/matBrotherStealAura.mat").WaitForCompletion();
        public static Material moonMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon/matMoonBridge.mat").WaitForCompletion();
        public static Material doppelMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/InvadingDoppelganger/matDoppelganger.mat").WaitForCompletion();
        public static SpawnCard mithrixHurtP3Card = ScriptableObject.CreateInstance<CharacterSpawnCard>();
        public static GameObject youngTeleporter = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/YoungTeleporter.prefab").WaitForCompletion();
        public static Transform practiceFire = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/Bazaar_Light.prefab").WaitForCompletion().transform.Find("FireLODLevel").gameObject, "PracticeFire").transform;
        public static GameObject implodeEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Vagrant/VagrantNovaExplosion.prefab").WaitForCompletion();
        public static GameObject tether = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteEarth/AffixEarthTetherVFX.prefab").WaitForCompletion();
        SkillDef fireLunarShardsDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/FireLunarShards.asset").WaitForCompletion();
        public static GameObject voidling = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase3.prefab").WaitForCompletion(), "InactiveVoidling");
        // TODO: actually make the skills and do this less lazily
        private static SkillDef shardDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/FireLunarShards.asset").WaitForCompletion();
        private static SkillDef slamDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/WeaponSlam.asset").WaitForCompletion();
        private static SkillDef bashDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/SprintBash.asset").WaitForCompletion();
        private static SkillDef leapDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/SkyLeap.asset").WaitForCompletion();


        public void Awake()
        {
            Log.Init(Logger);

            // TODO: actually make the skills and do this less lazily
            shardDef.activationState = new SerializableEntityStateType(typeof(EntityStates.FireUmbralShards));
            slamDef.activationState = new SerializableEntityStateType(typeof(EntityStates.UmbralHammerSlam));
            bashDef.activationState = new SerializableEntityStateType(typeof(EntityStates.UmbralBash));
            leapDef.activationState = new SerializableEntityStateType(typeof(EntityStates.EnterUmbralLeap));

            leapIndicatorPrefab.AddComponent<NetworkIdentity>();
            mithrixMaster.GetComponents<AISkillDriver>().Where(x => x.customName == "CastUlt").First().requiredSkill = null;
            //  mithrixMaster.GetComponents<AISkillDriver>().Where(x => x.customName == "Sprint after Target").First().minDistance = 25f;
            mithrixHurtP3.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(typeof(StaggerEnter));
            mithrixHurtP3Master.GetComponent<CharacterMaster>().bodyPrefab = mithrixHurtP3;
            mithrixHurtP3Card.name = "cscBrotherHurtP3";
            mithrixHurtP3Card.prefab = mithrixHurtP3Master;
            mithrixHurtP3Card.hullSize = mithrixHurtCard.hullSize;
            mithrixHurtP3Card.nodeGraphType = mithrixHurtCard.nodeGraphType;
            mithrixHurtP3Card.requiredFlags = mithrixHurtCard.requiredFlags;
            mithrixHurtP3Card.forbiddenFlags = mithrixHurtCard.forbiddenFlags;
            ContentAddition.AddBody(mithrixHurtP3);
            ContentAddition.AddMaster(mithrixHurtP3Master);
            Destroy(staticUltLine.GetComponent<RotateAroundAxis>());
            leftP4Line.GetComponent<RotateAroundAxis>().fastRotationSpeed = 10f;
            rightP4Line.GetComponent<RotateAroundAxis>().fastRotationSpeed = 10f;
            leftP4Line.GetComponent<RotateAroundAxis>().slowRotationSpeed = 10f;
            rightP4Line.GetComponent<RotateAroundAxis>().slowRotationSpeed = 10f;
            leftUltLine.GetComponent<RotateAroundAxis>().fastRotationSpeed = 21f;
            rightUltLine.GetComponent<RotateAroundAxis>().fastRotationSpeed = 21f;
            leftUltLine.GetComponent<RotateAroundAxis>().slowRotationSpeed = 21f;
            rightUltLine.GetComponent<RotateAroundAxis>().slowRotationSpeed = 21f;

            mithrixHurt.AddComponent<P4Controller>();

            AISkillDriver fireShards = mithrixMaster.GetComponents<AISkillDriver>().Where(x => x.customName == "Sprint and FireLunarShards").First();
            fireShards.minDistance = 20f;
            fireLunarShardsDef.baseMaxStock = 6;

            ModConfig.InitConfig(Config);
            CreateDoppelItem();
            MiscSetup();
            AddContent();
            P4DeathOrbSetup();
            ChangeVanillaEntityStateValues();
            MiscHooks miscHooks = new MiscHooks();
            MissionHooks missionHooks = new MissionHooks();
            MithrixMiscHooks mithrixMiscHooks = new MithrixMiscHooks();

            LanguageManager.Register(System.IO.Path.GetDirectoryName(Info.Location));
        }

        private void ChangeVanillaEntityStateValues()
        {
            SetAddressableEntityStateField("RoR2/Base/Brother/EntityStates.BrotherMonster.FistSlam.asset", "healthCostFraction", "0");
            SetAddressableEntityStateField("RoR2/Base/Brother/EntityStates.BrotherMonster.SpellChannelEnterState.asset", "duration", "3");
            SetAddressableEntityStateField("RoR2/Base/Brother/EntityStates.BrotherMonster.SpellChannelState.asset", "maxDuration", "5");
        }

        private void P4DeathOrbSetup()
        {
            if (voidling.TryGetComponent(out ModelLocator modelLocator))
            {
                Transform modelTransform = modelLocator.modelTransform;
                if (modelTransform)
                {
                    modelTransform.gameObject.SetActive(false);
                }
            }

            SphereZone safeZone = voidling.GetComponent<SphereZone>();
            safeZone.radius = 275f;

            if (safeZone.rangeIndicator)
            {
                MeshRenderer rangeIndicatorRenderer = safeZone.rangeIndicator.GetComponentInChildren<MeshRenderer>();
                if (rangeIndicatorRenderer)
                {
                    rangeIndicatorRenderer.sharedMaterials = [
                        preBossMat,
                        arenaWallMat,
                        stealAuraMat
                    ];
                }
            }

            FogDamageController fogDamageController = voidling.GetComponent<FogDamageController>();
            fogDamageController.healthFractionPerSecond = 0.01f;
            fogDamageController.healthFractionRampCoefficientPerSecond = 2.5f;
        }

        private void MiscSetup()
        {
            mithrix.GetComponent<CharacterBody>().bodyFlags |= CharacterBody.BodyFlags.ImmuneToExecutes;
            mithrixHurtP3.GetComponent<CharacterBody>().bodyFlags |= CharacterBody.BodyFlags.ImmuneToExecutes;
            mithrixGlass.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(typeof(EntityStates.GlassSpawnState));
        }

        public static void ArenaSetup()
        {
            GameObject finalAreaHolder = GameObject.Find("HOLDER: Final Arena");
            if (finalAreaHolder)
            {
                Transform innerColumns = finalAreaHolder.transform.Find("Columns_Inner");
                if (innerColumns)
                {
                    innerColumns.gameObject.SetActive(false);
                }

                Transform rocks = finalAreaHolder.transform.Find("Rocks");
                if (rocks)
                {
                    rocks.gameObject.SetActive(false);
                }

                /*
                Transform child1 = gameObject1.transform.GetChild(1);
                for (int index1 = 8; index1 < 12; ++index1)
                {
                  Transform child2 = child1.GetChild(index1);
                  for (int index2 = 0; index2 < 4; ++index2)
                    child2.GetChild(index2).gameObject.SetActive(true);
                }
                */
            }

            SceneInfo sceneInfo = SceneInfo.instance;
            if (sceneInfo)
            {
                Transform throneTransform = sceneInfo.transform.Find("BrotherMissionController/BrotherEncounter, Phase 1/PhaseObjects/mdlBrotherThrone");
                if (throneTransform)
                {
                    throneTransform.gameObject.SetActive(true);
                }
            }
        }

        public static void SpawnPracticeModeShrine()
        {
            GameObject gameObject1 = Instantiate(youngTeleporter, new Vector3(1090.1f, -283.1f, 1138.6f), Quaternion.identity);
            GameObject gameObject2 = Instantiate(practiceFire.gameObject, new Vector3(1090.1f, -283.1f, 1138.6f), Quaternion.identity);
            gameObject1.GetComponent<PurchaseInteraction>().NetworkcontextToken = "UMBRAL_PRACTICE_MODE_CONTEXT";
            gameObject1.name = "PracticeModeShrine";
            gameObject2.transform.parent = gameObject1.transform;
            gameObject2.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            NetworkServer.Spawn(gameObject1);
        }

        public static void AdjustBaseStats()
        {
            CharacterBody component1 = mithrix.GetComponent<CharacterBody>();
            CharacterDirection component2 = mithrix.GetComponent<CharacterDirection>();
            CharacterMotor component3 = mithrix.GetComponent<CharacterMotor>();
            CharacterBody component4 = mithrixGlass.GetComponent<CharacterBody>();
            CharacterDirection component5 = mithrixGlass.GetComponent<CharacterDirection>();
            CharacterMotor component6 = mithrixGlass.GetComponent<CharacterMotor>();
            component3.mass = ModConfig.mass.Value;
            component3.airControl = ModConfig.aircontrol.Value;

            if (Run.instance && Run.instance.name.Contains("Judgement"))
            {
                component4.baseMaxHealth = ModConfig.basehealth.Value * 0.25f;
                component4.levelMaxHealth = ModConfig.levelhealth.Value * 0.25f;
            }
            else
            {
                component4.baseMaxHealth = ModConfig.basehealth.Value;
                component4.levelMaxHealth = ModConfig.levelhealth.Value;
            }

            component4.baseDamage = ModConfig.basedamage.Value / 2f;
            component4.levelDamage = ModConfig.leveldamage.Value / 2f;
            component6.airControl = ModConfig.aircontrol.Value;
            component5.turnSpeed = ModConfig.turningspeed.Value;
            component1.baseMaxHealth = ModConfig.basehealth.Value;
            component1.levelMaxHealth = ModConfig.levelhealth.Value;
            component1.baseDamage = ModConfig.basedamage.Value;
            component1.levelDamage = ModConfig.leveldamage.Value;
            component1.baseAttackSpeed = ModConfig.baseattackspeed.Value;
            component1.baseMoveSpeed = ModConfig.basespeed.Value;
            component1.baseAcceleration = ModConfig.acceleration.Value;
            component1.baseJumpPower = ModConfig.jumpingpower.Value;
            component2.turnSpeed = ModConfig.turningspeed.Value;
            component1.baseArmor = ModConfig.basearmor.Value;
            FireLunarShards.projectilePrefab.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = ModConfig.ShardHoming.Value;
            ProjectileDirectionalTargetFinder component7 = FireLunarShards.projectilePrefab.GetComponent<ProjectileDirectionalTargetFinder>();
            component7.lookRange = ModConfig.ShardRange.Value;
            component7.lookCone = ModConfig.ShardCone.Value;
            component7.allowTargetLoss = true;
            WeaponSlam.duration = 3.5f / ModConfig.baseattackspeed.Value;
            ExitSkyLeap.waveProjectileCount = ModConfig.JumpWaveCount.Value;
            UltChannelState.waveProjectileCount = ModConfig.UltimateWaves.Value;
            UltChannelState.maxDuration = ModConfig.UltimateDuration.Value;
            UltChannelState.totalWaves = ModConfig.UltimateCount.Value;
            ExitSkyLeap.cloneDuration = (int)Math.Round((double)ModConfig.SpecialCD.Value);
        }

        public static void AdjustBaseSkills()
        {
            SkillLocator component = mithrix.GetComponent<SkillLocator>();
            SkillDef skillDef1 = component.primary.skillFamily.variants[0].skillDef;
            skillDef1.baseRechargeInterval = ModConfig.PrimCD.Value;
            skillDef1.baseMaxStock = ModConfig.PrimStocks.Value;
            SkillDef skillDef2 = component.secondary.skillFamily.variants[0].skillDef;
            skillDef2.baseRechargeInterval = ModConfig.SecCD.Value;
            skillDef2.baseMaxStock = ModConfig.SecStocks.Value;
            SkillDef skillDef3 = component.utility.skillFamily.variants[0].skillDef;
            skillDef3.baseMaxStock = ModConfig.UtilStocks.Value;
            skillDef3.baseRechargeInterval = ModConfig.UtilCD.Value;
            SkillDef skillDef4 = component.special.skillFamily.variants[0].skillDef;
            skillDef4.baseRechargeInterval = ModConfig.SpecialCD.Value;
            // skillDef4.activationState = PlayerCharacterMasterController.instances.Count > 1 ? new SerializableEntityStateType(typeof(EnterSkyLeap)) : new SerializableEntityStateType(typeof(EnterCrushingLeap));
        }

        public static void AdjustPhase2Stats()
        {
            mithrix.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(typeof(SkySpawnState));
            CharacterBody component1 = mithrix.GetComponent<CharacterBody>();
            CharacterDirection component2 = mithrix.GetComponent<CharacterDirection>();
            component1.baseMaxHealth = ModConfig.basehealth.Value * 1.5f;
            component1.levelMaxHealth = ModConfig.levelhealth.Value * 1.5f;
            component1.baseMoveSpeed = ModConfig.basespeed.Value;
            component1.baseAcceleration = ModConfig.acceleration.Value;
            component1.baseJumpPower = ModConfig.jumpingpower.Value;
            component2.turnSpeed = ModConfig.turningspeed.Value;
            WeaponSlam.duration = 3.5f / ModConfig.baseattackspeed.Value;
        }

        public static void AdjustPhase3Stats()
        {
            CharacterBody component1 = mithrix.GetComponent<CharacterBody>();
            CharacterBody component2 = mithrixHurtP3.GetComponent<CharacterBody>();
            CharacterDirection component3 = mithrix.GetComponent<CharacterDirection>();
            component1.baseMaxHealth = ModConfig.basehealth.Value;
            component1.levelMaxHealth = ModConfig.levelhealth.Value;
            component1.baseDamage = ModConfig.basedamage.Value;
            component1.levelDamage = ModConfig.leveldamage.Value;
            component2.baseMaxHealth = ModConfig.basehealth.Value;
            component2.levelMaxHealth = ModConfig.levelhealth.Value;
            component2.baseDamage = ModConfig.basedamage.Value / 2f;
            component2.levelDamage = ModConfig.leveldamage.Value / 2f;
            component1.baseMoveSpeed = ModConfig.basespeed.Value;
            component1.baseAcceleration = ModConfig.acceleration.Value;
            component1.baseJumpPower = ModConfig.jumpingpower.Value;
            component3.turnSpeed = ModConfig.turningspeed.Value;
            WeaponSlam.duration = 3.5f / ModConfig.baseattackspeed.Value;
            UltChannelState.waveProjectileCount = ModConfig.UltimateWaves.Value;
            UltChannelState.maxDuration = ModConfig.UltimateDuration.Value;
        }

        public static void AdjustPhase4Stats()
        {
            CharacterBody component = mithrixHurt.GetComponent<CharacterBody>();
            component.baseDamage = ModConfig.basedamage.Value;
            component.levelDamage = ModConfig.leveldamage.Value;
            component.GetComponent<SkillLocator>().primary = new GenericSkill();
            component.GetComponent<SkillLocator>().secondary = new GenericSkill();
        }

        private void AddContent()
        {
            ContentAddition.AddEntityState<EntityStates.GlassSpawnState>(out _);
            ContentAddition.AddEntityState<EntityStates.FireUmbralShards>(out _);
            ContentAddition.AddEntityState<EntityStates.UmbralHammerSlam>(out _);
            ContentAddition.AddEntityState<EntityStates.EnterUmbralLeap>(out _);
            ContentAddition.AddEntityState<EntityStates.HoldUmbralLeap>(out _);
            ContentAddition.AddEntityState<EntityStates.ExitUmbralLeap>(out _);
            ContentAddition.AddEntityState<EntityStates.UmbralBash>(out _);
        }

        private void CreateDoppelItem()
        {
            UmbralItem = ScriptableObject.CreateInstance<ItemDef>();
            UmbralItem.name = "UmbralMithrixUmbralItem";
            UmbralItem.deprecatedTier = ItemTier.Lunar;
            UmbralItem.tags = new ItemTag[] { ItemTag.WorldUnique, ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.CannotDuplicate };
            UmbralItem.nameToken = "UMBRALMITHRIX_UMBRAL_ITEM_NAME";
            UmbralItem.pickupToken = "UMBRALMITHRIX_UMBRAL_ITEM_PICKUP";
            UmbralItem.descriptionToken = "UMBRALMITHRIX_UMBRAL_ITEM_DESC";
            UmbralItem.loreToken = "UMBRALMITHRIX_UMBRAL_ITEM_LORE";
            UmbralItem.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/TeamDeath/texArtifactDeathDisabled.png").WaitForCompletion();
            UmbralItem.pickupModelPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/TeamDeath/PickupTeamDeath.prefab").WaitForCompletion(), "PickupUmbralCore", false);
            Material material = Addressables.LoadAssetAsync<Material>("RoR2/Base/InvadingDoppelganger/matDoppelganger.mat").WaitForCompletion();
            foreach (Renderer componentsInChild in UmbralItem.pickupModelPrefab.GetComponentsInChildren<Renderer>())
                componentsInChild.material = material;
            ContentAddition.AddItemDef(UmbralItem);
        }

        public static bool SetAddressableEntityStateField(string fullEntityStatePath, string fieldName, string value)
        {
            EntityStateConfiguration esc = Addressables.LoadAssetAsync<EntityStateConfiguration>(fullEntityStatePath).WaitForCompletion();
            for (int i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
            {
                if (esc.serializedFieldsCollection.serializedFields[i].fieldName == fieldName)
                {
                    esc.serializedFieldsCollection.serializedFields[i].fieldValue.stringValue = value;
                    return true;
                }
            }
            return false;
        }
    }
}