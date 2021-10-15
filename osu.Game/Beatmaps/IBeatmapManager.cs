// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    public interface IBeatmapManager<TModel> : IModelDownloader<TModel>, IModelManager<TModel>, IModelFileManager<TModel, BeatmapSetFileInfo>, IWorkingBeatmapCache, IDisposable, IPostImports<TModel> where TModel : class

    {
        /// <summary>
        /// Returns a list of all usable <typeparamref name="TModel"/>s.
        /// </summary>
        /// <returns>A list of available <typeparamref name="TModel"/>.</returns>
        List<TModel> GetAllUsableBeatmapSets(IncludedDetails includes = IncludedDetails.All, bool includeProtected = false);

        /// <summary>
        /// Returns a list of all usable <typeparamref name="TModel"/>s. Note that files are not populated.
        /// </summary>
        /// <param name="includes">The level of detail to include in the returned objects.</param>
        /// <param name="includeProtected">Whether to include protected (system) beatmaps. These should not be included for gameplay playable use cases.</param>
        /// <returns>A list of available <typeparamref name="TModel"/>.</returns>
        IEnumerable<TModel> GetAllUsableBeatmapSetsEnumerable(IncludedDetails includes, bool includeProtected = false);

        /// <summary>
        /// Perform a lookup query on available <typeparamref name="TModel"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="includes">The level of detail to include in the returned objects.</param>
        /// <returns>Results from the provided query.</returns>
        IEnumerable<TModel> QueryBeatmapSets(Expression<Func<TModel, bool>> query, IncludedDetails includes = IncludedDetails.All);

        /// <summary>
        /// Perform a lookup query on available <typeparamref name="TModel"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        TModel QueryBeatmapSet(Expression<Func<TModel, bool>> query);

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Results from the provided query.</returns>
        IQueryable<BeatmapInfo> QueryBeatmaps(Expression<Func<BeatmapInfo, bool>> query);

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        BeatmapInfo QueryBeatmap(Expression<Func<BeatmapInfo, bool>> query);

        /// <summary>
        /// Delete a beatmap difficulty.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap difficulty to hide.</param>
        void Hide(BeatmapInfo beatmapInfo);

        /// <summary>
        /// Restore a beatmap difficulty.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap difficulty to restore.</param>
        void Restore(BeatmapInfo beatmapInfo);
    }
}
