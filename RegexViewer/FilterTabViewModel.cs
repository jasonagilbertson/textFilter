using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace RegexViewer
{
    public class FilterTabViewModel : BaseTabViewModel<FilterFileItem>
    {
        #region Public Constructors

        public FilterTabViewModel()
        {
            //     PopulateColors();
        }

        #endregion Public Constructors

        //private List<string> colorArray = new List<string>();

        //   //public ObservableCollection<string> ColorArray
        //   public List<string> ColorArray
        //   {
        //       get
        //       {
        //           //Array arr = (string[])Enum.GetValues(typeof(KnownColor));
        //           //List<string> ls = new List<string>((string[])Enum.GetValues(typeof(KnownColor)));
        //           return colorArray;
        //           //return new ObservableCollection<string>();
        //       }
        //   }
        ////private void PopulateColors()
        ////{
        //http://www.codeproject.com/Articles/140521/Color-Picker-using-WPF-Combobox
        ////http://msdn.microsoft.com/en-us/library/system.drawing.knowncolor(v=vs.110).aspx
        ////    Array colors = Enum.GetValues(typeof(Colors));
        ////    foreach (KnownColor knownColor in colors)
        ////    {
        ////        colorArray.Add(System.Drawing.Color.FromKnownColor(knownColor).ToString());

        ////    }
        ////}

        //private void PopulateColors()
        //{
        //    //this.Size = new Size(650, 550);

        //    // Get all the values from the KnownColor enumeration.
        //    System.Array colorsArray = Enum.GetValues(typeof(KnownColor));
        //    KnownColor[] allColors = new KnownColor[colorsArray.Length];

        //    Array.Copy(colorsArray, allColors, colorsArray.Length);

        //    // Loop through printing out the values' names in the colors
        //    // they represent.
        //    float y = 0;
        //    float x = 10.0F;

        //    for (int i = 0; i < allColors.Length; i++)
        //    {
        //        // If x is a multiple of 30, start a new column.
        //        if (i > 0 && i % 30 == 0)
        //        {
        //            x += 105.0F;
        //            y = 15.0F;
        //        }
        //        else
        //        {
        //            // Otherwise, increment y by 15.
        //            y += 15.0F;
        //        }

        //        colorArray.Add(allColors[i].ToString());
        //        // Create a custom brush from the color and use it to draw
        //        // the brush's name.
        //        //SolidBrush aBrush =
        //        //    new SolidBrush(System.Drawing.Color.FromName(allColors[i].ToString()));
        //        //////e.Graphics.DrawString(allColors[i].ToString(),
        //        //    this.Font, aBrush, x, y);

        //        // Dispose of the custom brush.
        //      //  aBrush.Dispose();
        //    }

        //   }
        //private Command copyCommand;
        //public Command CopyCommand
        //{
        //    get
        //    {
        //        if (copyCommand == null)
        //        {
        //            copyCommand = new Command(CopyExecuted);
        //        }
        //        copyCommand.CanExecute = true;

        //        return copyCommand;
        //    }
        //    set { copyCommand = value; }
        //}

        #region Public Methods

        //public void CopyExecuted(List<ListBoxItem> ContentList)
        public override void CopyExecuted(object contentList)
        {
            try
            {
                List<FilterFileItem> ContentList = (List<FilterFileItem>)contentList;

                HtmlFragment htmlFragment = new HtmlFragment();
                foreach (FilterFileItem lbi in ContentList)
                {
                    if (lbi != null && lbi.IsSelected)
                    //&& htmlFragment.Length < (copyContent.MaxCapacity - lbi.Content.ToString().Length))
                    {
                        // todo: use BackgroundColorBrush?
                        htmlFragment.AddClipToList(lbi.Content.ToString(),
                            ((SolidColorBrush)new BrushConverter().ConvertFromString(lbi.BackgroundColor)),
                            ((SolidColorBrush)new BrushConverter().ConvertFromString(lbi.ForegroundColor)));
                        
                    }
                }

                htmlFragment.CopyListToClipboard();
            }
            catch (Exception ex)
            {
                MainModel.SetStatus("Exception:CopyCmdExecute:" + ex.ToString());
            }
        }

        #endregion Public Methods
    }
}