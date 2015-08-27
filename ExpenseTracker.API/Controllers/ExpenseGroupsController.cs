using System;
using System.Web;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Marvin.JsonPatch;
using System.Web.Http.Cors;
using System.Web.Http.Routing;
using ExpenseTracker.Repository;
using System.Collections.Generic;
using ExpenseTracker.API.Helpers;
using ExpenseTracker.Repository.Factories;
using Thinktecture.IdentityModel.WebApi;

namespace ExpenseTracker.API.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "GET,POST")]
    public class ExpenseGroupsController : ApiController
    {
        IExpenseTrackerRepository _repository;
        ExpenseGroupFactory _expenseGroupFactory = new ExpenseGroupFactory();

        private const int _maxPageSize = 10;

        public ExpenseGroupsController()
        {
            _repository = new ExpenseTrackerEFRepository(new 
                Repository.Entities.ExpenseTrackerContext());
        }

        public ExpenseGroupsController(IExpenseTrackerRepository repository)
        {
            _repository = repository;
        }

        [ResourceAuthorize("Read", "ExpenseGroup")]
        [Route("api/expensegroups", Name = "ExpenseGroupsList")]
        public IHttpActionResult Get(string fields = null,
                                    string sort = "id", string status = null, string userId = null,
                                    int page = 1, int pageSize = _maxPageSize)
        {
            try
            {
                bool includeExpenses = false;
                List<string> lstOfFields = new List<string>();

                //include expenses when the fields-string contains "expenses or expenses.id"
                if (fields != null)
                {
                    lstOfFields = fields.ToLower().Split(',').ToList();
                    includeExpenses = lstOfFields.Any(f => f.Contains("expenses"));
                }

                int statusId = -1;
                if (status != null)
                {
                    switch (status.ToLower())
                    {
                        case "open":
                            statusId = 1;
                            break;
                        case "confirmed":
                            statusId = 2;
                            break;
                        case "processed":
                            statusId = 3;
                            break;
                        default:
                            break;
                    }
                }

                IQueryable<Repository.Entities.ExpenseGroup> expenseGroups = null;
                if (includeExpenses)
                {
                    expenseGroups = _repository.GetExpenseGroupsWithExpenses();
                }
                else
                {
                    expenseGroups = _repository.GetExpenseGroups();
                }


                //get expensegroups from repository
                expenseGroups = expenseGroups.ApplySort(sort)
                    .Where(eg => (statusId == -1 || eg.ExpenseGroupStatusId == statusId))
                    .Where(eg => (userId == null || eg.UserId == userId));

                if (pageSize > _maxPageSize)
                {
                    pageSize = _maxPageSize;
                }

                // calculate data for metadata
                var totalCount = expenseGroups.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var urlHelper = new UrlHelper(Request);
                var prevLink = page > 1 ? urlHelper.Link("ExpenseGroupsList",
                    new
                    {
                        page = page - 1,
                        pageSize = pageSize,
                        sort = sort
                    ,
                        fields = fields
                    ,
                        status = status,
                        userId = userId
                    }) : "";
                var nextLink = page < totalPages ? urlHelper.Link("ExpenseGroupsList",
                    new
                    {
                        page = page + 1,
                        pageSize = pageSize,
                        sort = sort
                     ,
                        fields = fields
                     ,
                        status = status,
                        userId = userId
                    }) : "";

                var paginationHeader = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount =totalCount,
                    totalPages = totalPages,
                    previousPageLink = prevLink,
                    nextPageLink = nextLink
                };

                HttpContext.Current.Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationHeader));

                // return result
                return Ok(expenseGroups
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .ToList()
                    .Select(eg => _expenseGroupFactory.CreateDataShapedObject(eg, lstOfFields)));
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [ResourceAuthorize("Read", "ExpenseGroup")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            try
            {
                bool includeExpenses = false;
                List<string> lstOfFields = new List<string>();

                // we should include expenses when the fields-string contains "expenses"
                if (fields != null)
                {
                    lstOfFields = fields.ToLower().Split(',').ToList();
                    includeExpenses = lstOfFields.Any(f => f.Contains("expenses"));
                }

                Repository.Entities.ExpenseGroup expenseGroup;
                if (includeExpenses)
                {
                    expenseGroup = _repository.GetExpenseGroupWithExpenses(id);
                }
                else
                {
                    expenseGroup = _repository.GetExpenseGroup(id);

                }

                if (expenseGroup != null)
                {
                    return Ok(_expenseGroupFactory.CreateDataShapedObject(expenseGroup, lstOfFields));
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [ResourceAuthorize("Write", "ExpenseGroup")]
        [Route("api/expensegroups")]
        [HttpPost]
        public IHttpActionResult Post([FromBody] DTO.ExpenseGroup expenseGroup)
        {
            try
            {
                if (expenseGroup == null)
                {
                    return BadRequest();
                }

                var eg = _expenseGroupFactory.CreateExpenseGroup(expenseGroup);
                var result = _repository.InsertExpenseGroup(eg);

                if (result.Status == RepositoryActionStatus.Created)
                {
                    var newExpenseGroup = _expenseGroupFactory.CreateExpenseGroup((result.Entity));

                    return Created(Request.RequestUri + "/" + newExpenseGroup.Id, newExpenseGroup);
                }

                return BadRequest();

            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [ResourceAuthorize("Write", "ExpenseGroup")]
        public IHttpActionResult Put(int id, [FromBody] DTO.ExpenseGroup expenseGroup)
        {
            try
            {
                if (expenseGroup == null)
                {
                    return BadRequest();
                }

                //map
                var eg = _expenseGroupFactory.CreateExpenseGroup(expenseGroup);
                var result = _repository.UpdateExpenseGroup(eg);

                if (result.Status == RepositoryActionStatus.Updated)
                {
                    //Map to DTO
                    var updatedExpenseGroup = _expenseGroupFactory.CreateExpenseGroup((result.Entity));

                    return Ok(updatedExpenseGroup);
                }
                else if (result.Status == RepositoryActionStatus.NotFound)
                {
                    return NotFound();
                }

                return BadRequest();

            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpPatch]
        [ResourceAuthorize("Write", "ExpenseGroup")]
        public IHttpActionResult Patch(int id, [FromBody]JsonPatchDocument<DTO.ExpenseGroup> expenseGroupPatchDocument)
        {
            try
            {
                if (expenseGroupPatchDocument == null)
                {
                    return BadRequest();
                }

                var expenseGroup = _repository.GetExpenseGroup(id);
                if (expenseGroup == null)
                {
                    return NotFound();
                }

                //map
                var eg = _expenseGroupFactory.CreateExpenseGroup(expenseGroup);

                //apply the changes to the DTO
                expenseGroupPatchDocument.ApplyTo(eg);

                //map the DTO with applied changes to the entity and update
                var results = _repository.UpdateExpenseGroup(_expenseGroupFactory.CreateExpenseGroup(eg));

                if (results.Status == RepositoryActionStatus.Updated)
                {
                    //map to DTO
                    var patchedExpenseGroup = _expenseGroupFactory.CreateExpenseGroup(results.Entity);
                    return Ok(patchedExpenseGroup);
                }

                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [ResourceAuthorize("Write", "ExpenseGroup")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var result = _repository.DeleteExpenseGroup(id);

                if (result.Status == RepositoryActionStatus.Deleted)
                {
                    return StatusCode(HttpStatusCode.NoContent);
                }
                if (result.Status == RepositoryActionStatus.NotFound)
                {
                    return NotFound();
                }

                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }
    }
}
