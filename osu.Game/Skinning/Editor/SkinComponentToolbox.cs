// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Skinning.Editor
{
    public class SkinComponentToolbox : ScrollingToolboxGroup
    {
        public const float WIDTH = 200;

        public Action<Type> RequestPlacement;

        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor(new DummyRuleset())
        {
            Combo = { Value = RNG.Next(1, 1000) },
            TotalScore = { Value = RNG.Next(1000, 10000000) }
        };

        [Cached(typeof(HealthProcessor))]
        private HealthProcessor healthProcessor = new DrainingHealthProcessor(0);

        public SkinComponentToolbox(float height)
            : base("Components", height)
        {
            RelativeSizeAxes = Axes.None;
            Width = WIDTH;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            FillFlowContainer fill;

            Child = fill = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(2)
            };

            var skinnableTypes = typeof(OsuGame).Assembly.GetTypes()
                                                .Where(t => !t.IsInterface)
                                                .Where(t => typeof(ISkinnableDrawable).IsAssignableFrom(t))
                                                .OrderBy(t => t.Name)
                                                .ToArray();

            foreach (var type in skinnableTypes)
            {
                var component = attemptAddComponent(type);

                if (component != null)
                {
                    component.RequestPlacement = t => RequestPlacement?.Invoke(t);
                    fill.Add(component);
                }
            }
        }

        private static ToolboxComponentButton attemptAddComponent(Type type)
        {
            try
            {
                var instance = (Drawable)Activator.CreateInstance(type);

                Debug.Assert(instance != null);

                if (!((ISkinnableDrawable)instance).IsEditable)
                    return null;

                return new ToolboxComponentButton(instance);
            }
            catch
            {
                return null;
            }
        }

        private class ToolboxComponentButton : OsuButton
        {
            protected override bool ShouldBeConsideredForInput(Drawable child) => false;

            public override bool PropagateNonPositionalInputSubTree => false;

            private readonly Drawable component;

            public Action<Type> RequestPlacement;

            private Container innerContainer;

            private const float contracted_size = 60;
            private const float expanded_size = 120;

            public ToolboxComponentButton(Drawable component)
            {
                this.component = component;

                Enabled.Value = true;

                RelativeSizeAxes = Axes.X;
                Height = contracted_size;
            }

            protected override bool OnHover(HoverEvent e)
            {
                this.Delay(300).ResizeHeightTo(expanded_size, 500, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                this.ResizeHeightTo(contracted_size, 500, Easing.OutQuint);
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider, OsuColour colours)
            {
                BackgroundColour = colourProvider.Background3;

                AddRange(new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(10) { Bottom = 20 },
                        Masking = true,
                        Child = innerContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = component
                        },
                    },
                    new OsuSpriteText
                    {
                        Text = component.GetType().Name,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Margin = new MarginPadding(5),
                    },
                });

                // adjust provided component to fit / display in a known state.
                component.Anchor = Anchor.Centre;
                component.Origin = Anchor.Centre;
            }

            protected override void Update()
            {
                base.Update();

                if (component.DrawSize != Vector2.Zero)
                {
                    float bestScale = Math.Min(
                        innerContainer.DrawWidth / component.DrawWidth,
                        innerContainer.DrawHeight / component.DrawHeight);

                    innerContainer.Scale = new Vector2(bestScale);
                }
            }

            protected override bool OnClick(ClickEvent e)
            {
                RequestPlacement?.Invoke(component.GetType());
                return true;
            }
        }

        private class DummyRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type) => throw new NotImplementedException();
            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => throw new NotImplementedException();
            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => throw new NotImplementedException();
            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => throw new NotImplementedException();
            public override string Description => string.Empty;
            public override string ShortName => string.Empty;
        }
    }
}
