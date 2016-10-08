# IPlugin Interface

**Namespace:** tvdc.Plugin

### Properties
Name|Type|Description
----|-----------
pluginName|string|The name will be displayed when hovering above the plugin icon.

### Methods
Name|Return Type|Description
----|-------|-----------
getMenuIcon()|[ImageSource](https://msdn.microsoft.com/de-de/library/system.windows.media.imagesource(v=vs.110).aspx)|The icon that will be displayed in the bar with all the plugin icons (right of the settings icon).
getMenuIconHover()|[ImageSource](https://msdn.microsoft.com/de-de/library/system.windows.media.imagesource(v=vs.110).aspx)|The icon that will be displayed if the user hovers over the menu icon.
IconClicked()|void|This function gets called when the icon in the plugin bar gets pressed. It runs from the UI thread so avoid long operations in this method without threading.
Initialize([IPluginHost](IPluginHost.md))|void|This function will be called when the plugin gets loaded. It is suggested to assign the host parameter to a variable in your code so you can use it later.
End()|void|Called before the application quits. You can do some saving here.