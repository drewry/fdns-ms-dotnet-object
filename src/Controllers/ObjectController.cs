﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Foundation.ObjectService.Data;
using Foundation.ObjectService.ViewModel;
using Microsoft.AspNetCore.Authorization;

namespace Foundation.ObjectService.WebUI.Controllers
{
    /// <summary>
    /// Object service controller class
    /// </summary>
    [Route("api/1.0")]
    [ApiController]
    public class ObjectController : ControllerBase
    {
        private readonly IObjectRepository _repository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repository">The object repository to use for interacting with the underlying database</param>
        public ObjectController(IObjectRepository repository)
        {
            _repository = repository;
        }

        // GET api/1.0/db/collection/5
        /// <summary>
        /// Gets an object
        /// </summary>
        /// <param name="routeParameters">Required route parameters needed for the find operation</param>
        /// <returns>Object that has the matching id</returns>
        /// <response code="200">Returns the object that has a matching id</response>
        /// <response code="400">If the route parameters contain invalid data</response>
        /// <response code="401">If the HTTP header lacks a valid OAuth2 token</response>
        /// <response code="403">If the HTTP header has a valid OAuth2 token but lacks the appropriate scope to use this route</response>
        /// <response code="404">If an object with a matching id is not found</response>
        [Produces("text/plain")]
        [HttpGet("{db}/{collection}/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [Authorize(Common.READ_AUTHORIZATION_NAME)]
        public async Task<IActionResult> Get([FromRoute] ItemRouteParameters routeParameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var document = await _repository.GetAsync(routeParameters.DatabaseName, routeParameters.CollectionName, routeParameters.Id);
            if (document == null)
            {
                return NotFound();
            }
            return Ok(document);
        }

        // POST api/1.0/db/collection/6
        /// <summary>
        /// Inserts an object with a specified ID
        /// </summary>
        /// <remarks>
        /// Sample request to insert a new Json document with an id of 6:
        ///
        ///     POST /api/1.0/db/collection/6
        ///     {
        ///         "id": 6,
        ///         "status": "A",
        ///         "code": 200
        ///     }
        ///
        /// </remarks>
        /// <param name="routeParameters">Required route parameters needed for the operation</param>
        /// <param name="json">The Json representation of the object to insert</param>
        /// <param name="responseFormat">The format of the response type; defaults to 0</param>
        /// <returns>Object that was inserted</returns>
        /// <response code="201">Returns the object that was just created</response>
        /// <response code="400">If the route parameters or json payload contain invalid data</response>
        /// <response code="401">If the HTTP header lacks a valid OAuth2 token</response>
        /// <response code="403">If the HTTP header has a valid OAuth2 token but lacks the appropriate scope to use this route</response>
        /// <response code="406">If the content type is anything other than text/plain</response>
        /// <response code="413">If the Json payload is too large</response>
        [Consumes("text/plain")]
        [Produces("text/plain")]
        [HttpPost("{db}/{collection}/{id}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(406)]
        [ProducesResponseType(413)]
        [Authorize(Common.INSERT_AUTHORIZATION_NAME)]
        public async Task<IActionResult> InsertWithId([FromRoute] ItemRouteParameters routeParameters, [FromBody] string json, [FromQuery] ResponseFormat responseFormat)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var document = await _repository.InsertAsync(routeParameters.DatabaseName, routeParameters.CollectionName, routeParameters.Id, json);
            if (responseFormat == ResponseFormat.OnlyId)
            {
                document = routeParameters.Id;
            }
            return CreatedAtAction(nameof(Get), new { id = routeParameters.Id, db = routeParameters.DatabaseName, collection = routeParameters.CollectionName }, document);
        }

        // POST api/1.0/db/collection
        /// <summary>
        /// Inserts an object without a specified ID
        /// </summary>
        /// <remarks>
        /// Sample request to insert a new Json document:
        ///
        ///     POST /api/1.0/db/collection
        ///     {
        ///         "status": "A",
        ///         "code": 200
        ///     }
        ///
        /// </remarks>
        /// <param name="routeParameters">Required route parameters needed for the operation</param>
        /// <param name="json">The Json representation of the object to insert</param>
        /// <param name="responseFormat" default="0">The format of the response type; defaults to 0</param>
        /// <returns>Object that was inserted</returns>
        /// <response code="201">Returns the object that was just created</response>
        /// <response code="400">If the route parameters or json payload contain invalid data</response>
        /// <response code="401">If the HTTP header lacks a valid OAuth2 token</response>
        /// <response code="403">If the HTTP header has a valid OAuth2 token but lacks the appropriate scope to use this route</response>
        /// <response code="406">If the content type is anything other than text/plain</response>
        /// <response code="413">If the Json payload is too large</response>
        [Consumes("text/plain")]
        [Produces("text/plain")]
        [HttpPost("{db}/{collection}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(406)]
        [ProducesResponseType(413)]
        [Authorize(Common.INSERT_AUTHORIZATION_NAME)]
        public async Task<IActionResult> InsertWithNoId([FromRoute] DatabaseRouteParameters routeParameters, [FromBody] string json, [FromQuery] ResponseFormat responseFormat = ResponseFormat.EntireObject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var id = System.Guid.NewGuid().ToString();
            var document = await _repository.InsertAsync(routeParameters.DatabaseName, routeParameters.CollectionName, id, json);
            if (responseFormat == ResponseFormat.OnlyId)
            {
                document = id;
            }
            return CreatedAtAction(nameof(Get), new { id = id, db = routeParameters.DatabaseName, collection = routeParameters.CollectionName }, document);
        }

        // PUT api/1.0/db/collection/5
        /// <summary>
        /// Updates an object
        /// </summary>
        /// <remarks>
        /// Sample request to conduct a wholesale replacement of the object with an id of 6:
        ///
        ///     PUT /api/1.0/db/collection/6
        ///     {
        ///         "id": 6,
        ///         "status": "D",
        ///         "code": 400
        ///     }
        ///
        /// </remarks>
        /// <param name="routeParameters">Required route parameters needed for the operation</param>
        /// <param name="json">The Json representation of the object to update</param>
        /// <param name="responseFormat">The format of the response type; defaults to 0</param>
        /// <returns>The updated object</returns>
        /// <response code="200">Returns the object that was just updated</response>
        /// <response code="400">If the route parameters or json payload contain invalid data</response>
        /// <response code="401">If the HTTP header lacks a valid OAuth2 token</response>
        /// <response code="403">If the HTTP header has a valid OAuth2 token but lacks the appropriate scope to use this route</response>
        /// <response code="404">If an object with this id is not found in the collection</response>
        /// <response code="406">If the content type is anything other than text/plain</response>
        /// <response code="413">If the Json payload is too large</response>
        [Consumes("text/plain")]
        [Produces("text/plain")]
        [HttpPut("{db}/{collection}/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(406)]
        [ProducesResponseType(413)]
        [Authorize(Common.UPDATE_AUTHORIZATION_NAME)]
        public async Task<IActionResult> Put([FromRoute] ItemRouteParameters routeParameters, [FromBody] string json, [FromQuery] ResponseFormat responseFormat = ResponseFormat.EntireObject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var document = await _repository.ReplaceAsync(routeParameters.DatabaseName, routeParameters.CollectionName, routeParameters.Id, json);
            if (string.IsNullOrEmpty(document))
            {
                return NotFound();
            }
            if (responseFormat == ResponseFormat.OnlyId)
            {
                document = routeParameters.Id;
            }
            return Ok(document);
        }

        // DELETE api/1.0/db/collection/5
        /// <summary>
        /// Deletes an object
        /// </summary>
        /// <param name="routeParameters">Required route parameters needed for the operation</param>
        /// <returns>Whether the item was deleted or not</returns>
        /// <response code="200">Deletion successful</response>
        /// <response code="400">If the route parameters contain invalid data</response>
        /// <response code="401">If the HTTP header lacks a valid OAuth2 token</response>
        /// <response code="403">If the HTTP header has a valid OAuth2 token but lacks the appropriate scope to use this route</response>
        /// <response code="404">If an object with this id is not found in the collection</response>
        [Produces("text/plain")]
        [HttpDelete("{db}/{collection}/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [Authorize(Common.DELETE_AUTHORIZATION_NAME)]
        public async Task<IActionResult> Delete([FromRoute] ItemRouteParameters routeParameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var deleted = await _repository.DeleteAsync(routeParameters.DatabaseName, routeParameters.CollectionName, routeParameters.Id);
            if (deleted)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        // POST api/1.0/db/collection/find
        /// <summary>
        /// Finds one or more objects that match the specified criteria
        /// </summary>
        /// <remarks>
        /// Sample request to find one or more documents with a status of either 'A' or 'D' and that have a code equal to 400:
        ///
        ///     POST /api/1.0/db/collection/find
        ///     {
        ///         status:
        ///         {
        ///             $in: [ "A", "D" ]
        ///         },
        ///         code: 400
        ///     }
        ///
        /// </remarks>
        /// <param name="findExpression">The Json find expression</param>
        /// <param name="routeParameters">Required route parameters needed for the find operation</param>
        /// <param name="queryParameters">Additional optional parameters to use for the find operation</param>
        /// <returns>Array of objects that match the provided regular expression and inputs</returns>
        /// <response code="200">Returns the objects that match the inputs to the find operation</response>
        /// <response code="400">If the find expression contains any invalid inputs</response>
        /// <response code="401">If the HTTP header lacks a valid OAuth2 token</response>
        /// <response code="403">If the HTTP header has a valid OAuth2 token but lacks the appropriate scope to use this route</response>
        /// <response code="406">If the find expression is submitted as anything other than text/plain</response>
        /// <response code="413">If the Json payload is too large</response>
        [Produces("text/plain")]
        [Consumes("text/plain")]
        [HttpPost("{db}/{collection}/find")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(406)]
        [ProducesResponseType(413)]
        [Authorize(Common.READ_AUTHORIZATION_NAME)]
        public async Task<IActionResult> Find([FromBody] string findExpression, [FromRoute] DatabaseRouteParameters routeParameters, [FromQuery] FindQueryParameters queryParameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var findResults = await _repository.FindAsync(routeParameters.DatabaseName, routeParameters.CollectionName, findExpression, queryParameters.Start, queryParameters.Limit, queryParameters.SortFieldName, System.ComponentModel.ListSortDirection.Ascending);
            return Ok(findResults);
        }

        // POST api/1.0/db/collection/count
        /// <summary>
        /// Counts how many objects match the specified criteria
        /// </summary>
        /// <remarks>
        /// Sample request to count the number of documents with a status of either 'A' or 'D' and that have a code equal to 400:
        ///
        ///     POST /api/1.0/object/count
        ///     {
        ///         status:
        ///         {
        ///             $in: [ "A", "D" ]
        ///         },
        ///         code: 400
        ///     }
        ///
        /// </remarks>
        /// <param name="countExpression">The Json count expression</param>
        /// <param name="routeParameters">Required route parameters needed for the find operation</param>
        /// <returns>Number of objects that match the provided regular expression and inputs</returns>
        /// <response code="200">Returns the number of objects that match the inputs to the count operation</response>
        /// <response code="400">If the count expression contains any invalid inputs</response>
        /// <response code="401">If the HTTP header lacks a valid OAuth2 token</response>
        /// <response code="403">If the HTTP header has a valid OAuth2 token but lacks the appropriate scope to use this route</response>
        /// <response code="406">If the count expression is submitted as anything other than text/plain</response>
        /// <response code="413">If the Json payload is too large</response>
        [Produces("application/json")]
        [Consumes("text/plain")]
        [HttpPost("{db}/{collection}/count")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(406)]
        [ProducesResponseType(413)]
        [Authorize(Common.READ_AUTHORIZATION_NAME)]
        public async Task<IActionResult> Count([FromBody] string countExpression, [FromRoute] DatabaseRouteParameters routeParameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var countResults = await _repository.CountAsync(routeParameters.DatabaseName, routeParameters.CollectionName, countExpression);
            return Ok(countResults);
        }
    }
}
