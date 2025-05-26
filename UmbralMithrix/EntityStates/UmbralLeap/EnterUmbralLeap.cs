using RoR2;
using UnityEngine;
using EntityStates;
using EntityStates.BrotherMonster;

namespace UmbralMithrix.EntityStates;

public class EnterUmbralLeap : BaseState
{
    public static float baseDuration = 0.25f;
    public static string soundString = "Play_voidRaid_snipe_shoot_final";
    private float duration;
    private static int EnterUmbralLeapStateHash = Animator.StringToHash(nameof(EnterSkyLeap));
    private static int BufferEmptyStateHash = Animator.StringToHash("BufferEmpty");
    private static int SkyLeapParamHash = Animator.StringToHash("SkyLeap.playbackRate");

    public override void OnEnter()
    {
        base.OnEnter();
        this.duration = EnterUmbralLeap.baseDuration / this.attackSpeedStat;
        Util.PlaySound(EnterUmbralLeap.soundString, this.gameObject);
        this.PlayAnimation("Body", EnterUmbralLeap.EnterUmbralLeapStateHash, EnterUmbralLeap.SkyLeapParamHash, this.duration);
        this.PlayAnimation("FullBody Override", EnterUmbralLeap.BufferEmptyStateHash);
        this.characterDirection.moveVector = this.characterDirection.forward;
        this.characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, EnterUmbralLeap.baseDuration);
        AimAnimator aimAnimator = this.GetAimAnimator();
        if (!(bool)aimAnimator)
            return;
        aimAnimator.enabled = true;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!this.isAuthority || (double)this.fixedAge <= this.duration)
            return;
        this.outer.SetNextState(new HoldUmbralLeap());
    }
}
