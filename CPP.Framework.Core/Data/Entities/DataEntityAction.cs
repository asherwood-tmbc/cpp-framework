namespace CPP.Framework.Data.Entities
{
    /// <summary>
    /// Abstract base class for late-bound actions that are executed directly against a specific 
    /// data entity.
    /// </summary>
    public abstract class DataEntityAction : DataSourceAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntityAction"/> class.
        /// </summary>
        /// <param name="name">The name of the action to execute.</param>
        /// <param name="targetEntity">The target entity for the action.</param>
        /// <param name="singleResult">True if the action produces a single return value; otherwise, false. This value is ignored for actions that do not return a value.</param>
        protected DataEntityAction(string name, IDataEntity targetEntity, bool singleResult) : base(name, singleResult)
        {
            ArgumentValidator.ValidateNotNull(() => targetEntity);
            this.TargetEntity = targetEntity;
        }

        /// <summary>
        /// Gets the target entity for the action.
        /// </summary>
        public IDataEntity TargetEntity { get; }
    }
}
