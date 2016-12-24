// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 09-06-2015 ***********************************************************************
// <copyright file="WPFMenuItem.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System;

namespace TextFilter
{
    public class WPFMenuItem
    {
        public WPFMenuItem()
        {
        }

        public Command Command { get; set; }

        public String IconUrl { get; set; }

        public String Text { get; set; }
    }
}