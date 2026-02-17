using System.Linq;

using CPP.Framework.Services;

using JetBrains.Annotations;

namespace CPP.Framework.Data
{
    /// <summary>
    /// Abstract base class for all objects used to filter entity data for a data source context.
    /// </summary>
    /// <typeparam name="TState">A class to track entity state.</typeparam>
    [AutoRegisterService]
    [UsedImplicitly]
    public abstract class DataSourceFilter<TState> : CodeServiceSingleton
    {
        /// <summary>
        /// Notifies the filter that a new query is being created.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query that was created by the provider.</param>
        /// <returns>
        /// The <see cref="IQueryable{T}"/> object to return from the context.
        /// </returns>
        [UsedImplicitly]
        internal IQueryable<TEntity> CreateQuery<TEntity>(IQueryable<TEntity> query)
            where TEntity : class
        {
            return this.OnCreateQuery(query);
        }

        /// <summary>
        /// Notifies the filter that an entity object has been materialized from the underlying
        /// data source, or has been saved to the underlying data source.
        /// </summary>
        /// <param name="entity">The entity object that was materialized.</param>
        /// <param name="state">The current change state of <paramref name="entity"/>.</param>
        /// <returns>
        /// The initial change state to set for <paramref name="entity"/>, which should normally be
        /// EntityState.Unchanged.
        /// </returns>
        [UsedImplicitly]
        internal TState Materialize(object entity, TState state) => this.OnMaterialize(entity, state);

        /// <summary>
        /// Called by the framework when a new query is being created.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query that was created by the provider.</param>
        /// <returns>
        /// The <see cref="IQueryable{T}"/> object to return from the context.
        /// </returns>
        protected virtual IQueryable<TEntity> OnCreateQuery<TEntity>(IQueryable<TEntity> query)
            where TEntity : class
        {
            return query;
        }

        /// <summary>
        /// Called by the framework after an entity object has been 
        /// materialized from the underlying data source, or after an object has been saved to the 
        /// underlying data source.
        /// </summary>
        /// <param name="entity">The entity object that was materialized.</param>
        /// <param name="state">The current change state of <paramref name="entity"/>.</param>
        /// <returns>
        /// The initial change state to set for <paramref name="entity"/>, which should normally be
        /// EntityState.Unchanged.
        /// </returns>
        protected virtual TState OnMaterialize(object entity, TState state) => state;

        /// <summary>
        /// Called by the framework just before changes to an entity object are saved to the 
        /// underlying data source.
        /// </summary>
        /// <param name="entity">The object being saved.</param>
        /// <param name="changeState">The current change state of the object.</param>
        protected virtual void OnSaveChanges(object entity, TState changeState) { }

        /// <summary>
        /// Notifies the filter that changes to an entity object are being saved to the underlying 
        /// data source.
        /// </summary>
        /// <param name="entity">The object being saved.</param>
        /// <param name="state">The current change state of <paramref name="entity"/>.</param>
        [UsedImplicitly]
        internal void SaveChanges(object entity, TState state) => this.OnSaveChanges(entity, state);
    }
}
