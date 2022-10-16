using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace simple_web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger _logger;

    public IConfiguration Configuration {get; private set;}

    public IndexModel(IConfiguration config, ILogger<IndexModel> logger)
    {
        Configuration = config;
        _logger = logger;
    }

    public void OnGet()
    {

    }
}
