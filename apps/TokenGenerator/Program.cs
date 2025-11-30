using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;


//THIS IS USED FOR TESTING Authorization and Authentication via the API gateway

var issuer = "bizcord-auth";
var audience = "bizcord-api";
var secretKey = "SuperDuperMegaHemmeligTestNøgle"; 

var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

var claims = new[]
{
    new Claim("sub", "user-123"),        
    new Claim("name", "Test User"),
    new Claim("role", "Admin"),
    new Claim("scope", "profiles.read")

};

var token = new JwtSecurityToken(
    issuer: issuer,
    audience: audience,
    claims: claims,
    notBefore: DateTime.UtcNow,
    expires: DateTime.UtcNow.AddHours(1),
    signingCredentials: credentials
);

var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

Console.WriteLine("JWT:");
Console.WriteLine(tokenString);