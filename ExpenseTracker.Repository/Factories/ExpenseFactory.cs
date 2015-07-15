using System.Data.Odbc;
using ExpenseTracker.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository.Factories
{
    public class ExpenseFactory
    {

        public ExpenseFactory()
        {

        }

        public DTO.Expense CreateExpense(Expense expense)
        {
            return new DTO.Expense()
            {
                Amount = expense.Amount,
                Date = expense.Date,
                Description = expense.Description,
                ExpenseGroupId = expense.ExpenseGroupId,
                Id = expense.Id
            };
        }



        public Expense CreateExpense(DTO.Expense expense)
        {
            return new Expense()
            {
                Amount = expense.Amount,
                Date = expense.Date,
                Description = expense.Description,
                ExpenseGroupId = expense.ExpenseGroupId,
                Id = expense.Id
            };
        }

        public object CreateDataShapeObject(Expense expense, List<string> listOfFeilds)
        {
            //pass through from entity to DTO
            return CreateDataShapeObject(CreateExpense(expense), listOfFeilds);
        }

        public object CreateDataShapeObject(DTO.Expense expense, List<string> listOfFeilds)
        {
            if (!listOfFeilds.Any())
            {
                return expense;
            }
            else
            {
                ExpandoObject objectToReturn = new ExpandoObject();

                foreach (string field in listOfFeilds)
                {
                    var fieldValue = expense
                        .GetType()
                        .GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                        .GetValue(expense, null);

                    ((IDictionary<string, object>)objectToReturn).Add(field, fieldValue);
                }
                return objectToReturn;
            }
        }
    }
}
