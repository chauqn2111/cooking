﻿using Microsoft.AspNetCore.Mvc;
using ProjectLibrary.DataAccess;
using ProjectLibrary.ObjectBussiness;
using ProjectLibrary.Repository;
using ProjectWebAPI.Application;
using System.ComponentModel.Design;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private ICommentRepository repository = new CommentRepository(); // Assuming you have a CategoryRepository

        // GET: api/<CategoryController> get comment by recipeid
        [HttpGet("{recipeId}")]
        public ActionResult<IEnumerable<Comment>> GetComments(int recipeId) => repository.GetComments(recipeId);

        // GET api/<CategoryController>/5
        [HttpGet("{commentId}")]
        public ActionResult<Comment> GetCommentById(int commentId)
        {
            var comment = repository.GetCommentById(commentId);
            if (comment == null)
            {
                return NotFound(); // Return 404 if the category is not found
            }
            return comment;
        }


        // POST api/<CategoryController>
        [HttpPost]
        public IActionResult CreateComment([FromBody] CommentDTO commentDTO)
        {
            if (commentDTO == null)
            {
                return BadRequest("Invalid comment data");
            }

            var newComment = new Comment()
            {
                CommentId = commentDTO.CommentId,
                UserId = commentDTO.UserId,
                RecipeId = commentDTO.RecipeId,
                CommentText = commentDTO.CommentText,
            };
            // Call the service to add the category to the database
            repository.SaveComment(newComment);

            // Return a success message or other necessary information
            return Ok("Comment created successfully");
        }

        // PUT api/<CategoryController>/5
        [HttpPut("{commentId}")]
        public IActionResult UpdateComment(int commentid, [FromBody] CommentDTO updatedCommentDTO)
        {
            if (updatedCommentDTO == null || commentid != updatedCommentDTO.CommentId)
            {
                return BadRequest("Invalid comment data");
            }

            // Check if the category exists
            var existingComment = repository.GetCommentById(commentid);
            if (existingComment == null)
            {
                return NotFound("Comment not found");
            }

            // Update category information
            existingComment.CommentId = updatedCommentDTO.CommentId;
            existingComment.UserId = updatedCommentDTO.UserId;
            existingComment.RecipeId = updatedCommentDTO.RecipeId;
            existingComment.CommentText = updatedCommentDTO.CommentText;

            // Call the service to save changes to the database
            repository.UpdateComment(existingComment);

            // Return a success message or other necessary information
            return Ok("Comment updated successfully");
        }


        // DELETE api/<CategoryController>/5
        [HttpDelete("{commentId}")]
        public IActionResult DeleteCategory(int commentId)
        {
            var comment = repository.GetCommentById(commentId);
            if (comment == null)
            {
                return NotFound();
            }
            repository.DeleteComment(comment);
            return Ok("Comment deleted successfully");
        }
    }
}