using JwtAuth.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuth.Controllers;
[Route("api/[controller]")]
//[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorised]
[AllowedAudiences("web", "mobile")]
public class BaseController : ControllerBase;
