using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using BasketApi.Contracts;
using BasketApi.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BasketApi.ApiControllers
{
    /// <summary>
    /// Performs username/password authentication and issues a JWT token.
    /// <remarks>JWT token contains userId in claims</remarks>
    /// </summary>
    [Route("api/token")]
    [AllowAnonymous]
    public class TokenController : Controller
    {
        private readonly IAuthenticationService _authenticationService;

        public TokenController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost]
        public IActionResult Create([FromBody]CredentialsModel credentialsModel)
        {
            if(!_authenticationService.Authenticate(credentialsModel.Username, credentialsModel.Password, out var userMembershipClaims))
                return Unauthorized();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, "BasketApi"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }.Union(userMembershipClaims).ToList();

            var token =  new JwtSecurityToken(
                issuer: AuthenticationServiceStub.Issuer,
                audience: AuthenticationServiceStub.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(
                    JwtSecurityKey.Create(AuthenticationServiceStub.SecretValue),
                    SecurityAlgorithms.HmacSha256));

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(tokenValue);
        }
    }
}
