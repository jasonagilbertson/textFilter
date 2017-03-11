// ************************************************************************************
// Assembly: TextFilter
// File: WPFMenuItem.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

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