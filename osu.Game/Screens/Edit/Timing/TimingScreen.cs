﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public class TimingScreen : EditorScreenWithTimeline
    {
        [Cached]
        public readonly Bindable<ControlPointGroup> SelectedGroup = new Bindable<ControlPointGroup>();

        public TimingScreen()
            : base(EditorScreenMode.Timing)
        {
        }

        protected override Drawable CreateMainContent() => new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            ColumnDimensions = new[]
            {
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 350),
            },
            Content = new[]
            {
                new Drawable[]
                {
                    new ControlPointList(),
                    new ControlPointSettings(),
                },
            }
        };

        public class ControlPointList : CompositeDrawable
        {
            private OsuButton deleteButton;
            private ControlPointTable table;

            private readonly IBindableList<ControlPointGroup> controlPointGroups = new BindableList<ControlPointGroup>();

            [Resolved]
            private EditorClock clock { get; set; }

            [Resolved]
            protected EditorBeatmap Beatmap { get; private set; }

            [Resolved]
            private Bindable<ControlPointGroup> selectedGroup { get; set; }

            [Resolved(canBeNull: true)]
            private IEditorChangeHandler changeHandler { get; set; }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colours)
            {
                RelativeSizeAxes = Axes.Both;

                const float margins = 10;
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = colours.Background4,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Box
                    {
                        Colour = colours.Background3,
                        RelativeSizeAxes = Axes.Y,
                        Width = ControlPointTable.TIMING_COLUMN_WIDTH + margins,
                    },
                    new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = table = new ControlPointTable(),
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Direction = FillDirection.Horizontal,
                        Margin = new MarginPadding(margins),
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            deleteButton = new RoundedButton
                            {
                                Text = "-",
                                Size = new Vector2(30, 30),
                                Action = delete,
                                Anchor = Anchor.BottomRight,
                                Origin = Anchor.BottomRight,
                            },
                            new RoundedButton
                            {
                                Text = "+ Add at current time",
                                Action = addNew,
                                Size = new Vector2(160, 30),
                                Anchor = Anchor.BottomRight,
                                Origin = Anchor.BottomRight,
                            },
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                selectedGroup.BindValueChanged(selected => { deleteButton.Enabled.Value = selected.NewValue != null; }, true);

                controlPointGroups.BindTo(Beatmap.ControlPointInfo.Groups);
                controlPointGroups.BindCollectionChanged((sender, args) =>
                {
                    table.ControlGroups = controlPointGroups;
                    changeHandler?.SaveState();
                }, true);
            }

            protected override void Update()
            {
                base.Update();

                trackActivePoint();
            }

            /// <summary>
            /// Given the user has selected a control point group, we want to track any group which is
            /// active at the current point in time which matches the type the user has selected.
            ///
            /// So if the user is currently looking at a timing point and seeks into the future, a
            /// future timing point would be automatically selected if it is now the new "current" point.
            /// </summary>
            private void trackActivePoint()
            {
                // For simplicity only match on the first type of the active control point.
                var selectedPointType = selectedGroup.Value?.ControlPoints.FirstOrDefault()?.GetType();

                if (selectedPointType != null)
                {
                    // We don't have an efficient way of looking up groups currently, only individual point types.
                    // To improve the efficiency of this in the future, we should reconsider the overall structure of ControlPointInfo.
                    IEnumerable<ControlPointGroup> groups = Beatmap.ControlPointInfo.Groups;

                    bool currentTimeBeforeSelectedGroup = clock.CurrentTimeAccurate < selectedGroup.Value.Time;

                    // Decide whether we are searching backwards or forwards.
                    if (currentTimeBeforeSelectedGroup)
                        groups = groups.Reverse();

                    // Find the next group which has the same type as the selected one.
                    groups = groups.SkipWhile(g => g != selectedGroup.Value)
                                   .Skip(1)
                                   .Where(g => g.ControlPoints.Any(cp => cp.GetType() == selectedPointType));

                    ControlPointGroup newGroup = groups.FirstOrDefault();

                    if (newGroup != null)
                    {
                        if (currentTimeBeforeSelectedGroup)
                        {
                            // When seeking backwards, the first match from the LINQ query is always what we want.
                            selectedGroup.Value = newGroup;
                        }
                        else
                        {
                            // When seeking forwards, we also need to check that the next match is before the current time.
                            if (newGroup.Time <= clock.CurrentTimeAccurate)
                                selectedGroup.Value = newGroup;
                        }
                    }
                }
            }

            private void delete()
            {
                if (selectedGroup.Value == null)
                    return;

                Beatmap.ControlPointInfo.RemoveGroup(selectedGroup.Value);

                selectedGroup.Value = Beatmap.ControlPointInfo.Groups.FirstOrDefault(g => g.Time >= clock.CurrentTime);
            }

            private void addNew()
            {
                selectedGroup.Value = Beatmap.ControlPointInfo.GroupAt(clock.CurrentTime, true);
            }
        }
    }
}
