// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Models;
using Realms;

namespace osu.Game.Stores
{
    /// <summary>
    /// Class which adds all the missing pieces bridging the gap between <see cref="RealmArchiveModelImporter{TModel}"/> and <see cref="ArchiveModelManager{TModel,TFileModel}"/>.
    /// </summary>
    public abstract class RealmArchiveModelManager<TModel> : RealmArchiveModelImporter<TModel>, IModelManager<TModel>, IModelFileManager<TModel, RealmNamedFileUsage>
        where TModel : RealmObject, IHasRealmFiles, IHasGuidPrimaryKey, ISoftDelete
    {
        protected RealmArchiveModelManager([NotNull] Storage storage, [NotNull] RealmContextFactory contextFactory)
            : base(storage, contextFactory)
        {
        }

        /// <summary>
        /// Perform a lookup query on available models.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public ILive<TModel> Query(Expression<Func<TModel, bool>> query)
        {
            using (var realm = ContextFactory.CreateContext())
                return realm.All<TModel>().FirstOrDefault(query)?.ToLive();
        }

        public List<ILive<TModel>> QueryAll(Expression<Func<TModel, bool>> query)
        {
            using (var realm = ContextFactory.CreateContext())
                return realm.All<TModel>().Where(query).ToLive();
        }

        public void DeleteFile(TModel item, RealmNamedFileUsage singleOrDefault)
        {
        }

        public void ReplaceFile(TModel model, RealmNamedFileUsage file, Stream contents, string filename = null)
        {
        }

        public void AddFile(TModel item, Stream stream, string skinIni)
        {
        }

        public bool Delete(TModel skin)
        {
            return false;
        }

        public void Delete(List<TModel> items, bool silent = false)
        {
        }

        public void Undelete(List<TModel> items, bool silent = false)
        {
        }

        public void Undelete(TModel item)
        {
            // make compile LOL
            ItemRemoved?.Invoke(null);
            ItemUpdated?.Invoke(null);
        }

        public bool IsAvailableLocally(TModel model)
        {
            return false;
        }

        public event Action<TModel> ItemUpdated;
        public event Action<TModel> ItemRemoved;

        public void Update(TModel skin)
        {
        }
    }
}
