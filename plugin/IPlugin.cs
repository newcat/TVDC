using System.Windows.Media;

namespace tvdc.Plugin
{
    public interface IPlugin
    {

        /// <summary>
        /// The name will be displayed when hovering above the plugin icon.
        /// </summary>
        string pluginName { get; }

        /// <summary>
        /// The icon that will be displayed in the bar with all the plugin icons (right of the settings icon).
        /// </summary>
        /// <returns>The image to be drawn (Size 28x28)</returns>
        ImageSource getMenuIcon();

        /// <summary>
        /// The icon that will be displayed if the user hovers over the menu icon.
        /// </summary>
        /// <returns>The image to be drawn (Size 28x28)</returns>
        ImageSource getMenuIconHover();

        void IconClicked();
        void Initialize(IPluginHost host);


    }
}
