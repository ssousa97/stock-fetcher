// See https://aka.ms/new-console-template for more information
Console.WriteLine("What is your name?");


var name = Console.ReadLine();
Console.WriteLine($"{Environment.NewLine}Hello, {name}, on {DateTime.Now:d} at {DateTime.Now:t}!");


