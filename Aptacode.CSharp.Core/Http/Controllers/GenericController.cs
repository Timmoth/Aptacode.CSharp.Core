﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Aptacode.CSharp.Common.Persistence;
using Aptacode.CSharp.Common.Persistence.Repository;
using Aptacode.CSharp.Common.Persistence.Specification;
using Aptacode.CSharp.Common.Persistence.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace Aptacode.CSharp.Core.Http.Controllers
{
    /// <summary>
    ///     Provides a collection of generic Http methods for querying &
    ///     returning entities from an IRepository contained within the given IGenericUnitOfWork
    /// </summary>
    public abstract class GenericController : ControllerBase
    {
        protected readonly GenericUnitOfWork UnitOfWork;

        protected GenericController(GenericUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork ?? throw new NullReferenceException("IGenericUnitOfWork was null");
        }

        /// <summary>
        ///     Finds and updates the given entity in the matching IRepository<T> from the IGenericUnitOfWork
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <param name="validator"></param>
        /// <returns></returns>
        protected virtual async Task<ServerResponse<T>> Post<TKey, T>(TKey id, T entity, Validator<T> validator = null)
            where T : IEntity<TKey>
        {
            if (entity == null)
            {
                return new ServerResponse<T>(HttpStatusCode.BadRequest, "Null Entity was given");
            }

            if (!id.Equals(entity.Id))
            {
                return new ServerResponse<T>(HttpStatusCode.BadRequest, "Entity's Id did not match");
            }

            if (validator != null)
            {
                var result = await validator(entity).ConfigureAwait(false);
                if (!result.HasValue || !result.Value)
                {
                    return new ServerResponse<T>(result.StatusCode, result.Message);
                }
            }

            try
            {
                await UnitOfWork.Get<IGenericAsyncRepository<TKey, T>>().UpdateAsync(entity).ConfigureAwait(false);
                await UnitOfWork.Commit().ConfigureAwait(false);
                return new ServerResponse<T>(HttpStatusCode.OK, "Success", entity);
            }
            catch
            {
                return new ServerResponse<T>(HttpStatusCode.BadRequest, "DataBase Error");
            }
        }

        /// <summary>
        ///     Inserts a new entity in the matching IRepository<T> from the IGenericUnitOfWork
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="entity"></param>
        /// <param name="validator"></param>
        /// <returns></returns>
        protected virtual async Task<ServerResponse<T>> Put<TKey, T>(T entity, Validator<T> validator = null)
            where T : IEntity<TKey>
        {
            if (entity == null)
            {
                return new ServerResponse<T>(HttpStatusCode.BadRequest, "Null Entity was given");
            }

            if (validator != null)
            {
                var result = await validator(entity).ConfigureAwait(false);
                if (!result.HasValue || !result.Value)
                {
                    return new ServerResponse<T>(result.StatusCode, result.Message);
                }
            }

            try
            {
                await UnitOfWork.Get<IGenericAsyncRepository<TKey, T>>().CreateAsync(entity).ConfigureAwait(false);
                await UnitOfWork.Commit().ConfigureAwait(false);
                return new ServerResponse<T>(HttpStatusCode.OK, "Success", entity);
            }
            catch
            {
                return new ServerResponse<T>(HttpStatusCode.BadRequest, "DataBase Error");
            }
        }

        /// <summary>
        ///     Returns a collection of entities found to match the queryExpression in the matching IRepository
        ///     <T> from the IGenericUnitOfWork
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="specification"></param>
        /// <param name="validator"></param>
        /// <returns></returns>
        protected virtual async Task<ServerResponse<IEnumerable<T>>> Get<TKey, T>(
            Specification<T> specification = null, Validator validator = null)
            where T : IEntity<TKey>
        {
            if (validator != null)
            {
                var result = await validator().ConfigureAwait(false);
                if (!result.HasValue || !result.Value)
                {
                    return new ServerResponse<IEnumerable<T>>(result.StatusCode, result.Message);
                }
            }

            try
            {
                IEnumerable<T> results;

                if (specification == null)
                {
                    results = await UnitOfWork.Get<IGenericAsyncRepository<TKey, T>>().GetAllAsync()
                        .ConfigureAwait(false);
                }
                else
                {
                    results = await UnitOfWork.Get<ISpecificationAsyncRepository<TKey, T>>().GetAsync(specification)
                        .ConfigureAwait(false);
                }

                return new ServerResponse<IEnumerable<T>>(HttpStatusCode.OK, "Success", results);
            }
            catch
            {
                return new ServerResponse<IEnumerable<T>>(HttpStatusCode.BadRequest, "DataBase Error");
            }
        }

        /// <summary>
        ///     Returns the requested entity from the matching IRepository<T> within the IGenericUnitOfWork
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="validator"></param>
        /// <returns></returns>
        protected virtual async Task<ServerResponse<T>> Get<TKey, T>(TKey id,
            Validator<TKey> validator = null) where T : IEntity<TKey>
        {
            if (validator != null)
            {
                var result = await validator(id).ConfigureAwait(false);
                if (!result.HasValue || !result.Value)
                {
                    return new ServerResponse<T>(result.StatusCode, result.Message);
                }
            }

            try
            {
                var result = await UnitOfWork.Get<IGenericAsyncRepository<TKey, T>>().GetAsync(id)
                    .ConfigureAwait(false);
                return result != null
                    ? new ServerResponse<T>(HttpStatusCode.OK, "Success", result)
                    : new ServerResponse<T>(HttpStatusCode.BadRequest, "Not Found");
            }
            catch
            {
                return new ServerResponse<T>(HttpStatusCode.BadRequest, "DataBase Error");
            }
        }

        /// <summary>
        ///     Deletes the requested entity from the matching IRepository<T> within the IGenericUnitOfWork
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <param name="validator"></param>
        /// <returns></returns>
        protected virtual async Task<ServerResponse<bool>> Delete<TKey, TEntity>(TKey id,
            Validator<TKey> validator = null) where TEntity : IEntity<TKey>
        {
            if (validator != null)
            {
                var result = await validator(id).ConfigureAwait(false);
                if (!result.HasValue || !result.Value)
                {
                    return new ServerResponse<bool>(result.StatusCode, result.Message);
                }
            }

            try
            {
                await UnitOfWork.Get<IGenericAsyncRepository<TKey, TEntity>>().DeleteAsync(id).ConfigureAwait(false);
                await UnitOfWork.Commit().ConfigureAwait(false);

                return new ServerResponse<bool>(HttpStatusCode.OK, "Success", true);
            }
            catch
            {
                return new ServerResponse<bool>(HttpStatusCode.BadRequest, "DataBase Error", false);
            }
        }

        /// <summary>
        ///     Converts the given ServerResponse<T> into an ActionResult<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        protected ActionResult<T> ToActionResult<T>(ServerResponse<T> response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return Ok(response.Value);
                case HttpStatusCode.BadRequest:
                    return BadRequest(response.Message);
                case HttpStatusCode.NotFound:
                    return NotFound(response.Message);
                default:
                    return BadRequest(response.Message);
            }
        }
    }
}