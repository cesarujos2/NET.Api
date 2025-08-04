# Guía Completa de Unit Testing en .NET

## 📚 Índice
1. [¿Qué son los Unit Tests?](#qué-son-los-unit-tests)
2. [Configuración del Proyecto](#configuración-del-proyecto)
3. [Estructura de un Unit Test](#estructura-de-un-unit-test)
4. [Patrones y Mejores Prácticas](#patrones-y-mejores-prácticas)
5. [Mocking y Dependencias](#mocking-y-dependencias)
6. [Proceso de Desarrollo TDD](#proceso-de-desarrollo-tdd)
7. [Herramientas y Frameworks](#herramientas-y-frameworks)
8. [Ejemplos Prácticos](#ejemplos-prácticos)

---

## ¿Qué son los Unit Tests?

Los **Unit Tests** (pruebas unitarias) son pruebas automatizadas que verifican el comportamiento de una unidad específica de código (generalmente un método o clase) de forma aislada.

### 🎯 Objetivos principales:
- **Verificar funcionalidad**: Asegurar que el código hace lo que debe hacer
- **Detectar regresiones**: Identificar cuando cambios rompen funcionalidad existente
- **Documentar comportamiento**: Los tests sirven como documentación viva
- **Facilitar refactoring**: Permiten cambiar código con confianza

### ✅ Características de un buen Unit Test:
- **Rápido**: Se ejecuta en milisegundos
- **Independiente**: No depende de otros tests
- **Repetible**: Produce el mismo resultado siempre
- **Auto-verificable**: Pasa o falla claramente
- **Oportuno**: Se escribe junto con el código de producción

---

## Configuración del Proyecto

### 1. Crear proyecto de tests
```bash
# Crear proyecto de Unit Tests
dotnet new xunit -n MiProyecto.UnitTests

# Agregar referencia al proyecto principal
dotnet add MiProyecto.UnitTests reference MiProyecto
```

### 2. Paquetes NuGet esenciales
```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="AutoFixture" Version="4.18.0" />
```

### 3. Estructura de carpetas recomendada
```
MiProyecto.UnitTests/
├── Application/
│   ├── Services/
│   └── Features/
├── Domain/
│   ├── Entities/
│   └── Services/
├── Infrastructure/
│   ├── Services/
│   └── Repositories/
└── Shared/
    └── Constants/
```

---

## Estructura de un Unit Test

### Patrón AAA (Arrange-Act-Assert)

```csharp
[Fact]
public void MetodoAProbar_Condicion_ResultadoEsperado()
{
    // Arrange (Preparar)
    var input = "valor de entrada";
    var expectedResult = "resultado esperado";
    var service = new MiServicio();
    
    // Act (Actuar)
    var result = service.MetodoAProbar(input);
    
    // Assert (Verificar)
    result.Should().Be(expectedResult);
}
```

### Convenciones de nomenclatura

```csharp
// Formato: MetodoAProbar_Escenario_ResultadoEsperado
[Fact]
public void CalcularDescuento_ConClientePremium_DeberiaAplicar20PorCiento()

[Fact]
public void ValidarEmail_ConFormatoInvalido_DeberiaRetornarFalse()

[Fact]
public void CrearUsuario_ConDatosValidos_DeberiaRetornarUsuarioCreado()
```

---

## Patrones y Mejores Prácticas

### 1. Un test, una responsabilidad
```csharp
// ❌ MAL: Prueba múltiples cosas
[Fact]
public void ProcesarPedido_DeberiaValidarYGuardarYEnviarEmail()
{
    // Demasiadas responsabilidades
}

// ✅ BIEN: Una responsabilidad por test
[Fact]
public void ProcesarPedido_ConDatosValidos_DeberiaGuardarEnBaseDatos()

[Fact]
public void ProcesarPedido_ConDatosValidos_DeberiaEnviarEmailConfirmacion()
```

### 2. Usar Theory para múltiples casos
```csharp
[Theory]
[InlineData("admin@test.com", true)]
[InlineData("user@domain.co", true)]
[InlineData("invalid-email", false)]
[InlineData("", false)]
[InlineData(null, false)]
public void ValidarEmail_ConDiferentesFormatos_DeberiaRetornarResultadoCorrecto(
    string email, bool esperado)
{
    // Arrange
    var validator = new EmailValidator();
    
    // Act
    var resultado = validator.EsValido(email);
    
    // Assert
    resultado.Should().Be(esperado);
}
```

### 3. Usar FluentAssertions para assertions claras
```csharp
// ❌ Assertions básicas
Assert.True(result);
Assert.Equal(expected, actual);
Assert.NotNull(user);

// ✅ FluentAssertions
result.Should().BeTrue();
actual.Should().Be(expected);
user.Should().NotBeNull();
user.Name.Should().Be("Juan");
user.Age.Should().BeGreaterThan(18);
list.Should().HaveCount(3).And.Contain(x => x.Id == 1);
```

---

## Mocking y Dependencias

### ¿Cuándo usar Mocks?
- Cuando la clase tiene dependencias externas (base de datos, APIs, archivos)
- Para aislar la unidad bajo prueba
- Para simular diferentes escenarios (errores, timeouts, etc.)

### Ejemplo con Moq
```csharp
public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly UsuarioService _service;
    
    public UsuarioServiceTests()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _emailMock = new Mock<IEmailService>();
        _service = new UsuarioService(_repositoryMock.Object, _emailMock.Object);
    }
    
    [Fact]
    public async Task CrearUsuario_ConDatosValidos_DeberiaGuardarYEnviarEmail()
    {
        // Arrange
        var usuario = new Usuario { Email = "test@test.com", Nombre = "Juan" };
        _repositoryMock.Setup(x => x.GuardarAsync(It.IsAny<Usuario>()))
                      .ReturnsAsync(usuario);
        
        // Act
        var resultado = await _service.CrearUsuarioAsync(usuario);
        
        // Assert
        resultado.Should().NotBeNull();
        _repositoryMock.Verify(x => x.GuardarAsync(usuario), Times.Once);
        _emailMock.Verify(x => x.EnviarBienvenidaAsync(usuario.Email), Times.Once);
    }
}
```

### Configurar comportamientos del Mock
```csharp
// Retornar valor específico
_mock.Setup(x => x.ObtenerPorId(1)).Returns(usuario);

// Retornar valor basado en parámetro
_mock.Setup(x => x.ObtenerPorId(It.IsAny<int>()))
     .Returns<int>(id => new Usuario { Id = id });

// Simular excepción
_mock.Setup(x => x.GuardarAsync(It.IsAny<Usuario>()))
     .ThrowsAsync(new DatabaseException("Error de conexión"));

// Verificar que se llamó
_mock.Verify(x => x.GuardarAsync(It.IsAny<Usuario>()), Times.Once);
_mock.Verify(x => x.EliminarAsync(It.IsAny<int>()), Times.Never);
```

---

## Proceso de Desarrollo TDD

### Ciclo Red-Green-Refactor

1. **🔴 RED**: Escribir un test que falle
2. **🟢 GREEN**: Escribir el mínimo código para que pase
3. **🔵 REFACTOR**: Mejorar el código manteniendo los tests verdes

### Ejemplo práctico:

#### Paso 1: Test que falla (RED)
```csharp
[Fact]
public void CalcularDescuento_ConClientePremium_DeberiaAplicar20PorCiento()
{
    // Arrange
    var calculadora = new CalculadoraDescuento();
    var cliente = new Cliente { EsPremium = true };
    var monto = 100m;
    
    // Act
    var descuento = calculadora.Calcular(cliente, monto);
    
    // Assert
    descuento.Should().Be(20m);
}
```

#### Paso 2: Código mínimo (GREEN)
```csharp
public class CalculadoraDescuento
{
    public decimal Calcular(Cliente cliente, decimal monto)
    {
        if (cliente.EsPremium)
            return monto * 0.2m;
        return 0;
    }
}
```

#### Paso 3: Refactorizar (REFACTOR)
```csharp
public class CalculadoraDescuento
{
    private const decimal DESCUENTO_PREMIUM = 0.2m;
    
    public decimal Calcular(Cliente cliente, decimal monto)
    {
        return cliente.EsPremium ? monto * DESCUENTO_PREMIUM : 0;
    }
}
```

---

## Herramientas y Frameworks

### Testing Frameworks
- **xUnit**: Framework principal para .NET
- **NUnit**: Alternativa popular
- **MSTest**: Framework de Microsoft

### Assertion Libraries
- **FluentAssertions**: Assertions más legibles
- **Shouldly**: Otra opción para assertions fluidas

### Mocking Frameworks
- **Moq**: El más popular en .NET
- **NSubstitute**: Sintaxis más simple
- **FakeItEasy**: Otra alternativa

### Generación de datos
- **AutoFixture**: Genera datos de prueba automáticamente
- **Bogus**: Genera datos falsos realistas

---

## Ejemplos Prácticos

### Test de Entidad de Dominio
```csharp
public class UsuarioTests
{
    [Fact]
    public void CrearUsuario_ConDatosValidos_DeberiaInicializarCorrectamente()
    {
        // Arrange
        var email = "test@test.com";
        var nombre = "Juan Pérez";
        
        // Act
        var usuario = new Usuario(email, nombre);
        
        // Assert
        usuario.Email.Should().Be(email);
        usuario.Nombre.Should().Be(nombre);
        usuario.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        usuario.EstaActivo.Should().BeTrue();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CrearUsuario_ConEmailInvalido_DeberiaLanzarExcepcion(string emailInvalido)
    {
        // Arrange & Act & Assert
        Action act = () => new Usuario(emailInvalido, "Nombre");
        act.Should().Throw<ArgumentException>()
           .WithMessage("El email es requerido");
    }
}
```

### Test de Servicio con Dependencias
```csharp
public class PedidoServiceTests
{
    private readonly Mock<IPedidoRepository> _repositoryMock;
    private readonly Mock<IInventarioService> _inventarioMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly PedidoService _service;
    
    public PedidoServiceTests()
    {
        _repositoryMock = new Mock<IPedidoRepository>();
        _inventarioMock = new Mock<IInventarioService>();
        _emailMock = new Mock<IEmailService>();
        _service = new PedidoService(
            _repositoryMock.Object,
            _inventarioMock.Object,
            _emailMock.Object);
    }
    
    [Fact]
    public async Task ProcesarPedido_ConStockSuficiente_DeberiaCrearPedido()
    {
        // Arrange
        var pedido = new Pedido
        {
            ProductoId = 1,
            Cantidad = 5,
            ClienteEmail = "cliente@test.com"
        };
        
        _inventarioMock.Setup(x => x.TieneStockAsync(1, 5))
                      .ReturnsAsync(true);
        _repositoryMock.Setup(x => x.GuardarAsync(It.IsAny<Pedido>()))
                      .ReturnsAsync(pedido);
        
        // Act
        var resultado = await _service.ProcesarPedidoAsync(pedido);
        
        // Assert
        resultado.Should().NotBeNull();
        _inventarioMock.Verify(x => x.ReservarStockAsync(1, 5), Times.Once);
        _emailMock.Verify(x => x.EnviarConfirmacionAsync(pedido.ClienteEmail), Times.Once);
    }
    
    [Fact]
    public async Task ProcesarPedido_SinStock_DeberiaLanzarExcepcion()
    {
        // Arrange
        var pedido = new Pedido { ProductoId = 1, Cantidad = 5 };
        _inventarioMock.Setup(x => x.TieneStockAsync(1, 5))
                      .ReturnsAsync(false);
        
        // Act & Assert
        var act = async () => await _service.ProcesarPedidoAsync(pedido);
        await act.Should().ThrowAsync<StockInsuficienteException>();
        
        _repositoryMock.Verify(x => x.GuardarAsync(It.IsAny<Pedido>()), Times.Never);
    }
}
```

---

## 🚀 Proceso de Desarrollo Incremental

### 1. Empezar simple
```csharp
// Primer test: caso más básico
[Fact]
public void Sumar_DosNumeros_DeberiaRetornarSuma()
{
    var calculadora = new Calculadora();
    var resultado = calculadora.Sumar(2, 3);
    resultado.Should().Be(5);
}
```

### 2. Agregar casos edge
```csharp
[Theory]
[InlineData(0, 0, 0)]
[InlineData(-1, 1, 0)]
[InlineData(int.MaxValue, 1, int.MinValue)] // Overflow
public void Sumar_CasosEspeciales_DeberiaFuncionar(int a, int b, int esperado)
{
    var calculadora = new Calculadora();
    var resultado = calculadora.Sumar(a, b);
    resultado.Should().Be(esperado);
}
```

### 3. Refactorizar y mejorar
```csharp
public class CalculadoraTests
{
    private readonly Calculadora _calculadora;
    
    public CalculadoraTests()
    {
        _calculadora = new Calculadora();
    }
    
    [Theory]
    [MemberData(nameof(DatosSuma))]
    public void Sumar_ConDiferentesValores_DeberiaRetornarResultadoCorrecto(
        int a, int b, int esperado)
    {
        var resultado = _calculadora.Sumar(a, b);
        resultado.Should().Be(esperado);
    }
    
    public static IEnumerable<object[]> DatosSuma =>
        new List<object[]>
        {
            new object[] { 2, 3, 5 },
            new object[] { 0, 0, 0 },
            new object[] { -1, 1, 0 },
            new object[] { 100, -50, 50 }
        };
}
```

---

## 📋 Checklist para Unit Tests

### ✅ Antes de escribir el test:
- [ ] ¿Entiendo qué debe hacer el método?
- [ ] ¿Cuáles son los casos de uso principales?
- [ ] ¿Qué casos edge debo considerar?
- [ ] ¿Qué dependencias necesito mockear?

### ✅ Al escribir el test:
- [ ] Nombre descriptivo que explique el escenario
- [ ] Sigue el patrón AAA (Arrange-Act-Assert)
- [ ] Una sola responsabilidad por test
- [ ] Assertions claras y específicas

### ✅ Después de escribir el test:
- [ ] El test falla cuando debería fallar
- [ ] El test pasa cuando el código es correcto
- [ ] Es rápido (< 100ms)
- [ ] Es independiente de otros tests
- [ ] Es fácil de entender

---

## 🎯 Métricas y Cobertura

### Ejecutar tests con cobertura
```bash
# Instalar herramienta de cobertura
dotnet tool install --global dotnet-reportgenerator-globaltool

# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Generar reporte HTML
reportgenerator -reports:"**\coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:Html
```

### Interpretar métricas
- **Line Coverage**: % de líneas ejecutadas
- **Branch Coverage**: % de ramas (if/else) ejecutadas
- **Method Coverage**: % de métodos ejecutados

### Objetivos recomendados
- **80%+ cobertura** para código crítico
- **60%+ cobertura** para código general
- **100% cobertura** para lógica de negocio compleja

---

## 🔧 Comandos Útiles

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests con detalles
dotnet test --verbosity normal

# Ejecutar tests de una clase específica
dotnet test --filter "FullyQualifiedName~UsuarioServiceTests"

# Ejecutar tests que contengan una palabra
dotnet test --filter "Name~Crear"

# Ejecutar tests en paralelo
dotnet test --parallel

# Ver tests disponibles sin ejecutar
dotnet test --list-tests
```

---

## 📚 Recursos Adicionales

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Microsoft Testing Guidelines](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [Clean Code: Unit Tests](https://blog.cleancoder.com/uncle-bob/2017/05/05/TestDefinitions.html)

---

*Esta guía te ayudará a dominar el arte del unit testing en .NET. ¡Practica regularmente y verás cómo mejora la calidad de tu código!* 🚀