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

        /// <summary>
        /// Gets a value indicating whether gets whether this transfer used RoboSharp.
        /// </summary>
        [BsonIgnore]
        public bool IsRoboSharpTransfer => Summary.TransferMethod == "RoboSharp";

        /// <summary>
        /// Gets a value indicating whether gets whether the transfer had errors.
        /// </summary>
        [BsonIgnore]
        public bool HasErrors => Summary.Errors?.Count > 0;

        /// <summary>
        /// Gets the first error message if any.
        /// </summary>
        [BsonIgnore]
        public string? FirstError => Summary.Errors?.Count > 0 ? Summary.Errors[0] : null;
    }
}