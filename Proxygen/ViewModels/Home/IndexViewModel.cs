using System.ComponentModel.DataAnnotations;

namespace Proxygen.ViewModels.Home;

public class IndexViewModel
{
    [StringLength(2048)]
    public required string Decklist { get; set; }
};
