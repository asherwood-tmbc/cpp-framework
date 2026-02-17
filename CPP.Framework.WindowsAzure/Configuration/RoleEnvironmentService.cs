using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace CPP.Framework.Configuration
{
    /// <summary>
    /// Provides abstracted access to the static properties, events, and methods of the Windows
    /// Azure <see cref="RoleEnvironment"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RoleEnvironmentService : SingletonServiceBase, IConfiguarationManagerService
    {
        private static readonly ServiceInstance<RoleEnvironmentService> _ServiceInstance = new ServiceInstance<RoleEnvironmentService>(); 

        /// <summary>
        /// Occurs after a change to the service configuration is applied to the running instances 
        /// of a role.
        /// </summary>
        public event EventHandler<RoleEnvironmentChangedEventArgs> Changed;

        /// <summary>
        /// Occurs before a change to the service configuration is applied to the running instances 
        /// of a role.
        /// </summary>
        public event EventHandler<RoleEnvironmentChangingEventArgs> Changing;

        /// <summary>
        /// Occurs after a simultaneous change to the service configuration has been applied to the 
        /// running instances of a role. A simultaneous change affects all role instances at the 
        /// same time.
        /// </summary>
        public event EventHandler<SimultaneousChangedEventArgs> SimultaneousChanged;

        /// <summary>
        /// Occurs before a simultaneous change to the service configuration is applied to the 
        /// running instances of a role. A simultaneous change affects all role instances at the 
        /// same time.
        /// </summary>
        public event EventHandler<SimultaneousChangingEventArgs> SimultaneousChanging;

        /// <summary>
        /// Occurs at a regular interval to indicate the status of a role instance.
        /// </summary>
        public event EventHandler<RoleInstanceStatusCheckEventArgs> StatusCheck;

        /// <summary>
        /// Occurs when a role instance is about to be stopped.
        /// </summary>
        public event EventHandler<RoleEnvironmentStoppingEventArgs> Stopping;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        protected RoleEnvironmentService() { }

        /// <summary>
        /// Gets the current instance of the <see cref="RoleEnvironmentService"/> for the 
        /// application.
        /// </summary>
        public static RoleEnvironmentService Current { get { return _ServiceInstance.GetInstance(); } }

        /// <summary>
        /// Gets a RoleInstance object that represents the role instance in which the code is 
        /// currently running.
        /// </summary>
        public virtual RoleInstance CurrentRoleInstance { get { return RoleEnvironment.CurrentRoleInstance; } }

        /// <summary>
        /// Gets the unique identifier of the deployment in which the role instance is running.
        /// </summary>
        public virtual string DeloymentId { get { return RoleEnvironment.DeploymentId; } }

        /// <summary>
        /// Indicates whether the role instance is running in the Windows Azure environment.
        /// </summary>
        public virtual bool IsAvailable { get { return RoleEnvironment.IsAvailable; } }

        /// <summary>
        /// Indicates whether the role instance is running in the Windows Azure compute emulator.
        /// </summary>
        public virtual bool IsEmulated { get { return RoleEnvironment.IsEmulated; } }

        /// <summary>
        /// Indicates whether or not the Windows Azure storage emulator is active and running.
        /// </summary>
        public virtual bool IsAzureStorageEmulatorActive
        {
            get
            {
                try
                {
#if DEBUG
                    // attempt to connect to the local dev account (which is implemented by the storage
                    // emulator), and try to create/check for a special queue.
                    var account = CloudStorageAccount.DevelopmentStorageAccount;
                    var queue = account.CreateCloudQueueClient().GetQueueReference("ping");

                    if (queue != null)
                    {
                        var options = new QueueRequestOptions()
                        {
                            ServerTimeout = TimeSpan.FromMilliseconds(2500),
                            RetryPolicy = new NoRetry(),
                        };
                        queue.CreateIfNotExists(options);
                    }
                    return ((queue != null) && (queue.Exists()));   // if we get to this point, then the connection is valid.
#else
                return false;
#endif
                }
                catch (StorageException) { return false; }
            }
        }

        /// <summary>
        /// Gets the set of Role objects defined for the hosted service.
        /// </summary>
        public virtual IDictionary<string, Role> Roles { get { return RoleEnvironment.Roles; } }

        /// <summary>
        /// Gets the <see cref="TraceSource"/> for the role instance.
        /// </summary>
        public virtual TraceSource TraceSource { get { return RoleEnvironment.TraceSource; } }

        #region RoleEnvironment Event Delegates

        private void RoleChanging(object sender, RoleEnvironmentChangingEventArgs args) { this.OnChanging(sender, args); }

        private void RoleChanged(object sender, RoleEnvironmentChangedEventArgs args) { this.OnChanged(sender, args); }

        private void RoleSimultaneousChanging(object sender, SimultaneousChangingEventArgs args) { this.OnSimultaneousChanging(sender, args); }

        private void RoleSimultaneousChanged(object sender, SimultaneousChangedEventArgs args) { this.OnSimultaneousChanged(sender, args); }

        private void RoleStatusCheck(object sender, RoleInstanceStatusCheckEventArgs args) { this.OnStatusCheck(sender, args); }

        private void RoleStopping(object sender, RoleEnvironmentStoppingEventArgs args) { this.OnStopping(sender, args); }

        #endregion // RoleEnvironment Event Delegates

        /// <summary>
        /// Called by the base class to cleanup the current instance prior to it being destroyed.
        /// </summary>
        protected override void CleanupInstance()
        {
            RoleEnvironment.Changed -= this.RoleChanged;
            RoleEnvironment.Changing -= this.RoleChanging;
            RoleEnvironment.SimultaneousChanged -= this.RoleSimultaneousChanged;
            RoleEnvironment.SimultaneousChanging -= this.RoleSimultaneousChanging;
            RoleEnvironment.StatusCheck -= this.RoleStatusCheck;
            RoleEnvironment.Stopping -= this.RoleStopping;
            base.CleanupInstance();
        }

        /// <summary>
        /// Retrieves the value of a setting in the service configuration file.
        /// </summary>
        /// <param name="configurationSettingName">The name of the configuration setting.</param>
        /// <returns>A String that contains the value of the configuration setting.</returns>
        /// <exception cref="RoleEnvironmentException">The configuration setting that was being retrieved does not exist.</exception>
        public virtual string GetConfigurationSettingValue(string configurationSettingName)
        {
            return RoleEnvironment.GetConfigurationSettingValue(configurationSettingName);
        }

        /// <summary>
        /// Retrieves a specified local storage resource.
        /// </summary>
        /// <param name="localResourceName">The name of the local storage resource that is defined in the ServiceDefiniton.csdef file.</param>
        /// <returns>An instance of <see cref="LocalResource"/> that represents the local storage resource.</returns>
        /// <exception cref="RoleEnvironmentException">The local storage resource does not exist.</exception>
        public virtual LocalResource GetLocalResource(string localResourceName)
        {
            return RoleEnvironment.GetLocalResource(localResourceName);
        }

        /// <summary>
        /// Raises the <see cref="Changed"/> event.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">A <see cref="RoleEnvironmentChangedEventArgs"/> object that contains more information about the event.</param>
        protected virtual void OnChanged(object sender, RoleEnvironmentChangedEventArgs args)
        {
            var handler = this.Changed;
            if (handler != null) handler(sender, args);
        }

        /// <summary>
        /// Raises the <see cref="Changing"/> event.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">A <see cref="RoleEnvironmentChangingEventArgs"/> object that contains more information about the event.</param>
        protected virtual void OnChanging(object sender, RoleEnvironmentChangingEventArgs args)
        {
            var handler = this.Changing;
            if (handler != null) handler(sender, args);
        }

        /// <summary>
        /// Raises the <see cref="SimultaneousChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">A <see cref="SimultaneousChangedEventArgs"/> object that contains more information about the event.</param>
        protected virtual void OnSimultaneousChanged(object sender, SimultaneousChangedEventArgs args)
        {
            var handler = this.SimultaneousChanged;
            if (handler != null) handler(sender, args);
        }

        /// <summary>
        /// Raises the <see cref="SimultaneousChanging"/> event.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">A <see cref="SimultaneousChangingEventArgs"/> object that contains more information about the event.</param>
        protected virtual void OnSimultaneousChanging(object sender, SimultaneousChangingEventArgs args)
        {
            var handler = this.SimultaneousChanging;
            if (handler != null) handler(sender, args);
        }

        /// <summary>
        /// Raises the <see cref="StatusCheck"/> event.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">A <see cref="RoleInstanceStatusCheckEventArgs"/> object that contains more information about the event.</param>
        protected virtual void OnStatusCheck(object sender, RoleInstanceStatusCheckEventArgs args)
        {
            var handler = this.StatusCheck;
            if (handler != null) handler(sender, args);
        }

        /// <summary>
        /// Raises the <see cref="Stopping"/> event.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">A <see cref="RoleEnvironmentStoppingEventArgs"/> object that contains more information about the event.</param>
        protected virtual void OnStopping(object sender, RoleEnvironmentStoppingEventArgs args)
        {
            var handler = this.Stopping;
            if (handler != null) handler(sender, args);
        }

        /// <summary>
        /// Requests that the current role instance be stopped and restarted.
        /// </summary>
        public virtual void RequestRecycle() { RoleEnvironment.RequestRecycle(); }

        /// <summary>
        /// Called by the base class to perform any initialization tasks when the instance is being
        /// created.
        /// </summary>
        protected override void StartupInstance()
        {
            RoleEnvironment.Changed += this.RoleChanged;
            RoleEnvironment.Changing += this.RoleChanging;
            RoleEnvironment.SimultaneousChanged += this.RoleSimultaneousChanged;
            RoleEnvironment.SimultaneousChanging += this.RoleSimultaneousChanging;
            RoleEnvironment.StatusCheck += this.RoleStatusCheck;
            RoleEnvironment.Stopping += this.RoleStopping;
            base.StartupInstance();
        }
    }
}
