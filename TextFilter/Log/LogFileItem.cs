// ************************************************************************************
// Assembly: TextFilter
// File: LogFileItem.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

namespace TextFilter
{
    public class LogFileItem : FileItem
    {
        public int FilterIndex { get; set; }

        public string Group1 { get; set; }

        public string Group2 { get; set; }

        public string Group3 { get; set; }

        public string Group4 { get; set; }

        public int[,] Masked { get; set; }
    }
}