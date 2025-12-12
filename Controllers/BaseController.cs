using JwtAuth.Attributtes;
using JwtAuth.Identity;
using JwtAuth.ResponseWrapping;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuth.Controllers;
[Route("api/[controller]")]
[ApiController]
[WrapResponse]
[Authorised]
[AllowedAudiences("web", "mobile")]
[NormalizeEmptyStrings]
public class BaseController : ControllerBase;
