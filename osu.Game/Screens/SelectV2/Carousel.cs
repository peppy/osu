// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// A highly efficient vertical list display that is used primarily for the song select screen,
    /// but flexible enough to be used for other use cases.
    /// </summary>
    public abstract partial class Carousel<T> : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        #region Properties and methods for external usage

        /// <summary>
        /// Height of the area above the carousel that should be treated as visible due to transparency of elements in front of it.
        /// </summary>
        public float BleedTop { get; set; } = 0;

        /// <summary>
        /// Height of the area below the carousel that should be treated as visible due to transparency of elements in front of it.
        /// </summary>
        public float BleedBottom { get; set; } = 0;

        /// <summary>
        /// The number of pixels outside the carousel's vertical bounds to manifest drawables.
        /// This allows preloading content before it scrolls into view.
        /// </summary>
        public float DistanceOffscreenToPreload { get; set; }

        /// <summary>
        /// Vertical space between panel layout. Negative value can be used to create an overlapping effect.
        /// </summary>
        protected float SpacingBetweenPanels { get; set; } = -5;

        /// <summary>
        /// When a new request arrives to change filtering, the number of milliseconds to wait before performing the filter.
        /// Regardless of any external debouncing, this is a safety measure to avoid triggering too many threaded operations.
        /// </summary>
        public int DebounceDelay { get; set; }

        /// <summary>
        /// Whether an asynchronous filter / group operation is currently underway.
        /// </summary>
        public bool IsFiltering => !filterTask.IsCompleted;

        /// <summary>
        /// The number of displayable items currently being tracked (before filtering).
        /// </summary>
        public int ItemsTracked => Items.Count;

        /// <summary>
        /// The number of carousel items currently in rotation for display.
        /// </summary>
        public int DisplayableItems => displayedCarouselItems?.Count ?? 0;

        /// <summary>
        /// The number of items currently actualised into drawables.
        /// </summary>
        public int VisibleItems => scroll.Panels.Count;

        /// <summary>
        /// The currently selected model.
        /// </summary>
        /// <remarks>
        /// Of note, if no matching item exists all items will be deselected while waiting for potential new item which matches.
        /// </remarks>
        public virtual object? CurrentSelection
        {
            get => currentSelection;
            set
            {
                if (currentSelection == value)
                    return;

                currentSelection = value;
                currentKeyboardSelection = value;

                currentKeyboardSelectionYPosition = null;

                updateSelection();
                scrollToSelection();

                // TODO: hidden values may have changed. this isn't a great way of handling.
                displayedRange = null;
            }
        }

        /// <summary>
        /// Activate the current selection, if a selection exists.
        /// </summary>
        public void ActivateSelection()
        {
            if (currentSelectionCarouselItem == null)
                return;

            if (currentSelectionCarouselItem != currentKeyboardSelectionCarouselItem)
            {
                CurrentSelection = currentKeyboardSelection;
                return;
            }

            HandleItemActivated(currentSelectionCarouselItem, scroll.Panels.SingleOrDefault(p => ((ICarouselPanel)p).Item == currentSelectionCarouselItem));
        }

        #endregion

        #region Properties and methods concerning implementations

        /// <summary>
        /// A collection of filters which should be run each time a <see cref="FilterAsync"/> is executed.
        /// </summary>
        /// <remarks>
        /// Implementations should add all required filters as part of their initialisation.
        ///
        /// Importantly, each filter is sequentially run in the order provided.
        /// Each filter receives the output of the previous filter.
        ///
        /// A filter may add, mutate or remove items.
        /// </remarks>
        protected IEnumerable<ICarouselFilter> Filters { get; init; } = Enumerable.Empty<ICarouselFilter>();

        /// <summary>
        /// All items which are to be considered for display in this carousel.
        /// Mutating this list will automatically queue a <see cref="FilterAsync"/>.
        /// </summary>
        /// <remarks>
        /// Note that an <see cref="ICarouselFilter"/> may add new items which are displayed but not tracked in this list.
        /// </remarks>
        protected readonly BindableList<T> Items = new BindableList<T>();

        /// <summary>
        /// Queue an asynchronous filter operation.
        /// </summary>
        protected virtual Task FilterAsync() => filterTask = performFilter();

        /// <summary>
        /// Create a drawable for the given carousel item so it can be displayed.
        /// </summary>
        /// <remarks>
        /// For efficiency, it is recommended the drawables are retrieved from a <see cref="DrawablePool{T}"/>.
        /// </remarks>
        /// <param name="item">The item which should be represented by the returned drawable.</param>
        /// <returns>The manifested drawable.</returns>
        protected abstract Drawable GetDrawableForDisplay(CarouselItem item);

        /// <summary>
        /// Create an internal carousel representation for the provided model object.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>A <see cref="CarouselItem"/> representing the model.</returns>
        protected abstract CarouselItem CreateCarouselItemForModel(T model);

        /// <summary>
        /// Called when an item is "activated".
        /// </summary>
        /// <remarks>
        /// An activated item should for instance:
        /// - Open or close a folder
        /// - Start gameplay on a beatmap difficulty.
        /// </remarks>
        /// <param name="item">The carousel item which was activated.</param>
        /// <param name="drawableItem">The drawable representation of the item, if manifested.</param>
        protected virtual void HandleItemActivated(CarouselItem item, Drawable? drawableItem)
        {
        }

        /// <summary>
        /// Called when an item is "deselected".
        /// </summary>
        protected virtual void HandleItemDeselected(List<CarouselItem> carouselItems, CarouselItem carouselItem)
        {
        }

        /// <summary>
        /// Called when an item is "selected".
        /// </summary>
        protected virtual void HandleItemSelected(List<CarouselItem> allItems, CarouselItem item, Drawable? drawableItem)
        {
        }

        #endregion

        #region Initialisation

        private readonly CarouselScrollContainer scroll;

        protected Carousel()
        {
            InternalChild = scroll = new CarouselScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = false,
            };

            Items.BindCollectionChanged((_, _) => FilterAsync());
        }

        #endregion

        #region Filtering and display preparation

        private List<CarouselItem>? displayedCarouselItems;

        private Task filterTask = Task.CompletedTask;
        private CancellationTokenSource cancellationSource = new CancellationTokenSource();

        private async Task performFilter()
        {
            Debug.Assert(SynchronizationContext.Current != null);

            Stopwatch stopwatch = Stopwatch.StartNew();
            var cts = new CancellationTokenSource();

            lock (this)
            {
                cancellationSource.Cancel();
                cancellationSource = cts;
            }

            if (DebounceDelay > 0)
            {
                log($"Filter operation queued, waiting for {DebounceDelay} ms debounce");
                await Task.Delay(DebounceDelay, cts.Token).ConfigureAwait(true);
            }

            // Copy must be performed on update thread for now (see ConfigureAwait above).
            // Could potentially be optimised in the future if it becomes an issue.
            IEnumerable<CarouselItem> items = new List<CarouselItem>(Items.Select(CreateCarouselItemForModel));

            await Task.Run(async () =>
            {
                try
                {
                    foreach (var filter in Filters)
                    {
                        log($"Performing {filter.GetType().ReadableName()}");
                        items = await filter.Run(items, cts.Token).ConfigureAwait(false);
                    }

                    log("Updating Y positions");
                    UpdateYPositions(items, VisibleHalfHeight, SpacingBetweenPanels);
                }
                catch (OperationCanceledException)
                {
                    log("Cancelled due to newer request arriving");
                }
            }, cts.Token).ConfigureAwait(true);

            if (cts.Token.IsCancellationRequested)
                return;

            log("Items ready for display");
            displayedCarouselItems = items.ToList();
            displayedRange = null;

            updateSelection();

            void log(string text) => Logger.Log($"Carousel[op {cts.GetHashCode().ToString()}] {stopwatch.ElapsedMilliseconds} ms: {text}");
        }

        protected static void UpdateYPositions(IEnumerable<CarouselItem> carouselItems, float offset, float spacing)
        {
            float yPos = offset;

            foreach (var item in carouselItems)
            {
                item.CarouselYPosition = yPos;

                if (!item.IsVisible)
                    continue;

                yPos += item.DrawHeight + spacing;
            }
        }

        #endregion

        #region Input handling

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Select:
                    ActivateSelection();
                    return true;

                case GlobalAction.SelectNext:
                    SelectNext(1, isGroupSelection: false);
                    return true;

                case GlobalAction.SelectNextGroup:
                    SelectNext(1, isGroupSelection: true);
                    return true;

                case GlobalAction.SelectPrevious:
                    SelectNext(-1, isGroupSelection: false);
                    return true;

                case GlobalAction.SelectPreviousGroup:
                    SelectNext(-1, isGroupSelection: true);
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        protected bool SelectNext(int direction, bool isGroupSelection)
        {
            if (displayedCarouselItems == null || displayedCarouselItems.Count == 0)
                return false;

            CarouselItem? originalItem = currentKeyboardSelectionCarouselItem;

            // To keep things simple, let's first handle the cases where there's no selection yet.
            // Once we've confirmed that the first or last isn't valid, we can handle everything
            // using the same process.
            if (originalItem == null)
            {
                originalItem = direction > 0
                    ? displayedCarouselItems.First()
                    : displayedCarouselItems.Last();

                if (isValidItem(originalItem))
                {
                    performSelection(originalItem);
                    return true;
                }
            }

            // As a special case, if the user has a different keyboard selection and requests
            // group selection, first transfer the keyboard selection.
            if (isGroupSelection && currentSelectionCarouselItem != originalItem)
            {
                CurrentSelection = originalItem.Model;
                return true;
            }

            int currentSelectionIndex = displayedCarouselItems.IndexOf(originalItem);
            int newSelectionIndex = currentSelectionIndex;

            // As a second special case, if we're group selecting backwards and the current selection isn't
            // a group, base this selection operation from the closest previous group.
            if (isGroupSelection && direction < 0)
            {
                while (!displayedCarouselItems[newSelectionIndex].IsGroupSelectionTarget)
                    newSelectionIndex--;
            }

            Debug.Assert(currentSelectionIndex >= 0);

            CarouselItem newItem;

            while ((newItem = selectNextPanel()) != originalItem)
            {
                if (isValidItem(newItem))
                    break;
            }

            if (newItem == originalItem)
                return false;

            performSelection(newItem);
            return true;

            CarouselItem selectNextPanel()
            {
                newSelectionIndex += direction;
                return newItem = displayedCarouselItems[(newSelectionIndex + displayedCarouselItems.Count) % displayedCarouselItems.Count];
            }

            void performSelection(CarouselItem item)
            {
                if (isGroupSelection)
                {
                    CurrentSelection = item.Model;
                }
                else
                {
                    currentKeyboardSelection = item.Model;
                    updateSelection();
                }
            }

            bool isValidItem(CarouselItem item) => item.IsVisible && (!isGroupSelection || item.IsGroupSelectionTarget);
        }

        #endregion

        #region Selection handling

        private object? currentKeyboardSelection;
        private CarouselItem? currentKeyboardSelectionCarouselItem;
        private double? currentKeyboardSelectionYPosition;

        private object? currentSelection;
        private CarouselItem? currentSelectionCarouselItem;

        private void updateSelection()
        {
            if (displayedCarouselItems == null)
            {
                currentSelectionCarouselItem = null;
                currentKeyboardSelectionCarouselItem = null;
                return;
            }

            foreach (var item in displayedCarouselItems)
            {
                bool isSelected = item.Model == currentSelection;
                bool isKeyboardSelected = item.Model == currentKeyboardSelection;

                if (isKeyboardSelected)
                {
                    currentKeyboardSelectionCarouselItem = item;

                    if (currentKeyboardSelectionYPosition != item.CarouselYPosition)
                    {
                        if (currentKeyboardSelectionYPosition != null)
                        {
                            float adjustment = (float)(item.CarouselYPosition - currentKeyboardSelectionYPosition.Value);
                            scroll.OffsetScrollPosition(adjustment);
                        }

                        currentKeyboardSelectionYPosition = item.CarouselYPosition;
                    }
                }

                if (isSelected)
                {
                    if (item != currentSelectionCarouselItem)
                    {
                        if (currentSelectionCarouselItem != null)
                            HandleItemDeselected(displayedCarouselItems, currentSelectionCarouselItem);
                        currentSelectionCarouselItem = item;
                        HandleItemSelected(displayedCarouselItems, item, scroll.Panels.SingleOrDefault(p => ((ICarouselPanel)p).Item == currentSelectionCarouselItem));
                    }
                }
            }
        }

        private void scrollToSelection()
        {
            if (currentKeyboardSelectionYPosition != null)
                scroll.ScrollTo(currentKeyboardSelectionYPosition.Value - VisibleHalfHeight);
        }

        #endregion

        #region Display handling

        private DisplayRange? displayedRange;

        private readonly CarouselItem carouselBoundsItem = new CarouselItem(new object());

        /// <summary>
        /// The position of the lower visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleBottomBound => (float)(scroll.Current + DrawHeight + BleedBottom);

        /// <summary>
        /// The position of the upper visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleUpperBound => (float)(scroll.Current - BleedTop);

        /// <summary>
        /// Half the height of the visible content.
        /// </summary>
        protected float VisibleHalfHeight => (DrawHeight + BleedBottom + BleedTop) / 2;

        protected override void Update()
        {
            base.Update();

            if (displayedCarouselItems == null)
                return;

            var range = getDisplayRange();

            if (range != displayedRange)
            {
                Logger.Log($"Updating displayed range of carousel from {displayedRange} to {range}");
                displayedRange = range;

                updateDisplayedRange(range);
            }

            foreach (var panel in scroll.Panels)
            {
                var c = (ICarouselPanel)panel;

                if (panel.Depth != c.DrawYPosition)
                    scroll.Panels.ChangeChildDepth(panel, (float)c.DrawYPosition);

                Debug.Assert(c.Item != null);

                if (c.DrawYPosition != c.Item.CarouselYPosition)
                    c.DrawYPosition = Interpolation.DampContinuously(c.DrawYPosition, c.Item.CarouselYPosition, 50, Time.Elapsed);

                Vector2 posInScroll = scroll.ToLocalSpace(panel.ScreenSpaceDrawQuad.Centre);
                float dist = Math.Abs(1f - posInScroll.Y / VisibleHalfHeight);

                panel.X = offsetX(dist, VisibleHalfHeight);

                c.Selected.Value = c.Item == currentSelectionCarouselItem;
                c.KeyboardSelected.Value = c.Item == currentKeyboardSelectionCarouselItem;
            }
        }

        /// <summary>
        /// Computes the x-offset of currently visible items. Makes the carousel appear round.
        /// </summary>
        /// <param name="dist">
        /// Vertical distance from the center of the carousel container
        /// ranging from -1 to 1.
        /// </param>
        /// <param name="halfHeight">Half the height of the carousel container.</param>
        private static float offsetX(float dist, float halfHeight)
        {
            // The radius of the circle the carousel moves on.
            const float circle_radius = 3;
            float discriminant = MathF.Max(0, circle_radius * circle_radius - dist * dist);
            return (circle_radius - MathF.Sqrt(discriminant)) * halfHeight;
        }

        private DisplayRange getDisplayRange()
        {
            Debug.Assert(displayedCarouselItems != null);

            // Find index range of all items that should be on-screen
            carouselBoundsItem.CarouselYPosition = visibleUpperBound - DistanceOffscreenToPreload;
            int firstIndex = displayedCarouselItems.BinarySearch(carouselBoundsItem);
            if (firstIndex < 0) firstIndex = ~firstIndex;

            carouselBoundsItem.CarouselYPosition = visibleBottomBound + DistanceOffscreenToPreload;
            int lastIndex = displayedCarouselItems.BinarySearch(carouselBoundsItem);
            if (lastIndex < 0) lastIndex = ~lastIndex;

            firstIndex = Math.Max(0, firstIndex - 1);
            lastIndex = Math.Max(0, lastIndex - 1);

            return new DisplayRange(firstIndex, lastIndex);
        }

        private void updateDisplayedRange(DisplayRange range)
        {
            Debug.Assert(displayedCarouselItems != null);

            List<CarouselItem> toDisplay = range.Last - range.First == 0
                ? new List<CarouselItem>()
                : displayedCarouselItems.GetRange(range.First, range.Last - range.First + 1);

            // TODO: make more efficient
            toDisplay.RemoveAll(i => !i.IsVisible);

            // Iterate over all panels which are already displayed and figure which need to be displayed / removed.
            foreach (var panel in scroll.Panels)
            {
                var carouselPanel = (ICarouselPanel)panel;

                // The case where we're intending to display this panel, but it's already displayed.
                // Note that we **must compare the model here** as the CarouselItems may be fresh instances due to a filter operation.
                var existing = toDisplay.FirstOrDefault(i => i.Model == carouselPanel.Item!.Model);

                if (existing != null)
                {
                    carouselPanel.Item = existing;
                    toDisplay.Remove(existing);
                    continue;
                }

                // If the new display range doesn't contain the panel, it's no longer required for display.
                expirePanelImmediately(panel);
            }

            // Add any new items which need to be displayed and haven't yet.
            foreach (var item in toDisplay)
            {
                var drawable = GetDrawableForDisplay(item);

                if (drawable is not ICarouselPanel carouselPanel)
                    throw new InvalidOperationException($"Carousel panel drawables must implement {typeof(ICarouselPanel)}");

                carouselPanel.Item = item;
                scroll.Add(drawable);
            }

            // Update the total height of all items (to make the scroll container scrollable through the full height even though
            // most items are not displayed / loaded).
            if (displayedCarouselItems.Count > 0)
            {
                var lastItem = displayedCarouselItems[^1];
                scroll.SetLayoutHeight((float)(lastItem.CarouselYPosition + lastItem.DrawHeight + VisibleHalfHeight));
            }
            else
                scroll.SetLayoutHeight(0);
        }

        private static void expirePanelImmediately(Drawable panel)
        {
            panel.FinishTransforms();
            panel.Expire();
        }

        #endregion

        #region Internal helper classes

        private record DisplayRange(int First, int Last);

        /// <summary>
        /// Implementation of scroll container which handles very large vertical lists by internally using <c>double</c> precision
        /// for pre-display Y values.
        /// </summary>
        private partial class CarouselScrollContainer : UserTrackingScrollContainer, IKeyBindingHandler<GlobalAction>
        {
            public readonly Container Panels;

            public void SetLayoutHeight(float height) => Panels.Height = height;

            public CarouselScrollContainer()
            {
                // Managing our own custom layout within ScrollContent causes feedback with public ScrollContainer calculations,
                // so we must maintain one level of separation from ScrollContent.
                base.Add(Panels = new Container
                {
                    Name = "Layout content",
                    RelativeSizeAxes = Axes.X,
                });
            }

            public override void OffsetScrollPosition(double offset)
            {
                base.OffsetScrollPosition(offset);

                foreach (var panel in Panels)
                {
                    var c = (ICarouselPanel)panel;
                    Debug.Assert(c.Item != null);

                    c.DrawYPosition += offset;
                }
            }

            public override void Clear(bool disposeChildren)
            {
                Panels.Height = 0;
                Panels.Clear(disposeChildren);
            }

            public override void Add(Drawable drawable)
            {
                if (drawable is not ICarouselPanel)
                    throw new InvalidOperationException($"Carousel panel drawables must implement {typeof(ICarouselPanel)}");

                Panels.Add(drawable);
            }

            public override double GetChildPosInContent(Drawable d, Vector2 offset)
            {
                if (d is not ICarouselPanel panel)
                    return base.GetChildPosInContent(d, offset);

                return panel.DrawYPosition + offset.X;
            }

            protected override void ApplyCurrentToContent()
            {
                Debug.Assert(ScrollDirection == Direction.Vertical);

                double scrollableExtent = -Current + ScrollableExtent * ScrollContent.RelativeAnchorPosition.Y;

                foreach (var d in Panels)
                    d.Y = (float)(((ICarouselPanel)d).DrawYPosition + scrollableExtent);
            }

            #region Absolute scrolling

            private bool absoluteScrolling;

            protected override bool IsDragging => base.IsDragging || absoluteScrolling;

            public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                switch (e.Action)
                {
                    case GlobalAction.AbsoluteScrollSongList:
                        beginAbsoluteScrolling(e);
                        return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
            {
                switch (e.Action)
                {
                    case GlobalAction.AbsoluteScrollSongList:
                        endAbsoluteScrolling();
                        break;
                }
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (e.Button == MouseButton.Right)
                {
                    // To avoid conflicts with context menus, disallow absolute scroll if it looks like things will fall over.
                    if (GetContainingInputManager()!.HoveredDrawables.OfType<IHasContextMenu>().Any())
                        return false;

                    beginAbsoluteScrolling(e);
                }

                return base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (e.Button == MouseButton.Right)
                    endAbsoluteScrolling();
                base.OnMouseUp(e);
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                if (absoluteScrolling)
                {
                    ScrollToAbsolutePosition(e.CurrentState.Mouse.Position);
                    return true;
                }

                return base.OnMouseMove(e);
            }

            private void beginAbsoluteScrolling(UIEvent e)
            {
                ScrollToAbsolutePosition(e.CurrentState.Mouse.Position);
                absoluteScrolling = true;
            }

            private void endAbsoluteScrolling() => absoluteScrolling = false;

            #endregion
        }

        #endregion
    }
}
