using ExpenseTracker.DTO;
using System.Collections.Generic;
using ExpenseTracker.WebClient.Helpers;
using PagedList;

namespace ExpenseTracker.WebClient.Models
{
    public class ExpenseGroupViewModel
    {
        public IPagedList<ExpenseGroup> ExpenseGroups { get; set; }
        public List<ExpenseGroupStatus> ExpenseGroupStatusses { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}