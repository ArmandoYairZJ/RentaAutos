using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MongoDB.Driver;


namespace RentaAutos.Pantallas;

public partial class Galeria : ContentPage
{


    private readonly OpMongo _opMongo;
    private readonly IMongoCollection<Auto> _autoCollection;
    private Timer _timer;

    public Galeria()
	{
		InitializeComponent();
        _opMongo = new OpMongo("mongodb://localhost:27017", "Autos");
        _autoCollection = _opMongo.GetCollection<Auto>("autos");
        CargarAutos();


    }

    public async void CargarAutos()
    {
        try
        {
            var autos = await _autoCollection.Find(_ => true).ToListAsync();

            foreach (var auto in autos)
            {
                auto.ImagenSource = ImageSource.FromStream(() => new MemoryStream(auto.Imagen));
            }

            var listView = new ListView();
            listView.ItemTemplate = new DataTemplate(() =>
            {
                var image = new Image
                {
                    Aspect = Aspect.AspectFit,
                    WidthRequest = 100,
                    HeightRequest = 100
                };
                image.SetBinding(Image.SourceProperty, "ImagenSource");

                var marcaLabel = new Label { FontAttributes = FontAttributes.Bold };
                marcaLabel.SetBinding(Label.TextProperty, "Marca");

                var modeloLabel = new Label();
                modeloLabel.SetBinding(Label.TextProperty, "Modelo");

                var placasLabel = new Label();
                placasLabel.SetBinding(Label.TextProperty, "Placas");

                var anoLabel = new Label();
                anoLabel.SetBinding(Label.TextProperty, "Ano");

                var precioDiaLabel = new Label();
                precioDiaLabel.SetBinding(Label.TextProperty, "PrecioDia", stringFormat: "Precio por día: {0:C}");

                var stackLayout = new StackLayout
                {
                    Padding = new Thickness(10),
                    Orientation = StackOrientation.Horizontal,
                    Children =
                {
                    image,
                    new StackLayout
                    {
                        VerticalOptions = LayoutOptions.Center,
                        Spacing = 5,
                        Children =
                        {
                            marcaLabel,
                            modeloLabel,
                            placasLabel,
                            anoLabel,
                            precioDiaLabel
                        }
                    }
                }
                };

                return new ViewCell { View = stackLayout };
            });

            listView.ItemsSource = autos;

            listView.ItemTapped += async (sender, e) =>
            {
                if (e.Item != null && e.Item is Auto selectedAuto)
                {
                    await Navigation.PushAsync(new Registroxaml(selectedAuto));
                }
            };

            Content = listView;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al cargar los autos: {ex.Message}", "Aceptar");
        }
    }


}