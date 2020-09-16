using HttpClientTest.HttpHelpers;
using HttpClientTest.Model;
using System;
using System.Collections.Generic;

namespace HttpClientTest
{
    class Program
    {
        

        static void Main()
        {
            // Identify the search parameters and call the method to obtain the results
            Console.WriteLine("Enter a city to search: ");
            string city = Console.ReadLine();

            Console.WriteLine("Enter an establishment name: ");
            string name = Console.ReadLine();

            Console.WriteLine("Enter the start date (YYYY-MM-DD): ");
            string date = Console.ReadLine();

            // Pass in the search parameters, receive the list of JSON entries
            CommonServiceLayerProvider commonServiceLayerProvider = new CommonServiceLayerProvider();
            List<InspectionData> inspections = commonServiceLayerProvider.GetInspections(name, city, date);

            ShowInspections(inspections);
            Console.ReadLine();
        }

        public static void ShowInspections(List<InspectionData> inspections)
        {
            if (inspections == null || inspections.Count == 0)
            {
                Console.WriteLine("No inspection records were found.");
            }
            else
            {
                foreach (InspectionData inspection in inspections)
                {
                    Console.WriteLine($"Name: {inspection.Name}\tCity: {inspection.City}\tGrade: {inspection.Grade}");
                }
            }
        }
    }
}
