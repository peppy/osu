// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections;

namespace osu.Game.Overlays.FirstRunSetup
{
    public class ScreenBehaviour : FirstRunSetupScreen
    {
        private SearchContainer<SettingsSection> searchContainer;

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
                        new GeneralSection(),
                        new SkinSection(),
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

        private void applyClassic()
        {
            foreach (var i in searchContainer.ChildrenOfType<ISettingsItem>())
                i.ApplyClassicDefault(true);
        }

        private void applyStandard()
        {
            foreach (var i in searchContainer.ChildrenOfType<ISettingsItem>())
                i.ApplyClassicDefault(false);
        }
    }
}