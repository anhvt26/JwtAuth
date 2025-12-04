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
public class BaseController : ControllerBase;
