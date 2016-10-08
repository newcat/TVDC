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

        /// <summary>
        /// This function gets called when the icon in the plugin bar gets pressed.
        /// It runs from the UI thread so avoid long operations in this method without threading.
        /// </summary>
        void IconClicked();

        /// <summary>
        /// This function will be called when the plugin gets loaded.
        /// It is suggested to assign the host parameter to a variable in your code so you can use it later.
        /// </summary>
        /// <param name="host">The plugin host which you can use to subscribe to chat events and send messages.</param>
        void Initialize(IPluginHost host);

        /// <summary>
        /// Called before the application quits. You can do some saving here.
        /// </summary>
        void End();


    }
}
