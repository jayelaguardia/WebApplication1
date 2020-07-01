using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AuthController : ApiController
    {
        //remember to move this secret to secure config. don't just leave it here
        private const string Secret = "db3OIsj+BXE9NZDy0t8W3TcNekrF+2d/1sFnWG4HnV8TZY30iTOdtVWJG8abWvB1GlOgJuQZdcF2Luqm/hccMw==";

        [Route("api/auth/token")]
        public string Get()
        {
            string merp = "auth works";
            return merp;
        }

        //POST /api/auth/token
        //login and receive auth token
        [Route("api/auth/token")]
        public IHttpActionResult Post(AuthViewModel user)
        {
            //check valid incoming fields
            if (string.IsNullOrEmpty(user.username))
            {
                return BadRequest("Missing username in request body");
            }
            else if (string.IsNullOrEmpty(user.password))
            {
                return BadRequest("Missing password in request body");
            }
            else if (!ModelState.IsValid)
                return BadRequest("Invalid data in request body");

            //connect to database
            using (var ctx = new testEntities1())
            {
                //check if user exists
                var query = ctx.testusers
                            .Where(s => s.username == user.username)
                            .FirstOrDefault<testuser>();

                //if user does not exist, return error
                if (query == null)
                {
                    return BadRequest("Incorrect username or password");
                }

                //if user does exists, check pass
                byte[] savedPasswordBytes = Convert.FromBase64String(query.password);
                byte[] salt = new byte[16];
                Array.Copy(savedPasswordBytes, 0, salt, 0, 16);
                var inputPass = new Rfc2898DeriveBytes(user.password, salt, 10000);
                byte[] inputHash = inputPass.GetBytes(20);
                for(int i = 0; i < 20; i++)
                {
                    if(savedPasswordBytes[i+16] != inputHash[i])
                        return BadRequest("Incorrect username or password");
                }

                //create auth
                var symmetricKey = Convert.FromBase64String(Secret);
                var tokenHandler = new JwtSecurityTokenHandler();

                var now = DateTime.UtcNow;
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.username)
                    }),

                    Expires = now.AddDays(1),

                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(symmetricKey),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var stoken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(stoken);

                //send auth
                return Ok(token);
            }
        }

        //PUT /api/auth/token
        //refresh auth token
        [Route("api/auth/token")]
        public IHttpActionResult Put(AuthViewModel user)
        {
            return Ok(user);
        }
    }
}
