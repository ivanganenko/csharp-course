using Module05.Tasks;

namespace Module05.Tests;

public class VehicleTests
{
    [Fact]
    public void Vehicle_OwnMethods()
    {
        var vehicle = new Vehicle();

        Assert.Equal("Vehicle", vehicle.Kind());
        Assert.Equal("unknown", vehicle.Wheels());
    }

    [Fact]
    public void Kind_ThroughBaseReference_UsesRuntimeType()
    {
        Vehicle vehicle = new Car();

        Assert.Equal("Car", vehicle.Kind());
    }

    [Fact]
    public void Kind_SportsCarThroughAnyReference_IsSportsCar()
    {
        Vehicle asVehicle = new SportsCar();
        Car asCar = new SportsCar();

        Assert.Equal("SportsCar", asVehicle.Kind());
        Assert.Equal("SportsCar", asCar.Kind());
    }

    [Fact]
    public void Wheels_ThroughCarReference_IsFour()
    {
        Car car = new Car();
        Car sportsCar = new SportsCar();

        Assert.Equal("4", car.Wheels());
        Assert.Equal("4", sportsCar.Wheels());
    }

    [Fact]
    public void Wheels_ThroughVehicleReference_UsesCompileTimeType()
    {
        Vehicle vehicle = new Car();

        // new (method hiding) не участвует в полиморфизме: метод выбирается по типу ПЕРЕМЕННОЙ.
        Assert.Equal("unknown", vehicle.Wheels());
    }

    [Fact]
    public void Describe_MixesVirtualAndHiddenMethods()
    {
        // Describe объявлен в Vehicle: Kind() — виртуальный (берётся из Car), Wheels() — нет (берётся из Vehicle).
        Assert.Equal("Car with unknown wheels", new Car().Describe());
        Assert.Equal("SportsCar with unknown wheels", new SportsCar().Describe());
        Assert.Equal("Vehicle with unknown wheels", new Vehicle().Describe());
    }
}
