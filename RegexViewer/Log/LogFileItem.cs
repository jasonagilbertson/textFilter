namespace RegexViewer
{
    public class LogFileItem : FileItem
    {
        #region Public Properties

        public int FilterIndex { get; set; }

        public string Group1 { get; set; }

        public string Group2 { get; set; }

        public string Group3 { get; set; }

        public string Group4 { get; set; }

        public int[,] Masked { get; set; }

        #endregion Public Properties
    }
}