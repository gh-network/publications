﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GhostNetwork.Publications.Api.Helpers;
using GhostNetwork.Publications.Api.Models;
using GhostNetwork.Publications.Comments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GhostNetwork.Publications.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentsService commentService;

        public CommentsController(ICommentsService commentService)
        {
            this.commentService = commentService;
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
            var (domainResult, id) = await commentService.CreateAsync(model.PublicationId, model.Content, model.ReplyCommentId, (UserInfo)model.Author);

            if (domainResult.Successed)
            {
                return Created(Url.Action("GetById", new { id }), await commentService.GetByIdAsync(id));
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
        /// <param name="order">Order by creation date</param>
        /// <returns>Comments related to publications</returns>
        [HttpPost("comments/featured")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Dictionary<string, IEnumerable<Comment>>>> SearchByPublicationsAsync(
            [FromBody] FindCommentsByIdsModel model,
            [FromQuery] Ordering order = Ordering.Asc)
        {
            var result = await commentService.FindCommentsByPublicationsAsync(model.PublicationIds, order);
            return Ok(result);
        }

        /// <summary>
        /// Search comments for publication
        /// </summary>
        /// <param name="publicationId">Publication id</param>
        /// <param name="skip">Skip comments up to a specified position</param>
        /// <param name="take">Take comments up to a specified position</param>
        /// <returns>Comments related to publication</returns>
        [HttpGet("bypublication/{publicationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Comment>>> SearchAsync(
            [FromRoute] string publicationId,
            [FromQuery, Range(0, int.MaxValue)] int skip,
            [FromQuery, Range(0, 100)] int take = 10)
        {
            var (comments, totalCount) = await commentService.SearchAsync(publicationId, skip, take);
            Response.Headers.Add("X-TotalCount", totalCount.ToString());

            return Ok(comments);
        }

        /// <summary>
        /// Delete one comment
        /// </summary>
        /// <param name="id">Comment id</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Comment>>> DeleteAsync([FromRoute] string id)
        {
            if (await commentService.GetByIdAsync(id) == null)
            {
                return NotFound();
            }

            await commentService.DeleteAsync(id);

            return Ok();
        }
    }
}
