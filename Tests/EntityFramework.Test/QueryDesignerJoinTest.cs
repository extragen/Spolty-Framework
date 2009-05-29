using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Metadata.Edm;
using System.Linq;
using EntityFrameworkNorthwind.Entities;
using NUnit.Framework;
using Spolty.Framework.Designers;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Conditionals.Enums;
using Spolty.Framework.Parameters.Joins;

namespace EntityFramework.Test
{
    [TestFixture]
    public class QueryDesignerJoinTest
    {
        private readonly NorthwindEntities context = new NorthwindEntities(ConnectionString.ConnectionString);

        private static ConnectionStringSettings ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["NorthwindConnectionString"]; }
        }

        ~QueryDesignerJoinTest()
        {
            if (context != null)
            {
                context.Dispose();
            }
        }

        /*
         * Query created in Microsoft SQL Server Management Studio
         * 
         *  SELECT     Products.*
         *  FROM       Products  INNER JOIN Categories
		 *		        ON Products.CategoryID = Categories.CategoryID
         *		
         *  result of that script it's 77 rows that why I test row count equal 77
         *  
         * Query generated by QueryDesigner
         * 
         * SELECT [t0].[ProductID], [t0].[ProductName], [t0].[SupplierID], [t0].[CategoryID], [t0].[QuantityPerUnit], [t0].[UnitPrice], [t0].[UnitsInStock], [t0].[UnitsOnOrder], [t0].[ReorderLevel], [t0].[Discontinued]
         * FROM [dbo].[Products] AS [t0] INNER JOIN [dbo].[Categories] AS [t1] 
         *      ON [t0].[CategoryID] = [t1].[CategoryID]
         *      
         */
        [Test]
        public void TestJoinWithOneChild()
        {
            const int resultRowCount = 77;
            //create root node
            var root = new JoinNode(typeof (Products));

            // add child node Category with propertyName "Products". 
            // Because Category linked with Product by next property:
            // public EntitySet<Product> Products 
            root.AddChildren(new JoinNode(typeof (Categories), "Products"));

            var queryDesinger = new QueryDesinger(context, root);
            var list = new List<Products>(queryDesinger.Cast<Products>());
            Assert.AreEqual(resultRowCount, list.Count);
        }

        /*  
         * Query created in Microsoft SQL Server Management Studio
         * 
         *  SELECT     Products.* 
         *  FROM       Products  INNER JOIN Categories
         *              ON Products.CategoryID = Categories.CategoryID
         *  WHERE Categories.CategoryName = N'Condiments'
         *  
         * result of that script it's 12 rows that why I test row count equal 12
         * 
         * Query generated by QueryDesigner
         * 
         * exec sp_executesql N'SELECT [t0].[ProductID], [t0].[ProductName], [t0].[SupplierID], [t0].[CategoryID], [t0].[QuantityPerUnit], [t0].[UnitPrice], [t0].[UnitsInStock], [t0].[UnitsOnOrder], [t0].[ReorderLevel], [t0].[Discontinued]
         * FROM [dbo].[Products] AS [t0] INNER JOIN [dbo].[Categories] AS [t1] 
         *              ON [t0].[CategoryID] = [t1].[CategoryID]
         * WHERE [t1].[CategoryName] = @p0',N'@p0 nvarchar(10)',@p0=N'Condiments'
         * 
         */
        [Test]
        public void TestJoinWithOneChildAndFilteredByCategoryName()
        {
            const string categoryName = "Condiments";
            const int resultRowCount = 12;
            //create root node
            var root = new JoinNode(typeof (Products));

            // add child node Category with propertyName "Products". 
            // Because Category linked with Product by next property:
            // public EntitySet<Product> Products 
            var categoryNode = new JoinNode(typeof (Categories), "Products");
            root.AddChildren(categoryNode);

            // add condition for filtering by CategoryName == "Condiments"
            categoryNode.AddConditions(new Condition("CategoryName", categoryName));

            var queryDesinger = new QueryDesinger(context, root);
            var list = new List<Products>(queryDesinger.Cast<Products>());
            Assert.AreEqual(resultRowCount, list.Count);
        }

        /*  
         * Query created in Microsoft SQL Server Management Studio
         * 
         *  SELECT     Products.* 
         *  FROM       Products  INNER JOIN Categories
         *              ON Products.CategoryID = Categories.CategoryID
         *  WHERE Products.ProductName like N'Louisiana%' AND Categories.CategoryName = N'Condiments'
         *  
         * result of that script it's 2 rows that why I test row count equal 2
         * 
         * Query generated by QueryDesigner
         * 
         * exec sp_executesql N'SELECT [t0].[ProductID], [t0].[ProductName], [t0].[SupplierID], [t0].[CategoryID], [t0].[QuantityPerUnit], [t0].[UnitPrice], [t0].[UnitsInStock], [t0].[UnitsOnOrder], [t0].[ReorderLevel], [t0].[Discontinued]
         * FROM [dbo].[Products] AS [t0] INNER JOIN [dbo].[Categories] AS [t1] 
         *              ON [t0].[CategoryID] = [t1].[CategoryID]
         * WHERE ([t0].[ProductName] LIKE @p0) AND ([t1].[CategoryName] = @p1)',N'@p0 nvarchar(10),@p1 nvarchar(10)',@p0=N'Louisiana%',@p1=N'Condiments'
         * 
         */
        [Test]
        public void TestJoinWithOneChildAndFilteredByProductNameAndCategoryNameV1()
        {
            const string productName = "Louisiana";
            const string categoryName = "Condiments";
            const int resultRowCount = 2;
            //create root node
            var root = new JoinNode(typeof (Products));

            // add condition for filtering by ProductName Like "Louisiana%"
            root.AddConditions(new Condition("ProductName", productName, ConditionOperator.StartsWith));

            // add child node Category with propertyName "Products". 
            // Because Category linked with Product by next property:
            // public EntitySet<Product> Products 
            var categoryNode = new JoinNode(typeof (Categories), "Products");
            root.AddChildren(categoryNode);

            // add condition for filtering by CategoryName == "Condiments"
            categoryNode.AddConditions(new Condition("CategoryName", categoryName));

            var queryDesinger = new QueryDesinger(context, root);
            var list = new List<Products>(queryDesinger.Cast<Products>());
            Assert.AreEqual(resultRowCount, list.Count);
        }

        [Test]
        public void TestJoinWithOneChildAndFilteredByProductNameAndCategoryNameV2()
        {
            const string productName = "Louisiana";
            const string categoryName = "Condiments";
            const int resultRowCount = 2;
            //create root node
            var root = new JoinNode(typeof (Products));

            // add child node Category with propertyName "Products". 
            // Because Category linked with Product by next property:
            // public EntitySet<Product> Products 
            var categoryNode = new JoinNode(typeof (Categories), "Products");
            root.AddChildren(categoryNode);

            var queryDesinger = new QueryDesinger(context, root);

            // add condition for filtering by ProductName Like "Louisiana%"
            var product = new Condition("ProductName", productName, ConditionOperator.StartsWith);
            
            // add condition for filtering by CategoryName == "Condiments"
            var categoryCondition = new Condition("CategoryName", categoryName, ConditionOperator.EqualTo, typeof(Categories));
            
            queryDesinger.AddConditions(new ConditionList(product, categoryCondition));
            var list = new List<Products>(queryDesinger.Cast<Products>());
            Assert.AreEqual(resultRowCount, list.Count);
        }

        /*  
         * Query created in Microsoft SQL Server Management Studio
         * 
         * SELECT   DISTINCT  Products.ProductID, Products.ProductName, Products.SupplierID, Products.CategoryID, Products.QuantityPerUnit, Products.UnitPrice, Products.UnitsInStock, Products.UnitsOnOrder, Products.ReorderLevel, Products.Discontinued
         * FROM         Products 
         *         INNER JOIN Categories ON Products.CategoryID = Categories.CategoryID 
         *         INNER JOIN [Order Details] ON Products.ProductID = [Order Details].ProductID
         * WHERE   [Order Details].Discount > 0.15   
         *         AND ((Products.ProductName LIKE N'Louisiana%') OR (Categories.CategoryName = N'Condiments'))
         *  
         * result of that script it's 9 rows that why I test row count equal 9
         * 
         * Query generated by QueryDesigner
         * 
         * exec sp_executesql N'SELECT DISTINCT [t0].[ProductID], [t0].[ProductName], [t0].[SupplierID], [t0].[CategoryID], [t0].[QuantityPerUnit], [t0].[UnitPrice], [t0].[UnitsInStock], [t0].[UnitsOnOrder], [t0].[ReorderLevel], [t0].[Discontinued] 
         * FROM [dbo].[Products] AS [t0]
         *         INNER JOIN [dbo].[Categories] AS [t1] ON [t0].[CategoryID] = [t1].[CategoryID]
         *         INNER JOIN [dbo].[Order Details] AS [t2] ON [t0].[ProductID] = [t2].[ProductID]
         * WHERE ([t2].[Discount] > @p0) AND (([t0].[ProductName] LIKE @p1) OR ([t1].[CategoryName] = @p2))',N'@p0 real,@p1 nvarchar(10),@p2 nvarchar(10)',@p0=0,15,@p1=N'Louisiana%',@p2=N'Condiments'* 
         */
        [Test]
        public void TestJoinWithTwoChildrenAndComplicatedFilter()
        {
            ReadOnlyCollection<AssociationType> items = context.MetadataWorkspace.GetItems<AssociationType>(DataSpace.CSpace);
            var item = items[0];
            var members = item.AssociationEndMembers[0];
            ReadOnlyMetadataCollection<MetadataProperty> properties = members.MetadataProperties;
            RelationshipMultiplicity multiplicity = members.RelationshipMultiplicity;
            EdmType type = members.TypeUsage.EdmType.BaseType;
            
            const string productName = "Louisiana";
            const string categoryName = "Condiments";
            const int resultRowCount = 9;
            //create root node
            var root = new JoinNode(typeof (Products));

            // add first child node Category with propertyName "Products". 
            // Because Category linked with Product by next property:
            // public EntitySet<Product> Products 
            var categoryNode = new JoinNode(typeof(Categories), "Products");
            root.AddChildren(categoryNode);

            // add second child node Order_Detail. PropertyName not defined
            // because Order_Detail linked with Product by next property:
            // public Product Product - name of property is equal name of type 
            var order_DetailNode = new JoinNode(typeof(Order_Details), "Products");

            root.AddChildren(order_DetailNode);

            var queryDesinger = new QueryDesinger(context, root);

            // create conditions for filtering by ProductName Like "Louisiana%" Or CategoryName == "Condiments"
            var productCondition = new Condition("ProductName", productName, ConditionOperator.StartsWith,
                                                 typeof (Products));
            var categoryCondition = new Condition("CategoryName", categoryName, ConditionOperator.EqualTo,
                                                  typeof (Categories));
            var orCondition = new OrCondition(productCondition, categoryCondition);

            // create condition for filtering by [Order Details].Discount > 0.15
            var discountCondition = new Condition("Discount", 0.15F, ConditionOperator.GreaterThen,
                                                  typeof (Order_Details));

            var conditionals = new ConditionList(orCondition, discountCondition);

            // assign conditions
            queryDesinger.AddConditions(conditionals);

            // make Distinct
            IQueryable<Products> distictedProducts = queryDesinger.Cast<Products>().Distinct();
            
            var list = new List<Products>(distictedProducts);
            Assert.AreEqual(resultRowCount, list.Count);
        }
    }
}