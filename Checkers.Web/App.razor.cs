using Avalonia.Web.Blazor;

namespace Checkers.Web;

public partial class App
{
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        WebAppBuilder.Configure<Checkers.App>()
            .SetupWithSingleViewLifetime();
    }
}