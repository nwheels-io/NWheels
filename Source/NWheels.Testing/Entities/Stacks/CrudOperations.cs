using System;
using System.Linq;
using NUnit.Framework;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

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
                InsertCustomers(repoFactory());
                RetrieveProductsByName(repoFactory());
                InsertOrder1(repoFactory());
                InsertOrder2(repoFactory());
                InsertOrder3(repoFactory());
                UpdateStatusOfOrders(repoFactory());
                RetrieveOrdersWithOrderLinesAndProducts(repoFactory());

                //-- retrieving polymorphic entities in one query doesn't seem to be typically supported by ORM/ODMs
                RetrieveCustomerContactDetails(repoFactory()); 
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void ExecuteAdvancedRetrievals(Func<Interfaces.Repository1.IOnlineStoreRepository> repoFactory)
            {
                InsertAttributes(repoFactory());
                InsertCategories(repoFactory());
                InsertProducts(repoFactory());
                InsertCustomers(repoFactory());
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

                    var attributes = repo.Attributes.OrderBy(a => a.Name).ToArray();
                    Assert.That(attributes.Select(a => a.Name), Is.EqualTo(new[] { "Color", "Size" }));

                    var product1 = repo.Products.New();
                    product1.CatalogNo = "CN111";
                    product1.Name = "ABC";
                    product1.Price = 123.45m;
                    product1.Categories.Add(categories[0]);
                    product1.Attributes.Add(attributes[0]);
                    product1.Attributes.Add(attributes[1]);

                    var product2 = repo.Products.New();
                    product2.CatalogNo = "CN222";
                    product2.Name = "DEF";
                    product2.Price = 678.90m;
                    product2.Categories.Add(categories[0]);
                    product2.Categories.Add(categories[2]);
                    product2.Attributes.Add(attributes[0]);

                    repo.Products.Insert(product1);
                    repo.Products.Insert(product2);

                    repo.CommitChanges();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void InsertCustomers(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    #region Customer #1

                    var customer1 = repo.Customers.New();
                    customer1.FullName = "John Smith";

                    var customer1Email1 = repo.NewEmailContactDetail(email: "john.smith@email.com", isPrimary: true);
                    var customer1Email2 = repo.NewEmailContactDetail(email: "jsmith@nomail.com", isPrimary: false);
                    var customer1Phone1 = repo.NewPhoneContactDetail(phone: "555-555-555-555", isPrimary: true);

                    customer1.ContactDetails.Add(customer1Email1);
                    customer1.ContactDetails.Add(customer1Email2);
                    customer1.ContactDetails.Add(customer1Phone1);

                    #endregion

                    #region Customer #2

                    var customer2 = repo.Customers.New();
                    customer2.FullName = "Maria Garcia";

                    var customer2Email1 = repo.NewEmailContactDetail(email: "maria.garcia@email.com", isPrimary: true);
                    var customer2Post1 = repo.NewPostContactDetail(isPrimary: true);
                    customer2Post1.PostalAddress.StreetAddress = "Luna, 10-3";
                    customer2Post1.PostalAddress.ZipCode = "28300";
                    customer2Post1.PostalAddress.City = "ARANJUEZ (MADRID)";
                    customer2Post1.PostalAddress.Country = "SPAIN";

                    customer2.ContactDetails.Add(customer2Email1);
                    customer2.ContactDetails.Add(customer2Post1);

                    #endregion

                    repo.ContactDetails.Insert(customer1Email1);
                    repo.ContactDetails.Insert(customer1Email2);
                    repo.ContactDetails.Insert(customer1Phone1);

                    repo.ContactDetails.Insert(customer2Email1);
                    repo.ContactDetails.Insert(customer2Post1);

                    repo.Customers.Insert(customer1);
                    repo.Customers.Insert(customer2);

                    repo.CommitChanges();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void RetrieveProductsByName(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    var product1 = repo.Products.Include(p => p.Categories, p => p.Attributes).Single(p => p.Name == "ABC");
                    var product2 = repo.Products.Include(p => p.Categories, p => p.Attributes).Single(p => p.Name == "DEF");

                    Assert.That(product1.CatalogNo, Is.EqualTo("CN111"));
                    Assert.That(product1.Name, Is.EqualTo("ABC"));
                    Assert.That(product1.Price, Is.EqualTo(123.45m));

                    Assert.That(product1.Categories, Is.Not.Null);
                    Assert.That(product1.Categories.Count(), Is.EqualTo(1));
                    Assert.That(product1.Categories.Count(c => c.Name == "CAT1"), Is.EqualTo(1));

                    Assert.That(product1.Attributes, Is.Not.Null);
                    Assert.That(product1.Attributes.Count(), Is.EqualTo(2));
                    Assert.That(product1.Attributes.Count(a => a.Name == "Color"), Is.EqualTo(1));
                    Assert.That(product1.Attributes.Count(a => a.Name == "Size"), Is.EqualTo(1));

                    Assert.That(product2.CatalogNo, Is.EqualTo("CN222"));
                    Assert.That(product2.Name, Is.EqualTo("DEF"));
                    Assert.That(product2.Price, Is.EqualTo(678.90m));

                    Assert.That(product2.Categories, Is.Not.Null);
                    Assert.That(product2.Categories.Count(), Is.EqualTo(2));
                    Assert.That(product2.Categories.Count(c => c.Name == "CAT1"), Is.EqualTo(1));
                    Assert.That(product2.Categories.Count(c => c.Name == "CAT3"), Is.EqualTo(1));

                    Assert.That(product2.Attributes, Is.Not.Null);
                    Assert.That(product2.Attributes.Count(), Is.EqualTo(1));
                    Assert.That(product2.Attributes.Count(a => a.Name == "Color"), Is.EqualTo(1));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void RetrieveCustomerContactDetails(IR1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    #region Customer #1

                    var customer1 = repo.Customers.Include(c => c.ContactDetails).Where(c => c.FullName == "John Smith").FirstOrDefault();

                    Assert.That(customer1, Is.Not.Null);
                    Assert.That(customer1.ContactDetails.Count, Is.EqualTo(3));

                    var contactDetails = customer1.ContactDetails.ToArray();

                    Assert.That(customer1.ContactDetails.Count, Is.EqualTo(3));
                    
                    Assert.That(contactDetails[0], Is.InstanceOf<IR1.IEmailContactDetail>());
                    Assert.That(contactDetails[0].As<IR1.IEmailContactDetail>().Email, Is.EqualTo("john.smith@email.com"));

                    Assert.That(contactDetails[1], Is.InstanceOf<IR1.IEmailContactDetail>());
                    Assert.That(contactDetails[1].As<IR1.IEmailContactDetail>().Email, Is.EqualTo("jsmith@nomail.com"));

                    Assert.That(contactDetails[2], Is.InstanceOf<IR1.IPhoneContactDetail>());
                    Assert.That(contactDetails[2].As<IR1.IPhoneContactDetail>().Phone, Is.EqualTo("555-555-555-555"));

                    #endregion
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void InsertOrder1(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    var attributes = repo.Attributes.OrderBy(a => a.Name).ToDictionary(a => a.Name);

                    var product1 = repo.Products.Single(p => p.Name == "ABC");
                    var product2 = repo.Products.Single(p => p.Name == "DEF");

                    var order = repo.Orders.New();
                    order.OrderNo = "ORD001";
                    order.PlacedAt = new DateTime(2015, 1, 1, 12, 0, 0, DateTimeKind.Utc);

                    var orderLine1 = repo.NewOrderLine(order, product1, quantity: 22);
                    orderLine1.Attributes.Add(repo.NewAttributeValueChoice(attributes["Color"], "Black"));
                    orderLine1.Attributes.Add(repo.NewAttributeValueChoice(attributes["Size"], "L"));

                    var orderLine2 = repo.NewOrderLine(order, product2, quantity: 11);
                    orderLine2.Attributes.Add(repo.NewAttributeValueChoice(attributes["Color"], "White"));

                    order.OrderLines.Add(orderLine1);
                    order.OrderLines.Add(orderLine2);

                    repo.OrdersLines.Insert(orderLine1);
                    repo.OrdersLines.Insert(orderLine2);

                    order.Customer = repo.Customers.First(c => c.FullName == "John Smith");

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

                    var orderLine1 = repo.NewOrderLine(order, product1, quantity: 11);
                    var orderLine2 = repo.NewOrderLine(order, product2, quantity: 22);

                    order.OrderLines.Add(orderLine1);
                    order.OrderLines.Add(orderLine2);

                    repo.OrdersLines.Insert(orderLine1);
                    repo.OrdersLines.Insert(orderLine2);

                    order.Customer = repo.Customers.First(c => c.FullName == "Maria Garcia");

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

                    var orderLine1 = repo.NewOrderLine(order, product2, quantity: 33);

                    order.OrderLines.Add(orderLine1);

                    order.DeliveryAddress.StreetAddress = "85 Scottish Street";
                    order.DeliveryAddress.City = "London";
                    order.DeliveryAddress.ZipCode = "E1WS21";
                    order.DeliveryAddress.Country = "UK";

                    order.BillingAddress.StreetAddress = "123 Main Street";
                    order.BillingAddress.City = "London";
                    order.BillingAddress.ZipCode = "W127RJ";
                    order.BillingAddress.Country = "UK";

                    repo.OrdersLines.Insert(orderLine1);

                    order.Customer = repo.Customers.First(c => c.FullName == "John Smith");

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

                    var state = order1.As<IDomainObject>().State;

                    repo.CommitChanges();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void RetrieveOrdersWithOrderLinesAndProducts(Interfaces.Repository1.IOnlineStoreRepository repo)
            {
                using ( repo )
                {
                    Interfaces.Repository1.IOrder order1;
                    Interfaces.Repository1.IOrder order2;
                    Interfaces.Repository1.IOrder order3;

                    order1 = repo.Orders.Include(o => o.OrderLines.Select(ol => ol.Product)).Single(o => o.OrderNo == "ORD001");
                    order2 = repo.Orders.Include(o => o.OrderLines.Select(ol => ol.Product)).Single(o => o.OrderNo == "ORD002");
                    order3 = repo.Orders.Include(o => o.OrderLines.Select(ol => ol.Product)).Single(o => o.OrderNo == "ORD003");

                    var attributes = repo.Attributes.OrderBy(a => a.Name).ToDictionary(a => a.Name);

                    #region Assert order1

                    Assert.That(order1.OrderNo, Is.EqualTo("ORD001"));
                    Assert.That(order1.PlacedAt, Is.EqualTo(new DateTime(2015, 1, 1, 12, 0, 0, DateTimeKind.Utc)));
                    Assert.That(order1.Status, Is.EqualTo(Interfaces.Repository1.OrderStatus.ProductsShipped));

                    var order1Lines = order1.OrderLines.ToArray();
                    Assert.That(order1Lines.Length, Is.EqualTo(2));

                    Assert.That(order1Lines[0].Order, Is.SameAs(order1));
                    Assert.That(order1Lines[0].Product.CatalogNo, Is.EqualTo("CN111"));
                    Assert.That(order1Lines[0].Quantity, Is.EqualTo(22));
                    
                    Assert.That(order1Lines[0].Attributes, Is.Not.Null);
                    Assert.That(order1Lines[0].Attributes.Count, Is.EqualTo(2));

                    //Assert.That(order1Lines[0].Attributes.First().Attribute, Is.SameAs(attributes["Color"]));
                    //Assert.That(order1Lines[0].Attributes.First().Value, Is.EqualTo("Black"));
                    //Assert.That(order1Lines[0].Attributes.Last().Attribute, Is.SameAs(attributes["Size"]));
                    //Assert.That(order1Lines[0].Attributes.Last().Value, Is.EqualTo("L"));

                    Assert.That(order1Lines[0].Attributes.First().Attribute.Name, Is.EqualTo("Color"));
                    Assert.That(order1Lines[0].Attributes.First().Value, Is.EqualTo("Black"));
                    Assert.That(order1Lines[0].Attributes.Last().Attribute.Name, Is.EqualTo("Size"));
                    Assert.That(order1Lines[0].Attributes.Last().Value, Is.EqualTo("L"));

                    Assert.That(order1Lines[1].Order, Is.SameAs(order1));
                    Assert.That(order1Lines[1].Product.CatalogNo, Is.EqualTo("CN222"));
                    Assert.That(order1Lines[1].Quantity, Is.EqualTo(11));

                    Assert.That(order1Lines[1].Attributes, Is.Not.Null);
                    Assert.That(order1Lines[1].Attributes.Count, Is.EqualTo(1));
                    
                    //Assert.That(order1Lines[1].Attributes.First().Attribute, Is.SameAs(attributes["Color"]));
                    //Assert.That(order1Lines[1].Attributes.First().Value, Is.EqualTo("White"));

                    Assert.That(order1Lines[1].Attributes.First().Attribute.Name, Is.EqualTo("Color"));
                    Assert.That(order1Lines[1].Attributes.First().Value, Is.EqualTo("White"));

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

                    Assert.That(order3.DeliveryAddress, Is.Not.Null);
                    Assert.That(order3.DeliveryAddress.StreetAddress, Is.EqualTo("85 Scottish Street"));
                    Assert.That(order3.DeliveryAddress.City, Is.EqualTo("London"));
                    Assert.That(order3.DeliveryAddress.ZipCode, Is.EqualTo("E1WS21"));
                    Assert.That(order3.DeliveryAddress.Country, Is.EqualTo("UK"));

                    Assert.That(order3.BillingAddress, Is.Not.Null);
                    Assert.That(order3.BillingAddress.StreetAddress, Is.EqualTo("123 Main Street"));
                    Assert.That(order3.BillingAddress.City, Is.EqualTo("London"));
                    Assert.That(order3.BillingAddress.ZipCode, Is.EqualTo("W127RJ"));
                    Assert.That(order3.BillingAddress.Country, Is.EqualTo("UK"));

                    #endregion

                    Assert.That(order1Lines[0].Product, Is.SameAs(order2Lines[0].Product));
                    Assert.That(order1Lines[1].Product, Is.SameAs(order2Lines[1].Product));
                    Assert.That(order1Lines[1].Product, Is.SameAs(order3Lines[0].Product));
                }
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
