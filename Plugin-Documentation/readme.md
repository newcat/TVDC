# Plugin Documentation

### What you need
* Visual Studio
* The [plugin.dll](plugin.dll)
* (Optional) If you want the TVD-Styling for your plugin, use [this resource dictionary](tvdcStyling.xaml)

### Get started
Create a new project in Visual Studio. For the Template you select "WPF Custom Control Library" in the Classic Desktop section. When VS creates this project, it automatically puts a lot of stuff in there that we don't need. So just delete the "Themes" folder and the "CustomControl1.cs" file.

After that you can go ahead and add the reference to the plugin.dll. For that you right-click on the "References" entry in your project and choose "Add Reference". Go to the Browse tab and click the "Browse"-Button at the bottom of the window, then navigate to the plugin.dll and choose it.

Create a new class (you can name it how you want) and open up the source file. Now we need to import two namespaces:

```C#
using tvdc.EventArguments;
using tvdc.Plugin;
```

For your plugin to work, it is necessary that at least one class implements the `IPlugin`-Interface. I will use the TestClass in this example:

```C#
using tvdc.EventArguments;
using tvdc.Plugin;

namespace TestPlugin
{
    class TestPlugin : IPlugin
    {
    }
}
```

Now we need to implement the interface members:
```C#
public string pluginName { get { return "TestPlugin"; } }
public void Initialize(IPluginHost host) { }
public ImageSource getMenuIcon() { return null; }
public ImageSource getMenuIconHover() { return null; }
public void IconClicked() { }
public void End() { }
```

Ok well at least we can compile without errors now. But it doesn't do anything yet. So let's start with the `Initialize`-Method:

I suggest you create a variable in your class, which you assign the `host`-parameter to.
For my example, I want to send a message back to the user when he sends "Hello". So let's see what my TestClass looks like now:

```C#
class TestPlugin : IPlugin
{

    public string pluginName { get { return "TestPlugin"; } }

    private IPluginHost host;
    public void Initialize(IPluginHost host) {
        this.host = host;
        host.IRC_PrivmsgReceived += PrivmsgReceived;
    }

    private void PrivmsgReceived(object sender, PrivmsgReceivedEventArgs e)
    {
        if (e.message == "Hello")
            host.sendMesssage("Hello, " + e.username);
    }

    public ImageSource getMenuIcon() { return null; }
    public ImageSource getMenuIconHover() { return null; }
    public void IconClicked() { }
    public void End() { }

}
```

> NOTE: All IRC events are called from the IRC thread, so don't do long work in there or try to make it asynchronous.

To run the program, simpley build the project and copy the dll-File from your projects output directory to where your tvdc.exe is located. Also make sure, there is a plugin.dll in the same folder (it should be there by default though).


### Make your plugin show up in the plugin bar
If you're asking yourself now: "How the hell can I create a image source?", then I've got the answer here for you:
Basically it takes a `System.Drawing.Bitmap` and converts it into a `BitmapImage`, which is a subclass of `ImageSource`.
```C#
private BitmapImage BmpToImg(System.Drawing.Bitmap bmp)
{
    MemoryStream ms = new MemoryStream();
    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
    BitmapImage bi = new BitmapImage();
    bi.BeginInit();
    ms.Seek(0, SeekOrigin.Begin);
    bi.StreamSource = ms;
    bi.EndInit();
    return bi;
}
```

So, assuming you got two bitmaps in your resources, one for hover and one for default, your getMenuIcon-Methods could look something like this:
```C#
public ImageSource getMenuIcon() {
    return BmpToImg(Properties.Resources.btn);
}

public ImageSource getMenuIconHover() {
    return BmpToImg(Properties.Resources.btn_hover);
}
```


### Opening windows
Now for the last step. If you want your plugins to display data or let the user type in stuff, all you have to do is create a new WPF window in your project. Usually you want your window to open, when the user clicks the icon in the plugin bar, so do something like this:

```C#
public void IconClicked() {
    TestWindow testWindow = new TestWindow();
    testWindow.Show();
}
```

##### Styling windows
If you want to make the plugin UI look as (cool as ;) ) the UI of the TVD, I recommend you download the ResourceDictionary-File linked at the top of this page and add it to your project.
In your window, simply add this:

```XML
<Window.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="tvdcStyling.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Window.Resources>
```

Now all controls should be styled by default. The last thing to do then is to set the Background-Property of the window to `Background="{StaticResource WindowBackgroundBrush}"`. And you're done. Well, only if you don't care about functionality though ;) But thats your task now.

### Reference
If you want to know more, check out the [Reference](INSERTLINKHERE).
