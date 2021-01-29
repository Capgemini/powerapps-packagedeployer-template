namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System.Collections.Generic;

    /// <summary>
    /// A service which is responsible for setting the state of components.
    /// </summary>
    public interface IComponentStateSettingService
    {
        /// <summary>
        /// Sets the state of solution components. Activates solution components by default unless in the collection of processes to deactivate.
        /// </summary>
        /// <param name="solutions">The solutions to activate components within.</param>
        /// <param name="componentsToDeactivate">The components to deactivate rather than activate.</param>
        void SetStatesBySolution(IEnumerable<string> solutions, IEnumerable<string> componentsToDeactivate = null);

        /// <summary>
        /// Sets the state of components.
        /// </summary>
        /// <param name="componentsToActivate">The components to activate.</param>
        /// <param name="componentsToDeactivate">The components to deactivate.</param>
        void SetStates(IEnumerable<string> componentsToActivate, IEnumerable<string> componentsToDeactivate = null);
    }
}