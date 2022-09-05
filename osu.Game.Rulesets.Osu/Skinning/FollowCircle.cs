// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public abstract class FollowCircle : CompositeDrawable
    {
        [Resolved]
        protected DrawableHitObject? ParentObject { get; private set; }

        protected FollowCircle()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var drawableSlider = ((DrawableSlider?)ParentObject);

            drawableSlider?.Tracking.BindValueChanged(tracking =>
            {
                Debug.Assert(ParentObject != null);
                if (ParentObject.Judged)
                    return;

                if (Time.Current >= drawableSlider.StateUpdateTime && Time.Current <= drawableSlider.HitObject.GetEndTime())
                {
                    if (tracking.NewValue)
                        OnSliderPress();
                    else
                        OnSliderRelease();
                }
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (ParentObject != null)
            {
                ParentObject.ApplyCustomUpdateState += updateStateTransforms;
                updateStateTransforms(ParentObject, ParentObject.State.Value);
            }
        }

        private void updateStateTransforms(DrawableHitObject drawableObject, ArmedState state)
        {
            Debug.Assert(ParentObject != null);

            switch (state)
            {
                case ArmedState.Idle:
                    switch (drawableObject)
                    {
                        case DrawableSliderHead:
                            using (BeginAbsoluteSequence(ParentObject.StateUpdateTime))
                            {
                                Logger.Log($"Resetting at time {ParentObject.StateUpdateTime}");
                                ClearTransformsAfter(ParentObject.StateUpdateTime, true);
                                this.ScaleTo(1)
                                    .FadeOut();

                                if ((drawableObject as DrawableSlider)?.Tracking.Value == true)
                                    OnSliderPress();
                            }

                            break;
                    }

                    break;

                case ArmedState.Hit:
                    switch (drawableObject)
                    {
                        case DrawableSliderTail:
                            // Use ParentObject instead of drawableObject because slider tail's
                            // HitStateUpdateTime is ~36ms before the actual slider end (aka slider
                            // tail leniency)
                            using (BeginAbsoluteSequence(ParentObject.HitStateUpdateTime))
                            {
                                Logger.Log($"[TAIL] Applying slider end transforms at {ParentObject.HitStateUpdateTime}");
                                OnSliderEnd();
                            }

                            break;

                        case DrawableSliderTick:
                        case DrawableSliderRepeat:
                            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
                            {
                                Logger.Log($"[TICK / REPEAT] Applying slider end transforms at {drawableObject.HitStateUpdateTime}");
                                OnSliderTick();
                            }

                            break;
                    }

                    break;

                case ArmedState.Miss:
                    switch (drawableObject)
                    {
                        case DrawableSliderTail:
                        case DrawableSliderTick:
                        case DrawableSliderRepeat:
                            // Despite above comment, ok to use drawableObject.HitStateUpdateTime
                            // here, since on stable, the break anim plays right when the tail is
                            // missed, not when the slider ends
                            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
                            {
                                Logger.Log($"[MISS] Applying slider end transforms at {drawableObject.HitStateUpdateTime}");
                                OnSliderBreak();
                            }

                            break;
                    }

                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (ParentObject != null)
            {
                ParentObject.ApplyCustomUpdateState -= updateStateTransforms;
            }
        }

        protected abstract void OnSliderPress();

        protected abstract void OnSliderRelease();

        protected abstract void OnSliderEnd();

        protected abstract void OnSliderTick();

        protected abstract void OnSliderBreak();
    }
}
