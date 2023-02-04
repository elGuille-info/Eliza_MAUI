namespace Eliza_MAUI;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

    private async void ButtonEliza_Clicked(object sender, EventArgs e)
    {
        // También falla al cargar esta página
        await Navigation.PushAsync(new ElizaMainPage());
    }

    private async void ButtonAnalizar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AnalizarTextos());
    }


    //   int count = 0;
    //   private void OnCounterClicked(object sender, EventArgs e)
    //{
    //	count++;

    //	if (count == 1)
    //		CounterBtn.Text = $"Clicked {count} time";
    //	else
    //		CounterBtn.Text = $"Clicked {count} times";

    //	SemanticScreenReader.Announce(CounterBtn.Text);
    //}
}

