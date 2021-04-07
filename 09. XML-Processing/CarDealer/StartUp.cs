using CarDealer.Data;
using CarDealer.DataTransferObjects.Input;
using CarDealer.DataTransferObjects.Output;
using CarDealer.Models;
using CarDealer.XmlHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            CarDealerContext context = new CarDealerContext();
            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //var supplierXml = File.ReadAllText("Datasets/suppliers.xml");
            //ImportSuppliers(context, supplierXml);

            //var partsXml = File.ReadAllText("Datasets/parts.xml");
            //ImportParts(context, partsXml);

            //var carsXml = File.ReadAllText("Datasets/cars.xml");
            //ImportCars(context, carsXml);

            //var customersXml = File.ReadAllText("Datasets/customers.xml");
            //ImportCustomers(context, customersXml);

            //var salesXml = File.ReadAllText("Datasets/sales.xml");
            //ImportSales(context, salesXml);

            Console.WriteLine(GetSalesWithAppliedDiscount(context));
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Select(x => new SaleOutputModel
                {
                    Car = new CarSaleOutputModel
                    {
                        Make = x.Car.Make,
                        Model = x.Car.Model,
                        TravelledDistance = x.Car.TravelledDistance
                    },
                    Discount = x.Discount,
                    CustomerName = x.Customer.Name,
                    Price = x.Car.PartCars.Sum(pc => pc.Part.Price),
                    PriceWithDiscount = x.Car.PartCars.Sum(pc => pc.Part.Price) - x.Car.PartCars.Sum(pc => pc.Part.Price) * x.Discount / 100
                }).ToArray();

            var result = XmlConverter.Serialize<SaleOutputModel[]>(sales, "sales");
            return result;
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .Where(x => x.Sales.Count > 0)
                .Select(x => new CustomerOutputModel
                {
                    FullName = x.Name,
                    BoughtCars = x.Sales.Count,
                    SpentMoney = x.Sales.Select(x => x.Car)
                        .SelectMany(x => x.PartCars)
                        .Sum(x => x.Part.Price)
                })
                .OrderByDescending(x => x.SpentMoney)
                .ToArray();

            var result = XmlConverter.Serialize<CustomerOutputModel[]>(customers, "customers");
            return result;
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                .Select(x => new CarPartOutputModel
                {
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance,
                    Parts = x.PartCars.Select(pc => new CarPartInfoOutputModel
                    {
                        Name = pc.Part.Name,
                        Price = pc.Part.Price
                    })
                    .OrderByDescending(p => p.Price)
                    .ToArray()
                })
                .OrderByDescending(x => x.TravelledDistance)
                .ThenBy(x => x.Model)
                .Take(5)
                .ToArray();

            var result = XmlConverter.Serialize<CarPartOutputModel[]>(cars, "cars");
            return result;
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(x => !x.IsImporter)
                .Select(x => new SupplierOutputModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    PartsCount = x.Parts.Count
                }).ToArray();

            var result = XmlConverter.Serialize<SupplierOutputModel[]>(suppliers, "suppliers");

            return result;
        }

        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(x => x.Make == "BMW")
                .Select(x => new BMWOutputModel
                {
                    Id = x.Id,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance
                })
                .OrderBy(x => x.Model)
                .ThenByDescending(x => x.TravelledDistance)
                .ToArray();

            var result = XmlConverter.Serialize<BMWOutputModel[]>(cars, "cars");
            return result;
        }

        public static string GetCarsWithDistance(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(x => x.TravelledDistance > 2_000_000)
                .Select(x => new CarOutputModel
                {
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance
                })
                .OrderBy(x => x.Make)
                .ThenBy(x => x.Model)
                .Take(10)
                .ToArray();

            var result = XmlConverter.Serialize<CarOutputModel[]>(cars, "cars");
            return result;
        }

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            var salesDto = XmlConverter.Deserialize<SaleInputModel>(inputXml, "Sales");

            var sales = salesDto.Where(x => context.Cars.Any(c => c.Id == x.CarId))
                .Select(x => new Sale
                {
                    Discount = x.Discount,
                    CustomerId = x.CustomerId,
                    CarId = x.CarId
                }).ToList();

            context.Sales.AddRange(sales);
            context.SaveChanges();
            return $"Successfully imported {sales.Count}";
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            var customersDto = XmlConverter.Deserialize<CustomerInputModel>(inputXml, "Customers");

            var customers = customersDto.Select(x => new Customer
            {
                Name = x.Name,
                BirthDate = x.BirthDate,
                IsYoungDriver = x.IsYoungDriver
            }).ToList();

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}";
        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            var cars = new List<Car>();
            var carsDtos = XmlConverter.Deserialize<CarInputModel>(inputXml, "Cars");

            var allParts = context.Parts
                .Select(x => x.Id).ToList();

            foreach (var currentCar in carsDtos)
            {
                var distinctedParts = currentCar.CarPartsInputModel
                    .Select(x => x.Id).Distinct();
                var parts = distinctedParts.Intersect(allParts);

                var car = new Car
                {
                    Make = currentCar.Make,
                    Model = currentCar.Model,
                    TravelledDistance = currentCar.TraveledDistance
                };

                foreach (var part in parts)
                {
                    var partCar = new PartCar
                    {
                        PartId = part
                    };

                    car.PartCars.Add(partCar);
                }

                cars.Add(car);
            }
            context.Cars.AddRange(cars);
            context.SaveChanges();
            return $"Successfully imported {cars.Count}";
        }

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            var partsDto = XmlConverter.Deserialize<PartInputModel>(inputXml, "Parts");

            var supplierIds = context.Suppliers
                .Select(x => x.Id)
                .ToList();

            var parts = partsDto
                .Where(s => supplierIds.Contains(s.SupplierId))
                .Select(x => new Part
            {
                Name = x.Name,
                Price = x.Price,
                Quantity = x.Quantity,
                SupplierId = x.SupplierId
            }).ToList();

            context.Parts.AddRange(parts);
            context.SaveChanges();
            return $"Successfully imported {parts.Count}";
        }

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            var suppliersDto = XmlConverter.Deserialize<SupplierInputModel>(inputXml, "Suppliers");

            var suppliers = suppliersDto.Select(x => new Supplier
            {
                Name = x.Name,
                IsImporter = x.IsImporter
            }).ToList();

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count}";
        }
    }
}