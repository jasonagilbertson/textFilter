// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-31-2015 ***********************************************************************
// <copyright file="ItemsProvider.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************

using System.Collections.Generic;

namespace TextFilter
{
    public interface IItemsProvider<T>
    {
        int FetchCount();

        IList<T> FetchRange(int startIndex, int count);
    }
}