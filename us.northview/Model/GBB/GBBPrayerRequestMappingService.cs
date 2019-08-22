using Rock.Data;

namespace us.northviewchurch.Model.GBB
{
    public class GBBPrayerRequestMappingService : Service<GBBPrayerRequestMapping>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GBBPrayerRequestMappingService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public GBBPrayerRequestMappingService(RockContext context) : base(context) { }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete(GBBPrayerRequestMapping item, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}
