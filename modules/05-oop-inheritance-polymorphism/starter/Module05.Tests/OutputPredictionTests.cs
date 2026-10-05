using Module05.Tasks;

namespace Module05.Tests;

// Первый аргумент — что код РЕАЛЬНО возвращает, второй — твоё предсказание из OutputPredictions.cs.
public class OutputPredictionTests
{
    [Fact]
    public void ConstructorOrder_PredictionMatches()
    {
        Assert.Equal(ConstructorOrderPuzzle.Run(), OutputPredictions.ConstructorOrder);
    }

    [Fact]
    public void NewVsOverride_PredictionMatches()
    {
        Assert.Equal(NewVsOverridePuzzle.Run(), OutputPredictions.NewVsOverride);
    }

    [Fact]
    public void OverloadVsOverride_PredictionMatches()
    {
        Assert.Equal(OverloadVsOverridePuzzle.Run(), OutputPredictions.OverloadVsOverride);
    }

    [Fact]
    public void IsAndAs_PredictionMatches()
    {
        Assert.Equal(IsAsPuzzle.Run(), OutputPredictions.IsAndAs);
    }
}
