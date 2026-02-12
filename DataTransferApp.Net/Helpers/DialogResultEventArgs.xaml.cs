namespace DataTransferApp.Net.Helpers
{
    /// <summary>
    /// Custom EventArgs for dialog results.
    /// </summary>
    public class DialogResultEventArgs : EventArgs
    {
        public bool Result { get; }

        public DialogResultEventArgs(bool result)
        {
            Result = result;
        }
    }
}