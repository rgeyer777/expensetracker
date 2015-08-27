using ExpenseTracker.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Repository.Helpers;

namespace ExpenseTracker.Repository.Factories
{
    public class ExpenseGroupFactory
    {
        ExpenseFactory expenseFactory = new ExpenseFactory();

        public ExpenseGroupFactory()
        {

        }

        public ExpenseGroup CreateExpenseGroup(DTO.ExpenseGroup expenseGroup)
        {
            return new ExpenseGroup()
            {
                Description = expenseGroup.Description,
                ExpenseGroupStatusId = expenseGroup.ExpenseGroupStatusId,
                Id = expenseGroup.Id,
                Title = expenseGroup.Title,
                UserId = expenseGroup.UserId,
                Expenses = expenseGroup.Expenses == null ? new List<Expense>() : expenseGroup.Expenses.Select(e => expenseFactory.CreateExpense(e)).ToList()
            };
        }


        public DTO.ExpenseGroup CreateExpenseGroup(ExpenseGroup expenseGroup)
        {
            return new DTO.ExpenseGroup()
            {
                Description = expenseGroup.Description,
                ExpenseGroupStatusId = expenseGroup.ExpenseGroupStatusId,
                Id = expenseGroup.Id,
                Title = expenseGroup.Title,
                UserId = expenseGroup.UserId,
                Expenses = expenseGroup.Expenses.Select(e => expenseFactory.CreateExpense(e)).ToList()
            };
        }

        public object CreateDataShapedObject(ExpenseGroup expenseGroup, List<string> listOfFeilds)
        {
            //pass through from entity to DTO
            return CreateDataShapedObject(CreateExpenseGroup(expenseGroup), listOfFeilds);
        }

        public object CreateDataShapedObject(DTO.ExpenseGroup expenseGroup, List<string> listOfFields)
        {
            List<string> lstOfFieldsToWorkWith = new List<string>(listOfFields);
            
            if (!lstOfFieldsToWorkWith.Any())
            {
                return expenseGroup;
            }
            else
            {
                //does it include any expense-related fields?
                var lstOfExpenseFields = lstOfFieldsToWorkWith.Where(f => f.Contains("expenses")).ToList();

                //if one of those fields is "expense", ensure the full expanse is returned
                //otherwise only those subfields needed to be returned.

                bool returnPartialExpense = lstOfExpenseFields.Any() && !lstOfExpenseFields.Contains("expenses");

                //If full expense is not to be returned then spec which fields should be
                if (returnPartialExpense)
                {
                    //remove expense fields from the list
                    lstOfFieldsToWorkWith.RemoveRange(lstOfExpenseFields);
                    lstOfExpenseFields = lstOfExpenseFields.Select(f => f.Substring(f.IndexOf(".") + 1)).ToList();
                }
                else
                {
                    //don't return a partial expense
                    //client can still ask for subfield together with main field, ie: expese,expense.id
                    //remove those subfields in that case

                    lstOfExpenseFields.Remove("expenses");
                    lstOfFieldsToWorkWith.RemoveRange(lstOfExpenseFields);
                }
                
                //dynaically create properties for this object:
                ExpandoObject objectToReturn = new ExpandoObject();

                foreach (string field in lstOfFieldsToWorkWith)
                {
                    var fieldValue = expenseGroup
                        .GetType()
                        .GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                        .GetValue(expenseGroup, null);

                    ((IDictionary<string, object>)objectToReturn).Add(field, fieldValue);
                }

                if (returnPartialExpense)
                {
                    //add a list of expenses and their collections
                    List<object> expenses = new List<object>();
                    foreach (var expense in expenseGroup.Expenses)
                    {
                        expenses.Add(expenseFactory.CreateDataShapeObject(expense, lstOfExpenseFields));
                    }
                    ((IDictionary<string, object>)objectToReturn).Add("expenses", expenses);
                }


                return objectToReturn;
            }
        }
    }
}
