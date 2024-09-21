namespace RentaAutos.Pantallas;

public partial class Inicio : TabbedPage
{
	public Inicio()
	{
		InitializeComponent();

        var Galeria = new Galeria()
        {
            Title = "Galeria",
            IconImageSource = "galeria.svg"
        };

        var Registro = new Registroxaml(null)
        {
            Title = "Registro",
            IconImageSource = "registro.svg"
        };

        var Renta = new Renta()
        {
            Title = "Renta",
            IconImageSource = "renta.svg"
        };

        this.Children.Add(Galeria);
        this.Children.Add(Registro);
        this.Children.Add(Renta);
    }
}