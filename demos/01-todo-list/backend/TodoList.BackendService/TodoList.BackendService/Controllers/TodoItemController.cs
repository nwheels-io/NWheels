using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoList.BackendService.Domain;
using TodoList.BackendService.Repositories;

namespace TodoList.BackendService.Controllers
{
    [Route("api/todo-item")]
    [ApiController]
    public class TodoItemController : ControllerBase
    {
        private readonly ITodoItemRepository _repository;

        public TodoItemController(ITodoItemRepository repository)
        {
            _repository = repository;
        }
        
        // GET api/todo-item?description=aaa&done=true
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> Get(
            [FromQuery] string description,
            [FromQuery] bool? done)
        {
            var result = (await _repository.GetByQuery(description, done)).ToArray();
            return result;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}