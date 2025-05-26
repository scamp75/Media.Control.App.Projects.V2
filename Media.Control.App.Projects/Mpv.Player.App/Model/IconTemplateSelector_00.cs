using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Mpv.Player.App.Model
{
    class IconTemplateSelector : DataTemplateSelector
    {
        
        public DataTemplate PlayTemplate { get; set; }
        public DataTemplate StopTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is string icon && icon == "PLAY")
            {
                return PlayTemplate;
            }
            return StopTemplate;
        }
        
    }
}
