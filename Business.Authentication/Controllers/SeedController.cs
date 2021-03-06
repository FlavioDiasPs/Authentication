﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authentication.Extensions;
using Authentication.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers
{
    [Route("dev/seed")]
    public class SeedController : Controller
    {
        private readonly UserManager<BusinessUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedController(UserManager<BusinessUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            // Get the list of the roles from the enum
            Role[] roles = (Role[])Enum.GetValues(typeof(Role));
            foreach (var r in roles)
            {
                // Create an identity role object out of the enum value
                var identityRole = new IdentityRole
                {
                    Id = r.GetRoleName(),
                    Name = r.GetRoleName()
                };

                // Create the role if it doesn't already exist
                if (!await _roleManager.RoleExistsAsync(roleName: identityRole.Name))
                {
                    var result = await _roleManager.CreateAsync(identityRole);

                    // Return 500 if it fails
                    if (!result.Succeeded)
                        return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }

            // Our default user
            BusinessUser user = new BusinessUser
            {
                //FullName = "Jane Doe",
                Email = "janedoe@example.com",
                UserName = "janedoe@example.com",
                LockoutEnabled = false
            };

            // Add the user to the database if it doesn't already exist
            if (await _userManager.FindByEmailAsync(user.Email) == null)
            {
                // WARNING: Do NOT check in credentials of any kind into source control
                var result = await _userManager.CreateAsync(user, password: "5ESTdYB5cyYwA2dKhJqyjPYnKUc&45Ydw^gz^jy&FCV3gxpmDPdaDmxpMkhpp&9TRadU%wQ2TUge!TsYXsh77Qmauan3PEG8!6EP");

                if (!result.Succeeded) // Return 500 if it fails
                    return StatusCode(StatusCodes.Status500InternalServerError);

                // Assign all roles to the default user
                result = await _userManager.AddToRolesAsync(user, roles.Select(r => r.GetRoleName()));

                if (!result.Succeeded) // Return 500 if it fails
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }

            // All good, 200 OK!            
            return Ok();
        }
    }
}