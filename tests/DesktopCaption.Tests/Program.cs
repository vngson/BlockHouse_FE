using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        try { Run(args); } catch (Exception error) { Console.Error.WriteLine(error); Environment.ExitCode=1; }
    }
    private static void Run(string[] args)
    {
        var output=Path.GetFullPath(args[0]); Directory.CreateDirectory(output);
        var window=new BlockHouse.DesktopWindow();
        var frame=(Border)window.Content;
        var getBounds=typeof(BlockHouse.DesktopWindow).GetMethod("GetMaximizedBounds",BindingFlags.Static|BindingFlags.NonPublic)!;
        foreach(var sample in new[]{
            (new Int32Rect(0,0,1920,1080),new Int32Rect(0,0,1920,1020),new Int32Rect(0,0,1920,1020)),
            (new Int32Rect(-1920,0,1920,1080),new Int32Rect(-1880,0,1880,1080),new Int32Rect(40,0,1880,1080)),
            (new Int32Rect(0,-1080,1920,1080),new Int32Rect(0,-1040,1920,1040),new Int32Rect(0,40,1920,1040))})
            if((Int32Rect)getBounds.Invoke(null,new object[]{sample.Item1,sample.Item2})! != sample.Item3)
                throw new Exception("Maximized bounds must exclude taskbar on each monitor");
        var grid=(Grid)frame.Child;
        var caption=(Border)grid.Children[0];
        var theme=typeof(BlockHouse.DesktopWindow).GetMethod("ApplyCaptionTheme",BindingFlags.Instance|BindingFlags.NonPublic)!;
        foreach(var dark in new[]{false,true}) foreach(var width in new[]{360,1200})
        {
            theme.Invoke(window,new object[]{dark});
            caption.Width=width; caption.Height=40;
            caption.Measure(new Size(width,40)); caption.Arrange(new Rect(0,0,width,40)); caption.UpdateLayout();
            var buttons=Children(caption).OfType<Button>().ToArray();
            if(buttons.Length!=3 || buttons.Any(button=>string.IsNullOrWhiteSpace(AutomationProperties.GetName(button)))) throw new Exception("Accessible caption buttons missing");
            foreach(var button in buttons)
            {
                var bounds=button.TransformToAncestor(caption).TransformBounds(new Rect(0,0,button.ActualWidth,button.ActualHeight));
                if(bounds.Left<0 || bounds.Right>width || bounds.Top<6 || bounds.Bottom>34) throw new Exception("Caption button overflows");
            }
            if(window.IsLoaded) throw new Exception("Preview must not start the application or backend");
            var bitmap=new RenderTargetBitmap(width,40,96,96,PixelFormats.Pbgra32); bitmap.Render(caption);
            var encoder=new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream=File.Create(Path.Combine(output,$"caption-{(dark?"dark":"light")}-{width}.png")); encoder.Save(stream);
        }
        window.Close();
        Console.WriteLine("PASS actual WPF caption XAML: light/dark at 360 and 1200px, all three named buttons fit; window never shown and BE not started.");
    }
    private static IEnumerable<DependencyObject> Children(DependencyObject root)
    {
        for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)
        {var child=VisualTreeHelper.GetChild(root,i); yield return child; foreach(var nested in Children(child)) yield return nested;}
    }
}
