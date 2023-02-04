namespace Eliza_MAUI;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

        // Copiar el fichero key.json en LocalApplicationData
        //if (DeviceInfo.Platform != DevicePlatform.WinUI) { CopiarGoogleCredentials(); }
        CopiarGoogleCredentials();

        MainPage = new AppShell();
	}

    public string Titulo = "Eliza para .NET MAUI usando Cloud Natural Language";

    // Si se van a tener páginas de navegación no se puede hacer esto, o al menos no cambiar el título.

    // Una forma más simple de cambiar el tamaño de la ventana.
    // Lo dejo porque con este código no se ve el cambio de tamaño de la ventana.
    // Aunque en realidad solo funcionará con Windows, en Android no tiene efecto,
    // así que... lo dejo solo para Windows.
    //
    // Basado en: https://es.askxammy.com/manejo-de-tamano-y-posicion-de-ventanas-en-net-maui/
    // https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/windows?view=net-maui-7.0
    //
#if WINDOWS
    protected override Window CreateWindow(IActivationState activationState)
    {
        //return base.CreateWindow(activationState);
        var window = base.CreateWindow(activationState);

        // Add here your sizing code

        window.Width = 935 + 15;
        window.Height = 880 + 57;

        // Add here your positioning code

        DisplayInfo disp = DeviceDisplay.Current.MainDisplayInfo;
        window.X = (disp.Width / disp.Density - window.Width * disp.Density) / 2;
        window.Y = (disp.Height / disp.Density - window.Height * disp.Density) / 2;

        window.Title = Titulo;

        // En realidad este es código específico para Windows.

        //// Cambiar el color de la barra de Windows
        //Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        //{
        //    var mauiWindow = handler.VirtualView;
        //    var nativeWindow = handler.PlatformView;
        //    nativeWindow.Activate();
        //    IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
        //    var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
        //    var window = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

        //    // dispatcher is used to give the window time to actually resize
        //    Dispatcher.Dispatch(() =>
        //    {
        //        // Este es el color que tiene en mi equipo la barra de título.
        //        window.TitleBar.BackgroundColor = Microsoft.UI.ColorHelper.FromArgb(255, 0, 120, 212);
        //        window.TitleBar.ForegroundColor = Microsoft.UI.Colors.White;
        //    });
        //});

        return window;
    }
#endif

    /// <summary>
    /// Copia el fichero key.json de la carpeta Resources\Raw a LocalApplicationData.
    /// </summary>
    /// <remarks>En este método se asigna la variable de entorno GOOGLE_APPLICATION_CREDENTIALS con el path de key.json</remarks>
    static async void CopiarGoogleCredentials()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("key.json");
        using var reader = new StreamReader(stream);

        var contents = reader.ReadToEnd();

        // "C:\\Users\\Guille\\AppData\\Local\\Packages\\a042cc35-b74e-4b89-8610-bbf1abcd1309_9zz4h110yvjzm\\LocalState\\key.json"
        //string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "key.json");
        var localAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        var gAppCredentials = Path.Combine(localAppData, "key.json");

        using var outputStream = File.OpenWrite(gAppCredentials);
        using var streamWriter = new StreamWriter(outputStream);

        await streamWriter.WriteAsync(contents);

        // Asignar la variable de entorno indicando dónde está el fichero key.json
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", gAppCredentials);
    }

    // Asignar los ficheros que están en Resources\Raw\palabras
    // ya que no veo la forma de recorrer ese directorio.
    static readonly string[] PalabrasEliza = { "Eliza_SPA.txt", "ElizaVB_prog.txt", "ElizaVB_suenios.txt", "ElizaVB_zodiaco.txt" };

    private static string _elizaDatos;

    /// <summary>
    /// Directorio donde se copian los datos de Eliza.
    /// </summary>
    public static string ElizaDatos { get => _elizaDatos; }

    // Esto tarda mucho y parece que al final no copia nada, (03/feb/23 18.13)
    // parece que es porque hay un fichero llamado Eliza_sueños.txt, le cambio el nombre a Eliza_suenios.txt (18.23)
    // En Android se ve que lo hace, pero en Windows se queda pillado...
    // dejo el código para que solo asigne el path de los datos.

    /// <summary>
    /// Copiar los ficheros de palabras de Resources\Raw\palabras en LocalApplicarionData\Eliza\palabras
    /// </summary>
    public static async void CopiarPalabrasEliza()
    {
        var localAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        var localEliza = Path.Combine(localAppData, "Eliza");
        _elizaDatos = localEliza;

        // No copiar las palabras si es Windows, yo ya las tengo copiadas ;-)
        //if (DeviceInfo.Platform == DevicePlatform.WinUI) return;

        if (Directory.Exists(localEliza) == false)
        {
            Directory.CreateDirectory(localEliza);
        }
        var localElizaPalabras = Path.Combine(localEliza, "palabras");
        if (Directory.Exists(localElizaPalabras) == false)
        {
            Directory.CreateDirectory(localElizaPalabras);
        }

        foreach (string palabras in PalabrasEliza)
        {
            string fic = palabras; // System.IO.Path.Combine(@".\palabras", palabras);
            using var stream = await FileSystem.OpenAppPackageFileAsync(fic);
            using var reader = new StreamReader(stream);

            var contents = reader.ReadToEnd();

            var ficElizaPalabras = Path.Combine(localElizaPalabras, palabras);

            using var outputStream = File.OpenWrite(ficElizaPalabras);
            using var streamWriter = new StreamWriter(outputStream);

            await streamWriter.WriteAsync(contents);
        }
    }

}
