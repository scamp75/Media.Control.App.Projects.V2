
using System.Windows.Controls;
using System.Windows;

namespace Mpv.Player.App.Controls
{
    public class IconTemplateSelector : DataTemplateSelector
    {


        public DataTemplate PlayTemplate { get; set; }
        public DataTemplate StopTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is string icon && icon == "Play")
            {
                return PlayTemplate;
            }
            return StopTemplate;
        }


    }
}
