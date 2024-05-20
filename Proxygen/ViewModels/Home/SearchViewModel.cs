using SharedModel.Model;

namespace Proxygen.ViewModels.Home;

public class SearchViewModel
{
    public required IReadOnlyDictionary<Card, int> CardAmounts { get; init; }
};
