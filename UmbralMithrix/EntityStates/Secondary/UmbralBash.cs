using RoR2;
using UnityEngine;
using EntityStates;
using EntityStates.BrotherMonster;
using RoR2.Projectile;
using EntityStates.LunarWisp;

namespace UmbralMithrix.EntityStates;

public class UmbralBash : BasicMeleeAttack
{
    public static float durationBeforePriorityReduces = 0.5f;
    public float baseDuration = 4f;
    public float damageCoefficient = 2f;
    public string hitBoxGroupName = "Weapon";
    public GameObject hitEffectPrefab = UmbralMithrix.umbralSlamHitEffect;
    public float procCoefficient = 1f;
    public float pushAwayForce = 6000f;
    public Vector3 forceVector = new Vector3(0f, 2000f, 0f);
    public float hitPauseDuration = 0.1f;
    public GameObject swingEffectPrefab = UmbralMithrix.umbralSwingEffect;
    public string swingEffectMuzzleString = "MuzzleSprintBash";
    public string mecanimHitboxActiveParameter = "weapon.hitBoxActive";
    public float shorthopVelocityFromHit = 0f;
    public string beginStateSoundString = "Play_moonBrother_swing_horizontal";
    public float forceForwardVelocity = 1f;
    public NetworkSoundEventDef impactSound = UmbralMithrix.umbralSlamHitSound;
    public AnimationCurve forwardVelocityCurve = new SprintBash().forwardVelocityCurve;


    public override void PlayAnimation()
    {
        this.PlayCrossfade("FullBody Override", nameof(SprintBash), "SprintBash.playbackRate", this.duration, 0.05f);
    }

    public override void OnEnter()
    {
        base.OnEnter();
        if (this.isAuthority)
        {
            Ray aimRay = this.GetAimRay();
            if (this.characterBody.name == "BrotherBody(Clone)")
            {
                for (int index = 0; index < 6; ++index)
                {
                    Util.PlaySound(FireUmbralShards.fireSound, this.gameObject);
                    ProjectileManager.instance.FireProjectile(FireUmbralShards.projectilePrefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction), this.gameObject, (float)((double)this.characterBody.damage * 0.100000001490116 / 12.0), 0.0f, Util.CheckRoll(this.characterBody.crit, this.characterBody.master));
                }

                if (PhaseCounter.instance && PhaseCounter.instance.phase != 1 && ModConfig.addShockwave.Value)
                {
                    Vector3 vector3 = Vector3.ProjectOnPlane(this.inputBank.aimDirection, Vector3.up);
                    Vector3 footPosition = this.characterBody.footPosition;
                    Vector3 forward = Quaternion.AngleAxis(0.0f, Vector3.up) * vector3;
                    ProjectileManager.instance.FireProjectile(WeaponSlam.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), this.gameObject, this.characterBody.damage * WeaponSlam.waveProjectileDamageCoefficient, WeaponSlam.waveProjectileForce, Util.CheckRoll(this.characterBody.crit, this.characterBody.master));
                }
            }
            else
            {
                ProjectileManager.instance.FireProjectile(SeekingBomb.projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), this.gameObject, this.characterBody.damage * (SeekingBomb.bombDamageCoefficient * 0.75f), SeekingBomb.bombForce, Util.CheckRoll(this.critStat, this.characterBody.master), speedOverride: 0.0f);
            }
        }
        AimAnimator aimAnimator = this.GetAimAnimator();
        if ((bool)aimAnimator)
            aimAnimator.enabled = true;
        if (!(bool)this.characterDirection)
            return;
        this.characterDirection.forward = this.inputBank.aimDirection;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!this.isAuthority || !(bool)this.inputBank || !(bool)this.skillLocator || !this.skillLocator.utility.IsReady() || !this.inputBank.skill3.justPressed)
            return;
        this.skillLocator.utility.ExecuteIfReady();
    }

    public override void OnExit()
    {
        Transform modelChild = this.FindModelChild("SpinnyFX");
        if ((bool)modelChild)
            modelChild.gameObject.SetActive(false);
        this.PlayCrossfade("FullBody Override", "BufferEmpty", 0.1f);
        base.OnExit();
    }

    public override InterruptPriority GetMinimumInterruptPriority()
    {
        return (double)this.fixedAge <= SprintBash.durationBeforePriorityReduces ? InterruptPriority.PrioritySkill : InterruptPriority.Skill;
    }
}
