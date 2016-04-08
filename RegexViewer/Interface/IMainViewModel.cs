﻿// ***********************************************************************
// Assembly         : RegexViewer
// Author           : jason
// Created          : 09-06-2015
//
// Last Modified By : jason
// Last Modified On : 09-06-2015
// ***********************************************************************
// <copyright file="IMainViewModel.cs" company="">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace RegexViewer
{
    public interface IMainViewModel
    {
        #region Public Events

        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Methods

        void SetViewStatus(string statusData);

        #endregion Public Methods
    }
}