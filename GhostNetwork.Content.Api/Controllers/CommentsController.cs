﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GhostNetwork.Content.Api.Helpers;
using GhostNetwork.Content.Api.Models;
using GhostNetwork.Content.Comments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GhostNetwork.Content.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentsService commentService;
        private readonly IUserProvider userProvider;

        public CommentsController(ICommentsService commentService, IUserProvider userProvider)
        {
            this.commentService = commentService;
            this.userProvider = userProvider;
        }

        /// <summary>
        /// Create comment
        /// </summary>
        /// <param name="model">Comment</param>
        /// <returns>Created comment</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Comment>> CreateAsync([FromBody] CreateCommentModel model)
        {
            var author = await userProvider.GetByIdAsync(model.AuthorId);

#pragma warning disable 612
            var key = model.Key ?? PublicationKey(model.PublicationId);
            var (domainResult, id) = await commentService
                .CreateAsync(key, model.Content, model.ReplyCommentId, author);
#pragma warning restore 612

            if (domainResult.Successed)
            {
                return Created(Url.Action("GetById", new {id}), await commentService.GetByIdAsync(id));
            }

            return BadRequest(domainResult.ToProblemDetails());
        }

        /// <summary>
        /// Get one comment by id
        /// </summary>
        /// <param name="id">Comment id</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Comment>> GetByIdAsync([FromRoute] string id)
        {
            var comment = await commentService.GetByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }

        /// <summary>
        /// Search comments for publications
        /// </summary>
        /// <param name="model">Array of publications ids</param>
        /// <returns>Comments related to publications</returns>
        [HttpPost("comments/featured")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Dictionary<string, FeaturedInfo>>> SearchFeaturedAsync(
            [FromBody] FeaturedQuery model)
        {
#pragma warning disable 612
            var keys = model.Keys?.ToList() ?? model.PublicationIds
#pragma warning restore 612
                .Select(PublicationKey)
                .ToList();

            var result = await commentService.SearchFeaturedAsync(keys);
            return Ok(result);
        }

        /// <summary>
        /// Search comments for publication
        /// </summary>
        /// <param name="publicationId">Publication id</param>
        /// <param name="skip">Skip comments up to a specified position</param>
        /// <param name="take">Take comments up to a specified position</param>
        /// <returns>Comments related to publication</returns>
        [Obsolete]
        [HttpGet("bypublication/{publicationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Comment>>> SearchAsync(
            [FromRoute] string publicationId,
            [FromQuery, Range(0, int.MaxValue)] int skip,
            [FromQuery, Range(0, 100)] int take = 10)
        {
            var (comments, totalCount) = await commentService.SearchAsync(PublicationKey(publicationId), skip, take);
            Response.Headers.Add("X-TotalCount", totalCount.ToString());

            return Ok(comments);
        }

        /// <summary>
        /// Search comments for publication
        /// </summary>
        /// <param name="key">Comment key</param>
        /// <param name="skip">Skip comments up to a specified position</param>
        /// <param name="take">Take comments up to a specified position</param>
        /// <returns>Comments related to publication</returns>
        [HttpGet("bykey/{key}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Comment>>> SearchByKeyAsync(
            [FromRoute] string key,
            [FromQuery, Range(0, int.MaxValue)] int skip,
            [FromQuery, Range(0, 100)] int take = 10)
        {
            var (comments, totalCount) = await commentService.SearchAsync(key, skip, take);
            Response.Headers.Add("X-TotalCount", totalCount.ToString());

            return Ok(comments);
        }

        /// <summary>
        /// Delete one comment
        /// </summary>
        /// <param name="id">Comment id</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Comment>>> DeleteAsync([FromRoute] string id)
        {
            if (await commentService.GetByIdAsync(id) == null)
            {
                return NotFound();
            }

            await commentService.DeleteAsync(id);

            return NoContent();
        }

        /// <summary>
        /// Delete all publication comments
        /// </summary>
        /// <param name="publicationId">Publication id</param>
        [Obsolete]
        [HttpDelete("bypublication/{publicationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Comment>>> DeleteByPublicationAsync([FromRoute] string publicationId)
        {
            await commentService.DeleteByKeyAsync(PublicationKey(publicationId));

            return NoContent();
        }

        /// <summary>
        /// Delete all comments by key
        /// </summary>
        /// <param name="key">Comments key</param>
        [HttpDelete("bykey/{key}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Comment>>> DeleteByKeyAsync([FromRoute] string key)
        {
            await commentService.DeleteByKeyAsync(key);

            return NoContent();
        }

        private static string PublicationKey(string publicationId) => $"publication_{publicationId}";
    }
}