using System;

namespace RegexViewer
{
    internal class Worker
    {
        #region Public Fields

        public string CompareString;

        public string SourceFile;

        #endregion Public Fields

        #region Private Fields

        private int LinesCounted;

        private int WordCount;

        #endregion Private Fields

        #region Public Methods

        public void CountWords(
                        System.ComponentModel.BackgroundWorker worker,
                        System.ComponentModel.DoWorkEventArgs e)
        {
            // Initialize the variables.
            CurrentState state = new CurrentState();
            string line = "";
            int elapsedTime = 20;
            DateTime lastReportDateTime = DateTime.Now;

            if (CompareString == null ||
                CompareString == System.String.Empty)
            {
                throw new Exception("CompareString not specified.");
            }

            // Open a new stream.
            using (System.IO.StreamReader myStream = new System.IO.StreamReader(SourceFile))
            {
                // Process lines while there are lines remaining in the file.
                while (!myStream.EndOfStream)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                    else
                    {
                        line = myStream.ReadLine();
                        WordCount += CountInString(line, CompareString);
                        LinesCounted += 1;

                        // Raise an event so the form can monitor progress.
                        int compare = DateTime.Compare(
                            DateTime.Now, lastReportDateTime.AddMilliseconds(elapsedTime));
                        if (compare > 0)
                        {
                            state.LinesCounted = LinesCounted;
                            state.WordsMatched = WordCount;
                            worker.ReportProgress(0, state);
                            lastReportDateTime = DateTime.Now;
                        }
                    }
                    // Uncomment for testing.
                    //System.Threading.Thread.Sleep(5);
                }

                // Report the final count values.
                state.LinesCounted = LinesCounted;
                state.WordsMatched = WordCount;
                worker.ReportProgress(0, state);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private int CountInString(
                        string SourceString,
                        string CompareString)
        {
            // This function counts the number of times a word is found in a line.
            if (SourceString == null)
            {
                return 0;
            }

            string EscapedCompareString =
                System.Text.RegularExpressions.Regex.Escape(CompareString);

            System.Text.RegularExpressions.Regex regex;
            regex = new System.Text.RegularExpressions.Regex(
                // To count all occurrences of the string, even within words, remove both instances
                // of @"\b" from the following line.
                @"\b" + EscapedCompareString + @"\b",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            System.Text.RegularExpressions.MatchCollection matches;
            matches = regex.Matches(SourceString);
            return matches.Count;
        }

        #endregion Private Methods

        #region Public Classes

        // Object to store the current state, for passing to the caller.
        public class CurrentState
        {
            #region Public Fields

            public int LinesCounted;
            public int WordsMatched;

            #endregion Public Fields
        }

        #endregion Public Classes
    }
}