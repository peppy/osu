// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public partial class BeatmapSelectionScreen : OsuScreen
    {
        private OsuScrollContainer scroll = null!;
        private FillFlowContainer<Panel> panelFlow = null!;
        private Container<Panel> remainingPanelContainer = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                scroll = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = panelFlow = new FillFlowContainer<Panel>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(20),
                        Spacing = new Vector2(20),
                    }
                },
                remainingPanelContainer = new Container<Panel>
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };

            for (int i = 0; i < 50; i++)
            {
                panelFlow.Add(new Panel
                {
                    Size = new Vector2(300, 70),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                });
            }
        }

        private Panel[]? panelsToKeep;

        public void DistributeUsers(IReadOnlyList<APIUser> users)
        {
            panelsToKeep = Random.Shared.GetItems(panelFlow.Children.ToArray(), users.Count);

            for (int i = 0; i < users.Count; i++)
            {
                panelsToKeep[i].Selection.AddUser(users[i]);
            }
        }

        public void SelectFinalBeatmap()
        {
            const int num_steps = 25;
            const double duration = 4000;

            Panel? lastPanel = null;

            for (int i = 0; i < num_steps; i++)
            {
                float progress = ((float)i) / (num_steps - 1);

                double delay = Math.Pow(progress, 2.5) * duration;

                int index = i;

                Scheduler.AddDelayed(() =>
                {
                    var panel = remainingPanelContainer.Children[index % remainingPanelContainer.Children.Count];

                    lastPanel?.HideBorder();
                    panel.ShowBorder();

                    lastPanel = panel;
                }, delay);
            }

            Scheduler.AddDelayed(() =>
            {
                var finalPanel = lastPanel!;

                foreach (var panel in remainingPanelContainer)
                {
                    if (panel == finalPanel)
                    {
                        panel.PresentAsFinalBeatmap();
                    }
                    else
                    {
                        panel.FadeOut(200);
                        panel.PopOut(easing: Easing.InOutQuad);
                    }
                }
            }, duration + 1000);
        }

        private void updateLayout(Container<Panel> container, Vector2 panelSize, int? maxItemsPerRow = null, bool centerVertically = false, float stagger = 5)
        {
            const float spacing = 20f;
            const float duration = 1000;

            int numPanelsPerRow = (int)((container.ChildSize.X + spacing) / (panelSize.X + spacing));
            if (numPanelsPerRow > maxItemsPerRow)
                numPanelsPerRow = maxItemsPerRow.Value;

            int lastRowPanelCount = container.Children.Count % numPanelsPerRow;
            int outerPanelsWithOffsetCount = (numPanelsPerRow - lastRowPanelCount) % 2 == 0 ? (numPanelsPerRow - lastRowPanelCount) / 2 : 0;

            var children = container.Children.ToArray();

            var position = new Vector2(calculateNextRowStart(0), 0);

            int numRows = (children.Length + numPanelsPerRow - 1) / numPanelsPerRow;

            if (centerVertically)
            {
                float totalHeight = (numRows * (panelSize.Y + spacing)) - spacing;

                position.Y = (ChildSize.Y - totalHeight) / 2;
            }

            double delay = 0;

            for (int i = 0; i < children.Length; i++)
            {
                var panel = children[i];
                int rowIndex = i / numPanelsPerRow;
                int positionInRow = i % numPanelsPerRow;

                if (position.X + panelSize.X > panelFlow.ChildSize.X)
                {
                    position.X = calculateNextRowStart(i);
                    position.Y += panelSize.Y + spacing;
                }

                var panelPosition = position;

                if (centerVertically && rowIndex < numRows - 1 && (positionInRow < outerPanelsWithOffsetCount || positionInRow >= numPanelsPerRow - outerPanelsWithOffsetCount))
                {
                    panelPosition.Y += (panelSize.Y + spacing) / 2;
                }

                panel
                    .Delay(delay)
                    .MoveTo(panelPosition, duration, Easing.InOutQuint)
                    .ResizeTo(panelSize, duration, Easing.InOutQuint);

                position.X += panelSize.X + spacing;

                delay += stagger;
            }

            float calculateNextRowStart(int currentIndex)
            {
                int remaining = Math.Min(children.Length - currentIndex, numPanelsPerRow);
                float nextRowWidth = ((panelSize.X + spacing) * remaining - spacing);

                return (panelFlow.ChildSize.X - nextRowWidth) / 2;
            }
        }

        public void HidePanels(int remainingCount)
        {
            panelsToKeep ??= Random.Shared.GetItems(panelFlow.Children.ToArray(), remainingCount);

            scroll.ScrollbarVisible = false;
            panelFlow.AutoSizeAxes = Axes.None;

            foreach (var panel in panelFlow.Children.ToArray())
            {
                if (!panelsToKeep.Contains(panel))
                {
                    panel.PopOut(Random.Shared.NextSingle() * 500);
                }
                else
                {
                    var position = panel.ScreenSpaceDrawQuad.TopLeft;

                    panelFlow.Remove(panel, false);

                    panel.Anchor = panel.Origin = Anchor.TopLeft;
                    panel.Position = remainingPanelContainer.ToLocalSpace(position);

                    remainingPanelContainer.Add(panel);
                }
            }

            Scheduler.AddDelayed(() =>
            {
                int maxItemsPerRow = remainingCount == 3 ? 2 : 3;

                updateLayout(remainingPanelContainer, new Vector2(300, 70), maxItemsPerRow, true, stagger: 20);
            }, 200);
        }

        private partial class Panel : CompositeDrawable
        {
            public readonly BeatmapSelectionPanel Selection;
            private readonly Drawable border;

            public Panel()
            {
                InternalChildren = new[]
                {
                    border = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Padding = new MarginPadding(-4),
                        Alpha = 0,
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 10,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                        },
                    },
                    Selection = new BeatmapSelectionPanel(300, 70)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 6,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.LightSlateGray,
                            }
                        }
                    }
                };
            }

            public void PopOut(double delay = 0, Easing easing = Easing.InCubic)
            {
                Selection.Delay(delay)
                         .ScaleTo(0, 400, easing)
                         .FadeOut(400);
            }

            public void ShowBorder() => border.Show();

            public void HideBorder() => border.Hide();

            public void PresentAsFinalBeatmap()
            {
                Selection.ScaleTo(1.5f, 800, Easing.OutElasticHalf);
                border.ScaleTo(1.5f, 800, Easing.OutElasticHalf);

                this.MoveTo((Parent!.ChildSize - LayoutSize) / 2, 1200, Easing.OutExpo);
            }
        }
    }
}
