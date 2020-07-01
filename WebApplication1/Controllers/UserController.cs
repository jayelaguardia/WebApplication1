using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApplication1.Models;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace WebApplication1.Controllers
{
    public class UserController : ApiController
    {
        // GET api/user
        public IHttpActionResult Get()
        {
            IList<UserViewModel> students = null;

            using (var ctx = new testEntities1())
            {
                students = ctx.testusers.Include("StudentAddress")
                            .Select(s => new UserViewModel()
                            {
                                username = s.username,
                                password = s.password,
                                name = s.name
                            }).ToList<UserViewModel>();
            }

            if (students.Count == 0)
            {
                return NotFound();
            }

            return Ok(students);
        }

        //POST /api/user
        //sign up
        public IHttpActionResult Post(UserViewModel user)
        {
            //check valid incoming fields
            if(string.IsNullOrEmpty(user.username))
            {
                return BadRequest("Missing username in request body");
            }
            else if (string.IsNullOrEmpty(user.password))
            {
                return BadRequest("Missing password in request body");
            }
            else if (string.IsNullOrEmpty(user.name))
            {
                return BadRequest("Missing name in request body");
            }
            else if (!ModelState.IsValid)
                return BadRequest("Invalid data in request body");

            //connect to database
            using (var ctx = new testEntities1())
            {
                //check if username already exists
                var query = ctx.testusers
                            .Where(s => s.username == user.username)
                            .FirstOrDefault<testuser>();
                if (query != null)
                {
                    return BadRequest("This username already exists");
                }

                //password validation
                if (user.password.Length < 8)
                {
                    return BadRequest("Password must be longer than 8 characters");
                }
                if (user.password.Length > 72)
                {
                    return BadRequest("Password must be less than 72 characters");
                }
                if (user.password.StartsWith(" ") || user.password.EndsWith(" "))
                {
                    return BadRequest("Password must not start or end with empty spaces");
                }
                var rx = new Regex(@"(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&])[\S]+");
                if (rx.IsMatch(user.password) == false)
                {
                    return BadRequest("password must contain one upper case, lower case, number, and special character");
                }

                //hash password
                byte[] salt;
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
                var pbkdf2 = new Rfc2898DeriveBytes(user.password, salt, 10000);
                byte[] hash = pbkdf2.GetBytes(20);
                byte[] hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);
                string passwordHash = Convert.ToBase64String(hashBytes);

                //add new user to database
                ctx.testusers.Add(new testuser()
                {
                    //serialize new user
                    username = user.username,
                    password = passwordHash,
                    name = user.name
                });
                //save database with new info
                ctx.SaveChanges();
            }
            //return status 201 and message
            return Content(HttpStatusCode.Created, $"username: {user.username}, name: {user.name}");
        }
    }
}
