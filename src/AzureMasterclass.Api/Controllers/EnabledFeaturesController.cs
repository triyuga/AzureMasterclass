using AzureMasterclass.Domain;
using Microsoft.AspNetCore.Mvc;

namespace AzureMasterclass.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnabledFeaturesController : ControllerBase
{
    private readonly EnabledFeatures _enabledFeatures;

    public EnabledFeaturesController(EnabledFeatures enabledFeatures)
    {
        _enabledFeatures = enabledFeatures;
    }

    [HttpGet]
    public EnabledFeatures GetEnabledFeatures()
    {       
        return _enabledFeatures;
    }
}