using BepInEx;
using EntityStates;
using EntityStates.BrotherMonster;
using EntityStates.BrotherMonster.Weapon;
using R2API;
using Rewired.ComponentControls.Effects;
using RoR2;
using RoR2.CharacterAI;
using RoR2.ContentManagement;
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

        public static GameObject umbralSlamImpact;
        public static GameObject umbralSlamProjectile;
        public static GameObject umbralSlamPillar;
        public static GameObject umbralSlamHitEffect;
        public static NetworkSoundEventDef umbralSlamHitSound;

        public static GameObject umbralLeapWave;
        public static GameObject umbralSwingEffect;
        public static GameObject umbralUltMuzzleFlash;

        public static GameObject leapIndicatorPrefab;
        public static GameObject leapIndicator;
        public static SpawnCard timeCrystalCard;
        public static GameObject lunarMissile;
        public static GameObject mithrixHurtP3Master;
        public static GameObject mithrix;
        public static GameObject mithrixHurtP3;
        public static GameObject mithrixHurt;
        public static SpawnCard mithrixCard;
        public static SpawnCard mithrixHurtCard;
        public static GameObject mithrixGlass;
        public static SpawnCard mithrixGlassCard;
        public static GameObject leftP4Line;
        public static GameObject rightP4Line;
        public static GameObject leftUltLine;
        public static GameObject rightUltLine;
        public static GameObject staticUltLine;
        public static Material preBossMat;
        public static Material arenaWallMat;
        public static Material stealAuraMat;
        public static Material moonMat;
        public static Material doppelMat;
        public static GameObject youngTeleporter;
        public static Transform practiceFire;
        public static GameObject implodeEffect;
        public static GameObject tether;
        public static GameObject voidling;
        public static SpawnCard mithrixHurtP3Card = ScriptableObject.CreateInstance<CharacterSpawnCard>();

        // TODO: actually make the skills and do this less lazily
        private static SkillDef shardDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/FireLunarShards.asset").WaitForCompletion();
        private static SkillDef slamDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/WeaponSlam.asset").WaitForCompletion();
        private static SkillDef bashDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/SprintBash.asset").WaitForCompletion();
        private static SkillDef leapDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/SkyLeap.asset").WaitForCompletion();


        public void Awake()
        {
            Log.Init(Logger);

            LoadAssets();
            CloneAssets();
            SetupVoidling();

            // TODO: actually make the skills and do this less lazily
            shardDef.activationState = new SerializableEntityStateType(typeof(EntityStates.FireUmbralShards));
            slamDef.activationState = new SerializableEntityStateType(typeof(EntityStates.UmbralHammerSlam));
            bashDef.activationState = new SerializableEntityStateType(typeof(EntityStates.UmbralBash));
            leapDef.activationState = new SerializableEntityStateType(typeof(EntityStates.EnterUmbralLeap));

            ModConfig.InitConfig(Config);
            CreateDoppelItem();
            AddEntityStates();
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

        private void AddEntityStates()
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

        private void SetupVoidling()
        {
            AssetReferenceT<GameObject> voidlingRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_VoidRaidCrab.MiniVoidRaidCrabBodyPhase3_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(voidlingRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                voidling = PrefabAPI.InstantiateClone(result, "InactiveVoidling");

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
            };
        }

        private void CloneAssets()
        {
            AssetReferenceT<GameObject> leftUltLineRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherUltLineProjectileRotateLeft_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(leftUltLineRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                leftUltLine = PrefabAPI.InstantiateClone(result, "UmbralUltLineLeft");
                leftP4Line = PrefabAPI.InstantiateClone(result, "P4UltLineLeft");
                staticUltLine = PrefabAPI.InstantiateClone(result, "StaticUltLine");
                Destroy(staticUltLine.GetComponent<RotateAroundAxis>());

                RotateAroundAxis leftUltRotate = leftUltLine.GetComponent<RotateAroundAxis>();
                leftUltRotate.fastRotationSpeed = 21f;
                leftUltRotate.slowRotationSpeed = 21f;

                RotateAroundAxis leftP4Rotate = leftUltLine.GetComponent<RotateAroundAxis>();
                leftP4Rotate.fastRotationSpeed = 10f;
                leftP4Rotate.slowRotationSpeed = 10f;
            };
            AssetReferenceT<GameObject> rightUltLineRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherUltLineProjectileRotateRight_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(rightUltLineRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                rightUltLine = PrefabAPI.InstantiateClone(result, "UmbralUltLineRight");
                rightP4Line = PrefabAPI.InstantiateClone(result, "P4UltLineRight");

                RotateAroundAxis rightUltRotate = rightUltLine.GetComponent<RotateAroundAxis>();
                rightUltRotate.fastRotationSpeed = 21f;
                rightUltRotate.slowRotationSpeed = 21f;

                RotateAroundAxis rightP4Rotate = rightUltLine.GetComponent<RotateAroundAxis>();
                rightP4Rotate.fastRotationSpeed = 10f;
                rightP4Rotate.slowRotationSpeed = 10f;
            };
            AssetReferenceT<GameObject> bodyP3Ref = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherHurtBody_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(bodyP3Ref).Completed += (x) =>
            {
                GameObject result = x.Result;
                mithrixHurtP3 = PrefabAPI.InstantiateClone(result, "BrotherHurtBodyP3");
                mithrixHurtP3.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(typeof(StaggerEnter));
                mithrixHurtP3.GetComponent<CharacterBody>().bodyFlags |= CharacterBody.BodyFlags.ImmuneToExecutes;

                ContentAddition.AddBody(mithrixHurtP3);
            };
            AssetReferenceT<GameObject> masterP3Ref = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherHurtMaster_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(masterP3Ref).Completed += (x) =>
            {
                GameObject result = x.Result;
                mithrixHurtP3Master = PrefabAPI.InstantiateClone(result, "BrotherHurtMasterP3");
                mithrixHurtP3Master.GetComponent<CharacterMaster>().bodyPrefab = mithrixHurtP3;

                mithrixHurtP3Card.name = "cscBrotherHurtP3";
                mithrixHurtP3Card.prefab = mithrixHurtP3Master;
                mithrixHurtP3Card.hullSize = mithrixHurtCard.hullSize;
                mithrixHurtP3Card.nodeGraphType = mithrixHurtCard.nodeGraphType;
                mithrixHurtP3Card.requiredFlags = mithrixHurtCard.requiredFlags;
                mithrixHurtP3Card.forbiddenFlags = mithrixHurtCard.forbiddenFlags;

                ContentAddition.AddMaster(mithrixHurtP3Master);
            };
            AssetReferenceT<GameObject> practiceFireRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_bazaar_Bazaar.Light_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(practiceFireRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                practiceFire = PrefabAPI.InstantiateClone(result.transform.Find("FireLODLevel").gameObject, "PracticeFire").transform;
            };
            AssetReferenceT<GameObject> lunarMissileRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_EliteLunar.LunarMissileProjectile_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(lunarMissileRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                lunarMissile = PrefabAPI.InstantiateClone(result, "UmbralLunarMissile", false);
            };
            AssetReferenceT<GameObject> indicatorRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Vagrant.VagrantNovaAreaIndicator_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(indicatorRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                leapIndicatorPrefab = PrefabAPI.InstantiateClone(result, "UmbralLeapIndicator");
                leapIndicatorPrefab.AddComponent<NetworkIdentity>();
            };
            AssetReferenceT<GameObject> waveRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherSunderWave_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(waveRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                umbralLeapWave = PrefabAPI.InstantiateClone(result, "UmbralLeapWave");
            };
        }

        private void LoadAssets()
        {
            AssetReferenceT<GameObject> tpRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Junk.YoungTeleporter_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(tpRef).Completed += (x) => youngTeleporter = x.Result;
            AssetReferenceT<GameObject> tetherRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_EliteEarth.AffixEarthTetherVFX_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(tetherRef).Completed += (x) => tether = x.Result;
            AssetReferenceT<GameObject> implodeRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Vagrant.VagrantNovaExplosion_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(implodeRef).Completed += (x) => implodeEffect = x.Result;
            AssetReferenceT<GameObject> slamImpactRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherSlamImpact_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(slamImpactRef).Completed += (x) => umbralSlamImpact = x.Result;
            AssetReferenceT<GameObject> slamProjectileRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother_BrotherSunderWave.Energized_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(slamProjectileRef).Completed += (x) => umbralSlamProjectile = x.Result;
            AssetReferenceT<GameObject> slamPillarRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherFirePillar_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(slamPillarRef).Completed += (x) => umbralSlamPillar = x.Result;
            AssetReferenceT<GameObject> slamHitRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Huntress.OmniImpactVFXHuntress_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(slamHitRef).Completed += (x) => umbralSlamHitEffect = x.Result;
            AssetReferenceT<GameObject> swingEffectRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother_BrotherSwing1.Kickup_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(swingEffectRef).Completed += (x) => umbralSwingEffect = x.Result;
            AssetReferenceT<GameObject> ultFlashRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.ItemStealEndMuzzleflash_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(ultFlashRef).Completed += (x) => umbralUltMuzzleFlash = x.Result;
            AssetReferenceT<GameObject> masterRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherMaster_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(masterRef).Completed += (x) =>
            {
                GameObject mithrixMaster = x.Result;
                //  mithrixMaster.GetComponents<AISkillDriver>().Where(x => x.customName == "Sprint after Target").First().minDistance = 25f;
                mithrixMaster.GetComponents<AISkillDriver>().Where(x => x.customName == "CastUlt").First().requiredSkill = null;
                AISkillDriver fireShards = mithrixMaster.GetComponents<AISkillDriver>().Where(x => x.customName == "Sprint and FireLunarShards").First();
                fireShards.minDistance = 20f;
            };
            AssetReferenceT<GameObject> bodyRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherBody_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(bodyRef).Completed += (x) =>
            {
                mithrix = x.Result;
                mithrix.GetComponent<CharacterBody>().bodyFlags |= CharacterBody.BodyFlags.ImmuneToExecutes;
            };
            AssetReferenceT<GameObject> bodyHurtRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherHurtBody_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(bodyHurtRef).Completed += (x) =>
            {
                mithrixHurt = x.Result;
                mithrixHurt.AddComponent<P4Controller>();
            };
            AssetReferenceT<GameObject> glassBodyRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Junk_BrotherGlass.BrotherGlassBody_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(glassBodyRef).Completed += (x) =>
            {
                mithrixGlass = x.Result;
                mithrixGlass.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(typeof(EntityStates.GlassSpawnState));
            };

            AssetReferenceT<SpawnCard> crystalCardRef = new AssetReferenceT<SpawnCard>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_WeeklyRun.bscTimeCrystal_asset);
            AssetAsyncReferenceManager<SpawnCard>.LoadAsset(crystalCardRef).Completed += (x) => timeCrystalCard = x.Result;
            AssetReferenceT<SpawnCard> mithrixCardRef = new AssetReferenceT<SpawnCard>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.cscBrother_asset);
            AssetAsyncReferenceManager<SpawnCard>.LoadAsset(mithrixCardRef).Completed += (x) => mithrixCard = x.Result;
            AssetReferenceT<SpawnCard> mithrixHurtCardRef = new AssetReferenceT<SpawnCard>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.cscBrotherHurt_asset);
            AssetAsyncReferenceManager<SpawnCard>.LoadAsset(mithrixHurtCardRef).Completed += (x) => mithrixHurtCard = x.Result;
            AssetReferenceT<SpawnCard> glassCardRef = new AssetReferenceT<SpawnCard>(RoR2BepInExPack.GameAssetPaths.RoR2_Junk_BrotherGlass.cscBrotherGlass_asset);
            AssetAsyncReferenceManager<SpawnCard>.LoadAsset(glassCardRef).Completed += (x) => mithrixGlassCard = x.Result;

            AssetReferenceT<Material> bossMatRef = new AssetReferenceT<Material>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.matBrotherPreBossSphere_mat);
            AssetAsyncReferenceManager<Material>.LoadAsset(bossMatRef).Completed += (x) => preBossMat = x.Result;
            AssetReferenceT<Material> arenaWallMatRef = new AssetReferenceT<Material>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_moon.matMoonArenaWall_mat);
            AssetAsyncReferenceManager<Material>.LoadAsset(arenaWallMatRef).Completed += (x) => arenaWallMat = x.Result;
            AssetReferenceT<Material> stealAuraMatRef = new AssetReferenceT<Material>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.matBrotherStealAura_mat);
            AssetAsyncReferenceManager<Material>.LoadAsset(stealAuraMatRef).Completed += (x) => stealAuraMat = x.Result;
            AssetReferenceT<Material> moonBridgeMatRef = new AssetReferenceT<Material>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_moon.matMoonBridge_mat);
            AssetAsyncReferenceManager<Material>.LoadAsset(moonBridgeMatRef).Completed += (x) => moonMat = x.Result;
            AssetReferenceT<Material> doppelMatRef = new AssetReferenceT<Material>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_InvadingDoppelganger.matDoppelganger_mat);
            AssetAsyncReferenceManager<Material>.LoadAsset(doppelMatRef).Completed += (x) => doppelMat = x.Result;

            AssetReferenceT<NetworkSoundEventDef> slamSoundRef = new AssetReferenceT<NetworkSoundEventDef>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Croco.nseAcridBiteHit_asset);
            AssetAsyncReferenceManager<NetworkSoundEventDef>.LoadAsset(slamSoundRef).Completed += (x) => umbralSlamHitSound = x.Result;

            AssetReferenceT<SkillDef> fireShardSkillRef = new AssetReferenceT<SkillDef>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.FireLunarShards_asset);
            AssetAsyncReferenceManager<SkillDef>.LoadAsset(fireShardSkillRef).Completed += (x) =>
            {
                SkillDef fireLunarShardsDef = x.Result;
                fireLunarShardsDef.baseMaxStock = 6;
            };
        }
    }
}