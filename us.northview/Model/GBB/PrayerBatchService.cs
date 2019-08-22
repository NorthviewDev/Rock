using Rock.Data;
using System;
using System.Linq;

namespace us.northviewchurch.Model.GBB
{
    public class PrayerBatchService : Service<PrayerBatch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrayerBatchService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public PrayerBatchService(RockContext context) : base(context) { }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete(PrayerBatch item, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }

        public int? CreateNewBatch()
        {
            int? newId = null;

            try
            {
                var newBatch = new PrayerBatch()
                {
                    Active = true
                };

                this.Add(newBatch);
                this.Context.SaveChanges();

                newId = newBatch.Id;
            }
            catch (Exception e)
            {
                //Log?
                //throw e;
                Console.Error.Write(e);
            }


            return newId;
        }

        public PrayerBatch GetActiveBatch()
        {
            PrayerBatch batch = null;

            try
            {
                var list = this.Queryable().Where(x => x.Active).OrderByDescending(x => x.CreatedDateTime).ToList();

                batch = list.FirstOrDefault();
            }
            catch (Exception e)
            {
                //Log?
                //throw e;
                Console.Error.Write(e);
            }

            return batch;
        }

        public int? GetActiveBatchId()
        {
            int? id = null;
            var batch = GetActiveBatch();

            if(batch != null)
            {
                id = batch.Id;
            }

            return id;
        }
    }
}
