﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsStatisticService.WebApi.ActionFilters;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.RolesAndPermissions
{

    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRolesRepository _rolesRepository;
        private readonly IRolesCommandHandler _rolesCommandHandler;
        public RolesController(
            IRolesRepository rolesRepository, 
            IRolesCommandHandler rolesCommandHandler)
        {
            _rolesRepository = rolesRepository;
            _rolesCommandHandler = rolesCommandHandler;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int? limit, [FromQuery] int? offset)
        {
            var roles = await _rolesRepository.GetAllRoles(null, limit, offset);
            return Ok(roles);
        }

        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetOne([FromRoute] string roleId)
        {
            return Ok(await _rolesRepository.GetRole(roleId));
        }

        [HttpPost]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> Create([FromBody] Role role)
        {
            await _rolesCommandHandler.CreateRole(role);
            return Ok();
        }

        [HttpDelete]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> Delete([FromQuery] string roleId)
        {
            await _rolesCommandHandler.DeleteRole(roleId);
            return Ok();
        }

        [HttpPut]
        [CheckIfSuperAdmin]
        public async Task<IActionResult> Update([FromBody] Role role)
        {
            await _rolesCommandHandler.UpdateRole(role);
            return Ok();
        }
    }
}