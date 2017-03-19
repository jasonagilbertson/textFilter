// *********************************************************************** 
// Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-25-2015 
// <copyright file="http://dlaa.me/blog/post/9917797">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TextFilter
{
    class DragDrop : Base
    {
        public static DragDropEffects DoDragDrop(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, DragDropEffects allowedEffects)
        {
            int[] finalEffect = new int[1];
            try
            {
                NativeMethods.DoDragDrop(dataObject, new DropSource(), (int)allowedEffects, finalEffect);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print("DoDragDrop:exception:" + e.ToString());
            }

            return (DragDropEffects)(finalEffect[0]);
        }

        private class DropSource : NativeMethods.IDropSource
        {
            public int QueryContinueDrag(int fEscapePressed, uint grfKeyState)
            {
                var escapePressed = (0 != fEscapePressed);
                var keyStates = (DragDropKeyStates)grfKeyState;
                if (escapePressed)
                {
                    return NativeMethods.DRAGDROP_S_CANCEL;
                }
                else if (DragDropKeyStates.None == (keyStates & DragDropKeyStates.LeftMouseButton))
                {
                    return NativeMethods.DRAGDROP_S_DROP;
                }
                return NativeMethods.S_OK;
            }

            public int GiveFeedback(uint dwEffect)
            {
                return NativeMethods.DRAGDROP_S_USEDEFAULTCURSORS;
            }
        }

        private static class NativeMethods
        {
            public const int DRAGDROP_S_DROP = 0x00040100;
            public const int DRAGDROP_S_CANCEL = 0x00040101;
            public const int DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102;
            public const int S_OK = 0;
            public const int S_FALSE = 1;
            
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
            [DllImport("ole32.dll", CharSet = CharSet.Auto, ExactSpelling = true, PreserveSig = false)]
            public static extern void DoDragDrop(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, IDropSource dropSource, int allowedEffects, int[] finalEffect);

        }

    }
}
