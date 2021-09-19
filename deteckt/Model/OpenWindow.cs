using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace deteckt.Model
{
    class OpenWindow
    {
        public void DisplayWindow(Window windowToShow)
        {
            for (int i = 0; i < Application.Current.Windows.OfType<Window>().
        Where(w => w.IsVisible).Count(); i++)
            {
                Window windowToHide = Application.Current.Windows[i];
                windowToHide.Visibility = Visibility.Collapsed;
            }
            windowToShow.Visibility = Visibility.Visible;
        }
    }
}
