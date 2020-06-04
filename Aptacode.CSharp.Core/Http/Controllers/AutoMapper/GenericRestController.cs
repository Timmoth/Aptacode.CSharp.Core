﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Aptacode.CSharp.Utilities.Persistence;
using Aptacode.CSharp.Utilities.Persistence.UnitOfWork;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Aptacode.CSharp.Core.Http.Controllers.AutoMapper
{
    public class GenericRestController<TGetViewModel, TPutViewModel, TEntity> : AutoMapperGenericController
        where TEntity : IEntity
    {
        public GenericRestController(IGenericUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }

        #region HttpMethods

        [HttpPost("{id}")]
        public async Task<ActionResult<TGetViewModel>> Post(int id, [FromBody] TPutViewModel viewModel)
        {
            return await base.Post<TGetViewModel, TPutViewModel, TEntity>(id, viewModel).ConfigureAwait(false);
        }

        [HttpPut]
        public async Task<ActionResult<TGetViewModel>> Put([FromBody] TPutViewModel viewModel)
        {
            return await base.Put<TGetViewModel, TPutViewModel, TEntity>(viewModel).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TGetViewModel>>> Get()
        {
            return await base.Get<TGetViewModel, TEntity>().ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TGetViewModel>> Get(int id)
        {
            return await base.Get<TGetViewModel, TEntity>(id).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await base.Delete<TEntity>(id).ConfigureAwait(false);
        }

        #endregion
    }
}