using System.Threading.Tasks;

namespace Specflow.BackgroundOnce
{
    /// <summary>
    /// Represents an object where data can be restored to a snapshot.
    /// </summary>
    public interface ISnapshotData
    {
        /// <summary>
        /// Background data has been created, a snapshot should be created which can then be used in following scenarios in this feature.
        /// The data should be applicable to the Feature, use the FeatureContext as a means to store data, or a reference to the data as appropriate.
        /// </summary>
        /// <returns></returns>
        Task CreateSnapshot();

        /// <summary>
        /// The data store should be restored to the state recorded when <see cref="CreateSnapshot"/> was called.
        /// </summary>
        /// <returns></returns>
        Task RestoreSnapshot();
    }
}