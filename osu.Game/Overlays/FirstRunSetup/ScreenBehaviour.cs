// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections;
using osuTK.Graphics;

namespace osu.Game.Overlays.FirstRunSetup
{
    public class ScreenBehaviour : FirstRunSetupScreen
    {
        private SearchContainer<SettingsSection> searchContainer;

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.Children = new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 24))
                {
                    Text = FirstRunSetupOverlayStrings.BehaviourDescription,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 10),
                        new Dimension(),
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new[]
                        {
                            new TriangleButton
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Text = FirstRunSetupOverlayStrings.NewDefaults,
                                RelativeSizeAxes = Axes.X,
                                Action = applyStandard,
                            },
                            Empty(),
                            new DangerousTriangleButton
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Text = FirstRunSetupOverlayStrings.ClassicDefaults,
                                RelativeSizeAxes = Axes.X,
                                Action = applyClassic
                            }
                        },
                    },
                },
                searchContainer = new SearchContainer<SettingsSection>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new SettingsSection[]
                    {
                        // This list should be kept in sync with SettingsOverlay.
                        new GeneralSection(),
                        new SkinSection(),
                        // InputSection is intentionally omitted for now due to its sub-panel being a pain to set up.
                        new UserInterfaceSection(),
                        new GameplaySection(),
                        new RulesetSection(),
                        new AudioSection(),
                        new GraphicsSection(),
                        new OnlineSection(),
                        new MaintenanceSection(),
                        new DebugSection(),
                    },
                    SearchTerm = SettingsItem<bool>.CLASSIC_DEFAULT_SEARCH_TERM,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            foreach (var i in searchContainer.ChildrenOfType<ISettingsItem>().Where(s => s.HasClassicDefault))
            {
                var container = (Container)i;

                container.Padding = new MarginPadding { Right = 200 };
                container.Add(new GridContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 200,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopLeft,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = 5,
                                Margin = new MarginPadding(5),
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        Colour = colours.Blue,
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = i.GetDefault
                                    }
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = 5,
                                Margin = new MarginPadding(5),
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        Colour = colours.Pink,
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = i.GetClassicDefault
                                    }
                                }
                            },
                        },
                    }
                });
            }
        }

        private void applyClassic()
        {
            foreach (var i in searchContainer.ChildrenOfType<ISettingsItem>().Where(s => s.HasClassicDefault))
                i.ApplyClassicDefault();
        }

        private void applyStandard()
        {
            foreach (var i in searchContainer.ChildrenOfType<ISettingsItem>().Where(s => s.HasClassicDefault))
                i.ApplyDefault();
        }
    }
}
