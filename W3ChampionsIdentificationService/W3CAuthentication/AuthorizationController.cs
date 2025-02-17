﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.Twitch;

namespace W3ChampionsIdentificationService.W3CAuthentication
{

    [ApiController]
    [Route("api/oauth")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IBlizzardAuthenticationService _blizzardAuthenticationService;
        private readonly ITwitchAuthenticationService _twitchAuthenticationService;
        private readonly IUsersRepository _usersRepository;
        private readonly IRolesRepository _rolesRepository;

        private static readonly string JwtPrivateKey = Regex.Unescape(Environment.GetEnvironmentVariable("JWT_PRIVATE_KEY") ?? "");
        private static readonly string JwtPublicKey = Regex.Unescape(Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY") ?? "");

        public AuthorizationController(
            IBlizzardAuthenticationService blizzardAuthenticationService,
            ITwitchAuthenticationService twitchAuthenticationService,
            IUsersRepository usersRepository,
            IRolesRepository rolesRepository)
        {
            _blizzardAuthenticationService = blizzardAuthenticationService;
            _twitchAuthenticationService = twitchAuthenticationService;
            _usersRepository = usersRepository;
            _rolesRepository = rolesRepository;
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetBlizzardToken(
            [FromQuery] string code,
            [FromQuery] string redirectUri,
            [FromQuery] BnetRegion region)
        {
            var token = await _blizzardAuthenticationService.GetToken(code, redirectUri, region);
            if (token == null)
            {
                return Unauthorized("Sorry H4ckerb0i");
            }

            var userInfo = await _blizzardAuthenticationService.GetUser(token.access_token, region);
            if (userInfo == null)
            {
                return Unauthorized("Sorry H4ckerb0i");
            }

            var user = await _usersRepository.GetUser(userInfo.battletag);
            var roles = user != null ? await _rolesRepository.GetAllRoles(x => user.Roles.Contains(x.Id)) : new List<Role>();
            var permissions = roles.Count > 0 ? roles.SelectMany(x => x.Permissions).Distinct().ToList() : new List<string>();

            var w3User = W3CUserAuthentication.Create(userInfo.battletag, JwtPrivateKey, permissions);

            return Ok(w3User);
        }

        [HttpGet("user-info")]
        public IActionResult GetUserInfo([FromQuery] string jwt)
        {
            var user = W3CUserAuthentication.FromJWT(jwt, JwtPublicKey);
            return user != null ? (IActionResult) Ok(user) : Unauthorized("Sorry Hackerboi");
        }

        [HttpGet("twitch")]
        public async Task<IActionResult> GetTwitchToken()
        {
            var token = await _twitchAuthenticationService.GetToken();
            return Ok(token);
        }
    }
}