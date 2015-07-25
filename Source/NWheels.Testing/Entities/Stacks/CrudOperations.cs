using System;
using System.Linq;
using NUnit.Framework;
using NWheels.Entities;

namespace NWheels.Testing.Entities.Stacks
{
    public static class CrudOperations
    {
        public static class Repository1
        {
            public static void ExecuteBasic(Func<Interfaces.Repository1.IOnlineStoreRepository> repoFactory)
            {
                InsertAttributes(repoFactory());
                InsertCategories(repoFactory());
                InsertProducts(repoFactory());
                RetrieveProductsByName(repoFactory());
                InsertOrder1(repoFactory());
                InsertOrder2(repoFactory());
                InsertOrder3(repoFactory());
                UpdateStatusOfOrders(repoFactory());
                RetrieveOrdersWithOrderLinesAndProducts(repoFactory());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void ExecuteAdvancedRetrievals(Func<Interfaces.Repository1.IOnlineStoreRepository> repoFactory)
            {
                InsertAttributes(repoFactory());
                InsertCategories(repoFactory());
                InsertProducts(repoFactory());
                InsertOrder1(repoFactory());
                InsertOrder2(repoFactory());
                InsertOrder3(repoFactory());
                GroupOrderIdsByDate(repoFactory());
                FindOrdersOfProductsByNamePrefix(repoFactory());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void InsertCategories(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    var category1  = repo.Categories.New();
                    category1.Name = "CAT1";

                    var category2 = repo.Categories.New();
                    category2.Name = "CAT2";

                    var category3 = repo.Categories.New();
                    category3.Name = "CAT3";

                    repo.Categories.Insert(category1);
                    repo.Categories.Insert(category2);
                    repo.Categories.Insert(category3);

                    repo.CommitChanges();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void InsertAttributes(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    var attr1 = repo.Attributes.New();
                    attr1.Name = "Size";
                    attr1.TitleForUser = "Size";
                    attr1.Values.Add(repo.NewAttributeValue("S", 1));
                    attr1.Values.Add(repo.NewAttributeValue("M", 2));
                    attr1.Values.Add(repo.NewAttributeValue("L", 3));

                    var attr2 = repo.Attributes.New();
                    attr2.Name = "Color";
                    attr2.TitleForUser = "Color";
                    attr2.Values.Add(repo.NewAttributeValue("White", 1));
                    attr2.Values.Add(repo.NewAttributeValue("Black", 2));

                    repo.Attributes.Insert(attr1);
                    repo.Attributes.Insert(attr2);

                    repo.CommitChanges();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void InsertProducts(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    var categories = repo.Categories.OrderBy(c => c.Name).ToArray();
                    Assert.That(categories.Select(c => c.Name), Is.EqualTo(new[] { "CAT1", "CAT2", "CAT3" }));

                    var product1 = repo.Products.New();
                    product1.CatalogNo = "CN111";
                    product1.Name = "ABC";
                    product1.Price = 123.45m;
                    product1.Categories.Add(categories[0]);

                    var product2 = repo.Products.New();
                    product2.CatalogNo = "CN222";
                    product2.Name = "DEF";
                    product2.Price = 678.90m;
                    product2.Categories.Add(categories[0]);
                    product2.Categories.Add(categories[2]);

                    repo.Products.Insert(product1);
                    repo.Products.Insert(product2);

                    repo.CommitChanges();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void RetrieveProductsByName(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    var product1 = repo.Products.Include(p => p.Categories).Where(p => p.Name == "ABC").ToArray().First();
                    var product2 = repo.Products.Include(p => p.Categories).Where(p => p.Name == "DEF").ToArray().First();

                    Assert.That(product1.CatalogNo, Is.EqualTo("CN111"));
                    Assert.That(product1.Name, Is.EqualTo("ABC"));
                    Assert.That(product1.Price, Is.EqualTo(123.45m));

                    Assert.That(product1.Categories, Is.Not.Null);
                    Assert.That(product1.Categories.Count(), Is.EqualTo(1));
                    Assert.That(product1.Categories.Count(c => c.Name == "CAT1"), Is.EqualTo(1));

                    Assert.That(product2.CatalogNo, Is.EqualTo("CN222"));
                    Assert.That(product2.Name, Is.EqualTo("DEF"));
                    Assert.That(product2.Price, Is.EqualTo(678.90m));

                    Assert.That(product2.Categories, Is.Not.Null);
                    Assert.That(product2.Categories.Count(), Is.EqualTo(2));
                    Assert.That(product2.Categories.Count(c => c.Name == "CAT1"), Is.EqualTo(1));
                    Assert.That(product2.Categories.Count(c => c.Name == "CAT3"), Is.EqualTo(1));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void InsertOrder1(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    var product1 = repo.Products.Single(p => p.Name == "ABC");
                    var product2 = repo.Products.Single(p => p.Name == "DEF");

                    var order = repo.Orders.New();
                    order.OrderNo = "ORD001";
                    order.PlacedAt = new DateTime(2015, 1, 1, 12, 0, 0, DateTimeKind.Utc);

                    order.OrderLines.Add(repo.NewOrderLine(order, product1, quantity: 22));
                    order.OrderLines.Add(repo.NewOrderLine(order, product2, quantity: 11));

                    repo.Orders.Insert(order);
                    repo.CommitChanges();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void InsertOrder2(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    var product1 = repo.Products.Single(p => p.Name == "ABC");
                    var product2 = repo.Products.Single(p => p.Name == "DEF");

                    var order = repo.Orders.New();
                    order.OrderNo = "ORD002";
                    order.PlacedAt = new DateTime(2015, 1, 2, 12, 0, 0, DateTimeKind.Utc);

                    order.OrderLines.Add(repo.NewOrderLine(order, product1, quantity: 11));
                    order.OrderLines.Add(repo.NewOrderLine(order, product2, quantity: 22));

                    repo.Orders.Insert(order);
                    repo.CommitChanges();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void InsertOrder3(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    var product2 = repo.Products.Single(p => p.Name == "DEF");

                    var order = repo.Orders.New();
                    order.OrderNo = "ORD003";
                    order.PlacedAt = new DateTime(2015, 1, 2, 13, 0, 0, DateTimeKind.Utc);

                    order.OrderLines.Add(repo.NewOrderLine(order, product2, quantity: 33));

                    repo.Orders.Insert(order);
                    repo.CommitChanges();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void UpdateStatusOfOrders(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    var order1 = repo.Orders.Single(o => o.OrderNo == "ORD001");
                    order1.Status = Interfaces.Repository1.OrderStatus.ProductsShipped;
                    repo.Orders.Update(order1);

                    var order2 = repo.Orders.Single(o => o.OrderNo == "ORD002");
                    order2.Status = Interfaces.Repository1.OrderStatus.PaymentReceived;
                    repo.Orders.Update(order2);

                    repo.CommitChanges();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void RetrieveOrdersWithOrderLinesAndProducts(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                Interfaces.Repository1.IOrder order1;
                Interfaces.Repository1.IOrder order2;
                Interfaces.Repository1.IOrder order3;

                using ( repo )
                {
                    order1 = repo.Orders.Include(o => o.OrderLines.Select(ol => ol.Product)).Single(o => o.OrderNo == "ORD001");
                    order2 = repo.Orders.Include(o => o.OrderLines.Select(ol => ol.Product)).Single(o => o.OrderNo == "ORD002");
                    order3 = repo.Orders.Include(o => o.OrderLines.Select(ol => ol.Product)).Single(o => o.OrderNo == "ORD003");
                }

                #region Assert order1
                Assert.That(order1.OrderNo, Is.EqualTo("ORD001"));
                Assert.That(order1.PlacedAt, Is.EqualTo(new DateTime(2015, 1, 1, 12, 0, 0, DateTimeKind.Utc)));
                Assert.That(order1.Status, Is.EqualTo(Interfaces.Repository1.OrderStatus.ProductsShipped));
                
                var order1Lines = order1.OrderLines.ToArray();
                Assert.That(order1Lines.Length, Is.EqualTo(2));

                Assert.That(order1Lines[0].Order, Is.SameAs(order1));
                Assert.That(order1Lines[0].Product.CatalogNo, Is.EqualTo("CN111"));
                Assert.That(order1Lines[0].Quantity, Is.EqualTo(22));

                Assert.That(order1Lines[1].Order, Is.SameAs(order1));
                Assert.That(order1Lines[1].Product.CatalogNo, Is.EqualTo("CN222"));
                Assert.That(order1Lines[1].Quantity, Is.EqualTo(11));
                #endregion

                #region Assert order2
                Assert.That(order2.OrderNo, Is.EqualTo("ORD002"));
                Assert.That(order2.PlacedAt, Is.EqualTo(new DateTime(2015, 1, 2, 12, 0, 0, DateTimeKind.Utc)));
                Assert.That(order2.Status, Is.EqualTo(Interfaces.Repository1.OrderStatus.PaymentReceived));

                var order2Lines = order2.OrderLines.ToArray();
                Assert.That(order2Lines.Length, Is.EqualTo(2));

                Assert.That(order2Lines[0].Order, Is.SameAs(order2));
                Assert.That(order2Lines[0].Product.CatalogNo, Is.EqualTo("CN111"));
                Assert.That(order2Lines[0].Quantity, Is.EqualTo(11));

                Assert.That(order2Lines[1].Order, Is.SameAs(order2));
                Assert.That(order2Lines[1].Product.CatalogNo, Is.EqualTo("CN222"));
                Assert.That(order2Lines[1].Quantity, Is.EqualTo(22));
                #endregion

                #region Assert order3
                Assert.That(order3.OrderNo, Is.EqualTo("ORD003"));
                Assert.That(order3.PlacedAt, Is.EqualTo(new DateTime(2015, 1, 2, 13, 0, 0, DateTimeKind.Utc)));
                Assert.That(order3.Status, Is.EqualTo(Interfaces.Repository1.OrderStatus.New));

                var order3Lines = order3.OrderLines.ToArray();
                Assert.That(order3Lines.Length, Is.EqualTo(1));

                Assert.That(order3Lines[0].Order, Is.SameAs(order3));
                Assert.That(order3Lines[0].Product.CatalogNo, Is.EqualTo("CN222"));
                Assert.That(order3Lines[0].Quantity, Is.EqualTo(33));
                #endregion

                Assert.That(order1Lines[0].Product, Is.SameAs(order2Lines[0].Product));
                Assert.That(order1Lines[1].Product, Is.SameAs(order2Lines[1].Product));
                Assert.That(order1Lines[1].Product, Is.SameAs(order3Lines[0].Product));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void GroupOrderIdsByDate(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    var orderIdsByDate = (
                        from o in repo.Orders
                        group o by o.PlacedAt.Date into g  // note we're using DateTime.Date and not DbFunctions.TruncateTime!
                        orderby g.Key
                        select new { Date = g.Key, OrderIds = g.Select(x => x.OrderNo) }).ToArray();

                    Assert.That(orderIdsByDate.Length, Is.EqualTo(2));

                    Assert.That(orderIdsByDate[0].Date, Is.EqualTo(new DateTime(2015, 1, 1)));
                    Assert.That(orderIdsByDate[0].OrderIds.Count(), Is.EqualTo(1));
                    Assert.That(orderIdsByDate[0].OrderIds.First(), Is.EqualTo("ORD001"));

                    Assert.That(orderIdsByDate[1].Date, Is.EqualTo(new DateTime(2015, 1, 2)));
                    Assert.That(orderIdsByDate[1].OrderIds.Count(), Is.EqualTo(2));
                    Assert.That(orderIdsByDate[1].OrderIds.First(), Is.EqualTo("ORD002"));
                    Assert.That(orderIdsByDate[1].OrderIds.Last(), Is.EqualTo("ORD003"));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void FindOrdersOfProductsByNamePrefix(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    var ordersOfAProducts = (
                        from o in repo.Orders
                        where o.OrderLines.Any(ol => ol.Product.Name.StartsWith("A"))
                        orderby o.OrderNo
                        select o).ToArray();

                    Assert.That(ordersOfAProducts.Length, Is.EqualTo(2));
                    Assert.That(ordersOfAProducts[0].OrderNo, Is.EqualTo("ORD001"));
                    Assert.That(ordersOfAProducts[1].OrderNo, Is.EqualTo("ORD002"));
                }
            }
        }
    }
}
