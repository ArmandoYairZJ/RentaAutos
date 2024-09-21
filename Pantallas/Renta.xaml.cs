using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using MongoDB.Driver;

namespace RentaAutos.Pantallas;

public partial class Renta : ContentPage
{
    private readonly OpMongo _opMongo;
    private readonly IMongoCollection<Auto> _autoCollection;

    public Renta()
	{
		InitializeComponent();
        _opMongo = new OpMongo("mongodb://localhost:27017", "Autos");
        _autoCollection = _opMongo.GetCollection<Auto>("autos");

        CargarPlacasAutos();
        DPFechaInicio.DateSelected += DPFechaInicio_DateSelected;
        DPFechaRegreso.DateSelected += DPFechaRegreso_DateSelected;
        DPFechaInicio.MinimumDate = DateTime.Today;
        DPFechaRegreso.MinimumDate = DateTime.Today;
       


    }


    private async void CargarPlacasAutos()
    {
        try
        {
            var autos = await _autoCollection.Find(_ => true).ToListAsync();
            var placas = autos.Select(auto => auto.Placas).ToList();

            ePlacas.Items.Clear();
            foreach (var placa in placas)
            {
                ePlacas.Items.Add(placa);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al cargar las placas de los autos: {ex.Message}", "Aceptar");
        }
    }



    private void DPFechaInicio_DateSelected(object sender, DateChangedEventArgs e)
    {
        CalcularPrecioTotal();

    }

    private void DPFechaRegreso_DateSelected(object sender, DateChangedEventArgs e)
    {
        CalcularPrecioTotal();

    }


    private void CalcularPrecioTotal()
    {
        if (DPFechaInicio.Date != DateTime.MinValue && DPFechaRegreso.Date != DateTime.MinValue)
        {
            var diferenciaDias = (DPFechaRegreso.Date - DPFechaInicio.Date).Days;

            if (diferenciaDias > 0)
            {
                if (double.TryParse(ePrecioDia.Text, out double precioDia))
                {
                    var precioTotal = diferenciaDias * precioDia;

                    ePrecioTotal.Text = precioTotal.ToString();
                }
            }
            else
            {
                ePrecioTotal.Text = string.Empty;
            }
        }
    }
    private async void btnrentar_Clicked(object sender, EventArgs e)
    {
        try
        {
            var placa = ePlacas.SelectedItem?.ToString();
            var marca = eMarca.Text;
            var modelo = eModelo.Text;
            var precioDia = double.Parse(ePrecioDia.Text);
            var fechaInicio = DPFechaInicio.Date;
            var fechaRegreso = DPFechaRegreso.Date;
            var precioTotal = double.Parse(ePrecioTotal.Text);

            var rentaAuto = new RentaAuto
            {
                Placa = placa,
                Marca = marca,
                Modelo = modelo,
                PrecioDia = precioDia,
                FechaInicio = fechaInicio,
                FechaRegreso = fechaRegreso,
                PrecioTotal = precioTotal
            };

            await InsertarRenta(rentaAuto);

            await DisplayAlert("Éxito", "La renta se ha registrado correctamente", "Aceptar");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al registrar la renta: {ex.Message}", "Aceptar");
        }
    }

    private async Task InsertarRenta(RentaAuto rentaAuto)
    {
        try
        {
            var rentaCollection = _opMongo.GetCollection<RentaAuto>("rentas");
            await rentaCollection.InsertOneAsync(rentaAuto);
            LimpiarCampos();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al insertar la renta en la base de datos: {ex.Message}");

        }
    }

    private void ePlacas_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedPlaca = ePlacas.SelectedItem?.ToString();

        var selectedAuto = _autoCollection.Find(auto => auto.Placas == selectedPlaca).FirstOrDefault();

        if (selectedAuto != null)
        {
            eMarca.Text = selectedAuto.Marca;
            eModelo.Text = selectedAuto.Modelo;
            ePrecioDia.Text = selectedAuto.PrecioDia.ToString();
        }
    }

    private void LimpiarCampos()
    {
        ePlacas.SelectedItem = null;
        eMarca.Text = string.Empty;
        eModelo.Text = string.Empty;
        ePrecioDia.Text = string.Empty;
        ePrecioTotal.Text = string.Empty;
    }
}

public class RentaAuto
{
    public string Placa { get; set; }
    public string Marca { get; set; }
    public string Modelo { get; set; }
    public double PrecioDia { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaRegreso { get; set; }
    public double PrecioTotal { get; set; }
}
