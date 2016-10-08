# IPlugin Interface

**Namespace:** tvdc.Plugin

### Properties
Name|Description
----|-----------
pluginName|The name will be displayed when hovering above the plugin icon.

### Methods
Name|Description
----|-----------
getMenuIcon()|The icon that will be displayed in the bar with all the plugin icons (right of the settings icon).
getMenuIconHover()|The icon that will be displayed if the user hovers over the menu icon.
IconClicked()|This function gets called when the icon in the plugin bar gets pressed. It runs from the UI thread so avoid long operations in this method without threading.
Initialize(IPluginHost)|This function will be called when the plugin gets loaded. It is suggested to assign the host parameter to a variable in your code so you can use it later.
End()|Called before the application quits. You can do some saving here.
