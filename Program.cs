using Npgsql;
using Dapper;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connectionString = builder.Configuration.GetConnectionString("NeonDb")!;

// Serve la página HTML principal con el formulario para ingresar coordenadas
app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <title>Calculadora de Triángulos</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 30px; background-color: #f4f4f9; }
        .container { max-width: 500px; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
        .group { display: flex; gap: 10px; margin-bottom: 10px; }
        label { font-weight: bold; }
        input { width: 100%; padding: 8px; margin-top: 4px; box-sizing: border-box; }
        button { background-color: #007bff; color: white; border: none; padding: 10px 15px; border-radius: 5px; cursor: pointer; width: 100%; font-size: 16px; }
        button:hover { background-color: #0056b3; }
        #resultado { margin-top: 15px; padding: 10px; border-radius: 5px; display: none; }
        .exito { background-color: #d4edda; color: #155724; border: 1px solid #c3e6cb; }
        .error { background-color: #f8d7da; color: #721c24; border: 1px solid #f5c6cb; }
    </style>
</head>
<body>

<div class='container'>
    <h2>Ingresar Coordenadas del Triángulo</h2>
    <form id='formTriangulo'>
        <h4>Punto 1</h4>
        <div class='group'>
            <input type='number' step='any' id='x1' placeholder='X1' required>
            <input type='number' step='any' id='y1' placeholder='Y1' required>
        </div>
        <h4>Punto 2</h4>
        <div class='group'>
            <input type='number' step='any' id='x2' placeholder='X2' required>
            <input type='number' step='any' id='y2' placeholder='Y2' required>
        </div>
        <h4>Punto 3</h4>
        <div class='group'>
            <input type='number' step='any' id='x3' placeholder='X3' required>
            <input type='number' step='any' id='y3' placeholder='Y3' required>
        </div>
        <br>
        <button type='submit'>Calcular e Insertar</button>
    </form>

    <div id='resultado'></div>
    <br>
    <a href='/triangulos' target='_blank'>Ver todos los triángulos registrados (JSON)</a>
</div>

<script>
    document.getElementById('formTriangulo').addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const x1 = document.getElementById('x1').value;
        const y1 = document.getElementById('y1').value;
        const x2 = document.getElementById('x2').value;
        const y2 = document.getElementById('y2').value;
        const x3 = document.getElementById('x3').value;
        const y3 = document.getElementById('y3').value;

        const url = `/triangulos?x1=${x1}&y1=${y1}&x2=${x2}&y2=${y2}&x3=${x3}&y3=${y3}`;
        const resDiv = document.getElementById('resultado');

        try {
            const res = await fetch(url, { method: 'POST' });
            const data = await res.json();

            resDiv.style.display = 'block';
            if (res.ok) {
                resDiv.className = 'exito';
                resDiv.innerHTML = `<strong>${data.mensaje}</strong><br>` +
                    `Lado 1 (P1 a P2): ${data.datos.l1.toFixed(2)}<br>` +
                    `Lado 2 (P2 a P3): ${data.datos.l2.toFixed(2)}<br>` +
                    `Lado 3 (P3 a P1): ${data.datos.l3.toFixed(2)}<br>` +
                    `Tipo: <strong>${data.datos.tipo}</strong>`;
            } else {
                resDiv.className = 'error';
                resDiv.innerHTML = `<strong>Error:</strong> ${data.mensaje}`;
            }
        } catch (err) {
            resDiv.style.display = 'block';
            resDiv.className = 'error';
            resDiv.innerHTML = 'Ocurrió un error al conectar con el servidor.';
        }
    });
</script>

</body>
</html>
", "text/html"));

// Endpoint que calcula distancias, verifica duplicados en Neon e inserta
app.MapPost("/triangulos", async (double x1, double y1, double x2, double y2, double x3, double y3) =>
{
    // 1. Instanciar objetos Punto y calcular distancias/tipo mediante el constructor de Triangulo
    var p1 = new Punto(x1, y1);
    var p2 = new Punto(x2, y2);
    var p3 = new Punto(x3, y3);

    var triangulo = new Triangulo(p1, p2, p3);

    using var connection = new NpgsqlConnection(connectionString);

    // 2. Insertar solo si no existe el duplicado (ON CONFLICT DO NOTHING)
    string sql = @"
        INSERT INTO triangulo (x1, y1, x2, y2, x3, y3, l1, l2, l3, tipo)
        VALUES (@X1, @Y1, @X2, @Y2, @X3, @Y3, @L1, @L2, @L3, @Tipo)
        ON CONFLICT (x1, y1, x2, y2, x3, y3) DO NOTHING;";

    int filasAfectadas = await connection.ExecuteAsync(sql, triangulo);

    // 3. Si no afectó filas (0), significa que el triángulo ya existía
    if (filasAfectadas == 0)
    {
        return Results.Conflict(new 
        { 
            mensaje = "Este triángulo ya está registrado en la base de datos.",
            duplicado = true 
        });
    }

    return Results.Ok(new 
    { 
        mensaje = "Triángulo registrado exitosamente en Neon", 
        datos = triangulo 
    });
});

// Endpoint GET para ver la lista de triángulos
app.MapGet("/triangulos", async () =>
{
    using var connection = new NpgsqlConnection(connectionString);
    var lista = await connection.QueryAsync<Triangulo>("SELECT * FROM triangulo;");
    return Results.Ok(lista);
});

app.Run();