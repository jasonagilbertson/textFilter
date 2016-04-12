// ***********************************************************************
// Assembly         : TextFilter
// Author           : jason
// Created          : 09-06-2015
//
// Last Modified By : jason
// Last Modified On : 09-06-2015
// ***********************************************************************
// <copyright file="FilterCommand.cs" company="">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace TextFilter
{
    public enum FilterCommand
    {
        Filter,

        ShowAll,

        Unknown,

        Hide,
    }

    public enum FilterNeed
    {
        Current,

        Filter,

        ApplyColor,

        Unknown,

        ShowAll
    }
}