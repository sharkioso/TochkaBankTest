﻿
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


class HotelCapacity
{
    class Check
    {
        public Check(DateTime date, int change)
        {
            Date = date;
            Change = change;
        }
        public DateTime Date { get; set; }
        public int Change { get; set; }
    }

    static bool CheckCapacity(int maxCapacity, List<Guest> guests)
    {
        var checks = new List<Check>();

        foreach (var guest in guests)
        {
            checks.Add(new Check(DateTime.Parse(guest.CheckIn), 1));
            checks.Add(new Check(DateTime.Parse(guest.CheckOut), -1));
        }

        var sortedChecks = checks.OrderBy(c => c.Date)
                                .ThenBy(c => c.Change)
                                .ToList();

        var currentGuests = 0;
        foreach (var check in sortedChecks)
        {
            currentGuests += check.Change;
            if (currentGuests > maxCapacity) return false;
        }

        return true;
    }

    class Guest
    {
        public string Name { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
    }


    static void Main()
    {
        int maxCapacity = int.Parse(Console.ReadLine());
        int n = int.Parse(Console.ReadLine());


        List<Guest> guests = new List<Guest>();


        for (int i = 0; i < n; i++)
        {
            string line = Console.ReadLine();
            Guest guest = ParseGuest(line);
            guests.Add(guest);
        }


        bool result = CheckCapacity(maxCapacity, guests);


        Console.WriteLine(result ? "True" : "False");
    }


    // Простой парсер JSON-строки для объекта Guest
    static Guest ParseGuest(string json)
    {
        var guest = new Guest();


        // Извлекаем имя
        Match nameMatch = Regex.Match(json, "\"name\"\\s*:\\s*\"([^\"]+)\"");
        if (nameMatch.Success)
            guest.Name = nameMatch.Groups[1].Value;


        // Извлекаем дату заезда
        Match checkInMatch = Regex.Match(json, "\"check-in\"\\s*:\\s*\"([^\"]+)\"");
        if (checkInMatch.Success)
            guest.CheckIn = checkInMatch.Groups[1].Value;


        // Извлекаем дату выезда
        Match checkOutMatch = Regex.Match(json, "\"check-out\"\\s*:\\s*\"([^\"]+)\"");
        if (checkOutMatch.Success)
            guest.CheckOut = checkOutMatch.Groups[1].Value;


        return guest;
    }
}