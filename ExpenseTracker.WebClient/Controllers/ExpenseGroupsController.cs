using System;
using PagedList;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using ExpenseTracker.DTO;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Thinktecture.IdentityModel.Mvc;
using ExpenseTracker.WebClient.Models;
using ExpenseTracker.WebClient.Helpers;
using Thinktecture.IdentityModel.Owin.ResourceAuthorization;

namespace ExpenseTracker.WebClient.Controllers
{
    [Authorize]
    public class ExpenseGroupsController : Controller
    {
        //[ResourceAuthorize("Read", "ExpenseGroup")]
        public async Task<ActionResult> Index(int? page = 1)
        {
            HttpClient client = ExpenseTrackerHttpClient.GetClient();

            var model = new ExpenseGroupViewModel();

            HttpResponseMessage egsResponse = await client.GetAsync("api/expensegroupstatusses");

            if (egsResponse.IsSuccessStatusCode)
            {
                string egsContent = await egsResponse.Content.ReadAsStringAsync();
                var listExpenseGroupStatusses = JsonConvert.DeserializeObject<IEnumerable<ExpenseGroupStatus>>(egsContent);

                model.ExpenseGroupStatusses = listExpenseGroupStatusses.ToList();
            }
            else
            {
                return Content("An error occurred retrieving the data from the API");
            }

            HttpResponseMessage response = await client.GetAsync("api/expensegroups?sort=expensegroupstatusid, title&page=" + page + "&pagesize=5");

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                var pagingInfo = HeaderParser.FindAndParsePagingInfo(response.Headers);

                var listExpenseGroups = JsonConvert.DeserializeObject<IEnumerable<ExpenseGroup>>(content);

                var pagedExpenseGroupsList = new StaticPagedList<ExpenseGroup>(
                    listExpenseGroups,
                    pagingInfo.CurrentPage,
                    pagingInfo.PageSize,
                    pagingInfo.TotalCount);

                model.ExpenseGroups = pagedExpenseGroupsList;
                model.PagingInfo = pagingInfo;

            }
            else
            {
                return Content("An error occurred retrieving the data from the API");
            }

            return View(model);

        }

        // GET: ExpenseGroups/Details/5
        // GET: ExpenseGroups/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var client = ExpenseTrackerHttpClient.GetClient();

            HttpResponseMessage response = await client.GetAsync("api/expensegroups/" + id
                                + "?fields=id,description,title,expenses");
            string content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var model = JsonConvert.DeserializeObject<ExpenseGroup>(content);
                return View(model);
            }

            return Content("An error occurred");
        }

        // GET: ExpenseGroups/Create
 
        public ActionResult Create()
        {
            return View();
        }

        // POST: ExpenseGroups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ExpenseGroup expenseGroup)
        {
            try
            {
                var client = ExpenseTrackerHttpClient.GetClient();

                var claimsIdentity = this.User.Identity as ClaimsIdentity;
                var userId = claimsIdentity.FindFirst("unique_user_key").Value;

                //an expensegroup is created with status "Open", for the current user:
                expenseGroup.ExpenseGroupStatusId = 1;
                expenseGroup.UserId = userId;

                var serializedItemToCreate = JsonConvert.SerializeObject(expenseGroup);

                var response = await client.PostAsync("api/expensegroups",
                        new StringContent(serializedItemToCreate,
                        System.Text.Encoding.Unicode, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return Content("An error occurred");
                }
            }
            catch (Exception)
            {

                return Content("An error occurred");
            }
        }

        // GET: ExpenseGroups/Edit/5
        [ResourceAuthorize("Write", "ExpenseGroup")]
        public async Task<ActionResult> Edit(int id)
        {
            var client = ExpenseTrackerHttpClient.GetClient();

            HttpResponseMessage response = await client.GetAsync("api/expensegroups/" + id);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<ExpenseGroup>(content);
                return View(model);
            }

            return Content("An error occurred.");
        }

        // POST: ExpenseGroups/Edit/5   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, ExpenseGroup expenseGroup)
        {
            try
            {
                var client = ExpenseTrackerHttpClient.GetClient();

                var serializedItemToUpdate = JsonConvert.SerializeObject(expenseGroup);

                var response = await client.PutAsync("api/expensegroups/" + id,
                        new StringContent(serializedItemToUpdate,
                        System.Text.Encoding.Unicode, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return Content("An error occurred");
                }
            }
            catch (Exception)
            {

                return Content("An error occurred");
            }
        }
         
        // POST: ExpenseGroups/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var client = ExpenseTrackerHttpClient.GetClient();
                var response = await client.DeleteAsync("api/expensegroups/" + id);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return Content("An error occurred");
                }
            }
            catch (Exception)
            {
                return Content("An error occurred");
            }
        }
    }
}
