namespace DataTransferApp.Net.Models
{
    /// <summary>
    /// Represents the user's choice for handling existing drive contents.
    /// </summary>
    public enum DriveContentAction
    {
        /// <summary>
        /// Append new folders to existing contents.
        /// </summary>
        Append,

        /// <summary>
        /// Clear drive before transfer.
        /// </summary>
        Clear,

        /// <summary>
        /// Cancel the transfer operation.
        /// </summary>
        Cancel
    }
}