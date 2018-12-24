using Enivate.ResponseHub.Responsys.UI.Model;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Responsys.UI.Services
{
    public class PrintedMessageRecordService : BaseLiteDbService
    {


        private const string PrintedMessageRecordCollectionName = "printed_message_records";

        /// <summary>
        /// Gets the print record by the job id.
        /// </summary>
        /// <param name="jobId">The ID of the job to get the print record for.</param>
        /// <returns>The print record for the job.</returns>
        public MessagePrintRecord GetPrintRecordByJobId(Guid jobId)
        {
            using (LiteDatabase db = new LiteDatabase(ConnectionString))
            {
                // Get a collection (or create, if doesn't exist)
                LiteCollection<MessagePrintRecord> collection = db.GetCollection<MessagePrintRecord>(PrintedMessageRecordCollectionName);

                // Get all current briefs
                MessagePrintRecord result = collection.FindOne(i => i.JobMessageId == jobId);

                // return the results
                return result;
            }
        }

        /// <summary>
        /// Determines if any print records exist for the specified job id.
        /// </summary>
        /// <param name="jobId">The ID of the job to check if print records exist for.</param>
        /// <returns>True if print records exist, otherwise false.</returns>
        public bool PrintRecordExistsForJob(Guid jobId)
        {

            using (LiteDatabase db = new LiteDatabase(ConnectionString))
            {
                // Get a collection (or create, if doesn't exist)
                LiteCollection<MessagePrintRecord> collection = db.GetCollection<MessagePrintRecord>(PrintedMessageRecordCollectionName);

                // Get all current briefs
                int count = collection.Count(i => i.JobMessageId == jobId);

                // return the results
                return count > 0;
            }
        }

        /// <summary>
        /// Creates the print record in the database.
        /// </summary>
        /// <param name="jobId">The ID of the job to create the print record for.</param>
        /// <param name="timestamp">The timestamp it was printed.</param>
        /// <returns>The new print record object.</returns>
        public MessagePrintRecord CreatePrintRecord(Guid jobId, DateTime timestamp)
        {
            using (LiteDatabase db = new LiteDatabase(ConnectionString))
            {
                // Get a collection (or create, if doesn't exist)
                LiteCollection<MessagePrintRecord> collection = db.GetCollection<MessagePrintRecord>(PrintedMessageRecordCollectionName);

                // Get all current briefs
                MessagePrintRecord newPrintRecord = new MessagePrintRecord()
                {
                    JobMessageId = jobId,
                    DatePrinted = timestamp
                };

                collection.Upsert(newPrintRecord);

                // return the results
                return newPrintRecord;
            }
        }

    }
}
