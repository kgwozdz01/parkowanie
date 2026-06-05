# Projekt: System Parkingowy

Kompletne rozwiązanie zadania projektowego (aplikacja backendowa Web API).

## Repozytorium GitHub
Link do publicznego kodu źródłowego: [https://github.com/TWÓJ_NICK/ParkingSystem](https://github.com/TWÓJ_NICK/ParkingSystem)

## Technologie
* .NET 10.0
* ASP.NET Core Web API
* Entity Framework Core + SQLite
* ASP.NET Core Identity + JWT Bearer Auth

## Jak uruchomić lokalnie
1. Sklonuj repozytorium lub rozpakuj archiwum.
2. Otwórz solucję w środowisku JetBrains Rider lub Visual Studio.
3. Upewnij się, że projekt startowy to `ParkingSystem.WebApi` (lub `ASP.NET Core Web API`).
4. Uruchom projekt (`Run`).
5. Swagger otworzy się automatycznie pod adresem: `http://localhost:5270/swagger/`

## Testowanie API
W projekcie WebApi znajduje się plik `test.http` przeznaczony do szybkiego testowania endpointów autentykacji bezpośrednio ze środowiska IDE.