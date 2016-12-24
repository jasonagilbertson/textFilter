// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 09-06-2015 ***********************************************************************
// <copyright file="LogFileItem.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************

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