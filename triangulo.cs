using System;

public class Triangulo 
{
    // Objetos Punto
    public Punto P1 { get; set; } = new Punto();
    public Punto P2 { get; set; } = new Punto();
    public Punto P3 { get; set; } = new Punto();

    // Coordenadas individuales para la BD
    public double X1 { get => P1.X; set => P1.X = value; }
    public double Y1 { get => P1.Y; set => P1.Y = value; }
    public double X2 { get => P2.X; set => P2.X = value; }
    public double Y2 { get => P2.Y; set => P2.Y = value; }
    public double X3 { get => P3.X; set => P3.X = value; }
    public double Y3 { get => P3.Y; set => P3.Y = value; }

    // Lados y Tipo
    public double L1 { get; set; }
    public double L2 { get; set; }
    public double L3 { get; set; }
    public string Tipo { get; set; } = "";

    // Constructor vacío
    public Triangulo() 
    { 
        P1 = new Punto();
        P2 = new Punto();
        P3 = new Punto();
    }

    // Constructor con parámetros
    public Triangulo(Punto p1, Punto p2, Punto p3)
    {
        P1 = p1;
        P2 = p2;
        P3 = p3;

        // Cálculo de las distancias (lados)
        L1 = Math.Sqrt(Math.Pow(P2.X - P1.X, 2) + Math.Pow(P2.Y - P1.Y, 2));
        L2 = Math.Sqrt(Math.Pow(P3.X - P2.X, 2) + Math.Pow(P3.Y - P2.Y, 2));
        L3 = Math.Sqrt(Math.Pow(P1.X - P3.X, 2) + Math.Pow(P1.Y - P3.Y, 2));

        // Determinar el tipo de triángulo
        CalcularTipo();
    }

    private void CalcularTipo()
    {
        if (Math.Abs(L1 - L2) < 0.0001 && Math.Abs(L2 - L3) < 0.0001)
        {
            Tipo = "Equilatero";
        }
        else if (Math.Abs(L1 - L2) < 0.0001 || Math.Abs(L2 - L3) < 0.0001 || Math.Abs(L1 - L3) < 0.0001)
        {
            Tipo = "Isosceles";
        }
        else
        {
            Tipo = "Escaleno";
        }
    }
}