using System.Collections.Generic;
using LiteDB;

namespace DataTransferApp.Net.Models
{
    public class TransferLog
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();

        public TransferInfo TransferInfo { get; set; } = new();

        public IList<TransferredFile> Files { get; set; } = new List<TransferredFile>();

        public TransferSummary Summary { get; set; } = new();
    }
}