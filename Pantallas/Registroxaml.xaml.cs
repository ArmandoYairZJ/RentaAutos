using MongoDB.Driver;
using System.IO;
using Microsoft.Maui.Controls;
using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;



namespace RentaAutos.Pantallas;


public partial class Registroxaml : ContentPage
{

    private readonly OpMongo _opMongo;
    private readonly IMongoCollection<Auto> _autoCollection;
    private Auto _autoSeleccionado; 



    public Registroxaml(Auto auto)
	{
        InitializeComponent();
        _opMongo = new OpMongo("mongodb://localhost:27017", "Autos");
        _autoCollection = _opMongo.GetCollection<Auto>("autos");

      


        CargarPlacasAutos();
    }




    byte[] ImagenBytes;
    private async void btnAgregarImagen_Clicked(object sender, EventArgs e)
    {
		try {

			var op = new MediaPickerOptions
			{
				Title = "Selecciona tu imagen"
			};

			var res = await MediaPicker.PickPhotoAsync(op);
			if(res !=null)
			{
                using (MemoryStream ms = new MemoryStream())
                {
                    using (var stream = await res.OpenReadAsync())
                    {
                        await stream.CopyToAsync(ms);
                    }
                    ImagenBytes = ms.ToArray();
                    imagenCargar.Source = ImageSource.FromStream(() => new MemoryStream(ImagenBytes));
                }
            }

		} 
		catch (Exception ex) {
			await DisplayAlert("Error al cargar la imagen", $"Error al seleccionar su imagen:{ex.Message}", "Bueno") ;
		}
    }

    private async void bGuardar_Clicked(object sender, EventArgs e)
    {
        try
        {
            var auto = new Auto
            {
                Placas = ePlacas.Text,
                Marca = eMrca.Text,
                Modelo = eModelo.Text,
                ano = Convert.ToInt32(eAno.Text),
                PrecioDia = Convert.ToDouble(ePrecioDia.Text),
                Imagen = ImagenBytes 
            };

            await _autoCollection.InsertOneAsync(auto);

            await DisplayAlert("Éxito", "Auto registrado correctamente", "Aceptar");
            LimpiarCampos();


        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al registrar el auto: {ex.Message}", "Aceptar");
        }
    }

   
    private async void bModificar_Clicked(object sender, EventArgs e)
    {
        try
        {
            string placaSeleccionada = EPlACASS.SelectedItem?.ToString();

            if (placaSeleccionada != null)
            {
                // Obtener el auto seleccionado por la placa
                var autoSeleccionado = await _autoCollection.Find(x => x.Placas == placaSeleccionada).FirstOrDefaultAsync();

                if (autoSeleccionado != null)
                {
                    // Actualizar los datos del auto seleccionado, excepto la imagen
                    autoSeleccionado.Placas = ePlacas.Text;
                    autoSeleccionado.Marca = eMrca.Text;
                    autoSeleccionado.Modelo = eModelo.Text;
                    autoSeleccionado.ano = Convert.ToInt32(eAno.Text);
                    autoSeleccionado.PrecioDia = Convert.ToDouble(ePrecioDia.Text);

                    // Crear el filtro y la actualización
                    var filter = Builders<Auto>.Filter.Eq("_id", autoSeleccionado.Id);
                    var update = Builders<Auto>.Update
                        .Set("Placas", autoSeleccionado.Placas)
                        .Set("Marca", autoSeleccionado.Marca)
                        .Set("Modelo", autoSeleccionado.Modelo)
                        .Set("ano", autoSeleccionado.ano)
                        .Set("PrecioDia", autoSeleccionado.PrecioDia);

                    // Ejecutar la actualización en la base de datos
                    var result = await _autoCollection.UpdateOneAsync(filter, update);

                    // Mostrar mensaje de éxito y limpiar campos
                    await DisplayAlert("Éxito", "Auto modificado correctamente", "Aceptar");
                    LimpiarCampos();
                }
                else
                {
                    await DisplayAlert("Error", "No se encontró información para la placa seleccionada", "Aceptar");
                }
            }
            else
            {
                await DisplayAlert("Advertencia", "Por favor, seleccione una placa para modificar", "Aceptar");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al modificar el auto: {ex.Message}", "Aceptar");
        }
    }

    private void LimpiarCampos()
    {
        ePlacas.Text = string.Empty;
        eMrca.Text = string.Empty;
        eModelo.Text = string.Empty;
        eAno.Text = string.Empty;
        ePrecioDia.Text = string.Empty;
        imagenCargar.Source = null;
        ImagenBytes = null;
    }


    private async void CargarPlacasAutos()
    {
        try
        {
            var autos = await _autoCollection.Find(_ => true).ToListAsync();
            var placas = autos.Select(auto => auto.Placas).ToList();

            EPlACASS.Items.Clear();
            foreach (var placa in placas)
            {
                EPlACASS.Items.Add(placa);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al cargar las placas de los autos: {ex.Message}", "Aceptar");
        }
    }


    private void EPlACASS_SelectedIndexChanged(object sender, EventArgs e)
    {
        string placaSeleccionada = EPlACASS.SelectedItem?.ToString();

        if (placaSeleccionada != null)
        {
            var autoSeleccionado = _autoCollection.Find(x => x.Placas == placaSeleccionada).FirstOrDefault();

            if (autoSeleccionado != null)
            {
                ePlacas.Text = autoSeleccionado.Placas;
                eMrca.Text = autoSeleccionado.Marca;
                eModelo.Text = autoSeleccionado.Modelo;
                eAno.Text = autoSeleccionado.ano.ToString();
                ePrecioDia.Text = autoSeleccionado.PrecioDia.ToString();
                imagenCargar.Source = ImageSource.FromStream(() => new MemoryStream(autoSeleccionado.Imagen));
            }
            else
            {
                LimpiarCampos();
                DisplayAlert("Error", "No se encontró información para la placa seleccionada", "Aceptar");
            }
        }
    }
}

public class Auto
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string Placas { get; set; }
    public string Marca { get; set; }
    public string Modelo { get; set; }
    public int ano { get; set; }
    public double PrecioDia { get; set; }

    public byte[] Imagen { get; set; }

    
    [BsonIgnore] 
    public ImageSource ImagenSource { get; set; }
}

