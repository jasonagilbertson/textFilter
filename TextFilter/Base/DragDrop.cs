// ************************************************************************************
// Assembly: TextFilter
// File: Base.cs
// Created: 9/6/2016
// Modified: 2/12/2017
// Copyright (c) 2017 jason gilbertson
// file="http://dlaa.me/blog/post/9917797">
// file="https://bhrnjica.net/2014/04/28/drag-and-drop-item-outside-wpf-application/">
//
// ************************************************************************************

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace TextFilter
{
    internal class DragDrop : Base
    {
        private static DateTime dropTime = new DateTime();

        // todo: review once ver 0.8.x merged
        private static Point startingPoint = new Point();

        public static DragDropEffects DoDragDrop(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, DragDropEffects allowedEffects)
        {
            Debug.Print("DoDragDrop:enter:");
            startingPoint = Mouse.GetPosition(null);
            dropTime = DateTime.Now;

            int[] finalEffect = new int[1];
            try
            {
                if ((Mouse.LeftButton == MouseButtonState.Pressed))
                {
                    NativeMethods.DoDragDrop(dataObject, new DropSource(), (int)allowedEffects, finalEffect);
                }
                else
                {
                    Debug.Print("DoDragDrop:mouse button not pressed:");
                }
            }
            catch (Exception e)
            {
                Debug.Print("DoDragDrop:exception:" + e.ToString());
            }

            Debug.Print("DoDragDrop:exit:");
            return (DragDropEffects)(finalEffect[0]);
        }

        private static class NativeMethods
        {
            public const int DRAGDROP_S_CANCEL = 0x00040101;
            public const int DRAGDROP_S_DROP = 0x00040100;
            public const int DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102;
            public const int S_FALSE = 1;
            public const int S_OK = 0;

            [DllImport("ole32.dll", CharSet = CharSet.Auto, ExactSpelling = true, PreserveSig = false)]
            public static extern void DoDragDrop(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, IDropSource dropSource, int allowedEffects, int[] finalEffect);

            [ComImport]
            [Guid("00000121-0000-0000-C000-000000000046")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IDropSource
            {
                [PreserveSig]
                int QueryContinueDrag(int fEscapePressed, uint grfKeyState);

                [PreserveSig]
                int GiveFeedback(uint dwEffect);
            }
        }

        private class DropSource : NativeMethods.IDropSource
        {
            public int GiveFeedback(uint dwEffect)
            {
                return NativeMethods.DRAGDROP_S_USEDEFAULTCURSORS;
            }

            public int QueryContinueDrag(int fEscapePressed, uint grfKeyState)
            {
                var escapePressed = (0 != fEscapePressed);
                Debug.Print(string.Format("QueryContinueDrag escapePressed: {0}", escapePressed));
                Debug.Print(string.Format("QueryContinueDrag left mouse button pressed: {0}", (Mouse.LeftButton == MouseButtonState.Pressed)));
                bool timedOut = DateTime.Now.Subtract(dropTime).TotalSeconds > 5;
                var keyStates = (DragDropKeyStates)grfKeyState;

                if (timedOut)
                {
                    Debug.Print(string.Format("QueryContinueDrag timedOut: {0}", timedOut));
                    return NativeMethods.DRAGDROP_S_CANCEL;
                }
                else if (escapePressed)
                {
                    return NativeMethods.DRAGDROP_S_CANCEL;
                }
                else if (DragDropKeyStates.None == (keyStates & DragDropKeyStates.LeftMouseButton))
                {
                    double distance = Point.Subtract(Mouse.GetPosition(null), startingPoint).Length;
                    Debug.Print(string.Format("QueryContinueDrag distance: {0}", distance));
                    if (distance > 100)
                    {
                        return NativeMethods.DRAGDROP_S_DROP;
                    }
                    else
                    {
                        return NativeMethods.DRAGDROP_S_CANCEL;
                    }
                }
                return NativeMethods.S_OK;
            }
        }
    }
}