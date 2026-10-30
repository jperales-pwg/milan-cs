# CLAUDE.md

MILAN C# Lightweight: tienda educativa de drones en ASP.NET Core Razor Pages (.NET 10). Sin base de datos ni servicios externos; todo vive en memoria.

## Comandos (desde la raíz)

```powershell
dotnet restore Milan.sln
dotnet build Milan.sln
dotnet test Milan.sln                      # xUnit, 16 pruebas
dotnet test Milan.sln --filter "FullyQualifiedName~CommerceTests"   # una clase
dotnet run --project src/Milan.Web         # https://localhost:54626
```

El aviso `NU1900` (feed privado inaccesible) es inofensivo.

## Estructura

- `src/Milan.Web/Program.cs`: punto de entrada, DI, sesión y autenticación por cookie.
- `src/Milan.Web/Models/`: `Product`, `Cart`/`CartItem`, `Order`/`OrderLine`.
- `src/Milan.Web/Services/`: `CatalogService` (catálogo fijo en código), `SessionCartService` (carrito en sesión), `OrderService` (pedidos en memoria), `DemoAccountService` (2 cuentas ficticias).
- `src/Milan.Web/Pages/`: Index (catálogo), Carrito, Compra, PedidoConfirmado, MisPedidos, Cuenta/Entrar y Cuenta/Salir.
- `tests/Milan.Tests/`: `CommerceTests` (unitarias), `CommerceFlowTests` (integración HTTP con `WebApplicationFactory<Program>`) e `IndexModelCharacterizationTests` (caracterización de los valores y el orden de `Brands`/`Types` en `IndexModel.OnGet`).
- `evidencia/`: notas locales por misión; está en `.gitignore`, no se versiona.

## Convenciones no evidentes

- **Idioma:** identificadores de código en inglés; rutas, handlers (`OnPostAgregar`, `OnPostCantidad`, `OnPostEliminar`), textos de UI y mensajes de error en español.
- **Estilo compacto:** varias sentencias por línea, constructores primarios, clases `sealed`, sin comentarios. Respétalo al editar.
- **Cultura:** `es-MX` fijada globalmente en `Program.cs`; los importes son `decimal` y se formatean con `"C2"`.
- **Ciclos de vida de DI:** `CatalogService`, `OrderService` y `DemoAccountService` son singleton (el estado de pedidos vive ahí); `SessionCartService` es scoped.
- **`CartItem` debe seguir siendo una clase con setters públicos:** se serializa con `System.Text.Json` en la sesión (`milan.cart`). `Order` y `OrderLine` son records inmutables.
- **El precio se copia al agregar al carrito** (`Cart.Add`), no al comprar.
- **Cantidades (límites inconsistentes):** `Cart` limita a 10 (`MaxQuantity`; `Add` se detiene en 10 y `SetQuantity` lanza `ArgumentOutOfRangeException` fuera de 0–10; 0 elimina la línea), y las pruebas fijan ese 10. En cambio, `SessionCartService.SetQuantity`, el mensaje de `Carrito` y el `max` del input aceptan 0–99, y `OrderService.Create` valida 1–99. Por confirmar: una cantidad de 11–99 pasa el filtro del servicio y probablemente termina en una excepción no controlada (500); no se ha ejecutado. Antes de cambiar límites, decide cuál es el correcto y alinea los cuatro sitios.
- **Idempotencia de compra:** cada cambio al carrito invalida `milan.checkout-key`; `OrderService.Create` usa `"{customerId}:{confirmationKey}"` y devuelve el pedido existente si la clave ya se usó.
- **Privacidad de pedidos:** consulta siempre con `customerId` (claim `NameIdentifier`, p. ej. `cliente-1`); un pedido ajeno responde 404.
- **`public partial class Program;`** al final de `Program.cs` es necesario para las pruebas de integración; no lo quites.
- **Redirecciones de login:** usa `SafeReturnUrl` en `Cuenta/Entrar` (solo rutas locales).
- **Sin persistencia:** reiniciar el proceso borra carritos y pedidos. No hay roles administrativos; ambas cuentas son "Cliente".

## Instrucciones adicionales

- Siempre que no encuentres algo incierto en el codigo marcalo como «por confirmar» explica el por que no lo puedes validar y que se propone para la validacion.